using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business
{
	public static class MessageSignatureTypeProvider
	{
		public static string GetSignType(string messageSubType)
		{
			switch (messageSubType)
			{
				case ILEDIMessageSubTypeList.Codes.GatepassMovementRequest:
				case ILEDIMessageSubTypeList.Codes.DeliveryOrderRequest:
				case ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest:
				case ILEDIMessageSubTypeList.Codes.SupportingDocumentsRequest:
					return MessageSignatureType.CompanySignature;

				case ILEDIMessageSubTypeList.Codes.ImportDeclarationRequest:
				case ILEDIMessageSubTypeList.Codes.ExportDeclarationRequest:
					return MessageSignatureType.PersonalSignature;

				default:
					return MessageSignatureType.NoneSignatureType;
			}
		}
	}
}
