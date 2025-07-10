using CargoWise.Types;
using Enterprise.Registry.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUASPEntryChargeTypeList))]
	sealed class AUASPEntryChargeTypeListTest : Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
	{
		protected override ZString CountryCode => Core.Constants.CountryCodes.Australia;

		protected override EntryChargeTypeList GetNewEntryChargeTypeList()
		{
			return new AUASPEntryChargeTypeList();
		}

		public void TestList()
		{
			AssertEquals(1, ChargeTypeList.Count);
			AssertListElementIsCorrect(ChargeTypeList[0], Registry.Business.Customs.AU.EntryChargeTypeList.Codes.AQISServicePaymentAmount, Registry.Business.Customs.AU.EntryChargeTypeList.Descriptions.AQISServicePaymentAmount, true, "");
		}
	}
}
