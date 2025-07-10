using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	public class PriceHeaderDataTransferProcessorTest : TestCaseWithFactory
	{
		[TestDate(2016, 11, 2)]
		public void TestImport()
		{
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var priceList1 = BillingTestHelper.CreatePriceHeader(stdCompany, BillingConstants.PriceHeaderType.ODM, "ODM1", "AUD", new ZDateTime(2016, 1, 1), false);
			var priceList2 = BillingTestHelper.CreatePriceHeader(stdCompany, BillingConstants.PriceHeaderType.ODM, "ODM2", "USD", new ZDateTime(2016, 2, 1), false);
			var priceList3 = BillingTestHelper.CreatePriceHeader(stdCompany, BillingConstants.PriceHeaderType.ODM, "ODM3", "AUD", new ZDateTime(2016, 1, 1), false);
			stdCompany.Factory.Save();
			var collection = new PriceHeaderImportCollection(Factory);
			var collectionInfo = new PriceHeaderImportInfo(collection);
			var orgWithoutLicence = Factory.New<EDIOrgHeader>();
			orgWithoutLicence.OH_Code = "SOMEORG1";
			var licCompany1 = BillingTestHelper.CreateLicencedOrganization(Factory, "EN1", "CO1");
			var licCompany2 = BillingTestHelper.CreateLicencedOrganization(Factory, "EN2", "CO2");
			var licCompany3 = BillingTestHelper.CreateLicencedOrganization(Factory, "EN3", "CO3");
			var licCompany4 = BillingTestHelper.CreateLicencedOrganization(Factory, "EN4", "CO4");
			var licCompany5 = BillingTestHelper.CreateLicencedOrganization(Factory, "EN5", "CO5");
			var licCompany6 = BillingTestHelper.CreateLicencedOrganization(Factory, "EN6", "CO6");
			var licCompany7 = BillingTestHelper.CreateLicencedOrganization(Factory, "EN7", "CO7");
			var priceHeader1 = licCompany7.PriceHeaders.AddNew();
			priceHeader1.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			priceHeader1.L6_RX_NKCurrency = "AUD";
			priceHeader1.L6_IsStandard = true;
			priceHeader1.L6_LicenceUnitRate = 1;
			priceHeader1.L6_ValidFrom = new ZDateTime(2016, 10, 1);
			Factory.Save();
			var rec1 = CreateImportRecord(collection, licCompany1.Header.OH_Code, "ODM1", "AUD", new ZDateTime(2016, 1, 1));
			var rec2 = CreateImportRecord(collection, licCompany2.Header.OH_Code, "ODM2", "USD", new ZDateTime(2016, 2, 1));
			rec2.UseStdDiscount = false;
			var recWithInvalidCurrency = CreateImportRecord(collection, licCompany3.Header.OH_Code, "ODM1", "", new ZDateTime(2016, 11, 1));
			var recWithInvalidOrg = CreateImportRecord(collection, "NOEXISTS", "ODM1", "AUD", new ZDateTime(2016, 11, 1));
			var recWithoutValidFrom = CreateImportRecord(collection, licCompany4.Header.OH_Code, "ODM1", "AUD", ZDateTime.Empty);
			var recWithInvalidPricelist = CreateImportRecord(collection, licCompany5.Header.OH_Code, "FOO", "AUD", new ZDateTime(2016, 10, 1));
			var recWithCurrencyWithoutPrice = CreateImportRecord(collection, licCompany6.Header.OH_Code, "ODM1", "NZD", new ZDateTime(2016, 10, 1));
			var recWithDuplicateValidFrom = CreateImportRecord(collection, licCompany7.Header.OH_Code, "ODM1", "AUD", new ZDateTime(2016, 10, 1));
			var rec1Duplicate = CreateImportRecord(collection, licCompany1.Header.OH_Code, "ODM1", "AUD", new ZDateTime(2016, 1, 1));
			var recWithDuplicateSettings = CreateImportRecord(collection, licCompany1.Header.OH_Code, "ODM1", "AUD", new ZDateTime(2016, 12, 1));
			var headers = ClientLicencePriceHeaderCollection.CreateAdhocCollection(Factory);
			var processor = new PriceHeaderDataTransferProcessor(headers, collectionInfo);
			processor.Import();
			AssertEquals("Imported count", 2, headers.Count);
			var header1 = headers.FirstOrDefault(x => x.L6_LC == licCompany1.PK);
			var header2 = headers.FirstOrDefault(x => x.L6_LC == licCompany2.PK);
			header1.L6_TestDbPriceCode = ZString.Empty;
			header2.L6_TestDbPriceCode = ZString.Empty;
			header1.Validation.ValidateAll();
			header2.Validation.ValidateAll();
			AssertNoErrors(header1);
			AssertNoErrors(header2);
			AssertEquals(true, header1.L6_IsStandard);
			AssertEquals(BillingConstants.LicenceEdition.Country, header1.L6_LicenceEdition);
			AssertEquals("ODM1", header1.L6_PricelistVersion);
			AssertEquals("AUD", header1.L6_RX_NKCurrency);
			AssertEquals(BillingConstants.PriceHeaderType.ODM, header1.L6_SystemCode);
			AssertEquals(true, header1.L6_UseStandardDiscount);
			AssertEquals(new ZDateTime(2016, 1, 1), header1.L6_ValidFrom);
			AssertEquals(ZDateTime.Empty, header1.L6_ValidTo);
			AssertEquals(true, header2.L6_IsStandard);
			AssertEquals(BillingConstants.LicenceEdition.Country, header2.L6_LicenceEdition);
			AssertEquals("ODM2", header2.L6_PricelistVersion);
			AssertEquals("USD", header2.L6_RX_NKCurrency);
			AssertEquals(BillingConstants.PriceHeaderType.ODM, header2.L6_SystemCode);
			AssertEquals(false, header2.L6_UseStandardDiscount);
			AssertEquals(new ZDateTime(2016, 2, 1), header2.L6_ValidFrom);
			AssertEquals(ZDateTime.Empty, header2.L6_ValidTo);
			AssertEquals("Record [Org. Code: EN3CO3, Price List version: ODM1, Currency: ] excluded: Currency not found\r\n" + "Record [Org. Code: NOEXISTS, Price List version: ODM1, Currency: AUD] excluded: Org. Code not found\r\n" + "Record [Org. Code: EN4CO4, Price List version: ODM1, Currency: AUD] excluded: Valid From is missing or out of range\r\n" + "Record [Org. Code: EN5CO5, Price List version: FOO, Currency: AUD] excluded: Matching version and currency not found\r\n" + "Record [Org. Code: EN6CO6, Price List version: ODM1, Currency: NZD] excluded: Matching version and currency not found\r\n" + "Record [Org. Code: EN7CO7, Price List version: ODM1, Currency: AUD] excluded: Price List with same settings or Valid From already exists\r\n" + "Record [Org. Code: EN1CO1, Price List version: ODM1, Currency: AUD] excluded: Price List with same settings or Valid From already exists\r\n" + "Record [Org. Code: EN1CO1, Price List version: ODM1, Currency: AUD] excluded: Price List with same settings or Valid From already exists\r\n", processor.Log);
		}

		PriceHeaderImport CreateImportRecord(PriceHeaderImportCollection collection, string orgCode, string priceListVersion, string currency, ZDateTime validFrom)
		{
			var result = collection.AddNew();
			result.OrgCode = orgCode;
			result.PricelistVersion = priceListVersion;
			result.Currency = currency;
			result.ValidFrom = validFrom;
			return result;
		}
	}
}
