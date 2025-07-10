using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public class EdecXmlSchemaErrorMessageProcessor : BaseResponseMessageProcessor<IXMLSchemaErrorsResponseDetail>
{
	public EdecXmlSchemaErrorMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("8EBA0674-91DC-4106-985A-31C609159DEE", "XML Schema Error Message Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.Import, MessageTypeCodeList.Codes.Export };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.XmlSchemaError };

	protected override BusinessObject FindLinkedObject(EDIMessage message, IXMLSchemaErrorsResponseDetail xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override void ProcessResponseMessage(CHEDIMessage message, IXMLSchemaErrorsResponseDetail customsResponse)
	{
		if (customsResponse != null && message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			var rejectionDateTime = customsResponse.RejectionDateTime;
			entryHeader.CH_Status = CHLogicalStatusList.Codes.Invalid;
			entryHeader.Logs.AddNew(AutoEvents.DeclarationRejected, rejectionDateTime);
		}
	}
}
