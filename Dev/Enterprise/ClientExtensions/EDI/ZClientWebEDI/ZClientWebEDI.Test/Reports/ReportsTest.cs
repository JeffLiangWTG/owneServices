using System;
using System.Linq;
using System.Reflection;
using System.Web.UI.HtmlControls;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Web;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class ReportsTest : ZPageTestCase
	{
		public void TestValidate()
		{
			var page = Page as ReportsForTest;
			var methodInfo = typeof(ZPage).GetMethod("OnLoad", BindingFlags.NonPublic | BindingFlags.Instance);
			methodInfo.Invoke(page, new object[] { EventArgs.Empty });
			page.AppInstance.SiteUser.Login(Org.OH_Code, Contact.OC_Email, "1234");
			var ds = page.DataSource as WebReportHolder;
			ds.ReportPK = ds.WebReports.OfType<StmMenuItem>().First(x => x.SU_MenuName == "Reconciliation Report").PK;
			page.ReportsDropDownList_SelectedIndexChanged_Exposed();
			AssertEquals("Reconciliation Report", ds.SelectedReport.Name);
			var filter = ds.SelectedReport.FilterCollection["Importer"] as LookupField;
			filter.ZValueInfo.AddError("some error");
			AssertEquals(1, filter.Notifications.Count());
			page.Validate();
			AssertEquals(0, filter.Notifications.Count());
			ds.ReportPK = ZGuid.Empty;
		}

		OrgHeader Org;
		OrgContact Contact;
		protected override void SetUp()
		{
			base.SetUp();
			EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			Org = Factory.NewWithValidTestData<OrgHeader>();
			Org.OH_Code = "MEHMEH";
			Org.OH_FullName = "MEHMEH";
			Contact = Org.Contacts.AddNew();
			Contact.OC_ContactName = "CWSupport";
			Contact.OC_Email = "newuser@cargowise.com";
			Contact.OC_WebAccessEnabled = true;
			Contact.SetHashedPassword("1234");
			Factory.Save();
		}

		protected override ZPage GetNewZPage()
		{
			var page = new ReportsForTest();
			return page;
		}

		class ReportsForTest : Reports
		{
			public ReportsForTest()
			{
				FilterControlHolder = new HtmlGenericControl();
				FilterControlHolder.Page = this;
			}

			protected override ReportFilterControl GetNewFilterControl()
			{
				if (FilterControl == null)
				{
					FilterControl = new ReportFilterControlForTest();
				}

				FilterControl.Page = this;
				return FilterControl;
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			public void ReportsDropDownList_SelectedIndexChanged_Exposed() => ReportsDropDownList_SelectedIndexChanged(null, null);
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}

		class ReportFilterControlForTest : ReportFilterControl
		{
			public ReportFilterControlForTest()
			{
				FilterTable = new HtmlTable();
				ColumnConfigLabel = new ZTextLabel();
				ColumnConfigDropDownList = new ZDropDownList();
				ColumnConfigDIV = new HtmlGenericControl();
				SortDIV = new HtmlGenericControl();
				SortLabel = new ZTextLabel();
				SortOrderList = new ZRadioButtonList();
				GroupByDIV = new HtmlGenericControl();
				GroupByLabel = new ZTextLabel();
				GroupBysList = new ZRadioButtonList();
				PageBreakOnNewGroup = new ZCheckBox();
				OptionalTemplateDIV = new HtmlGenericControl();
				OptionalTemplateLabel = new ZTextLabel();
				OptionalTemplateList = new ZCheckBoxList();
				FormatTypeLabel = new ZTextLabel();
				FormatTypeDropDownList = new ZDropDownList();
				ReportLanguageLabel = new ZTextLabel();
				LanguageDropDownList = new ZDropDownList();
			}

			public void RunReportButton_ClickForTest()
			{
				RunReportButton_Click(this, EventArgs.Empty);
			}
		}
	}
}
