using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest
	{
		public void TestCusCodeDataGetTypeListForJobDeclaration()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(JobDeclarationSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(CusCodeDataTypeList.Codes.Permit, CusCodeDataTypeList.Descriptions.Permit, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.Permit));
		}

		public void TestCusCodeDataGetTypeListForJobComInvoiceLine()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(3, list.Count);
			AssertEquals(CusCodeDataTypeList.Codes.CFIANumber, CusCodeDataTypeList.Descriptions.CFIANumber, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.CFIANumber));
			AssertEquals(CusCodeDataTypeList.Codes.SITTNumber, CusCodeDataTypeList.Descriptions.SITTNumber, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.SITTNumber));
			AssertEquals(CusCodeDataTypeList.Codes.Permit, CusCodeDataTypeList.Descriptions.Permit, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.Permit));
		}

		public void TestCusCodeDataGetCodeListForJobComInvoiceLine()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataCodeList(JobComInvoiceLineSchema.Constants.Prefix);
			AssertEquals(3, list.Count);
			AssertEquals(CusCodeDataTypeList.Codes.CFIANumber, CusCodeDataTypeList.Descriptions.CFIANumber, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.CFIANumber));
			AssertEquals(CusCodeDataTypeList.Codes.SITTNumber, CusCodeDataTypeList.Descriptions.SITTNumber, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.SITTNumber));
			AssertEquals(CusCodeDataTypeList.Codes.Permit, CusCodeDataTypeList.Descriptions.Permit, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.Permit));
		}

		public void TestCusCodeDataGetTypeListForCusAddInfo()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(CusAddInfoSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(CusCodeDataTypeList.Codes.AIRSNumber, CusCodeDataTypeList.Descriptions.AIRSNumber, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.AIRSNumber));
		}
	}
}
