using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Module.Testing
{
	public class EdiLicenceSettingFlattenedDataTransferProcessorTest : TestCaseWithFactory
	{
		public void TestImport_InvalidInput()
		{
			var flattenedCollection = new EdiLicenceSettingFlattenedCollection(Factory);
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseID = "XX1";
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = "XX2";
			var rec3 = flattenedCollection.AddNew();
			rec3.OrgCode = "XX3";
			var rec4 = flattenedCollection.AddNew();
			rec4.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec4.ServerCode = "XX4";
			AssertImport(flattenedCollection, @"01-Jan-17 00:00:00 - 31-Jan-17 00:00:00 - BWL - 40.0000 - Y - AUS - 0.00 - BW40
01-Jan-17 00:00:00 - 31-Jan-17 00:00:00 - DIS - 0.0000 - Y - VOLUME - 20.00 - VOL20
01-Jan-17 00:00:00 - 31-Jan-17 00:00:00 - PRI - 30.0000 - Y - STL-#HL - 0.00 - #HL30", @"Record [Ent. ID: XX1, Ent. Code: , Org. Code: , Server Code: ] excluded: Ent. ID not found
Record [Ent. ID: , Ent. Code: XX2, Org. Code: , Server Code: ] excluded: Ent. Code not found
Record [Ent. ID: , Ent. Code: , Org. Code: XX3, Server Code: ] excluded: Org. Code not found
Record [Ent. ID: , Ent. Code: OG1, Org. Code: , Server Code: XX4] excluded: Database not found
");
		}

		public void TestImport_NewRecords()
		{
			var flattenedCollection = new EdiLicenceSettingFlattenedCollection(Factory);
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec1.ServerCode = Db1.LD_ServerCode;
			rec1.ValidFrom = new ZDateTime(2018, 1, 1);
			rec1.DiscountName = "WISECLOUD";
			rec1.DiscountPercentAsText = "5";
			rec1.DiscountActiveAsText = "Y";
			rec1.Comment = "WC5";
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec2.OrgCode = Org1.OH_Code;
			rec2.ServerCode = Db1.LD_ServerCode;
			rec2.ValidFrom = new ZDateTime(2018, 1, 1);
			rec2.PriceCategory = "STL";
			rec2.PriceCode = "#MF";
			rec2.PriceAsText = "10";
			rec2.Comment = "#MF10";
			var rec3 = flattenedCollection.AddNew();
			rec3.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec3.ServerCode = Db1.LD_ServerCode;
			rec3.ValidFrom = new ZDateTime(2018, 1, 1);
			rec3.BWPurchasedLicencesAsText = "20";
			rec3.Comment = "BW20";
			rec3.PriceCategory = Setting3.PriceKey.Category;
			rec3.PriceCode = Setting3.PriceKey.Code;
			AssertImport(flattenedCollection, @"01-Jan-17 00:00:00 - 31-Jan-17 00:00:00 - BWL - 40.0000 - Y - AUS - 0.00 - BW40
01-Jan-17 00:00:00 - 31-Jan-17 00:00:00 - DIS - 0.0000 - Y - VOLUME - 20.00 - VOL20
01-Jan-17 00:00:00 - 31-Jan-17 00:00:00 - PRI - 30.0000 - Y - STL-#HL - 0.00 - #HL30
01-Jan-18 00:00:00 -  - BWL - 20.0000 - Y - AUS - 0.00 - BW20
01-Jan-18 00:00:00 -  - DIS - 0.0000 - Y - WISECLOUD - 5.00 - WC5
01-Jan-18 00:00:00 -  - PRI - 10.0000 - Y - STL-#MF - 0.00 - #MF10", null);
		}

		public void TestImport_MergeRecords()
		{
			var flattenedCollection = new EdiLicenceSettingFlattenedCollection(Factory);
			//has a Valid To within one day of the other Valid From.
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec1.ServerCode = Db1.LD_ServerCode;
			rec1.ValidFrom = new ZDateTime(2017, 2, 1);
			rec1.DiscountName = "VOLUME";
			rec1.DiscountPercentAsText = "20";
			rec1.DiscountActiveAsText = "Y";
			rec1.Comment = "VOL20 Merged";
			//has a Valid To that is empty, and the other has Valid To empty or after the other Valid From
			Setting2.LS9_ValidTo = ZDateTime.Empty;
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec2.OrgCode = Org1.OH_Code;
			rec2.ServerCode = Db1.LD_ServerCode;
			rec2.ValidFrom = new ZDateTime(2015, 1, 1);
			rec2.PriceCategory = "STL";
			rec2.PriceCode = "#HL";
			rec2.PriceAsText = "30";
			rec2.Comment = "#HL30 Merged";
			Setting3.LS9_ValidTo = ZDateTime.Empty;
			var rec3 = flattenedCollection.AddNew();
			rec3.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec3.ServerCode = Db1.LD_ServerCode;
			rec3.ValidFrom = new ZDateTime(2014, 1, 1);
			rec3.ValidTo = new ZDateTime(2019, 1, 31);
			rec3.BWPurchasedLicencesAsText = "40";
			rec3.PriceCode = Setting3.LS9_Name;
			rec3.Comment = "BW40 Merged";
			Factory.Save();
			AssertImport(flattenedCollection, @"01-Jan-14 00:00:00 -  - BWL - 40.0000 - Y - AUS - 0.00 - BW40 Merged
01-Jan-15 00:00:00 -  - PRI - 30.0000 - Y - STL-#HL - 0.00 - #HL30 Merged
01-Jan-17 00:00:00 -  - DIS - 0.0000 - Y - VOLUME - 20.00 - VOL20 Merged", null);
		}

		public void TestImport_Validation()
		{
			var flattenedCollection = new EdiLicenceSettingFlattenedCollection(Factory);
			var rec1 = flattenedCollection.AddNew();
			rec1.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec1.ServerCode = Db1.LD_ServerCode;
			rec1.ValidFrom = new ZDateTime(2017, 1, 1);
			rec1.ValidTo = new ZDateTime(2017, 1, 31);
			rec1.DiscountName = "VOLUME";
			rec1.DiscountPercentAsText = "25";
			rec1.DiscountActiveAsText = "Y";
			var rec2 = flattenedCollection.AddNew();
			rec2.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec2.OrgCode = Org1.OH_Code;
			rec2.ServerCode = Db1.LD_ServerCode;
			rec2.ValidFrom = new ZDateTime(2018, 1, 1);
			rec2.PriceCategory = "STL";
			rec2.PriceCode = "@#!";
			rec2.PriceAsText = "50";
			var rec3 = flattenedCollection.AddNew();
			rec3.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec3.ServerCode = Db1.LD_ServerCode;
			rec3.ValidFrom = new ZDateTime(2018, 1, 1);
			rec3.BWPurchasedLicencesAsText = "AAC";
			var rec4 = flattenedCollection.AddNew();
			rec4.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec4.ServerCode = Db1.LD_ServerCode;
			rec4.ValidFrom = new ZDateTime(2018, 1, 1);
			rec4.PriceCode = "USR";
			rec4.PriceAsText = "50";
			var rec5 = flattenedCollection.AddNew();
			rec5.EnterpriseCode = Org1.LicEnterprise.LE_EnterpriseCode;
			rec5.ServerCode = Db1.LD_ServerCode;
			rec5.ValidFrom = new ZDateTime(2018, 1, 1);
			rec5.PriceCategory = "ZZZ";
			rec5.PriceCode = "USR";
			rec5.PriceAsText = "50";
			AssertImport(flattenedCollection, @"01-Jan-17 00:00:00 - 31-Jan-17 00:00:00 - BWL - 40.0000 - Y - AUS - 0.00 - BW40
01-Jan-17 00:00:00 - 31-Jan-17 00:00:00 - DIS - 0.0000 - Y - VOLUME - 20.00 - VOL20
01-Jan-17 00:00:00 - 31-Jan-17 00:00:00 - PRI - 30.0000 - Y - STL-#HL - 0.00 - #HL30", @"Record [Ent. ID: , Ent. Code: OG1, Org. Code: , Server Code: OG1] excluded: Another Discount setting with same values exists
Record [Ent. ID: , Ent. Code: OG1, Org. Code: OG1SYD, Server Code: OG1] excluded: Error - LS9_Name: Enter a valid Price Code
Record [Ent. ID: , Ent. Code: OG1, Org. Code: , Server Code: OG1] excluded: Error - BWPurchasedLicences: AAC is not a valid number
Record [Ent. ID: , Ent. Code: OG1, Org. Code: , Server Code: OG1] excluded: Please enter Price Category
Record [Ent. ID: , Ent. Code: OG1, Org. Code: , Server Code: OG1] excluded: Category ZZZ is not defined in registry " + EDIDataRegistry.Instance.BillingUsageCategoryCodes.Caption + @"
");
		}

		protected override void SetUp()
		{
			base.SetUp();
			Org1 = BillingTestHelper.CreateOrganisation(Factory, "OG1");
			Db1 = Org1.LicEnterprise.Databases.OfType<LicenceDatabase>().First();
			var priceList1 = BillingTestHelper.CreatePriceHeader(Org1.LicCompany, "STL", "STL v1", "AUD", new ZDateTime(2010, 1, 1), false);
			var price1 = BillingTestHelper.AddPriceItem(priceList1, "#HL", "TRA", "", 100m);
			var price2 = BillingTestHelper.AddPriceItem(priceList1, "#MF", "TRA", "", 200m);
			var link1 = BillingTestHelper.CreatePriceLink(Db1, priceList1, new ZDateTime(2010, 1, 1));
			link1.PHL_ValidFrom = new ZDateTime(2010, 1, 1);
			var settings = Db1.LicenceSettings;
			EdiLicenceSetting newSetting = Factory.New<DiscountLicenceSetting>();
			newSetting.LS9_Name = "VOLUME";
			newSetting.LS9_Percent = 20;
			newSetting.LS9_IsActive = true;
			newSetting.LS9_ValidFrom = new ZDateTime(2017, 1, 1);
			newSetting.LS9_ValidTo = new ZDateTime(2017, 1, 31);
			newSetting.LS9_Comment = "VOL20";
			settings.Add(newSetting);
			newSetting = Factory.New<PriceLicenceSetting>();
			newSetting.PriceKey = price1.CodeKey;
			newSetting.LS9_Price = 30;
			newSetting.LS9_ValidFrom = new ZDateTime(2017, 1, 1);
			newSetting.LS9_ValidTo = new ZDateTime(2017, 1, 31);
			newSetting.LS9_Comment = "#HL30";
			settings.Add(newSetting);
			Setting2 = newSetting;
			var bwSetting = Factory.New<BorderWisePurchasedLicenceSetting>();
			bwSetting.LicenceCount = 40;
			bwSetting.LS9_ValidFrom = new ZDateTime(2017, 1, 1);
			bwSetting.LS9_ValidTo = new ZDateTime(2017, 1, 31);
			bwSetting.LS9_Comment = "BW40";
			bwSetting.PriceCode = BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode;
			settings.Add(bwSetting);
			Setting3 = bwSetting;
			Factory.Save();
		}

		void AssertImport(EdiLicenceSettingFlattenedCollection flattenedCollection, string rowsAsTextExpected, string logExpected)
		{
			var collectionInfo = new EdiLicenceSettingImportInfo(flattenedCollection);
			var collection = new EdiLicenceSettingCollectionNonDependent(Factory);
			var processor = new EdiLicenceSettingFlattenedDataTransferProcessor(collection, collectionInfo);
			processor.Import();
			Factory.Save();
			var rowsAsText = string.Join("\r\n", new BusinessObjectFactory().Load<EdiLicenceSetting>(new ZQuery()).Select(x => $"{x.LS9_ValidFrom} - {x.LS9_ValidTo} - {x.LS9_Type} - {x.LS9_Price} - {x.LS9_IsActive} - {x.LS9_Name} - {x.LS9_Percent} - {x.LS9_Comment}").OrderBy(x => x));
			CombineAssertions(() =>
			{
				AssertEquals("rowsAsText", rowsAsTextExpected, rowsAsText);
				AssertEquals("log", logExpected, processor.Log);
			});
		}

		EDIOrgHeader Org1;
		LicenceDatabase Db1;
		EdiLicenceSetting Setting2;
		EdiLicenceSetting Setting3;
	}
}
