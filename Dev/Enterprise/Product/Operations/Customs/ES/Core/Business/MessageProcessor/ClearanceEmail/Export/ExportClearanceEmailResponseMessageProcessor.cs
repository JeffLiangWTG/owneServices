using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business
{
	public class ExportClearanceEmailResponseMessageProcessor : ESResponseMessageProcessor<IExportClearanceEmailProvider>
	{
		public ExportClearanceEmailResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		const int CSVClearanceLength = 16;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string CSVClearanceString = "n(C.S.V.):";
		const int ReleaseDateLength = 10;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string ReleaseDateString = "FechadeLevante:";
		const int LimitDateOfArrivalLength = 10;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string LimitDateOfArrivalString = "ximadeLlegada:";
		const int ClearanceResultLength = 2;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string ClearanceResultString = "ResultadoalDespacho:";

		protected override string MessageFriendlyNameCore => ExportMessageName;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string ExportMessageName = "Export Clearance Email Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.ExportClearanceEmail };
		protected override ZBool ShouldHaveSentInterchange => false;

		protected override IExportClearanceEmailProvider GetMessageProviderCore(EDIMessage message)
		{
			var messageText = message.EM_MessageText.Replace(" ", "");

			var csvClearance = GetSpecificData(messageText, CSVClearanceString, CSVClearanceLength);

			var releaseDateStringData = GetSpecificData(messageText, ReleaseDateString, ReleaseDateLength);
			var releaseDateCorrect = ZDateTime.TryParseExact(releaseDateStringData, out var releaseDate, CustomsDateTimeExtension.DateFormatSpainWithDash);

			if (releaseDateStringData.IsEmpty)
			{
				releaseDate = ZDateTime.Today;
			}

			var clearanceResult = GetSpecificData(messageText, ClearanceResultString, ClearanceResultLength);

			if (csvClearance.IsEmpty)
			{
				throw new InvalidOperationException(Res.GetString("3029DAFC-142B-488E-91BC-E9471E86D519", "Email Body doesn't have the correct data"));
			}

			var limitDateOfArrivalStringData = GetSpecificData(messageText, LimitDateOfArrivalString, LimitDateOfArrivalLength);
			var limitDateOfArrivalCorrect = ZDateTime.TryParseExact(limitDateOfArrivalStringData, out var limitDateOfArrival, CustomsDateTimeExtension.DateFormatSpainWithDash);

			if (!limitDateOfArrivalCorrect)
			{
				limitDateOfArrival = ZDateTime.Empty;
			}

			return new ExportClearanceEmailObject(csvClearance, releaseDate, limitDateOfArrival, clearanceResult);
		}

		ZString GetSpecificData(ZString fullText, ZString initialLineText, ZInt dataLength)
		{
			var result = ZString.Empty;

			if (fullText.Contains(initialLineText))
			{
				int startIndex = fullText.IndexOf(initialLineText, StringComparison.OrdinalIgnoreCase) + initialLineText.Length;
				result = fullText.SubstringSafe(startIndex, dataLength);
			}

			return result;
		}

		protected override CusEntryHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
			=> MessageProcessorHelper.GetRelevantBusinessObjectForEmailResponse<CusEntryHeader>(message, CusEntryNumberTypes.Standard.MovementReferenceNumber);

		protected override void ProcessMessageCore(EDIMessage message, CusEntryHeader linkedBusinessObject, IExportClearanceEmailProvider provider)
		{
			linkedBusinessObject.SetCSVClearanceNum(provider.CSVClearance);
			linkedBusinessObject.CH_EntryReleaseDate = provider.ReleaseDate;
			linkedBusinessObject.ZG_LimitDateOfArrival = provider.LimitDateOfArrival;
			linkedBusinessObject.ZG_ClearanceResult = provider.ClearanceResult;

			var entryInstructionIsSubStyleBOrC = linkedBusinessObject.EntryInstruction?.IsSubStyleBOrC ?? ZBool.False;
			linkedBusinessObject.CH_EntryStatus = entryInstructionIsSubStyleBOrC ? MessageProcessorConstants.EntryStatusCodes.ClearedWithPendingComplementaryDeclarations : MessageProcessorConstants.EntryStatusCodes.Cleared;

			TriggerMisingDocumentRequestForEmails(linkedBusinessObject, message);

			SetMessageStatusAsReceived(message);
			SetMessageSubTypeAsAccepted(message);
		}

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new ExportDocumentRequest(businessObject, certName);
	}
}
