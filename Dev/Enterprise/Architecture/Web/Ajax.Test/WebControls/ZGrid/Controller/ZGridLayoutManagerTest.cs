using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.ZGridInternals
{
	class ZGridLayoutManagerForTesting : ZGridLayoutManager
	{
		public ZGridLayoutManagerForTesting(ZGrid grid) : base(grid)
		{
		}

		internal string LoadGridLayoutForTesting(string layoutName = null) => LoadGridLayout(layoutName);
	}

	sealed class ZGridLayoutManagerTest : TestCaseWithFactory
	{
		public void TestColumnProviderNotNull()
		{
			AssertNotNull(TestGridLayoutManager.ColumnProvider);
		}

		[HttpContextEnabledTest]
		public void TestLoadGridLayoutMultiUsers()
		{
			var user = new OrgContactWebUser();

			var sharedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var sharedOrgUser1 = CreateNewContact(sharedOrg, "test1@test.com", "test");
			var sharedOrgUser2 = CreateNewContact(sharedOrg, "test2@test.com", "test");

			var otherOrgSameCompany = Factory.NewWithValidTestData<OrgHeader>();
			var otherOrgSameCompanyUser = CreateNewContact(otherOrgSameCompany, "test3@test.com", "test");

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_GC = otherCompany.PK;
			var otherOrgOtherCompany = Factory.NewWithValidTestData<OrgHeader>();
			otherOrgOtherCompany.CompanyData.OB_GC = otherCompany.PK;
			otherOrgOtherCompany.CompanyData.OB_GB_ControllingBranch = otherBranch.PK;
			var otherOrgOtherCompanyUser = CreateNewContact(otherOrgOtherCompany, "test4@test.com", "test");

			Factory.Save();

			user.Login(sharedOrg.OH_Code, "test1@test.com", "test");
			AssertEquals(sharedOrgUser1.PK, WebEnv.CurrentUser.PK);

			TestGridLayoutManager.GridLayout = "contact1, company shared, layout";
			TestGridLayoutManager.SaveAsGridLayout("TestLayout", false, true);

			TestGridLayoutManager.GridLayout = "contact1, organisation shared, layout";
			TestGridLayoutManager.SaveAsGridLayout("TestLayout", true, false);

			TestGridLayoutManager.GridLayout = "contact1, layout";
			TestGridLayoutManager.SaveAsGridLayout("TestLayout", false, false);

			AssertEquals("contact1, layout", TestGridLayoutManager.LoadGridLayoutForTesting("TestLayout"));

			LoginContact(sharedOrgUser2);
			AssertEquals("contact1, organisation shared, layout", TestGridLayoutManager.LoadGridLayoutForTesting("TestLayout"));

			LoginContact(otherOrgSameCompanyUser);
			AssertEquals("contact1, company shared, layout", TestGridLayoutManager.LoadGridLayoutForTesting("TestLayout"));

			LoginContact(otherOrgOtherCompanyUser);
			AssertEquals(null, TestGridLayoutManager.LoadGridLayoutForTesting("TestLayout"));
		}

		void LoginContact(OrgContact contact)
		{
			WebUser.Login(contact.OrgCode, contact.Email, contact.PasswordForTesting);
			WebEnv.AppInstance.Session.Clear();
			AssertEquals(contact.PK, WebEnv.CurrentUser.PK);
		}

		OrgContactWebUser WebUser => webUser ?? (webUser = new OrgContactWebUser());
		OrgContactWebUser webUser;

		OrgContact CreateNewContact(OrgHeader company, string email, string password)
		{
			OrgContact result = company.Contacts.AddNew();
			result.OC_ContactName = email;
			result.OC_Email = email;
			result.SetHashedPassword(password);
			result.OC_WebAccessEnabled = true;
			return result;
		}

		[HttpContextEnabledTest]
		public void TestSaveAsGridLayout()
		{
			TestGrid.Page = new TestPage();
			AssertNull(TestGridLayoutManager.LoadGridLayoutForTesting("TestLayout"));

			TestGridLayoutManager.GridLayout = "company, layout";
			TestGridLayoutManager.SaveAsGridLayout("TestLayout", false, true);
			AssertEquals("company, layout", TestGridLayoutManager.LoadGridLayoutForTesting("TestLayout"));

			TestGridLayoutManager.GridLayout = "organisation, layout";
			TestGridLayoutManager.SaveAsGridLayout("TestLayout", true, false);
			AssertEquals("organisation, layout", TestGridLayoutManager.LoadGridLayoutForTesting("TestLayout"));

			TestGridLayoutManager.GridLayout = "user, layout";
			TestGridLayoutManager.SaveAsGridLayout("TestLayout", false, false);
			AssertEquals("user, layout", TestGridLayoutManager.LoadGridLayoutForTesting("TestLayout"));

			TestGridLayoutManager.GridLayout = string.Empty;
			TestGridLayoutManager.SaveAsGridLayout("TestLayout", false, false);
			AssertEquals("organisation, layout", TestGridLayoutManager.LoadGridLayoutForTesting("TestLayout"));
		}

		[HttpContextEnabledTest]
		public void TestInvalidGridLayoutUsesDefault()
		{
			var provider = new GridColumnProvider();
			var defaultColumn = new ZTextEditColumn("defaultColumn", "whateverBindTo");
			provider.AddToDictionaryAsDefault(defaultColumn);
			TestGridLayoutManager.ColumnProvider = provider;
			TestGrid.Page = new TestPage();

			TestGridLayoutManager.GridLayout = "0,1,Y";
			AssertEquals(defaultColumn, TestGrid.Columns[0]);
			AssertEquals(1, TestGrid.Columns.Count);
		}

		public void TestRePopulateColumns()
		{
			GridColumnProvider provider = new GridColumnProvider();

			TestGrid.Columns.Add(new ZHyperLinkColumn("hyperlinkheader", "bindto"));
			TestGrid.Columns.Add(new ZHyperLinkColumn("hyperlinkheader2", "bindto2"));

			provider.AddButtonColumn("headertext", "name");
			TestGridLayoutManager.ColumnProvider = provider;

			TestGridLayoutManager.PopulateColumns();
			Assert(TestGrid.Columns.Count == 1);
			Assert(TestGrid.Columns[0] is ZButtonColumn);
			AssertEquals(TestGrid.Columns[0].HeaderText, "headertext");
		}

		public void TestRepopulateColumnsWithDefault()
		{
			GridColumnProvider provider = new GridColumnProvider();

			TestGrid.Columns.Add(new ZHyperLinkColumn("hyperlinkheader", "bindto"));
			TestGrid.Columns.Add(new ZHyperLinkColumn("hyperlinkheader2", "bindto2"));

			provider.AddToDictionary(new ZTextEditColumn("textHeader", "1textBindTo"));
			provider.AddToDictionaryAsDefault(new ZTextEditColumn("textHeader2", "2textBindTo"));

			TestGridLayoutManager.ColumnProvider = provider;
			TestGridLayoutManager.PopulateColumns();

			Assert(TestGrid.Columns.Count == 1);
			Assert(TestGrid.Columns[0] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[0].HeaderText, "textHeader2");
		}

		public void TestRepopulateColumnsWithDefaultAndRequired()
		{
			GridColumnProvider provider = new GridColumnProvider();

			TestGrid.Columns.Add(new ZHyperLinkColumn("hyperlinkheader", "bindto"));
			TestGrid.Columns.Add(new ZHyperLinkColumn("hyperlinkheader2", "bindto2"));

			provider.AddToDictionary(new ZTextEditColumn("textHeader", "1textBindTo"));
			provider.AddToDictionaryAsDefault(new ZTextEditColumn("textHeaderDef", "2textBindTo"));
			provider.AddToDictionaryAsRequired(new ZTextEditColumn("textHeaderReq", "2textBindTo"));

			TestGridLayoutManager.ColumnProvider = provider;
			TestGridLayoutManager.PopulateColumns();

			Assert(TestGrid.Columns.Count == 2);
			Assert(TestGrid.Columns[0] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[0].HeaderText, "textHeaderReq");
			Assert(TestGrid.Columns[1] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[1].HeaderText, "textHeaderDef");
		}

		[HttpContextEnabledTest]
		public void TestRequiredWillAppearWithSavedLayout()
		{
			GridColumnProvider provider = new GridColumnProvider();

			TestGrid.Columns.Add(new ZHyperLinkColumn("hyperlinkheader", "bindto"));
			TestGrid.Columns.Add(new ZHyperLinkColumn("hyperlinkheader2", "bindto2"));

			var col1 = new ZTextEditColumn("textHeader", "1textBindTo");
			var col2 = new ZTextEditColumn("textHeaderDef", "2textBindTo");
			provider.AddToDictionary(col1);
			provider.AddToDictionaryAsDefault(col2);

			TestGridLayoutManager.GridLayout = col1.UniqueKey + "," + col2.UniqueKey;
			TestGridLayoutManager.ColumnProvider = provider;
			TestGridLayoutManager.PopulateColumns();

			Assert(TestGrid.Columns.Count == 2);
			Assert(TestGrid.Columns[0] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[0].HeaderText, "textHeader");
			Assert(TestGrid.Columns[1] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[1].HeaderText, "textHeaderDef");

			provider.AddToDictionaryAsRequired(new ZTextEditColumn("textHeaderReq", "2textBindTo"));
			TestGridLayoutManager.PopulateColumns();
			Assert(TestGrid.Columns.Count == 3);
			Assert(TestGrid.Columns[0] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[0].HeaderText, "textHeaderReq");
			Assert(TestGrid.Columns[1] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[1].HeaderText, "textHeader");
			Assert(TestGrid.Columns[2] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[2].HeaderText, "textHeaderDef");
		}
		[HttpContextEnabledTest]
		public void TestRequiredAppearAtTheEnd()
		{
			GridColumnProvider provider = new GridColumnProvider();

			TestGrid.Columns.Add(new ZHyperLinkColumn("hyperlinkheader", "bindto"));
			TestGrid.Columns.Add(new ZHyperLinkColumn("hyperlinkheader2", "bindto2"));

			var col1 = new ZNewRowColumn("1textBindTo");
			var col2 = new ZTextEditColumn("textHeaderDef", "textBindTo");
			provider.AddToDictionary(col1);
			provider.AddToDictionaryAsDefault(col2);

			TestGridLayoutManager.GridLayout = col1.UniqueKey + "," + col2.UniqueKey;
			TestGridLayoutManager.ColumnProvider = provider;
			TestGridLayoutManager.PopulateColumns();

			Assert(TestGrid.Columns.Count == 2);
			Assert(TestGrid.Columns[0] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[0].HeaderText, "textHeaderDef");
			Assert(TestGrid.Columns[1] is ZNewRowColumn);

			provider.AddToDictionaryAsRequired(new ZTextEditColumn("textHeaderReq", "2textBindTo"));
			TestGridLayoutManager.PopulateColumns();
			Assert(TestGrid.Columns.Count == 3);
			Assert(TestGrid.Columns[0] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[0].HeaderText, "textHeaderReq");
			Assert(TestGrid.Columns[1] is ZTextEditColumn);
			AssertEquals(TestGrid.Columns[1].HeaderText, "textHeaderDef");
			Assert(TestGrid.Columns[2] is ZNewRowColumn);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestGrid = new ZGrid();
			TestGrid.ID = "TestGrid";
			TestGridLayoutManager = new ZGridLayoutManagerForTesting(TestGrid);
		}

		ZGridLayoutManagerForTesting TestGridLayoutManager;
		ZGrid TestGrid;

		class TestPage : ZPage
		{
			public TestPage()
				: base()
			{
			}

			protected override string GetPageName()
			{
				return "TestPage";
			}
		}
	}
}
