using CargoWise.EntityFramework.Testing;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class MessageSignatureTypeProviderTest : TestCaseWithFactory
	{
		public void GetSignType_ShouldReturnCompanySignatureType_ForSpecificMessageSubTypes()
		{
			var result1 = MessageSignatureTypeProvider.GetSignType(ILEDIMessageSubTypeList.Codes.GatepassMovementRequest);
			var result2 = MessageSignatureTypeProvider.GetSignType(ILEDIMessageSubTypeList.Codes.DeliveryOrderRequest);
			var result3 = MessageSignatureTypeProvider.GetSignType(ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest);
			var result4 = MessageSignatureTypeProvider.GetSignType(ILEDIMessageSubTypeList.Codes.SupportingDocumentsRequest);

			AssertEquals("130", MessageSignatureType.CompanySignature, result1);
			AssertEquals("120", MessageSignatureType.CompanySignature, result2);
			AssertEquals("170", MessageSignatureType.CompanySignature, result3);
			AssertEquals("271", MessageSignatureType.CompanySignature, result4);
		}

		public void GetSignType_ShouldReturnPersonalSignatureType_ForSpecificMessageSubTypes()
		{
			var result1 = MessageSignatureTypeProvider.GetSignType(ILEDIMessageSubTypeList.Codes.ImportDeclarationRequest);
			var result2 = MessageSignatureTypeProvider.GetSignType(ILEDIMessageSubTypeList.Codes.ExportDeclarationRequest);

			AssertEquals("275", MessageSignatureType.PersonalSignature, result1);
			AssertEquals("751", MessageSignatureType.PersonalSignature, result2);
		}

		public void GetSignType_ShouldReturnNoneSignatureType_ForUnknownMessageSubType()
		{
			var unknownMessageSubType = "UnknownSubType";
			var result = MessageSignatureTypeProvider.GetSignType(unknownMessageSubType);
			AssertEquals(MessageSignatureType.NoneSignatureType, result);
		}
	}
}
