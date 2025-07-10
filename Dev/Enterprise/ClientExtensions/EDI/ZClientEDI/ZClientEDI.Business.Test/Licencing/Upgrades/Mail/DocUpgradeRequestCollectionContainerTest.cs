using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(DocUpgradeRequestCollectionContainer))]
	class DocUpgradeRequestCollectionContainerTest : DocumentWrapperTestCase
	{
		public void TestClientProperties()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_FullName = "DDD Testing";
			org.OH_Code = "DDDTST";
			EDIOrgHeader org2 = Factory.New<EDIOrgHeader>();
			org2.OH_FullName = "CAC Sydney";
			org2.OH_Code = "CACSYD";

			UpgradeRequestCollectionContainerTestHelper upgrader = new UpgradeRequestCollectionContainerTestHelper(Factory, org);
			DocUpgradeRequestCollectionContainer wrapper = DocUpgradeRequestCollectionContainer.New(upgrader, Factory);
			AssertEquals("", wrapper.ClientName);
			AssertEquals("", wrapper.ClientCode);

			upgrader.CurrentOrganisationForTest = org;
			AssertEquals("DDD Testing", wrapper.ClientName);
			AssertEquals("DDDTST", wrapper.ClientCode);

			upgrader.CurrentOrganisationForTest = org2;
			AssertEquals("CAC Sydney", wrapper.ClientName);
			AssertEquals("CACSYD", wrapper.ClientCode);
		}

		public void TestReleaseBuildProperties()
		{
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_Product = ProductTypes.Codes.Enterprise;
			build.HL_MajorVersion = 1;
			build.HL_MinorVersion = 4;
			build.HL_Release = 6985;
			build.HL_Patch = 26;
			build.HL_ReleaseStatus = ReleaseRings.Codes.GPR;
			build.HL_ExeVersionDate = new ZDateTime(2011, 4, 19, 5, 21, 39);

			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			UpgradeRequestCollectionContainerTestHelper upgrader = new UpgradeRequestCollectionContainerTestHelper(Factory, org);
			DocUpgradeRequestCollectionContainer wrapper = DocUpgradeRequestCollectionContainer.New(upgrader, Factory);
			AssertEquals("", wrapper.ReleaseDescription);
			AssertEquals("", wrapper.ReleaseExeDate);
			AssertEquals("", wrapper.ReleaseVersionNumber);

			upgrader.ReleaseBuildPK = build.PK;
			AssertContains("GP Release", wrapper.ReleaseDescription);
			AssertContains("patch 26", wrapper.ReleaseDescription);
			AssertEquals("19-Apr-11 05:21", wrapper.ReleaseExeDate);
			AssertEquals("1.4.6985.26", wrapper.ReleaseVersionNumber);
		}

		[TestDate(2011, 4, 19, 17, 55, 0)]
		public void TestUpgradeNotificationMessage()
		{
			EDIDataRegistry.Instance.AllSystemMessagesViaEhubReleaseBuilds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "16.10.20.0");
			ReleaseBuild build = Factory.New<ReleaseBuild>();
			build.HL_Product = ProductTypes.Codes.Enterprise;
			build.HL_MajorVersion = 16;
			build.HL_MinorVersion = 10;
			build.HL_Release = 20;
			build.HL_Patch = 0;
			build.HL_ReleaseStatus = ReleaseRings.Codes.ALP;

			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			LicenceDatabase database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			database1.LD_HL_CurrentRunningVersion = build.PK;
			database1.LD_ServerCode = "AD1";
			database1.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;
			LicenceDatabase database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			database2.LD_HL_CurrentRunningVersion = build.PK;
			database2.LD_ServerCode = "AD2";
			database2.LD_AvailableUpgradeMethod = UpgradeMethods.Codes.Http;

			UpgradeRequest request1 = new UpgradeRequest(Factory, org, database1);
			request1.SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
			UpgradeRequest request2 = new UpgradeRequest(Factory, org, database2);
			request2.SupportedUpgradeMethod = UpgradeMethods.Codes.Http;
			UpgradeRequest[] upgradesForOrg = new UpgradeRequest[] { request1, request2 };

			UpgradeRequestCollectionContainerTestHelper upgrader = new UpgradeRequestCollectionContainerTestHelper(Factory, org);
			DocUpgradeRequestCollectionContainer wrapper = DocUpgradeRequestCollectionContainer.New(upgrader, Factory);

			AssertEquals("", wrapper.UpgradeNotificationMessage);

			upgrader.UpgradesForCurrentOrganisationForTest = upgradesForOrg;
			string expectedMessage =
@"Server AD1 at 19-Apr-11 17:55:00 via HTTP Download.
Server AD2 at 19-Apr-11 17:55:00 via HTTP Download.
";

			AssertEquals(expectedMessage, wrapper.UpgradeNotificationMessage);
		}

		public void TestNotificationSubjectPrefix()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_FullName = "DDD Testing";
			org.OH_Code = "DDDTST";

			UpgradeRequestCollectionContainerTestHelper upgrader = new UpgradeRequestCollectionContainerTestHelper(Factory, org);
			DocUpgradeRequestCollectionContainer wrapper = DocUpgradeRequestCollectionContainer.New(upgrader, Factory);

			AssertEquals("", wrapper.NotificationSubjectPrefix);

			upgrader.NotificationSubjectPrefix = "prefix";
			AssertEquals("prefix", wrapper.NotificationSubjectPrefix);
		}

		public void TestAdditionalNotification()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_FullName = "DDD Testing";
			org.OH_Code = "DDDTST";

			UpgradeRequestCollectionContainerTestHelper upgrader = new UpgradeRequestCollectionContainerTestHelper(Factory, org);
			DocUpgradeRequestCollectionContainer wrapper = DocUpgradeRequestCollectionContainer.New(upgrader, Factory);

			AssertEquals("", wrapper.AdditionalNotification);

			upgrader.AdditionalNotification = "prefix";
			AssertEquals("prefix\r\n", wrapper.AdditionalNotification);
		}

		#region Implementation

		class UpgradeRequestCollectionContainerTestHelper : UpgradeRequestCollectionContainer
		{
			public UpgradeRequestCollectionContainerTestHelper(BusinessObjectFactory factory, params EDIOrgHeader[] organisationsToUpgrade)
				: base(factory, organisationsToUpgrade)
			{ }

			public override EDIOrgHeader CurrentOrganisation
			{
				get { return CurrentOrganisationForTest; }
			}
			public EDIOrgHeader CurrentOrganisationForTest { get; set; }

			public override UpgradeRequest[] UpgradesForCurrentOrganisation
			{
				get { return UpgradesForCurrentOrganisationForTest; }
			}
			public UpgradeRequest[] UpgradesForCurrentOrganisationForTest { get; set; }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "CDESYD";
			org.Contacts.AddNew();
			org.Contacts[0].OC_ContactName = "Test Contact";
			org.Contacts[0].OC_Email = "Test.Contact@edi.com";

			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = org.PK;
			LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			org.LicCompany.LC_LE = licEnt.PK;

			UpgradeRequestCollectionContainerTestHelper upgrader = new UpgradeRequestCollectionContainerTestHelper(Factory, org);
			return new DocumentWrapper[] { DocUpgradeRequestCollectionContainer.New(upgrader, Factory) };
		}

		#endregion
	}
}
