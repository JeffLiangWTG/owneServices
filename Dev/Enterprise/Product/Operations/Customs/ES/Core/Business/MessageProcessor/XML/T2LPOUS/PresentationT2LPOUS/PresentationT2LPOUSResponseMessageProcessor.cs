using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEJECV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class PresentationT2LPOUSResponseMessageProcessor : T2LPOUSCommonResponseMessageProcessor<IejecSalType, IMessagePrettyFormatter>
	{
		public PresentationT2LPOUSResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"T2L POUS Presentation Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCCIEJECV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.T2lPresentationPous };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(IejecSalType response, EDIMessage message, CusEntryHeader entryHeader) => new PresentationT2LPOUSMessagePrettyFormatter(response);

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new T2LClearanceDocumentRequest(businessObject, certName);

		protected override ZString ProcessAcceptedDeclaration(IejecSalType response, EDIMessage message, CusEntryHeader entryHeader)
		{
			SetT2CMovementReferenceNumber(entryHeader, response.Mrnjec);
			ProcessCommonAcceptedDeclaration(entryHeader, response.RiskAnalysisResultCode, response.Csvjec, message, preparationDateAndTime: response.Message.PreparationDateAndTime);

			return ZString.Empty;
		}

		protected override Dictionary<string, string> FileNamesDict(string mrn, string oldCSVClearance, EDIMessage message) => EDocHelper.FileNamesDictT2C(mrn, oldCSVClearance);

		const string XsdSchemaNameCCIEJECV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.Incoming.CCIEJECV1Sal.xsd";
	}
}
