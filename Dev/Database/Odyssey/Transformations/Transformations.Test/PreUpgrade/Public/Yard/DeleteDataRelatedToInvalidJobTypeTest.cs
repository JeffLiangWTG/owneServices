using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Freight.ContainerYard;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PreUpgrade.Public.Yard
{
	[TestedType(typeof(DeleteDataRelatedToInvalidJobType))]
	public class DeleteDataRelatedToInvalidJobTypeTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertDataWithInvalidJobTypeIsDeleted("AccChargeBranchOverride", "YA_JobType");
			AssertDataWithInvalidJobTypeIsDeleted("AccChargeComplianceDescription", "ADE_JobType");
			AssertDataWithInvalidJobTypeIsDeleted("AccChargeCreditorOverride", "ACC_JobType");
			AssertDataWithInvalidJobTypeIsDeleted("AccChargeGLPostingOverride", "Y1_JobType");
			AssertDataWithInvalidJobTypeIsDeleted("AccChargeGovtChargeCodeOverride", "ACG_JobType");
			AssertDataWithInvalidJobTypeIsDeleted("AccChargeSupplyTypeOverride", "ACS_JobType");
			AssertDataWithInvalidJobTypeIsDeleted("AccChargeTaxOverride", "AO_JobType");
			AssertDataWithInvalidJobTypeIsDeleted("AccSurchargeApplication", "ASP_JobType");
			AssertDataWithInvalidJobTypeIsDeleted("RatingDateConfig", "RDT_JobType");
			AssertDataWithInvalidJobTypeIsDeleted("AccChargeRevRecOverride", "AE_JobType");

			var sqlText = @"SELECT count(*) FROM dbo.OrgARTerms WHERE PY_JobType IN ('CYI', 'CYO', 'CYS')";
			AssertEquals("OrgARTerms should not have data with the invalid job types", 0, Db.Connection.ExecuteScalar(sqlText));
		}

		public void AssertDataWithInvalidJobTypeIsDeleted(string tableName, string columnName)
		{
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT {0} FROM dbo.{1}", columnName, tableName));

			AssertEquals(1, dataTable.Rows.Count);
			AssertEquals("ALL", dataTable.Rows[0][columnName]);
		}

		protected override void PrepareTestData()
		{
			DeleteConstranitsWithInvalidJobTypes();

			var sql = $@"
DECLARE @companyPK UNIQUEIDENTIFIER;
DECLARE @branchPK UNIQUEIDENTIFIER;
DECLARE @departmentPK UNIQUEIDENTIFIER;
DECLARE @chargeCodePK UNIQUEIDENTIFIER;
DECLARE @orgHeaderPK UNIQUEIDENTIFIER;
DECLARE @taxRatePK UNIQUEIDENTIFIER;
DECLARE @orgCompanyDataPK UNIQUEIDENTIFIER;
 
set @companyPK = (select top 1 GC_PK from GlbCompany)
set @branchPK = (select top 1 GB_PK from GlbBranch)
set @departmentPK = (select top 1 GE_PK from GlbDepartment)
set @chargeCodePK = (select top 1 AC_PK from AccChargeCode)
set @orgHeaderPK = (select top 1 OH_PK from OrgHeader)
set @taxRatePK = (select top 1 AT_PK from AccTaxRate)
set @orgCompanyDataPK = (select top 1 OB_PK from OrgCompanyData)
 
INSERT INTO dbo.AccChargeBranchOverride
(YA_PK, YA_AC_ChargeCode, YA_JobType, YA_Direction, YA_TransportMode, YA_DefaultingRule, YA_GB_SpecificBranch, YA_SystemCreateTimeUtc, YA_SystemCreateUser, YA_SystemLastEditTimeUtc, YA_SystemLastEditUser)
VALUES
(NEWID(), @chargeCodePK, 'ALL', '', '', 'SBA', @branchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYI', '', '', 'SBA', @branchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYO', '', '', 'SBA', @branchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYS', '', '', 'SBA', @branchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
 
INSERT INTO dbo.AccChargeComplianceDescription (ADE_PK, ADE_AC, ADE_JobType, ADE_TransportMode, ADE_SupplyType, ADE_Description, ADE_SystemCreateTimeUtc, ADE_SystemCreateUser, ADE_SystemLastEditTimeUtc, ADE_SystemLastEditUser)
VALUES
(NEWID(), @chargeCodePK, 'ALL', '', '', 'desc', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYI', '', '', 'desc', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYO', '', '', 'desc', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYS', '', '', 'desc', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
 
INSERT INTO dbo.AccChargeCreditorOverride (ACC_PK, ACC_AC_ChargeCode, ACC_JobType, ACC_Direction, ACC_TransportMode, ACC_DefaultingRule, ACC_GE_Department, ACC_OH_Creditor, ACC_CreditorRole, ACC_PaymentTerm, ACC_SystemCreateTimeUtc, ACC_SystemCreateUser, ACC_SystemLastEditTimeUtc, ACC_SystemLastEditUser)
VALUES
(NEWID(), @chargeCodePK, 'ALL', '', '', 'SCA', @departmentPK, @orgHeaderPK, '', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYI', '', '', 'SCA', @departmentPK, @orgHeaderPK, '', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYO', '', '', 'SCA', @departmentPK, @orgHeaderPK, '', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYS', '', '', 'SCA', @departmentPK, @orgHeaderPK, '', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
 
INSERT INTO dbo.AccChargeGLPostingOverride (Y1_PK, Y1_AC, Y1_ConsolidationAccountingCategoryClass, Y1_ConsolContainerMode, Y1_Direction, Y1_HousePaymentType, Y1_JobType, Y1_MasterPaymentType, Y1_TransportMode, Y1_SystemCreateTimeUtc, Y1_SystemCreateUser, Y1_SystemLastEditTimeUtc, Y1_SystemLastEditUser)
VALUES 
(NEWID(), @chargeCodePK, 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'ALL', 'ALL', 'ALL', 'ALL', 'CYI', 'ALL', 'ALL', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'ALL', 'ALL', 'ALL', 'ALL', 'CYO', 'ALL', 'ALL', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'ALL', 'ALL', 'ALL', 'ALL', 'CYS', 'ALL', 'ALL', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
 
INSERT INTO dbo.AccChargeGovtChargeCodeOverride (ACG_PK, ACG_AC, ACG_JobType, ACG_TransportMode, ACG_Direction, ACG_GovtChargeCode, ACG_SystemCreateTimeUtc, ACG_SystemCreateUser, ACG_SystemLastEditTimeUtc, ACG_SystemLastEditUser)
VALUES 
(NEWID(), @chargeCodePK, 'ALL', 'ALL', 'ALL', 'XXX', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYI', 'ALL', 'ALL', 'XXX', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYO', 'ALL', 'ALL', 'XXX', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @chargeCodePK, 'CYS', 'ALL', 'ALL', 'XXX', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
 
INSERT INTO dbo.AccChargeRevRecOverride (AE_PK, AE_JobType, AE_Direction, AE_Mode, AE_BrokerType, AE_RecognitionType, AE_AC, AE_SystemCreateTimeUtc, AE_SystemCreateUser, AE_SystemLastEditTimeUtc, AE_SystemLastEditUser)
VALUES 
(NEWID(), 'ALL', 'ALL', 'ALL', 'ALL', 'ARV', @chargeCodePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'CYI', 'ALL', 'ALL', 'ALL', 'ARV', @chargeCodePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'CYO', 'ALL', 'ALL', 'ALL', 'ARV', @chargeCodePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'CYS', 'ALL', 'ALL', 'ALL', 'ARV', @chargeCodePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
 
INSERT INTO dbo.AccChargeSupplyTypeOverride (ACS_PK, ACS_ParentTableCode, ACS_ParentID, ACS_JobType, ACS_TransportMode, ACS_Direction, ACS_IncoTerm, ACS_GE, ACS_SupplyType, ACS_SystemCreateTimeUtc, ACS_SystemCreateUser, ACS_SystemLastEditTimeUtc, ACS_SystemLastEditUser)
VALUES 
(NEWID(), 'AC', @chargeCodePK, 'ALL', 'ALL', 'ALL', 'ALL', @departmentPK, 'LOC', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'AC', @chargeCodePK, 'CYI', 'ALL', 'ALL', 'ALL', @departmentPK, 'LOC', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'AC', @chargeCodePK, 'CYO', 'ALL', 'ALL', 'ALL', @departmentPK, 'LOC', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'AC', @chargeCodePK, 'CYS', 'ALL', 'ALL', 'ALL', @departmentPK, 'LOC', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
 
INSERT INTO dbo.AccChargeTaxOverride (AO_PK, AO_AT, AO_ParentID, AO_CostSellAll, AO_Direction, AO_IncoTerm, AO_JobType, AO_TaxRegCntryOrGroup, AO_Origin, AO_Destination, AO_TransportMode,AO_OrganisationCategory, AO_HomeCountryOrZone, AO_VATExemptOnExportCharges, AO_SplitPaymentVATOrganisation, AO_SupplyType, AO_DebtorRole, AO_CreateTaxRecord, AO_SystemCreateTimeUtc, AO_SystemCreateUser, AO_SystemLastEditTimeUtc, AO_SystemLastEditUser)
VALUES 
(NEWID(), @taxRatePK, @chargeCodePK, 'COS', 'ALL', 'CIF', 'ALL', 'ALL', 'ALL', 'ALL', 'ALL','ALL', '', 0, 0, 'LOC', '', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @taxRatePK, @chargeCodePK, 'COS', 'ALL', 'CIF', 'CYI', 'ALL', 'ALL', 'ALL', 'ALL','ALL', '', 0, 0, 'LOC', '', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @taxRatePK, @chargeCodePK, 'COS', 'ALL', 'CIF', 'CYO', 'ALL', 'ALL', 'ALL', 'ALL','ALL', '', 0, 0, 'LOC', '', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), @taxRatePK, @chargeCodePK, 'COS', 'ALL', 'CIF', 'CYS', 'ALL', 'ALL', 'ALL', 'ALL','ALL', '', 0, 0, 'LOC', '', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
 
INSERT INTO dbo.AccSurchargeApplication (ASP_PK, ASP_IsValid, ASP_JobType, ASP_SupplyType, ASP_HomeCountryOrZone, ASP_OrganizationCategory, ASP_PlaceOfSupplyType, ASP_PlaceOfSupply, ASP_ASC_NKSurchargeCode , ASP_GC_Company, ASP_SystemCreateTimeUtc, ASP_SystemCreateUser, ASP_SystemLastEditTimeUtc, ASP_SystemLastEditUser, ASP_AT)
VALUES 
(NEWID(), 1, 'ALL', '', '', 'ALL', '', 'ALL', 'XXX', @companyPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @taxRatePK),
(NEWID(), 1, 'CYI', '', '', 'ALL', '', 'ALL', 'XXX', @companyPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @taxRatePK),
(NEWID(), 1, 'CYO', '', '', 'ALL', '', 'ALL', 'XXX', @companyPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @taxRatePK),
(NEWID(), 1, 'CYS', '', '', 'ALL', '', 'ALL', 'XXX', @companyPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @taxRatePK);
 
INSERT INTO dbo.OrgARTerms (PY_PK, PY_IsValid, PY_InvoiceClass, PY_InvoiceTerm, PY_InvoiceDays, PY_AgreedPaymentMethod, PY_JobType, PY_Direction, PY_TransportMode, PY_GE_Department, PY_GB_Branch, PY_OB, PY_SystemCreateTimeUtc, PY_SystemCreateUser, PY_SystemLastEditTimeUtc, PY_SystemLastEditUser)
VALUES	
(NEWID(), 1, 'ALL', 'ALL', 0, 'ALL', 'ALL', 'ALL', 'ALL', @departmentPK, @branchPK, @orgCompanyDataPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 1, 'ALL', 'ALL', 0, 'ALL', 'CYI', 'ALL', 'ALL', @departmentPK, @branchPK, @orgCompanyDataPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 1, 'ALL', 'ALL', 0, 'ALL', 'CYO', 'ALL', 'ALL', @departmentPK, @branchPK, @orgCompanyDataPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 1, 'ALL', 'ALL', 0, 'ALL', 'CYS', 'ALL', 'ALL', @departmentPK, @branchPK, @orgCompanyDataPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
 
INSERT INTO dbo.RatingDateConfig (RDT_PK, RDT_AutoVersion,RDT_GC_Company, RDT_ParentTableCode, RDT_ParentID, RDT_ChargeGroup, RDT_JobType, RDT_Direction, RDT_TransportMode, RDT_RateType, RDT_ContainerMode, RDT_Location, RDT_AutoratingDate, RDT_NoFallback, RDT_SystemCreateTimeUtc, RDT_SystemCreateUser, RDT_SystemLastEditTimeUtc, RDT_SystemLastEditUser)
VALUES 
(NEWID(), 1, @companyPK, 'OH', @orgHeaderPK, 'FRT', 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', 'ARV', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 1, @companyPK, 'OH', @orgHeaderPK, 'FRT', 'CYI', 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', 'ARV', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 1, @companyPK, 'OH', @orgHeaderPK, 'FRT', 'CYO', 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', 'ARV', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 1, @companyPK, 'OH', @orgHeaderPK, 'FRT', 'CYS', 'ALL', 'ALL', 'ALL', 'ALL', 'ALL', 'ARV', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
";
			TestConnection.ExecuteNonQuery(sql);
		}

		void DeleteConstranitsWithInvalidJobTypes()
		{
			var sql = @"
ALTER TABLE AccChargeBranchOverride DROP CONSTRAINT CHK_YA_JobType;
ALTER TABLE AccChargeComplianceDescription DROP CONSTRAINT Constraint_ADE_JobType;
ALTER TABLE AccChargeCreditorOverride DROP CONSTRAINT Constraint_ACC_JobType;
ALTER TABLE AccChargeGLPostingOverride DROP CONSTRAINT Constraint_Y1_JobType;
ALTER TABLE AccChargeGovtChargeCodeOverride DROP CONSTRAINT Constraint_ACG_JobType;
ALTER TABLE AccChargeRevRecOverride DROP CONSTRAINT CHK_AE_JobType ;
ALTER TABLE AccChargeSupplyTypeOverride DROP CONSTRAINT Constraint_ACS_JobType;
ALTER TABLE AccChargeTaxOverride DROP CONSTRAINT Constraint_AO_JobType;
ALTER TABLE AccSurchargeApplication DROP CONSTRAINT Constraint_ASP_JobType;
ALTER TABLE OrgARTerms DROP CONSTRAINT Constraint_PY_JobType;
ALTER TABLE RatingDateConfig DROP CONSTRAINT Constraint_RDT_JobType;";

			TestConnection.ExecuteNonQuery(sql);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new DeleteDataRelatedToInvalidJobType();
		}
	}
}


