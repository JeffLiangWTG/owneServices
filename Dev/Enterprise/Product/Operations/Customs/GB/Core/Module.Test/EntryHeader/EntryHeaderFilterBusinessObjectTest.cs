using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	sealed class EntryHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestActualOfficeOfExitFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_ExitActualOffice = "GB000084";

			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_ExitActualOffice = "XI000084";
			Factory.Save();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[Module.EntryHeaderFilterBusinessObject.FilterConstants.ActualOfficeOfExit];
			AssertNotNull("Filter " + Module.EntryHeaderFilterBusinessObject.FilterConstants.ActualOfficeOfExit, filter);
			AssertEquals("ActualOfficeOfExit Multilingual Description", Module.EntryHeaderFilterBusinessObject.FilterConstants.ActualOfficeOfExit, filter.MultilingualDescription);

			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "GB";
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", expected: true, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", expected: false, entry2.MatchesFilter(filterStrip.Filter));
			});

			filter.Property = "XI";
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", expected: false, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", expected: true, entry2.MatchesFilter(filterStrip.Filter));
			});
		}

		public void TestDateOfExitFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_ExitDate = ZDateTime.Today;

			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_ExitDate = ZDateTime.Today.AddDays(1);

			var declaration3 = Factory.New<JobDeclaration>();
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_ExitDate = ZDateTime.Today.AddDays(-2);

			Factory.Save();
			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleSingleDateFilter)filterStrip[Module.EntryHeaderFilterBusinessObject.FilterConstants.DateOfExit];
			AssertNotNull("Filter " + Module.EntryHeaderFilterBusinessObject.FilterConstants.DateOfExit, filter);
			AssertEquals("DateOfExit Multilingual Description", Module.EntryHeaderFilterBusinessObject.FilterConstants.DateOfExit, filter.MultilingualDescription);

			filter.IsActive = true;
			filter.Property1 = ZDateTime.Today;
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter1", expected: true, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter1", expected: false, entry2.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration3, MatchesFilter1", expected: false, entry3.MatchesFilter(filterStrip.Filter));
			});

			filter.Property1 = ZDateTime.Today.AddDays(1);
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter2", expected: false, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter2", expected: true, entry2.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration3, MatchesFilter2", expected: false, entry3.MatchesFilter(filterStrip.Filter));
			});

			filter.Property1 = ZDateTime.Today.AddDays(-2);
			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter3", expected: false, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter3", expected: false, entry2.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration3, MatchesFilter3", expected: true, entry3.MatchesFilter(filterStrip.Filter));
			});
		}

		public void TestInventoryConsignmentReferenceFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MasterUCR = "HBAC5554444444433";
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MasterUCR = "HBAC6664444444411";
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			Factory.Save();

			AssertEquals("Pre-condition", entry1.CH_MasterUCR, "HBAC5554444444433");
			AssertEquals("Pre-condition", entry2.CH_MasterUCR, "HBAC6664444444411");

			var stripBO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)stripBO["Inventory Consignment Reference (MUCR)"];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "HBAC5";

			AssertEquals("InventoryConsignmentReference Multilingual Description", Module.EntryHeaderFilterBusinessObject.FilterConstants.InventoryConsignmentReference, filter.MultilingualDescription);
			Assert(entry1.MatchesFilter(stripBO.Filter));
			Assert(!entry2.MatchesFilter(stripBO.Filter));
		}

		public void TestDUCRFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.SetValue(JobDeclarationSchema.JE_UCR, new ZString("DUCR123"));

			var entry1 = declaration1.CustomsEntryHeaders.AddNew();

			var ducrEntry1 = Factory.New<CusEntryNumber>();
			ducrEntry1.CE_EntryType = "DUC";
			ducrEntry1.CE_ParentTable = "CusEntryHeader";
			ducrEntry1.CE_ParentID = entry1.GetValue(CusEntryHeaderSchema.PK);
			ducrEntry1.CE_EntryNum = "DUCR123";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.SetValue(JobDeclarationSchema.JE_UCR, new ZString("DUCR456"));

			var entry2 = declaration2.CustomsEntryHeaders.AddNew();

			var ducrEntry2 = Factory.New<CusEntryNumber>();
			ducrEntry2.CE_EntryType = "DUC";
			ducrEntry2.CE_ParentTable = "CusEntryHeader";
			ducrEntry2.CE_ParentID = entry2.GetValue(CusEntryHeaderSchema.PK);
			ducrEntry2.CE_EntryNum = "DUCR456";

			Factory.Save();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip[Module.EntryHeaderFilterBusinessObject.FilterConstants.DUCR];
			AssertNotNull("Filter " + Module.EntryHeaderFilterBusinessObject.FilterConstants.DUCR, filter);

			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "DUCR123"; 

			CombineAssertions(() =>
			{
				AssertEquals("Entry of Declaration1, MatchesFilter", expected: true, entry1.MatchesFilter(filterStrip.Filter));
				AssertEquals("Entry of Declaration2, MatchesFilter", expected: false, entry2.MatchesFilter(filterStrip.Filter));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
