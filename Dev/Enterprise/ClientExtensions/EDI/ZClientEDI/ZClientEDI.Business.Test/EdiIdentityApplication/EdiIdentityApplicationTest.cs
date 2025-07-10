using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityRedirectUrl.Business;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityApplication.Business.Testing
{
	[TestedType(typeof(EdiIdentityApplication))]
	public class EdiIdentityApplicationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestApplicationHasDefaultValue()
		{
			var application = Factory.New<EdiIdentityApplication>();
			AssertEquals("", application.IDA_ApplicationName);
			Assert(!application.IDA_IsRollback);
			Assert(application.IDA_IsActive);
			AssertEquals(EdiIdentityApplicationRedirectUrlStatus.Codes.None, application.IDA_RedirectUrlStatus);
		}

		public void TestApplicationNameReadOnly()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			AssertEquals(false, application.IDA_ApplicationName_ReadOnly);

			Factory.Save();
			AssertEquals(true, application.IDA_ApplicationName_ReadOnly);
		}

		public void TestApplicationTypeReadOnly()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			AssertEquals(false, application.ApplicationType_ReadOnly);

			var lic = Factory.NewWithValidTestData<LicenceDatabase>();
			application.IDA_LD = lic.PK;
			AssertEquals(true, application.ApplicationType_ReadOnly);
		}

		public void TestApplicationType()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			AssertEquals("", application.ApplicationType);

			application.IDA_ApplicationType = "TST";
			AssertEquals("TST", application.ApplicationType);

			var lic = Factory.NewWithValidTestData<LicenceDatabase>();
			lic.LD_LicenceType = "PRD";
			application.IDA_LD = lic.PK;
			AssertEquals("PRD", application.ApplicationType);

			application.Product = "TRN";
			AssertEquals("ApplicationType set wont change column IDA_ApplicationType value if the license info is not empty", "TST", application.IDA_ApplicationType);
		}

		public void TestProductReadOnly()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			AssertEquals(false, application.Product_ReadOnly);

			var lic = Factory.NewWithValidTestData<LicenceDatabase>();
			application.IDA_LD = lic.PK;
			AssertEquals(true, application.Product_ReadOnly);
		}

		public void TestProduct()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			AssertEquals("", application.Product);

			application.IDA_Product = "CW1";
			AssertEquals("CW1", application.Product);

			var lic = Factory.NewWithValidTestData<LicenceDatabase>();
			lic.LD_Product = "ENT";
			application.IDA_LD = lic.PK;
			AssertEquals("ENT", application.Product);

			application.Product = "GLW";
			AssertEquals("Product set wont change column IDA_Product value if the license info is not empty", "CW1", application.IDA_Product);
		}

		public void TestApplicationHumanReadableName()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "Test Application";
			AssertEquals("Application - Test Application", application.HumanReadableName);
		}

		public void TestApplicationHumanShortcutName()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "Test Application";
			application.IDA_ClientID = "0B62CACB-2E31-4200-BCE0-7564AAC4ABE2";
			AssertEquals("Test Application - 0B62CACB-2E31-4200-BCE0-7564AAC4ABE2", application.HumanReadableShortcutName);
		}

		public void TestApplicationLicenseInfo()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			CombineAssertions(() =>
			{
				AssertNullOrEmpty("LicenceType", application.LicenceType);
				AssertEquals(false, application.IsLicenceActive);
				AssertNullOrEmpty("LicenceProductCode", application.LicenceProductCode);
			});

			var licenseDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenseDatabase.LD_LicenceType = "PRD";
			licenseDatabase.LD_Product = "CW1";
			application.IDA_LD = licenseDatabase.PK;

			CombineAssertions(() =>
			{
				AssertEquals("PRD", application.LicenceType);
				AssertEquals(true, application.IsLicenceActive);
				AssertEquals("CW1", application.LicenceProductCode);
			});
		}

		public void TestApplicationRedirectUrlStatus()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			Factory.Save();
			AssertEquals(EdiIdentityApplicationRedirectUrlStatus.Codes.None, application.IDA_RedirectUrlStatus);

			var redirectUrl = application.RedirectUrls.AddNew();
			redirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			redirectUrl.IAR_RedirectUrl = "https://web.com";
			Factory.Save();
			AssertEquals(EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged, application.IDA_RedirectUrlStatus);

			application.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.Scheduled;
			Factory.Save();
			AssertEquals(EdiIdentityApplicationRedirectUrlStatus.Codes.Scheduled, application.IDA_RedirectUrlStatus);

			redirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
			Factory.Save();
			AssertEquals(EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged, application.IDA_RedirectUrlStatus);

			application.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.None;
			redirectUrl.Delete();
			Factory.Save();
			AssertEquals(EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged, application.IDA_RedirectUrlStatus);

			application.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.None;

			var redirectUrl2 = application.RedirectUrls.AddNew();
			redirectUrl2.Delete();
			Factory.Save();
			AssertEquals(EdiIdentityApplicationRedirectUrlStatus.Codes.None, application.IDA_RedirectUrlStatus);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestParentApplication()
		{
			var parentApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			var childApplication = Factory.NewWithValidTestData<EdiIdentityApplication>();
			childApplication.IDA_IDA_ParentApplication = parentApplication.PK;
			Factory.Save();
			AssertEquals(childApplication.ParentApplication, parentApplication);
		}

		public void TestIsRollbackWhenIsCanceledIsSetToTrue()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IsCancelled = true;
			Factory.Save();
			application.ReloadSafe();
			Assert(application.IsCancelled);
			Assert(!application.IDA_IsRollback);
		}

		public void TestParentOrg()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_OH_ParentOrg = orgHeader.PK;
			Factory.Save();

			AssertEquals(orgHeader, application.ParentOrg);
		}

		public void TestIsCustomerApplication()
		{
			var application = Factory.New<EdiIdentityApplication>();
			Factory.Save();

			Assert(application.IsCustomerApplication);

			var ediIdentityTenant = Factory.New<EdiIdentityTenant>();
			application.IDA_IDT = ediIdentityTenant.PK;
			Factory.Save();

			Assert(!application.IsCustomerApplication);
		}

		public void TestSaveCustomerApplicationWillCreateClientId()
		{
			var application = Factory.New<EdiIdentityApplication>();
			application.IsCustomerApplication = true;
			Factory.Save();

			AssertNotNullOrEmpty(application.IDA_ClientID);
		}

		public void TestSaveNonCustomerApplicationWillCreateTenantId()
		{
			var ediIdentityTenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			ediIdentityTenant.IDT_TenantId = "TestTenantId";
			var ediIdentityTenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			Factory.Save();

			using (EDIDataRegistry.Instance.AzureApplicationManagementTenantID.SetTemporaryValue(Guid.Empty, Guid.Empty,
				Guid.Empty, "TestTenantId"))
			{
				var application = Factory.New<EdiIdentityApplication>();
				application.IsCustomerApplication = false;
				Factory.Save();
				AssertEquals("The IDA_IDT should match the PK of Azure Application Management Tenant ID", ediIdentityTenant.PK, application.IDA_IDT);

				var application2 = Factory.New<EdiIdentityApplication>();
				application2.IsCustomerApplication = false;
				application2.IDA_IDT = ediIdentityTenant2.PK;
				Factory.Save();
				AssertEquals("The IDA_IDT should not be updated", ediIdentityTenant2.PK, application2.IDA_IDT);

				var application3 = Factory.New<EdiIdentityApplication>();
				application3.IsCustomerApplication = true;
				application3.IDA_IDT = Guid.Empty;
				Factory.Save();
				AssertEquals("The IDA_IDT should not be set when it is a customer application.", Guid.Empty, application3.IDA_IDT);
			}
		}

		public void TestIDA_IDTReadOnly()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			Assert(application.TenantID_ReadOnly);
		}

		public void TestTenant()
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_IDT = tenant.PK;
			Factory.Save();
			AssertEquals(application.Tenant, tenant);
		}

		public void TestUniqueIndexForIDA_LDAndIDA_IDT()
		{
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var application1 = Factory.NewWithValidTestData<EdiIdentityApplication>();

			var application2 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application2.IDA_IDT = tenant.PK;

			var application3 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application3.IDA_LD = licenceDatabase.PK;

			var application4 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application4.IDA_IDT = tenant.PK;
			application4.IDA_LD = licenceDatabase.PK;

			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();

			Factory.Save();

			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_IDT = application1.IDA_IDT;
			application.IDA_LD = application1.IDA_LD;
			AssertNoExceptionThrown("Should not throw for empty IDA_IDT and empty IDA_LD", Factory.Save);

			application.IDA_IDT = application2.IDA_IDT;
			application.IDA_LD = application2.IDA_LD;
			AssertNoExceptionThrown("Should not throw for empty IDA_LD and assigned IDA_IDT", Factory.Save);

			application.IDA_IDT = application3.IDA_IDT;
			application.IDA_LD = application3.IDA_LD;
			var exception = AssertExceptionThrown<ZSaveException>("Should throw for assigned IDA_LD and empty IDA_IDT", Factory.Save);
			AssertContains("NR_UX__IDA_LD_IDA_IDT", exception.Message);

			application.IDA_IDT = application4.IDA_IDT;
			application.IDA_LD = application4.IDA_LD;
			exception = AssertExceptionThrown<ZSaveException>("Should throw for assigned IDA_LD and IDA_IDT", Factory.Save);
			AssertContains("NR_UX__IDA_LD_IDA_IDT", exception.Message);
		}

		public void TestUniqueIndex_CombinationOfTenantIdAndClientId()
		{
			var tenant1 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var tenant2 = Factory.NewWithValidTestData<EdiIdentityTenant>();
			var application1 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application1.IDA_ClientID = "TestClientID";
			application1.IDA_IDT = tenant1.PK;

			var application2 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application2.IDA_ClientID = "TestClientID";
			application2.IDA_IDT = tenant2.PK;
			AssertNoExceptionThrown(Factory.Save);

			var application3 = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application3.IDA_ClientID = "TestClientID";
			application3.IDA_IDT = tenant1.PK;
			var exception = AssertExceptionThrown<ZSaveException>("Should throw for assigned IDA_ClientID and IDA_IDT", Factory.Save);
			AssertContains("NR_UX__IDA_ClientID_IDA_IDT", exception.Message);
		}
	}
}
