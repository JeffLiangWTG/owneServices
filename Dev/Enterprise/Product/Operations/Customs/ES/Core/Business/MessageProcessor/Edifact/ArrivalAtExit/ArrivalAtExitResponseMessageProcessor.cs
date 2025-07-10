using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public class ArrivalAtExitResponseMessageProcessor : ESCommonResponseMessageProcessor<CusExitDetail, ICUSRESV921ESMessageProvider>
	{
		public ArrivalAtExitResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Export Arrival at Exit Declaration Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.ArrivalAtExit };

		protected override void ProcessMessageCore(EDIMessage message, CusExitDetail linkedBusinessObject, ICUSRESV921ESMessageProvider provider)
		{
			var messageFunction = provider.MessageFunction;
			SetStatus(messageFunction, linkedBusinessObject);
			if (messageFunction != MessageFunctionCodeList.Codes.Rejected)
			{
				linkedBusinessObject.ZG_Circuit = messageFunction;
			}

			linkedBusinessObject.ZG_AcceptanceDate = provider.AdmissionDate;

			SetCSVClearanceAndTriggerDocumentRequest(linkedBusinessObject, message, provider.CSVReleaseCode, linkedBusinessObject.CED_MovementReferenceNumber);

			linkedBusinessObject.CED_ExitDate = provider.CSVReleaseCreationDate.Date;

			if (provider.FreeTextErrors != null && provider.FreeTextErrors.Any())
			{
				ProcessRejectedCusExitDetail(provider, message);
				SetMessageSubTypeAsRejected(message);
			}
			else
			{
				ProcessAcceptedCusExitDetail(provider, message);
				SetMessageSubTypeAsAccepted(message);
			}

			SetMessageStatusAsReceived(message);
		}

		void SetStatus(ZString messageFunction, CusExitDetail exitDetail)
		{
			var cedStatus = (string)messageFunction switch
			{
				MessageFunctionCodeList.Codes.Rejected => EntryStatusCodes.Error,
				MessageFunctionCodeList.Codes.GreenCircuit => EntryStatusCodes.Cleared,
				MessageFunctionCodeList.Codes.RedCircuit or MessageFunctionCodeList.Codes.OrangeCircuit => EntryStatusCodes.CustomsDeclarationAccepted,
				_ => null
			};

			if (cedStatus is not null)
			{
				exitDetail.CED_Status = cedStatus;
			}
		}

		void ProcessRejectedCusExitDetail(ICUSRESV921ESMessageProvider messageHelper, EDIMessage message)
		{
			message.EM_MessageInterpretation = new ArrivalAtExitMessagePrettyFormatter(messageHelper).CreateMessageDetailsRejected();
		}

		void ProcessAcceptedCusExitDetail(ICUSRESV921ESMessageProvider messageHelper, EDIMessage message)
		{
			message.EM_MessageInterpretation = new ArrivalAtExitMessagePrettyFormatter(messageHelper).CreateMessageDetailsAccepted();
		}

		protected override ICUSRESV921ESMessageProvider GetMessageProviderCore(EDIMessage message)
		{
			if (message.EM_MessageText.Contains(EdifactCodes.UNHSegmentCode))
			{
				return CUSRESV921ESMessageHelper.New(message);
			}
			else
			{
				return (ICUSRESV921ESMessageProvider)EdifactProcessorHelper.ProcessEdifactErrorResponse(message);
			}
		}

		protected override CommonDocumentRequest<CusExitDetail> GetNewDocumentRequest(CusExitDetail businessObject, ZString certName, EDIMessage message) => new ArrivalAtExitDocumentRequest(businessObject, certName);
		protected override Dictionary<string, string> FileNamesDict(string mrn, string oldCSVClearance, EDIMessage message) => EDocHelper.FileNamesDictArrivalAtExit(mrn, oldCSVClearance);
		protected override ZString GetOldCSVClearance(CusExitDetail businessObject) => businessObject.ZG_CSVClearance;
		protected override void SetNewCSVClearance(CusExitDetail businessObject, ZString newCSVClearance) => businessObject.ZG_CSVClearance = newCSVClearance;
	}
}
