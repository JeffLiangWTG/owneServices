using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest
	{
		public void TestCusSupportingInfoGetListForJobComInvoiceHeader() => AssertCusSupportingInfoGetList(JobComInvoiceHeaderSchema.Constants.Prefix);

		public void TestCusSupportingInfoGetListForJobComInvoiceLine() => AssertCusSupportingInfoGetList(JobComInvoiceLineSchema.Constants.Prefix);

		public void TestCusSupportingInfoGetListForJobDeclaration() => AssertCusSupportingInfoGetList(JobDeclarationSchema.Constants.Prefix);

		public void TestCusSupportingInfoGetListForNctsGoodsItem() => AssertCusSupportingInfoGetList(CusInBondCargoDescSchema.Constants.Prefix);

		public void TestCusSupportingInfoGetListForNctsHeader() => AssertCusSupportingInfoGetList(CusInBondHeaderSchema.Constants.Prefix, "OTH, PRE");

		public void TestCusSupportingInfoGetListForNctsMovementHeader() => AssertCusSupportingInfoGetList(CusInBondMoveHeaderSchema.Constants.Prefix, "SUP");
		static void AssertCusSupportingInfoGetList(string tableCode, string expectedCodesAsString = "OTH, PRE, SUP, GVM")
		{
			var list = (CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusSupportingInfoTypeList(tableCode, "");
			AssertEquals(expectedCodesAsString, list.CodesAsString);
		}
	}
}
