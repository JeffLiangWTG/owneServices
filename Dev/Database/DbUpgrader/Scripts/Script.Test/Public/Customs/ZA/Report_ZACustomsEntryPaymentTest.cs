using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZA.Testing
{
	[TestedType(typeof(Report_ZACustomsEntryPayment))]
	class Report_ZACustomsEntryPaymentTest : DbCreateScriptTest
	{
		public void TestFunctionalityOfTheFunction()
		{
			var orgPk = TestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			TestDataCreator.CreateOrgCusCode(orgPk, "AGT", "12345", "ZA");
			var org2Pk = TestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER");
			TestDataCreator.CreateOrgCusCode(org2Pk, "AGT", "12345", "ZA");

			var sql = $@"DECLARE @departmentPK UNIQUEIDENTIFIER 
SET @departmentPK = NEWID()
INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code, GE_Desc) values (@departmentPK, 'DP$', 'DPDESC') 

DECLARE @usDeclarationPK UNIQUEIDENTIFIER, @zaDeclarationPK UNIQUEIDENTIFIER
SET @usDeclarationPK = NEWID()
SET @zaDeclarationPK = NEWID()
INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ApplicationCode, JE_ClusterKey) values(@usDeclarationPK, 'ZA', 'BUS100TEST', '{usBranchPK}', '{usCompanyPK}', 'BLT', 1)
INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ApplicationCode, JE_CustomsOffice, JE_AddInfo, JE_ClusterKey) values(@zaDeclarationPK, 'ZA', 'BZA100TEST', '{zaBranchPK}', '{zaCompanyPK}', 'BLT', 'AAA', 'AGTCode=12345', 2)

