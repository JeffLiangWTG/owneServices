using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LdatadoV2Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class T2LClearanceResponseMessageProcessor : T2LCommonResponseMessageProcessor<T2LdatadoV2Sal>
	{
		public T2LClearanceResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameT2LdatadoV2Sal;
		const string XsdSchemaNameT2LdatadoV2Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.T2L.Incoming.T2LdatadoV2Sal.xsd";

		protected override string MessageFriendlyNameCore => (NoResString)"T2L Clearance Declaration Message Processor";

		protected override ZString ProcessAcceptedDeclaration(T2LdatadoV2Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			SetCircuit(response, entryHeader);

			var cusEntryNumber = entryHeader.Factory.New<CusEntryNumber>();
			cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Spain.T2CMovementReferenceNumber;
			cusEntryNumber.CE_ParentID = entryHeader.PK;
			cusEntryNumber.CE_ParentTable = entryHeader.TableName;
			cusEntryNumber.CE_EntryNum = response.NumeroDeReferenciaDelJec;

			var csvClearance = response.CsVdelJustificanteDeLevante;
			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, csvClearance);

			return ZString.Empty;
		}

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(T2LdatadoV2Sal response, EDIMessage message, CusEntryHeader entryHeader) => new T2LClearanceMessagePrettyFormatter(response);

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new T2LClearanceDocumentRequest(businessObject, certName);

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.T2lClearance };
	}
}
