using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackHelperTest : TestCaseWithFactory
	{
		public void TestConvertPackUQToCustomsUQ()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "XX";
			pack1.RP_Type = "";
			pack1.RP_CustomsCountry = "US";
			pack1.RP_ConversionFactor = 12;
			pack1.RP_CommercialPack = "XXX";

			Factory.Save();

			AssertNull("", AsycudaPackHelper.LoadRefPackForManifestLine(Factory, "XXX", "AU"));

			var pack2 = Factory.New<CusRefPacks>();
			pack2.RP_CustomsPack = "XX";
			pack2.RP_Type = "";
			pack2.RP_CustomsCountry = "AU";
			pack2.RP_ConversionFactor = 12;
			pack2.RP_CommercialPack = "XXX";

			Factory.Save();

			var loadedPack = AsycudaPackHelper.LoadRefPackForManifestLine(Factory, "XXX", "AU");

			AssertNotNull("", loadedPack);
			AssertEquals("", "XX", loadedPack.RP_CustomsPack);

			var pack3 = Factory.New<CusRefPacks>();
			pack3.RP_CustomsPack = "YY";
			pack3.RP_Type = "GMB";
			pack3.RP_CustomsCountry = "AU";
			pack3.RP_ConversionFactor = 12;
			pack3.RP_CommercialPack = "YYY";

			Factory.Save();

			loadedPack = AsycudaPackHelper.LoadRefPackForManifestBill(Factory, "YYY", "AU");

			AssertNotNull("", loadedPack);
			AssertEquals("", "YY", loadedPack.RP_CustomsPack);
		}
	}
}
