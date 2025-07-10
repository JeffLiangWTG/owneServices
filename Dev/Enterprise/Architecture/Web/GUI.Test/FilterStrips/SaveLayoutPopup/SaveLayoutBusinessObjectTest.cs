using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;
using Enterprise.ZArchitecture.Web.Business.FilterStrips.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	[TestedType(typeof(SaveLayoutBusinessObject))]
	sealed class SaveLayoutBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			fredLayout.S9_IsPublished = true;
			StmData data = TestDataHelper.NewLastUsedLayout(fredLayout, Contact.PK);
			Factory.Save();

			SaveLayoutBusinessObject bizO = new SaveLayoutBusinessObject(FilterStripBizO, TestUser);
			AssertEquals(bizO.LayoutName, "Fred");
			AssertEquals(bizO.IsPublished, true);
			AssertEquals(bizO.IsSavingColumns, false);
			AssertEquals(bizO.HasChanges, false);
			AssertEquals(bizO.FilterStripBizO, FilterStripBizO);
		}

		public void TestLayoutNameWhenFilterStripBizOIsNull()
		{
			var saveLayoutBizO = new SaveLayoutBusinessObject(null, TestUser);
			saveLayoutBizO.LayoutName = "Fred";
			AssertEquals("LayoutName should be as assigned", "Fred", saveLayoutBizO.LayoutName);
		}

		public void TestLayoutName()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			AssertEquals("Should be one Layout added", 1, FilterStripBizO.Layouts.Count);
			AssertEquals("It should be Fred Layout", "Fred", FilterStripBizO.Layouts[0].S9_FilterName);

			Assert("LayoutName should be empty by default", SaveLayoutBizO.LayoutName.IsEmpty);
			AssertNoErrors("LayoutName should have no Errors", SaveLayoutBizO.LayoutNameInfo);

			SaveLayoutBizO.LayoutName = "Fred";
			AssertEquals("LayoutName should be as assigned", "Fred", SaveLayoutBizO.LayoutName);
			AssertNoErrors("LayoutName should have no Errors", SaveLayoutBizO.LayoutNameInfo);

			SaveLayoutBizO.LayoutName = "Brett";
			AssertEquals("LayoutName should be as assigned", "Brett", SaveLayoutBizO.LayoutName);
			AssertNoErrors("LayoutName should have no Errors", SaveLayoutBizO.LayoutNameInfo);

			fredLayout.S9_IsSystem = true;
			Factory.Save();

			//Reset BizO to force it reload from DB
			fSaveLayoutBizO = null;
			fFilterStripBizO = null;

			SaveLayoutBizO.LayoutName = "Fred";
			SaveLayoutBizO.IsPublished = true;
			SaveLayoutBizO.RunPreSaveValidation();
			AssertEquals("LayoutName should be as assigned", "Fred", SaveLayoutBizO.LayoutName);
			AssertHasError("LayoutName should have the Error", SaveLayoutBizO.LayoutNameInfo, "System Layout cannot be overridden.\r\nPlease enter a different name to save Layout.");

			SaveLayoutBizO.LayoutName = "Brett";
			AssertEquals("LayoutName should be as assigned", "Brett", SaveLayoutBizO.LayoutName);
			AssertNoErrors("LayoutName should have no Errors", SaveLayoutBizO.LayoutNameInfo);

			CombineAssertions(() =>
			{
				SaveLayoutBizO.LayoutName = "[Brett";
				AssertEquals("LayoutName should be as assigned", "[Brett", SaveLayoutBizO.LayoutName);
				AssertHasError("LayoutName should have errors", SaveLayoutBizO.LayoutNameInfo, "The layout name cannot start or end with square brackets.");

				SaveLayoutBizO.LayoutName = "Brett]";
				AssertEquals("LayoutName should be as assigned", "Brett]", SaveLayoutBizO.LayoutName);
				AssertHasError("LayoutName should have errors", SaveLayoutBizO.LayoutNameInfo, "The layout name cannot start or end with square brackets.");

				SaveLayoutBizO.LayoutName = "[Brett]";
				AssertEquals("LayoutName should be as assigned", "[Brett]", SaveLayoutBizO.LayoutName);
				AssertHasError("LayoutName should have errors", SaveLayoutBizO.LayoutNameInfo, "The layout name cannot start or end with square brackets.");
			});
		}

		public void TestIsPublished()
		{
			AssertEquals("Precondition: Current User should be allowed to publish layouts", true, SaveLayoutBizO.CanPublishLayouts);
			AssertEquals("IsPublished should be False by default", false, SaveLayoutBizO.IsPublished);
			AssertNoErrors("IsPublished should have no Errors", SaveLayoutBizO.IsPublishedInfo);

			SaveLayoutBizO.IsPublished = true;
			AssertEquals("IsPublished should be as assigned", true, SaveLayoutBizO.IsPublished);
			AssertNoErrors("IsPublished should have no Errors", SaveLayoutBizO.IsPublishedInfo);

			SaveLayoutBizO.IsPublished = false;
			AssertEquals("IsPublished should be as assigned", false, SaveLayoutBizO.IsPublished);
			AssertNoErrors("IsPublished should have no Errors", SaveLayoutBizO.IsPublishedInfo);

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.SecurityRightsForBindingOnly.DenyAll();
			contact.OC_ContactName = "Fred";
			contact.OC_Email = "fred@wisetechglobal.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			TestUser.Login(contact.OrganisationCode, contact.OC_Email, "1234");
			AssertEquals(true, TestUser.IsLoggedIn);
			AssertEquals("Current User should not be allowed to publish layouts", false, SaveLayoutBizO.CanPublishLayouts);

			SaveLayoutBizO.IsPublished = true;
			AssertEquals("IsPublished should be as assigned", true, SaveLayoutBizO.IsPublished);
			AssertHasError("LayoutName should have the Error", SaveLayoutBizO.IsPublishedInfo, "You do not have permissions to save or override Published Layout.");

			SaveLayoutBizO.IsPublished = false;
			AssertEquals("IsPublished should be as assigned", false, SaveLayoutBizO.IsPublished);
			AssertNoErrors("IsPublished should have no Errors", SaveLayoutBizO.IsPublishedInfo);
		}

		public void TestIsPublishedForCompany()
		{
			TestUserMock.Setup(m => m.CanPublishCompanyLayouts)
				.Returns(false);
			AssertEquals(false, SaveLayoutBizO.CanPublishCompanyLayouts);
			AssertEquals(false, SaveLayoutBizO.IsPublishedForCompany);
			AssertNoErrors(SaveLayoutBizO.IsPublishedForCompanyInfo);

			SaveLayoutBizO.IsPublishedForCompany = true;

			AssertEquals("IsPublishedForCompany should be as assigned", true, SaveLayoutBizO.IsPublishedForCompany);
			AssertHasError("LayoutName should have the Error", SaveLayoutBizO.IsPublishedForCompanyInfo, "You do not have permissions to save or override Company Layout.");

			//reset
			fTestUserMock = null;
			fSaveLayoutBizO = null;

			TestUserMock.Setup(m => m.CanPublishCompanyLayouts)
				.Returns(false);
			AssertNoErrors(SaveLayoutBizO.IsPublishedInfo);
		}

		public void TestAddRowErrorIfFilterStripBizObjIsNull()
		{
			var saveLayoutBizO = new SaveLayoutBusinessObject(null, TestUser);
			saveLayoutBizO.RunPreSaveValidation();
			AssertEquals("should not have a row error", false, saveLayoutBizO.HasRowErrors);
			AssertEquals("should not have a row error", false, saveLayoutBizO.HasErrors);
		}

		public void TestAddRowErrorIfFilterStripBizObjHasErrors()
		{
			ZArchitecture.GUI.Testing.DummyWithCodeDescriptionPairListCollection list = new ZArchitecture.GUI.Testing.DummyWithCodeDescriptionPairListCollection(Factory);
			ModuleCodeFilterTest.DummyModuleCodeFilter testFilter = new ModuleCodeFilterTest.DummyModuleCodeFilter("moo", DummyBizoSchema.Z0_Description, list, DummyBizoSchema.Z0_Code, list);
			testFilter.IsActive = true;
			testFilter.Property1 = "~~";

			AssertHasErrors(testFilter.Property1Info);

			FilterStripBizO.AddModuleFilterForTest(testFilter);
			FilterStripBizO.RegisterEditableChildObject(testFilter);

			SaveLayoutBizO.RunPreSaveValidation();
			AssertEquals("should have a row error", true, SaveLayoutBizO.HasRowErrors);
			AssertEquals("should have a row error", true, SaveLayoutBizO.HasErrors);
			AssertHasRowError(SaveLayoutBizO, SaveLayoutBusinessObject.FilterStripBizErrorMessage);
		}

		public void TestRunPreSaveValidation()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			AssertEquals("Should be one Layout added", 1, FilterStripBizO.Layouts.Count);
			AssertEquals("It should be Fred Layout", "Fred", FilterStripBizO.Layouts[0].S9_FilterName);

			SaveLayoutBizO.RunPreSaveValidation();
			AssertNoErrors("Should have no Errors", SaveLayoutBizO);

			fredLayout.S9_IsSystem = true;
			fredLayout.S9_IsPublished = true;
			Factory.Save();

			//Reset BizO to force it reload from DB
			fSaveLayoutBizO = null;
			fFilterStripBizO = null;

			SaveLayoutBizO.LayoutName = "Fred";
			SaveLayoutBizO.IsPublished = true;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.SecurityRightsForBindingOnly.DenyAll();
			contact.OC_ContactName = "Fred";
			contact.OC_Email = "fred@wisetechglobal.com";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("1234");
			Factory.Save();
			TestUser.Login(contact.OrganisationCode, contact.OC_Email, "1234");
			AssertEquals(true, TestUser.IsLoggedIn);
			AssertEquals("Current User should not be allowed to publish layouts", false, SaveLayoutBizO.CanPublishLayouts);

			AssertNoErrors("Should have no Errors", SaveLayoutBizO);

			SaveLayoutBizO.RunPreSaveValidation();
			AssertHasError("LayoutName should have the Error", SaveLayoutBizO.LayoutNameInfo, "System Layout cannot be overridden.\r\nPlease enter a different name to save Layout.");
			AssertHasError("LayoutName should have the Error", SaveLayoutBizO.IsPublishedInfo, "You do not have permissions to save or override Published Layout.");
		}

		public void TestRunPreSaveValidation_CompanyLayout()
		{
			StmModuleFilter companyLayout = TestDataHelper.NewPublishedCompanyLayoutForWeb("Company", EnvProxy.Instance.CurrentCompany.PK);
			Factory.Save();

			TestUserMock.Setup(m => m.CanPublishLayouts)
				.Returns(true);
			TestUserMock.Setup(m => m.CanPublishCompanyLayouts)
				.Returns(false);

			SaveLayoutBizO.LayoutName = "Company";
			SaveLayoutBizO.IsPublishedForCompany = true;
			SaveLayoutBizO.RunPreSaveValidation();
			AssertHasError("LayoutName should have the Error", SaveLayoutBizO.LayoutNameInfo, "Company Layout cannot be overridden.\r\nPlease enter a different name to save Layout.");
			AssertHasError("LayoutName should have the Error", SaveLayoutBizO.IsPublishedForCompanyInfo, "You do not have permissions to save or override Company Layout.");

			//reset
			fSaveLayoutBizO = null;
			fTestUserMock = null;

			TestUserMock.Setup(m => m.CanPublishLayouts)
				.Returns(true);
			TestUserMock.Setup(m => m.CanPublishCompanyLayouts)
				.Returns(true);

			SaveLayoutBizO.LayoutName = "Company";
			SaveLayoutBizO.IsPublishedForCompany = true;
			SaveLayoutBizO.RunPreSaveValidation();
			AssertNoErrors("Should have no Errors", SaveLayoutBizO);
		}

		protected override void SetUp()
		{
			Globals.IsWeb = true;
		}

		#region Implementation

		OrgContactWebUser TestUser
		{
			get
			{
				return TestUserMock.Object;
			}
		}

		Mock<OrgContactWebUser> TestUserMock
		{
			get
			{
				if (fTestUserMock == null)
				{
					fTestUserMock = new Mock<OrgContactWebUser>();
					fTestUserMock.CallBase = true;
					fTestUserMock.Object.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				}

				return fTestUserMock;
			}
		}

		Mock<OrgContactWebUser> fTestUserMock;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SaveLayoutBusinessObject(FilterStripBizO, TestUser);
		}

		SaveLayoutBusinessObject SaveLayoutBizO
		{
			get { return fSaveLayoutBizO ?? (fSaveLayoutBizO = new SaveLayoutBusinessObject(FilterStripBizO, TestUser)); }
		}

		DummyFilterStripBusinessObjectForWeb FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = new DummyFilterStripBusinessObjectForWeb();
					fFilterStripBizO.LayoutsHelper = new FilterStripLayoutsHelperForWeb(fFilterStripBizO, Contact);
				}
				return fFilterStripBizO;
			}
		}

		OrgContact Contact
		{
			get { return fContact ?? (fContact = Factory.NewWithValidTestData<OrgContact>()); }
		}

		LayoutsTestDataHelper TestDataHelper
		{
			get { return fTestDataHelper ?? (fTestDataHelper = new LayoutsTestDataHelper(Factory)); }
		}

		SaveLayoutBusinessObject fSaveLayoutBizO;
		DummyFilterStripBusinessObjectForWeb fFilterStripBizO;
		OrgContact fContact;
		LayoutsTestDataHelper fTestDataHelper;

		#endregion
	}
}
