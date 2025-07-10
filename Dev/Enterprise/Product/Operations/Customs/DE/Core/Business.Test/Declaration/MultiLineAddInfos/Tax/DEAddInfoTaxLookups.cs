using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	public class DEAddInfoTaxLookupsTest : EUAddInfoTaxLookupsTest
	{
		public override void TestTypeList()
		{
			Assert("DE's behaviour is different to base EU", true);
		}

		public override void TestSortTypeList()
		{
			Assert("DE's behaviour is different to base EU", true);
		}
	}
}
