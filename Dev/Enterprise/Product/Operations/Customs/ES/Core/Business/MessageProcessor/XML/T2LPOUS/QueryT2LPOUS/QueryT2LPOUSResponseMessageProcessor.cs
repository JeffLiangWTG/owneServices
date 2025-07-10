using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01CONSV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class QueryT2LPOUSResponseMessageProcessor : T2LPOUSCommonResponseMessageProcessor<Iep01Cons, IMessagePrettyFormatter>
	{
		public QueryT2LPOUSResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"T2L POUS Query Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCCIEP01CONSV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.T2lQueryPous };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Iep01Cons response, EDIMessage message, CusEntryHeader entryHeader) => new QueryT2LPOUSMessagePrettyFormatter(response);

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new T2LClearanceDocumentRequest(businessObject, certName);

		protected override ZString ProcessAcceptedDeclaration(Iep01Cons response, EDIMessage message, CusEntryHeader entryHeader)
		{
			SetT2CMovementReferenceNumber(entryHeader, response.ProofData?.Mrnt2L);
			ProcessCommonAcceptedDeclaration(entryHeader, response.ProofData?.RiskAnalysisResultCode, response.ProofData?.Csvt2L, message);
			return ZString.Empty;
		}

		protected override Dictionary<string, string> FileNamesDict(string mrn, string oldCSVClearance, EDIMessage message) => EDocHelper.FileNamesDictT2C(mrn, oldCSVClearance);

		const string XsdSchemaNameCCIEP01CONSV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.Incoming.CCIEP01CONSV1Sal.xsd";
	}
}
