using System.Linq;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AIRSRegistrationNumberCollection))]
	sealed class AIRSRegistrationNumberCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<AIRSRegistrationNumber>
	{
		public void TestAddNewIfNotExist()
		{
			var header = Factory.New<CFIAPGAHeader>();
			var collection = new AIRSRegistrationNumberCollection(header);
			collection.AddNewIfNotExist("01");
			AssertContainsExactElementsInAnyOrder(new[] { "01" }, collection.Cast<AIRSRegistrationNumber>().Select(x => x.CY_Code));

			collection.AddNewIfNotExist("01");
			AssertContainsExactElementsInAnyOrder(new[] { "01" }, collection.Cast<AIRSRegistrationNumber>().Select(x => x.CY_Code));

			collection.AddNewIfNotExist("02");
			AssertContainsExactElementsInAnyOrder(new[] { "01", "02" }, collection.Cast<AIRSRegistrationNumber>().Select(x => x.CY_Code));
		}

		protected override CusCodeDataCollection<AIRSRegistrationNumber> GetCusCodeDataCollection()
		{
			var header = Factory.New<CFIAPGAHeader>();
			return new AIRSRegistrationNumberCollection(header);
		}
	}
}
