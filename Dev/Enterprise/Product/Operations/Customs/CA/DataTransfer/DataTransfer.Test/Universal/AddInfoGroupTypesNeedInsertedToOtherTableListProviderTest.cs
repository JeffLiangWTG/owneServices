using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest
	{
		public void TestAddInfoGroupTypesNeedInsertedToOtherTableListForJobDeclaration()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(JobDeclarationSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(Constants.AddInfoKeys.CusCALPCO.CusAddInfoType, "LPCO", list.GetDescriptionFromCode(Constants.AddInfoKeys.CusCALPCO.CusAddInfoType));
		}

		public void TestAddInfoGroupTypesNeedInsertedToOtherTableListForCusAddInfo()
		{
			var list = new UniversalCustomsDataObjectProvider().TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(CusAddInfoSchema.Constants.Prefix);
			AssertEquals(1, list.Count);
			AssertEquals(Constants.AddInfoKeys.CusCALPCO.CusAddInfoType, "LPCO", list.GetDescriptionFromCode(Constants.AddInfoKeys.CusCALPCO.CusAddInfoType));
		}
	}
}
