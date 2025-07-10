using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicenceBillingDiscountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckL5_SystemCode()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var discount = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.Validation.ValidateAll();
			AssertHasErrors(discount.L5_SystemCodeInfo);

			discount.L5_SystemCode = "XXX";
			AssertHasErrors(discount.L5_SystemCodeInfo);

			discount.L5_SystemCode = BillingConstants.BillingSystem.eBACCA;
			AssertNoErrors(discount.L5_SystemCodeInfo);

			discount.L5_Type = BillingConstants.DiscountType.Capped;
			discount.Validation.ValidateAll();
			AssertHasError(discount.L5_SystemCodeInfo, "Only ODM system supports capped discount.");

			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			AssertNoErrors(discount.L5_SystemCodeInfo);

			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			discount.L5_SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			AssertHasErrors(discount.L5_SystemCodeInfo);

			discount.L5_SystemCode = BillingConstants.BillingSystem.NZCustoms;
			AssertHasErrors(discount.L5_SystemCodeInfo);

			discount.L5_SystemCode = BillingConstants.BillingSystem.PortMessaging;
			AssertHasErrors(discount.L5_SystemCodeInfo);

			discount.L5_SystemCode = BillingConstants.BillingSystem.ImporterSecurityFiling;
			AssertNoErrors(discount.L5_SystemCodeInfo);

			discount.L5_Type = BillingConstants.DiscountType.Volume;
			discount.L5_SystemCode = "ZZZ";
			AssertHasErrors(discount.L5_SystemCodeInfo);
			Factory.Save();

			discount.Validation.ValidateL5_SystemCode();
			AssertNoErrors(discount.L5_SystemCodeInfo);
		}

		public void TestCheckL5_Type()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.Validation.ValidateAll();
			AssertHasErrors(discount.L5_TypeInfo);

			discount.L5_Type = "XXX";
			AssertHasErrors(discount.L5_TypeInfo);

			discount.L5_Type = BillingConstants.DiscountType.Volume;
			AssertNoErrors(discount.L5_TypeInfo);
		}

		public void TestCheckL5_Type_WiseCloud()
		{
			var collection = new ClientLicenceBillingDiscountCollection(Factory.New<ClientLicenceBilling>());
			var discount = collection.AddNew();

			discount.L5_SystemCode = "123";
			discount.L5_Type = "WIS";
			AssertHasError(discount.L5_TypeInfo, "WiseCloud discounts are applicable to ODM only.");

			discount.L5_SystemCode = "ODM";
			discount.L5_Type = "123";
			discount.L5_Type = "WIS";
			AssertNoErrors(discount.L5_TypeInfo);
		}

		public void TestHaveOverlappingDates()
		{
			ClientLicenceBillingDiscount discount1 = Factory.New<ClientLicenceBillingDiscount>();
			ClientLicenceBillingDiscount discount2 = Factory.New<ClientLicenceBillingDiscount>();
			AssertEquals(true, discount1.Validation.HaveOverlappingDates(discount1, discount2));

			discount1.L5_StartDate = new ZDateTime(2010, 01, 01);
			AssertEquals(true, discount1.Validation.HaveOverlappingDates(discount1, discount2));

			discount1.L5_EndDate = new ZDateTime(2010, 12, 31);
			AssertEquals(true, discount1.Validation.HaveOverlappingDates(discount1, discount2));

			discount2.L5_StartDate = new ZDateTime(2010, 08, 01);
			AssertEquals(true, discount1.Validation.HaveOverlappingDates(discount1, discount2));

			discount2.L5_StartDate = new ZDateTime(2011, 01, 01);
			AssertEquals(false, discount1.Validation.HaveOverlappingDates(discount1, discount2));

			discount1.L5_EndDate = ZDateTime.Empty;
			AssertEquals(true, discount1.Validation.HaveOverlappingDates(discount1, discount2));

			discount1 = Factory.New<ClientLicenceBillingDiscount>();
			discount2 = Factory.New<ClientLicenceBillingDiscount>();

			discount1.L5_StartDate = new ZDateTime(2011, 01, 01);
			discount1.L5_EndDate = new ZDateTime(2012, 12, 31);

			discount2.L5_StartDate = new ZDateTime(2010, 01, 01);
			discount2.L5_EndDate = new ZDateTime(2010, 12, 31);
			AssertEquals(false, discount1.Validation.HaveOverlappingDates(discount1, discount2));

			discount1.L5_EndDate = ZDateTime.Empty;
			AssertEquals(false, discount1.Validation.HaveOverlappingDates(discount1, discount2));

			discount2.L5_EndDate = ZDateTime.Empty;
			AssertEquals(true, discount1.Validation.HaveOverlappingDates(discount1, discount2));
		}

		public void TestCheckL5_Type_CommitmentPrepayment()
		{
			ClientLicenceBillingDiscountCollection collection = new ClientLicenceBillingDiscountCollection(Factory.New<ClientLicenceBilling>());
			ClientLicenceBillingDiscount discountWithAnotherCode = collection.AddNew();
			discountWithAnotherCode.L5_SystemCode = "AAA";
			discountWithAnotherCode.L5_DiscountCode = "111";
			discountWithAnotherCode.L5_Type = BillingConstants.DiscountType.Commitment;

			ClientLicenceBillingDiscount discount1 = collection.AddNew();
			discount1.L5_Type = BillingConstants.DiscountType.Commitment;
			AssertNoErrors(discount1.L5_TypeInfo);

			ClientLicenceBillingDiscount discount2 = collection.AddNew();
			discount2.L5_SystemCode = "AAA";
			discount2.L5_Type = BillingConstants.DiscountType.Commitment;
			AssertNoErrors(discount2.L5_TypeInfo);

			discount2.L5_SystemCode = discount1.L5_SystemCode;
			discount2.Validation.ValidateAll();
			AssertHasError(discount2.L5_TypeInfo, "You can't have commitment discounts with overlapping dates.");

			discount1.L5_StartDate = new ZDateTime(2010, 01, 01);
			discount1.L5_EndDate = new ZDateTime(2010, 12, 31);
			discount2.Validation.ValidateAll();
			AssertHasError("Second discount has open start date", discount2.L5_TypeInfo, "You can't have commitment discounts with overlapping dates.");

			discount2.L5_StartDate = new ZDateTime(2010, 08, 01);
			discount2.Validation.ValidateAll();
			AssertHasError(discount2.L5_TypeInfo, "You can't have commitment discounts with overlapping dates.");

			discount2.L5_StartDate = new ZDateTime(2011, 01, 01);
			discount2.Validation.ValidateAll();
			AssertNoErrors(discount2.L5_TypeInfo);

			discount2.L5_Type = BillingConstants.DiscountType.Prepayment;
			AssertNoErrors(discount2.L5_TypeInfo);

			discount2.L5_StartDate = new ZDateTime(2010, 06, 01);
			discount2.Validation.ValidateAll();
			AssertHasError(discount2.L5_TypeInfo, "You can't have commitment and prepayment discounts at the same time.");

			discount2.L5_Type = BillingConstants.DiscountType.Volume;
			AssertNoErrors(discount2.L5_TypeInfo);

			collection.DeleteAll();

			discount1 = collection.AddNew();
			discount1.L5_Type = BillingConstants.DiscountType.Prepayment;
			AssertNoErrors(discount1.L5_TypeInfo);

			discount2 = collection.AddNew();
			discount2.L5_Type = BillingConstants.DiscountType.Prepayment;
			AssertHasError(discount2.L5_TypeInfo, "You can't have prepayment discounts with overlapping dates.");

			discount1.L5_StartDate = new ZDateTime(2010, 01, 01);
			discount1.L5_EndDate = new ZDateTime(2010, 12, 31);
			discount2.Validation.ValidateAll();
			AssertHasError(discount2.L5_TypeInfo, "You can't have prepayment discounts with overlapping dates.");

			discount2.L5_StartDate = new ZDateTime(2010, 08, 01);
			discount2.Validation.ValidateAll();
			AssertHasError(discount2.L5_TypeInfo, "You can't have prepayment discounts with overlapping dates.");

			discount2.L5_StartDate = new ZDateTime(2011, 01, 01);
			discount2.Validation.ValidateAll();
			AssertNoErrors(discount2.L5_TypeInfo);

			discount2.L5_Type = BillingConstants.DiscountType.Commitment;
			AssertNoErrors(discount2.L5_TypeInfo);

			discount2.L5_StartDate = new ZDateTime(2010, 06, 01);
			discount2.Validation.ValidateAll();
			AssertHasError(discount2.L5_TypeInfo, "You can't have commitment and prepayment discounts at the same time.");

			discount2.L5_Type = BillingConstants.DiscountType.Volume;
			AssertNoErrors(discount2.L5_TypeInfo);
		}

		public void TestCheckL5_ModuleCode()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var discount = org.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_ModuleCode = "XXX";
			AssertNoErrors(discount.L5_ModuleCodeInfo);

			discount.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			discount.L5_ModuleCode = "XXX";
			AssertHasErrors(discount.L5_ModuleCodeInfo);

			Factory.Save();
			discount.Validation.ValidateL5_ModuleCode();
			AssertNoErrors(discount.L5_ModuleCodeInfo);
			AssertHasWarnings(discount.L5_ModuleCodeInfo);

			discount.L5_ModuleCode = discount.Lookups.ModuleCodeList[0].Code;
			AssertNoErrors(discount.L5_ModuleCodeInfo);
		}

		public void TestCheckL5_BreakAmount()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.Validation.ValidateAll();
			AssertHasErrors(discount.L5_BreakAmountInfo);

			discount.L5_Type = BillingConstants.DiscountType.Capped;
			discount.Validation.ValidateAll();
			AssertNoErrors(discount.L5_BreakAmountInfo);

			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.Validation.ValidateAll();
			AssertNoErrors(discount.L5_BreakAmountInfo);

			discount.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			discount.Validation.ValidateAll();
			AssertNoErrors(discount.L5_BreakAmountInfo);

			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			discount.Validation.ValidateAll();
			AssertHasErrors(discount.L5_BreakAmountInfo);

			discount.L5_BreakAmount = 10m;
			AssertNoErrors(discount.L5_BreakAmountInfo);

			discount.L5_BreakAmount = -10m;
			AssertHasErrors(discount.L5_BreakAmountInfo);
		}

		public void TestCheckL5_BreakAmount_Volume()
		{
			ClientLicenceBillingDiscountCollection collection = new ClientLicenceBillingDiscountCollection(Factory.New<ClientLicenceBilling>());

			ClientLicenceBillingDiscount discount1 = collection.AddNew();
			discount1.L5_SystemCode = "SY1";
			discount1.L5_Type = BillingConstants.DiscountType.Volume;
			discount1.L5_BreakAmount = 10m;
			AssertNoErrors(discount1.L5_BreakAmountInfo);

			ClientLicenceBillingDiscount discount2 = collection.AddNew();
			discount2.L5_SystemCode = "DPS";
			discount2.L5_Type = BillingConstants.DiscountType.Volume;
			discount2.L5_BreakAmount = -20m;
			AssertHasErrors(discount2.L5_BreakAmountInfo);
		}

		public void TestCheckL5_Units_IncrementalVolume()
		{
			ClientLicenceBillingDiscountCollection collection = new ClientLicenceBillingDiscountCollection(Factory.New<ClientLicenceBilling>());
			ClientLicenceBillingDiscount discountWithAnotherCode = collection.AddNew();
			discountWithAnotherCode.L5_DiscountCode = "111";
			discountWithAnotherCode.L5_Type = BillingConstants.DiscountType.IncrementalVolume;
			discountWithAnotherCode.L5_Units = 10;

			ClientLicenceBillingDiscount discount1 = collection.AddNew();
			discount1.L5_Type = BillingConstants.DiscountType.IncrementalVolume;
			discount1.L5_Units = 10;
			AssertNoErrors(discount1.L5_UnitsInfo);

			ClientLicenceBillingDiscount discount2 = collection.AddNew();
			discount2.L5_Type = BillingConstants.DiscountType.IncrementalVolume;
			discount2.L5_Units = 20;
			AssertNoErrors(discount2.L5_UnitsInfo);

			ClientLicenceBillingDiscount discount3 = collection.AddNew();
			discount3.L5_SystemCode = "DPS";
			discount3.L5_Type = BillingConstants.DiscountType.IncrementalVolume;
			discount3.L5_Units = 10;
			AssertNoErrors(discount3.L5_UnitsInfo);

			discount3.L5_SystemCode = discount2.L5_SystemCode;
			discount3.Validation.ValidateL5_Units();
			AssertHasError(discount3.L5_UnitsInfo, "You can't have two equal incremental volume units.");
		}

		public void TestCheckL5_Units()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.Validation.ValidateAll();
			AssertNoErrors(discount.L5_UnitsInfo);

			discount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			discount.Validation.ValidateAll();
			AssertHasErrors(discount.L5_UnitsInfo);

			discount.L5_Units = 10;
			AssertNoErrors(discount.L5_UnitsInfo);

			discount.L5_Units = -10;
			AssertHasErrors(discount.L5_UnitsInfo);

			discount.L5_Units = 20;
			AssertNoErrors(discount.L5_UnitsInfo);

			discount.L5_Units = 0;
			AssertHasErrors(discount.L5_UnitsInfo);

			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.Validation.ValidateAll();
			AssertNoErrors("0 units allowed for ODM Min Fee", discount.L5_UnitsInfo);
		}

		public void TestCheckL5_Discount()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.Validation.ValidateAll();
			AssertHasErrors(discount.L5_DiscountInfo);

			discount.L5_Discount = 10m;
			AssertNoErrors(discount.L5_DiscountInfo);

			discount.L5_Discount = -10m;
			AssertHasErrors(discount.L5_DiscountInfo);

			discount.L5_Type = BillingConstants.DiscountType.Surcharge;
			discount.L5_Discount = -15m;
			AssertNoErrors(discount.L5_DiscountInfo);
			discount.L5_Discount = 10m;
			AssertHasErrors(discount.L5_DiscountInfo);

			discount.L5_Type = BillingConstants.DiscountType.Volume;
			discount.L5_Discount = 101m;
			AssertHasErrors(discount.L5_DiscountInfo);

			discount.L5_Discount = 20m;
			AssertNoErrors(discount.L5_DiscountInfo);

			discount.L5_Discount = 0m;
			AssertHasErrors(discount.L5_DiscountInfo);

			discount.L5_Type = BillingConstants.DiscountType.MinimumFee;
			discount.Validation.ValidateAll();
			AssertNoErrors("Discount not used for minimum fee", discount.L5_DiscountInfo);

			discount.L5_Type = "XXX";
			discount.L5_Discount = 0m;
			AssertHasErrors(discount.L5_DiscountInfo);

			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			discount.Validation.ValidateAll();
			AssertNoErrors("Commitment discount can have 0%", discount.L5_DiscountInfo);
		}

		public void TestCheckL5_Description()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_Description = "";
			AssertNoErrors(discount.L5_DescriptionInfo);

			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Description = "";
			AssertHasErrors(discount.L5_DescriptionInfo);

			discount.L5_Type = BillingConstants.DiscountType.Volume;
			discount.L5_Description = "";
			AssertNoErrors(discount.L5_DescriptionInfo);

			discount.L5_Type = BillingConstants.DiscountType.Surcharge;
			discount.L5_Description = "";
			AssertHasErrors(discount.L5_DescriptionInfo);

			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.L5_Description = "hello";
			AssertNoErrors(discount.L5_DescriptionInfo);
		}

		public void TestCheckL5_StartDate()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_StartDate = new ZDateTime(2010, 01, 01);
			AssertNoErrors(discount.L5_StartDateInfo);

			discount.L5_StartDate = new ZDateTime(2010, 01, 01);
			discount.L5_EndDate = new ZDateTime(2020, 01, 01);
			AssertNoErrors(discount.L5_StartDateInfo);

			discount.L5_EndDate = new ZDateTime(2000, 01, 01);
			discount.L5_StartDate = new ZDateTime(2010, 01, 01);
			AssertHasErrors(discount.L5_StartDateInfo);

			discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_StartDate = new ZDateTime(2010, 08, 01);
			AssertNoErrors(discount.L5_StartDateInfo);

			discount.L5_StartDate = ZDateTime.Empty;
			AssertNoErrors(discount.L5_StartDateInfo);

			discount.L5_StartDate = new ZDateTime(2010, 08, 02);
			AssertHasError(discount.L5_StartDateInfo, "Discount should start on the first day of the month.");

			discount.L5_StartDate = new ZDateTime(2010, 08, 15);
			AssertHasError(discount.L5_StartDateInfo, "Discount should start on the first day of the month.");

			discount.L5_StartDate = new ZDateTime(2010, 08, 01);
			AssertNoErrors(discount.L5_StartDateInfo);

			discount.L5_StartDate = new ZDateTime(2000, 1, 1);
			AssertNoErrors(discount.L5_StartDateInfo);
		}

		public void TestCheckL5_EndDate()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_EndDate = new ZDateTime(2012, 01, 31);
			AssertNoErrors(discount.L5_EndDateInfo);

			discount.L5_StartDate = new ZDateTime(2010, 01, 01);
			discount.L5_EndDate = new ZDateTime(2012, 01, 31);
			AssertNoErrors(discount.L5_EndDateInfo);

			discount.L5_StartDate = new ZDateTime(2013, 01, 01);
			discount.L5_EndDate = new ZDateTime(2012, 01, 31);
			AssertHasErrors(discount.L5_EndDateInfo);

			discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_EndDate = new ZDateTime(2010, 08, 31);
			AssertNoErrors(discount.L5_EndDateInfo);

			discount.L5_EndDate = ZDateTime.Empty;
			AssertNoErrors(discount.L5_EndDateInfo);

			discount.L5_EndDate = new ZDateTime(2010, 08, 30);
			AssertHasError(discount.L5_EndDateInfo, "Discount should end on the last day of the month.");

			discount.L5_EndDate = new ZDateTime(2010, 08, 15);
			AssertHasError(discount.L5_EndDateInfo, "Discount should end on the last day of the month.");

			discount.L5_EndDate = new ZDateTime(2010, 10, 31);
			AssertNoErrors(discount.L5_EndDateInfo);

			discount.L5_EndDate = new ZDateTime(ZDateTime.Today.Year + 30, 12, 31);
			AssertNoErrors(discount.L5_EndDateInfo);
		}

		public void TestCheckL5_BreakUnits()
		{
			ClientLicenceBillingDiscount discount = Factory.New<ClientLicenceBillingDiscount>();

			foreach (ICodeDescription pair in BillingConstants.GetDiscountTypeList())
			{
				discount.L5_Type = pair.Code;
				discount.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
				AssertNoErrors("Break-units of type currency is valid for all discount type", discount.L5_BreakUnitsInfo);
			}
			foreach (ICodeDescription pair in BillingConstants.GetDiscountTypeList())
			{
				discount.L5_Type = pair.Code;
				discount.L5_BreakUnits = BillingConstants.DiscountBreakUnit.LicenceUnits;
				if ((pair.Code == BillingConstants.DiscountType.Volume) || (pair.Code == BillingConstants.DiscountType.Commitment))
				{
					AssertNoErrors("Break-units of type licence-units is only valid for volume discount or commitment discount", discount.L5_BreakUnitsInfo);
				}
				else
				{
					AssertHasErrors("Break-units of type licence-units is only valid for volume discount or commitment discount", discount.L5_BreakUnitsInfo);
				}
			}

			discount.L5_BreakUnits = "";
			AssertHasErrors(discount.L5_BreakUnitsInfo);

			discount.L5_BreakUnits = "XXX";
			AssertHasErrors(discount.L5_BreakUnitsInfo);

			discount.L5_BreakUnits = "BBB";
			discount.L5_Type = BillingConstants.DiscountType.Volume;
			ClientLicenceBillingDiscount discount2 = Factory.New<ClientLicenceBillingDiscount>();
			discount2.L5_SystemCode = discount.L5_SystemCode;
			discount2.L5_BreakUnits = "AAA";
			discount2.L5_Type = BillingConstants.DiscountType.Volume;
			discount2.Validation.ValidateL5_BreakAmount();
			AssertHasErrors("Volume break units must be the same for discount with the same volume type, system and overlapping date.", discount2.L5_BreakUnitsInfo);
		}

		public void TestStandardDiscounts()
		{
			ClientLicenceBillingDiscountCollection collection = new ClientLicenceBillingDiscountCollection(Factory.New<ClientLicenceBilling>());
			ClientLicenceBillingDiscount discount = collection.AddNew();
			discount.L5_Discount = 10;
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_Type = BillingConstants.DiscountType.Capped;
			discount.L5_Description = "hello";
			discount.Validation.ValidateAll();
			AssertNoErrors(discount);

			discount.L5_DiscountCode = "V1";
			discount.Validation.ValidateAll();
			AssertHasError(discount.L5_TypeInfo, "Standard discounts cannot be commitment discounts.");

			discount.L5_Type = BillingConstants.DiscountType.Special;
			discount.Validation.ValidateAll();
			AssertNoErrors(discount);

			discount.L5_Duration = 5;
			discount.Validation.ValidateAll();
			AssertHasError(discount.L5_DurationInfo, "Standard discounts cannot have a duration.");
		}

		public void TestCheckL5_SubCode()
		{
			var discount = Factory.New<ClientLicenceBillingDiscount>();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ABMCustoms;
			discount.Validation.ValidateL5_SubCode();
			AssertNoErrors(discount.L5_SubCodeInfo);

			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			discount.Validation.ValidateL5_SubCode();
			AssertHasErrors(discount.L5_SubCodeInfo);

			discount.L5_SubCode = "AAA";
			AssertHasErrors(discount.L5_SubCodeInfo);

			discount.L5_SubCode = ABMCustomsTransactionTypes.Codes.Customs;
			AssertNoErrors(discount.L5_SubCodeInfo);
		}
	}
}
