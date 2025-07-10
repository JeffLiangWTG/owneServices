using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(NotificationRolesContactsModule))]
	public class NotificationRolesContactsModuleTest : ZFilterStripGridModuleTestCase
	{
		protected override void SetUp()
		{
			TransactionedTestCase.RunClientDbCreateScripts();
			base.SetUp();
		}

		protected override Dictionary<string, string> GetExpectedAuditFilters()
		{
			Dictionary<string, string> result = base.GetExpectedAuditFilters();
			result.Add("Created On Web/Internal", "Created On Web/Internal");
			result.Add("Created Time", "Created Time");
			result.Add("Last Edit Time", "Last Edit Time");
			return result;
		}

		protected override bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection) => false;
		protected override ColumnDetailsForTest[] ExpectedColumnDetails
		{
			get
			{
				var result = new List<ColumnDetailsForTest>();
				result.Add(new ColumnDetailsForTest("Contact Name", 0, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Email", 1, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("Company Name", 2, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("UNLOCO", 3, typeof(ZTextEditColumn)));
				result.Add(new ColumnDetailsForTest("CSV", 4, typeof(ZCheckBoxColumn)));
				result.Add(new ColumnDetailsForTest("A/R", 5, typeof(ZCheckBoxColumn)));
				result.Add(new ColumnDetailsForTest("BOR", 6, typeof(ZCheckBoxColumn)));
				result.Add(new ColumnDetailsForTest("ERA", 7, typeof(ZCheckBoxColumn)));
				result.Add(new ColumnDetailsForTest("IST", 8, typeof(ZCheckBoxColumn)));
				result.Add(new ColumnDetailsForTest("CCP", 9, typeof(ZCheckBoxColumn)));
				return result.ToArray();
			}
		}

		protected override WebModuleID TestID => WebModuleIDs.CargoWiseEDINotificationRolesContacts;
		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(OrgContactSchema.OC_ContactName.Name, ListSortDirection.Ascending) };
		protected override FilterLayoutCodePairRegistryItem ExpectedDefaultLayoutRegistryItem => EDIDataRegistry.Instance.DefaultFilterLayoutForNotificationRoles;
		protected override string ExpectedDefaultLayoutName => string.Empty;
		public void TestGridCollectionTypeMatchExpected()
		{
			AssertEquals("Grid collection type is incorrect", typeof(OrgContactCollection), FilterStripGridModule.GridCollectionType);
		}

		protected override bool AllowActiveStatusFilterTest() => false;
		public override bool GridHasHyperLinkColumn => false;
		public override void TestDefaultGridColumns() => Assert(true);
	}
}