DECLARE @usEntryPK UNIQUEIDENTIFIER, @zaEntryPK UNIQUEIDENTIFIER
SET @usEntryPK = NEWID()
SET @zaEntryPK = NEWID()
INSERT INTO dbo.CusEntryHeader(CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
values
(@usEntryPK, 'US', @usDeclarationPK, 'ENT1', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(@zaEntryPK, 'ZA', @zaDeclarationPK, 'ENT2', 2, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO dbo.CusEntryPayInfo(C9_PK, C9_CH, C9_PaymentDate, C9_PaymentAmount, C9_TransactionType, C9_PaymentParty, C9_ClusterKey, C9_SystemCreateTimeUtc, C9_SystemCreateUser, C9_SystemLastEditTimeUtc, C9_SystemLastEditUser)
values
(newid(), @zaEntryPK, '2016-01-01',1, 'DTY', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-01-01',1, 'VAT', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-01-01',1, 'VAT', 'V', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-01-01',1, 'DTY', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-01-01',1, 'VAT', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-01-01',1, 'OTH', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2, 'DTY', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.1, 'DTY', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.2, 'DTY', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',1.1, 'DTY', 'V', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.3, 'VAT', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.4, 'OTH', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.5, 'PEN', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.7, 'PPA', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-01',3.1, 'DTY', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-01',3.2, 'VAT', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-01',3.3, 'VAT', 'V', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-02',4, 'DTY', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-02',4, 'VAT', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-02',4, 'VAT', 'V', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-02',4, 'DTY', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-02',4, 'OTH', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @usEntryPK, '2016-12-01',55, 'DTY', 'D', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @usEntryPK, '2016-12-01',55, 'VAT', 'D', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @usEntryPK, '2016-12-01',55, 'VAT', 'V', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @usEntryPK, '2016-12-01',55, 'DTY', 'C', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @usEntryPK, '2016-12-01',55, 'VAT', 'C', 1, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @usEntryPK, '2016-12-01',55, 'OTH', 'C', 1, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO dbo.CusEntryPayInfo(C9_PK, C9_CH, C9_PaymentDate, C9_PaymentAmount, C9_TransactionType, C9_PaymentParty, C9_PaymentReference, C9_ClusterKey, C9_SystemCreateTimeUtc, C9_SystemCreateUser, C9_SystemLastEditTimeUtc, C9_SystemLastEditUser)
values(newid(), @zaEntryPK, '2016-11-01',2.6, 'PEN', 'C', 'REF1', 2, getutcdate(), '~BP', getutcdate(), '~BP');";

			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}

			string insertRegistryValues = $@"DECLARE @registryRawValue nvarchar(MAX)
Set @registryRawValue = '<ArrayOfFinancialAccountNumberPortMap xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<FinancialAccountNumberPortMap>
		<OrganizationPK>{orgPk}</OrganizationPK>
		<CustomsOfficeCode>AAA</CustomsOfficeCode>
		<FinancialAccountNumber>8123564989</FinancialAccountNumber>
		<Cash>N</Cash>
		<ImporterPays>Y</ImporterPays>
		<CreditorPK>00000000-0000-0000-0000-000000000000</CreditorPK>
		<AccountStartDay>31</AccountStartDay>
		<DutyDefermentAmount>0.00</DutyDefermentAmount>
		<PaymentDay>1</PaymentDay>
		<AutoAllocationAllowed>N</AutoAllocationAllowed>
		<VatDefermentAmount>999999999.99</VatDefermentAmount>
	  </FinancialAccountNumberPortMap>
	<FinancialAccountNumberPortMap>
		<OrganizationPK>{org2Pk}</OrganizationPK>
		<CustomsOfficeCode>AAA</CustomsOfficeCode>
		<FinancialAccountNumber>8123564989</FinancialAccountNumber>
		<Cash>N</Cash>
		<ImporterPays>Y</ImporterPays>
		<CreditorPK>00000000-0000-0000-0000-000000000000</CreditorPK>
		<AccountStartDay>31</AccountStartDay>
		<DutyDefermentAmount>0.00</DutyDefermentAmount>
		<PaymentDay>1</PaymentDay>
		<AutoAllocationAllowed>N</AutoAllocationAllowed>
		<VatDefermentAmount>999999999.99</VatDefermentAmount>
	  </FinancialAccountNumberPortMap>
</ArrayOfFinancialAccountNumberPortMap>'
INSERT INTO dbo.StmData
(SD_PK, SD_Name, SD_Owner, SD_Type, SD_BinaryValue, SD_IsLogged, SD_IsCancelled)
Values
(newID(), 'ZAFINANCIALACCOUNTNUMBERPORTMAPS', '{zaCompanyPK}', 'BIN', CONVERT(varbinary(MAX), @registryRawValue), 1, 0)";

			TestConnection.ExecuteNonQuery(insertRegistryValues);

			sql = $@"select * FROM Report_ZACustomsEntryPayment('{zaCompanyPK}', '20161101', '20161203', 'ALL') ORDER BY AccountingDate";
			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals("A - row 1", true, reader.Read());
						AssertEquals("A - 0", "ENT2", reader["LRNNumber"].ToString());
						AssertEquals("A - 1", new DateTime(2016, 11, 1), (DateTime)reader["AccountingDate"]);
						AssertEquals("A - 2", "4.1000", reader["DutyDeferred"].ToString());
						AssertEquals("A - 3", "0.0000", reader["VATDeferred"].ToString());
						AssertEquals("A - 4", "3.3000", reader["DutyCash"].ToString());
						AssertEquals("A - 5", "2.3000", reader["VATCash"].ToString());
						AssertEquals("A - 6", "4.9000", reader["OtherCash"].ToString());
						AssertEquals("A - 7", "4.1000", reader["TotalDeferred"].ToString());
						AssertEquals("A - 8", "10.5000", reader["TotalCash"].ToString());
						AssertEquals("A - 9", "14.6000", reader["GrandTotal"].ToString());

						AssertEquals("B - row 2", true, reader.Read());
						AssertEquals("B - 0", "ENT2", reader["LRNNumber"].ToString());
						AssertEquals("B - 1", new DateTime(2016, 12, 1), (DateTime)reader["AccountingDate"]);
						AssertEquals("B - 2", "3.1000", reader["DutyDeferred"].ToString());
						AssertEquals("B - 3", "6.5000", reader["VATDeferred"].ToString());
						AssertEquals("B - 4", "0.0000", reader["DutyCash"].ToString());
						AssertEquals("B - 5", "0.0000", reader["VATCash"].ToString());
						AssertEquals("B - 6", "0.0000", reader["OtherCash"].ToString());
						AssertEquals("B - 7", "9.6000", reader["TotalDeferred"].ToString());
						AssertEquals("B - 8", "0.0000", reader["TotalCash"].ToString());
						AssertEquals("B - 9", "9.6000", reader["GrandTotal"].ToString());

						AssertEquals("A - row 3", true, reader.Read());
						AssertEquals("A - 0", "ENT2", reader["LRNNumber"].ToString());
						AssertEquals("A - 1", new DateTime(2016, 12, 2), (DateTime)reader["AccountingDate"]);
						AssertEquals("A - 2", "4.0000", reader["DutyDeferred"].ToString());
						AssertEquals("A - 3", "8.0000", reader["VATDeferred"].ToString());
						AssertEquals("A - 4", "4.0000", reader["DutyCash"].ToString());
						AssertEquals("A - 5", "0.0000", reader["VATCash"].ToString());
						AssertEquals("A - 6", "4.0000", reader["OtherCash"].ToString());
						AssertEquals("A - 7", "12.0000", reader["TotalDeferred"].ToString());
						AssertEquals("A - 8", "8.0000", reader["TotalCash"].ToString());
						AssertEquals("A - 9", "20.0000", reader["GrandTotal"].ToString());

						AssertEquals("Three rows for three entries", false, reader.Read());
					});
				}
			}
		}

		public void TestFunctionalityOfTheFunction_OverrideCustomsOffice()
		{
			var orgPk = TestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			TestDataCreator.CreateOrgCusCode(orgPk, "AGT", "12345", "ZA");
			var org2Pk = TestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER");
			TestDataCreator.CreateOrgCusCode(org2Pk, "AGT", "12345", "ZA");

			var sql = $@"DECLARE @departmentPK UNIQUEIDENTIFIER 
SET @departmentPK = NEWID()
INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code, GE_Desc) values (@departmentPK, 'DP$', 'DPDESC') 

