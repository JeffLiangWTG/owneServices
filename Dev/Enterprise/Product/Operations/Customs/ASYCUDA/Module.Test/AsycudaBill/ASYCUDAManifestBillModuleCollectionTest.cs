using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Module.Testing
{
	[TestedType(typeof(ASYCUDAManifestBillModuleCollection))]
	sealed class ASYCUDAManifestBillModuleCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultFilter()
		{
			var newFactory = new BusinessObjectFactory();
			var concol1 = newFactory.New<ForwardingConsol>();
			var manifestHeader1ZA = (AsycudaManifestHeader)newFactory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestHeader1ZA.SetParent(concol1);
			manifestHeader1ZA.AMA_JobReference = "VV1";
			manifestHeader1ZA.AMA_RN_NKCountry = "ZA";
			var bill1ZA = manifestHeader1ZA.Bills.AddNew();
			bill1ZA.ABL_IsActive = false;
			bill1ZA.ABL_BolType = "STD";
			var bill2ZA = manifestHeader1ZA.Bills.AddNew();
			bill2ZA.ABL_IsActive = true;
			bill2ZA.ABL_BolType = "CLD";
			var bill3ZA = manifestHeader1ZA.Bills.AddNew();
			bill3ZA.ABL_IsActive = true;
			bill3ZA.ABL_BolType = "BOL";
			var concol2 = newFactory.New<ForwardingConsol>();
			var manifestHeader2ZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			manifestHeader2ZA.AMA_ApplicationCode = "OUT";
			manifestHeader2ZA.SetParent(concol2);
			manifestHeader2ZA.AMA_JobReference = "VV2";
			var bill4ZA = manifestHeader2ZA.Bills.AddNew();
			bill4ZA.ABL_IsActive = true;
			bill4ZA.ABL_BolType = "STD";
			var bill5ZA = manifestHeader2ZA.Bills.AddNew();
			bill5ZA.ABL_IsActive = true;
			bill5ZA.ABL_BolType = "CLD";
			var concol3 = newFactory.New<ForwardingConsol>();
			var manifestHeader3 = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			manifestHeader3.AMA_ApplicationCode = "OUT";
			manifestHeader3.SetParent(concol3);
			manifestHeader3.AMA_JobReference = "VV3";
			var bill6 = manifestHeader2ZA.Bills.AddNew();
			bill6.ABL_IsActive = true;
			bill6.ABL_BolType = "STD";
			var manifestHeader4 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			manifestHeader4.AMA_RN_NKCountry = "SG";
			var bill7 = manifestHeader4.Bills.AddNew();
			bill7.ABL_IsActive = true;
			bill7.ABL_BolType = "STD";
			newFactory.Save();
			var collection = new ASYCUDAManifestBillModuleCollection(Factory);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(new[] { bill1ZA.PK, bill2ZA.PK }, collection.Select(x => x.PK));
			collection.DeleteAll();
			var newFactoryTR = new BusinessObjectFactory();
			var concolTR1 = newFactoryTR.New<ForwardingConsol>();
			var manifestHeader1TR = (AsycudaManifestHeader)newFactoryTR.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			manifestHeader1TR.SetParent(concolTR1);
			manifestHeader1TR.AMA_JobReference = "UU1";
			manifestHeader1TR.AMA_RN_NKCountry = "TR";
			var bill1TR = manifestHeader1TR.Bills.AddNew();
			bill1TR.ABL_IsActive = false;
			bill1TR.ABL_BolType = "STD";
			var bill2TR = manifestHeader1TR.Bills.AddNew();
			bill2TR.ABL_IsActive = true;
			bill2TR.ABL_BolType = "CLD";
			var bill3TR = manifestHeader1TR.Bills.AddNew();
			bill3TR.ABL_IsActive = true;
			bill3TR.ABL_BolType = "BOL";
			var concolTR2 = newFactoryTR.New<ForwardingConsol>();
			var manifestHeader2TR = (AsycudaManifestHeader)newFactoryTR.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			manifestHeader2TR.AMA_ApplicationCode = "ETR";
			manifestHeader2TR.SetParent(concolTR2);
			manifestHeader2TR.AMA_JobReference = "UU2";
			var bill4TR = manifestHeader2TR.Bills.AddNew();
			bill4TR.ABL_IsActive = true;
			bill4TR.ABL_BolType = "STD";
			var bill5TR = manifestHeader2TR.Bills.AddNew();
			bill5TR.ABL_IsActive = true;
			bill5TR.ABL_BolType = "CLD";
			var concolTR3 = newFactoryTR.New<ForwardingConsol>();
			var manifestHeader3TR = (AsycudaManifestHeader)newFactoryTR.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			manifestHeader3TR.AMA_ApplicationCode = "ETR";
			manifestHeader3TR.SetParent(concolTR3);
			manifestHeader3TR.AMA_JobReference = "UU3";
			var bill6TR = manifestHeader3TR.Bills.AddNew();
			bill6TR.ABL_IsActive = true;
			bill6TR.ABL_BolType = "STD";
			var manifestHeaderSG4 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			manifestHeaderSG4.AMA_RN_NKCountry = "SG";
			var billSG7 = manifestHeaderSG4.Bills.AddNew();
			billSG7.ABL_IsActive = true;
			billSG7.ABL_BolType = "STD";
			newFactoryTR.Save();
			var collection2 = new ASYCUDAManifestBillModuleCollection(Factory);
			collection2.Load();
			AssertContainsExactElementsInAnyOrder(new[] { bill1TR.PK, bill2TR.PK }, collection2.Select(x => x.PK));
		}

		public void TestDefaultFilterDoesNotContainMax()
		{
			var collection = new ASYCUDAManifestBillModuleCollection(Factory);
			var filter = collection.CompleteFilter;
			AssertNotContains("MAX(", filter.LiteralTextADO, true);
		}

		public void TestFetchForView()
		{
			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.NZManifest.IAsycudaManifestHeader>();
			header1.AMA_JobReference = "MAM001";
			header1.AMA_MasterBill = "MN001";
			header1.AMA_TransportMode = "SEA";
			header1.RegistrationNumber = "RN001";
			var bill1 = header1.Bills.AddNew();
			bill1.ABL_BillNumber = "BIL001";
			var billEntryNumber1 = bill1.CustomsEntryNumbers.AddNew();
			billEntryNumber1.CE_EntryType = "ASY";
			billEntryNumber1.CE_EntryNum = "BEN001";
			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.NZManifest.IAsycudaManifestHeader>();
			header2.AMA_JobReference = "MAM002";
			header2.AMA_MasterBill = "MN002";
			header2.AMA_TransportMode = "SEA";
			header2.RegistrationNumber = "RN002";
			var bill2 = header2.Bills.AddNew();
			bill2.ABL_BillNumber = "BIL002";
			var billEntryNumber2 = bill2.CustomsEntryNumbers.AddNew();
			billEntryNumber2.CE_EntryType = "ASY";
			billEntryNumber2.CE_EntryNum = "BEN002";
			Factory.Save();
			var factory = new BusinessObjectFactory();
			using (factory.SuspendCustomsValuesFetchHint(typeof(AsycudaBill)))
			using (factory.SuspendCustomsValuesFetchHint(typeof(AsycudaManifestHeader)))
			{
				var collection = new ASYCUDAManifestBillModuleCollection(factory);
				collection.Load();
				AssertEquals(2, collection.Count);
				int count = factory.ActiveTableFetchHints;
				var strategy = new ASYCUDAManifestBillModuleCollectionFetchStrategy<AsycudaBill>(collection);
				var tc1 = new TableColumn("", "Header+AMA_JobReference");
				var tc2 = new TableColumn("", "Header+AMA_MasterBill");
				var tc3 = new TableColumn("", "ABL_BillNumber");
				var tc4 = new TableColumn("", "CusEntryNumber+CE_EntryStatus");
				var tc5 = new TableColumn("", "Header+AMA_ManifestType");
				var tc6 = new TableColumn("", "ABL_BillIssuer");
				var tc7 = new TableColumn("", "CustomsEntryNumber");
				var tc8 = new TableColumn("", "Header+CycleDate");
				var tc9 = new TableColumn("", "CycleDate");
				var tc10 = new TableColumn("", "StatusDescription");
				strategy.FetchForView(collection.ToArray(), new[] { tc1 });
				AssertEquals("Added 1 extra fetch hint.", count, factory.ActiveTableFetchHints);
				AssertEquals("Bill was loaded, so fetch hint is 0.", 0, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc2 });
				AssertEquals("Added 1 extra fetch hint.", count + 1, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc3 });
				AssertEquals("No new fetch hint needed.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc4 });
				AssertEquals("Added 1 extra fetch hint.", count + 1, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc5 });
				AssertEquals("Added 1 extra fetch hint.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc6 });
				AssertEquals("No new fetch hint needed.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc7 });
				AssertEquals("No new fetch hint needed.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc8 });
				AssertEquals("Add 1 fetch hint for GenAddOnColumn, reduce 1 fetch hint for header country after header country is loaded.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(GenAddOnColumnSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc9 });
				AssertEquals("No new fetch hint needed.", count, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(GenAddOnColumnSchema.Constants.TableName));
				count = factory.ActiveTableFetchHints;
				strategy.FetchForView(collection.ToArray(), new[] { tc10 });
				AssertEquals("Added 1 extra fetch hint.", count + 1, factory.ActiveTableFetchHints);
				AssertEquals(2, factory.ActiveFetchHintsForTable(AsycudaBillSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(AsycudaManifestHeaderSchema.Constants.TableName));
				AssertEquals(0, factory.ActiveFetchHintsForTable(GenAddOnColumnSchema.Constants.TableName));
				AssertEquals(2, factory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new ASYCUDAManifestBillModuleCollection(Factory);
	}
}
