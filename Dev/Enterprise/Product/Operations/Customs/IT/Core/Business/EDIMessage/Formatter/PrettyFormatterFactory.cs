using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

static class PrettyFormatterFactory
{
	public static IITEDIMessagePrettyFormatter GetMessagePrettyFormatter(BusinessObjectFactory factory, ITEDIMessage messageEDI)
	{
		factory = Argument.NotNull(factory, nameof(factory));

		switch (messageEDI?.EM_MessageType)
		{
			case SADConstants.CustomsInterchangeType.IrispX:
				return new SadIrispEDIMessagePrettyFormatter(factory);

			case MessageProcessorConstants.InterchangeTypes.SingleWindowRequest:
			case MessageProcessorConstants.InterchangeTypes.SingleWindowStatusResponseMessageType:
				return new SingleWindowEDIMessagePrettyFormatter(factory);

			case MessageProcessorConstants.InterchangeTypes.Ucc6ResponseMessageType:
				return new Ucc6ResponseEDIMessagePrettyFormatter(factory);

			case EDIMessageTypeList.Codes.NewDeclaration:
			case EDIMessageTypeList.Codes.Amendment:
			case EDIMessageTypeList.Codes.Cancellation:
			case EDIMessageTypeList.Codes.SignatureResponse:
				return GetNewUcc6RequestEDIMessagePrettyFormatter(factory, messageEDI);

			case MessageProcessorConstants.InterchangeTypes.ElectronicFolderQueryType:
			case EDIMessageTypeList.Codes.AccountingSummaryRequest:
				return new SoapMessageInputPrettyFormatter(factory, AidaSoapMessageNamespaceConstants.ElectronicFolderRequestTypeNamespace);

			case MessageProcessorConstants.InterchangeTypes.ElectronicFolderResponseType:
				return new SoapMessageOutputPrettyFormatter(factory);

			case EDIMessageTypeList.Codes.IvistoRequest:
				return new SoapMessageInputPrettyFormatter(factory, AidaSoapMessageNamespaceConstants.ExportServiceTypeNamespace);

			case EDIMessageTypeList.Codes.IvistoResponse:
				return new Ucc6IvistoResponseEDIMessagePrettyFormatter(factory);

			case EDIMessageTypeList.Codes.IrildesRequest:
				return new SoapMessageInputPrettyFormatter(factory, AidaSoapMessageNamespaceConstants.NctsServiceTypeNamespace);

			case EDIMessageTypeList.Codes.IrildesResponse:
				return new IrildesResponseEDIMessagePrettyFormatter(factory);

			case EDIMessageTypeList.Codes.SummaryProspectusRequest:
			case EDIMessageTypeList.Codes.SummaryProspectusDownload:
			case EDIMessageTypeList.Codes.ReleaseProspectusRequest:
				return GetNewSoapInputOutputEDIMessagePrettyFormatter(factory, messageEDI);

			default:
				return null;
		}
	}

	static IITEDIMessagePrettyFormatter GetNewUcc6RequestEDIMessagePrettyFormatter(BusinessObjectFactory factory, ITEDIMessage messageEDI)
	{
		return (string)messageEDI.EM_ApplicationReference switch
		{
			EDIMessageApplicationReferenceList.Codes.Import => new SoapMessageInputPrettyFormatter(factory, AidaSoapMessageNamespaceConstants.ImportServiceTypeNamespace),
			EDIMessageApplicationReferenceList.Codes.Export => new SoapMessageInputPrettyFormatter(factory, AidaSoapMessageNamespaceConstants.ExportServiceTypeNamespace),
			EDIMessageApplicationReferenceList.Codes.Ncts => new SoapMessageInputPrettyFormatter(factory, AidaSoapMessageNamespaceConstants.NctsServiceTypeNamespace),
			EDIMessageApplicationReferenceList.Codes.TemporaryStorage => new SoapMessageInputPrettyFormatter(factory, AidaSoapMessageNamespaceConstants.PntsServiceTypeNamespace),
			_ => null,
		};
	}

	static IITEDIMessagePrettyFormatter GetNewSoapInputOutputEDIMessagePrettyFormatter(BusinessObjectFactory factory, ITEDIMessage messageEDI)
	{
		if (!messageEDI.IsTransmitMessage)
		{
			return new SoapMessageOutputPrettyFormatter(factory);
		}
		return new SoapMessageInputPrettyFormatter(factory, AidaSoapMessageNamespaceConstants.ElectronicFolderRequestTypeNamespace);
	}
}
