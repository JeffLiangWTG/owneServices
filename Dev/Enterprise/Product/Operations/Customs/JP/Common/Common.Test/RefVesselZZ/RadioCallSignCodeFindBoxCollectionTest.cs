using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(RadioCallSignCodeFindBoxCollection))]
	sealed class RadioCallSignCodeFindBoxCollectionTest : ActiveBusinessObjectCollectionTestCase<RadioCallSignCodeFindBoxCollection>
	{
		public void TestGetCachedCollection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateVesselZZ("LA BOUDEUSE", "LXBH", "CV", "JP");
			helper.CreateVesselZZ("A P MOLLER", "OVYQ2", "CV", "JP");
			Factory.Save();

			var radioCallSigns = RadioCallSignCodeFindBoxCollection.GetCachedCollection(Factory, "LXBH");
			AssertContainsExactElementsInAnyOrder(["LA BOUDEUSE", "A P MOLLER"], radioCallSigns.Cast<RefVesselZZForRadioCallSign>().Select(c => c.ZZO_Code));
		}

		protected override RadioCallSignCodeFindBoxCollection GetCollectionToTest()
		{
			return new RadioCallSignCodeFindBoxCollection(Factory);
		}
	}
}
