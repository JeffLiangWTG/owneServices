using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.ODPL.Test
{
	public class OdplDiscountCalculatorTest : TestCaseWithFactory
	{
		#region GetDiscounts

		public void TestGetDiscounts_DifferentSystemCodes()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var org = lic.Company.Header;
			var discounts = lic.Company.SelfBilling.BillingDiscounts;
			ClientLicenceBillingDiscount discount = discounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Discount = 10m;

			ClientLicenceBillingDiscount discount1 = discounts.AddNew();
			discount1.L5_SystemCode = "AAA";
			discount1.L5_Type = BillingConstants.DiscountType.Special;
			discount1.L5_Discount = 10m;

			ClientLicenceBillingDiscount discount2 = discounts.AddNew();
			discount2.L5_SystemCode = "BBB";
			discount2.L5_Type = BillingConstants.DiscountType.Special;
			discount2.L5_Discount = 20m;

			Discountable discountable1 = new Discountable("AAA", 100m, "");
			Discountable discountable2 = new Discountable("BBB", 100m, "");
			Discountable discountable3 = new Discountable("XXX", 100m, "");

			OdplSystemBill odplBill = new OdplSystemBill(Factory);
			OdplUsage usage = new OdplUsage(Factory, lic, new ZDateTime(2010, 10, 1), null);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { usage });
			var odplDiscounts = OdplDiscountCalculator.GetDiscounts(odplBill);
			AssertContainsExactElementsInAnyOrder("ODM discounts only", new ClientLicenceBillingDiscount[] { discount }, odplDiscounts);
		}

		public void TestGetDiscounts_Standard()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var stdPrices = stdCompany.PriceHeaders.AddNew();
			stdPrices.L6_RX_NKCurrency = "AUD";
			stdPrices.L6_PricelistVersion = "P1";
			stdPrices.L6_DiscountCode = "V2";
			BillingTestHelper.AddPriceItem(stdPrices, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);

			var stdDiscount = stdCompany.SelfBilling.BillingDiscounts.AddNew();
			stdDiscount.L5_DiscountCode = "V1";
			stdDiscount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			stdDiscount.L5_Type = BillingConstants.DiscountType.Special;
			stdDiscount.L5_Discount = 10;

			var stdDiscount2 = stdCompany.SelfBilling.BillingDiscounts.AddNew();
			stdDiscount2.L5_DiscountCode = "V2";
			stdDiscount2.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			stdDiscount2.L5_Type = BillingConstants.DiscountType.Special;
			stdDiscount2.L5_Discount = 50;

			stdCompany.Factory.Save();

			var org = BillingTestHelper.CreateOrganisation(Factory, "YYY");
			var user = new UsingParty(org);

			ClientLicenceBillingDiscount discount = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Discount = 5;

			ClientLicencePriceHeader prices = org.LicCompany.PriceHeaders.AddNew();
			prices.L6_UseStandardDiscount = false;
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			prices.L6_DiscountCode = "V1";
			prices.L6_PricelistVersion = "P1";
			prices.L6_IsStandard = true;
			prices.L6_TestDbPriceCode = ZString.Empty;

			Factory.Save();

			OdplUsage odplUsage = new OdplUsage(Factory, user, new ZDateTime(2010, 10, 1));
			OdplUsageTest.AddModuleUsage(odplUsage, BillingConstants.CoreModuleCode, 10, 10m, 10);
			OdplSystemBill odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage });

			var discounts = OdplDiscountCalculator.GetDiscounts(odplBill);
			AssertContainsExactElementsInAnyOrder("discount includes V1 std discount", new ClientLicenceBillingDiscount[] { discount, stdDiscount }, discounts);

			prices.L6_DiscountCode = "V2";
			odplUsage = new OdplUsage(Factory, user, new ZDateTime(2010, 10, 1));
			OdplUsageTest.AddModuleUsage(odplUsage, BillingConstants.CoreModuleCode, 10, 10m, 10);
			odplBill = new OdplSystemBill(Factory);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { odplUsage });
			discounts = OdplDiscountCalculator.GetDiscounts(odplBill);
			AssertContainsExactElementsInAnyOrder("discount includes V2 std discount", new ClientLicenceBillingDiscount[] { discount, stdDiscount2 }, discounts);
		}

		public void TestGetDiscounts_IsDateRangeMatched()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			var org = lic.Company.Header;
			var discounts = lic.Company.SelfBilling.BillingDiscounts;

			// Discount dates expired
			ZDateTime startDate = new ZDateTime(2010, 01, 01);
			ZDateTime endDate = new ZDateTime(2010, 06, 01);

			FieldInfo[] discountTypeFields = typeof(BillingConstants.DiscountType).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (FieldInfo discountTypeField in discountTypeFields)
			{
				string feeType = (string)discountTypeField.GetValue(null);

				ClientLicenceBillingDiscount discount = discounts.AddNew();
				discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
				discount.L5_Type = feeType;
				discount.L5_Discount = 20m;
				discount.L5_BreakAmount = 100m;
				discount.L5_ModuleCode = "COR";

				discount.L5_StartDate = startDate;
				discount.L5_EndDate = endDate;
			}

			Discountable discountable = CreateDiscountable(800m);
			discountable.ModuleCode = "COR";

			OdplSystemBill odplBill = new OdplSystemBill(Factory);
			OdplUsage usage = new OdplUsage(Factory, lic, new ZDateTime(2010, 10, 1), null);
			odplBill.PopulateFromSystemUsages(new SystemUsage[] { usage });
			var odplDiscounts = OdplDiscountCalculator.GetDiscounts(odplBill);
			AssertEquals("No discounts -- dates not matched", 0, odplDiscounts.Count);

			var discount2 = discounts.AddNew();
			discount2.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount2.L5_Type = BillingConstants.DiscountType.Special;
			discount2.L5_Discount = 20m;

			discount2.L5_StartDate = new ZDateTime(2010, 10, 1);
			odplDiscounts = OdplDiscountCalculator.GetDiscounts(odplBill);
			AssertContainsExactElementsInAnyOrder("date matched discounts only", new ClientLicenceBillingDiscount[] { discount2 }, odplDiscounts);
		}

		#endregion

		#region Calculate

		public void TestCalculate_ModuleSpecific()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			discount.L5_Discount = 20m;
			discount.L5_ModuleCode = "COR";

			var discounts = new List<ClientLicenceBillingDiscount>();
			discounts.Add(discount);
			Discountable discountable = CreateDiscountable(800m);
			discountable.ModuleCode = "COR";
			DiscountCalculation calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals("Discount applied", true, calculation.DiscountAmount > 0);
			AssertEquals(true, calculation.DiscountDescriptions.Any(x => x.Contains("Module")));
		}

		public void TestCalculate_ModuleSpecificWithCommitment()
		{
			var discounts = new List<ClientLicenceBillingDiscount>();

			// COR free, usage below commitment
			ClientLicenceBillingDiscount discount = AddNew(discounts);
			discount.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			discount.L5_Discount = 100m;
			discount.L5_ModuleCode = "COR";

			ClientLicenceBillingDiscount commitment = AddNew(discounts);
			commitment.L5_Type = BillingConstants.DiscountType.Commitment;
			commitment.L5_BreakAmount = 1000m;
			commitment.L5_Discount = 20m;

			Discountable discountable = CreateDiscountable(800m);
			discountable.ModuleCode = "COR";

			DiscountCalculation calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals("Amount was bumped", 1000m, calculation.Amount);
			AssertEquals("Discount amount is just from commitment", 200m, calculation.DiscountAmount);
			AssertEquals("Total amount", 800m, calculation.TotalAmount);
			AssertEquals("Commitment reduces Module Specific Discount to 0.00", calculation.DiscountDescriptions.ElementAt(1));

			// COR free, usage above commitment
			discountable.AmountToDiscount = 2000m;
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals("Amount was not adjusted since above threshold", 2000m, calculation.Amount);
			AssertEquals("Discount amount is part commitment and part module specific", 1200m, calculation.DiscountAmount);
			AssertEquals("Total amount", 800m, calculation.TotalAmount);
			AssertEquals("Commitment reduces Module Specific Discount to 1,000.00", calculation.DiscountDescriptions.ElementAt(1));

			// COR 50% off, usage below commitment
			discount.L5_Discount = 50m;
			discountable.AmountToDiscount = 800m;
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals("Amount was bumped", 1000m, calculation.Amount);
			AssertEquals("Discount amount is just from commitment", 200m, calculation.DiscountAmount);
			AssertEquals("Total amount", 800m, calculation.TotalAmount);
			AssertEquals("Commitment reduces Module Specific Discount to 0.00", calculation.DiscountDescriptions.ElementAt(1));

			// COR 50% off, raw usage above commitment, but usage after COR discount below commitment
			discountable.AmountToDiscount = 1500m;
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals("Amount was not adjusted since above threshold", 1500m, calculation.Amount);
			AssertEquals("Discount amount is part commitment and part module specific", 700m, calculation.DiscountAmount);
			AssertEquals("Total amount", 800m, calculation.TotalAmount);
			AssertEquals("Commitment reduces Module Specific Discount to 500.00", calculation.DiscountDescriptions.ElementAt(1));

			// COR 50% off, usage after COR discount above commitment
			discountable.AmountToDiscount = 3000m;
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals("Amount was not adjusted since above threshold", 3000m, calculation.Amount);
			AssertEquals("Commitment discount applies to the amount left after module discount - no double discounting", 1500m + 0.2m * 1500m, calculation.DiscountAmount);
			AssertEquals("Total amount", 1200m, calculation.TotalAmount);
			AssertNotContains("Commitment reduces Module Specific Discount", calculation.DiscountDescriptions.ElementAt(1));
		}

		public void TestCalculate_Prepayment()
		{
			AssertSimpleDiscountCalculation(BillingConstants.DiscountType.Prepayment);

			var discounts = new List<ClientLicenceBillingDiscount>();
			ClientLicenceBillingDiscount discount = AddNew(discounts);
			discount.L5_Type = BillingConstants.DiscountType.Prepayment;
			discount.L5_BreakAmount = 100m;
			discount.L5_Discount = 10m;

			discount = AddNew(discounts);
			discount.L5_Type = BillingConstants.DiscountType.Volume;
			discount.L5_BreakAmount = 10m;
			discount.L5_Discount = 20m;

			Discountable discountable = CreateDiscountable(160m);
			DiscountCalculation calculation = OdplDiscountCalculator.Calculate(discounts, discountable);

			AssertEquals("Amount", 160m, calculation.Amount);
			ZDecimal expectedPrepaymentDiscount = 60m * 10m / 100;
			ZDecimal expectedVolumeDiscount = 60m * 20m / 100;
			AssertEquals("Volume discount applied only to amount higher than prepayment break amount", expectedPrepaymentDiscount + expectedVolumeDiscount, calculation.DiscountAmount);

			discounts.Clear();
			discount = AddNew(discounts);
			discount.L5_Type = BillingConstants.DiscountType.Prepayment;
			discount.L5_BreakAmount = 1000m;
			discount.L5_Discount = 20m;

			discountable = CreateDiscountable(800m);
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);

			AssertEquals("Amount was raised", 1000m, calculation.Amount);
			AssertEquals("Discount amount is zero", 0m, calculation.DiscountAmount);
		}

		public void TestCalculate_Volume()
		{
			AssertSimpleDiscountCalculation(BillingConstants.DiscountType.Volume);
		}

		public void TestCalculate_Special()
		{
			AssertSimpleDiscountCalculation(BillingConstants.DiscountType.Special);
		}

		public void TestCalculate_Capped()
		{
			AssertSimpleDiscountCalculation(BillingConstants.DiscountType.Capped);
		}

		public void TestCalculate_SequenceOfCalculation()
		{
			var discounts = new List<ClientLicenceBillingDiscount>();
			ClientLicenceBillingDiscount discount1 = AddNew(discounts);
			discount1.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			discount1.L5_Discount = 20m;
			discount1.L5_ModuleCode = "COR";

			ClientLicenceBillingDiscount discount2 = AddNew(discounts);
			discount2.L5_Type = BillingConstants.DiscountType.Commitment;
			discount2.L5_Discount = 20m;

			ClientLicenceBillingDiscount discount3 = AddNew(discounts);
			discount3.L5_Type = BillingConstants.DiscountType.Volume;
			discount3.L5_Discount = 20m;
			discount3.L5_BreakAmount = 100m;

			ClientLicenceBillingDiscount discount4 = AddNew(discounts);
			discount4.L5_Type = BillingConstants.DiscountType.Special;
			discount4.L5_Discount = 20m;

			ClientLicenceBillingDiscount discount5 = AddNew(discounts);
			discount5.L5_Type = BillingConstants.DiscountType.Capped;
			discount5.L5_Discount = 20m;

			Discountable discountable = CreateDiscountable(800m);
			discountable.ModuleCode = "COR";

			DiscountCalculation calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals("Discount applied", true, calculation.DiscountAmount > 0);
			AssertEquals(5, calculation.DiscountDescriptions.Count());

			ZString[] discountDescriptions = calculation.DiscountDescriptions.ToArray();
			AssertEquals(true, discountDescriptions[0].Contains("Module"));
			AssertEquals(true, discountDescriptions[1].Contains("Commitment"));
			AssertEquals(true, discountDescriptions[2].Contains("Volume"));
			AssertEquals(true, discountDescriptions[3].Contains("Special"));
			AssertEquals(true, discountDescriptions[4].Contains("Capped"));
		}

		public void TestCalculate_Volume_LicenceUnits()
		{
			var discounts = new List<ClientLicenceBillingDiscount>();
			ClientLicenceBillingDiscount discount = AddNew(discounts);
			discount.L5_BreakUnits = BillingConstants.DiscountBreakUnit.LicenceUnits;
			discount.L5_Type = BillingConstants.DiscountType.Volume;
			discount.L5_BreakAmount = 1000m;
			discount.L5_Discount = 20m;
			Discountable discountable1 = CreateDiscountable(700m, 900m);
			DiscountCalculation calculation = OdplDiscountCalculator.Calculate(discounts, discountable1);
			AssertEquals("NO Discount applied", 0m, calculation.DiscountAmount);
			AssertEquals(0, calculation.DiscountDescriptions.Count());

			discount.L5_BreakAmount = 800m;
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable1);
			AssertEquals("Discount applied - Licence Usage > Break Amount", 140m, calculation.DiscountAmount);
			AssertEquals(1, calculation.DiscountDescriptions.Count());
			ZString expectedDescription = discount.Lookups.DiscountTypes.GetDescriptionFromCode(BillingConstants.DiscountType.Volume);
			AssertEquals("Volume discount exist", true, calculation.DiscountDescriptions.Any(x => x.Contains(expectedDescription)));

			discount.L5_BreakAmount = 800m;
			Discountable discountable2 = CreateDiscountable(950m, 750m);
			DiscountCalculation calculation2 = OdplDiscountCalculator.Calculate(discounts, discountable2);
			AssertEquals("NO Discount applied - Amount Usage > Break Amount", 0m, calculation2.DiscountAmount);
			AssertEquals(0, calculation2.DiscountDescriptions.Count());
		}

		public void TestCalculate_Commitment()
		{
			AssertSimpleDiscountCalculation(BillingConstants.DiscountType.Commitment);

			var discounts = new List<ClientLicenceBillingDiscount>();
			ClientLicenceBillingDiscount discount = AddNew(discounts);
			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			discount.L5_BreakAmount = 1000m;
			discount.L5_Discount = 20m;

			Discountable discountable = CreateDiscountable(800m);
			DiscountCalculation calculation = OdplDiscountCalculator.Calculate(discounts, discountable);

			AssertEquals("Amount was raised", 1000m, calculation.Amount);
			AssertEquals("Discount applied", 1000m * 0.20m, calculation.DiscountAmount);

			// Commitment in LicenceUnits
			discount.L5_BreakUnits = BillingConstants.DiscountBreakUnit.LicenceUnits;
			discount.L5_BreakAmount = 100m;
			discountable.AmountToDiscount = 10000m;
			discountable.LicenceUnitsToDiscount = 50m;
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable, 5.1234m);
			AssertEquals("Discount applied", Utilities.Round(100m * 5.1234m * 0.2m, BillingConstants.RoundingDecimals), calculation.DiscountAmount);
		}

		public void TestDiscountOnDemandMinimumFee()
		{
			var discounts = new List<ClientLicenceBillingDiscount>();
			ClientLicenceBillingDiscount discount = AddNew(discounts);
			discount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			discount.L5_Units = 3;
			discount.L5_BreakAmount = 360;

			var discountable = CreateDiscountable(9999m);
			discountable.CoreOnDemandUsers = 10;

			var calculation = OdplDiscountCalculator.Calculate(discounts, discountable);

			AssertEquals("discount", 0m, calculation.DiscountAmount);
			AssertEquals("amount", 10m * 120, calculation.Amount);

			ClientLicenceBillingDiscount volumeDiscount = AddNew(discounts);
			volumeDiscount.L5_Type = BillingConstants.DiscountType.Volume;
			volumeDiscount.L5_BreakAmount = 10m;
			volumeDiscount.L5_Discount = 20m;

			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals("volume discount is present", 10m * 120 * 0.2m, calculation.DiscountAmount);
			AssertEquals("amount", 10m * 120, calculation.Amount);

			volumeDiscount.L5_BreakAmount = 100000;
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals("volume discount not present since break amount not reached", 0m, calculation.DiscountAmount);
			AssertEquals("amount", 10m * 120, calculation.Amount);
		}

		public void TestDiscountsVolumeWithPurchasedSeats()
		{
			var discounts = new List<ClientLicenceBillingDiscount>();
			ClientLicenceBillingDiscount discount = AddNew(discounts);
			discount.L5_Type = BillingConstants.DiscountType.Volume;
			discount.L5_Discount = 25;
			discount.L5_BreakAmount = 559;

			var discountable = CreateDiscountable(56m);
			discountable.MixedAmountAsMoney = 560;
			var calculation = OdplDiscountCalculator.Calculate(discounts, discountable);

			AssertEquals("volume discount applies since total usage is above break even if on demand usage is below", 56m * 0.25m, calculation.DiscountAmount);
			AssertEquals("amount", 56m, calculation.Amount);

			discount.L5_BreakAmount = 561m;
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals("volume discount not present since break amount not reached", 0m, calculation.DiscountAmount);
			AssertEquals("amount", 56m, calculation.Amount);
		}

		public void TestCalculate_WiseCloud()
		{
			var discounts = new List<ClientLicenceBillingDiscount>();
			var discountOnPriceList = AddNew(discounts);
			discountOnPriceList.L5_Type = BillingConstants.DiscountType.WiseCloud;
			discountOnPriceList.L5_Discount = 15;
			discountOnPriceList.L5_DiscountCode = "price list 1";

			var discountable = CreateDiscountable(56m);
			discountable.IsHostedOnWiseCloud = false;
			discountable.MixedAmountAsMoney = 560;
			var calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals(0m, calculation.DiscountAmount);
			AssertEquals(0, calculation.DiscountDescriptions.Count());

			discountable.IsHostedOnWiseCloud = true;
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals(8.4m, calculation.DiscountAmount);
			AssertEquals(1, calculation.DiscountDescriptions.Count());
			AssertEquals("WiseCloud Discount: -8.40 (-15% * 56.00)", calculation.DiscountDescriptions.First());

			var discountOnOrg = AddNew(discounts);
			discountOnOrg.L5_Type = BillingConstants.DiscountType.WiseCloud;
			discountOnOrg.L5_Discount = 20;
			discountOnOrg.L5_DiscountCode = "";
			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);
			AssertEquals(11.2m, calculation.DiscountAmount);
			AssertEquals(1, calculation.DiscountDescriptions.Count());
			AssertEquals("WiseCloud Discount: -11.20 (-20% * 56.00)", calculation.DiscountDescriptions.First());
		}

		public void TestVolumeWithCommitment()
		{
			var discounts = new List<ClientLicenceBillingDiscount>();
			var discount1 = AddNew(discounts);
			discount1.L5_Type = BillingConstants.DiscountType.Volume;
			discount1.L5_Discount = 25;
			discount1.L5_BreakAmount = 559;

			var discountable = CreateDiscountable(56m);
			discountable.MixedAmountAsMoney = 560;
			var calculation = OdplDiscountCalculator.Calculate(discounts, discountable);

			AssertEquals(false, calculation.ErrorDescriptions.Any());

			var discount2 = AddNew(discounts);
			discount2.L5_Type = BillingConstants.DiscountType.Commitment;
			discount2.L5_Discount = 25;
			discount2.L5_BreakAmount = 1;

			calculation = OdplDiscountCalculator.Calculate(discounts, discountable);

			AssertEquals(true, calculation.ErrorDescriptions.Any());
			AssertEquals("Volume discount is not allowed with a Commitment discount", calculation.ErrorDescriptions.Single());
		}

		void AssertSimpleDiscountCalculation(ZString discountType)
		{
			var discounts = new List<ClientLicenceBillingDiscount>();

			ClientLicenceBillingDiscount discount = AddNew(discounts);
			discount.L5_Type = discountType;
			discount.L5_BreakAmount = 500m;
			discount.L5_Discount = 20m;

			Discountable discountable = CreateDiscountable(800m);
			DiscountCalculation calculation = OdplDiscountCalculator.Calculate(discounts, discountable);

			AssertEquals("Discount applied", true, calculation.DiscountAmount > 0);
			AssertEquals(1, calculation.DiscountDescriptions.Count());
			ZString expectedDescription = discount.Lookups.DiscountTypes.GetDescriptionFromCode(discountType);
			AssertEquals(true, calculation.DiscountDescriptions.Any(x => x.Contains(expectedDescription)));
		}

		#endregion

		ClientLicenceBillingDiscount AddNew(List<ClientLicenceBillingDiscount> discounts)
		{
			var discount = Factory.New<ClientLicenceBillingDiscount>();
			discounts.Add(discount);
			return discount;
		}

		Discountable CreateDiscountable(ZDecimal amountAsMoney, decimal amountAsLicenceUnits = 0m)
		{
			return new Discountable("", amountAsMoney, "", amountAsLicenceUnits)
			{
				MixedAmountAsMoney = amountAsMoney,
				MixedAmountAsLicenceUnits = amountAsLicenceUnits
			};
		}

		class Discountable : IOdplDiscountable
		{
			public Discountable(ZString systemCode, ZDecimal amount, string moduleCode, decimal quantityForVolumeDiscount = 0m)
			{
				SystemCode = systemCode;
				AmountToDiscount = amount;
				ModuleCode = moduleCode;
				UnitCount = 0;
				LicenceUnitsToDiscount = quantityForVolumeDiscount;
			}

			public ZString SystemCode { get; set; }

			public ZDecimal AmountToDiscount { get; set; }

			public ZDecimal AmountToDiscountForModule(string moduleCode, out string moduleName)
			{
				moduleName = "";
				if (moduleCode == ModuleCode)
				{
					return AmountToDiscount;
				}

				return 0m;
			}

			public int UnitCount { get; set; }
			public string ModuleCode { get; set; }
			public ZDecimal LicenceUnitsToDiscount { get; set; }
			public int CoreOnDemandUsers { get; set; }

			public ZDecimal MixedAmountAsMoney { get; set; }
			public ZDecimal MixedAmountAsLicenceUnits { get; set; }

			public ZBool IsHostedOnWiseCloud { get; set; }
		}
	}
}