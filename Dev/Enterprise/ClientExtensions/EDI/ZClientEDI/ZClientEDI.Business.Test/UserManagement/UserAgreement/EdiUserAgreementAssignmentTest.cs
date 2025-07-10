using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	[TestedType(typeof(EdiUserAgreementAssignment))]
	public class EdiUserAgreementAssignmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetParent()
		{
			var assignment = Factory.New<EdiUserAgreementAssignment>();
			var attachable = Factory.NewWithValidTestData<LicenceEnterprise>();

			assignment.Parent = attachable;
			AssertEquals(attachable.PK, assignment.EAE_ParentID);
		}

		public void TestCurrentAgreement()
		{
			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; DISABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var myaAgreement = Factory.New<EdiUserAgreement>();
			myaAgreement.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			myaAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-1);

			var cwnAgreement = Factory.New<EdiUserAgreement>();
			cwnAgreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			cwnAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-1);

			Factory.Save();

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.Parent = enterprise;

			assignment.EAE_AgreementType = ZString.Empty;
			AssertNull(assignment.CurrentAgreement);

			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			AssertEquals(myaAgreement.PK, assignment.CurrentAgreement.PK);

			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			AssertEquals(cwnAgreement.PK, assignment.CurrentAgreement.PK);
		}

		public void TestRebuildAcceptanceLogsCollection()
		{
			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; DISABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();

			var enterprise2 = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise2.LE_EnterpriseID = "EXXXXX";

			var myaAgreement = Factory.New<EdiUserAgreement>();
			myaAgreement.ERA_Type = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			myaAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-4);

			var ca1Agreement = Factory.New<EdiUserAgreement>();
			ca1Agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			ca1Agreement.ERA_VariantCode = "CA1";
			ca1Agreement.ERA_VariantDescription = "CA1";
			ca1Agreement.ERA_VersionNumber = 1;
			ca1Agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);

			var disabledAgreement = Factory.New<EdiUserAgreement>();
			disabledAgreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			disabledAgreement.ERA_VariantCode = "CA1";
			disabledAgreement.ERA_VariantDescription = "CA1";
			disabledAgreement.ERA_VersionNumber = 2;
			disabledAgreement.ERA_IsActive = false;
			disabledAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-1);

			var futureAgreement = Factory.New<EdiUserAgreement>();
			futureAgreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			futureAgreement.ERA_VariantCode = "CA1";
			futureAgreement.ERA_VariantDescription = "CA1";
			futureAgreement.ERA_VersionNumber = 3;
			futureAgreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(1);

			var ca2Agreement1 = Factory.New<EdiUserAgreement>();
			ca2Agreement1.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			ca2Agreement1.ERA_VariantCode = "CA2";
			ca2Agreement1.ERA_VariantDescription = "CA2";
			ca2Agreement1.ERA_VersionNumber = 4;
			ca2Agreement1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-5);

			var ca2Agreement2 = Factory.New<EdiUserAgreement>();
			ca2Agreement2.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			ca2Agreement2.ERA_VariantCode = "CA2";
			ca2Agreement2.ERA_VariantDescription = "CA2";
			ca2Agreement2.ERA_VersionNumber = 5;
			ca2Agreement2.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-6);

			Factory.Save();

			var myaAgreementLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			myaAgreementLog.EUL_ERA = myaAgreement.PK;
			myaAgreementLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			myaAgreementLog.EUL_LE = enterprise.PK;

			var enterpriseLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			enterpriseLog.EUL_ERA = ca1Agreement.PK;
			enterpriseLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			enterpriseLog.EUL_LE = enterprise.PK;

			var ca2Agreement1Log = Factory.New<EdiUserAgreementAcceptanceLog>();
			ca2Agreement1Log.EUL_ERA = ca2Agreement1.PK;
			ca2Agreement1Log.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			ca2Agreement1Log.EUL_LE = enterprise.PK;

			var ca2Agreement2Log = Factory.New<EdiUserAgreementAcceptanceLog>();
			ca2Agreement2Log.EUL_ERA = ca2Agreement2.PK;
			ca2Agreement2Log.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			ca2Agreement2Log.EUL_LE = enterprise.PK;

			var enterpriseDisabledAgreementLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			enterpriseDisabledAgreementLog.EUL_ERA = disabledAgreement.PK;
			enterpriseDisabledAgreementLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			enterpriseDisabledAgreementLog.EUL_LE = enterprise.PK;

			var enterpriseFutureAgreementLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			enterpriseFutureAgreementLog.EUL_ERA = futureAgreement.PK;
			enterpriseFutureAgreementLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			enterpriseFutureAgreementLog.EUL_LE = enterprise.PK;

			var enterprise2Log = Factory.New<EdiUserAgreementAcceptanceLog>();
			enterprise2Log.EUL_ERA = ca1Agreement.PK;
			enterprise2Log.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			enterprise2Log.EUL_LE = enterprise2.PK;

			Factory.Save();

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			AssertEquals(0, assignment.AcceptanceLogs.Count);

			assignment.Parent = enterprise;
			AssertEquals(0, assignment.AcceptanceLogs.Count);

			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			AssertEquals(0, assignment.AcceptanceLogs.Count);

			assignment.EAE_VariantCode = "CA1";
			AssertEquals(1, assignment.RelatedAgreements.Count);
			AssertEquals(ca1Agreement.PK, assignment.CurrentAgreement.PK);
			AssertEquals(1, assignment.AcceptanceLogs.Count);
			AssertEquals(enterpriseLog.PK, assignment.AcceptanceLogs[0].PK);
			AssertNotEquals("Collection should not contain inactive agreement log", disabledAgreement.PK, assignment.AcceptanceLogs[0].PK);
			AssertNotEquals("Collection should not contain future agreement log", futureAgreement.PK, assignment.AcceptanceLogs[0].PK);

			assignment.EAE_VariantCode = "CA2";
			AssertEquals(2, assignment.AcceptanceLogs.Count);
			Assert("The enterpriseLog result should be decided by Assignment's type, variant code, instead of parent", assignment.AcceptanceLogs.Any(x => x.PK == ca2Agreement1Log.PK));
			Assert("The enterpriseLog result should be decided by Assignment's type, variant code, instead of parent", assignment.AcceptanceLogs.Any(x => x.PK == ca2Agreement2Log.PK));

			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.UserAccountCollection;
			AssertNullOrEmpty(assignment.EAE_VariantCode);
			AssertEquals(0, assignment.AcceptanceLogs.Count);
		}

		public void TestVariantCodeReadOnly()
		{
			var assignment = Factory.New<EdiUserAgreementAssignment>();

			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			Assert(EdiUserAgreementTypesMapper.IsVariantEnabled(EdiUserAgreementTypes.Codes.CargoWiseNext));
			Assert("The property should not be read only if the type enabled variant", !assignment.EAE_VariantCodeInfo.ReadOnly);

			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo;
			Assert(!EdiUserAgreementTypesMapper.IsVariantEnabled(EdiUserAgreementTypes.Codes.MyAccountLoginUserInfo));
			Assert("The property should be read only if the type disabled variant", assignment.EAE_VariantCodeInfo.ReadOnly);

			assignment.EAE_AgreementType = "";
			Assert(!EdiUserAgreementTypesMapper.IsVariantEnabled(string.Empty));
			Assert(assignment.EAE_VariantCodeInfo.ReadOnly);
		}

		public void TestParentProperty_TypeRestriction()
		{
			var dummyObject = Factory.NewWithValidTestData<DummyBusinessObject>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			Factory.Save();

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_ParentTableCode = dummyObject.TablePrefix;
			assignment.EAE_ParentID = dummyObject.PK;
			AssertHasError(assignment.EAE_ParentTableCodeInfo, "The current parent is not supported");
			AssertExceptionThrown(typeof(ZSaveException), () => Factory.Save());

			AssertExceptionThrown(typeof(InvalidOperationException), () => { _ = assignment.Parent; });
			AssertNoExceptionThrown(() => { assignment.Parent = enterprise; });
			AssertExceptionThrown(typeof(InvalidOperationException), () => { assignment.Parent = dummyObject; });
		}

		public void TestGetCurrentAssignedAgreement()
		{
			EdiUserAgreementTest.DisableEffectiveDateTriggers(TestConnection);
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var licence = Factory.NewWithValidTestData<LicenceEnterprise>();
			Factory.Save();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.Parent = licence;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";
			Factory.Save();

			AssertEquals(agreement.PK, assignment.CurrentAgreement.PK);
		}

		public void TestGetCurrentAssignment()
		{
			EdiUserAgreementTest.DisableEffectiveDateTriggers(TestConnection);
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var licence = Factory.NewWithValidTestData<LicenceEnterprise>();
			Factory.Save();

			var agreement2 = Factory.New<EdiUserAgreement>();
			agreement2.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement2.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-1);
			agreement2.ERA_VariantCode = "VA1";
			agreement2.ERA_VariantDescription = "VA1";

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.Parent = licence;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";
			Factory.Save();

			var parent = (new BusinessObjectFactory() { RefreshEnabled = false }).Load<LicenceEnterprise>(licence.PK);
			var loadedAssignment = EdiUserAgreementAssignment.GetCurrentAssignment(parent, EdiUserAgreementTypes.Codes.CargoWiseNext);
			AssertEquals(assignment.PK, loadedAssignment.PK);
			AssertEquals(agreement2.PK, loadedAssignment.CurrentAgreement.PK);
		}

		public void TestGetCurrentAssignment_Database()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.Parent = database;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";
			assignment.EAE_OH_ClientAgreementOrg = org1.PK;
			Factory.Save();

			AssertEquals("Should get agreement since there's a match on assignment", assignment, EdiUserAgreementAssignment.GetCurrentAssignment(database, EdiUserAgreementTypes.Codes.CargoWiseNext));
			AssertEquals("Should not return an agreement since there's no assignment", null, EdiUserAgreementAssignment.GetCurrentAssignment(database2, EdiUserAgreementTypes.Codes.CargoWiseNext));

			assignment.Parent = database.LicEnterprise;
			AssertEquals("Should get agreement since there's a match on enterprise level assignment", assignment, EdiUserAgreementAssignment.GetCurrentAssignment(database, EdiUserAgreementTypes.Codes.CargoWiseNext));
		}

		public void TestClientAgreementOrg()
		{
			EdiUserAgreementTest.DisableEffectiveDateTriggers(TestConnection);
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org2.PK;
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = database;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = "VA1";
			assignment1.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = enterprise;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = "VA1";
			Factory.Save();

			AssertEquals("Database level assignment should return EAE_OH_ClientAgreementOrg", org1.PK, assignment1.ClientAgreementOrg.PK);
			AssertEquals("Enterprise level assignment should return LE_OH org", org2.PK, assignment2.ClientAgreementOrg.PK);
		}

		public void TestClientAgreementOrgPK()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org2.PK;
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = database;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = "VA1";
			assignment1.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = enterprise;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = "VA1";
			Factory.Save();

			AssertEquals("Database level assignment should return EAE_OH_ClientAgreementOrg", org1.PK, assignment1.ClientAgreementOrgPK);
			AssertEquals("Enterprise level assignment should return LE_OH org", org2.PK, assignment2.ClientAgreementOrgPK);
			AssertEquals(true, assignment2.ClientAgreementOrgPKInfo.ReadOnly);

			assignment1.ClientAgreementOrgPK = org3.PK;
			AssertEquals("Should set client agreement org to org3", org3.PK, assignment1.ClientAgreementOrgPK);
			AssertEquals("Should set client agreement org to org3", org3.PK, assignment1.ClientAgreementOrg.PK);
			AssertEquals("Should set client agreement org to org3", org3.PK, assignment1.EAE_OH_ClientAgreementOrg);
		}

		public void TestClientAgreementOrgPK_ReadOnly()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org1.PK;
			Factory.Save();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.Parent = database;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";
			AssertEquals("Should not be readonly since parent is db", false, assignment.EAE_OH_ClientAgreementOrgInfo.ReadOnly);
			AssertEquals("Should not be readonly since parent is db", false, assignment.ClientAgreementOrgPKInfo.ReadOnly);

			assignment.Parent = enterprise;
			AssertEquals("Should be readonly since parent is enterprise", true, assignment.EAE_OH_ClientAgreementOrgInfo.ReadOnly);
			AssertEquals("Should be readonly since parent is enterprise", true, assignment.ClientAgreementOrgPKInfo.ReadOnly);
		}

		public void TestEAE_OH_ClientAgreementOrg_ShouldResetWhenEAE_ParentTableCodeSetToLE()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org1.PK;
			Factory.Save();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.Parent = database1;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";
			assignment.EAE_OH_ClientAgreementOrg = org1.PK;

			assignment.Parent = database2;
			AssertEquals("Should remain unchanged if reassigning to different database", org1.PK, assignment.EAE_OH_ClientAgreementOrg);

			assignment.Parent = enterprise;
			AssertEquals("Should be cleared when setting to enterprise", true, assignment.EAE_OH_ClientAgreementOrg.IsEmpty);
		}

		public void TestParentTableCodeDescription()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org1.PK;
			Factory.Save();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.Parent = database1;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";
			assignment.EAE_OH_ClientAgreementOrg = org1.PK;

			AssertEquals("Precondition", LicenceDatabaseSchema.Constants.Prefix, assignment.EAE_ParentTableCode);
			AssertEquals("Should match the lookup description", assignment.Lookups.ParentTypes.GetDescriptionFromCode(LicenceDatabaseSchema.Constants.Prefix), assignment.ParentTableCodeDescription);

			assignment.ParentTableCodeDescription = assignment.Lookups.ParentTypes.GetDescriptionFromCode(LicenceEnterpriseSchema.Constants.Prefix);
			AssertEquals("Should be set to the code", LicenceEnterpriseSchema.Constants.Prefix, assignment.EAE_ParentTableCode);

			assignment.ParentTableCodeDescription = "Invalid";
			AssertEquals("Should remain unchanged", LicenceEnterpriseSchema.Constants.Prefix, assignment.EAE_ParentTableCode);

			assignment.ParentTableCodeDescription = assignment.Lookups.ParentTypes.GetDescriptionFromCode(LicenceDatabaseSchema.Constants.Prefix);
			AssertEquals("Should be set to the code", LicenceDatabaseSchema.Constants.Prefix, assignment.EAE_ParentTableCode);
		}

		public void TestEAE_ParentIDReadOnly()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org1.PK;
			Factory.Save();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.Parent = database1;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";
			assignment.EAE_OH_ClientAgreementOrg = org1.PK;

			AssertEquals("Precondition", LicenceDatabaseSchema.Constants.Prefix, assignment.EAE_ParentTableCode);
			AssertEquals("Should be editable if parent is licence database", false, assignment.EAE_ParentIDReadOnly);

			assignment.EAE_ParentTableCode = LicenceEnterpriseSchema.Constants.Prefix;
			AssertEquals("Should be readonly if parent is licence enterprise", true, assignment.EAE_ParentIDReadOnly);
		}

		public void TestEAE_ParentIDShouldBeSetToEnterpriseParentWhenParentTableCodeSetToLE()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org1.PK;
			Factory.Save();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EnterpriseParent = enterprise;
			assignment.Parent = database1;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";
			assignment.EAE_OH_ClientAgreementOrg = org1.PK;

			AssertEquals("Precondition: Parent should be database", database1.PK, assignment.EAE_ParentID);
			AssertEquals("Precondition: Enterprise Parent should be enterprise", enterprise, assignment.EnterpriseParent);

			assignment.EAE_ParentTableCode = LicenceEnterpriseSchema.Constants.Prefix;
			AssertEquals("Parent should be automatically set to the enteprise parent", enterprise.PK, assignment.EAE_ParentID);
		}

		public void TestEAE_ClientAgreementOrg_ShouldBeSetToEnterpriseOrgWhenParentTableCodeSetToLD()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org1.PK;
			Factory.Save();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EnterpriseParent = enterprise;
			assignment.Parent = enterprise;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";

			AssertEquals("Precondition: Client agreement org should be empty if parent is enterprise", true, assignment.EAE_OH_ClientAgreementOrg.IsEmpty);

			assignment.EAE_ParentTableCode = LicenceDatabaseSchema.Constants.Prefix;
			AssertEquals("Client agreement org should be automatically set to the enterprise org", enterprise.LE_OH, assignment.EAE_OH_ClientAgreementOrg);

			assignment.EAE_ParentTableCode = LicenceEnterpriseSchema.Constants.Prefix;
			AssertEquals("Client agreement org should be erased", true, assignment.EAE_OH_ClientAgreementOrg.IsEmpty);
		}

		public void TestSettingEAE_ParentTableCodeToDatabaseShouldResetEAE_ParentID()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org1.PK;
			Factory.Save();

			var assignment = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment.EnterpriseParent = enterprise;
			assignment.Parent = enterprise;
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.EAE_VariantCode = "VA1";

			assignment.EAE_ParentTableCode = LicenceDatabaseSchema.Constants.Prefix;
			AssertEquals("EAE_ParentID should be cleared", true, assignment.EAE_ParentID.IsEmpty);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var enterpriseParent = factory.NewWithValidTestData<LicenceEnterprise>();
			var bizO = (EdiUserAgreementAssignment)base.GetNewBusinessObjectForDeleteTest(factory);
			bizO.EAE_ParentTableCode = enterpriseParent.TablePrefix;
			bizO.EAE_ParentID = enterpriseParent.PK;
			return bizO;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var enterpriseParent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var bizO = (EdiUserAgreementAssignment)base.GetNewBusinessObject();
			bizO.EAE_ParentTableCode = enterpriseParent.TablePrefix;
			bizO.EAE_ParentID = enterpriseParent.PK;
			bizO.EnterpriseParent = enterpriseParent;
			return bizO;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				var result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result.Add(nameof(EdiUserAgreementAssignment.ParentTableCodeDescription), (ZString)EdiUserAgreementAssignmentLookups.DatabaseParentDescription);
				return result;
			}
		}
	}
}
