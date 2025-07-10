using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Intrastat.Module.Testing
{
	[TestedType(typeof(IntrastatTransactionsFilterStripBusinessObject))]
	sealed class IntrastatTransactionsFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = GetNewFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				AssertNotNull("TradersReference", filter[IntrastatTransactionsFilterStripBusinessObject.Schema.TradersReference]);
				AssertNotNull("TransactionDate", filter[IntrastatTransactionsFilterStripBusinessObject.Schema.TransactionDate]);
				AssertNotNull("CountryOfSupply", filter[IntrastatTransactionsFilterStripBusinessObject.Schema.CountryOfSupply]);
				AssertNotNull("CountryOfReceipt", filter[IntrastatTransactionsFilterStripBusinessObject.Schema.CountryOfReceipt]);
				AssertNotNull("Consignee", filter[IntrastatTransactionsFilterStripBusinessObject.Schema.Consignee]);
				AssertNotNull("Supplier", filter[IntrastatTransactionsFilterStripBusinessObject.Schema.Supplier]);
				AssertNotNull("ConsigneeName", filter[IntrastatTransactionsFilterStripBusinessObject.Schema.ConsigneeName]);
				AssertNotNull("SupplierName", filter[IntrastatTransactionsFilterStripBusinessObject.Schema.SupplierName]);
				AssertNotNull("TransportMode", filter[IntrastatTransactionsFilterStripBusinessObject.Schema.TransportMode]);
			});
		}

		public void TestFilters_MultilingualDescription()
		{
			var filter = (IntrastatTransactionsFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				var tradersReferenceFilter = (ModuleTextFilter)filter[IntrastatTransactionsFilterStripBusinessObject.Schema.TradersReference];
				AssertEquals("TradersReference", IntrastatTransactionsFilterStripBusinessObject.Schema.TradersReference, tradersReferenceFilter.MultilingualDescription);

				var transactionDateFilter = (ModuleDateFilter)filter[IntrastatTransactionsFilterStripBusinessObject.Schema.TransactionDate];
				AssertEquals("TransactionDate", IntrastatTransactionsFilterStripBusinessObject.Schema.TransactionDate, transactionDateFilter.MultilingualDescription);

				var countryOfSupplyFilter = (ModuleTextFilter)filter[IntrastatTransactionsFilterStripBusinessObject.Schema.CountryOfSupply];
				AssertEquals("CountryOfSupply", IntrastatTransactionsFilterStripBusinessObject.Schema.CountryOfSupply, countryOfSupplyFilter.MultilingualDescription);

				var countryOfReceiptFilter = (ModuleTextFilter)filter[IntrastatTransactionsFilterStripBusinessObject.Schema.CountryOfReceipt];
				AssertEquals("CountryOfReceipt", IntrastatTransactionsFilterStripBusinessObject.Schema.CountryOfReceipt, countryOfReceiptFilter.MultilingualDescription);

				var consigneeFilter = (ModuleGuidFilter)filter[IntrastatTransactionsFilterStripBusinessObject.Schema.Consignee];
				AssertEquals("Consignee", IntrastatTransactionsFilterStripBusinessObject.Schema.Consignee, consigneeFilter.MultilingualDescription);

				var supplierFilter = (ModuleGuidFilter)filter[IntrastatTransactionsFilterStripBusinessObject.Schema.Supplier];
				AssertEquals("Supplier", IntrastatTransactionsFilterStripBusinessObject.Schema.Supplier, supplierFilter.MultilingualDescription);

				var consigneeNameFilter = (ModuleTextFilter)filter[IntrastatTransactionsFilterStripBusinessObject.Schema.ConsigneeName];
				AssertEquals("ConsigneeName", IntrastatTransactionsFilterStripBusinessObject.Schema.ConsigneeName, consigneeNameFilter.MultilingualDescription);

				var supplierNameFilter = (ModuleTextFilter)filter[IntrastatTransactionsFilterStripBusinessObject.Schema.SupplierName];
				AssertEquals("SupplierName", IntrastatTransactionsFilterStripBusinessObject.Schema.SupplierName, supplierNameFilter.MultilingualDescription);

				var transportModeFilter = (ModuleTextFilter)filter[IntrastatTransactionsFilterStripBusinessObject.Schema.TransportMode];
				AssertEquals("TransportMode", IntrastatTransactionsFilterStripBusinessObject.Schema.TransportMode, transportModeFilter.MultilingualDescription);
			});
		}
		public void TestNumberFilters()
		{
			//TradersReference
			var header1 = CreateCusIntrastatHeader("I00001001");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertIntrastatHeaderIsInTextFilterResults(header1, IntrastatTransactionsFilterStripBusinessObject.Schema.TradersReference, "I00001001", "Should Find Declaration by Traders Ref. #");
			});
		}

		public void TestDateFilters()
		{
			// Transaction Date
			var header1 = CreateCusIntrastatHeader("I00001001");
			header1.CIH_TransactionDate = TestDate;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertIntrastatHeaderIsInDateFilterResults(header1, IntrastatTransactionsFilterStripBusinessObject.Schema.TransactionDate, "Should Find Intrastat Header by TransactionDate");
			});
		}

		public void TestModesAndTypesFilters()
		{
			// Transport Mode
			var header1 = CreateCusIntrastatHeader("I000010014");
			header1.CIH_ModeOfTransport = TransportTypeList.Codes.Road;

			var header2 = CreateCusIntrastatHeader("I000010015");
			header2.CIH_ModeOfTransport = TransportTypeList.Codes.Sea;

			var header3 = CreateCusIntrastatHeader("I000010016");
			header3.CIH_ModeOfTransport = TransportTypeList.Codes.Air;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertIntrastatHeaderIsInTextFilterResults(header1, IntrastatTransactionsFilterStripBusinessObject.Schema.TransportMode, TransportTypeList.Codes.Road, "Should Find Intrastat Header by TransportMode Road");
				AssertIntrastatHeaderIsInTextFilterResults(header2, IntrastatTransactionsFilterStripBusinessObject.Schema.TransportMode, TransportTypeList.Codes.Sea, "Should Find Intrastat Header by TransportMode Sea");
				AssertIntrastatHeaderIsInTextFilterResults(header3, IntrastatTransactionsFilterStripBusinessObject.Schema.TransportMode, TransportTypeList.Codes.Air, "Should Find Intrastat Header by TransportMode Air");
			});
		}

		public void TestOrganizationFilters()
		{
			var filter = new IntrastatTransactionsFilterStripBusinessObject();
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			// Consignee
			var header1 = CreateCusIntrastatHeader("B00001001");
			header1.CIH_OH_Consignee = organisation.PK;
			header1.CIH_ConsigneeName = null;
			Factory.Save();

			// Supplier
			var header2 = CreateCusIntrastatHeader("B00001002");
			header2.CIH_OH_Supplier = organisation.PK;
			header2.CIH_SupplierName = null;
			Factory.Save();

			// Consignee Name
			var header3 = CreateCusIntrastatHeader("B00001003");
			header3.CIH_OH_Consignee = ZGuid.Empty;
			header3.CIH_ConsigneeName = "CONSIGNEEE";
			Factory.Save();

			// Supplier Name
			var header4 = CreateCusIntrastatHeader("B00001004");
			header4.CIH_OH_Supplier = ZGuid.Empty;
			header4.CIH_SupplierName = "SUPPLIER";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertIntrastatHeaderIsTheOnlyOrgFilterResult(organisation.PK, IntrastatTransactionsFilterStripBusinessObject.Schema.Consignee, header1, "Should Find Intrastat Header by Consignee");
				AssertIntrastatHeaderIsTheOnlyOrgFilterResult(organisation.PK, IntrastatTransactionsFilterStripBusinessObject.Schema.Supplier, header2, "Should Find Intrastat Header by Supplier");
				AssertIntrastatHeaderIsInTextFilterResults(header3, IntrastatTransactionsFilterStripBusinessObject.Schema.ConsigneeName, "CONSIGNEEE", "Should Find Intrastat Header by Consignee Name");
				AssertIntrastatHeaderIsInTextFilterResults(header4, IntrastatTransactionsFilterStripBusinessObject.Schema.SupplierName, "SUPPLIER", "Should Find Intrastat Header by Supplier Name");
			});
		}

		public void TestLocationsFilters()
		{
			//Country of supply
			var header1 = CreateCusIntrastatHeader("I00001001");
			header1.CIH_CountryOfSupply = "DE";
			Factory.Save();

			var header2 = CreateCusIntrastatHeader("I00001002");
			header2.CIH_CountryOfReceipt = "IT";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertIntrastatHeaderIsInTextFilterResults(header1, IntrastatTransactionsFilterStripBusinessObject.Schema.CountryOfSupply, "DE", "Should Find Declaration by Country of supply");
				AssertIntrastatHeaderIsInTextFilterResults(header2, IntrastatTransactionsFilterStripBusinessObject.Schema.CountryOfReceipt, "IT", "Should Find Declaration by Country of receipt");
			});
		}

		public void TestLookups()
		{
			var filter = (IntrastatTransactionsFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				AssertType<IntrastatTransactionsFilterLookups>("Type", filter.Lookups);
				AssertSame("Cached", filter.Lookups, filter.Lookups);
			});
		}

		public void TestDefaultCreatedTimeFilter()
		{
			var filterBO = new IntrastatTransactionsFilterStripBusinessObject();
			filterBO.QueryObjectType = typeof(CusIntrastatHeader);
			var filter = filterBO.ModuleFilters["Created Time"] as ModuleDateFilter;
			AssertEquals(true, filter.Visible);
			AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
			AssertEquals(ModuleDateFilter.DateRangeSearchTexts.Last3Mths, filter.PropertySearch);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new IntrastatTransactionsFilterStripBusinessObject();

		ZDate TestDate => ZDate.Today.AddDays(2);

		CusIntrastatHeader CreateCusIntrastatHeader(ZString tradersReference)
		{
			var intrastatHeader = Factory.NewWithValidTestData<CusIntrastatHeader>();
			intrastatHeader.CIH_GC_Company = GlbCompany.CurrentCompany.PK;
			intrastatHeader.CIH_TradersReference = tradersReference;

			return intrastatHeader;
		}

		void AssertIntrastatHeaderIsInTextFilterResults(CusIntrastatHeader header, string filterName, ZString filterProperty, ZString message)
		{
			var filter = new IntrastatTransactionsFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filter[filterName];

			AssertNotNull(message, textFilter);

			textFilter.Property = filterProperty;
			textFilter.IsActive = true;

			var coll = new CusIntrastatHeaderCollection(Factory, filter.Filter);
			AssertEquals(message + " [count]", 1, coll.Count);
			AssertEquals(message, header.CIH_TradersReference, coll[0].CIH_TradersReference);

			textFilter.IsActive = false;
		}

		void AssertIntrastatHeaderIsInDateFilterResults(CusIntrastatHeader header, string filterName, ZString message)
		{
			var filterStrip = new IntrastatTransactionsFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filterStrip[filterName];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = TestDate;
			dateFilter.Property2 = TestDate;
			dateFilter.IsActive = true;

			var coll = new CusIntrastatHeaderCollection(Factory, filterStrip.Filter);
			AssertEquals(message + " [count]", 1, coll.Count);
			AssertEquals(message, header.CIH_TradersReference, coll[0].CIH_TradersReference);

			dateFilter.IsActive = false;
		}

		void AssertIntrastatHeaderIsTheOnlyOrgFilterResult(ZGuid orgPK, string filterName, CusIntrastatHeader header, ZString message)
		{
			var filter = new IntrastatTransactionsFilterStripBusinessObject();
			var orgFilter = (ModuleGuidFilter)filter[filterName];
			orgFilter.Property = orgPK;
			orgFilter.IsActive = true;

			var coll = new CusIntrastatHeaderCollection(Factory, filter.Filter);
			AssertEquals(message + " [count]", 1, coll.Count);
			AssertEquals(message, header.CIH_TradersReference, coll[0].CIH_TradersReference);

			orgFilter.IsActive = false;
		}
	}
}
