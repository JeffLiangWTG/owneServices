using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Customs.EU.Module.EntryHeaderFilterBusinessObject;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	sealed class EntryHeaderDeclarationFilterApplierTest : FilterStripBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new EntryHeaderDeclarationFilterApplier(null));
		}

		public void TestAddFilters()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new EntryHeaderDeclarationFilterApplier(new EntryHeaderFilterBusinessObject()).AddFilters(null));
		}

		public void TestEntryStyleFilter()
		{
			declaration1.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			declaration2.JE_EntryStyle = ZString.Empty;
			Factory.Save();

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});

			var entryStyleFilter = (ModuleTextFilter)filterStripBusinessObject[EUFilterConstants.DeclarationEntryStyleFilter];
			AssertNotNull("Declaration Entry Style Filter", entryStyleFilter);
			entryStyleFilter.Property = "EX";
			entryStyleFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			entryStyleFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Entry Style Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection1[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 0, entryHeaderCollection2.Count);
			});

			entryStyleFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Entry Style filter is 'IsBlank'", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 0, entryHeaderCollection1.Count);
				AssertEquals("EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection2[0].PK);
			});
		}

		public void TestCustomsOfficeFilter()
		{
			declaration1.JE_CustomsOffice = "IT137100";
			Factory.Save();

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});

			var customsOfficeFilter = (ModuleNkFilter)filterStripBusinessObject[EUFilterConstants.DeclarationCustomsOfficeOfPresentation];
			AssertNotNull("Customs Office of Presentation Filter", customsOfficeFilter);
			customsOfficeFilter.Property = "IT137100";
			customsOfficeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			customsOfficeFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Customs Office of Presentation Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection1[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 0, entryHeaderCollection2.Count);
			});

			customsOfficeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Customs Office of Presentation filter is 'IsBlank'", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 0, entryHeaderCollection1.Count);
				AssertEquals("EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection2[0].PK);
			});
		}

		public void TestDispatchFilter()
		{
			declaration1.JE_GoodsOrigin = "US";
			declaration2.JE_GoodsOrigin = "";
			Factory.Save();

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});

			var declarationDispatchFilter = (ModuleTextFilter)filterStripBusinessObject[EUFilterConstants.DeclarationDispatch];
			AssertNotNull("Dispatch Filter", declarationDispatchFilter);
			declarationDispatchFilter.Property = "US";
			declarationDispatchFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			declarationDispatchFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Dispatch Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection1[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 0, entryHeaderCollection2.Count);
			});

			declarationDispatchFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Dispatch filter is 'IsBlank'", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 0, entryHeaderCollection1.Count);
				AssertEquals("EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection2[0].PK);
			});
		}

		public void TestDestinationFilter()
		{
			declaration1.JE_GoodsDestination = "DE";
			Factory.Save();

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});

			var declarationDestinationFilter = (ModuleTextFilter)filterStripBusinessObject[EUFilterConstants.DeclarationDestination];
			AssertNotNull("Destination Filter", declarationDestinationFilter);
			declarationDestinationFilter.Property = "DE";
			declarationDestinationFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			declarationDestinationFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Destination Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection1[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 0, entryHeaderCollection2.Count);
			});

			declarationDestinationFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Destination filter is 'IsBlank'", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 0, entryHeaderCollection1.Count);
				AssertEquals("EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection2[0].PK);
			});
		}

		public void TestApprovalDeferNoFilter()
		{
			declaration1.JE_DefermentAccountNumber = "1111111";
			declaration2.JE_DefermentAccountNumber = "";
			Factory.Save();

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});

			var approvalDeferNoFilter = (ModuleTextFilter)filterStripBusinessObject[EUFilterConstants.DeclarationApprovalDeferNo];
			AssertNotNull("Approval Defer No Filter", approvalDeferNoFilter);
			approvalDeferNoFilter.Property = "1111111";
			approvalDeferNoFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			approvalDeferNoFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Approval Defer No Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection1[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 0, entryHeaderCollection2.Count);
			});

			approvalDeferNoFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Approval Defer No filter is 'IsBlank'", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 0, entryHeaderCollection1.Count);
				AssertEquals("EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection2[0].PK);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration1 = Factory.New<JobDeclaration>();
			entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			declaration2 = Factory.New<JobDeclaration>();
			entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();

			entryHeaderCollection1 = new CusEntryHeaderCollection<CusEntryHeader>(declaration1, Factory);
			entryHeaderCollection2 = new CusEntryHeaderCollection<CusEntryHeader>(declaration2, Factory);
			filterStripBusinessObject = GetNewFilterStripBusinessObject();
		}

		JobDeclaration declaration1;
		JobDeclaration declaration2;
		CusEntryHeader entryHeader1;
		CusEntryHeader entryHeader2;
		CusEntryHeaderCollection<CusEntryHeader> entryHeaderCollection1;
		CusEntryHeaderCollection<CusEntryHeader> entryHeaderCollection2;
		FilterStripBusinessObject filterStripBusinessObject;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
