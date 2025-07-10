using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CNS.ServiceTasks.StatusUpdateXML.TextProcessors
{
	abstract class CnsChildProcessor : IEntryNumberProvider, IUcnProvider
	{
		internal virtual void Process(EDIMessage receivedEdiMessage, ILogger serviceLogger, IEDocsDelayedSaver eDocsSaver)
		{
			this.receivedEdiMessage = receivedEdiMessage;
			CusEntryHeader entryHeader = null;
			if (!RegexPatternForEntryNumberAndDate.IsEmpty && !EntryNumber.IsEmpty)
			{
				var entryHeaders = GbExtensionHelpers.GetCusEntryHeaderFromCusEntryNumberAndDate(EntryNumber, receivedEdiMessage.Factory, EntryDate);
				if (entryHeaders.Length == 1)
				{
					entryHeader = entryHeaders[0];
				}
			}
			else if (!UcnNumberVerbatim.IsEmpty)
			{
				entryHeader = this.GetEntryHeaderFromUcnUsingMucr(receivedEdiMessage);
			}

			InterpretMessage(receivedEdiMessage);
			var firstLineOfBody = GetFirstLineOfMessageText(receivedEdiMessage);
			if (entryHeader != null)
			{
				serviceLogger.Information($"Found entry {entryHeader.CH_BGMReference} when processing message {receivedEdiMessage.EM_MessageNum}");
				DoFurtherProcessingForSuccessfullyFoundEntry(entryHeader);
				entryHeader.Messages.Add(receivedEdiMessage);
				receivedEdiMessage.EM_Status = "RCV";
				SaveToEDocs(entryHeader, eDocsSaver, firstLineOfBody);
				SendNotificationEmail(entryHeader, firstLineOfBody);
			}
			else
			{
				receivedEdiMessage.EM_Status = "FAL";  // Failed to find job.

				if (receivedEdiMessage.EM_ReceiveTransmit == EDIMessage.Direction.Receive && receivedEdiMessage.Interchange != null)
				{
					receivedEdiMessage.Interchange.EI_Status = "FAL";
				}

				serviceLogger.Error($"Could not find entry for message number {receivedEdiMessage.EM_MessageNum}");
				SendFailureNotificationEmail("Reference or job could not be found - " + firstLineOfBody, receivedEdiMessage);
			}
		}

		protected virtual void DoFurtherProcessingForSuccessfullyFoundEntry(CusEntryHeader entryHeader)
		{
		}

		protected virtual ZString RegexPatternForEntryNumberAndDate { get { return ZString.Empty; } }
		protected virtual ZString RegexPatternForUcn { get { return @"UCN:?\s+([A-Za-z0-9]+?)\s"; } }
		protected virtual ZString DateMaskPatternForEntryDate { get { return ZString.Empty; } }
		protected EDIMessage receivedEdiMessage;

		protected virtual ZString EntryNumberCore()
		{
			return RegexMatchForEntryNumberAndDate.Groups[1].Value;
		}

		protected virtual ZDateTime EntryDateCore()
		{
			var dateString = RegexMatchForEntryNumberAndDate.Groups[2].Value;
			var result = ZDateTime.Empty;
			ZDateTime.TryParseExact(dateString, out result, DateMaskPatternForEntryDate);
			return result;
		}

		protected Match RegexMatchForEntryNumberAndDate
		{
			get { return new Regex(RegexPatternForEntryNumberAndDate).Match(receivedEdiMessage.EM_MessageText); }
		}

		protected virtual Match RegexMatchForUcn
		{
			get { return new Regex(RegexPatternForUcn).Match(receivedEdiMessage.EM_MessageText); }
		}

		protected virtual ZString ManipulateUcn(string crappyUcnIn)
		{
			return crappyUcnIn;
		}

		protected ZString GetFirstGroupMatchFromMessageText(string pattern)
		{
			var result = "";
			var regex = new Regex(pattern);
			var match = regex.Match(receivedEdiMessage.EM_MessageText);
			if (match.Success)
			{
				result = match.Groups[1].Value;
			}
			return result;
		}

		internal static void SendFailureNotificationEmail(string firstLineOfBody, EDIMessage receivedEdiMessage)
		{
			var emailSender = new HtmlNotificationEmailSender();
			var emailDef = emailSender.CreateEmail(firstLineOfBody, receivedEdiMessage.EM_MessageInterpretation);
			new Customs.Business.EmailSender(new BatchProcessor.LoggingInformation()).SendNotification(
				emailDef,
				GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCnsErrors, "", receivedEdiMessage.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty),
				GBCustomsDataRegistry.Instance.NotificationCnsErrors,
				receivedEdiMessage.Factory);
		}

		protected void SendNotificationEmail(CusEntryHeader entryHeader, string firstLineOfBody)
		{
			var staffQuery = new ZQuery(GlbStaffSchema.GS_Code, entryHeader.Declaration.JE_GS_NKCusAgent);
			var originalSender = entryHeader.Factory.LoadTop1<GlbStaff>(staffQuery);
			var emailSender = new HtmlNotificationEmailSender();
			var emailDef = emailSender.CreateEmail(firstLineOfBody + " " + entryHeader.CH_BGMReference + " " + entryHeader.EntryNumber, receivedEdiMessage.EM_MessageInterpretation, entryHeader.RegistryCompanyPK, entryHeader.RegistryBranchPK, Guid.Empty);
			new Customs.Business.EmailSender(new BatchProcessor.LoggingInformation()).SendNotification(
				emailDef,
				originalSender,
				GBCustomsDataRegistry.Instance.CustomsResponseNotifications,
				GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCnsTextUpdates, "", entryHeader.RegistryCompanyPK, Guid.Empty, Guid.Empty),
				GBCustomsDataRegistry.Instance.NotificationCnsTextUpdates,
				receivedEdiMessage.Factory);
		}

		protected void SaveToEDocs(CusEntryHeader cusEntryHeader, IEDocsDelayedSaver eDocsSaver, string firstLineOfBody)
		{
			if (GBCustomsDataRegistry.Instance.CnsSaveStatusNotificationsToEDocs.Value && eDocsSaver != null)
			{
				// eDocs is so unbelievably slow... adding a small text file to eDocs for each of many jobs will make processing (in particular saving) times rocket by orders of magnitude.
				var docManagerInfo = ((IDocManagerSupport)cusEntryHeader.Declaration).DocManagerInfo;
				string fileName = MakeFilenameSafe.MakeSafe(firstLineOfBody.Trim() + " entry " + cusEntryHeader.EntryNumber) + ".txt";
				var printFile = docManagerInfo.AddFileOrDocument(ZBlob.FromAscii(receivedEdiMessage.EM_MessageText), fileName, Core.Constants.RefDocTypes.ClearanceAdvice, false);
				printFile.Description = firstLineOfBody;
				eDocsSaver.QueueForSaving(docManagerInfo);
			}
		}

		internal static string GetFirstLineOfMessageText(EDIMessage receivedEdiMessage)
		{
			var firstLineOfBody = Regex.Replace(Regex.Split(receivedEdiMessage.EM_MessageText.TrimStart(), System.Environment.NewLine)[0], @"\s+", " ");
			return firstLineOfBody;
		}

		internal static void InterpretMessage(EDIMessage receivedEdiMessage, string optionalPreamble = "")
		{
			var formattedPreamble = string.IsNullOrEmpty(optionalPreamble) ? "" : "<h3>This report could not be parsed and no job has been updated - " + optionalPreamble + "</h3>";
			receivedEdiMessage.EM_MessageInterpretation = formattedPreamble + "<pre>" + receivedEdiMessage.EM_MessageText + "</pre>";
		}

		#region IUcnProvider
		public ZString UcnNumberProperlyTruncated
		{
			get { return CNScargoStatus.TruncateCnsUcn(UcnNumberVerbatim); }
		}

		public ZString UcnNumberVerbatim
		{
			get { return ManipulateUcn(RegexMatchForUcn.Groups[1].Value); }
		}
		#endregion

		#region IEntryNumberProvider
		public ZDateTime EntryDate
		{
			get { return EntryDateCore(); }
		}

		public ZString EntryNumber
		{
			get { return EntryNumberCore(); }
		}
		#endregion
	}
}
