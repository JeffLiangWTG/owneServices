using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	sealed class AdditionalInfoLookupsHelperTest : TestCase
	{
		public void TestGetImportRefCusCodeListType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("When SubType is empty, RefCusCodeType", "", AdditionalInfoLookupsHelper.GetImportRefCusCodeListType(subType: ""));
				AssertEquals("When SubType is unknown, RefCusCodeType", "", AdditionalInfoLookupsHelper.GetImportRefCusCodeListType(subType: "XXX"));
				AssertEquals("When SubType is INF, RefCusCodeType", "AI44I", AdditionalInfoLookupsHelper.GetImportRefCusCodeListType(subType: "INF"));
				AssertEquals("When SubType is REF, RefCusCodeType", "AR44I", AdditionalInfoLookupsHelper.GetImportRefCusCodeListType(subType: "REF"));
				AssertEquals("When SubType is TRA, RefCusCodeType", "TD44I", AdditionalInfoLookupsHelper.GetImportRefCusCodeListType(subType: "TRA"));
			});
		}

		public void TestGetUCCExportRefCusCodeListType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("When SubType is empty, RefCusCodeType", "", AdditionalInfoLookupsHelper.GetUCCExportRefCusCodeListType(subType: ""));
				AssertEquals("When SubType is unknown, RefCusCodeType", "", AdditionalInfoLookupsHelper.GetUCCExportRefCusCodeListType(subType: "XXX"));
				AssertEquals("When SubType is INF, RefCusCodeType", "AI44E", AdditionalInfoLookupsHelper.GetUCCExportRefCusCodeListType(subType: "INF"));
				AssertEquals("When SubType is REF, RefCusCodeType", "AR44E", AdditionalInfoLookupsHelper.GetUCCExportRefCusCodeListType(subType: "REF"));
				AssertEquals("When SubType is TRA, RefCusCodeType", "TD44E", AdditionalInfoLookupsHelper.GetUCCExportRefCusCodeListType(subType: "TRA"));
			});
		}
	}
}
