using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest
	{
		public void TestCusCodeDataGetTypeListForJobDeclaration()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(JobDeclarationSchema.Constants.Prefix, "");
			AssertEquals(1, list.Count);
			AssertEquals(CusCodeDataTypeList.Codes.OfficeCode, CusCodeDataTypeList.Descriptions.OfficeCode, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.OfficeCode));
		}

		public void TestCusCodeDataGetTypeListForJobComInvoiceLine()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(JobComInvoiceLineSchema.Constants.Prefix, "");
			AssertEquals(2, list.Count);
			AssertEquals(CusCodeDataTypeList.Codes.SupplementaryCode, CusCodeDataTypeList.Descriptions.SupplementaryCode, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.SupplementaryCode));
			AssertEquals(CusCodeDataTypeList.Codes.AdditionalProcedureCode, CusCodeDataTypeList.Descriptions.AdditionalProcedureCode, list.GetDescriptionFromCode(CusCodeDataTypeList.Codes.AdditionalProcedureCode));
		}

		public void TestCusCodeDataGetTypeListForCusInBondCargoDesc()
		{
			var cusCodeDataTypeList = new UniversalCustomsDataObjectProvider().TableSpecificCusCodeDataTypeList(CusInBondCargoDescSchema.Constants.Prefix, "");
			CombineAssertions(() =>
			{
				AssertEquals("CusCodeDataTypeList Count", 1, cusCodeDataTypeList.Count);
				AssertEquals($"ContainsCode '{CusCodeDataTypeList.Codes.SupplementaryCode}'?", true, cusCodeDataTypeList.ContainsCode(CusCodeDataTypeList.Codes.SupplementaryCode));
				AssertEquals($"CusCodeDataTypeList['{CusCodeDataTypeList.Codes.SupplementaryCode}'] description", CusCodeDataTypeList.Descriptions.SupplementaryCode, cusCodeDataTypeList.GetDescriptionFromCode(CusCodeDataTypeList.Codes.SupplementaryCode));
			});
		}
	}
}
