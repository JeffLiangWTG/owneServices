using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.DataTransfer.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.DataTransfer.Test.Universal
{
	public class CusSupportingInfoTypeListProviderTest : TestCaseWithFactory
	{
		public void TestCusSupportingInfoGetListForCusEntryInstruction() => AssertCusSupportingInfoGetList(CusInBondHeaderSchema.Constants.Prefix, "OTH, PRE");

		static void AssertCusSupportingInfoGetList(string tableCode, string expectedCodesAsString)
		{
			var list = (CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusSupportingInfoTypeList(tableCode, "");
			AssertEquals(expectedCodesAsString, list.CodesAsString);
		}
	}
}
