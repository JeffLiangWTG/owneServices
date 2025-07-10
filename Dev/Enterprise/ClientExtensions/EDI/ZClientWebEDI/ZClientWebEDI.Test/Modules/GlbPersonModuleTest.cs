using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Web.Module
{
	[TestedType(typeof(GlbPersonModule))]
	public class GlbPersonModuleTest : ZFilterStripGridModuleTestCase
	{
		protected override bool AllowActiveStatusFilterTest()
		{
			return false;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery
		protected override bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection)
		{
			return false;
		}

		#endregion
		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				List<ColumnDetailsForTest> result = new List<ColumnDetailsForTest>();
				result.Add(new ColumnDetailsForTest("Full Name", 0, typeof(ZButtonColumn)));
				result.Add(new ColumnDetailsForTest("Email Address", 1, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Primary Workplace", 2, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Working Location", 3, typeof(ZTextEditColumn)));
				return result.ToArray();
			}
		}

		public override void TestLoadCollectionReturnsRowCount()
		{
			FilterBusinessObject filterBizO = FilterGridModule.CreateNewFilterBusinessObject();
			var contact1 = TestHelper.TestOrg.Contacts.AddNew();
			contact1.OC_ContactName = "name 1";
			contact1.OC_Email = "name1@contact.com";
			var contact2 = TestHelper.TestOrg.Contacts.AddNew();
			contact2.OC_ContactName = "name 2";
			contact2.OC_Email = "name2@contact.com";
			var contact3 = TestHelper.TestOrg.Contacts.AddNew();
			contact3.OC_ContactName = "name 3";
			contact3.OC_Email = "name3@contact.com";
			TestHelper.TestOrg.Factory.Save();
			contact1.Person.SetPrimaryRelationship(contact1);
			contact2.Person.SetPrimaryRelationship(contact2);
			contact3.Person.SetPrimaryRelationship(contact3);
			TestHelper.TestOrg.Factory.Save();
			AssertEquals("FilterGridModule should limit to 1000 rows by default", 1000, FilterGridModule.MaxRows);
			FilterGridModule.MaxRows = 2;
			FilterGridModule.LoadCollection(filterBizO);
			if (FilterGridModule.GridCollection.TypeOfElements.IsClass && !FilterGridModule.GridCollection.TypeOfElements.IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				int expectedCount = Factory.GetDatabaseCount(FilterGridModule.GridCollection.TypeOfElements, filterBizO.Filter);
				expectedCount = (expectedCount > FilterGridModule.MaxRows) ? FilterGridModule.MaxRows : expectedCount;
				AssertEquals("LoadCollection should not have returned more than 2 rows", expectedCount, FilterGridModule.GridCollection.Count);
			}
			else
			{
				Assert("LoadCollection should not have returned more than 2 rows", FilterGridModule.GridCollection.Count <= 250);
			}
		}

		int ExcelExportRecordCounter;
		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var contact = TestHelper.TestOrg.Contacts.AddNew();
			contact.OC_ContactName = $"Name {ExcelExportRecordCounter++}";
			contact.OC_Email = $"name{ExcelExportRecordCounter}@contact.com";
			contact.Factory.Save();
			contact.Person.SetPrimaryRelationship(contact);
			return contact;
		}

		protected override WebModuleID TestID
		{
			get
			{
				return WebModuleIDs.GlbPerson;
			}
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(GlbPersonSchema.PER_FullName.Name, ListSortDirection.Ascending) };
		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem
		{
			get
			{
				return EDIDataRegistry.Instance.DefaultFilterLayoutForWebGlbPerson;
			}
		}

		protected override string ExpectedDefaultLayoutName
		{
			get
			{
				return string.Empty;
			}
		}

		public void TestGridCollectionTypeMatchExpected()
		{
			AssertEquals("Grid collection type is incorrect", typeof(GlbPersonCollection), FilterStripGridModule.GridCollectionType);
		}

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			var result = new Dictionary<string, string>(3);
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			return result;
		}

		protected override void SetUp()
		{
			TransactionedTestCase.RunClientDbCreateScripts();
			var sql = @"
DELETE FROM dbo.OrgContact
WHERE OC_PK IN
(
  SELECT TOP 1000
    OC_PK
  FROM dbo.OrgContact
)

DELETE FROM dbo.GlbPersonPrimaryRelationship
where PPR_PER not in
(
select OC_PER from dbo.OrgContact
)

DELETE FROM dbo.PatternMatchingEmail
DELETE FROM dbo.PatternMatchingPhone

DELETE FROM dbo.GlbPerson
where PER_PK not in
(
select OC_PER from dbo.OrgContact
)
";
			Db.Connection.ExecuteNonQuery(sql);
			base.SetUp();
		}
	}
}
