using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRInstrument))]
	sealed class CMRInstrumentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoad()
		{
			var instrument1 = CMRInstrument.New(Factory);
			instrument1.IN_Number = "111111";

			var instrument2 = CMRInstrument.New(Factory);
			instrument2.IN_Number = "222222";

			AssertEquals("Load with 111111", instrument1, CMRInstrument.Load(Factory, "111111"));
			AssertEquals("Load with 222222", instrument2, CMRInstrument.Load(Factory, "222222"));
			AssertEquals("Load with 333333", null, CMRInstrument.Load(Factory, "333333"));
		}

		public void TestIsValidForThisDate()
		{
			var instrument = CMRInstrument.New(Factory);
			instrument.IN_Number = "111111";
			instrument.IN_StartDate = new ZDateTime(2005, 1, 1);

			AssertEquals("Is not valid for this date as this date is empty", false, instrument.IsValidForThisDate(ZDateTime.Empty));
			AssertEquals("Is not Valid for this date", false, instrument.IsValidForThisDate(new ZDateTime(2004, 12, 31)));
			AssertEquals("Is Valid for this date", true, instrument.IsValidForThisDate(new ZDateTime(2005, 1, 1)));
			AssertEquals("Is Valid for this date", true, instrument.IsValidForThisDate(new ZDateTime(2005, 1, 2)));

			instrument.IN_EndDate = new ZDateTime(2005, 1, 1);
			AssertEquals("Is not Valid for this date", false, instrument.IsValidForThisDate(new ZDateTime(2004, 12, 31)));
			AssertEquals("Is Valid for this date", true, instrument.IsValidForThisDate(new ZDateTime(2005, 1, 1)));
			AssertEquals("Is not Valid for this date", false, instrument.IsValidForThisDate(new ZDateTime(2005, 1, 2)));

			instrument.IN_EndDate = ZDateTime.Empty;
			instrument.IN_RevocationDate = new ZDateTime(2005, 1, 1);
			AssertEquals("Is not Valid for this date", false, instrument.IsValidForThisDate(new ZDateTime(2004, 12, 31)));
			AssertEquals("Is Valid for this date", true, instrument.IsValidForThisDate(new ZDateTime(2005, 1, 1)));
			AssertEquals("Is not Valid for this date", false, instrument.IsValidForThisDate(new ZDateTime(2005, 1, 2)));

			instrument.IN_RevocationDate = new ZDateTime(2005, 1, 1);
			instrument.IN_EndDate = new ZDateTime(2005, 1, 2);
			AssertEquals("Is not Valid for this date", false, instrument.IsValidForThisDate(new ZDateTime(2004, 12, 31)));
			AssertEquals("Is Valid for this date", true, instrument.IsValidForThisDate(new ZDateTime(2005, 1, 1)));
			AssertEquals("Is not Valid for this date", false, instrument.IsValidForThisDate(new ZDateTime(2005, 1, 2)));
			AssertEquals("Is not Valid for this date", false, instrument.IsValidForThisDate(new ZDateTime(2005, 1, 3)));
		}

		public void TestTariffGroupRelevant()
		{
			var instrumentTariff = CMRInstrumentTariffGroup.New(Factory);
			instrumentTariff.IG_TariffGroupItem = "00000000";
			instrumentTariff.IG_InstrumentType = "XX";
			instrumentTariff.IG_InstrumentNumber = "9999";

			var instrumentTariff2 = CMRInstrumentTariffGroup.New(Factory);
			instrumentTariff2.IG_TariffGroupItem = "00000002";
			instrumentTariff2.IG_InstrumentType = "XX";
			instrumentTariff2.IG_InstrumentNumber = "9990";

			var instrumentTariff3 = CMRInstrumentTariffGroup.New(Factory);
			instrumentTariff3.IG_TariffGroupItem = "00000002";
			instrumentTariff3.IG_InstrumentType = "XY";
			instrumentTariff3.IG_InstrumentNumber = "9999";

			var instrument = CMRInstrument.New(Factory);
			instrument.IN_Number = "9999";
			instrument.IN_Type = "XX";

			var tariffRelated = instrument.TariffGroupRelevant;

			AssertEquals("Tariff related contains 00000000", true, tariffRelated.Contains("00000000"));
			AssertEquals("Tariff related does not contain 00000001", false, tariffRelated.Contains("00000001"));
			AssertEquals("Tariff related does not contain 00000002", false, tariffRelated.Contains("00000002"));
		}

		protected override BusinessObject GetNewBusinessObject() => CMRInstrument.New(Factory);
	}
}
