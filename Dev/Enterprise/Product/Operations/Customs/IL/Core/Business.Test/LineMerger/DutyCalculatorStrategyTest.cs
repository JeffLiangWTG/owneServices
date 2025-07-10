using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	sealed class DutyCalculatorStrategyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		public override void TestCalculateDuties()
		{
			SetupReferenceData();
			SetupReferenceData("EXC", "RC2", "CV + RC1");
			SetupVATReferenceData();
			var dutyCalculatorStrategy = GetDutyCalculatorStrategy();

			CombineAssertions(() =>
			{
				var entryLine1 = GetEntryHeader(Declaration, "0101100000").MergedLines[0];
				entryLine1.RandomLine.JI_ZZF_NKTaxType = "VAT";
				var entryLine2 = GetEntryHeader(Declaration, "0102200000").MergedLines[0];
				entryLine2.RandomLine.JI_ZZF_NKTaxType = "VZR";

				AssertEquals("Precondition: entryLine1", 0, entryLine1.Fees.Count);
				AssertEquals("Precondition: entryLine2", 0, entryLine2.Fees.Count);

				dutyCalculatorStrategy.CalculateDuties();
				AssertEquals("entryLine1: Duty & Excise calculated and added to Fee", 2, entryLine1.Fees.Count);
				AssertEquals("entryLine1: DTY/RC1 - RateCode", "RC1", entryLine1.Fees[0].CF_ChargeType);
				AssertEquals("entryLine1: DTY/RC1 - Amout = CV * 0.8", 17.60m, entryLine1.Fees[0].CF_ChargeAmount);
				AssertEquals("entryLine1: DTY/15 - RateCode", "15", entryLine1.Fees[1].CF_ChargeType);
				AssertEquals("entryLine1: DTY/15 - Amout = CV * 0.17", 6.732m, entryLine1.Fees[1].CF_ChargeAmount);

				AssertEquals("entryLine2: Duty & Excise calculated and added to Fee", 1, entryLine2.Fees.Count);
				AssertEquals("entryLine2: DTY/RC1 - RateCode", "RC1", entryLine2.Fees[0].CF_ChargeType);
				AssertEquals("entryLine2: DTY/RC1 - Amout = CV * 0.5", -5m, entryLine2.Fees[0].CF_ChargeAmount);
			});
		}

		protected override bool ExpectedShouldCalculateDuties => true;

		protected override ZString UniversalTariffType => Universal.Constants.TariffTypes.Import;

		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

		void SetupVATReferenceData()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			refDataHelper.CreateTaxOrFee("VAT", 0.17m, currentCountryCode, startDate, endDate);
			refDataHelper.CreateTaxOrFee("VZR", 0.00m, currentCountryCode, startDate, endDate);
			Factory.Save();
		}
	}
}
