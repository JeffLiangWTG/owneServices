using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(RollCollection))]
	sealed class RollCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHasExporterRoll()
		{
			var wrapper = new OrgHeaderWrapper(Factory.NewWithValidTestData<OrgHeader>());
			var dataProvider = wrapper.CLREGInfoProvider;
			var roll = Factory.New<Roll>();
			roll.ZA_Roll = CMRClientRolls.Codes.AirCargoReporter;

			dataProvider.Rolls.Add(roll);
			AssertEquals("Has Exporter roll", false, dataProvider.Rolls.HasExporterRoll);

			roll = Factory.New<Roll>();
			roll.ZA_Roll = CMRClientRolls.Codes.Exporter;

			dataProvider.Rolls.Add(roll);
			AssertEquals("Has Exporter roll", true, dataProvider.Rolls.HasExporterRoll);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var wrapper = new OrgHeaderWrapper(Factory.NewWithValidTestData<OrgHeader>());
			return new RollCollection(wrapper.CLREGInfoProvider);
		}
	}
}
