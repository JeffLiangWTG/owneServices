using CargoWise.Application;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Ebd;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using Enterprise.Customs.CH.NCTS.Business;

namespace Enterprise.Customs.CH.Business;

public static class MessagePrettyFormatterFactory
{
	internal static IMessagePrettyFormatter GetMessageFormatter(CHEDIMessage message)
	{
		switch (message.EM_MessageType)
		{
			case MessageTypeCodeList.Codes.Import:
			case MessageTypeCodeList.Codes.Export:
				switch (message.EM_MessageSubType)
				{
					case MessageSubTypeCodeList.Codes.Accepted:
						return new AcceptanceMessagePrettyFormatter(message.Factory, message.MessageDetail as IAcceptanceResponseDetail);
					case MessageSubTypeCodeList.Codes.RuleError:
						return new RuleErrorPrettyFormatter(message.MessageDetail as IRuleErrorResponseDetail);
					case MessageSubTypeCodeList.Codes.XmlSchemaError:
						return new XMLSchemaErrorPrettyFormatter(message.MessageDetail as IXMLSchemaErrorsResponseDetail);
					case MessageSubTypeCodeList.Codes.CustomsRejected:
						return new CustomsRejectionMessagePrettyFormatter(message.MessageDetail as ICustomsRejectionResponseDetail);
				}
				break;
			case MessageTypeCodeList.Codes.EBD:
				switch (message.EM_MessageSubType)
				{
					case MessageSubTypeCodeList.Codes.Accepted:
					case MessageSubTypeCodeList.Codes.CustomsRejected:
						return new EbdResponsePrettyFormatter(message.MessageDetail as IDocumentImportResponseDetail);
				}
				break;
			case MessageTypeCodeList.Codes.ECM:
				switch (message.EM_MessageSubType)
				{
					case MessageSubTypeCodeList.Codes.Accepted:
						return new EComAcceptancePrettyFormatter(message.MessageDetail as IEComAcceptanceResponseDetail);
					case MessageSubTypeCodeList.Codes.Request:
						return new EComRequestPrettyFormatter(message.Factory, message.MessageDetail as IEdecComplaintRequestDetail, message.IsTransmitMessage);
					case MessageSubTypeCodeList.Codes.RuleError:
						return new EComRuleErrorPrettyFormatter(message.MessageDetail as IEComRuleErrorsResponseDetail);
					case MessageSubTypeCodeList.Codes.XmlSchemaError:
						return new EComXMLSchemaErrorPrettyFormatter(message.MessageDetail as IEComXMLSchemaErrorsResponseDetail);
				}
				break;
			case MessageTypeCodeList.Codes.EVV:
				switch (message.EM_MessageSubType)
				{
					case MessageSubTypeCodeList.Codes.RuleError:
						return new EvvRuleErrorPrettyFormatter(message.MessageDetail as IEvvRuleErrorsResponseProvider);
					case MessageSubTypeCodeList.Codes.XmlSchemaError:
						return new EvvXMLSchemaErrorPrettyFormatter(message.MessageDetail as IEvvXMLSchemaErrorsResponseProvider);
				}
				break;
			case MessageTypeCodeList.Codes.MSG:
				switch (message.EM_MessageSubType)
				{
					case MessageSubTypeCodeList.Codes.PassarActivationResponse:
						return new NC124ResponsePrettyFormatter(message.Factory, message.MessageDetail as INC124ResponseDetail);
					case MessageSubTypeCodeList.Codes.PassarTechnicalError:
						return new NC909ResponsePrettyFormatter(message.Factory, message.MessageDetail as INC909ResponseDetail);
					case MessageSubTypeCodeList.Codes.PassarExportDeclarationActivationResponse:
						return new NE131ResponsePrettyFormatter(message.Factory, message.MessageDetail as INE131ResponseDetail);
					case MessageSubTypeCodeList.Codes.PassarExportDeclarationAmendmentResponse:
						return new Nxx04ResponsePrettyFormatter(message.Factory, message.MessageDetail as INxx04ResponseDetail);
					case MessageSubTypeCodeList.Codes.PassarExportWithdrawalResponseRejected:
						return new NE009ResponsePrettyFormatter(message.Factory, message.MessageDetail as INE009ResponseDetail);
					case MessageSubTypeCodeList.Codes.PassarExportDeclarationResponse:
						return new NE028ResponsePrettyFormatter(message.Factory, message.MessageDetail as INE028ResponseDetail);
					case MessageSubTypeCodeList.Codes.PassarExportControlDecisionNotification:
						return new NE060ResponsePrettyFormatter(message.Factory, message.MessageDetail as INE060ResponseDetail);
				}
				break;
		}

		var prettyFormatter = ObjectFactory.Get<ICHNctsMessagePrettyFormatterProvider>().GetFormatter(message);
		if (prettyFormatter != null)
		{
			return prettyFormatter;
		}

		if (message.IsUniversalEvent)
		{
			if (message.EM_MessageSubType == MessageSubTypeCodeList.Codes.Rejected || message.EM_MessageSubType == MessageSubTypeCodeList.Codes.Acknowledged)
			{
				return new UniversalEventPrettyFormatter(message.UniversalEventData);
			}
			return new UniversalEventXmlPrettyFormatter(message.UniversalEventData);
		}

		return null;
	}
}
