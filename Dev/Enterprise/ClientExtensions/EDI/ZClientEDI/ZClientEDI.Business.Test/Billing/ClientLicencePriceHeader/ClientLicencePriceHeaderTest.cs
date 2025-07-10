using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicencePriceHeader))]
	internal class ClientLicencePriceHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestL6_Rounding()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			AssertEquals("", priceHeader.L6_Rounding);
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			priceHeader.L6_HasExchangeRates = true;
			AssertEquals("", priceHeader.L6_Rounding);
		}

		public void TestLicCompany()
		{
			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "XYZABC";
			testHeader.MainAddress.OA_Address1 = "Address";
			testHeader.CreateAndLoadLicenceForOrg();
			var priceHeader = testHeader.LicCompany.PriceHeaders.AddNew();
			AssertEquals(testHeader.LicCompany.PK, priceHeader.LicCompany.PK);
		}

		public void TestPropertiesReadOnly()
		{
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_UseStandardDiscount = false;

			foreach (ZPropertyInfo propertyInfo in priceHeader.ZPropertyInfoHash)
			{
				bool isReadonly = propertyInfo.Name == ClientLicencePriceHeader.Schema.L6_SystemCreateUser
					|| propertyInfo.Name == ClientLicencePriceHeader.Schema.L6_SystemCreateTimeUtc
					|| propertyInfo.Name == ClientLicencePriceHeader.Schema.L6_UseStandardDiscount
					|| propertyInfo.Name == ClientLicencePriceHeader.Schema.L6_HasExchangeRates
					|| propertyInfo.Name == ClientLicencePriceHeader.Schema.L6_Rounding
					|| propertyInfo.Name == nameof(ClientLicencePriceHeader.L6_RoundingForBinding);
				AssertEquals(propertyInfo.Name, isReadonly, propertyInfo.ReadOnly);
			}

			priceHeader.L6_IsStandard = true;
			ZPropertyInfo propertyInfoLicenceUnitRate = priceHeader.ZPropertyInfoHash[ClientLicencePriceHeader.Schema.L6_LicenceUnitRate];
			AssertEquals(true, propertyInfoLicenceUnitRate.ReadOnly);

			priceHeader.L6_IsStandard = false;
			ZPropertyInfo propertyInfoVersion = priceHeader.ZPropertyInfoHash[ClientLicencePriceHeader.Schema.L6_PricelistVersion];
			AssertEquals(false, propertyInfoVersion.ReadOnly);

			priceHeader.L6_IsStandard = true;
			propertyInfoVersion = priceHeader.ZPropertyInfoHash[ClientLicencePriceHeader.Schema.L6_PricelistVersion];
			AssertEquals(false, propertyInfoVersion.ReadOnly);

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			foreach (ZPropertyInfo propertyInfo in priceHeader.ZPropertyInfoHash)
			{
				AssertEquals(true, propertyInfo.ReadOnly);
			}
		}

		public void TestDiscounts()
		{
			LicenceCompany company = Factory.NewWithValidTestData<LicenceCompany>();
			ClientLicencePriceHeader prices = company.PriceHeaders.AddNew();
			prices.L6_UseStandardDiscount = false;
			AssertEquals("discount code blank", 0, prices.Discounts.Count);

			prices.L6_DiscountCode = "V1";
			AssertEquals("no std company defined", 0, prices.Discounts.Count);

			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var discount1 = stdCompany.SelfBilling.BillingDiscounts.AddNew();
			var discount2 = stdCompany.SelfBilling.BillingDiscounts.AddNew();
			var discount3 = stdCompany.SelfBilling.BillingDiscounts.AddNew();
			discount1.L5_DiscountCode = "V1";
			discount2.L5_DiscountCode = "V1";
			discount3.L5_DiscountCode = "V2";
			stdCompany.Factory.Save();

			AssertNotNull(LicenceCompany.StandardPricesCompany.ReadonlySelfBilling);

			prices.L6_DiscountCode = "V3";
			AssertEquals("no matching std discounts defined", 0, prices.Discounts.Count);

			prices.L6_DiscountCode = "V1";
			var discounts = prices.Discounts;
			AssertEquals(2, discounts.Count);
			Assert("discount1", discounts.Contains(discount1));
			Assert("discount2", discounts.Contains(discount2));

			prices.L6_DiscountCode = "V2";
			discounts = prices.Discounts;
			AssertEquals(1, discounts.Count);
			Assert("discount3", discounts.Contains(discount3));
		}

		public void TestL6_DiscountCode_ReadOnly()
		{
			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_UseStandardDiscount = true;
			var propertyInfo = priceHeader.L6_DiscountCodeInfo;
			AssertEquals(true, propertyInfo.ReadOnly);

			priceHeader.L6_UseStandardDiscount = false;
			AssertEquals(false, propertyInfo.ReadOnly);
		}

		public void TestL6_UseStandardDiscount_ReadOnly()
		{
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			stdCompany.Factory.Save();

			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_IsStandard = false;
			var propertyInfo = priceHeader.L6_UseStandardDiscountInfo;
			AssertEquals(true, propertyInfo.ReadOnly);

			priceHeader.L6_LC = stdCompany.PK;
			AssertEquals(false, propertyInfo.ReadOnly);
			priceHeader.L6_LC = ZGuid.Empty;

			priceHeader.L6_IsStandard = true;
			AssertEquals(false, propertyInfo.ReadOnly);
		}

		public void TestPriceItemsCollectionReadOnly()
		{
			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = true;
			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			AssertEquals(false, priceHeader.Items.ReadOnly);

			EDISecurityCheckpoints.OrgLicenceBilling.IsAllowed = false;
			priceHeader = Factory.New<ClientLicencePriceHeader>();
			AssertEquals(true, priceHeader.Items.ReadOnly);
		}

		public void TestL6_TestDbPriceCode()
		{
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);

			var stdPricelist1 = stdCompany.PriceHeaders.AddNew();
			stdPricelist1.L6_PricelistVersion = "CW1 v16.11";
			stdPricelist1.L6_TestDbPriceCode = "#N1";
			stdPricelist1.L6_LiveMonthsUntilTestDbBilling = 1;

			var stdPricelist2 = stdCompany.PriceHeaders.AddNew();
			stdPricelist2.L6_PricelistVersion = "CW1 v16.12";
			stdPricelist2.L6_TestDbPriceCode = "#N2";
			stdPricelist2.L6_LiveMonthsUntilTestDbBilling = 2;

			stdCompany.Factory.Save();

			var company = Factory.NewWithValidTestData<LicenceCompany>();
			ClientLicencePriceHeader pricelist = company.PriceHeaders.AddNew();
			pricelist.L6_IsStandard = false;
			AssertEquals("discount code blank", "#NP", pricelist.L6_TestDbPriceCode);

			pricelist.L6_IsStandard = true;
			pricelist.L6_PricelistVersion = "CW1 v16.10";
			AssertEquals("no matching std pricelist found", "#NP", pricelist.L6_TestDbPriceCode);
			AssertEquals("no matching std pricelist found", 3, pricelist.L6_LiveMonthsUntilTestDbBilling);

			pricelist.L6_PricelistVersion = "CW1 v16.11";
			AssertEquals("found matching std pricelist", "#N1", pricelist.L6_TestDbPriceCode);
			AssertEquals("found matching std pricelist", 1, pricelist.L6_LiveMonthsUntilTestDbBilling);

			pricelist.L6_PricelistVersion = "CW1 v16.12";
			AssertEquals("found matching std pricelist", "#N2", pricelist.L6_TestDbPriceCode);
			AssertEquals("found matching std pricelist", 2, pricelist.L6_LiveMonthsUntilTestDbBilling);
		}

		public void TestL6_PricelistVersionMaxLength()
		{
			var stdPricelist = Factory.New<ClientLicencePriceHeader>();
			stdPricelist.L6_TestDbPriceCode = "#N1";
			stdPricelist.L6_LiveMonthsUntilTestDbBilling = 1;

			AssertNoExceptionThrown("L6_PricelistVersion could accept the string whose length is 50",
				() =>
				{
					stdPricelist.L6_PricelistVersion = new string('a', 50);
					Factory.Save();
				});

			AssertEquals("L6_PricelistVersion's max length should be equal to 50", 50, ClientLicencePriceHeaderSchema.L6_PricelistVersion.MaxLength);
		}

		public void TestIsQuickTransactionalPricelist()
		{
			ClientLicencePriceHeader priceHeader = Factory.New<ClientLicencePriceHeader>();
			AssertEquals(true, priceHeader.IsQuickTransactionalPricelist);

			priceHeader.Items.AddNew().L7_Code = "AAA";
			AssertEquals(true, priceHeader.IsQuickTransactionalPricelist);

			priceHeader.Items.AddNew().L7_Code = BillingConstants.CoreModuleCode;
			AssertEquals(false, priceHeader.IsQuickTransactionalPricelist);
		}

		public void TestLogChanges()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RN_NKCountry = "AU";
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			priceHeader.L6_LicenceEdition = "EXP";

			Factory.Save();

			ZQuery query = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "PriceList");
			StmALog[] logs = org.Logs.Find(query);
			AssertEquals("No log for new added price header", 0, logs.Length);

			priceHeader.L6_RN_NKCountry = "US";
			priceHeader.L6_RX_NKCurrency = "USD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 3);
			priceHeader.L6_ValidTo = new ZDateTime(2010, 12, 31);
			priceHeader.L6_LicenceEdition = "COU";

			Factory.Save();

			logs = org.Logs.Find(query);
			AssertEquals("Should be one log for price header changes", 1, logs.Length);

			string expected = "PriceList | Country:AU=>US | Currency:AUD=>USD | ValidFrom:"
				+ new ZDateTime(2010, 1, 1).ToShortDateString() + "=>" + new ZDateTime(2010, 1, 3).ToShortDateString()
				+ " | ValidTo:=>" + new ZDateTime(2010, 12, 31).ToShortDateString()
				+ " | Edition:EXP=>COU";
			AssertEquals(expected, logs[0].SL_Reference);
			AssertEquals(Events.EditedARecordCode, logs[0].SL_SE_NKEvent);
		}

		public void TestHasSettings()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RN_NKCountry = "AU";
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			priceHeader.L6_LicenceEdition = "EXP";
			priceHeader.L6_PricelistVersion = "V102";

			Assert(priceHeader.HasSettings(BillingConstants.BillingSystem.ODM, "au", "exp", "aud"));
			Assert(priceHeader.HasSettings(BillingConstants.BillingSystem.ODM, "AU", "EXP", "AUD"));
			Assert(priceHeader.HasSettings(BillingConstants.BillingSystem.ODM, "AU", "EXP", "AUD", "v102"));
			Assert("wrong system", !priceHeader.HasSettings(BillingConstants.BillingSystem.Maintenance, "AU", "EXP", "AUD", "V102"));
			Assert("wrong country", !priceHeader.HasSettings(BillingConstants.BillingSystem.ODM, "US", "EXP", "AUD", "V102"));
			Assert("wrong edition", !priceHeader.HasSettings(BillingConstants.BillingSystem.ODM, "AU", "UNI", "AUD", "V102"));
			Assert("wrong currency", !priceHeader.HasSettings(BillingConstants.BillingSystem.ODM, "AU", "EXP", "NZD", "V102"));
			Assert("wrong version", !priceHeader.HasSettings(BillingConstants.BillingSystem.ODM, "AU", "EXP", "AUD", "V103"));
		}

		public void TestLocalOrStandardItems()
		{
			// Populate standard pricelist
			LicenceHeader stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			LicenceCompany stdLicCompany = stdHeader.Company;
			ClientLicencePriceHeader stdPriceList = stdLicCompany.PriceHeaders.AddNew();
			stdPriceList.L6_PricelistVersion = "V19";
			stdPriceList.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			stdPriceList.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			stdPriceList.L6_ValidTo = new ZDateTime(2010, 1, 31);
			ClientLicencePriceItem stdPriceItem1 = stdPriceList.Items.AddNew();
			stdPriceItem1.L7_Code = "ST1";
			ClientLicencePriceItem stdPriceItem2 = stdPriceList.Items.AddNew();
			stdPriceItem2.L7_Code = "ST2";
			ClientLicencePriceItem stdPriceItem3 = stdPriceList.Items.AddNew();
			stdPriceItem3.L7_Code = "ST3";
			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			// Validate standard pricelist
			EDIOrgHeader testHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader.OH_Code = "TGBLOG";
			testHeader.OH_RL_NKClosestPort = "AUBNE";
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceCompany testLicCompany = testHeader.LicCompany;
			ClientLicencePriceHeader testStandardPriceList = testLicCompany.PriceHeaders.AddNew();
			testStandardPriceList.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			testStandardPriceList.L6_IsStandard = true; // Standard pricelist
			testStandardPriceList.L6_PricelistVersion = "abc"; // invalid Pricelist version
			AssertEquals(testStandardPriceList.LocalOrStandardItems.Count, 0);
			testStandardPriceList.L6_PricelistVersion = "V19"; // valid Pricelist version
			AssertEquals(0, testStandardPriceList.Items.Count);
			AssertEquals(3, testStandardPriceList.LocalOrStandardItems.Count);
			Assert(testStandardPriceList.LocalOrStandardItems.Any(p => p.L7_Code == stdPriceItem1.L7_Code));
			Assert(testStandardPriceList.LocalOrStandardItems.Any(p => p.L7_Code == stdPriceItem2.L7_Code));
			Assert(testStandardPriceList.LocalOrStandardItems.Any(p => p.L7_Code == stdPriceItem3.L7_Code));

			// Validate local pricelist
			ClientLicencePriceHeader testLocalPriceList = testLicCompany.PriceHeaders.AddNew();
			testLocalPriceList.L6_IsStandard = false; // Local pricelist
			testLocalPriceList.L6_RN_NKCountry = "AU";
			testLocalPriceList.L6_RX_NKCurrency = "AUD";
			testLocalPriceList.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			testLocalPriceList.L6_LicenceEdition = "EXP";
			testLocalPriceList.L6_PricelistVersion = "V102";
			ClientLicencePriceItem priceItem1 = testLocalPriceList.Items.AddNew();
			priceItem1.L7_Code = "LO1";
			ClientLicencePriceItem priceItem2 = testLocalPriceList.Items.AddNew();
			priceItem2.L7_Code = "LO2";
			ClientLicencePriceItem priceItem3 = testLocalPriceList.Items.AddNew();
			priceItem3.L7_Code = "LO3";

			AssertEquals(3, testLocalPriceList.Items.Count);
			AssertEquals(3, testStandardPriceList.LocalOrStandardItems.Count);
			Assert(testLocalPriceList.HasSettings(BillingConstants.BillingSystem.ODM, "au", "exp", "aud"));
			Assert(testLocalPriceList.HasSettings(BillingConstants.BillingSystem.ODM, "AU", "EXP", "AUD"));
			Assert(testLocalPriceList.HasSettings(BillingConstants.BillingSystem.ODM, "AU", "EXP", "AUD", "v102"));
			Assert("wrong system", !testLocalPriceList.HasSettings(BillingConstants.BillingSystem.Maintenance, "AU", "EXP", "AUD", "V102"));
			Assert("wrong country", !testLocalPriceList.HasSettings(BillingConstants.BillingSystem.ODM, "US", "EXP", "AUD", "V102"));
			Assert("wrong edition", !testLocalPriceList.HasSettings(BillingConstants.BillingSystem.ODM, "AU", "UNI", "AUD", "V102"));
			Assert("wrong currency", !testLocalPriceList.HasSettings(BillingConstants.BillingSystem.ODM, "AU", "EXP", "NZD", "V102"));
			Assert("wrong version", !testLocalPriceList.HasSettings(BillingConstants.BillingSystem.ODM, "AU", "EXP", "AUD", "V103"));
			Assert(testLocalPriceList.LocalOrStandardItems.Any(p => p.L7_Code == priceItem2.L7_Code));
			Assert(testLocalPriceList.LocalOrStandardItems.Any(p => p.L7_Code == priceItem2.L7_Code));
			Assert(testLocalPriceList.LocalOrStandardItems.Any(p => p.L7_Code == priceItem2.L7_Code));
		}

		public void TestLicenceUnitRate()
		{
			// Populate standard pricelist
			LicenceHeader stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			LicenceCompany stdLicCompany = stdHeader.Company;
			ClientLicencePriceHeader stdPriceList = stdLicCompany.PriceHeaders.AddNew();
			stdPriceList.L6_PricelistVersion = "V19";
			stdPriceList.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			stdPriceList.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			stdPriceList.L6_ValidTo = new ZDateTime(2010, 1, 31);
			stdPriceList.L6_LicenceUnitRate = 1.1234m;

			ClientLicencePriceHeader stdPriceList2 = stdLicCompany.PriceHeaders.AddNew();
			stdPriceList2.L6_PricelistVersion = "V20";
			stdPriceList2.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			stdPriceList2.L6_ValidFrom = new ZDateTime(2010, 1, 15);
			stdPriceList2.L6_ValidTo = new ZDateTime(2010, 1, 31);
			stdPriceList2.L6_LicenceUnitRate = 2.2222m;

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdHeader);

			// Validate standard pricelist
			EDIOrgHeader testHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			testHeader.OH_Code = "TGBLOG";
			testHeader.OH_RL_NKClosestPort = "AUBNE";
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceCompany testLicCompany = testHeader.LicCompany;
			ClientLicencePriceHeader testStandardPriceList = testLicCompany.PriceHeaders.AddNew();
			testStandardPriceList.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			testStandardPriceList.L6_IsStandard = true; // Standard pricelist
			testStandardPriceList.L6_SystemCode = stdPriceList.L6_SystemCode;
			testStandardPriceList.L6_RN_NKCountry = stdPriceList.L6_RN_NKCountry;
			testStandardPriceList.L6_LicenceEdition = stdPriceList.L6_LicenceEdition;
			testStandardPriceList.L6_RX_NKCurrency = stdPriceList.L6_RX_NKCurrency;
			testStandardPriceList.L6_PricelistVersion = stdPriceList.L6_PricelistVersion;

			AssertEquals("Licence Unit Rate must be retrieved from Standard pricelist", 1.1234m, testStandardPriceList.L6_LicenceUnitRate);

			testStandardPriceList.L6_PricelistVersion = stdPriceList2.L6_PricelistVersion;
			AssertEquals("Licence Unit Rate must be retrieved other Standard pricelist", 2.2222m, testStandardPriceList.L6_LicenceUnitRate);
		}

		[TestDate(2024, 01, 01)]
		public void TestExchangeRates()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RN_NKCountry = "AU";
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2025, 1, 1);
			priceHeader.L6_LicenceEdition = "EXP";
			priceHeader.L6_PricelistVersion = "V102";
			priceHeader.L6_SystemCode = "ODM";

			var expectedReadonly = new string[]
			{
				BillingConstants.PriceHeaderType.ABMCustoms,
				BillingConstants.PriceHeaderType.BorderWise,
				BillingConstants.PriceHeaderType.GlobalContainerTracking,
				BillingConstants.PriceHeaderType.EHub,
				BillingConstants.PriceHeaderType.FlightStats,
				BillingConstants.PriceHeaderType.LDaaS,
				BillingConstants.PriceHeaderType.Maintenance,
				BillingConstants.PriceHeaderType.ODM,
				BillingConstants.PriceHeaderType.Other,
			};

			var expectedWriteable = new string[]
			{
				BillingConstants.PriceHeaderType.STL,
				BillingConstants.PriceHeaderType.GoldenTax,
				BillingConstants.PriceHeaderType.CargoWiseNext,
			};

			foreach (var priceHeaderCode in BillingConstants.PriceHeaderType.GetPriceHeaderTypeList().GetAllCodes())
			{
				if (expectedReadonly.Contains(priceHeaderCode))
				{
					priceHeader.L6_SystemCode = priceHeaderCode;
					AssertEquals(priceHeaderCode, true, priceHeader.L6_HasExchangeRatesInfo.ReadOnly);
					AssertEquals(priceHeaderCode, true, priceHeader.ExchangeRates.ReadOnly);
				}
				else if (expectedWriteable.Contains(priceHeaderCode))
				{
					priceHeader.L6_SystemCode = priceHeaderCode;
					AssertEquals(priceHeaderCode, false, priceHeader.L6_HasExchangeRatesInfo.ReadOnly);
					priceHeader.L6_HasExchangeRates = true;
					AssertEquals(priceHeaderCode, false, priceHeader.ExchangeRates.ReadOnly);
				}
				else
				{
					Fail("Price header code not covered: " + priceHeaderCode);
				}
			}
		}

		public void TestIsSecondaryPriceListTypeWithLocallyUniqueCodes()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			var expectedTypes = new string[] { BillingConstants.PriceHeaderType.BorderWise };

			foreach (ICodeDescription pair in BillingConstants.PriceHeaderType.GetPriceHeaderTypeList())
			{
				priceHeader.L6_SystemCode = pair.Code;
				bool expected = expectedTypes.Any(x => x == pair.Code);
				AssertEquals(pair.Code, expected, priceHeader.IsSecondaryPriceListTypeWithLocallyUniqueCodes);
			}
		}

		[TestDate(2024, 05, 01)]
		public void TestIsCargoWiseNext()
		{
			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;

			AssertEquals(false, genericPriceHeader.IsCargoWiseNext);
			AssertEquals(true, firstActiveCargoWiseNextPriceHeader.IsCargoWiseNext);
		}

		[TestDate(2024, 05, 01)]
		public void TestIsCargoWiseNextAndPastValidFrom()
		{
			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var secondActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;

			genericPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 07, 01);

			AssertEquals(false, genericPriceHeader.IsCargoWiseNextAndPastValidFrom);
			AssertEquals(false, firstActiveCargoWiseNextPriceHeader.IsCargoWiseNextAndPastValidFrom);
			AssertEquals(false, secondActiveCargoWiseNextPriceHeader.IsCargoWiseNextAndPastValidFrom);

			TestDateAttribute.AddMonths(1);

			AssertEquals(false, genericPriceHeader.IsCargoWiseNextAndPastValidFrom);
			AssertEquals(true, firstActiveCargoWiseNextPriceHeader.IsCargoWiseNextAndPastValidFrom);
			AssertEquals(false, secondActiveCargoWiseNextPriceHeader.IsCargoWiseNextAndPastValidFrom);
		}

		//This date is 1 second before 2024-06-01 after converting UTC time to the first timezone to move to a new day (Line Islands - Kiribati)
		//UTC time is 14 hours behind the first timezone
		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCargoWiseNextSystemCode_PastValidFromInWorldsEarliestTimezone_ShouldBeReadOnly()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var secondActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			genericPriceHeader.L6_HasExchangeRates = firstActiveCargoWiseNextPriceHeader.L6_HasExchangeRates = secondActiveCargoWiseNextPriceHeader.L6_HasExchangeRates = true;
			genericPriceHeader.L6_UseStandardDiscount = firstActiveCargoWiseNextPriceHeader.L6_UseStandardDiscount = secondActiveCargoWiseNextPriceHeader.L6_UseStandardDiscount = false;
			genericPriceHeader.L6_DiscountCode = "V1";
			firstActiveCargoWiseNextPriceHeader.L6_DiscountCode = "V2";
			secondActiveCargoWiseNextPriceHeader.L6_DiscountCode = "V3";

			genericPriceHeader.L6_ValidFrom = new ZDateTime(2024, 05, 01);
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 07, 01);

			var genericItem = genericPriceHeader.Items.AddNew();
			var item1 = firstActiveCargoWiseNextPriceHeader.Items.AddNew();
			var item2 = secondActiveCargoWiseNextPriceHeader.Items.AddNew();
			var genericStlDiscount = genericPriceHeader.StlDiscounts.AddNew();
			genericStlDiscount.PHD_Name = "A";
			genericStlDiscount.PHD_Version = genericPriceHeader.L6_DiscountCode;
			var genericStlItemDiscount = genericPriceHeader.StlItemDiscounts.AddNew();
			genericStlItemDiscount.PGM_PHD = genericStlDiscount.PK;
			genericStlItemDiscount.PGM_GroupCode = "G1";
			var stlDiscount1 = firstActiveCargoWiseNextPriceHeader.StlDiscounts.AddNew();
			stlDiscount1.PHD_Name = "B";
			stlDiscount1.PHD_Version = firstActiveCargoWiseNextPriceHeader.L6_DiscountCode;
			var stlItemDiscount1 = firstActiveCargoWiseNextPriceHeader.StlItemDiscounts.AddNew();
			stlItemDiscount1.PGM_PHD = stlDiscount1.PK;
			stlItemDiscount1.PGM_GroupCode = "G1";
			var stlDiscount2 = secondActiveCargoWiseNextPriceHeader.StlDiscounts.AddNew();
			stlDiscount2.PHD_Name = "C";
			stlDiscount2.PHD_Version = secondActiveCargoWiseNextPriceHeader.L6_DiscountCode;
			var stlItemDiscount2 = secondActiveCargoWiseNextPriceHeader.StlItemDiscounts.AddNew();
			stlItemDiscount2.PGM_PHD = stlDiscount2.PK;
			stlItemDiscount2.PGM_GroupCode = "G1";
			var genericUsageMapping = genericPriceHeader.UsageMaps.AddNew();
			var usageMapping1 = firstActiveCargoWiseNextPriceHeader.UsageMaps.AddNew();
			var usageMapping2 = secondActiveCargoWiseNextPriceHeader.UsageMaps.AddNew();
			var genericExchangeRate = genericPriceHeader.ExchangeRates.AddNew();
			var exchangeRate1 = firstActiveCargoWiseNextPriceHeader.ExchangeRates.AddNew();
			var exchangeRate2 = secondActiveCargoWiseNextPriceHeader.ExchangeRates.AddNew();

			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.Items.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericItem.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericStlDiscount.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericStlItemDiscount.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericUsageMapping.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericExchangeRate.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, firstActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, firstActiveCargoWiseNextPriceHeader.Items.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, firstActiveCargoWiseNextPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, firstActiveCargoWiseNextPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, firstActiveCargoWiseNextPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, firstActiveCargoWiseNextPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, item1.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, stlDiscount1.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, stlItemDiscount1.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, usageMapping1.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, exchangeRate1.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.Items.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, item2.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, stlDiscount2.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, stlItemDiscount2.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, usageMapping2.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, exchangeRate2.ReadOnly);

			Factory.Save();

			TestDateAttribute.AddSeconds(1);
			var newFactory = new BusinessObjectFactory();
			genericPriceHeader = newFactory.Load<ClientLicencePriceHeader>(genericPriceHeader.PK);
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			secondActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(secondActiveCargoWiseNextPriceHeader.PK);
			genericItem = genericPriceHeader.Items[0];
			item1 = firstActiveCargoWiseNextPriceHeader.Items[0];
			item2 = secondActiveCargoWiseNextPriceHeader.Items[0];
			genericStlDiscount = genericPriceHeader.StlDiscounts[0];
			genericStlItemDiscount = genericPriceHeader.StlItemDiscounts[0];
			stlDiscount1 = firstActiveCargoWiseNextPriceHeader.StlDiscounts[0];
			stlItemDiscount1 = firstActiveCargoWiseNextPriceHeader.StlItemDiscounts[0];
			stlDiscount2 = secondActiveCargoWiseNextPriceHeader.StlDiscounts[0];
			stlItemDiscount2 = secondActiveCargoWiseNextPriceHeader.StlItemDiscounts[0];
			genericUsageMapping = genericPriceHeader.UsageMaps[0];
			usageMapping1 = firstActiveCargoWiseNextPriceHeader.UsageMaps[0];
			usageMapping2 = secondActiveCargoWiseNextPriceHeader.UsageMaps[0];
			genericExchangeRate = genericPriceHeader.ExchangeRates[0];
			exchangeRate1 = firstActiveCargoWiseNextPriceHeader.ExchangeRates[0];
			exchangeRate2 = secondActiveCargoWiseNextPriceHeader.ExchangeRates[0];

			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.Items.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericItem.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericStlDiscount.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericStlItemDiscount.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericUsageMapping.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericExchangeRate.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.Items.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, item1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, stlDiscount1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, stlItemDiscount1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, usageMapping1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, exchangeRate1.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.Items.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, secondActiveCargoWiseNextPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, item2.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, stlDiscount2.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, stlItemDiscount2.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, usageMapping2.ReadOnly);
			AssertEquals("Should not be readonly since current date is prior to its valid from date", false, exchangeRate2.ReadOnly);

			TestDateAttribute.AddMonths(1);
			newFactory = new BusinessObjectFactory();
			genericPriceHeader = newFactory.Load<ClientLicencePriceHeader>(genericPriceHeader.PK);
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			secondActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(secondActiveCargoWiseNextPriceHeader.PK);
			genericItem = genericPriceHeader.Items[0];
			item1 = firstActiveCargoWiseNextPriceHeader.Items[0];
			item2 = secondActiveCargoWiseNextPriceHeader.Items[0];
			genericStlDiscount = genericPriceHeader.StlDiscounts[0];
			genericStlItemDiscount = genericPriceHeader.StlItemDiscounts[0];
			stlDiscount1 = firstActiveCargoWiseNextPriceHeader.StlDiscounts[0];
			stlItemDiscount1 = firstActiveCargoWiseNextPriceHeader.StlItemDiscounts[0];
			stlDiscount2 = secondActiveCargoWiseNextPriceHeader.StlDiscounts[0];
			stlItemDiscount2 = secondActiveCargoWiseNextPriceHeader.StlItemDiscounts[0];
			genericUsageMapping = genericPriceHeader.UsageMaps[0];
			usageMapping1 = firstActiveCargoWiseNextPriceHeader.UsageMaps[0];
			usageMapping2 = secondActiveCargoWiseNextPriceHeader.UsageMaps[0];
			genericExchangeRate = genericPriceHeader.ExchangeRates[0];
			exchangeRate1 = firstActiveCargoWiseNextPriceHeader.ExchangeRates[0];
			exchangeRate2 = secondActiveCargoWiseNextPriceHeader.ExchangeRates[0];

			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.Items.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Generic price headers should not be readonly", false, genericPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericItem.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericStlDiscount.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericStlItemDiscount.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericUsageMapping.ReadOnly);
			AssertEquals("Generic price headers' items should not be readonly", false, genericExchangeRate.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.Items.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, firstActiveCargoWiseNextPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, item1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, stlDiscount1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, stlItemDiscount1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, usageMapping1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, exchangeRate1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, secondActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, secondActiveCargoWiseNextPriceHeader.Items.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, secondActiveCargoWiseNextPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, secondActiveCargoWiseNextPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, secondActiveCargoWiseNextPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, secondActiveCargoWiseNextPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, item2.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, stlDiscount2.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, stlItemDiscount2.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, usageMapping2.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date", true, exchangeRate2.ReadOnly);
		}

		[TestDate(2024, 07, 01)]
		public void TestCargoWiseNextSystemCode_PastValidFrom_ShouldBeEditableIfNotInDatabase()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			firstActiveCargoWiseNextPriceHeader.L6_HasExchangeRates = true;
			firstActiveCargoWiseNextPriceHeader.L6_UseStandardDiscount = false;
			firstActiveCargoWiseNextPriceHeader.L6_DiscountCode = "V2";

			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);

			var item1 = firstActiveCargoWiseNextPriceHeader.Items.AddNew();
			var stlDiscount1 = firstActiveCargoWiseNextPriceHeader.StlDiscounts.AddNew();
			stlDiscount1.PHD_Name = "B";
			stlDiscount1.PHD_Version = firstActiveCargoWiseNextPriceHeader.L6_DiscountCode;
			var stlItemDiscount1 = firstActiveCargoWiseNextPriceHeader.StlItemDiscounts.AddNew();
			stlItemDiscount1.PGM_PHD = stlDiscount1.PK;
			stlItemDiscount1.PGM_GroupCode = "G1";
			var usageMapping1 = firstActiveCargoWiseNextPriceHeader.UsageMaps.AddNew();
			var exchangeRate1 = firstActiveCargoWiseNextPriceHeader.ExchangeRates.AddNew();

			AssertEquals("Should not be readonly since the price list is unsaved", false, firstActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should not be readonly since the price list is unsaved", false, firstActiveCargoWiseNextPriceHeader.Items.ReadOnly);
			AssertEquals("Should not be readonly since the price list is unsaved", false, firstActiveCargoWiseNextPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Should not be readonly since the price list is unsaved", false, firstActiveCargoWiseNextPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Should not be readonly since the price list is unsaved", false, firstActiveCargoWiseNextPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Should not be readonly since the price list is unsaved", false, firstActiveCargoWiseNextPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Should not be readonly since the price list is unsaved", false, item1.ReadOnly);
			AssertEquals("Should not be readonly since the price list is unsaved", false, stlDiscount1.ReadOnly);
			AssertEquals("Should not be readonly since the price list is unsaved", false, stlItemDiscount1.ReadOnly);
			AssertEquals("Should not be readonly since the price list is unsaved", false, usageMapping1.ReadOnly);
			AssertEquals("Should not be readonly since the price list is unsaved", false, exchangeRate1.ReadOnly);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			item1 = firstActiveCargoWiseNextPriceHeader.Items[0];
			stlDiscount1 = firstActiveCargoWiseNextPriceHeader.StlDiscounts[0];
			stlItemDiscount1 = firstActiveCargoWiseNextPriceHeader.StlItemDiscounts[0];
			usageMapping1 = firstActiveCargoWiseNextPriceHeader.UsageMaps[0];
			exchangeRate1 = firstActiveCargoWiseNextPriceHeader.ExchangeRates[0];

			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, firstActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, firstActiveCargoWiseNextPriceHeader.Items.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, firstActiveCargoWiseNextPriceHeader.StlDiscounts.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, firstActiveCargoWiseNextPriceHeader.StlItemDiscounts.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, firstActiveCargoWiseNextPriceHeader.UsageMaps.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, firstActiveCargoWiseNextPriceHeader.ExchangeRates.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, item1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, stlDiscount1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, stlItemDiscount1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, usageMapping1.ReadOnly);
			AssertEquals("Should be readonly since current time in Line Islands is past the valid from date and is in database", true, exchangeRate1.ReadOnly);
		}

		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCargoWiseNextSystemCode_HasChangesPastValidFromInWorldsEarliestTimezone_ShouldThrowRowErrorBeforeSaving()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var secondActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;

			genericPriceHeader.L6_ValidFrom = new ZDateTime(2024, 05, 01);
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 07, 01);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			genericPriceHeader = newFactory.Load<ClientLicencePriceHeader>(genericPriceHeader.PK);
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			secondActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(secondActiveCargoWiseNextPriceHeader.PK);
			TestDateAttribute.AddSeconds(1);

			genericPriceHeader.L6_RN_NKCountry = firstActiveCargoWiseNextPriceHeader.L6_RN_NKCountry = secondActiveCargoWiseNextPriceHeader.L6_RN_NKCountry = "NZ";
			genericPriceHeader.RunPreSaveValidation();
			firstActiveCargoWiseNextPriceHeader.RunPreSaveValidation();
			secondActiveCargoWiseNextPriceHeader.RunPreSaveValidation();

			AssertEquals("Should not have row errors if non CargoWise Next", false, genericPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if CargoWise Next and past valid from date", true, firstActiveCargoWiseNextPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if CargoWise Next and past valid from date", 1, firstActiveCargoWiseNextPriceHeader.RowErrors.Count());
			AssertEquals("Should have row error if CargoWise Next and past valid from date", "Price list cannot be edited past its Valid From date.", firstActiveCargoWiseNextPriceHeader.RowErrors.First().Message);
			AssertEquals("Should not have row errors if before valid from date", false, secondActiveCargoWiseNextPriceHeader.HasRowErrors);

			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 07, 01);
			AssertNoErrors("Should have no validation error since valid from date is in the future", firstActiveCargoWiseNextPriceHeader.L6_ValidFromInfo);
			AssertEquals("Should remove row error since valid from date has been changed", false, firstActiveCargoWiseNextPriceHeader.HasRowErrors);

			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 05, 01);
			AssertHasError("Should have normal validation error since valid from date has been changed", firstActiveCargoWiseNextPriceHeader.L6_ValidFromInfo, "Valid From date for CargoWise Next price lists must be in the future.");
			AssertEquals("Should remove row error since valid from date has been changed", false, firstActiveCargoWiseNextPriceHeader.HasRowErrors);
		}

		[TestDate(2024, 01, 01)]
		public void TestCargoWiseNextSystemCode_PastValidFromInWorldsEarliestTimezone_ShouldOnlyBeReadOnlyIfSystemCodeAndValidFromAreUnchanged()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var secondActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			genericPriceHeader.L6_HasExchangeRates = firstActiveCargoWiseNextPriceHeader.L6_HasExchangeRates = secondActiveCargoWiseNextPriceHeader.L6_HasExchangeRates = true;
			genericPriceHeader.L6_UseStandardDiscount = firstActiveCargoWiseNextPriceHeader.L6_UseStandardDiscount = secondActiveCargoWiseNextPriceHeader.L6_UseStandardDiscount = false;
			genericPriceHeader.L6_DiscountCode = "V1";
			firstActiveCargoWiseNextPriceHeader.L6_DiscountCode = "V2";
			secondActiveCargoWiseNextPriceHeader.L6_DiscountCode = "V3";

			genericPriceHeader.L6_ValidFrom = new ZDateTime(2024, 02, 01);
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 02, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2023, 12, 01);
			Factory.Save();

			AssertEquals("Should be editable since it is not CargoWise Next", false, genericPriceHeader.ReadOnly);
			AssertEquals("Should be editable since its valid from date is in the future", false, firstActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should be editable since it is not CargoWise Next", false, secondActiveCargoWiseNextPriceHeader.ReadOnly);

			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 01, 01);
			secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			AssertEquals("Should not be readonly since it has changes", false, firstActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should not be readonly since it has changes", false, secondActiveCargoWiseNextPriceHeader.ReadOnly);

			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 02, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 03, 01);
			Factory.Save();
			TestDateAttribute.AddMonths(5);
			var newFactory = new BusinessObjectFactory();
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			secondActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(secondActiveCargoWiseNextPriceHeader.PK);
			AssertEquals("Should be readonly since system code and valid from do not have changes", true, firstActiveCargoWiseNextPriceHeader.ReadOnly);
			AssertEquals("Should be readonly since system code and valid from do not have changes", true, secondActiveCargoWiseNextPriceHeader.ReadOnly);
		}

		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCargoWiseNextSystemCode_HasChangesPastValidFromInWorldsEarliestTimezone_ShouldThrowRowErrorBeforeFirstSave()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var secondActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;

			genericPriceHeader.L6_ValidFrom = new ZDateTime(2024, 05, 01);
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 07, 01);
			TestDateAttribute.AddSeconds(1);

			genericPriceHeader.RunPreSaveValidation();
			firstActiveCargoWiseNextPriceHeader.RunPreSaveValidation();
			secondActiveCargoWiseNextPriceHeader.RunPreSaveValidation();

			AssertEquals("Should not have row errors if non CargoWise Next", false, genericPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if CargoWise Next and past valid from date", true, firstActiveCargoWiseNextPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if CargoWise Next and past valid from date", 1, firstActiveCargoWiseNextPriceHeader.RowErrors.Count());
			AssertEquals("Should have row error if CargoWise Next and past valid from date", "Price list cannot be edited past its Valid From date.", firstActiveCargoWiseNextPriceHeader.RowErrors.First().Message);
			AssertEquals("Should not have row errors if before valid from date", false, secondActiveCargoWiseNextPriceHeader.HasRowErrors);
		}

		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCargoWiseNextSystemCode_HasChangesPastValidFromInWorldsEarliestTimezone_ThrowRowErrorBeforeSaving_ShouldClearOnL6_SystemCodeEdit()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			genericPriceHeader = newFactory.Load<ClientLicencePriceHeader>(genericPriceHeader.PK);
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			TestDateAttribute.AddSeconds(1);

			genericPriceHeader.L6_RN_NKCountry = firstActiveCargoWiseNextPriceHeader.L6_RN_NKCountry = "NZ";
			genericPriceHeader.RunPreSaveValidation();
			firstActiveCargoWiseNextPriceHeader.RunPreSaveValidation();

			AssertEquals("Precondition: Should not have row errors if non CargoWise Next", false, genericPriceHeader.HasRowErrors);
			AssertEquals("Precondition: Should have row error if CargoWise Next and past valid from date", true, firstActiveCargoWiseNextPriceHeader.HasRowErrors);
			AssertEquals("Precondition: Should have row error if CargoWise Next and past valid from date", 1, firstActiveCargoWiseNextPriceHeader.RowErrors.Count());
			AssertEquals("Precondition: Should have row error if CargoWise Next and past valid from date", "Price list cannot be edited past its Valid From date.", firstActiveCargoWiseNextPriceHeader.RowErrors.First().Message);

			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			AssertNoErrors("Should have no validation error since system code is not CargoWise Next", firstActiveCargoWiseNextPriceHeader.L6_ValidFromInfo);
			AssertEquals("Should remove row error since valid from date has been changed", false, firstActiveCargoWiseNextPriceHeader.HasRowErrors);

			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			firstActiveCargoWiseNextPriceHeader.RunPreSaveValidation();
			AssertEquals("Precondition: Should have row error if CargoWise Next and past valid from date", true, firstActiveCargoWiseNextPriceHeader.HasRowErrors);
			AssertEquals("Precondition: Should have row error if CargoWise Next and past valid from date", 1, firstActiveCargoWiseNextPriceHeader.RowErrors.Count());
			AssertEquals("Precondition: Should have row error if CargoWise Next and past valid from date", "Price list cannot be edited past its Valid From date.", firstActiveCargoWiseNextPriceHeader.RowErrors.First().Message);
		}

		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCargoWiseNextSystemCode_ItemHasChangesPastValidFromInWorldsEarliestTimezone_ShouldThrowRowErrorBeforeSaving()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var secondActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;

			genericPriceHeader.L6_ValidFrom = new ZDateTime(2024, 05, 01);
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 07, 01);

			genericPriceHeader.Items.AddNew();
			firstActiveCargoWiseNextPriceHeader.Items.AddNew();
			secondActiveCargoWiseNextPriceHeader.Items.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			genericPriceHeader = newFactory.Load<ClientLicencePriceHeader>(genericPriceHeader.PK);
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			secondActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(secondActiveCargoWiseNextPriceHeader.PK);
			TestDateAttribute.AddSeconds(1);

			var genericItem = genericPriceHeader.Items[0];
			var item1 = firstActiveCargoWiseNextPriceHeader.Items[0];
			var item2 = secondActiveCargoWiseNextPriceHeader.Items[0];

			AssertEquals("Precondition", false, genericPriceHeader.HasChanges);
			AssertEquals("Precondition", false, firstActiveCargoWiseNextPriceHeader.HasChanges);
			AssertEquals("Precondition", false, secondActiveCargoWiseNextPriceHeader.HasChanges);
			genericItem.L7_Description = item1.L7_Description = item2.L7_Description = "ABC";
			genericPriceHeader.RunPreSaveValidation();
			firstActiveCargoWiseNextPriceHeader.RunPreSaveValidation();
			secondActiveCargoWiseNextPriceHeader.RunPreSaveValidation();

			AssertEquals("Should not have row errors if non CargoWise Next", false, genericPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", true, firstActiveCargoWiseNextPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", 1, firstActiveCargoWiseNextPriceHeader.RowErrors.Count());
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", "Price list cannot be edited past its Valid From date.", firstActiveCargoWiseNextPriceHeader.RowErrors.First().Message);
			AssertEquals("Should not have row errors if before valid from date", false, secondActiveCargoWiseNextPriceHeader.HasRowErrors);
		}

		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCargoWiseNextSystemCode_StlDiscountHasChangesPastValidFromInWorldsEarliestTimezone_ShouldThrowRowErrorBeforeSaving()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var secondActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			genericPriceHeader.L6_UseStandardDiscount = firstActiveCargoWiseNextPriceHeader.L6_UseStandardDiscount = secondActiveCargoWiseNextPriceHeader.L6_UseStandardDiscount = false;
			genericPriceHeader.L6_DiscountCode = "V1";
			firstActiveCargoWiseNextPriceHeader.L6_DiscountCode = "V2";
			secondActiveCargoWiseNextPriceHeader.L6_DiscountCode = "V3";

			genericPriceHeader.L6_ValidFrom = new ZDateTime(2024, 05, 01);
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 07, 01);

			var genericStlDiscount = genericPriceHeader.StlDiscounts.AddNew();
			genericStlDiscount.PHD_Name = "A";
			genericStlDiscount.PHD_Version = genericPriceHeader.L6_DiscountCode;
			var stlDiscount1 = firstActiveCargoWiseNextPriceHeader.StlDiscounts.AddNew();
			stlDiscount1.PHD_Name = "B";
			stlDiscount1.PHD_Version = firstActiveCargoWiseNextPriceHeader.L6_DiscountCode;
			var stlDiscount2 = secondActiveCargoWiseNextPriceHeader.StlDiscounts.AddNew();
			stlDiscount2.PHD_Name = "C";
			stlDiscount2.PHD_Version = secondActiveCargoWiseNextPriceHeader.L6_DiscountCode;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			genericPriceHeader = newFactory.Load<ClientLicencePriceHeader>(genericPriceHeader.PK);
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			secondActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(secondActiveCargoWiseNextPriceHeader.PK);
			TestDateAttribute.AddSeconds(1);

			genericStlDiscount = genericPriceHeader.StlDiscounts[0];
			stlDiscount1 = firstActiveCargoWiseNextPriceHeader.StlDiscounts[0];
			stlDiscount2 = secondActiveCargoWiseNextPriceHeader.StlDiscounts[0];
			AssertNotEquals("Precondition", BillingConstants.DiscountCalculator.Volume, genericStlDiscount.PHD_Type);
			AssertNotEquals("Precondition", BillingConstants.DiscountCalculator.Volume, stlDiscount1.PHD_Type);
			AssertNotEquals("Precondition", BillingConstants.DiscountCalculator.Volume, stlDiscount2.PHD_Type);
			AssertEquals("Precondition", false, genericPriceHeader.HasChanges);
			AssertEquals("Precondition", false, firstActiveCargoWiseNextPriceHeader.HasChanges);
			AssertEquals("Precondition", false, secondActiveCargoWiseNextPriceHeader.HasChanges);
			genericStlDiscount.PHD_Type = stlDiscount1.PHD_Type = stlDiscount2.PHD_Type = BillingConstants.DiscountCalculator.Volume;
			genericPriceHeader.RunPreSaveValidation();
			firstActiveCargoWiseNextPriceHeader.RunPreSaveValidation();
			secondActiveCargoWiseNextPriceHeader.RunPreSaveValidation();

			AssertEquals("Should not have row errors if non CargoWise Next", false, genericPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", true, firstActiveCargoWiseNextPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", 1, firstActiveCargoWiseNextPriceHeader.RowErrors.Count());
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", "Price list cannot be edited past its Valid From date.", firstActiveCargoWiseNextPriceHeader.RowErrors.First().Message);
			AssertEquals("Should not have row errors if before valid from date", false, secondActiveCargoWiseNextPriceHeader.HasRowErrors);
		}

		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCargoWiseNextSystemCode_StlItemDiscountHasChangesPastValidFromInWorldsEarliestTimezone_ShouldThrowRowErrorBeforeSaving()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var secondActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			genericPriceHeader.L6_UseStandardDiscount = firstActiveCargoWiseNextPriceHeader.L6_UseStandardDiscount = secondActiveCargoWiseNextPriceHeader.L6_UseStandardDiscount = false;
			genericPriceHeader.L6_DiscountCode = "V1";
			firstActiveCargoWiseNextPriceHeader.L6_DiscountCode = "V2";
			secondActiveCargoWiseNextPriceHeader.L6_DiscountCode = "V3";

			genericPriceHeader.L6_ValidFrom = new ZDateTime(2024, 05, 01);
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 07, 01);

			var genericStlDiscount = genericPriceHeader.StlDiscounts.AddNew();
			genericStlDiscount.PHD_Name = "A";
			genericStlDiscount.PHD_Version = genericPriceHeader.L6_DiscountCode;
			var stlDiscount1 = firstActiveCargoWiseNextPriceHeader.StlDiscounts.AddNew();
			stlDiscount1.PHD_Name = "B";
			stlDiscount1.PHD_Version = firstActiveCargoWiseNextPriceHeader.L6_DiscountCode;
			var stlDiscount2 = secondActiveCargoWiseNextPriceHeader.StlDiscounts.AddNew();
			stlDiscount2.PHD_Name = "C";
			stlDiscount2.PHD_Version = secondActiveCargoWiseNextPriceHeader.L6_DiscountCode;
			var genericStlItemDiscount = genericPriceHeader.StlItemDiscounts.AddNew();
			genericStlItemDiscount.PGM_PHD = genericStlDiscount.PK;
			genericStlItemDiscount.PGM_GroupCode = "G1";
			var stlItemDiscount1 = firstActiveCargoWiseNextPriceHeader.StlItemDiscounts.AddNew();
			stlItemDiscount1.PGM_PHD = stlDiscount1.PK;
			stlItemDiscount1.PGM_GroupCode = "G1";
			var stlItemDiscount2 = secondActiveCargoWiseNextPriceHeader.StlItemDiscounts.AddNew();
			stlItemDiscount2.PGM_PHD = stlDiscount2.PK;
			stlItemDiscount2.PGM_GroupCode = "G1";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			genericPriceHeader = newFactory.Load<ClientLicencePriceHeader>(genericPriceHeader.PK);
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			secondActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(secondActiveCargoWiseNextPriceHeader.PK);
			TestDateAttribute.AddSeconds(1);

			genericStlItemDiscount = genericPriceHeader.StlItemDiscounts[0];
			stlItemDiscount1 = firstActiveCargoWiseNextPriceHeader.StlItemDiscounts[0];
			stlItemDiscount2 = secondActiveCargoWiseNextPriceHeader.StlItemDiscounts[0];
			AssertNotEquals("Precondition", "G3", genericStlItemDiscount.PGM_GroupCode);
			AssertNotEquals("Precondition", "G3", stlItemDiscount1.PGM_GroupCode);
			AssertNotEquals("Precondition", "G3", stlItemDiscount2.PGM_GroupCode);
			AssertEquals("Precondition", false, genericPriceHeader.HasChanges);
			AssertEquals("Precondition", false, firstActiveCargoWiseNextPriceHeader.HasChanges);
			AssertEquals("Precondition", false, secondActiveCargoWiseNextPriceHeader.HasChanges);
			genericStlItemDiscount.PGM_GroupCode = stlItemDiscount1.PGM_GroupCode = stlItemDiscount2.PGM_GroupCode = "G3";
			genericPriceHeader.RunPreSaveValidation();
			firstActiveCargoWiseNextPriceHeader.RunPreSaveValidation();
			secondActiveCargoWiseNextPriceHeader.RunPreSaveValidation();

			AssertEquals("Should not have row errors if non CargoWise Next", false, genericPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", true, firstActiveCargoWiseNextPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", 1, firstActiveCargoWiseNextPriceHeader.RowErrors.Count());
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", "Price list cannot be edited past its Valid From date.", firstActiveCargoWiseNextPriceHeader.RowErrors.First().Message);
			AssertEquals("Should not have row errors if before valid from date", false, secondActiveCargoWiseNextPriceHeader.HasRowErrors);
		}

		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCargoWiseNextSystemCode_UsageMappingHasChangesPastValidFromInWorldsEarliestTimezone_ShouldThrowRowErrorBeforeSaving()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var secondActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;

			genericPriceHeader.L6_ValidFrom = new ZDateTime(2024, 05, 01);
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 07, 01);

			genericPriceHeader.UsageMaps.AddNew();
			firstActiveCargoWiseNextPriceHeader.UsageMaps.AddNew();
			secondActiveCargoWiseNextPriceHeader.UsageMaps.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			genericPriceHeader = newFactory.Load<ClientLicencePriceHeader>(genericPriceHeader.PK);
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			secondActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(secondActiveCargoWiseNextPriceHeader.PK);
			TestDateAttribute.AddSeconds(1);

			var genericUsageMapping = genericPriceHeader.UsageMaps[0];
			var usageMapping1 = firstActiveCargoWiseNextPriceHeader.UsageMaps[0];
			var usageMapping2 = secondActiveCargoWiseNextPriceHeader.UsageMaps[0];
			AssertNotEquals("Precondition", "SHP", genericUsageMapping.PUM_PriceCode);
			AssertNotEquals("Precondition", "SHP", usageMapping1.PUM_PriceCode);
			AssertNotEquals("Precondition", "SHP", usageMapping2.PUM_PriceCode);
			AssertEquals("Precondition", false, genericPriceHeader.HasChanges);
			AssertEquals("Precondition", false, firstActiveCargoWiseNextPriceHeader.HasChanges);
			AssertEquals("Precondition", false, secondActiveCargoWiseNextPriceHeader.HasChanges);
			genericUsageMapping.PUM_PriceCode = usageMapping1.PUM_PriceCode = usageMapping2.PUM_PriceCode = "SHP";
			genericPriceHeader.RunPreSaveValidation();
			firstActiveCargoWiseNextPriceHeader.RunPreSaveValidation();
			secondActiveCargoWiseNextPriceHeader.RunPreSaveValidation();

			AssertEquals("Should not have row errors if non CargoWise Next", false, genericPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", true, firstActiveCargoWiseNextPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", 1, firstActiveCargoWiseNextPriceHeader.RowErrors.Count());
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", "Price list cannot be edited past its Valid From date.", firstActiveCargoWiseNextPriceHeader.RowErrors.First().Message);
			AssertEquals("Should not have row errors if before valid from date", false, secondActiveCargoWiseNextPriceHeader.HasRowErrors);
		}

		[TestDate(2024, 05, 31, 09, 59, 59)]
		public void TestCargoWiseNextSystemCode_ExchangeRateHasChangesPastValidFromInWorldsEarliestTimezone_ShouldThrowRowErrorBeforeSaving()
		{
			var globalPriceLists = new CodeDescriptionPairList();
			globalPriceLists.AddPair(BillingConstants.PriceHeaderType.CargoWiseNext, "CargoWise Next");
			EDIDataRegistry.Instance.BillingStlGlobalPriceLists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, globalPriceLists);

			var genericPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var firstActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var secondActiveCargoWiseNextPriceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			genericPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			firstActiveCargoWiseNextPriceHeader.L6_SystemCode = secondActiveCargoWiseNextPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			genericPriceHeader.L6_HasExchangeRates = firstActiveCargoWiseNextPriceHeader.L6_HasExchangeRates = secondActiveCargoWiseNextPriceHeader.L6_HasExchangeRates = true;

			genericPriceHeader.L6_ValidFrom = new ZDateTime(2024, 05, 01);
			firstActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 06, 01);
			secondActiveCargoWiseNextPriceHeader.L6_ValidFrom = new ZDateTime(2024, 07, 01);

			genericPriceHeader.ExchangeRates.AddNew();
			firstActiveCargoWiseNextPriceHeader.ExchangeRates.AddNew();
			secondActiveCargoWiseNextPriceHeader.ExchangeRates.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			genericPriceHeader = newFactory.Load<ClientLicencePriceHeader>(genericPriceHeader.PK);
			firstActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(firstActiveCargoWiseNextPriceHeader.PK);
			secondActiveCargoWiseNextPriceHeader = newFactory.Load<ClientLicencePriceHeader>(secondActiveCargoWiseNextPriceHeader.PK);
			TestDateAttribute.AddSeconds(1);

			var genericExchangeRate = genericPriceHeader.ExchangeRates[0];
			var exchangeRate1 = firstActiveCargoWiseNextPriceHeader.ExchangeRates[0];
			var exchangeRate2 = secondActiveCargoWiseNextPriceHeader.ExchangeRates[0];
			AssertNotEquals("Precondition", 2m, genericExchangeRate.PHE_Rate);
			AssertNotEquals("Precondition", 2m, exchangeRate1.PHE_Rate);
			AssertNotEquals("Precondition", 2m, exchangeRate2.PHE_Rate);
			AssertEquals("Precondition", false, genericPriceHeader.HasChanges);
			AssertEquals("Precondition", false, firstActiveCargoWiseNextPriceHeader.HasChanges);
			AssertEquals("Precondition", false, secondActiveCargoWiseNextPriceHeader.HasChanges);
			genericExchangeRate.PHE_Rate = exchangeRate1.PHE_Rate = exchangeRate2.PHE_Rate = 2m;
			genericPriceHeader.RunPreSaveValidation();
			firstActiveCargoWiseNextPriceHeader.RunPreSaveValidation();
			secondActiveCargoWiseNextPriceHeader.RunPreSaveValidation();

			AssertEquals("Should not have row errors if non CargoWise Next", false, genericPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", true, firstActiveCargoWiseNextPriceHeader.HasRowErrors);
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", 1, firstActiveCargoWiseNextPriceHeader.RowErrors.Count());
			AssertEquals("Should have row error if child was edited and CargoWise Next and past valid from date", "Price list cannot be edited past its Valid From date.", firstActiveCargoWiseNextPriceHeader.RowErrors.First().Message);
			AssertEquals("Should not have row errors if before valid from date", false, secondActiveCargoWiseNextPriceHeader.HasRowErrors);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIOrgHeader testHeader = factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "XYZABC";
			testHeader.MainAddress.OA_Address1 = "Address";
			testHeader.CreateAndLoadLicenceForOrg();

			ClientLicencePriceHeader priceHeader = testHeader.LicCompany.PriceHeaders.AddNew();
			priceHeader.Items.AddNew();

			return priceHeader;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = (ClientLicencePriceHeader)base.GetNewBusinessObject();
			result.L6_UseStandardDiscount = false;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		#endregion
	}
}
