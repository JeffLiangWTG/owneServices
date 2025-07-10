using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Enterprise.Client.EDI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class SwitchCompanyTest : ZPageLifeCycleTest
	{
		public void TestDataSource()
		{
			using (var page = PageForTest)
			{
				page.LoadForTest();
				page.SiteUser.Login(TestContact.OrganisationCode, TestContact.OC_Email, Password);
				AssertEquals("Precondition", true, page.SiteUser.IsLoggedIn);
				AssertNotNull("This is bound to CurrentCompanyLabel", ZPropertyAccessor.Get(page.DataSource, "SiteUser.LoggedInOrgContact.WorkingAddressCompanyName"));
				AssertNotNull("This is bound to CompaniesDataGrid", ZPropertyAccessor.Get(page.DataSource, "SiteUser.AllUserRelatedOrgs"));
			}
		}

		public void TestSetupCompaniesGrid()
		{
			using (var page = PageForTest)
			{
				page.SetupCompaniesGridForTest();
				AssertEquals("RelatedContactsGrid should contain 3 columns", 3, page.RelatedContactsDataGridForTest.Columns.Count);
				AssertEquals("First Column Title", "Company Code", page.RelatedContactsDataGridForTest.Columns[0].HeaderText);
				AssertEquals("Second Column Title", "Company Name", page.RelatedContactsDataGridForTest.Columns[1].HeaderText);
				AssertEquals("Third Column Title", "Email", page.RelatedContactsDataGridForTest.Columns[2].HeaderText);
			}
		}

		void SetupOIDCUsers(out OrgContact oidcContact01, out OrgContact oidcContact02, out OrgContact contact)
		{
			var oidcOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var oidcOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			oidcOrg1.OH_Code = "oidc01";
			oidcOrg2.OH_Code = "oidc02";
			org1.OH_Code = "normalOrg01";

			oidcOrg1.OH_FullName = oidcOrg1.OH_Code;
			oidcOrg2.OH_FullName = oidcOrg2.OH_Code;
			org1.OH_FullName = org1.OH_Code;
			Factory.Save();

			TestContact.OC_Email = "oidc@123.com";
			oidcContact01 = oidcOrg1.Contacts.AddNew();
			oidcContact02 = oidcOrg2.Contacts.AddNew();
			contact = org1.Contacts.AddNew();

			oidcContact01.OC_Email = TestContact.OC_Email;
			oidcContact02.OC_Email = TestContact.OC_Email;
			contact.OC_Email = TestContact.OC_Email;

			oidcContact01.OC_PER = TestContact.PK;
			oidcContact02.OC_PER = TestContact.OC_PER;
			contact.OC_PER = TestContact.OC_PER;

			oidcContact01.SetHashedPassword(Password);
			oidcContact02.SetHashedPassword(Password);
			contact.SetHashedPassword(Password);

			oidcContact01.OC_WebAccessEnabled = true;
			oidcContact02.OC_WebAccessEnabled = true;
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			EDIDataRegistry.Instance.RedirectedEmailDomains.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { "@123.com" });
			EDIDataRegistry.Instance.RedirectedOrganisations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { oidcOrg1.PK.ToGuid(), oidcOrg2.PK.ToGuid() });
		}

		void LoginViaOIDC(ZPage page, OrgContact orgContact)
		{
			HttpContext.Current.Session[OIDCLoginHelper.Constants.OIDCSessionKey] = true;
			page.SiteUser.Login(orgContact.OrganisationCode, orgContact.OC_Email, Enterprise.ZArchitecture.Environment.User.WebTransientPassword);
		}

		public void TestOIDCFeature_NormalUserToOIDC()
		{
			SetupOIDCUsers(out var oidcContact01, out _, out var contact);
			var items = new OrgContactCollection(Factory) { oidcContact01, contact };
			using (var page = PageForTest)
			{
				page.SiteUser.Login(contact.OrganisationCode, contact.OC_Email, Password);
				Assert(page.SiteUser.IsLoggedIn);
				page.LoadForTest();
				page.SetupCompaniesGridForTest();
				page.RelatedContactsDataGridForTest.Bind(items);
				page.InvokeSwitchCompanyCommandForTest(page.RelatedContactsDataGridForTest.Items[0]);
				Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				Assert(HttpContext.Current.Response.RedirectLocation.Contains($"?{MyAccountLoginHelper.RefKey}={page.DataSourceIndexer}"));
				Assert(!page.SiteUser.IsLoggedIn);
			}
		}

		public void TestOIDCFeature_OIDCToNormalUser()
		{
			SetupOIDCUsers(out var oidcContact01, out _, out var contact);
			var items = new OrgContactCollection(Factory) { oidcContact01, contact };
			using (var page = PageForTest)
			{
				LoginViaOIDC(page, oidcContact01);
				Assert(page.SiteUser.IsLoggedIn);
				page.LoadForTest();
				page.SetupCompaniesGridForTest();
				page.RelatedContactsDataGridForTest.Bind(items);
				page.InvokeSwitchCompanyCommandForTest(page.RelatedContactsDataGridForTest.Items[1]);
				Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				Assert(HttpContext.Current.Response.RedirectLocation.Contains($"?{MyAccountLoginHelper.RefKey}={page.DataSourceIndexer}"));
				Assert(!page.SiteUser.IsLoggedIn);
			}
		}

		public void TestOIDCFeature_OIDCClickedSelf()
		{
			SetupOIDCUsers(out var oidcContact01, out _, out _);
			var items = new OrgContactCollection(Factory) { oidcContact01 };
			using (var page = PageForTest)
			{
				LoginViaOIDC(page, oidcContact01);
				Assert(page.SiteUser.IsLoggedIn);
				page.LoadForTest();
				page.SetupCompaniesGridForTest();
				page.RelatedContactsDataGridForTest.Bind(items);
				page.InvokeSwitchCompanyCommandForTest(page.RelatedContactsDataGridForTest.Items[0]);
				Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				Assert(!HttpContext.Current.Response.RedirectLocation.Contains($"?{MyAccountLoginHelper.RefKey}={page.DataSourceIndexer}"));
				Assert(page.SiteUser.IsLoggedIn);
				AssertEquals(oidcContact01.OC_OH, page.SiteUser.CurrentOrg);
			}
		}

		public void TestOIDCFeature_OIDCToOIDC()
		{
			SetupOIDCUsers(out var oidcContact01, out var oidcContact02, out _);
			var items = new OrgContactCollection(Factory) { oidcContact01, oidcContact02 };
			using (var page = PageForTest)
			{
				LoginViaOIDC(page, oidcContact01);
				Assert(page.SiteUser.IsLoggedIn);
				page.LoadForTest();
				page.SetupCompaniesGridForTest();
				page.RelatedContactsDataGridForTest.Bind(items);
				page.InvokeSwitchCompanyCommandForTest(page.RelatedContactsDataGridForTest.Items[1]);
				Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				Assert(!HttpContext.Current.Response.RedirectLocation.Contains($"?{MyAccountLoginHelper.RefKey}={page.DataSourceIndexer}"));
				Assert(page.SiteUser.IsLoggedIn);
				AssertEquals(oidcContact02.OC_OH, page.SiteUser.CurrentOrg);
			}
		}

		public void TestHighlightRowForCurrentCompany()
		{
			using (var page = PageForTest)
			{
				page.LoadForTest();
				page.LoginManForTest.CompanyCode = "ABC";
				AssertHighlightRowForCurrentCompany(page, "ABC", true, "Row should be highlighted");
				AssertHighlightRowForCurrentCompany(page, "DFG", false, "Row should not be highlighted");
			}
		}

		#region TestHighlightRowForCurrentCompany
		public void TestSwitchCompany_OnFailure_RedirectsToLoginWithSessionRefParameter()
		{
			var item = Factory.NewWithValidTestData<OrgContact>();
			var items = new OrgContactCollection(Factory) { item };
			using (var page = PageForTest)
			{
				page.LoadForTest();
				page.SetupCompaniesGridForTest();
				page.RelatedContactsDataGridForTest.Bind(items);
				page.InvokeSwitchCompanyCommandForTest(page.RelatedContactsDataGridForTest.Items[0]);
				Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
				Assert(HttpContext.Current.Response.RedirectLocation.Contains($"?{MyAccountLoginHelper.RefKey}={page.DataSourceIndexer}"));
			}
		}

		void AssertHighlightRowForCurrentCompany(SwitchCompanyForTest page, string code, bool expectedValue, string message)
		{
			OrgHeader org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			org.OH_Code = code;
			DataGridItem row = new DataGridItem(0, 0, ListItemType.Item);
			row.DataItem = contact;
			var cell = new TableCell { BackColor = Color.White };
			row.Cells.Add(cell);
			page.HighlightRowForCurrentCompanyForTest(row);
			AssertEquals("Precondition: Should have one cell in the row", 1, row.Cells.Count);
			AssertEquals(message, expectedValue, row.Cells.Cast<TableCell>().All(x => x.BackColor == Color.FromArgb(230, 255, 230)));
		}

		#endregion
		#region Implementation
		OrgContact TestContact { get; set; }

		string Password { get; set; }

		protected override void SetUp()
		{
			base.SetUp();
			Password = "ChangeMe123";
			TestContact = Factory.NewWithValidTestData<OrgContact>();
			TestContact.SetHashedPassword(Password);
			TestContact.OC_WebAccessEnabled = true;
			Factory.Save();
		}

		SwitchCompanyForTest PageForTest
		{
			get
			{
				var testPage = new SwitchCompanyForTest();
				var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
				method.Invoke(testPage, new object[] { HttpContext.Current });
				return testPage;
			}
		}

		#region SwitchCompanyForTest
		class SwitchCompanyForTest : SwitchCompany
		{
			public void SetupCompaniesGridForTest()
			{
				RelatedContactsDataGrid = new ZDataGrid();
				Controls.Add(RelatedContactsDataGrid);
				SetupCompaniesGrid();
			}

			public ZDataGrid RelatedContactsDataGridForTest
			{
				get
				{
					return RelatedContactsDataGrid;
				}
			}

			public LoginManager LoginManForTest
			{
				get
				{
					return LoginMan;
				}
			}

			public ZTextLabel CurrentCompanyLabelForTest
			{
				get
				{
					return CurrentCompanyLabel;
				}
			}

			public void HighlightRowForCurrentCompanyForTest(DataGridItem row)
			{
				HighlightRowForCurrentCompany(row);
			}

			public void InvokeSwitchCompanyCommandForTest(DataGridItem item)
			{
				CompaniesDataGrid_ItemCommand(this, new DataGridCommandEventArgs(item, this, new CommandEventArgs(CmdSwitchCompany, null)));
			}

			public void LoadForTest()
			{
				OnLoad(EventArgs.Empty);
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}
		}

		#endregion
		#endregion
		protected override ZPage GetNewPage()
		{
			return PageForTest;
		}
	}
}
