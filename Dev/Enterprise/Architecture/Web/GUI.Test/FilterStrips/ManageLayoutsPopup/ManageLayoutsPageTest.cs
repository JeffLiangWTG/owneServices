using System;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;
using Enterprise.ZArchitecture.Web.Business.FilterStrips.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	sealed class ManageLayoutsPageTest : ZIFramePageTest
	{
		protected override System.Web.UI.Control GetNewControl()
		{
			return new DummyManageLayoutsPage();
		}

		DummyManageLayoutsPage TestManageLayoutsPage
		{
			get
			{
				return (DummyManageLayoutsPage)Control;
			}
		}

		public void TestPublishedLayoutsNeedPermissionToDelete()
		{
			Globals.IsWeb = true;
			ZWebTestHelper helper = new ZWebTestHelper(Factory);
			Factory.Save();

			TestManageLayoutsPage.SiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			Assert("User should be logged in", TestManageLayoutsPage.SiteUser.IsLoggedIn);
			Assert("Precondition: User should not be granted to publish layouts", !((OrgContactWebUser)TestManageLayoutsPage.SiteUser).CanPublishLayouts);

			TestManageLayoutsPage.LoadForTest();

			StmModuleFilter publishedLayout = TestManageLayoutsPage.FilterStripBizO.Layouts.AddNew();
			publishedLayout.S9_FilterName = "published";
			publishedLayout.S9_IsPublished = true;
			publishedLayout.S9_RelatedEntityID = helper.TestOrg.PK;

			StmModuleFilter unpublishedLayout = TestManageLayoutsPage.FilterStripBizO.Layouts.AddNew();
			unpublishedLayout.S9_FilterName = "unpublished";
			unpublishedLayout.S9_IsPublished = false;
			unpublishedLayout.S9_RelatedEntityID = helper.TestContact.PK;

			StmModuleFilter companyLayout = TestManageLayoutsPage.FilterStripBizO.Layouts.AddNew();
			companyLayout.S9_FilterName = "company";
			companyLayout.S9_IsPublished = true;
			companyLayout.S9_RelatedEntityID = ZGuid.Empty;
			companyLayout.S9_GC = EnvProxy.Instance.CurrentCompany.PK;

			//as ZDBOnlyQuery is used, make sure to save the collection
			TestManageLayoutsPage.FilterStripBizO.Layouts.Factory.Save();

			TestManageLayoutsPage.DeleteLayout(publishedLayout);
			TestManageLayoutsPage.DeleteLayout(unpublishedLayout);
			TestManageLayoutsPage.DeleteLayout(companyLayout);
			((ManageLayoutsBusinessObject)TestManageLayoutsPage.DataSource).DeleteFilterLayoutsMarkedForDelete();

			Assert("Unpublished layout should be deleted", unpublishedLayout.IsDeleted);
			Assert("Published layout should not be deleted", !publishedLayout.IsDeleted);
			Assert("Company layout should not be deleted", !companyLayout.IsDeleted);

			TestManageLayoutsPage.SiteUser.Logout();
			TestManageLayoutsPage.SiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			foreach (OrgSecurityContacts right in ((OrgContactWebUser)TestManageLayoutsPage.SiteUser).LoggedInUser.SecurityRightsForBindingOnly)
			{
				if (right.SecurityItemName == WebSecurityRightsList.WebPublishLayouts.Code)
				{
					right.OZ_Granted = true;
				}
			}

			TestManageLayoutsPage.DeleteLayout(publishedLayout);
			TestManageLayoutsPage.DeleteLayout(companyLayout);
			((ManageLayoutsBusinessObject)TestManageLayoutsPage.DataSource).DeleteFilterLayoutsMarkedForDelete();

			Assert("Published layout should be deleted", publishedLayout.IsDeleted);
			Assert("Company layout should not be deleted", !companyLayout.IsDeleted);

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = helper.TestOrg.PK;
			TestManageLayoutsPage.DeleteLayout(companyLayout);
			((ManageLayoutsBusinessObject)TestManageLayoutsPage.DataSource).DeleteFilterLayoutsMarkedForDelete();
			Assert("Company layout should be deleted", companyLayout.IsDeleted);
		}

		#region UselessTests

		public override void TestButtonsContainer()
		{
			Assert(true);
		}

		public override void TestCancelFunctionArguments()
		{
			Assert(true);
		}

		public override void TestGetNewDataSource()
		{
			Assert(true);
		}

		public override void TestOKButton()
		{
			Assert(true);
		}

		public override void TestOKButtonClick()
		{
			Assert(true);
		}

		public override void TestOKFunctionArguments()
		{
			Assert(true);
		}

		#endregion

		class DummyManageLayoutsPage : ManageLayoutsPage
		{
			public DummyManageLayoutsPage()
			{
				DeleteButton = new ZButton();
				RenameButton = new ZButton();
				FilterLayoutsListBox = new ZListBox
				{
					BindToList = "FilterLayouts",
					BindTo = "SelectedFilterLayoutName"
				};
				HiddenFilterNameInput = new HtmlInputHidden();
			}

			public void LoadForTest()
			{
				base.OnLoad(EventArgs.Empty);
			}

			protected override BusinessObject GetNewDataSource()
			{
				return new ManageLayoutsBusinessObject(FilterStripBizO, Page.SiteUser);
			}

			public new DummyFilterStripBusinessObjectForWeb FilterStripBizO
			{
				get
				{
					if (fFilterStripBizO == null)
					{
						fFilterStripBizO = new DummyFilterStripBusinessObjectForWeb();
						if (SiteUser.IsLoggedIn)
						{
							fFilterStripBizO.LayoutsHelper = new FilterStripLayoutsHelperForWeb(fFilterStripBizO, SiteUser.LoggedInUser, WebDataRegistry.Instance.DefaultFilterLayoutForwardingBookings);
						}
					}
					return fFilterStripBizO;
				}
			}
			DummyFilterStripBusinessObjectForWeb fFilterStripBizO;

			public void DeleteLayout(StmModuleFilter layout)
			{
				DeleteFilterLayout(layout);
			}
		}
	}
}
