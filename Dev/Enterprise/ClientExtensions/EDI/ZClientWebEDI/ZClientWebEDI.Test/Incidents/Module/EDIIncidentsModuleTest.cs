using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Client;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using Enterprise.ZArchitecture.Web.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Module
{
	[TestedType(typeof(EDIIncidentsModule))]
	public class EDIIncidentsModuleTest : ZFilterGridModuleTest
	{
		protected override bool AllowActiveStatusFilterTest()
		{
			return false;
		}

		protected override FilterBusinessObject GetFilterObjectForExcelExport()
		{
			var filter = (WebSupportIncidentFilterBusinessObject)base.GetFilterObjectForExcelExport();
			filter.IM_OH_Client = SiteUser.LoggedInOrganisation.PK;
			return filter;
		}

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var incident = (SupportIncident)base.CreateNewElementForExcelExport();
			incident.IM_OH_Client = SiteUser.LoggedInOrganisation.PK;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			return incident;
		}

		#region TestCollectionLoadDBHitsWithDBOnlyQuery
		protected override bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection)
		{
			return false;
		}

		#endregion
		protected override WebModuleID TestID
		{
			get
			{
				return WebModuleIDs.CargoWiseEDIIncidents;
			}
		}

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			return Array.Empty<FilterBusinessObjectDefault>();
		}

		protected override ColumnAndSortOrder[] ExpectedSortInfos => new[] { new ColumnAndSortOrder(IncidentMainSchema.IM_SystemCreateTimeUtc.Name, ListSortDirection.Descending) };
		protected override ListSortDirection ExpectedDefaultSortOrder
		{
			get
			{
				return ListSortDirection.Descending;
			}
		}

		protected override Type ExpectedCollectionSorterType
		{
			get
			{
				return typeof(EDIIncidentWebCollectionSorter);
			}
		}

		public void TestGetNewGridColumnFields()
		{
			ZPage dummyPage = new ZPage();
			using (EDIIncidentsModule module = new EDIIncidentsModule(Factory, dummyPage))
			{
				DataGridColumn[] columns = module.InternalGetNewGridColumnFields();
				AssertEquals("Columns count", 8, columns.Length);
				AssertEquals("Header text", "Number", columns[0].HeaderText);
				AssertEquals("Header text", "Module", columns[1].HeaderText);
				AssertEquals("Header text", "Crit.", columns[2].HeaderText);
				AssertEquals("Header text", "Added", columns[3].HeaderText);
				AssertEquals("Header text", "Closed", columns[4].HeaderText);
				AssertEquals("Header text", "Status", columns[5].HeaderText);
				AssertEquals("Header text", "Stage", columns[6].HeaderText);
				AssertEquals("Header text", "Description", columns[7].HeaderText);
			}
		}

		protected override void SetUp()
		{
			overridenClientHook = ClientHookLoader.Instance.OverrideClientHookForTest(ClientOverride.Instance);
			TransactionedTestCase.RunClientDbCreateScripts();
			base.SetUp();
		}

		protected override void TearDown()
		{
			overridenClientHook.Dispose();
			base.TearDown();
		}

		IDisposable overridenClientHook;
	}
}
