using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class ImportCUSTAXMessageProcessor : ImportMessageProcessor<AtlasInboundEDIMessage<ICUSTAX>, ICUSTAX>
	{
		public ImportCUSTAXMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("88486AA9-A177-46B6-985F-715AD35630D2", "Import CUSTAX Message Processor");

		protected override bool DelayStatusError => true;

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSTAX> message) => GetEntryHeaderFromMRN(message.Factory, message.DataProvider?.MRN, message.DataProvider?.ReferenceNumber);

		protected override bool UpdateWarehouseCore => CurrentMessage?.EM_LinkedObject is CusEntryHeader entry
			&& ((entry.IsIntoWarehouseWarehousing && entry.CH_EntryStatus.EqualsAny(CustomsClearedStatuses) && !entry.EntryInstruction.CEI_OA_Warehouse2.IsEmpty)
		|| entry.IsOutOfWarehouseWarehousing
				|| entry.Declaration.IsOutwardOrderImported);

		protected override bool ShouldPublishWhsOutwardAcceptEventCore(CusEntryHeader entryHeader) => entryHeader.CH_EntryStatus.EqualsAny(CustomsClearedStatuses);

		protected override bool UpdateWarehouseOrderForCurrentEntryLine(CusEntryLine entryLine) => entryLine.ZG_CustomsStatus.EqualsAny(CustomsClearedStatuses);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSTAX> message)
		{
			var dataProvider = message.DataProvider;
			var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
			var referenceNumber = dataProvider.ReferenceNumber;
			var mrn = dataProvider.MRN;

			UpdateEntryHeaderStatus(dataProvider, entryHeader);
			entryHeader.Logs.AddNew(Events.CustomsEntryStatus, entryHeader.CH_EntryStatus, ZDateTime.Now.ToOffset());
			AddDeclarationClearedEventIfAllEntriesCleared(entryHeader);
			UpdateOrDeleteCusReconEntryAndLines(dataProvider, entryHeader);
			entryHeader.CH_TotalPaid = dataProvider.TotalCustomsDutyAmount;

			if (entryHeader.CH_EntryReleaseDate.IsEmpty)
			{
				entryHeader.CH_EntryReleaseDate = ZDate.Today;
			}

			CreateOrDeleteEntryLineConfirmedFees(dataProvider, entryHeader);
			UpdateMatchingCusEntryLines(dataProvider, entryHeader);
			UpdateInvoiceValuationDateAndApportion(dataProvider.AcceptanceDate, entryHeader);
			UpdateBondValidToDate(entryHeader, dataProvider);

			message.EM_Status = AtlasEDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(new ZString[] { referenceNumber, mrn });

			var declaration = entryHeader.Declaration;
			SkipSnapshotUpdate(declaration);
			SendEmail();

			void SendEmail()
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(factory, declaration
					, Res.GetString("0D142B2A-C25B-473F-B795-2B5A69D9EF87", "Import CUSTAX – Customs Tax Assessment")
					, GetEmailBody(declaration, dataProvider)
					, false
					, message.Branch
					, declaration
					, () => entryHeader.Messages.LastSentOutgoingMessage);
			}

			if (UpdateWarehouse)
			{
				BondedWarehousingHelper.SetWarehouseTransactionStatusForImportMessageProcessing(entryHeader);
			}
		}

		void CreateOrDeleteEntryLineConfirmedFees(ICUSTAX provider, CusEntryHeader entryHeader)
		{
			foreach (var line in provider.Lines.Where(l => l.RequiresProcessingOfEntryLineConfirmedFees()))
			{
				if (ZInt.TryParse(line.LineNumber, out ZInt number) && entryHeader.AllEntryLines.FindByLineNumber(number) is CusEntryLine cusEntryLine)
				{
					cusEntryLine.DeleteEntryLineConfirmedFees();
					cusEntryLine.CreateEntryLineConfirmedFeesIfLineIsFinal(line);
				}
			}
		}

		void UpdateOrDeleteCusReconEntryAndLines(ICUSTAX dataProvider, CusEntryHeader entryHeader)
		{
			var cusReconEntry = entryHeader.GetCusReconEntry();
			var actualMRN = entryHeader.MovementReferenceNumber;
			var mrn = dataProvider.MRN;
			var referenceNumber = dataProvider.ReferenceNumber;

			if (cusReconEntry != null && dataProvider.CompletionFlag == ImportCompletionFlagList.Codes._7)
			{
				if (actualMRN == referenceNumber
					&& referenceNumber.Left(3).In(new ZString[] { ATLASReferenceNumberIdentifier.ATE, ATLASReferenceNumberIdentifier.ATD }))
				{
					cusReconEntry.CRE_OriginalEntryNumber = referenceNumber;
				}
				else if (!mrn.IsNullOrEmpty()
					&& actualMRN == mrn
					&& mrn.Substring(9, 1).In(new string[] { "D", "E" }))
				{
					cusReconEntry.CRE_OriginalEntryNumber = mrn;
				}
				cusReconEntry.CRE_EntryDate = new ZDateTime(dataProvider.RegistrationDate.GetValueOrDefault()).Date;

				UpdateOrDeleteCusReconEntryLines();
			}
			else
			{
				cusReconEntry?.Delete();
			}

			void UpdateOrDeleteCusReconEntryLines()
			{
				foreach (var line in dataProvider.Lines)
				{
					var cusReconEntryLine = cusReconEntry.GetCusReconEntryLineByOriginalEntryLineNumber(line.LineNumber);
					if (cusReconEntryLine != null && line.LineCompletionFlag == ImportCompletionFlagList.Codes._7)
					{
						cusReconEntryLine.CRL_CustomsStatus = EntryStatus.TX7;
					}
					else
					{
						cusReconEntryLine?.Delete();
					}
				}
			}
		}

		void UpdateEntryHeaderStatus(ICUSTAX dataProvider, CusEntryHeader entryHeader)
		{
			if (TryMapCompletionFlagToStatus(dataProvider.CompletionFlag, out var status))
			{
				entryHeader.CH_EntryStatus = status;
			}
		}

		void AddDeclarationClearedEventIfAllEntriesCleared(CusEntryHeader entryHeader)
		{
			if (entryHeader.Declaration.ActiveEntryHeaders.OfType<CusEntryHeader>().All(x => CUSTAXEntryClearedStatusHelper.IsCleared(x.CH_EntryStatus)))
			{
				entryHeader.Declaration.Logs.AddNew(Events.CustomsCleared, "Customs Cleared", ZDateTime.Now.ToOffset());
			}
		}

		bool TryMapCompletionFlagToStatus(string completionFlag, out string status)
		{
			var mapped = true;
			switch (completionFlag)
			{
				case ImportCompletionFlagList.Codes._1:
					status = EntryStatus.TX1;
					break;
				case ImportCompletionFlagList.Codes._2:
					status = EntryStatus.TX2;
					break;
				case ImportCompletionFlagList.Codes._3:
					status = EntryStatus.TX3;
					break;
				case ImportCompletionFlagList.Codes._4:
					status = EntryStatus.TX4;
					break;
				case ImportCompletionFlagList.Codes._5:
					status = EntryStatus.TX5;
					break;
				case ImportCompletionFlagList.Codes._6:
					status = EntryStatus.TX6;
					break;
				case ImportCompletionFlagList.Codes._7:
					status = EntryStatus.TX7;
					break;
				case ImportCompletionFlagList.Codes._8:
					status = EntryStatus.TX8;
					break;
				default:
					status = string.Empty;
					mapped = false;
					break;
			}

			return mapped;
		}

		void UpdateMatchingCusEntryLines(ICUSTAX provider, CusEntryHeader entryHeader)
		{
			foreach (var line in provider.Lines)
			{
				if (ZInt.TryParse(line.LineNumber, out var number) && entryHeader.AllEntryLines.FindByLineNumber(number) is CusEntryLine cusEntryLine)
				{
					UpdateCustomsValue(line, cusEntryLine);
					UpdateEntryLineStatus(line, cusEntryLine);
					UpdateDutyPercent(line, cusEntryLine);
				}
			}

			void UpdateEntryLineStatus(ICUSTAXLine line, CusEntryLine cusEntryLine)
			{
				if (TryMapCompletionFlagToStatus(line.LineCompletionFlag, out var status))
				{
					cusEntryLine.ZG_CustomsStatus = status;
				}
			}

			void UpdateCustomsValue(ICUSTAXLine line, CusEntryLine cusEntryLine)
			{
				if (line.CustomsValue.HasValue)
				{
					cusEntryLine.CL_CustomsValue = line.CustomsValue.Value;
				}
			}

			void UpdateDutyPercent(ICUSTAXLine line, CusEntryLine cusEntryLine)
			{
				var lineDutyOrNull =
					line.Duties?.FirstOrDefault(e => e is { ChargeType: "A0000", DutyRates.Count: > 0 });
				var dutyRateOrNull =
					lineDutyOrNull?.DutyRates?.FirstOrDefault(e => e is { AssessmentScale: "00" })?.Rate;

				if (dutyRateOrNull.HasValue)
				{
					cusEntryLine.CL_DutyPercent = dutyRateOrNull.Value;
				}
			}
		}

		void UpdateInvoiceValuationDateAndApportion(System.DateTime? acceptanceDateTime, CusEntryHeader entryHeader)
		{
			if (acceptanceDateTime.HasValue)
			{
				var accountingInputDateValue = acceptanceDateTime.Value;
				foreach (var invoice in entryHeader.InvoiceHeaders)
				{
					invoice.JZ_ValuationDateOverride = accountingInputDateValue;
				}
				entryHeader.Factory.Save();
				entryHeader.Declaration.ResumeApportionment();
			}
		}

		void UpdateBondValidToDate(CusEntryHeader entryHeader, ICUSTAX dataProvider)
		{
			if (entryHeader.EntryInstruction?.CEI_Procedure.LeftOrNull(2) == CustomsProcedureCodeList.Import.ProcedureCode._51 && dataProvider.Lines.FirstOrDefault(x => x.ExportLimitDate.HasValue) is { } line)
			{
				entryHeader.CH_BondValidToDate = line.ExportLimitDate.ConvertToZDate();
			}
		}

		static ZString GetEmailBody(JobDeclaration declaration, ICUSTAX provider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("545D4D67-0937-4282-9C0A-21417A1208B1", "Your Import Declaration for Job {0} received a Customs Tax Assessment. For details please follow the link to the job.", declaration.JE_DeclarationReference));

			htmlBody.Append("<br />");
			htmlBody.Append("<br />");

			var tableCreator = new HtmlTableCreator();

			if (!provider.MRN.IsNullOrEmpty())
			{
				tableCreator.WriteRow(Res.GetString("865AAD6A-D9A8-4C3E-9353-8354EA5D7F60", "MRN"), provider.MRN);
			}
			if (!provider.ReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("EA589B86-1AB0-44B2-A176-9B22EB0060BC", "Registration Number"), provider.ReferenceNumber);
			}
			if (!provider.LocalReferenceNumber.IsEmpty)
			{
				tableCreator.WriteRow(Res.GetString("073AF705-BA63-4990-BBC4-02480F293C31", "Local Reference Number"), provider.LocalReferenceNumber);
			}
			var completionFlag = provider.CompletionFlag;
			if (!completionFlag.IsEmpty)
			{
				var completionFlagList = declaration.Factory.GetCachedValue<ImportCompletionFlagList>();
				var completionFlagWithDescription = completionFlag + " - " + completionFlagList.GetDescriptionFromCode(completionFlag);
				tableCreator.WriteRow(Res.GetString("A0405F0D-2798-466B-B034-9DF87314F929", "Completion Flag"), completionFlagWithDescription);
			}
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}

		static ImmutableHashSet<ZString> CustomsClearedStatuses { get; } =
			new HashSet<ZString> { EntryStatus.TX5, EntryStatus.TX6, EntryStatus.TX7 }.ToImmutableHashSet();
	}
}
