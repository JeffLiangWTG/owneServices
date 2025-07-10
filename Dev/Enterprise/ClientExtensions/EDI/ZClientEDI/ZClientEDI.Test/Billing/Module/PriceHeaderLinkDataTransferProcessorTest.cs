using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	public class PriceHeaderLinkDataTransferProcessorTest : TestCaseWithFactory
	{
		[TestDate(2016, 11, 2)]
		public void TestImport()
		{
			var stdCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var priceList1 = BillingTestHelper.CreateStlPriceList(stdCompany);
			var priceList2 = BillingTestHelper.CreateStlPriceList(stdCompany);
			var priceList3 = BillingTestHelper.CreateStlPriceList(stdCompany);
			priceList1.L6_PricelistVersion = "STL1";
			priceList2.L6_PricelistVersion = "STL2";
			priceList3.L6_PricelistVersion = "STL3";
			stdCompany.Factory.Save();
			var collection = new PriceHeaderLinkImportCollection(Factory);
			var collectionInfo = new PriceHeaderLinkImportInfo(collection);
			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "SOMEORG1";
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "EN5", "CO5", "DB5", false);
			var orgWithLicenceAndDb1 = licHeader1.Company.Header;
			var licHeader2 = BillingTestHelper.CreateLicence(Factory, "EN6", "CO6", "DB6", false);
			var orgWithLicenceAndDb2 = licHeader2.Company.Header;
			var licHeader3 = BillingTestHelper.CreateLicence(Factory, "EN7", "CO7", "DB7", false);
			var orgWithLicenceAndDb3 = licHeader3.Company.Header;
			var licHeader4 = BillingTestHelper.CreateLicence(Factory, "EN8", "CO8", "DB8", false);
			var orgWithLicenceAndDb4 = licHeader4.Company.Header;
			var existingLink1 = licHeader2.Database.PriceHeaderLinks.AddNew();
			existingLink1.PHL_L6 = priceList1.PK;
			existingLink1.PHL_RX_NKCurrency = "NZD";
			existingLink1.PHL_ValidFrom = new ZDateTime(2016, 9, 1);
			Factory.Save();
			var rec1 = CreateImportRecord(collection, orgWithLicenceAndDb1.OH_Code, licHeader1.Database.LD_ServerCode, "STL1", "AUD", new ZDateTime(2016, 11, 1));
			var rec2 = CreateImportRecord(collection, orgWithLicenceAndDb2.OH_Code, licHeader2.Database.LD_ServerCode, "STL2", "NZD", new ZDateTime(2016, 10, 1));
			var recWithDuplicateValidFrom = CreateImportRecord(collection, orgWithLicenceAndDb2.OH_Code, licHeader2.Database.LD_ServerCode, "STL2", "AUD", new ZDateTime(2016, 9, 1));
			var recWithInvalidDb = CreateImportRecord(collection, org1.OH_Code, "DB1", "STL1", "AUD", new ZDateTime(2016, 11, 1));
			var recWithInvalidCurrency = CreateImportRecord(collection, orgWithLicenceAndDb3.OH_Code, licHeader3.Database.LD_ServerCode, "STL1", "", new ZDateTime(2016, 11, 1));
			var recWithInvalidOrg = CreateImportRecord(collection, "NOEXISTS", "FOO", "STL1", "AUD", new ZDateTime(2016, 11, 1));
			var recWithoutValidFrom = CreateImportRecord(collection, orgWithLicenceAndDb4.OH_Code, licHeader4.Database.LD_ServerCode, "STL1", "AUD", ZDateTime.Empty);
			var recWithInvalidVolume = CreateImportRecord(collection, orgWithLicenceAndDb4.OH_Code, licHeader4.Database.LD_ServerCode, "STL1", "AUD", new ZDateTime(2016, 11, 1));
			recWithInvalidVolume.Volume = "BAD";
			var recWithInvalidCore = CreateImportRecord(collection, orgWithLicenceAndDb4.OH_Code, licHeader4.Database.LD_ServerCode, "STL1", "AUD", new ZDateTime(2016, 11, 1));
			recWithInvalidCore.CorePack = "BAD";
			var recWithInvalidCorePercent = CreateImportRecord(collection, orgWithLicenceAndDb4.OH_Code, licHeader4.Database.LD_ServerCode, "STL1", "AUD", new ZDateTime(2016, 11, 1));
			recWithInvalidCorePercent.CoreUpliftPercent = 10000;
			var rec1Duplicate = CreateImportRecord(collection, orgWithLicenceAndDb1.OH_Code, licHeader1.Database.LD_ServerCode, "STL1", "AUD", new ZDateTime(2016, 11, 1));
			var linkCollection = new EdiPriceHeaderLinkCollection(Factory, new AdhocCollectionRelationship(typeof(EdiPriceHeaderLink)));
			var processor = new PriceHeaderLinkDataTransferProcessor(linkCollection, collectionInfo);
			processor.Import();
			IEnumerable<EdiPriceHeaderLink> linkList = linkCollection.Cast<EdiPriceHeaderLink>();
			AssertEquals("EdiPriceHeaderLink count", 2, linkCollection.Count);
			var link1 = linkList.FirstOrDefault(x => x.PHL_LD == licHeader1.LA_LD);
			var link2 = linkList.FirstOrDefault(x => x.PHL_LD == licHeader2.LA_LD);
			link1.Validation.ValidateAll();
			link2.Validation.ValidateAll();
			AssertNoErrors(link1);
			AssertNoErrors(link2);
			AssertEquals(@"Record [Org. Code: EN6CO6, Server Code: DB6, Price List version: STL2, Currency: AUD] excluded: Price List with same Valid From already exists
Record [Org. Code: SOMEORG1, Server Code: DB1, Price List version: STL1, Currency: AUD] excluded: Server Code not found
Record [Org. Code: EN7CO7, Server Code: DB7, Price List version: STL1, Currency: ] excluded: Currency not found
Record [Org. Code: NOEXISTS, Server Code: FOO, Price List version: STL1, Currency: AUD] excluded: Org. Code not found
Record [Org. Code: EN8CO8, Server Code: DB8, Price List version: STL1, Currency: AUD] excluded: Valid From is missing or out of range
Record [Org. Code: EN8CO8, Server Code: DB8, Price List version: STL1, Currency: AUD] excluded: Error - PHL_VolumeCode: Enter a valid selection.
Record [Org. Code: EN8CO8, Server Code: DB8, Price List version: STL1, Currency: AUD] excluded: Error - PHL_CorePackCode: Enter a valid selection.
Record [Org. Code: EN8CO8, Server Code: DB8, Price List version: STL1, Currency: AUD] excluded: Error - PHL_CoreUpliftPercent: The number 10,000 is too large, the maximum value allowed for selection is 999.99.
Record [Org. Code: EN5CO5, Server Code: DB5, Price List version: STL1, Currency: AUD] excluded: Price List with same Valid From already exists
", processor.Log);
		}

		PriceHeaderLinkImport CreateImportRecord(PriceHeaderLinkImportCollection collection, string orgCode, string serverCode, string priceListVersion, string currency, ZDateTime validFrom)
		{
			var result = collection.AddNew();
			result.OrgCode = orgCode;
			result.ServerCode = serverCode;
			result.PricelistVersion = priceListVersion;
			result.Currency = currency;
			result.ValidFrom = validFrom;
			result.CorePack = EdiPriceHeaderLinkCorePackCodeList.Codes.INC;
			result.CoreUpliftPercent = 0;
			result.Volume = EdiPriceHeaderLinkVolumeCodeList.Codes.STD;
			result.VolumePercent = 100m;
			return result;
		}
	}
}
