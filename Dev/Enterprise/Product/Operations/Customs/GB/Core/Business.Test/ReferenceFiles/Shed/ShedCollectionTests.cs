using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(ShedCollection))]
	class ShedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ShedCollection>
	{
		public void TestLoad_DataGrouping()
		{
			var shedCollection = new ShedCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			AssertContainsExactElementsInAnyOrder(new[] { "LHRAAS", "LHRACS", "MANAAS" }, shedCollection.Cast<Shed>().Select(x => x.Code));
		}

		public void TestLoad_DataGrouping_PortCode()
		{
			var shedCollection = new ShedCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom, "LHR", "");
			AssertContainsExactElementsInAnyOrder(new[] { "LHRAAS", "LHRACS" }, shedCollection.Cast<Shed>().Select(x => x.Code));
		}

		public void TestLoad_DataGrouping_ShedCode()
		{
			var shedCollection = new ShedCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom, "", "AAS");
			AssertContainsExactElementsInAnyOrder(new[] { "LHRAAS", "MANAAS" }, shedCollection.Cast<Shed>().Select(x => x.Code));
		}

		public void TestLoad_DataGrouping_PortCode_ShedCode()
		{
			var shedCollection = new ShedCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom, "LHR", "AAS");
			AssertContainsExactElementsInAnyOrder(new[] { "LHRAAS" }, shedCollection.Cast<Shed>().Select(x => x.Code));
		}

		protected override ShedCollection GetCollectionToTest() => new ShedCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new Shed(Factory.New<ZZRefCusCodeListCombined>());

		protected override void SetUp()
		{
			base.SetUp();
			ShedTest.CreateShed(Factory, Core.Constants.CountryCodes.UnitedKingdom, "LHRAAS", "AMERICAN AIRLINES at Heathrow");
			ShedTest.CreateShed(Factory, Core.Constants.CountryCodes.UnitedKingdom, "LHRACS", "AIR CANADA  at Heathrow");
			ShedTest.CreateShed(Factory, Core.Constants.CountryCodes.UnitedKingdom, "MANAAS", "AMERICAN AIRLINES FALLBACK MANAAS at Manchester");
			ShedTest.CreateShed(Factory, Core.Constants.CountryCodes.France, "CDGAAS", "AMERICAN AIRLINES at Charles de Gaulle");
			Factory.Save();
		}
	}
}
