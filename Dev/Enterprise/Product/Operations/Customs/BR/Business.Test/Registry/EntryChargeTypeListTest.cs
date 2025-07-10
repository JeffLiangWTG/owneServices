using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Registry.Testing
{
	[TestedType(typeof(EntryChargeTypeList))]
	sealed class EntryChargeTypeListTest : Enterprise.Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
	{
		public void TestList()
		{
			AssertListElementIsCorrect(ChargeTypeList[0], EntryChargeTypeList.Codes.Antidumping, EntryChargeTypeList.Descriptions.Antidumping, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[1], EntryChargeTypeList.Codes.Cofins, EntryChargeTypeList.Descriptions.Cofins, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[2], EntryChargeTypeList.Codes.DutyAmount, EntryChargeTypeList.Descriptions.DutyAmount, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[3], EntryChargeTypeList.Codes.ICMS, EntryChargeTypeList.Descriptions.ICMS, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[4], EntryChargeTypeList.Codes.ImportLicenseFine, EntryChargeTypeList.Descriptions.ImportLicenseFine, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[5], EntryChargeTypeList.Codes.IPI, EntryChargeTypeList.Descriptions.IPI, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[6], EntryChargeTypeList.Codes.PIS, EntryChargeTypeList.Descriptions.PIS, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[7], EntryChargeTypeList.Codes.SiscomexUsageEntryFee, EntryChargeTypeList.Descriptions.SiscomexUsageEntryFee, true, ZString.Empty);
		}

		protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetNewEntryChargeTypeList() => new EntryChargeTypeList();

		protected override ZString CountryCode => Core.Constants.CountryCodes.Brazil;
	}
}
