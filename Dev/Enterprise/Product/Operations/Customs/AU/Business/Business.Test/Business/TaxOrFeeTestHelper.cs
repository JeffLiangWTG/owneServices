using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public sealed class TaxOrFeeTestHelper
	{
		public static void SetDeminimus(BusinessObjectFactory factory, ZDecimal deminimus)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateTaxOrFee("DEM", deminimus, "AU", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 6, 6), "Deminimus");
			factory.Save();
			UniversalReferenceHelper.InvalidateCache(factory);
		}

		public static void SetUp(BusinessObjectFactory factory = null)
		{
			SetDeminimus(factory ?? new BusinessObjectFactory(), 1000m);
		}
	}
}
