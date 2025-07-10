using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	[TestedType(typeof(EdiUserAgreementAssignmentValidation))]
	public class EdiUserAgreementAssignmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEAE_AgreementType()
		{
			var parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			Factory.Save();

			var assignment = Factory.New<EdiUserAgreementAssignment>();
			assignment.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment.Parent = parent;
			Factory.Save();

			var assignment2 = Factory.New<EdiUserAgreementAssignment>();
			assignment2.Parent = parent;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;

			AssertHasError(assignment2.EAE_AgreementTypeInfo, "Duplicate agreement type");
		}

		public void TestCheckEAE_ParentTableCode()
		{
			var parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			Factory.Save();

			var assignment = Factory.New<EdiUserAgreementAssignment>();

			assignment.EAE_ParentTableCode = DummyBusinessObjectSchema.Constants.Prefix;
			AssertHasError(assignment.EAE_ParentTableCodeInfo, "The current parent is not supported");

			assignment.EAE_ParentTableCode = LicenceEnterpriseSchema.Constants.Prefix;
			AssertNoError(assignment.EAE_ParentTableCodeInfo, "The current parent is not supported");

			assignment.EAE_ParentTableCode = LicenceDatabaseSchema.Constants.Prefix;
			AssertNoError(assignment.EAE_ParentTableCodeInfo, "The current parent is not supported");
		}

		public void TestCheckEAE_AllowOnlineAcceptance_ShouldOnlyAllowOnOneVariant()
		{
			var parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var agreementVariant1 = Factory.New<EdiUserAgreement>();
			agreementVariant1.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreementVariant1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreementVariant1.ERA_VariantCode = "VA1";
			agreementVariant1.ERA_VariantDescription = "VA1";
			var agreementVariant2 = Factory.New<EdiUserAgreement>();
			agreementVariant2.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreementVariant2.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreementVariant2.ERA_VariantCode = "VA2";
			agreementVariant2.ERA_VariantDescription = "VA2";
			Factory.Save();

			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			parent.Databases.Add(db1);
			parent.Databases.Add(db2);

			AssertEquals("Precondition", parent.PK, db1.LD_LE);
			AssertEquals("Precondition", parent.PK, db2.LD_LE);
			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = db1;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = agreementVariant1.ERA_VariantCode;
			assignment1.EAE_AllowOnlineAcceptance = true;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = db2;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = agreementVariant2.ERA_VariantCode;
			assignment2.EAE_AllowOnlineAcceptance = true;

			var errorMessage = "Only one Variant can have online acceptance enabled per Agreement Type on an Enterprise License.";
			assignment1.Validation.ValidateAll();
			AssertHasError(assignment1.EAE_AllowOnlineAcceptanceInfo, errorMessage);
			AssertHasError(assignment2.EAE_AllowOnlineAcceptanceInfo, errorMessage);

			assignment2.EAE_AllowOnlineAcceptance = false;
			AssertNoError(assignment2.EAE_AllowOnlineAcceptanceInfo, errorMessage);

			assignment2.EAE_AllowOnlineAcceptance = true;
			AssertHasError(assignment2.EAE_AllowOnlineAcceptanceInfo, errorMessage);

			assignment2.EAE_VariantCode = agreementVariant1.ERA_VariantCode;
			AssertNoError(assignment2.EAE_AllowOnlineAcceptanceInfo, errorMessage);

			assignment2.EAE_VariantCode = agreementVariant2.ERA_VariantCode;
			AssertHasError(assignment2.EAE_AllowOnlineAcceptanceInfo, errorMessage);

			assignment2.EAE_AgreementType = "ZZZ";
			AssertNoError(assignment2.EAE_AllowOnlineAcceptanceInfo, errorMessage);
		}

		public void TestCheckEAE_ParentID_EnterpriseParent()
		{
			var parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var agreementVariant1 = Factory.New<EdiUserAgreement>();
			agreementVariant1.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreementVariant1.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreementVariant1.ERA_VariantCode = "VA1";
			agreementVariant1.ERA_VariantDescription = "VA1";
			var agreementVariant2 = Factory.New<EdiUserAgreement>();
			agreementVariant2.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreementVariant2.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreementVariant2.ERA_VariantCode = "VA2";
			agreementVariant2.ERA_VariantDescription = "VA2";
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = parent;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = agreementVariant1.ERA_VariantCode;
			assignment1.EAE_AllowOnlineAcceptance = true;

			AssertNoErrors(assignment1.EAE_ParentIDInfo);

			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = parent;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = agreementVariant2.ERA_VariantCode;
			assignment2.EAE_AllowOnlineAcceptance = true;

			assignment1.Validation.ValidateAll();
			AssertHasError(assignment1.EAE_ParentIDInfo, "There can only be one Assignment per Agreement Type for Enterprise Level Assignments.");
			AssertHasError(assignment2.EAE_ParentIDInfo, "There can only be one Assignment per Agreement Type for Enterprise Level Assignments.");

			assignment2.EAE_AgreementType = "ZZZ";
			assignment1.Validation.ValidateAll();
			AssertNoErrors(assignment1.EAE_ParentIDInfo);
		}

		public void TestCheckEAE_ParentID_EnterpriseAndDatabaseParentShouldNotBeAllowedForSameType()
		{
			var parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";
			Factory.Save();

			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			parent.Databases.Add(db1);
			AssertEquals("Precondition", parent.PK, db1.LD_LE);

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = parent;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = agreement.ERA_VariantCode;
			assignment1.EAE_AllowOnlineAcceptance = true;

			AssertNoErrors(assignment1.EAE_ParentIDInfo);

			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = db1;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = agreement.ERA_VariantCode;
			assignment2.EAE_AllowOnlineAcceptance = true;

			AssertHasError(assignment2.EAE_ParentIDInfo, "There is already an Enterprise level Assignment for this Agreement Type. It must be removed if you want to specify Database level Assignments.");

			assignment2.EAE_AgreementType = "ZZZ";
			AssertNoErrors(assignment2.EAE_ParentIDInfo);
		}

		public void TestCheckEAE_ParentID_DatabaseParent()
		{
			var parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";
			Factory.Save();

			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			parent.Databases.Add(db1);
			AssertEquals("Precondition", parent.PK, db1.LD_LE);

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = db1;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = agreement.ERA_VariantCode;
			assignment1.EAE_AllowOnlineAcceptance = true;

			AssertNoErrors(assignment1.EAE_ParentIDInfo);

			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = db1;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = agreement.ERA_VariantCode;
			assignment2.EAE_AllowOnlineAcceptance = true;

			AssertHasError(assignment2.EAE_ParentIDInfo, "There is already an Assignment against this Database for this Agreement Type.");

			assignment2.EAE_AgreementType = "ZZZ";
			AssertNoErrors(assignment1.EAE_ParentIDInfo);
		}

		public void TestCheckEAE_ClientAgreementOrg_ShouldBeEnteredIfAndOnlyIfParentIsDatabase()
		{
			var parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";
			Factory.Save();

			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			parent.Databases.Add(db1);
			AssertEquals("Precondition", parent.PK, db1.LD_LE);

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.EnterpriseParent = parent;
			assignment1.Parent = db1;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = agreement.ERA_VariantCode;
			assignment1.EAE_AllowOnlineAcceptance = true;

			assignment1.EAE_OH_ClientAgreementOrg = Guid.Empty;
			AssertHasError(assignment1.EAE_OH_ClientAgreementOrgInfo, "Client Agreement Organization must be entered on Database Level Assignments.");

			assignment1.EAE_OH_ClientAgreementOrg = org.PK;
			AssertNoError(assignment1.EAE_OH_ClientAgreementOrgInfo, "Client Agreement Organization must be entered on Database Level Assignments.");

			assignment1.Parent = parent;
			AssertEquals("Precondition: Should have cleared agreement org", true, assignment1.EAE_OH_ClientAgreementOrg.IsEmpty);
			AssertNoError(assignment1.EAE_OH_ClientAgreementOrgInfo, "Client Agreement Organization must be entered on Database Level Assignments.");

			assignment1.EAE_OH_ClientAgreementOrg = org.PK;
			AssertHasError(assignment1.EAE_OH_ClientAgreementOrgInfo, "Enterprise Level Assignments cannot have Client Agreement Organization overrides.");
		}

		public void TestCheckEAE_ParentID_ShouldShowErrorIfDatabaseLevelWithInvalidParent()
		{
			var parent = Factory.NewWithValidTestData<LicenceEnterprise>();
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			parent.Databases.Add(db1);
			parent.Databases.Add(db2);

			AssertEquals("Precondition", parent.PK, db1.LD_LE);
			AssertEquals("Precondition", parent.PK, db2.LD_LE);

			db1.LD_Product = ProductTypes.Codes.CargoWiseOne;
			db2.LD_Product = ProductTypes.Codes.CargoSphere;
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EnterpriseParent = parent;
			var lookups = new EdiUserAgreementAssignmentLookups(assignment1);
			AssertEquals("Precondition: CWN agreements should only allow particular products", 1, lookups.LicenceDatabaseParents.Count);

			assignment1.Parent = db1;
			AssertNoErrors("Should not have error since parent db is from the lookup list", assignment1.EAE_ParentIDInfo);

			assignment1.Parent = db2;
			AssertHasError("Should have error since parent db is from the lookup list", assignment1.EAE_ParentIDInfo, "Enter a valid Parent.");

			assignment1.Parent = parent;
			AssertNoErrors("Should not have error since parent is the enterprise", assignment1.EAE_ParentIDInfo);
		}
	}
}
