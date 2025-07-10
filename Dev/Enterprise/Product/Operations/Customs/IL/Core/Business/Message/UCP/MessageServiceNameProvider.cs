using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business
{
	public static class MessageServiceNameProvider
	{
		public static string GetServiceName(string messageSubType)
		{
			switch (messageSubType)
			{
				case ILEDIMessageSubTypeList.Codes.DeliveryOrderRequest:
					return CustomsServiceName.DeliveryOrderRequest;
				case ILEDIMessageSubTypeList.Codes.GatepassMovementRequest:
					return CustomsServiceName.GatePassMovementRequest;
				case ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest:
					return CustomsServiceName.ForwarderManifestRequest;
				case ILEDIMessageSubTypeList.Codes.CurrencyRateRequest:
					return CustomsServiceName.CurrencyRateRequest;
				case ILEDIMessageSubTypeList.Codes.ImportDeclarationRequest:
					return CustomsServiceName.ImportDeclarationRequest;
				case ILEDIMessageSubTypeList.Codes.SystemTableRequest:
					return CustomsServiceName.SystemTableRequest;
				case ILEDIMessageSubTypeList.Codes.SyncOutgoingMessageRequest:
					return CustomsServiceName.Sync9100OutgoingMessageRequest;
				case ILEDIMessageSubTypeList.Codes.SyncAcknowledgementMessage:
					return CustomsServiceName.Sync9200OutgoingMessageRequest;
				case ILEDIMessageSubTypeList.Codes.SupportingDocumentsRequest:
					return CustomsServiceName.SupportingDocumentsRequest;
				case ILEDIMessageSubTypeList.Codes.ManifestQueryRequest:
					return CustomsServiceName.ManifestQueryRequest;
				case ILEDIMessageSubTypeList.Codes.ExportDeclarationRequest:
					return CustomsServiceName.ExportDeclarationRequest;

				default:
					return string.Empty;
			}
		}
	}
}