DECLARE @zaDeclarationPK UNIQUEIDENTIFIER
SET @zaDeclarationPK = NEWID()
INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ApplicationCode, JE_CustomsOffice, JE_AddInfo, JE_ClusterKey, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES (@zaDeclarationPK, 'ZA', 'BZA100TEST', '{zaBranchPK}', '{zaCompanyPK}', 'BLT', 'AAA', 'AGTCode=12345', 2, getutcdate(), '~BP', getutcdate(), '~BP')

DECLARE @zaInstructionPK UNIQUEIDENTIFIER
SET @zaInstructionPK = NEWID()
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_DateForDuty, CEI_ClusterKey, CEI_AddInfo, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
VALUES (@zaInstructionPK, 'ZA', @zaDeclarationPK, '2016-01-01', 2, 'CustomsOfficeOverride=BBB', getutcdate(), '~BP', getutcdate(), '~BP')

DECLARE @zaEntryPK UNIQUEIDENTIFIER
SET @zaEntryPK = NEWID()
INSERT INTO dbo.CusEntryHeader(CH_PK, CH_DataModel, CH_JE, CH_BGMReference, CH_ClusterKey, CH_CEI_Instruction, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@zaEntryPK, 'ZA', @zaDeclarationPK, 'ENT2', 2, @zaInstructionPK, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO dbo.CusEntryPayInfo(C9_PK, C9_CH, C9_PaymentDate, C9_PaymentAmount, C9_TransactionType, C9_PaymentParty, C9_ClusterKey, C9_SystemCreateTimeUtc, C9_SystemCreateUser, C9_SystemLastEditTimeUtc, C9_SystemLastEditUser)
values
(newid(), @zaEntryPK, '2016-01-01',1, 'DTY', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-01-01',1, 'VAT', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-01-01',1, 'VAT', 'V', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-01-01',1, 'DTY', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-01-01',1, 'VAT', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-01-01',1, 'OTH', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2, 'DTY', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.1, 'DTY', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.2, 'DTY', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',1.1, 'DTY', 'V', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.3, 'VAT', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.4, 'OTH', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.5, 'PEN', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-11-01',2.7, 'PPA', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-01',3.1, 'DTY', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-01',3.2, 'VAT', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-01',3.3, 'VAT', 'V', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-02',4, 'DTY', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-02',4, 'VAT', 'D', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-02',4, 'VAT', 'V', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-02',4, 'DTY', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP'),
(newid(), @zaEntryPK, '2016-12-02',4, 'OTH', 'C', 2, getutcdate(), '~BP', getutcdate(), '~BP');

INSERT INTO dbo.CusEntryPayInfo(C9_PK, C9_CH, C9_PaymentDate, C9_PaymentAmount, C9_TransactionType, C9_PaymentParty, C9_PaymentReference, C9_ClusterKey, C9_SystemCreateTimeUtc, C9_SystemCreateUser, C9_SystemLastEditTimeUtc, C9_SystemLastEditUser)
values(newid(), @zaEntryPK, '2016-11-01',2.6, 'PEN', 'C', 'REF1', 2, getutcdate(), '~BP', getutcdate(), '~BP');";

			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}

			string insertRegistryValues = $@"DECLARE @registryRawValue nvarchar(MAX)
Set @registryRawValue = '<ArrayOfFinancialAccountNumberPortMap xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<FinancialAccountNumberPortMap>
		<OrganizationPK>{orgPk}</OrganizationPK>
		<CustomsOfficeCode>BBB</CustomsOfficeCode>
		<FinancialAccountNumber>8123564989</FinancialAccountNumber>
		<Cash>N</Cash>
		<ImporterPays>Y</ImporterPays>
		<CreditorPK>00000000-0000-0000-0000-000000000000</CreditorPK>
		<AccountStartDay>31</AccountStartDay>
		<DutyDefermentAmount>0.00</DutyDefermentAmount>
		<PaymentDay>1</PaymentDay>
		<AutoAllocationAllowed>N</AutoAllocationAllowed>
		<VatDefermentAmount>999999999.99</VatDefermentAmount>
	  </FinancialAccountNumberPortMap>
	<FinancialAccountNumberPortMap>
		<OrganizationPK>{org2Pk}</OrganizationPK>
		<CustomsOfficeCode>BBB</CustomsOfficeCode>
		<FinancialAccountNumber>8123564989</FinancialAccountNumber>
		<Cash>N</Cash>
		<ImporterPays>Y</ImporterPays>
		<CreditorPK>00000000-0000-0000-0000-000000000000</CreditorPK>
		<AccountStartDay>31</AccountStartDay>
		<DutyDefermentAmount>0.00</DutyDefermentAmount>
		<PaymentDay>1</PaymentDay>
		<AutoAllocationAllowed>N</AutoAllocationAllowed>
		<VatDefermentAmount>999999999.99</VatDefermentAmount>
	  </FinancialAccountNumberPortMap>
</ArrayOfFinancialAccountNumberPortMap>'
INSERT INTO dbo.StmData
(SD_PK, SD_Name, SD_Owner, SD_Type, SD_BinaryValue, SD_IsLogged, SD_IsCancelled)
Values
(newID(), 'ZAFINANCIALACCOUNTNUMBERPORTMAPS', '{zaCompanyPK}', 'BIN', CONVERT(varbinary(MAX), @registryRawValue), 1, 0)";

			TestConnection.ExecuteNonQuery(insertRegistryValues);

			sql = $@"select * FROM Report_ZACustomsEntryPayment('{zaCompanyPK}', '20161101', '20161203', 'ALL') ORDER BY AccountingDate";
			using (var command = CargoWise.Data.Db.Connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						AssertEquals("A - row 1", true, reader.Read());
						AssertEquals("A - 0", "ENT2", reader["LRNNumber"].ToString());
						AssertEquals("A - 1", new DateTime(2016, 11, 1), (DateTime)reader["AccountingDate"]);
						AssertEquals("A - 2", "4.1000", reader["DutyDeferred"].ToString());
						AssertEquals("A - 3", "0.0000", reader["VATDeferred"].ToString());
						AssertEquals("A - 4", "3.3000", reader["DutyCash"].ToString());
						AssertEquals("A - 5", "2.3000", reader["VATCash"].ToString());
						AssertEquals("A - 6", "4.9000", reader["OtherCash"].ToString());
						AssertEquals("A - 7", "4.1000", reader["TotalDeferred"].ToString());
						AssertEquals("A - 8", "10.5000", reader["TotalCash"].ToString());
						AssertEquals("A - 9", "14.6000", reader["GrandTotal"].ToString());

						AssertEquals("B - row 2", true, reader.Read());
						AssertEquals("B - 0", "ENT2", reader["LRNNumber"].ToString());
						AssertEquals("B - 1", new DateTime(2016, 12, 1), (DateTime)reader["AccountingDate"]);
						AssertEquals("B - 2", "3.1000", reader["DutyDeferred"].ToString());
						AssertEquals("B - 3", "6.5000", reader["VATDeferred"].ToString());
						AssertEquals("B - 4", "0.0000", reader["DutyCash"].ToString());
						AssertEquals("B - 5", "0.0000", reader["VATCash"].ToString());
						AssertEquals("B - 6", "0.0000", reader["OtherCash"].ToString());
						AssertEquals("B - 7", "9.6000", reader["TotalDeferred"].ToString());
						AssertEquals("B - 8", "0.0000", reader["TotalCash"].ToString());
						AssertEquals("B - 9", "9.6000", reader["GrandTotal"].ToString());

						AssertEquals("A - row 3", true, reader.Read());
						AssertEquals("A - 0", "ENT2", reader["LRNNumber"].ToString());
						AssertEquals("A - 1", new DateTime(2016, 12, 2), (DateTime)reader["AccountingDate"]);
						AssertEquals("A - 2", "4.0000", reader["DutyDeferred"].ToString());
						AssertEquals("A - 3", "8.0000", reader["VATDeferred"].ToString());
						AssertEquals("A - 4", "4.0000", reader["DutyCash"].ToString());
						AssertEquals("A - 5", "0.0000", reader["VATCash"].ToString());
						AssertEquals("A - 6", "4.0000", reader["OtherCash"].ToString());
						AssertEquals("A - 7", "12.0000", reader["TotalDeferred"].ToString());
						AssertEquals("A - 8", "8.0000", reader["TotalCash"].ToString());
						AssertEquals("A - 9", "20.0000", reader["GrandTotal"].ToString());

						AssertEquals("Three rows for three entries", false, reader.Read());
					});
				}
			}
		}
		Guid usCompanyPK;
		Guid usBranchPK;
		Guid zaCompanyPK;
		Guid zaBranchPK;

		protected override void SetUp()
		{
			base.SetUp();
			usCompanyPK = TestDataCreator.CreateCompany("US#", "USD", "US");
			usBranchPK = TestDataCreator.CreateBranch(usCompanyPK, "US$", "USMIA");
			zaCompanyPK = TestDataCreator.CreateCompany("ZA#", "USD", "ZA");
			zaBranchPK = TestDataCreator.CreateBranch(zaCompanyPK, "ZA$", "ZAJNB");
		}
	}
}

