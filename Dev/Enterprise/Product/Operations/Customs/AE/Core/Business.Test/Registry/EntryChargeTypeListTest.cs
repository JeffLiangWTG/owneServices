using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Registry.Testing
{
	[TestedType(typeof(EntryChargeTypeList))]
	sealed class EntryChargeTypeListTest : Enterprise.Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
	{
		public void TestList()
		{
			AssertListElementIsCorrect(ChargeTypeList[0], EntryChargeTypeList.Codes.DutyAmount, EntryChargeTypeList.Descriptions.DutyAmount, true, "");
			AssertListElementIsCorrect(ChargeTypeList[1], EntryChargeTypeList.Codes.RegistrationFee, EntryChargeTypeList.Descriptions.RegistrationFee, true, "");
		}

		protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetNewEntryChargeTypeList() => new EntryChargeTypeList();

		protected override ZString CountryCode => Core.Constants.CountryCodes.UnitedArabEmirates;
	}
}
