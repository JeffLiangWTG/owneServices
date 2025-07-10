using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.Modules;
using static Enterprise.ZArchitecture.Web.GUI.WebControls.ZDocAddressControl;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	public class ZDocAddressControlTest : TestCaseWithFactory
	{
		public void TestResidentialAddress()
		{
			Assert("ShowResidentialAddressOnOverride is false by default", !TestControl.ShowResidentialAddressOnOverride);

			TestControl.OnInitForTesting(EventArgs.Empty);
			Assert("Residential Address CheckBox is not visible by default", !TestControl.ResidentialCheckBoxForTesting.Visible);

			TestControl.ShowResidentialAddressOnOverride = true;
			TestControl.OnInitForTesting(EventArgs.Empty);

			TestControl.OverrideCheckBoxForTesting.Checked = true;
			TestControl.OverrideCheckBox_CheckedChanged(TestControl.OverrideCheckBoxForTesting, EventArgs.Empty);

			Assert("Residential Address CheckBox should be visible", TestControl.ResidentialCheckBoxForTesting.Visible);
		}

		public void TestInitializeComponents()
		{
			AssertNotNull("Should be not null", TestControl);

			TestControl.Caption = "Test Caption";
			TestControl.CaptionCssClass = "TestCssClass";
			TestControl.AddressCaption = "Test Address Caption";
			TestControl.ContactCaption = "Test Contact Caption";
			TestControl.OrgModuleID = WebModuleIDs.OrganisationTracking;

			TestControl.OnInitForTesting(EventArgs.Empty);

			AssertEquals("Caption", "Test Caption", TestControl.DocAddressLabelForTesting.Text);
			AssertEquals("Caption CssClass", "TestCssClass", TestControl.DocAddressLabelForTesting.CssClass);
			AssertEquals("Address Caption", "Test Address Caption", TestControl.AddressLabelForTesting.Text);
			AssertEquals("Address Override Caption", "Test Address Caption", TestControl.AddressOverrideLabelForTesting.Text);
			AssertEquals("Contact Caption", "Test Contact Caption", TestControl.ContactLabelForTesting.Text);
			AssertEquals("Contact Override Caption", "Test Contact Caption", TestControl.ContactOverrideLabelForTesting.Text);
			AssertEquals("OrgModuleID", WebModuleIDs.OrganisationTracking, TestControl.OrgFindBoxForTesting.ModuleID);
		}

		public void TestDataBinding()
		{
			AssertNull("Precondition: DocAddress should be null", TestControl.DocAddress);
			AssertNull("Precondition: BindTo should be null", TestControl.BindTo);
			AssertEquals("IsBindable should be false", false, TestControl.IsBindable(null));
			AssertEquals("IsBindable should be true", true, TestControl.IsBindable(TestDataSource));

			TestControl.BindTo = "JobDocAddress";
			TestControl.BindToOrgList = "OrgList";
			AssertEquals("IsBindable should be false", false, TestControl.IsBindable(null));
			AssertEquals("IsBindable should be true", true, TestControl.IsBindable(TestDataSource));

			TestControl.Bind(TestDataSource);

			AssertEquals("OrgFindBox.BindTo", "JobDocAddress.OrganisationPK", TestControl.OrgFindBoxForTesting.BindTo);
			AssertEquals("OrgFindBox.BindToList", "OrgList", TestControl.OrgFindBoxForTesting.BindToList);
			AssertEquals("DocAddress", TestDataSource.JobDocAddress, TestControl.DocAddress);

			TestControl.UnBind();
			AssertNull("DcoAddres should be null", TestControl.DocAddress);
		}

		public void TestAppearanceProperties()
		{
			TestControl.Caption = "Test Caption";
			AssertEquals("Label Text should be as assigned", "Test Caption", TestControl.DocAddressLabelForTesting.Text);

			TestControl.CaptionCssClass = "TestCssClass";
			AssertEquals("Label CssClass should be as assigned", "TestCssClass", TestControl.DocAddressLabelForTesting.CssClass);

			TestControl.AddressCaption = "Test Address Caption";
			AssertEquals("Label Text still unassigneed without Initializing", "", TestControl.AddressLabelForTesting.Text);
			AssertEquals("Label Text still unassigneed without Initializing", "", TestControl.AddressOverrideLabelForTesting.Text);

			TestControl.ContactCaption = "Test Contact Caption";
			AssertEquals("Label Text still unassigneed without Initializing", "", TestControl.ContactLabelForTesting.Text);
			AssertEquals("Label Text still unassigneed without Initializing", "", TestControl.ContactOverrideLabelForTesting.Text);

			TestControl.OnInitForTesting(EventArgs.Empty);

			AssertEquals("Address Caption", "Test Address Caption", TestControl.AddressLabelForTesting.Text);
			AssertEquals("Address Override Caption", "Test Address Caption", TestControl.AddressOverrideLabelForTesting.Text);
			AssertEquals("Contact Caption", "Test Contact Caption", TestControl.ContactLabelForTesting.Text);
			AssertEquals("Contact Override Caption", "Test Contact Caption", TestControl.ContactOverrideLabelForTesting.Text);
		}

		public void TestDetails()
		{
			AssertEquals("Should be All Details by default", OrganisationDetails.None, TestControl.Details);

			TestControl.Details = OrganisationDetails.All;
 
			AssertEquals("OrgFullNameLabel visible", true, TestControl.OrgFullNameLabelForTesting.Visible);
			AssertEquals("OrgAddressLabel visible", true, TestControl.OrgAddressLabelForTesting.Visible);
			AssertEquals("OrgPhoneLabel visible", true, TestControl.OrgPhoneLabelForTesting.Visible);
			AssertEquals("OrgFaxLabel visible", true, TestControl.OrgFaxLabelForTesting.Visible);
			AssertEquals("OrgEmailLabel visible", true, TestControl.OrgEmailLabelForTesting.Visible);
			AssertEquals("OrgWebLabel visible", true, TestControl.OrgWebLabelForTesting.Visible);
			AssertEquals("OrgWebLink visible", true, TestControl.OrgWebLinkForTesting.Visible);

			TestControl.Details = OrganisationDetails.FullName;

			AssertEquals("OrgFullNameLabel visible", true, TestControl.OrgFullNameLabelForTesting.Visible);
			AssertEquals("OrgAddressLabel hidden", false, TestControl.OrgAddressLabelForTesting.Visible);
			AssertEquals("OrgPhoneLabel hidden", false, TestControl.OrgPhoneLabelForTesting.Visible);
			AssertEquals("OrgFaxLabel hidden", false, TestControl.OrgFaxLabelForTesting.Visible);
			AssertEquals("OrgEmailLabel hidden", false, TestControl.OrgEmailLabelForTesting.Visible);
			AssertEquals("OrgWebLabel hidden", false, TestControl.OrgWebLabelForTesting.Visible);
			AssertEquals("OrgWebLink hidden", false, TestControl.OrgWebLinkForTesting.Visible);

			TestControl.Details = OrganisationDetails.None;

			AssertEquals("OrgFullNameLabel hidden", false, TestControl.OrgFullNameLabelForTesting.Visible);
			AssertEquals("OrgAddressLabel hidden", false, TestControl.OrgAddressLabelForTesting.Visible);
			AssertEquals("OrgPhoneLabel hidden", false, TestControl.OrgPhoneLabelForTesting.Visible);
			AssertEquals("OrgFaxLabel hidden", false, TestControl.OrgFaxLabelForTesting.Visible);
			AssertEquals("OrgEmailLabel hidden", false, TestControl.OrgEmailLabelForTesting.Visible);
			AssertEquals("OrgWebLabel hidden", false, TestControl.OrgWebLabelForTesting.Visible);
			AssertEquals("OrgWebLink hidden", false, TestControl.OrgWebLinkForTesting.Visible);
		}

		public void TestSetOrgAddressLabelsForValidOrg()
		{
			AssertNotNull("JobDocAdress.Organisation", TestDataSource.JobDocAddress.Organisation);
			TestControl.Details = OrganisationDetails.All;
			TestDataSource.JobDocAddress.E2_Mobile = "Mobile";
			TestDataSource.JobDocAddress.Organisation.MainWebURL.PU_URL = "Web";

			TestControl.BindTo = "JobDocAddress";
			TestControl.BindToOrgList = "OrgList";
			TestControl.Bind(TestDataSource);

			AssertEquals("OrgFullNameLabel visible", true, TestControl.OrgFullNameLabelForTesting.Visible);
			AssertEquals("OrgFullNameLabel.Text", TestDataSource.JobDocAddress.E2_CompanyName, TestControl.OrgFullNameLabelForTesting.Text);

			AssertEquals("OrgAddressLabel visible", true, TestControl.OrgAddressLabelForTesting.Visible);
			string expectedOrgAddress = TestDataSource.JobDocAddress.E2_Address1 + "<br />" +
				TestDataSource.JobDocAddress.E2_Address2 + "<br />" +
				TestDataSource.JobDocAddress.Address.OA_RL_NKRelatedPortCode;
			AssertEquals("OrgAddressLabel.Text", expectedOrgAddress, TestControl.OrgAddressLabelForTesting.Text);

			AssertEquals("OrgPhoneLabel visible", true, TestControl.OrgPhoneLabelForTesting.Visible);
			string expectedPhone = "Phone: " + TestDataSource.JobDocAddress.E2_Phone + ", (Mobile) " + TestDataSource.JobDocAddress.E2_Mobile;
			AssertEquals("OrgPhoneLabel.Text", expectedPhone, TestControl.OrgPhoneLabelForTesting.Text);

			AssertEquals("OrgFaxLabel visible", true, TestControl.OrgFaxLabelForTesting.Visible);
			string expectedFax = "Fax: " + TestDataSource.JobDocAddress.E2_Fax;
			AssertEquals("OrgFaxLabel.Text", expectedFax, TestControl.OrgFaxLabelForTesting.Text);

			AssertEquals("OrgEmailLabel visible", true, TestControl.OrgEmailLabelForTesting.Visible);
			string expectedEmail = "Email: " + TestDataSource.JobDocAddress.E2_Email;
			AssertEquals("OrgEmailLabel.Text", expectedEmail, TestControl.OrgEmailLabelForTesting.Text);

			AssertEquals("OrgWebLabel visible", true, TestControl.OrgWebLabelForTesting.Visible);
			AssertEquals("OrgWebLabel.Text", "Web: ", TestControl.OrgWebLabelForTesting.Text);

			AssertEquals("OrgWebLink visible", true, TestControl.OrgWebLinkForTesting.Visible);
			AssertEquals("OrgWebLink.Text", TestDataSource.JobDocAddress.Organisation.MainWebURL.PU_URL, TestControl.OrgWebLinkForTesting.Text);
		}

		public void TestSetOrgAddressLabelsForInvalidOrg()
		{
			AssertNotNull("Need to invoke OrgList", TestDataSource.OrgList);
			TestDataSource.JobDocAddress.E2_OA_Address = ZGuid.Empty;
			AssertNull("JobDocAdress.Organisation", TestDataSource.JobDocAddress.Organisation);
			AssertNotNull("OrgList should be not null", TestDataSource.OrgList);
			TestControl.Details = OrganisationDetails.All;

			TestControl.BindTo = "JobDocAddress";
			TestControl.BindToOrgList = "OrgList";
			TestControl.Bind(TestDataSource);

			AssertEquals("OrgFullNameLabel visible", true, TestControl.OrgFullNameLabelForTesting.Visible);
			AssertEquals("OrgFullNameLabel.Text", OrganisationMessages.NoOrgIsSelected, TestControl.OrgFullNameLabelForTesting.Text);

			AssertEquals("OrgPhoneLabel visible", true, TestControl.OrgPhoneLabelForTesting.Visible);
			AssertEquals("OrgPhoneLabel.Text", OrganisationMessages.NoOrgIsSelected, TestControl.OrgPhoneLabelForTesting.Text);

			AssertEquals("OrgAddressLabel hidden", false, TestControl.OrgAddressLabelForTesting.Visible);
			AssertEquals("OrgFaxLabel hidden", false, TestControl.OrgFaxLabelForTesting.Visible);
			AssertEquals("OrgEmailLabel hidden", false, TestControl.OrgEmailLabelForTesting.Visible);
			AssertEquals("OrgWebLabel hidden", false, TestControl.OrgWebLabelForTesting.Visible);
			AssertEquals("OrgWebLink hidden", false, TestControl.OrgWebLinkForTesting.Visible);
		}

		#region Implementation

		#region Setup

		protected override void SetUp()
		{
			TestPage = new ZTestPage();
			Helper = GetNewHelper();
			Factory.Save();
			base.SetUp();
			TestPage.SiteUser.Login(Helper.TestOrg.OH_Code, Helper.TestContact.OC_Email, Helper.TestContact.PasswordForTesting);
			TestDataSource = GetNewDataSource();
			TestControl = GetNewControl();
			TestPage.Controls.Add(TestControl);
		}

		protected virtual ZWebTestHelper GetNewHelper()
		{
			return new ZWebTestHelper(Factory);
		}
		ZWebTestHelper Helper;

		protected override void TearDown()
		{
			if (TestPage != null)
			{
				TestPage.Dispose();
			}
			base.TearDown();
		}

		ZPage TestPage;

		BizOWithJobDocAddress TestDataSource;

		BizOWithJobDocAddress GetNewDataSource()
		{
			BizOWithJobDocAddress result = Factory.New<BizOWithJobDocAddress>();

			return result;
		}

		ZDocAddressControlForTest TestControl;

		ZDocAddressControlForTest GetNewControl()
		{
			return new ZDocAddressControlForTest();
		}

		#endregion Setup

		protected class BizOWithJobDocAddress : DummyBusinessObject
		{
			public BizOWithJobDocAddress(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public JobDocAddress JobDocAddress
			{
				get
				{
					if (fJobDocAddress == null)
					{
						fJobDocAddress = GetNewJobDocAddress();
					}
					return fJobDocAddress;
				}
				set { fJobDocAddress = value; }
			}
			JobDocAddress fJobDocAddress;

			JobDocAddress GetNewJobDocAddress()
			{
				JobDocAddress result = Factory.NewWithValidTestData<JobDocAddress>();

				if (result.OrganisationPK.IsEmpty)
				{
					OrgAddress address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_Address1 = "Address1";
					address.OA_Address2 = "Address2";
					address.OA_Phone = "Phone";
					address.OA_Mobile = "Mobile";
					address.OA_Fax = "Fax";
					address.OA_Email = "Email";
					address.OA_RL_NKRelatedPortCode = "AUSYD";

					result.E2_OA_Address = address.PK;
				}

				return result;
			}

			public OrgHeaderCollection OrgList
			{
				get
				{
					if (fOrgList == null)
					{
						fOrgList = GetNewOrgList();
					}
					return fOrgList;
				}
			}
			OrgHeaderCollection fOrgList;

			OrgHeaderCollection GetNewOrgList()
			{
				OrgHeaderCollection result = new OrgHeaderCollection(Factory);
				result.Add(JobDocAddress.Organisation);

				return result;
			}
		}

		#endregion
	}
}
