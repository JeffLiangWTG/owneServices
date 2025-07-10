using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	class BaseJobComInvHeaderChargeCollectionTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestHasChargeOfThisTypeAndAmountAndCurrency()
		{
			var oFTKey = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true, false);
			var oNSKey = new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasInsurance, false, true, false);

			var charge = testCollection.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			NUnit.Framework.Assert.That(testCollection.HasChargeOfThisTypeAndAmountAndCurrency(oFTKey), Is.EqualTo(false), "HasChargeOfThisTypeAndAmountAndCurrency");

			charge.J7_Amount = 100m;
			NUnit.Framework.Assert.That(testCollection.HasChargeOfThisTypeAndAmountAndCurrency(oFTKey), Is.EqualTo(false), "HasChargeOfThisTypeAndAmountAndCurrency");

			charge.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			NUnit.Framework.Assert.That(testCollection.HasChargeOfThisTypeAndAmountAndCurrency(oFTKey), Is.EqualTo(true), "HasChargeOfThisTypeAndAmountAndCurrency");

			NUnit.Framework.Assert.That(testCollection.HasChargeOfThisTypeAndAmountAndCurrency(oFTKey, oNSKey), Is.EqualTo(true), "HasChargeOfThisTypeAndAmountAndCurrency");
			NUnit.Framework.Assert.That(testCollection.HasChargeOfThisTypeAndAmountAndCurrency(oNSKey), Is.EqualTo(false), "HasChargeOfThisTypeAndAmountAndCurrency");
		}

		[ExpectNoExceptions]
		public void TestHasChargesDistributedByOtherThanValue()
		{
			var charge = testCollection.AddNew();
			charge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			NUnit.Framework.Assert.That(testCollection.HasChargesDistributedByOtherThanValue(charge.ChargeKey), Is.EqualTo(false), "HasChargeDistributedByOtherThanValue");

			charge.J7_DistributeBy = ChargeDistributeByList.Codes.Weight;
			NUnit.Framework.Assert.That(testCollection.HasChargesDistributedByOtherThanValue(charge.ChargeKey), Is.EqualTo(false), "HasChargeDistributedByOtherThanValue");

			charge.J7_Amount = 1000m;
			charge.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			NUnit.Framework.Assert.That(testCollection.HasChargesDistributedByOtherThanValue(charge.ChargeKey), Is.EqualTo(true), "HasChargeDistributedByOtherThanValue");
		}

		[ExpectNoExceptions]
		public void TestAllChargeKeys()
		{
			testCollection.RemoveAndDeleteAll();
			NUnit.Framework.Assert.That(testCollection.AllChargeKeys.Length, Is.EqualTo(0), "No charges");

			var oFT = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			NUnit.Framework.Assert.That(testCollection.AllChargeKeys.Length, Is.EqualTo(0), "No charges");

			oFT.J7_Amount = 50m;
			oFT.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			NUnit.Framework.Assert.That(testCollection.AllChargeKeys.Length, Is.EqualTo(1), "One charge Key");
			NUnit.Framework.Assert.That(testCollection.AllChargeKeys[0].ToString(), Is.EqualTo(oFT.ApportionChargeKey.ToString()), "One charge Key");

			testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
			oFT.J7_Amount = 150m;
			oFT.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			NUnit.Framework.Assert.That(testCollection.AllChargeKeys.Length, Is.EqualTo(1), "One charge Key");
			NUnit.Framework.Assert.That(testCollection.AllChargeKeys[0].ToString(), Is.EqualTo(oFT.ApportionChargeKey.ToString()), "One charge Key");

			var oNS = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			oNS.J7_Amount = 250m;
			oNS.J7_RX_NKCurrency = invoice.LocalCurrencyCode;
			NUnit.Framework.Assert.That(testCollection.AllChargeKeys.Length, Is.EqualTo(2), "Two charge Keys");

			bool hasSeenOFTKey = false, hasSeenONSKey = false;
			foreach (var chargeKey in testCollection.AllChargeKeys)
			{
				if (chargeKey.ToString() == oFT.ApportionChargeKey.ToString())
				{
					hasSeenOFTKey = true;
				}
				else if (chargeKey.ToString() == oNS.ApportionChargeKey.ToString())
				{
					hasSeenONSKey = true;
				}
			}
			NUnit.Framework.Assert.That(hasSeenOFTKey, Is.True);
			NUnit.Framework.Assert.That(hasSeenONSKey, Is.True);
		}

		[ExpectNoExceptions]
		public void TestSetParent()
		{
			var invoiceCharge = testCollection.AddNew();
			NUnit.Framework.Assert.That(invoiceCharge.J7_ParentID, Is.EqualTo(invoice.PK), "Invoice Charge has foreign Key set");
			NUnit.Framework.Assert.That(invoiceCharge.J7_ParentTableCode, Is.EqualTo("Z0").Using(CustomComparers.TypeComparison), "Invoice Charge has Table Code set");
			NUnit.Framework.Assert.That(invoiceCharge.Parent, Is.EqualTo(invoice).Using(CustomComparers.TypeComparison), "Parent object");
		}

		[ExpectNoExceptions]
		public void TestSetDefaultsForNewChild()
		{
			var invoiceCharge = testCollection.AddNew();
			NUnit.Framework.Assert.That(invoiceCharge.Parent, Is.EqualTo(invoice).Using(CustomComparers.TypeComparison), "Parent has set");
		}

		[ExpectNoExceptions]
		public void TestHasValidCharges()
		{
			var charge = testCollection.AddNew();
			NUnit.Framework.Assert.That(testCollection.HasAnElementWithValidCharges(), Is.EqualTo(false), "Charge is not valid yet");

			charge.J7_Amount = 100m;
			NUnit.Framework.Assert.That(testCollection.HasAnElementWithValidCharges(), Is.EqualTo(false), "Charge is not valid yet");

			charge.J7_RX_NKCurrency = "AUD";
			NUnit.Framework.Assert.That(testCollection.HasAnElementWithValidCharges(), Is.EqualTo(true), "Charge is valid");
		}

		[ExpectNoExceptions]
		public void TestFindWithChargeCodeChargeKey()
		{
			var oFTInAUD = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "USD");
			var oNS = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, "AUD");

			var result = testCollection.Find(oFTInAUD.ChargeKey);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(2), "There should be 2 items");

			result = testCollection.Find(oNS.ChargeKey);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(1), "There should be 1 items");

			result = testCollection.Find(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.PackingCost, true, true, false));
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(0), "There should be no items");
		}

		[ExpectNoExceptions]
		public void TestAmountToAddToITOTForFOB()
		{
			var includedApplicableCharge = testCollection.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, "AUD");
			includedApplicableCharge.J7_IsIncludedInITOT = true;
			includedApplicableCharge.J7_IsDutiable = true;

			var includedNotApplicableCharge = testCollection.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200m, "AUD");
			includedNotApplicableCharge.J7_IsIncludedInITOT = true;
			includedNotApplicableCharge.J7_IsDutiable = false;

			var notIncludedApplicableCharge = testCollection.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300m, "AUD");
			notIncludedApplicableCharge.J7_IsDutiable = true;

			var discountNotApplicable = testCollection.AddNew(CustomsChargeTypeList.Codes.Discount, 400m, "AUD");
			discountNotApplicable.J7_IsDutiable = false;

			var expected = -200 + 300 - 400m;
			NUnit.Framework.Assert.That(testCollection.AmountToAddToITOTForDutiableCharges(audCurrency), Is.EqualTo(expected).Using(CustomComparers.TypeComparison), "Amount to add to ITOT for FOB");
		}

		[ExpectNoExceptions]
		public void TestAmountToAddToITOTForCIF()
		{
			var includedApplicableCharge = testCollection.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, "AUD");
			includedApplicableCharge.J7_IsIncludedInITOT = true;
			includedApplicableCharge.J7_IsGSTApplicable = true;

			var includedNotApplicableCharge = testCollection.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200m, "AUD");
			includedNotApplicableCharge.J7_IsIncludedInITOT = true;
			includedNotApplicableCharge.J7_IsGSTApplicable = false;

			var notIncludedApplicableCharge = testCollection.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 300m, "AUD");
			notIncludedApplicableCharge.J7_IsGSTApplicable = true;

			var discountNotApplicable = testCollection.AddNew(CustomsChargeTypeList.Codes.Discount, 400m, "AUD");
			discountNotApplicable.J7_IsGSTApplicable = false;

			var expected = -200 + 300 - 400m;
			NUnit.Framework.Assert.That(testCollection.AmountToAddToITOTForVatableGstableCharges(audCurrency), Is.EqualTo(expected).Using(CustomComparers.TypeComparison), "Amount to add to ITOT for CIF");
		}

		[ExpectNoExceptions]
		public void TestGetChargeWithChargeCodeString()
		{
			testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "AUD");
			testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "USD");
			testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 100m, "AUD");

			var result = testCollection.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(2), "Two OFTs");

			result = testCollection.GetCharge(CustomsChargeTypeList.Codes.OverseasInsurance);
			NUnit.Framework.Assert.That(result.Length, Is.EqualTo(1), "One ONS");
		}

		[ExpectNoExceptions]
		public void TestHasChargeWithKey()
		{
			var oFT = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 0m, "KRW");
			var oNS = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 200m);

			NUnit.Framework.Assert.That(testCollection.HasChargeWithThisKey(oFT.ChargeKey), Is.EqualTo(true), "OFT has a currency");
			NUnit.Framework.Assert.That(testCollection.HasChargeWithThisKey(oNS.ChargeKey), Is.EqualTo(false), "ONS does not have a currency");
		}

		[ExpectNoExceptions]
		public void TestHasChargesIncludedInITOT()
		{
			var oFT = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 0m, "KRW");
			oFT.J7_IsIncludedInITOT = true;
			var anotherOFT = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "KRW");
			anotherOFT.J7_IsIncludedInITOT = false;
			var oNS = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 200m, "AUD");
			oNS.J7_IsIncludedInITOT = false;

			NUnit.Framework.Assert.That(testCollection.HasChargesIncludedInITOT(CustomsChargeTypeList.Codes.OverseasFreight), Is.EqualTo(true), "Included-in-line OFT exist");
			NUnit.Framework.Assert.That(testCollection.HasChargesIncludedInITOT(CustomsChargeTypeList.Codes.OverseasInsurance), Is.EqualTo(false), "Included-in-line ONS does not exist");
		}

		[ExpectNoExceptions]
		public void TestGetChargeInDesignatedCurrency()
		{
			testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, invoice.LocalCurrencyCode);
			testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 150m, "USD");

			var result = testCollection.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight, invoice.LocalCurrency);
			NUnit.Framework.Assert.That(result, Is.EqualTo(400m).Using(CustomComparers.TypeComparison), "OFT in local currency");
		}

		[ExpectNoExceptions]
		public void TestHasChargesExcludedInITOT()
		{
			var oFT = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 0m, "KRW");
			oFT.J7_IsIncludedInITOT = true;
			var anotherOFT = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "KRW");
			anotherOFT.J7_IsIncludedInITOT = false;
			var oNS = testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 200m, "AUD");
			oNS.J7_IsIncludedInITOT = true;

			NUnit.Framework.Assert.That(testCollection.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.OverseasFreight), Is.EqualTo(true), "Excluded-from-line OFT exist");
			NUnit.Framework.Assert.That(testCollection.HasChargesExcludedInITOT(CustomsChargeTypeList.Codes.OverseasInsurance), Is.EqualTo(false), "Excluded-From-line ONS does not exist");
		}

		[ExpectNoExceptions]
		public void TestRemoveAll()
		{
			testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 0m, "KRW");
			testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, "USD");
			testCollection.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 200m, "AUD");

			testCollection.RemoveAll(CustomsChargeTypeList.Codes.OverseasFreight);
			NUnit.Framework.Assert.That(testCollection.Count, Is.EqualTo(1), "There should be only one item");
		}

		[ExpectNoExceptions]
		public void TestHasNonDutiableGSTApplicableCharges()
		{
			NUnit.Framework.Assert.That(testCollection.HasNonDutiableGSTApplicableCharges(), Is.EqualTo(false));

			var charge = testCollection.AddNew("OFT", 0m);
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			NUnit.Framework.Assert.That(testCollection.HasNonDutiableGSTApplicableCharges(), Is.EqualTo(false));

			charge.J7_RX_NKCurrency = "AUD";
			NUnit.Framework.Assert.That(testCollection.HasNonDutiableGSTApplicableCharges(), Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestGetCommonApportionedChargesCurrencyOrInvoiceHeaderCurrency()
		{
			var chargeAUD = testCollection.AddNew();
			chargeAUD.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			chargeAUD.J7_Amount = 100m;
			chargeAUD.J7_RX_NKCurrency = "AUD";

			var chargeAUD2 = testCollection.AddNew();
			chargeAUD2.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			chargeAUD2.J7_Amount = 200m;
			chargeAUD2.J7_RX_NKCurrency = "AUD";

			NUnit.Framework.Assert.That(testCollection.GetCommonApportionedChargesCurrencyOrInvoiceHeaderCurrency(krwCurrency).Code, Is.EqualTo(audCurrency.Code), "charges in same currency AUD");

			var chargeUSD = testCollection.AddNew();
			chargeUSD.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			chargeUSD.J7_Amount = 300m;
			chargeUSD.J7_RX_NKCurrency = "USD";

			NUnit.Framework.Assert.That(testCollection.GetCommonApportionedChargesCurrencyOrInvoiceHeaderCurrency(krwCurrency).Code, Is.EqualTo(krwCurrency.Code), "mixed currency return default KRW");

			chargeUSD.J7_RX_NKCurrency = "AUD";

			NUnit.Framework.Assert.That(testCollection.GetCommonApportionedChargesCurrencyOrInvoiceHeaderCurrency(krwCurrency).Code, Is.EqualTo(audCurrency.Code), "charges in same currency AUD");
		}

		[ExpectNoExceptions]
		public void TestGetChargeFilteredByGSTApplicabilityAndITOTInclusion()
		{
			var charge1 = testCollection.AddNew();
			charge1.J7_ChargeType = "OTH";
			charge1.J7_IsDutiable = true;
			charge1.J7_IsGSTApplicable = true;
			charge1.J7_IsIncludedInITOT = true;
			charge1.J7_Amount = 100m;
			charge1.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			var charge2 = testCollection.AddNew();
			charge2.J7_ChargeType = "OTH";
			charge2.J7_IsDutiable = false;
			charge2.J7_IsGSTApplicable = true;
			charge2.J7_IsIncludedInITOT = false;
			charge2.J7_Amount = 50;
			charge2.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			var charge3 = testCollection.AddNew();
			charge3.J7_ChargeType = "OTH";
			charge3.J7_IsDutiable = false;
			charge3.J7_IsGSTApplicable = false;
			charge3.J7_IsIncludedInITOT = false;
			charge3.J7_Amount = 25;
			charge3.J7_RX_NKCurrency = invoice.LocalCurrencyCode;

			NUnit.Framework.Assert.That(testCollection.GetChargeFilteredByGSTApplicabilityAndITOTInclusion(true, true).Amount, Is.EqualTo(100m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testCollection.GetChargeFilteredByGSTApplicabilityAndITOTInclusion(true, false).Amount, Is.EqualTo(50m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testCollection.GetChargeFilteredByGSTApplicabilityAndITOTInclusion(false, false).Amount, Is.EqualTo(25m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(testCollection.GetChargeFilteredByGSTApplicabilityAndITOTInclusion(false, true).Amount, Is.EqualTo(0m).Using(CustomComparers.TypeComparison));
		}

		#region Implementation

		TestInvoice invoice;
		IJobComInvChargeCollection<TestCharge> testCollection;

		RefCurrency audCurrency;
		RefCurrency usdCurrency;
		RefCurrency krwCurrency;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryLoad", Justification = "Baseline")]
		protected override void SetUp()
		{
			base.SetUp();
			invoice = Factory.New<TestInvoice>();
			invoice.Z0_Guid = Factory.New<TestDeclaration>().PK;
			invoice.DateOfValuation = ZDateTime.Today;

			testCollection = invoice.Charges;
			audCurrency = (RefCurrency)Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "AUD");
			usdCurrency = (RefCurrency)Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "USD");
			SetExchangeRate(invoice.DateOfValuation.AddDays(-1), invoice.DateOfValuation.AddDays(1), 0.5m, usdCurrency, "CUS");
			krwCurrency = (RefCurrency)Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, "KRW");
			SetExchangeRate(invoice.DateOfValuation.AddDays(-1), invoice.DateOfValuation.AddDays(1), 1.3m, krwCurrency, "CUS");
		}

		public void SetExchangeRate(ZDateTime startDate, ZDateTime endDate, ZDecimal exchangeRate, RefCurrency foreignCurrency, string exRateType)
		{
			var sqlFilter = new ZQuery();
			sqlFilter.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, foreignCurrency.RX_Code);
			sqlFilter.AddToFilter(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK);
			sqlFilter.AddToFilter(RefExchangeRateSchema.RE_ExRateType, exRateType);
			sqlFilter.AddToFilter(RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThan, startDate.AddDays(1));
			sqlFilter.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, endDate);

			var exchangeRateDuty = Factory.LoadTop1<RefExchangeRate>(sqlFilter);
			if (exchangeRateDuty == null)
			{
				exchangeRateDuty = Factory.New<RefExchangeRate>();
				exchangeRateDuty.RE_ExpiryDate = endDate;
				exchangeRateDuty.RE_ExRateType = exRateType;
				exchangeRateDuty.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRateDuty.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
				exchangeRateDuty.RE_StartDate = startDate;
				exchangeRateDuty.RE_SellRate = exchangeRate;
			}
			else
			{
				exchangeRateDuty.RE_SellRate = exchangeRate;
			}
		}
		#endregion
	}
}

