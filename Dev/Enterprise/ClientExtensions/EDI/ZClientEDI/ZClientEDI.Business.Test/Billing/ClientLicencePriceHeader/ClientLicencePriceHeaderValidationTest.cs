using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Client.EDI.Billing.Business.BillingConstants;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicencePriceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckL6_Rounding()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_HasExchangeRates = true;
			AssertEquals(false, priceHeader.L6_Rounding_ReadOnly);
			priceHeader.L6_Rounding = "V1";
			priceHeader.Validation.ValidateL6_Rounding();
			AssertNoErrors(priceHeader.L6_RoundingInfo);
			AssertNoErrors(priceHeader.L6_RoundingForBindingInfo);

			priceHeader.L6_Rounding = "";
			AssertHasErrors(priceHeader.L6_RoundingInfo);
			AssertHasErrors(priceHeader.L6_RoundingForBindingInfo);

			priceHeader.L6_Rounding = "ZXY";
			AssertHasErrors(priceHeader.L6_RoundingInfo);
			AssertHasErrors(priceHeader.L6_RoundingForBindingInfo);
		}

		public void TestValidationOnlyLoadsItemsIfNeeded()
		{
			var lic = BillingTestHelper.CreateLicenceCompany(Factory, "ENT", "COM");
			var stlPrices = BillingTestHelper.CreateStlPriceList(lic, "USR", "#NP");
			stlPrices.L6_TestDbPriceCode = "#NP";

			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var pricesInNewFactory = factory2.Load<ClientLicencePriceHeader>(stlPrices.PK);
			pricesInNewFactory.Validation.ValidateAll();
			AssertEquals(false, pricesInNewFactory.AreItemsLoaded);
		}

		public void TestCheckL6_LicenceEdition()
		{
			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.Validation.ValidateAll();
			AssertHasErrors(priceHeader.L6_LicenceEditionInfo);

			priceHeader.L6_LicenceEdition = "XXX";
			AssertHasErrors(priceHeader.L6_LicenceEditionInfo);

			priceHeader.L6_LicenceEdition = BillingConstants.LicenceEdition.Universal;
			AssertNoErrors(priceHeader.L6_LicenceEditionInfo);

			priceHeader.L6_LicenceEdition = "";
			AssertHasErrors(priceHeader.L6_LicenceEditionInfo);

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.Validation.ValidateL6_LicenceEdition();
			AssertNoErrors(priceHeader.L6_LicenceEditionInfo);

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			priceHeader.Validation.ValidateL6_LicenceEdition();
			AssertNoErrors(priceHeader.L6_LicenceEditionInfo);
		}

		public void TestCheckL6_RN_NKCountry()
		{
			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.Validation.ValidateAll();

			priceHeader.L6_RN_NKCountry = "XX";
			AssertHasErrors(priceHeader.L6_RN_NKCountryInfo);

			priceHeader.L6_RN_NKCountry = "AU";
			AssertNoErrors(priceHeader.L6_RN_NKCountryInfo);
		}

		public void TestCheckL6_RX_NKCurrency()
		{
			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.Validation.ValidateAll();
			AssertHasErrors(priceHeader.L6_RX_NKCurrencyInfo);

			priceHeader.L6_RX_NKCurrency = "XXX";
			AssertHasErrors(priceHeader.L6_RX_NKCurrencyInfo);

			priceHeader.L6_RX_NKCurrency = "AUD";
			AssertNoErrors(priceHeader.L6_RX_NKCurrencyInfo);
		}

		public void TestCheckL6_ValidFrom()
		{
			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.Validation.ValidateAll();
			AssertHasErrors(priceHeader.L6_ValidFromInfo);

			priceHeader.L6_ValidFrom = ZDateTime.Now;
			AssertNoErrors(priceHeader.L6_ValidFromInfo);
		}

		[TestDate(2023, 01, 01)]
		public void TestCheckL6_ValidFrom_CargoWiseNextSystemCode_ShouldOnlyAllowFirstOfMonth()
		{
			var priceHeader1 = Factory.New<ClientLicencePriceHeader>();
			var priceHeader2 = Factory.New<ClientLicencePriceHeader>();
			priceHeader2.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;

			priceHeader1.Validation.ValidateAll();
			priceHeader2.Validation.ValidateAll();
			AssertHasErrors(priceHeader1.L6_ValidFromInfo);
			AssertHasErrors(priceHeader2.L6_ValidFromInfo);

			priceHeader1.L6_ValidFrom = priceHeader2.L6_ValidFrom = new ZDateTime(2024, 01, 05);
			AssertNoErrors(priceHeader1.L6_ValidFromInfo);
			AssertHasError(priceHeader2.L6_ValidFromInfo, "Valid From date for CargoWise Next price lists must be on the first of the month.");

			priceHeader1.L6_ValidFrom = priceHeader2.L6_ValidFrom = new ZDateTime(2024, 06, 12);
			AssertNoErrors(priceHeader1.L6_ValidFromInfo);
			AssertHasError(priceHeader2.L6_ValidFromInfo, "Valid From date for CargoWise Next price lists must be on the first of the month.");

			priceHeader1.L6_ValidFrom = priceHeader2.L6_ValidFrom = new ZDateTime(2024, 03, 01);
			AssertNoErrors(priceHeader1.L6_ValidFromInfo);
			AssertNoErrors(priceHeader1.L6_ValidFromInfo);
		}

		//This date is 1 second before 2024-06-01 after converting UTC time to the first timezone to move to a new day (Line Islands - Kiribati)
		//UTC time is 14 hours behind the first timezone
		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCheckL6_ValidFrom_CargoWiseNextSystemCode_ShouldOnlyAllowFutureDates()
		{
			var priceHeader1 = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var priceHeader2 = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			priceHeader2.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;

			priceHeader1.L6_ValidFrom = priceHeader2.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			AssertNoErrors(priceHeader1.L6_ValidFromInfo);
			AssertNoErrors(priceHeader2.L6_ValidFromInfo);

			TestDateAttribute.AddSeconds(1);

			priceHeader1.Validation.ValidateL6_ValidFrom();
			priceHeader2.Validation.ValidateL6_ValidFrom();
			AssertNoErrors("Should not have errors if not a CargoWise Next price list", priceHeader1.L6_ValidFromInfo);
			AssertHasError(priceHeader2.L6_ValidFromInfo, "Valid From date for CargoWise Next price lists must be in the future.");

			priceHeader2.L6_ValidFrom = new ZDateTime(2024, 07, 01);
			AssertNoErrors(priceHeader1.L6_ValidFromInfo);
			Factory.Save();

			TestDateAttribute.AddMonths(1);
			priceHeader2.Validation.ValidateL6_ValidFrom();
			AssertNoErrors("Should not have errors since value is unchanged and in the database", priceHeader2.L6_ValidFromInfo);

			priceHeader2.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			AssertHasError("Should have error since value has been updated to one in the past", priceHeader2.L6_ValidFromInfo, "Valid From date for CargoWise Next price lists must be in the future.");
		}

		//This date is 1 second before 2024-06-01 after converting UTC time to the first timezone to move to a new day (Line Islands - Kiribati)
		//UTC time is 14 hours behind the first timezone
		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCheckL6_ValidFrom_EditL6_SystemCode_CargoWiseNextSystemCode_ShouldOnlyAllowFutureDates()
		{
			var priceHeader1 = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var priceHeader2 = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			priceHeader2.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;

			priceHeader1.L6_ValidFrom = priceHeader2.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			AssertNoErrors(priceHeader1.L6_ValidFromInfo);
			AssertNoErrors(priceHeader2.L6_ValidFromInfo);
			Factory.Save();

			TestDateAttribute.AddSeconds(1);

			priceHeader1.Validation.ValidateL6_ValidFrom();
			priceHeader2.Validation.ValidateL6_ValidFrom();
			AssertNoErrors("Should not have errors if not a CargoWise Next price list", priceHeader1.L6_ValidFromInfo);
			AssertNoErrors("Should not have errors since value is unchanged and in the database", priceHeader1.L6_ValidFromInfo);

			priceHeader1.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader2.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			AssertHasError("Should have error since system code is now CargoWise Next and date is in the past", priceHeader1.L6_ValidFromInfo, "Valid From date for CargoWise Next price lists must be in the future.");
			AssertNoErrors("Should not have errors if not a CargoWise Next price list", priceHeader2.L6_ValidFromInfo);

			priceHeader2.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			AssertNoErrors("Should not have errors since value matches its database value", priceHeader2.L6_ValidFromInfo);
		}

		public void TestCheckL6_SystemCode()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();

			CreateStandardPricesCompany();

			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.Validation.ValidateAll();
			AssertNoErrors(priceHeader.L6_SystemCodeInfo);

			priceHeader.L6_SystemCode = "ZZZ";
			AssertHasErrors(priceHeader.L6_SystemCodeInfo);

			foreach (ICodeDescription pair in BillingConstants.PriceHeaderType.PriceHeaderTypeList)
			{
				if (!BillingConstants.PriceHeaderType.IsAlwaysStandard(pair.Code))
				{
					priceHeader.L6_SystemCode = pair.Code;
					AssertNoErrors(pair.Code, priceHeader.L6_SystemCodeInfo);
				}
			}

			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.STL;
			AssertHasErrors(priceHeader.L6_SystemCodeInfo);

			priceHeader.L6_LC = LicenceCompany.StandardPricesCompany.PK;
			priceHeader.Validation.ValidateL6_SystemCode();
			AssertNoErrors(priceHeader.L6_SystemCodeInfo);

			priceHeader.L6_LC = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.PK;
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.GlobalContainerTracking;
			AssertHasErrors(priceHeader.L6_SystemCodeInfo);

			priceHeader.L6_LC = LicenceCompany.StandardPricesCompany.PK;
			priceHeader.Validation.ValidateL6_SystemCode();
			AssertNoErrors(priceHeader.L6_SystemCodeInfo);

			priceHeader.L6_LC = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.PK;
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ABMCustoms;
			AssertHasErrors(priceHeader.L6_SystemCodeInfo);

			priceHeader.L6_LC = LicenceCompany.StandardPricesCompany.PK;
			priceHeader.Validation.ValidateL6_SystemCode();
			AssertNoErrors(priceHeader.L6_SystemCodeInfo);

			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		public void TestCheckL6_PricelistVersion()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();

			CreateStandardPricesCompany();
			ClientLicencePriceHeaderLookupsTest.AddPriceHeader(LicenceCompany.StandardPricesCompany, "V100");

			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.Validation.ValidateAll();
			AssertNoErrors("non-std can have blank version", priceHeader.L6_PricelistVersionInfo);

			priceHeader.L6_IsStandard = true;
			priceHeader.Validation.ValidateAll();
			AssertHasErrors("std can't have blank version", priceHeader.L6_PricelistVersionInfo);

			priceHeader.L6_PricelistVersion = "V100";
			priceHeader.Validation.ValidateAll();
			AssertNoErrors("std and version", priceHeader.L6_PricelistVersionInfo);

			AssertEquals("L6_PricelistVersion's max length should be equal to 50", 50, priceHeader.L6_PricelistVersionInfo.MaxLength);
			priceHeader.L6_PricelistVersion = new string('a', 50);
			priceHeader.Validation.ValidateAll();
			AssertNoErrors("L6_PricelistVersion's max length should be equal to 50", priceHeader.L6_PricelistVersionInfo);

			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		public void TestCheckL6_DiscountCode()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();

			CreateStandardPricesCompany();
			ClientLicencePriceHeaderLookupsTest.AddDiscount(LicenceCompany.StandardPricesCompany, "V100");

			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_UseStandardDiscount = false;
			priceHeader.Validation.ValidateAll();
			AssertNoErrors("can have blank", priceHeader.L6_DiscountCodeInfo);

			priceHeader.L6_DiscountCode = "unknown";
			AssertHasErrors("value not in list", priceHeader.L6_DiscountCodeInfo);

			priceHeader.L6_DiscountCode = "V100";
			AssertNoErrors("value in list", priceHeader.L6_DiscountCodeInfo);

			priceHeader.L6_PricelistVersion = "STL1";
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.Validation.ValidateL6_DiscountCode();
			AssertNoErrors(priceHeader.L6_DiscountCodeInfo);

			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.GlobalContainerTracking;
			priceHeader.Validation.ValidateL6_DiscountCode();
			AssertNoErrors(priceHeader.L6_DiscountCodeInfo);

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			priceHeader.Validation.ValidateL6_LicenceEdition();
			AssertNoErrors(priceHeader.L6_DiscountCodeInfo);

			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		public void TestCheckL6_HasExchangeRates()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_SystemCode = "ODM";
			priceHeader.L6_HasExchangeRates = true;
			AssertHasError(priceHeader.L6_HasExchangeRatesInfo, "Exchange Rates are not valid for ODM or Maintenance.");

			priceHeader.L6_SystemCode = "STL";
			priceHeader.L6_HasExchangeRates = true;
			priceHeader.RunPreSaveValidation();
			AssertNoError(priceHeader.L6_HasExchangeRatesInfo, "Exchange Rates are not valid for ODM or Maintenance.");
		}

		public void TestCheckL6_IsDisbursementBundle()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair("PL0", "PL0");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);
			AssertEquals(true, PriceHeaderType.IsGlobal("PL0"));

			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_SystemCode = "ODM";
			priceHeader.L6_IsDisbursementBundle = true;
			AssertHasError(priceHeader.L6_IsDisbursementBundleInfo, "You can only bundle Global Pricelists into the Disbursement Pricing Model.");
			priceHeader.L6_IsDisbursementBundle = false;
			AssertNoErrors(priceHeader.L6_IsDisbursementBundleInfo);

			priceHeader.L6_SystemCode = "PL0";
			priceHeader.L6_IsDisbursementBundle = true;
			AssertNoErrors(priceHeader.L6_IsDisbursementBundleInfo);
			priceHeader.L6_IsDisbursementBundle = false;
			AssertNoErrors(priceHeader.L6_IsDisbursementBundleInfo);
		}

		void CreateStandardPricesCompany()
		{
			ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
		}
	}
}
