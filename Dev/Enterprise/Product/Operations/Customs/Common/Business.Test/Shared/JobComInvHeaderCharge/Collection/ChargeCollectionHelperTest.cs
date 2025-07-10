using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class ChargeCollectionHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetChargeDutiableAndIncludedInITOTAndGSTApplicable()
		{
			var charge1 = testCollection.AddNew();
			charge1.J7_ChargeType = "OTH";
			charge1.J7_IsDutiable = true;
			charge1.J7_IsIncludedInITOT = true;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_Amount = 100m;
			charge1.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			var charge2 = testCollection.AddNew();
			charge2.J7_ChargeType = "OTH";
			charge2.J7_IsDutiable = false;
			charge2.J7_IsIncludedInITOT = true;
			charge2.J7_IsGSTApplicable = true;
			charge2.J7_Amount = 50;
			charge2.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			var charge3 = testCollection.AddNew();
			charge3.J7_ChargeType = "OTH";
			charge3.J7_IsDutiable = false;
			charge3.J7_IsIncludedInITOT = false;
			charge3.J7_IsGSTApplicable = true;
			charge3.J7_Amount = 35;
			charge3.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			var charge4 = testCollection.AddNew();
			charge4.J7_ChargeType = "OTH";
			charge4.J7_IsDutiable = false;
			charge4.J7_IsIncludedInITOT = false;
			charge4.J7_IsGSTApplicable = false;
			charge4.J7_Amount = 25;
			charge4.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(testCollection.GetCharge(true, true, true).Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "GetCharge return 100");
				NUnit.Framework.Assert.That(testCollection.GetCharge(false, true, true).Amount, Is.EqualTo(50m).Using(CustomComparers.TypeComparison), "GetCharge return 50");
				NUnit.Framework.Assert.That(testCollection.GetCharge(false, false, true).Amount, Is.EqualTo(35m).Using(CustomComparers.TypeComparison), "GetCharge return 35");
				NUnit.Framework.Assert.That(testCollection.GetCharge(false, false, false).Amount, Is.EqualTo(25m).Using(CustomComparers.TypeComparison), "GetCharge return 25");
			});
		}

		[ExpectNoExceptions]
		public void TestGetChargeWithChargeKeys()
		{
			JobComInvCharge oFT = testCollection.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 100m;
			oFT.J7_RX_NKCurrency = "USD";

			JobComInvCharge oNS = testCollection.AddNew();
			oNS.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			oNS.J7_Amount = 370m;
			oNS.J7_RX_NKCurrency = "USD";

			JobComInvCharge fIFT = testCollection.AddNew();
			fIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			fIFT.J7_Amount = 40m;
			fIFT.J7_RX_NKCurrency = "AUD";

			CurrencyConverter currencyConverter = invoice.CurrencyConverter;
			Money result = testCollection.GetCharge(currencyConverter, new ChargeCodeChargeKey[] { oFT.ChargeKey, oNS.ChargeKey });
			NUnit.Framework.Assert.That(result.Amount, Is.EqualTo(470m).Using(CustomComparers.TypeComparison), "OFT + ONS");
			NUnit.Framework.Assert.That(result.Currency.Code, Is.EqualTo("USD"), "Currency");

			result = testCollection.GetCharge(currencyConverter, new ChargeCodeChargeKey[] { fIFT.ChargeKey });
			NUnit.Framework.Assert.That(result.Amount, Is.EqualTo(40m).Using(CustomComparers.TypeComparison), "FIFT");
			NUnit.Framework.Assert.That(result.Currency.Code, Is.EqualTo("AUD"), "Currency");
		}

		[ExpectNoExceptions]
		public void TestGetChargeWithApportionChargeKey()
		{
			JobComInvCharge oFTFull = testCollection.AddNew();
			oFTFull.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFTFull.J7_Amount = 100m;
			oFTFull.J7_RX_NKCurrency = "USD";
			oFTFull.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;

			JobComInvCharge oFTPartial = testCollection.AddNew();
			oFTPartial.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFTPartial.J7_Amount = 150m;
			oFTPartial.J7_RX_NKCurrency = "AUD";
			oFTPartial.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.PartialApportionment;

			Money result = testCollection.GetCharge(oFTFull.ApportionChargeKey);
			NUnit.Framework.Assert.That(result.Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "GetCharge with full charge key");
			NUnit.Framework.Assert.That(result.Currency.Code, Is.EqualTo("USD"), "GetCharge with full charge key");

			result = testCollection.GetCharge(oFTPartial.ApportionChargeKey);
			NUnit.Framework.Assert.That(result.Amount, Is.EqualTo(150m).Using(CustomComparers.TypeComparison), "GetCharge with Partial charge key");
			NUnit.Framework.Assert.That(result.Currency.Code, Is.EqualTo("AUD"), "GetCharge with Partial charge key");
		}

		[ExpectNoExceptions]
		public void TestFindWithApportionChargeKey()
		{
			JobComInvCharge oFTFull = testCollection.AddNew();
			oFTFull.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFTFull.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;

			JobComInvCharge oFTPartial = testCollection.AddNew();
			oFTPartial.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFTPartial.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.PartialApportionment;

			JobComInvCharge[] charges = testCollection.Find(oFTFull.ApportionChargeKey);
			NUnit.Framework.Assert.That(charges.Length, Is.EqualTo(1), "One charge only");
			NUnit.Framework.Assert.That(charges[0], Is.EqualTo(oFTFull), "It should be OFTFull");

			charges = testCollection.Find(oFTPartial.ApportionChargeKey);
			NUnit.Framework.Assert.That(charges.Length, Is.EqualTo(1), "One charge only");
			NUnit.Framework.Assert.That(charges[0], Is.EqualTo(oFTPartial), "It should be OFTPartial");
		}

		[ExpectNoExceptions]
		public void TestHasChargesDistributedBy()
		{
			JobComInvCharge oFT = testCollection.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_DistributeBy = ChargeDistributeByList.Codes.Value;

			NUnit.Framework.Assert.That(testCollection.HasChargeDistributedBy(ChargeDistributeByList.Codes.Weight), Is.EqualTo(false), "Distribute by value only");
			NUnit.Framework.Assert.That(testCollection.HasChargeDistributedBy(ChargeDistributeByList.Codes.Volume), Is.EqualTo(false), "Distribute by value only");
			NUnit.Framework.Assert.That(testCollection.HasChargeDistributedBy(ChargeDistributeByList.Codes.Value), Is.EqualTo(true), "Distribute by value only");
		}

		[ExpectNoExceptions]
		public void TestHasChargesWithCurrencyWithApportionChargeKey()
		{
			JobComInvCharge oFTFull = testCollection.AddNew();
			oFTFull.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFTFull.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;
			oFTFull.J7_Amount = 100m;
			oFTFull.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			JobComInvCharge oFTPartial = testCollection.AddNew();
			oFTPartial.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFTPartial.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.PartialApportionment;

			NUnit.Framework.Assert.That(testCollection.HasChargeWithCurrency(oFTFull.ApportionChargeKey), Is.EqualTo(true), "HasChargesWithCurrency");
			NUnit.Framework.Assert.That(testCollection.HasChargeWithCurrency(oFTPartial.ApportionChargeKey), Is.EqualTo(false), "HasChargeWithCurrency with OFTPartial");
		}

		[ExpectNoExceptions]
		public void TestHasChargesWithThisKeyWithApportionChargeKey()
		{
			JobComInvCharge oFTFull = testCollection.AddNew();
			oFTFull.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFTFull.J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.FullApportionment;
			oFTFull.J7_Amount = 100m;
			oFTFull.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			JobComInvCharge comWithPercentage = testCollection.AddNew();
			comWithPercentage.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;
			comWithPercentage.J7_Percentage = 10m;

			NUnit.Framework.Assert.That(testCollection.HasChargeWithThisKey(oFTFull.ApportionChargeKey), Is.EqualTo(true), "HasChargesWithThisKey");
			NUnit.Framework.Assert.That(testCollection.HasChargeWithThisKey(new ApportionChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, ApportionmentTypeList.Codes.PartialApportionment, GroupIsIncludedInLinesOptionList.Codes.No, GroupIsIncludedInLinesOptionList.Codes.No, ChargeDistributeByList.Codes.Value, 0m, false, "", false, false)), Is.EqualTo(false), "HasChargesWithThisKey with OFTPartial");
			NUnit.Framework.Assert.That(testCollection.HasChargeWithThisKey(comWithPercentage.ApportionChargeKey), Is.EqualTo(true), "HasChargeWithThisKey with Commission");
		}

		[ExpectNoExceptions]
		public void TestHasChargesIncludedInITOTWithChargeKey()
		{
			JobComInvCharge charge = testCollection.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			charge.J7_IsIncludedInITOT = true;

			ChargeCodeChargeKey oFTKey = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, false);
			NUnit.Framework.Assert.That(testCollection.HasChargesIncludedInITOT(oFTKey), Is.EqualTo(true), "Exists with OFT chargeKey");

			charge.J7_IsDutiable = true;
			NUnit.Framework.Assert.That(testCollection.HasChargesIncludedInITOT(oFTKey), Is.EqualTo(false), "Does not exist with OFT ChargeKey");
		}

		[ExpectNoExceptions]
		public void TestHasValidCharges()
		{
			JobComInvCharge charge = testCollection.AddNew();
			NUnit.Framework.Assert.That(testCollection.HasAnElementWithValidCharges(), Is.EqualTo(false), "Charge is not valid yet");

			charge.J7_Amount = 100m;
			NUnit.Framework.Assert.That(testCollection.HasAnElementWithValidCharges(), Is.EqualTo(false), "Charge is not valid yet");

			charge.J7_RX_NKCurrency = "AUD";
			NUnit.Framework.Assert.That(testCollection.HasAnElementWithValidCharges(), Is.EqualTo(true), "Charge is valid");
		}

		[ExpectNoExceptions]
		public void TestAmountDutiableAndGSTApplicable()
		{
			JobComInvCharge charge1 = testCollection.AddNew();
			charge1.J7_ChargeType = "OTH";
			charge1.J7_IsDutiable = true;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_Amount = 100m;
			charge1.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			JobComInvCharge charge2 = testCollection.AddNew();
			charge2.J7_ChargeType = "OTH";
			charge2.J7_IsDutiable = false;
			charge2.J7_IsGSTApplicable = true;
			charge2.J7_Amount = 50;
			charge2.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			JobComInvCharge charge3 = testCollection.AddNew();
			charge3.J7_ChargeType = "OTH";
			charge3.J7_IsDutiable = false;
			charge3.J7_IsGSTApplicable = false;
			charge3.J7_Amount = 25;
			charge3.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			NUnit.Framework.Assert.That(testCollection.AmountDutiable(invoice.LocalCurrency), Is.EqualTo(100m).Using(CustomComparers.TypeComparison), "Amount Dutiable");
			NUnit.Framework.Assert.That(testCollection.AmountGSTApplicable(invoice.LocalCurrency), Is.EqualTo(150m).Using(CustomComparers.TypeComparison), "Amount GST");
		}

		[ExpectNoExceptions]
		public void TestHasOnlyOneChargeCurrency()
		{
			var chargeAUD = testCollection.AddNew();
			chargeAUD.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			chargeAUD.J7_Amount = 100m;
			chargeAUD.J7_RX_NKCurrency = "AUD";

			var chargeAUD2 = testCollection.AddNew();
			chargeAUD2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			chargeAUD2.J7_Amount = 200m;
			chargeAUD2.J7_RX_NKCurrency = "AUD";

			NUnit.Framework.Assert.That(testCollection.HasOnlyOneChargeCurrency(), Is.EqualTo(true), "charges in same currency AUD");

			var chargeUSD = testCollection.AddNew();
			chargeUSD.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			chargeUSD.J7_Amount = 300m;
			chargeUSD.J7_RX_NKCurrency = "USD";

			NUnit.Framework.Assert.That(testCollection.HasOnlyOneChargeCurrency(), Is.EqualTo(false), "charges in mixed currency");
			chargeUSD.J7_RX_NKCurrency = "AUD";
			NUnit.Framework.Assert.That(testCollection.HasOnlyOneChargeCurrency(), Is.EqualTo(true), "charges in same currency AUD");
		}

		[ExpectNoExceptions]
		public void TestGetChargeFilteredByGSTApplicabilityAndITOTInclusion()
		{
			var charge1 = testCollection.AddNew();
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_IsIncludedInITOT = true;
			charge1.J7_Amount = 20m;
			charge1.J7_RX_NKCurrency = "AUD";

			var charge2 = testCollection.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge2.J7_IsGSTApplicable = true;
			charge2.J7_IsIncludedInITOT = false;
			charge2.J7_Amount = 30m;
			charge2.J7_RX_NKCurrency = "AUD";

			var charge3 = testCollection.AddNew();
			charge3.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge3.J7_IsGSTApplicable = false;
			charge3.J7_IsIncludedInITOT = true;
			charge3.J7_Amount = 70m;
			charge3.J7_RX_NKCurrency = "AUD";

			var charge4 = testCollection.AddNew();
			charge4.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge4.J7_IsGSTApplicable = false;
			charge4.J7_IsIncludedInITOT = false;
			charge4.J7_Amount = 130m;
			charge4.J7_RX_NKCurrency = "AUD";

			NUnit.Framework.Assert.That(testCollection.GetChargeFilteredByGSTApplicabilityAndITOTInclusion(true, true).Amount, Is.EqualTo(20m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testCollection.GetChargeFilteredByGSTApplicabilityAndITOTInclusion(true, false).Amount, Is.EqualTo(30m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testCollection.GetChargeFilteredByGSTApplicabilityAndITOTInclusion(false, true).Amount, Is.EqualTo(70m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testCollection.GetChargeFilteredByGSTApplicabilityAndITOTInclusion(false, false).Amount, Is.EqualTo(130m).Using(CustomComparers.TypeComparison));
		}

		[TestDate(2016, 01, 01)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryLoad", Justification = "Baseline")]
		[ExpectNoExceptions]
		public void TestGetTotal()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var usd = (RefCurrency)Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "USD");
				var aud = (RefCurrency)Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "AUD");
				var effectiveDate = invoice.DateOfValuation;
				ZQuery filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, usd.RX_Code);
				filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
				filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
				filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
				filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, "CUS");

				var exchangeRate = Factory.LoadTop1<RefExchangeRate>(filter);
				if (exchangeRate == null)
				{
					exchangeRate = Factory.New<RefExchangeRate>();
					exchangeRate.RE_RX_NKExCurrency = usd.RX_Code;
					exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
					exchangeRate.RE_StartDate = effectiveDate;
					exchangeRate.RE_ExpiryDate = effectiveDate.AddDays(1);
					exchangeRate.RE_ExRateType = "CUS";
				}

				exchangeRate.RE_SellRate = 0.5m;

				var charge = testCollection.AddNew("COM", 10m, "AUD");
				charge.J7_IsDutiable = false;
				charge.J7_IsIncludedInITOT = true;
				testCollection.AddNew("OFS", 100m, "AUD");
				testCollection.AddNew("ONS", 100m, "USD");
				var currencyConverter = invoice.CurrencyConverter;
				CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(testCollection.GetTotal(currencyConverter, aud, (x) => !x.J7_IsDutiable && x.J7_IsIncludedInITOT), Is.EqualTo(10m).Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(testCollection.GetTotal(currencyConverter, aud, (x) => !x.J7_IsDutiable && !x.J7_IsIncludedInITOT), Is.EqualTo(300m).Using(CustomComparers.TypeComparison));
					currencyConverter = null;
					NUnit.Framework.Assert.That(testCollection.GetTotal(currencyConverter, aud, (x) => !x.J7_IsDutiable && x.J7_IsIncludedInITOT), Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(testCollection.GetTotal(currencyConverter, aud, (x) => !x.J7_IsDutiable && !x.J7_IsIncludedInITOT), Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
				});
			}
		}

		[ExpectNoExceptions]
		public void TestMoneyToAddToITOTWithThisFlag()
		{
			var charge1 = testCollection.AddNew();
			charge1.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_IsIncludedInITOT = true;
			charge1.J7_Amount = 20m;
			charge1.J7_RX_NKCurrency = "AUD";
			charge1.J7_AdjustedCharge = false;

			var charge2 = testCollection.AddNew();
			charge2.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge2.J7_IsGSTApplicable = true;
			charge2.J7_IsIncludedInITOT = false;
			charge2.J7_Amount = 30m;
			charge2.J7_RX_NKCurrency = "AUD";
			charge2.J7_AdjustedCharge = false;

			var charge3 = testCollection.AddNew();
			charge3.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge3.J7_IsGSTApplicable = false;
			charge3.J7_IsIncludedInITOT = true;
			charge3.J7_Amount = 9m;
			charge3.J7_RX_NKCurrency = "AUD";

			var charge4 = testCollection.AddNew();
			charge4.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;
			charge4.J7_IsGSTApplicable = true;
			charge4.J7_IsIncludedInITOT = false;
			charge4.J7_Amount = 5m;
			charge4.J7_RX_NKCurrency = "AUD";
			charge4.J7_AdjustedCharge = true;

			NUnit.Framework.Assert.That(testCollection.MoneyToAddToITOTWithThisFlag(x => x.J7_IsGSTApplicable), Is.EquivalentTo(new[] { new Money(30, charge2.Currency), new Money(-9, charge3.Currency) }), "Applicable GST");
		}

		#region Implementation

		TestInvoice invoice;
		IJobComInvChargeCollection<JobComInvCharge> testCollection;

		protected override void SetUp()
		{
			base.SetUp();
			invoice = Factory.New<TestInvoice>();
			invoice.Z0_Guid = Factory.New<TestDeclaration>().PK;
			invoice.DateOfValuation = new ZDateTime(2004, 10, 30);
			testCollection = invoice.Charges;
		}

		#endregion
	}
}
