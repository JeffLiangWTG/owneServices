using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Customs.EU.Module.EntryHeaderFilterBusinessObject;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	sealed class EntryHeaderAddressesFilterApplierTest : FilterStripBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new EntryHeaderAddressesFilterApplier(null));
		}

		public void TestAddFilters()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new EntryHeaderAddressesFilterApplier(new EntryHeaderFilterBusinessObject()).AddFilters(null));
		}

		public void TestFromWarehouseFilter()
		{
			Factory.Save();

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});

			var fromWarehouseFilter = (ModuleGuidFilter)filterStripBusinessObject[EUFilterConstants.AddressFromWarehouseFilter];
			AssertNotNull("From Warehouse Filter", fromWarehouseFilter);
			fromWarehouseFilter.Property = warehouseAddress1.OA_OH;
			fromWarehouseFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When From Warehouse Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection1[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 0, entryHeaderCollection2.Count);
			});

			fromWarehouseFilter.Property = warehouseAddress2.OA_OH;
			fromWarehouseFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When From Warehouse Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 0, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection2[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});
		}

		public void TestToWarehouseFilter()
		{
			Factory.Save();

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});

			var toWarehouseFilter = (ModuleGuidFilter)filterStripBusinessObject[EUFilterConstants.AddressToWarehouseFilter];
			AssertNotNull("To Warehouse Filter", toWarehouseFilter);
			toWarehouseFilter.Property = warehouseAddress1.OA_OH;
			toWarehouseFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When To Warehouse Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection1[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 0, entryHeaderCollection2.Count);
			});

			toWarehouseFilter.Property = warehouseAddress2.OA_OH;
			toWarehouseFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When To Warehouse Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 0, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection2[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});
		}

		public void TestLocalClientCodeFilter()
		{
			var localClientAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var jobHeader1 = Factory.NewJobForTesting<JobHeader>();
			jobHeader1.JH_ParentID = declaration1.PK;
			jobHeader1.JH_GC = GlbCompany.CurrentCompany.PK;
			declaration1.Job.JH_OA_LocalChargesAddr = localClientAddress1.PK;

			var localClientAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			var jobHeader2 = Factory.NewJobForTesting<JobHeader>();
			jobHeader2.JH_ParentID = declaration2.PK;
			jobHeader2.JH_GC = GlbCompany.CurrentCompany.PK;
			declaration2.Job.JH_OA_LocalChargesAddr = localClientAddress2.PK;

			Factory.Save();

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});

			var localClientFilter = (ModuleGuidFilter)filterStripBusinessObject[EUFilterConstants.AddressLocalClientCodeFilter];
			AssertNotNull("Local Client Code Filter", localClientFilter);
			localClientFilter.Property = localClientAddress1.OA_OH;
			localClientFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When To Warehouse Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 1, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader1.PK, entryHeaderCollection1[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 0, entryHeaderCollection2.Count);
			});

			localClientFilter.Property = localClientAddress2.OA_OH;
			localClientFilter.IsActive = true;

			entryHeaderCollection1.Load(filterStripBusinessObject.Filter);
			entryHeaderCollection2.Load(filterStripBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When To Warehouse Filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection1 Count", 0, entryHeaderCollection1.Count);
				AssertEquals("Single EntryHeader found PK", entryHeader2.PK, entryHeaderCollection2[0].PK);
				AssertEquals("EntryHeaderCollection2 Count", 1, entryHeaderCollection2.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration1 = Factory.New<JobDeclaration>();
			warehouseAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			entryInstruction1 = Factory.New<CusEntryInstruction>();
			entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			entryHeaderCollection1 = new CusEntryHeaderCollection<CusEntryHeader>(declaration1, Factory);
			entryInstruction1.CEI_OA_Warehouse = warehouseAddress1.PK;
			entryInstruction1.CEI_OA_Warehouse2 = warehouseAddress1.PK;
			declaration1.CustomsEntryInstructions.Add(entryInstruction1);

			declaration2 = Factory.New<JobDeclaration>();
			warehouseAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			entryInstruction2 = Factory.New<CusEntryInstruction>();
			entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			entryHeaderCollection2 = new CusEntryHeaderCollection<CusEntryHeader>(declaration2, Factory);
			entryInstruction2.CEI_OA_Warehouse = warehouseAddress2.PK;
			entryInstruction2.CEI_OA_Warehouse2 = warehouseAddress2.PK;
			declaration2.CustomsEntryInstructions.Add(entryInstruction2);

			filterStripBusinessObject = GetNewFilterStripBusinessObject();
		}

		JobDeclaration declaration1;
		JobDeclaration declaration2;
		CusEntryHeader entryHeader1;
		CusEntryHeader entryHeader2;
		CusEntryHeaderCollection<CusEntryHeader> entryHeaderCollection1;
		CusEntryHeaderCollection<CusEntryHeader> entryHeaderCollection2;
		FilterStripBusinessObject filterStripBusinessObject;
		OrgAddress warehouseAddress1;
		OrgAddress warehouseAddress2;
		CusEntryInstruction entryInstruction1;
		CusEntryInstruction entryInstruction2;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
