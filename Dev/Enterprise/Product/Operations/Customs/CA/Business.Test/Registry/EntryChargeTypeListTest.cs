using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(EntryChargeTypeList))]
	sealed class EntryChargeTypeListTest : Enterprise.Registry.Business.Customs.Testing.EntryChargeTypeListTestCase
	{
		public void TestList()
		{
			AssertListElementIsCorrect(ChargeTypeList[0], EntryChargeTypeList.Codes.CustomsValueForTax, EntryChargeTypeList.Descriptions.CustomsValueForTax, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[1], EntryChargeTypeList.Codes.CustomsValueInInvoiceCurrency, EntryChargeTypeList.Descriptions.CustomsValueInInvoiceCurrency, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[2], EntryChargeTypeList.Codes.TotalDutyAmount, EntryChargeTypeList.Descriptions.TotalDutyAmount, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[3], EntryChargeTypeList.Codes.Duty1, EntryChargeTypeList.Descriptions.Duty1, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[4], EntryChargeTypeList.Codes.Duty2, EntryChargeTypeList.Descriptions.Duty2, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[5], EntryChargeTypeList.Codes.Duty3, EntryChargeTypeList.Descriptions.Duty3, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[6], EntryChargeTypeList.Codes.TotalExciseTaxAmount, EntryChargeTypeList.Descriptions.TotalExciseTaxAmount, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[7], EntryChargeTypeList.Codes.TotalGSTAmount, EntryChargeTypeList.Descriptions.TotalGSTAmount, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[8], EntryChargeTypeList.Codes.TotalSIMAAmount, EntryChargeTypeList.Descriptions.TotalSIMAAmount, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[9], EntryChargeTypeList.Codes.TotalNonBillableSIMAAmount, EntryChargeTypeList.Descriptions.TotalNonBillableSIMAAmount, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[10], EntryChargeTypeList.Codes.K84LateFilingPenalty, EntryChargeTypeList.Descriptions.K84LateFilingPenalty, false, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[11], EntryChargeTypeList.Codes.TotalGSTDirectAmount, EntryChargeTypeList.Descriptions.TotalGSTDirectAmount, true, ZString.Empty);
			AssertListElementIsCorrect(ChargeTypeList[12], EntryChargeTypeList.Codes.Others, EntryChargeTypeList.Descriptions.Others, false, ZString.Empty);
		}

		protected override Enterprise.Registry.Business.Customs.EntryChargeTypeList GetNewEntryChargeTypeList()
		{
			return new EntryChargeTypeList();
		}

		protected override ZString CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
