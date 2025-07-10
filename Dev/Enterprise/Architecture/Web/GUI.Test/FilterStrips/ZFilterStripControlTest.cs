using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using PopulatedFilterStripBizO = Enterprise.ZArchitecture.Web.Business.FilterStrips.Testing.FilterStripURLParameterHelperTest.FilterStripBusinessObjectForTest;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	sealed class ZFilterStripControlTest : TestCaseWithFactory
	{
		#region TestFilterStripBizO

		public void TestFilterStripBizO()
		{
			using (PageForTest page = new PageForTest())
			{
				page.OnLoad();
				using (var control = new ZFilterStripControl())
				{
					var dataSourceFilterStripBizO = page.DataSource as DummyFilterStripBusinessObject;
					control.Page = page;

					AssertEquals(dataSourceFilterStripBizO, control.FilterStripBizO);

					var anotherFilterStripBizO = new DummyFilterStripBusinessObject();
					control.FilterStripBizO = anotherFilterStripBizO;

					AssertNotEquals(dataSourceFilterStripBizO, control.FilterStripBizO);
					AssertEquals(anotherFilterStripBizO, control.FilterStripBizO);
				}
			}
		}

		#endregion

		#region TestGetFilterStripDataSourceReturnsNull

		public void TestGetFilterStripDataSourceReturnsNull()
		{
			using (PageForTest page = new PageForTest())
			{
				page.OnLoad();
				using (ZFilterStripControlForTest control = new ZFilterStripControlForTest())
				{
					DummyFilterStripBusinessObject dataSourceFilterStripBizO = page.DataSource as DummyFilterStripBusinessObject;
					control.Page = page;

					AssertEquals(0, control.HtmlTableExposed.Rows.Count);
					AssertEquals(0, dataSourceFilterStripBizO.FilterStrips.Count);

					FilterStrip filterStrip = dataSourceFilterStripBizO.FilterStrips.AddNew();
					control.AddFilterStripExposed(filterStrip.PK);

					AssertEquals(1, control.HtmlTableExposed.Rows.Count);

					try
					{
						ExceptionReporterTestListener.Instance.Enabled = false;

						control.AddFilterStripExposed(ZGuid.NewZGuid());

						AssertEquals(1, control.HtmlTableExposed.Rows.Count);

						AssertNull(control.GetFilterStripDataSourceExposed(ZGuid.NewZGuid()));
					}
					finally
					{
						ExceptionReporterTestListener.Instance.Enabled = true;
					}
				}
			}
		}
		#endregion

		#region TestResources

		public void TestResources()
		{
			using (PageForTest page = new PageForTest())
			{
				page.OnLoad();
				using (ZFilterStripControl control = new ZFilterStripControl())
				{
					string version = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(control.GetType().Assembly, typeof(AssemblyFileVersionAttribute))).Version;
					string expRuntimeDirectory = String.Format(@"/Runtime/Enterprise.ZArchitecture.Web.GUI/{0}/ZFilterGridModule/ZFilterStripGridModule/", version);
					AssertEquals("NavigationBarScript", String.Format("{0}{1}", expRuntimeDirectory.Replace(".", "_"), "help.png"), control.HelpImageResource.FileName);

					AssertNotNull("Resources", control.Resources);
					AssertEquals("Should contain 1 Resources", 1, control.Resources.Count);
					AssertCollectionContains("HelpImage should be in Resources", control.HelpImageResource, control.Resources);
				}
			}
		}
		#endregion

		#region Test OnLoad URL Parameter FilterStrip Setup

		[HttpContextEnabledTest]
		public void TestOnLoadWithNoQueryString()
		{
			AssertEquals("Incorrect test setup", "", HttpContext.Current.Request.QueryString.ToString());
			using (PageForTestWithPopulatedFilterStrip page = new PageForTestWithPopulatedFilterStrip())
			{
				page.OnLoad();
				using (ZFilterStripControlForTest control = new ZFilterStripControlForTest())
				{
					control.Page = page;
					PopulatedFilterStripBizO filterStripBizO = page.DataSource as PopulatedFilterStripBizO;
					filterStripBizO.LayoutsHelper = new FilterStripLayoutsHelperForWeb(filterStripBizO, page.SiteUser.LoggedInUser);

					control.OnPage_Load(page, EventArgs.Empty);
					AssertEquals("Should have no active Module Filters when Querystring is empty", 1, filterStripBizO.ActiveModuleFilters.Count);
				}
			}
		}

		[HttpContextEnabledTest]
		[QueryString("InvalidFilter1=123&InvalidFilter2=abc")]
		public void TestOnLoadWithNoValidFiltersInQueryString()
		{
			AssertEquals("Incorrect test setup", "InvalidFilter1=123&InvalidFilter2=abc", HttpContext.Current.Request.QueryString.ToString());
			using (PageForTestWithPopulatedFilterStrip page = new PageForTestWithPopulatedFilterStrip())
			{
				page.OnLoad();
				using (ZFilterStripControlForTest control = new ZFilterStripControlForTest())
				{
					control.Page = page;
					PopulatedFilterStripBizO filterStripBizO = page.DataSource as PopulatedFilterStripBizO;
					filterStripBizO.LayoutsHelper = new FilterStripLayoutsHelperForWeb(filterStripBizO, page.SiteUser.LoggedInUser);

					control.OnPage_Load(page, EventArgs.Empty);
					AssertEquals("Should have no active Module Filters when Querystring has no valid filters", 1, filterStripBizO.ActiveModuleFilters.Count);
				}
			}
		}

		[HttpContextEnabledTest]
		[QueryString("Filter1=123&Filter2=abc")]
		public void TestLayoutLoadingCannotCauseException()
		{
			AssertEquals("Test setup", "Filter1=123&Filter2=abc", HttpContext.Current.Request.QueryString.ToString());
			using (PageForTestWithPopulatedFilterStrip page = new PageForTestWithPopulatedFilterStrip())
			{
				page.OnLoad();
				using (ZFilterStripControlForTest control = new ZFilterStripControlForTest())
				{
					control.Page = page;
					PopulatedFilterStripBizO filterStripBizO = page.DataSource as PopulatedFilterStripBizO;
					filterStripBizO.LayoutsHelper = new FilterStripLayoutsHelperForWeb(filterStripBizO, page.SiteUser.LoggedInUser);

					bool exceptionWasFired = false;

					filterStripBizO.LayoutLoaded += (x, y) =>
													{
														exceptionWasFired = true;
														throw new SystemException();
													};

					control.OnPage_Load(page, EventArgs.Empty);
					Assert("The exception was thrown and successfully catched", exceptionWasFired);
				}
			}
		}

		[HttpContextEnabledTest]
		[QueryString("TestDateFilter=1-APR-08,2-JUN-08&TestTextFilter=abc")]
		public void TestOnLoadWithValidFiltersInQueryString()
		{
			AssertEquals("Incorrect test setup", "TestDateFilter=1-APR-08%2c2-JUN-08&TestTextFilter=abc", HttpContext.Current.Request.QueryString.ToString());
			using (PageForTestWithPopulatedFilterStrip page = new PageForTestWithPopulatedFilterStrip())
			{
				page.OnLoad();
				using (ZFilterStripControlForTest control = new ZFilterStripControlForTest())
				{
					control.Page = page;
					PopulatedFilterStripBizO filterStripBizO = page.DataSource as PopulatedFilterStripBizO;
					filterStripBizO.LayoutsHelper = new FilterStripLayoutsHelperForWeb(filterStripBizO, page.SiteUser.LoggedInUser);

					control.OnPage_Load(page, EventArgs.Empty);

					AssertEquals("Active Module Filters", 2, filterStripBizO.ActiveModuleFilters.Count);
					AssertEquals("Displayed FilterStrips", 2, filterStripBizO.FilterStrips.Count);
				}
			}
		}

		[HttpContextEnabledTest]
		[QueryString("InvalidFilter1=123&InvalidFilter2=abc&TestNkFilter=123")]
		public void TestOnLoadWithValidAndInvalidFilters()
		{
			AssertEquals("Incorrect test setup", "InvalidFilter1=123&InvalidFilter2=abc&TestNkFilter=123", HttpContext.Current.Request.QueryString.ToString());
			using (PageForTestWithPopulatedFilterStrip page = new PageForTestWithPopulatedFilterStrip())
			{
				page.OnLoad();
				using (ZFilterStripControlForTest control = new ZFilterStripControlForTest())
				{
					control.Page = page;
					PopulatedFilterStripBizO filterStripBizO = page.DataSource as PopulatedFilterStripBizO;
					filterStripBizO.LayoutsHelper = new FilterStripLayoutsHelperForWeb(filterStripBizO, page.SiteUser.LoggedInUser);

					control.OnPage_Load(page, EventArgs.Empty);

					AssertEquals("Active Module Filters", 1, filterStripBizO.ActiveModuleFilters.Count);
					AssertEquals("Displayed FilterStrips", 1, filterStripBizO.FilterStrips.Count);
				}
			}
		}

		#endregion

		#region TestUnsavedFilterRowIsOnlyVisibleIfAuthenticated

		public void TestUnsavedFilterRowIsOnlyVisibleIfAuthenticated()
		{
			using (ZPage testPage = new ZPage())
			{
				try
				{
					testPage.Controls.Add(Control);
					Control.fFilterWasChanged = true;
					Control.OnPage_PreRender();
					Assert("Label is not visible when User has not logged in", Control.AssertLayoutControlsVisibility(false));

					OrgHeader company = Factory.New<OrgHeader>();
					company.OH_Code = "XXXXX";
					Factory.Save();
					testPage.SiteUser.LoginSupportForTest(company.OH_Code);
					Control.fFilterWasChanged = true;
					Control.OnPage_PreRender();
					Assert("Label is visible when User has logged in", Control.AssertLayoutControlsVisibility(true));

					testPage.SiteUser.Logout();
					Control.fFilterWasChanged = true;
					Control.OnPage_PreRender();
					Assert("Label is not visible when User has logged out", Control.AssertLayoutControlsVisibility(false));
				}
				finally
				{
					testPage.Controls.Remove(Control);
				}
			}
		}

		#endregion

		public void TestLayoutsHelperInvalidCastException()
		{
			using (ZPage testPage = new ZPage())
			using (var control = new ZFilterStripControlForTest())
			{
				try
				{
					testPage.Controls.Add(control);
					control.fFilterWasChanged = true;
					control.OnPage_PreRender();
					control.FilterStripBizO = new DummyFilterStripBusinessObject();
					control.FilterStripBizO.LayoutsHelper = new FilterStripLayoutsHelper();
				}
				finally
				{
					AssertNoExceptionThrown(() =>
					{
						testPage.Controls.Remove(control);
					});
				}
			}
		}

		#region TestRegisterScriptForReturnKeyCapture

		public void TestRegisterScriptForReturnKeyCapture()
		{
			using (ZTestPage page = new ZTestPage())
			{
				page.OnLoad();
				using (ZFilterStripControlForTest control = new ZFilterStripControlForTest())
				{
					control.Page = page;

					ModuleFilterCollection moduleFilters = new ModuleFilterCollection();
					moduleFilters.AddCustomFilter(new ModuleTextFilter("text filter", DummyBizoSchema.Z0_Description));

					FilterStrip filterStrip = new FilterStrip(moduleFilters);
					filterStrip.FilterDescription = "text filter";
					control.AddFilterStrip(filterStrip);

					control.OnPage_PreRender();

					TextBox tb = new TextBox();
					ZPage.ClientFunctions.RegisterReturnKeyCapture(tb, control.Footer.FindButton);
					string expectedScript = tb.Attributes["onkeydown"];

					foreach (ZFilterStripRow strip in control.FilterStripRows)
					{
						foreach (Control controlToCaptureReturnKey in strip.ControlsToCaptureReturnKey)
						{
							string script = controlToCaptureReturnKey is WebControl ? ((WebControl)controlToCaptureReturnKey).Attributes["onkeydown"] : ((HtmlControl)controlToCaptureReturnKey).Attributes["onkeydown"];
							AssertEquals("Incorrect script for key capture", expectedScript, script);
						}
					}
				}
			}
		}

		#endregion

		[HttpContextEnabledTest]
		public void TestFooterControlsAreReused()
		{
			using (var testPage = new PageForTestWithPopulatedFilterStrip())
			{
				testPage.OnLoad();
				using (var control = new ZFilterStripControlForTest())
				{
					var filterStripBizO = testPage.DataSource as PopulatedFilterStripBizO;
					filterStripBizO.LayoutsHelper = new FilterStripLayoutsHelperForWeb(filterStripBizO, testPage.SiteUser.LoggedInUser);

					testPage.Controls.Add(control);
					control.OnPage_Load(testPage, EventArgs.Empty);
					AssertEquals("Precondition", 4, control.HtmlTableExposed.Rows.Count);

					var hr = control.HtmlTableExposed.Rows[1];
					var footer = control.HtmlTableExposed.Rows[2];
					var unsavedFilter = control.HtmlTableExposed.Rows[3];
					control.HtmlTableExposed.Rows.Remove(footer); //some unknown operation removes at least the footer row.

					testPage.IsPostBack = true;
					control.OnPage_Load(testPage, EventArgs.Empty);

					AssertEquals(4, control.HtmlTableExposed.Rows.Count);
					AssertEquals("HrRow should not be a new control for postbacks. Can get failed to load viewstate exception.", hr, control.HtmlTableExposed.Rows[1]);
					AssertEquals("Footer should not be a new control for postbacks. Can get failed to load viewstate exception.", footer, control.HtmlTableExposed.Rows[2]);
					AssertEquals("Unsaved Filter should not be a new control for postbacks. Can get failed to load viewstate exception.", unsavedFilter, control.HtmlTableExposed.Rows[3]);
				}
			}
		}

		ZFilterStripControlForTest Control
		{
			get
			{
				if (fControl == null)
				{
					fControl = new ZFilterStripControlForTest();
				}
				return fControl;
			}
		}
		ZFilterStripControlForTest fControl;
	}
}
