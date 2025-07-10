using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicencePriceHeaderCollection))]
	public class ClientLicencePriceHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientLicencePriceHeaderCollection>
	{
		public void TestCollectionHasDuplicates()
		{
			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.OH_Code = "XYZABC";
			testHeader.MainAddress.OA_Address1 = "Address";
			testHeader.CreateAndLoadLicenceForOrg();
			var priceHeader = testHeader.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_RN_NKCountry = "AU";
			priceHeader.L6_LicenceEdition = "EXP";
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_PricelistVersion = "AAA";
			priceHeader.L6_ValidFrom = ZDateTime.Now;
			var priceHeader2 = testHeader.LicCompany.PriceHeaders.AddNew();
			priceHeader2.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader2.L6_RN_NKCountry = "AU";
			priceHeader2.L6_LicenceEdition = "EXP";
			priceHeader2.L6_RX_NKCurrency = "AUD";
			priceHeader2.L6_PricelistVersion = "AAA";
			priceHeader2.L6_ValidFrom = ZDateTime.Now;

			testHeader.RunPreSaveValidation();
			AssertEquals("Count", 2, testHeader.LicCompany.PriceHeaders.Count);
			AssertEquals("Duplicate Price List already exists", true, testHeader.LicCompany.PriceHeaders[0].HasRowErrors);
			AssertHasRowError("Duplicate Price List Exists", testHeader.LicCompany.PriceHeaders[0], "Duplicate Price List already exists");
			AssertHasRowError("Duplicate Price List Exists", testHeader.LicCompany.PriceHeaders[1], "Duplicate Price List already exists");
		}

		public void TestValidateStandardPricesExist()
		{
			LicenceCompany company = Factory.NewWithValidTestData<LicenceCompany>();
			ClientLicencePriceHeader prices = company.PriceHeaders.AddNew();
			prices.L6_PricelistVersion = "bad";
			prices.L6_LicenceEdition = BillingConstants.LicenceEdition.Country;
			prices.L6_RN_NKCountry = "AU";
			prices.L6_RX_NKCurrency = "AUD";
			prices.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			prices.L6_ValidFrom = ZDateTime.Now;
			prices.L6_TestDbPriceCode = ZString.Empty;

			company.RunPreSaveValidation();
			AssertNoErrors("non-std prices list doesn't check for standard company", prices);

			prices.L6_IsStandard = true;
			company.RunPreSaveValidation();
			AssertHasRowError(prices, "Standard prices company not found");

			var stdCompany = CreateAndSetStandardPricesCompany(Factory);
			ClientLicencePriceHeaderLookupsTest.AddPriceHeader(stdCompany, "V100");
			ClientLicencePriceHeaderLookupsTest.AddPriceHeader(stdCompany, "V300");
			ClientLicencePriceHeaderLookupsTest.AddPriceHeader(stdCompany, "V200");
			stdCompany.Factory.Save();

			var expected = new CodeDescriptionPairList();
			expected.AddPair("V100");
			expected.AddPair("V200");
			expected.AddPair("V300");

			company.RunPreSaveValidation();
			AssertHasRowError(prices, "Standard prices for these settings not found. Please have a pricelist matching to " + LicenceCompany.StandardPricesCompany.Header.OH_Code);

			prices.L6_PricelistVersion = "V100";
			prices.L6_TestDbPriceCode = ZString.Empty;
			company.RunPreSaveValidation();
			AssertNoErrors(prices);
		}

		public void TestRelationshipDefaultsForNewElement()
		{
			ClientLicencePriceHeader item = Collection.AddNew();
			AssertEquals("Master", Master.PK, item.L6_LC);
		}

		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)Collection).AllowNew);
		}

		public void TestCreateAdhocCollection()
		{
			var collection = ClientLicencePriceHeaderCollection.CreateAdhocCollection(Factory);
			var item1 = Factory.New<ClientLicencePriceHeader>();
			var item2 = Factory.New<ClientLicencePriceHeader>();
			collection.Add(item1);
			collection.Add(item2);
			AssertEquals(2, collection.Count);
		}

		#region Implementation

		LicenceCompany Master;

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

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ClientLicencePriceHeaderCollection);
		}

		protected override ClientLicencePriceHeaderCollection GetCollectionToTest()
		{
			Master = Factory.NewWithValidTestData<LicenceCompany>();
			return new ClientLicencePriceHeaderCollection(Master);
		}

		public static LicenceHeader CreateStandardPricesHeader(BusinessObjectFactory factory)
		{
			EDIOrgHeader org = factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var db = org.LicCompany.LicDatabases.AddNew();
			db.LD_ServerCode = "PRD";
			LicenceCompany stdCompany = org.LicCompany;
			stdCompany.LicEnterprise.LE_EnterpriseCode = "DDD";
			LicenceHeader header = org.LicCompany.GetHeader(db);
			return header;
		}

		public static LicenceCompany CreateAndSetStandardPricesCompany(BusinessObjectFactory factory)
		{
			LicenceHeader header = CreateStandardPricesHeader(factory);
			factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(header);
			return header.Company;
		}

		public static void SetStandardPriceCompanyLicence(LicenceHeader stdHeader)
		{
			AssertEquals("LicenceCode.Length", 9, stdHeader.LicenceCode.Length);
			EDIDataRegistry.Instance.StandardPriceCompanyLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, stdHeader.LicenceCode);
		}

		#endregion
	}
}
