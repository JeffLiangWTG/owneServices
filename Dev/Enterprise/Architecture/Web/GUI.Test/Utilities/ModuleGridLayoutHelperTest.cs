using System;
using System.IO;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.Utilities.Testing
{
	[HttpContextEnabledTest]
	public class ModuleGridLayoutHelperTest : TestCaseWithFactory
	{
		#region TestCurrentGridLayout

		public void TestHelperCreatedByModuleID()
		{
			using (ModuleGridLayoutHelper helper = new ModuleGridLayoutHelper(WebModuleIDs.Dummy, Factory))
			{
				AssertHelper(helper);
			}
		}

		public void TestHelperCreatedBySearchControl()
		{
			using (var page = new ZPageForTest())
			using (var control = new SearchControlForTest())
			{
				control.SearchResultsDataGrid = new ZDataGrid();
				control.ModuleID = WebModuleIDs.Dummy;
				control.Page = page;
				control.SetGridLayoutControl(new ZGridLayoutControl());

				using (var helper = new ModuleGridLayoutHelper(control))
				{
					AssertHelper(helper);
				}
			}
		}

		void AssertHelper(ModuleGridLayoutHelper helper)
		{
			var layoutKey = helper.GetGridColumnsLayoutKey();

			AssertEquals("GridColumnsLayoutKey", WebModuleIDs.Dummy.ToString() + ".GridLayout", layoutKey);

			var layoutInSession = HttpContext.Current.Session[layoutKey] as string;
			GridLayoutRegistry registry = new GridLayoutRegistry();

			AssertNull("Pre-condition - No layout should be stored in Session", layoutInSession);
			AssertNull("Pre-condition - No layout in the Registry", registry.GetGridLayout(layoutKey, WebEnv.CurrentUser.PK.ToGuid()));

			helper.CurrentGridLayout = "0,1";

			layoutInSession = HttpContext.Current.Session[layoutKey] as string;
			AssertNotNull("CurrentLayout should be stored in Session", layoutInSession);
			AssertEquals("Value in Session should match assigned layout", "0,1", layoutInSession);

			using (MemoryStream stream = registry.GetGridLayout(layoutKey, WebEnv.CurrentUser.PK.ToGuid()))
			{
				TextReader reader = new StreamReader(stream);
				var layout = reader.ReadToEnd();
				AssertEquals("Value in the Registry should match assigned layout", "0,1", layout);
			}

			var columns = helper.GetGridColumns();
			AssertEquals("Should be two columns in the grid", 2, columns.Count);

			HttpContext.Current.Session[layoutKey] = null;

			AssertEquals("Layout should be converted for groups", "0,4", helper.CurrentGridLayout);

			helper.CurrentGridLayout = "0,bug,1";

			HttpContext.Current.Session[layoutKey] = null;

			AssertEquals("Layout should be empty", "", helper.CurrentGridLayout);
		}

		#region Classes For Test

		class ZPageForTest : ZPage, IRememberFilterCriteriaPage
		{
			protected override BusinessObject GetNewDataSource()
			{
				return new WebFilterBusinessObjectFactory(Factory).New<OrganisationFilterBusinessObject>();
			}

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}
		}

		class SearchControlForTest : SearchControl
		{
			public void SetGridLayoutControl(ZGridLayoutControl control)
			{
				base.GridLayoutControl = control;
			}
		}

		#endregion

		#endregion
	}
}
