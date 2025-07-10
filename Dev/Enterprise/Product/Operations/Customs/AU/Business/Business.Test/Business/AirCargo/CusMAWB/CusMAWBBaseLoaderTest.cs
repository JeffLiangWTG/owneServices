using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusMAWBBase.Loader))]
	sealed class CusMAWBBaseLoaderTest : LoaderTestCase
	{
		public void TestAUAppCodes()
		{
			AssertArrayEqualsByElements("AU Codes are the same", CusMAWBBase.Loader.CMRApplicationCodes, ForwardingConsol.GetAUCusMAWBAppCodes());
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new CusMAWBBase.Loader(Factory);
	}
}
