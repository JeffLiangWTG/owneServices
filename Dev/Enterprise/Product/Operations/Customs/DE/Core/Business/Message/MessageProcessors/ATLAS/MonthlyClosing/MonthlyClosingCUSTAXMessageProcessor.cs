using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class MonthlyClosingCUSTAXMessageProcessor : MonthlyClosingMessageProcessor<AtlasInboundEDIMessage<ICUSTAX>, ICUSTAX>
	{
		public MonthlyClosingCUSTAXMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("50D3BF0C-2344-4E43-BEAE-5E2E93ACC7FC", "Monthly Closing CUSTAX Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSTAX> message) => GetCusReconDeclarationFromMRN(message.Factory, message.Branch.GB_GC, message.DataProvider?.MRN, message.DataProvider?.ReferenceNumber);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSTAX> message)
		{
			var dataProvider = message.DataProvider;
			var reconDeclaration = (CusReconDeclaration)message.EM_LinkedObject;
			var referenceNumber = dataProvider.ReferenceNumber;
			var mrn = dataProvider.MRN;

			UpdateReconDeclarationStatus(dataProvider, reconDeclaration);
			UpdateReconEntryLineStatus(dataProvider, reconDeclaration);
			UpdateEntryLine(dataProvider, reconDeclaration);
			CreateOrDeleteEntryLineConfirmedFees(dataProvider, reconDeclaration);
			reconDeclaration.CreateFinalizationFlagNote(MonthlyClosingHelper.DeclarationIsFinalizedFlag);

			message.EM_Status = AtlasEDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });

			SendEmail(message, reconDeclaration);
		}

		void UpdateReconDeclarationStatus(ICUSTAX dataProvider, CusReconDeclaration reconDeclaration)
		{
			if (CustomsStatusesByCompletionFlag.TryGetValue(dataProvider.CompletionFlag, out var customsStatus))
			{
				reconDeclaration.CRD_CustomsStatus = customsStatus;
			}
		}

		void CreateOrDeleteEntryLineConfirmedFees(ICUSTAX provider, CusReconDeclaration reconDeclaration)
		{
			var allCusReconEntryLines = reconDeclaration.CusReconEntries
				.SelectMany(x => x.CusReconEntryLines)
				.Cast<CusReconEntryLine>()
				.ToDictionary(x => x.CRL_LineNumber);
			foreach (var line in provider.Lines.Where(l => l.RequiresProcessingOfEntryLineConfirmedFees()))
			{
				if (ZShort.TryParse(line.LineNumber, out var number) && allCusReconEntryLines.TryGetValue(number, out var cusReconEntryLine)
					&& cusReconEntryLine.EntryLine != null)
				{
					var entryLine = cusReconEntryLine.EntryLine;
					entryLine.DeleteEntryLineConfirmedFees();
					entryLine.CreateEntryLineConfirmedFeesIfLineIsFinal(line);
				}
				else
				{
					Logger.LogWarning(GetLineNotFoundWarningMessage(line.LineNumber));
				}
			}
		}

		void UpdateReconEntryLineStatus(ICUSTAX dataProvider, CusReconDeclaration reconDeclaration)
		{
			var allCusReconEntryLines = reconDeclaration.CusReconEntries.SelectMany(x => x.CusReconEntryLines).ToDictionary(x => x.CRL_LineNumber.ToString());
			foreach (var line in dataProvider.Lines)
			{
				if (allCusReconEntryLines.TryGetValue(line.LineNumber, out var cusReconEntryLine) &&
					CustomsStatusesByCompletionFlag.TryGetValue(line.LineCompletionFlag, out var customsStatus))
				{
					cusReconEntryLine.CRL_CustomsStatus = customsStatus;
				}
			}
		}

		void UpdateEntryLine(ICUSTAX dataProvider, CusReconDeclaration reconDeclaration)
		{
			var allCusReconEntryLines = reconDeclaration.CusReconEntries.SelectMany(x => x.CusReconEntryLines).Cast<CusReconEntryLine>().ToDictionary(x => x.CRL_LineNumber.ToString());
			foreach (var line in dataProvider.Lines)
			{
				var lineDutyOrNull = line.Duties?.FirstOrDefault(e => e is { ChargeType: "A0000", DutyRates.Count: > 0 });
				var dutyRateOrNull = lineDutyOrNull?.DutyRates?.FirstOrDefault(e => e is { AssessmentScale: "00" })?.Rate;

				if (line.CustomsValue.HasValue || dutyRateOrNull.HasValue)
				{
					if (allCusReconEntryLines.TryGetValue(line.LineNumber, out var cusReconEntryLine) && cusReconEntryLine.EntryLine != null)
					{
						UpdateEntryLineCustomsValue(line.CustomsValue, cusReconEntryLine.EntryLine);
						UpdateEntryLineDutyPercent(dutyRateOrNull, cusReconEntryLine.EntryLine);
					}
					else
					{
						Logger.LogWarning(GetLineNotFoundWarningMessage(line.LineNumber));
					}
				}
			}
		}

		static void UpdateEntryLineCustomsValue(decimal? customsValue, CusEntryLine cusEntryLine)
		{
			if (customsValue.HasValue)
			{
				cusEntryLine.CL_CustomsValue = customsValue.Value;
			}
		}

		static void UpdateEntryLineDutyPercent(decimal? dutyPercent, CusEntryLine cusEntryLine)
		{
			if (dutyPercent.HasValue)
			{
				cusEntryLine.CL_DutyPercent = dutyPercent.Value;
			}
		}

		void SendEmail(AtlasInboundEDIMessage<ICUSTAX> message, CusReconDeclaration reconDeclaration)
		{
			var declaration = reconDeclaration;
			var dataProvider = message.DataProvider;
			var mrn = dataProvider.MRN;

			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, declaration
				, Res.GetString("C973FD59-EB8A-406B-87F2-34E09580EDAF", "Monthly Closing CUSTAX – Customs Tax Assessment")
				, GetEmailBody(declaration, message.DataProvider)
				, false
				, message.Branch
				, declaration
				, () => declaration.Messages.LastSentOutgoingMessage);
		}

		static ZString GetEmailBody(CusReconDeclaration declaration, ICUSTAX provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("5659DCFD-53F3-498C-BC83-E502753427AD", "Your Monthly Closing Declaration for Job {0} received a Customs Tax Assessment. For details please follow the link to the job.", declaration.CRD_JobReferenceNumber));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();

			var mrn = provider.MRN;
			var referenceNumber = provider.ReferenceNumber;
			var localReferenceNumber = provider.LocalReferenceNumber;
			if (!string.IsNullOrWhiteSpace(mrn))
			{
				tableCreator.WriteRow(Res.GetString("38AF35F5-A209-4C1B-ADA6-D12C55177EFA", "MRN"), mrn);
			}
			if (!referenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("119FD776-C436-400C-9B05-AEFCCB1179BA", "Registration Number"), referenceNumber);
			}
			if (!localReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("6F867769-B7F3-4731-B5DB-1180B8044C98", "Local Reference Number"), localReferenceNumber);
			}
			var completionFlag = provider.CompletionFlag;
			if (!completionFlag.IsEmpty)
			{
				var completionFlagList = declaration.Factory.GetCachedValue<ImportCompletionFlagList>();
				var completionFlagWithDescription = completionFlag + " - " + completionFlagList.GetDescriptionFromCode(completionFlag);
				tableCreator.WriteRow(Res.GetString("3AC81DA6-616E-46F5-987E-E5F149D73186", "Completion Flag"), completionFlagWithDescription);
			}
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}

		static readonly ImmutableDictionary<ZString, ZString> CustomsStatusesByCompletionFlag = new Dictionary<ZString, ZString>()
		{
			{ ImportCompletionFlagList.Codes._1, EntryStatus.TX1 },
			{ ImportCompletionFlagList.Codes._2, EntryStatus.TX2 },
			{ ImportCompletionFlagList.Codes._3, EntryStatus.TX3 },
			{ ImportCompletionFlagList.Codes._4, EntryStatus.TX4 },
			{ ImportCompletionFlagList.Codes._5, EntryStatus.TX5 },
			{ ImportCompletionFlagList.Codes._6, EntryStatus.TX6 },
		}.ToImmutableDictionary();

		static string GetLineNotFoundWarningMessage(string lineNumber)
		{
			return Res.GetString("F9067E08-E982-454D-A3F1-30AA869FDCB7", "The corresponding Line with Line Number {0} couldn't be found.", lineNumber);
		}
	}
}
