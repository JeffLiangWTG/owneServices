using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRInstrumentTariffGroupCollection))]
	sealed class CMRInstrumentTariffGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestHasThisTariffANdStatNumber()
		{
			var instrumentTariff = CMRInstrumentTariffGroup.New(Factory);
			instrumentTariff.IG_InstrumentNumber = "000000";
			instrumentTariff.IG_TariffGroupItem = "00001111";

			var collection = new CMRInstrumentTariffGroupCollection(Factory);
			collection.Add(instrumentTariff);

			AssertEquals("HasThisTariffAndStatNumber with the exact number", true, collection.HasThisTariffAndStatNumber("00001111"));
			AssertEquals("HasThisTariffAndStatNumber with shorter tariff number", false, collection.HasThisTariffAndStatNumber("0000"));
			AssertEquals("HasThisTariffAndStatNumber with dot and space", true, collection.HasThisTariffAndStatNumber("0000.11.11 11"));
			AssertEquals("HasThisTariffAndStatNumber with empty string", false, collection.HasThisTariffAndStatNumber(""));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new CMRInstrumentTariffGroupCollection(Factory);
	}
}
