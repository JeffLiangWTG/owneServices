using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceEnterprise))]
	public class LicenceEnterpriseTest : SecurityBusinessObjectTestCase
	{
		#region Test Organisation Is Valid

		public void TestOrganisationIsValid()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();

			Assert("Organisation Exists & is loaded properly", testHeader.LicEnterprise.Organisation != null && testHeader.LicEnterprise.Organisation.PK == testHeader.PK);
		}

		#endregion

		#region Databases

		public void TestDatabases()
		{
			LicenceEnterprise ent = Factory.New<LicenceEnterprise>();
			LicenceDatabase database = ent.Databases.AddNew();
			AssertEquals("Collection has 1 element", 1, ent.Databases.Count);

			ent.Databases.RemoveAndDelete(database);
			AssertEquals("Collection has 0 elements", 0, ent.Databases.Count);
			AssertEquals("Collection should NOT be readonly", false, ent.Databases.ReadOnly);
		}

		#endregion

		#region Companies

		public void TestCompanies()
		{
			LicenceEnterprise ent = Factory.New<LicenceEnterprise>();
			LicenceCompany company = ent.Companies.AddNew();
			AssertEquals("Collection has 1 element", 1, ent.Companies.Count);

			ent.Companies.RemoveAndDelete(company);
			AssertEquals("Collection has 0 elements", 0, ent.Companies.Count);
		}

		#endregion

		#region Logging

		public void TestGetCustomLogReference()
		{
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			Factory.Save();

			LicenceEnterprise licence = org.LicEnterprise;
			AssertEquals("Licence Enterprise should have add event", 1, licence.Logs.GetAllLogs().Count);
			AssertEquals("Licence Enterprise should have add event", Events.AddedARecordToTheSystem.Code, licence.Logs.GetAllLogs()[0].SL_SE_NKEvent);

			licence.LE_EnterpriseCode = "PP/";
			Factory.Save();
			AssertEquals("Licence Enterprise should have edit event", 2, licence.Logs.GetAllLogs().Count);
			AssertEquals("Licence Enterprise should have edit event", "Enterprise Record " + licence.LE_EnterpriseCode, licence.Logs.GetAllLogs()[1].SL_Reference);
		}

		#endregion

		#region Read Only Security

		public void TestReadOnly()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModify.IsAllowed;

			LicenceEnterprise ent = Factory.New<LicenceEnterprise>();
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			ent.LE_OH = org.PK;
			ent.LE_EnterpriseCode = "ABC";
			ent.Databases.AddNew().LD_Product = "CW1";

			try
			{
				string[] propertyNamesToExcept = new string[] { "LE_EnterpriseCode", "LE_OH", "LE_AuthorityUri", "LE_OidcClientID", "LE_DomainHint", "LE_IsInternal", "LE_TokenAuthenticationEnabled", "LE_SystemCreateTimeUtc", "LE_SystemCreateUser", "LE_SystemLastEditTimeUtc", "LE_SystemLastEditUser" };
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = true;
				AssertPropertyInfosReadOnly(ent, true, propertyNamesToExcept);

				Factory.Save();
				AssertPropertyInfosReadOnly(ent, false, new string[] { "LE_EnterpriseID", "LE_EnterpriseCode" });
				AssertPropertyInfosReadOnly(ent, true, new string[] { "LE_OH", "LE_AuthorityUri", "LE_OidcClientID", "LE_DomainHint", "LE_IsInternal", "LE_TokenAuthenticationEnabled", "LE_SystemCreateTimeUtc", "LE_SystemCreateUser", "LE_SystemLastEditTimeUtc", "LE_SystemLastEditUser" });
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = oldValue;
			}
		}

		public void TestReadOnlySecurity()
		{
			bool oldValue = EDISecurityCheckpoints.OrgLicenceModify.IsAllowed;

			LicenceEnterprise ent = Factory.NewWithValidTestData<LicenceEnterprise>();
			EDIOrgHeader org = Factory.NewWithValidTestData<EDIOrgHeader>();
			ent.LE_OH = org.PK;
			ent.LE_EnterpriseCode = "ABC";
			ent.Databases.AddNew().LD_Product = "CW1";

			Factory.Save();
			try
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = true;
				AssertPropertyInfosReadOnly(ent, false, new string[] { "LE_EnterpriseCode", "LE_EnterpriseID" });
				AssertPropertyInfosReadOnly(ent, true, new string[] { "LE_OH", "LE_AuthorityUri", "LE_OidcClientID", "LE_DomainHint", "LE_IsInternal", "LE_TokenAuthenticationEnabled", "LE_SystemCreateTimeUtc", "LE_SystemCreateUser", "LE_SystemLastEditTimeUtc", "LE_SystemLastEditUser" });

				string[] propertyNamesToExcept = Array.Empty<string>();
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = false;
				AssertPropertyInfosReadOnly(ent, true, propertyNamesToExcept);
			}
			finally
			{
				EDISecurityCheckpoints.OrgLicenceModify.IsAllowed = oldValue;
			}
		}

		#endregion

		public void TestIsUATEnterprise()
		{
			LicenceEnterprise enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = "ZAW";
			LicenceCompany company1 = Factory.NewWithValidTestData<LicenceCompany>();
			company1.LC_LE = enterprise1.PK;
			LicenceHeader licence1 = Factory.NewWithValidTestData<LicenceHeader>();
			licence1.LA_LC = company1.PK;
			Factory.Save();

			ZGuid pk = enterprise1.PK;
			AssertEquals(false, LicenceEnterprise.IsUATEnterprise(pk));

			InternalIncidentLicenceSettings licenceSettings = EDIDataRegistry.Instance.InternalIncidentLicenceSettings.Value;
			LicenceEnterpriseKey internalEnterprise = licenceSettings.LicenceEnterpriseKeys.AddNew();
			internalEnterprise.LE_PK = enterprise1.PK;
			licenceSettings.EdiProd_LicencePK = licence1.PK;
			licenceSettings.UAT_ALP_LicencePK = licence1.PK;
			licenceSettings.UAT_DPR_LicencePK = licence1.PK;
			licenceSettings.UAT_STD_LicencePK = licence1.PK;
			licenceSettings.UAT_GPC_LicencePK = licence1.PK;
			licenceSettings.UAT_GPR_LicencePK = licence1.PK;
			EDIDataRegistry.Instance.InternalIncidentLicenceSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, licenceSettings);
			AssertEquals(true, LicenceEnterprise.IsUATEnterprise(pk));
		}

		public void TestEnterpriseID()
		{
			var enterprise1 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise1.LE_EnterpriseCode = "ZAW";
			enterprise1.LE_EnterpriseID = "EEE12345";

			var enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_EnterpriseCode = "ZAA";
			enterprise2.LE_EnterpriseID = "";

			Factory.Save();

			AssertEquals("EEE12345", enterprise1.LE_EnterpriseID);
			AssertEquals("E000001", enterprise2.LE_EnterpriseID);

			var enterprise3 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise3.LE_EnterpriseCode = "ZAB";
			enterprise3.LE_EnterpriseID = "EEE12345";
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertEquals(false, enterprise3.IsInDatabase);
			AssertEquals("", enterprise3.LE_EnterpriseID);
		}

		#region Implementation

		public EDIOrgHeader HeaderForTest
		{
			get
			{
				EDIOrgHeader fHeaderForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
				fHeaderForTest.OH_Code = "TGBLOG";
				fHeaderForTest.OH_RL_NKClosestPort = "AUBNE";

				return fHeaderForTest;
			}
		}

		#endregion
	}
}
