using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.ReportFunctions.Customs
{
	[TestedType(typeof(Report_ZACustomsEntryPayment))]
	sealed class Report_ZACustomsEntryPaymentTest : BiCreateScriptTest
	{
		public void TestSQLHasNotChanged()
		{
			var expectedText = @"--DROP FUNCTION Report_ZACustomsEntryPayment
CREATE FUNCTION [dbo].[Report_ZACustomsEntryPayment]
(
	@CurrentCompany AS UNIQUEIDENTIFIER,
	@FromDate AS SMALLDATETIME,
	@ToDate AS SMALLDATETIME,
	@PaymentType AS VARCHAR(3)
)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
WITH RelatedPayInfos(PaymentDate, EntryHeaderKey, DutyDeferred, VATDeferred, DutyCash, VATCash, OtherCash) AS
(
	SELECT PaymentDate, EntryHeaderKey
	, SUM(DutyDeferred) AS DutyDeferred
	, SUM(VATDeferred) AS VATDeferred
	, SUM(DutyCash) AS DutyCash
	, SUM(VATCash) AS VATCash
	, SUM(OtherCash) AS OtherCash
	FROM (
		SELECT
		CASE WHEN TransactionType = 'DTY' AND PaymentParty = 'D' THEN PaymentAmount ELSE 0 END AS DutyDeferred
		, CASE WHEN TransactionType = 'VAT' AND PaymentParty IN ('D', 'V') THEN PaymentAmount ELSE 0 END AS VATDeferred
		, CASE WHEN TransactionType = 'DTY' AND PaymentParty IN ('C', 'V') THEN PaymentAmount ELSE 0 END AS DutyCash
		, CASE WHEN TransactionType = 'VAT' AND PaymentParty = 'C' THEN PaymentAmount ELSE 0 END AS VATCash
		, CASE WHEN TransactionType NOT IN ('DTY', 'VAT') AND PaymentParty = 'C' THEN PaymentAmount ELSE 0 END AS OtherCash
		, PaymentDate, EntryHeaderKey
		FROM Customs.BAS__CusEntryPayInfo
		WHERE ISNULL(PaymentStatus, '') <> 'PEN'
		AND PaymentDate >= @FromDate
		AND PaymentAmount != 0
		AND PaymentDate < @ToDate
		AND (
			(@PaymentType = 'DEF' AND PaymentParty IN ('D', 'V'))
			OR (@PaymentType = 'CSH' AND PaymentParty IN ('C', 'V'))
			OR (@PaymentType != 'CSH' AND @PaymentType != 'DEF' AND PaymentParty IN ('D', 'V', 'C'))
		)
		AND (
			TransactionType IN ('DTY', 'OTH', 'VAT')
			OR (TransactionType = 'PEN' AND ISNULL(PaymentReference, '') = '')
		)
	) as Base
	GROUP BY PaymentDate, EntryHeaderKey
)
SELECT declaration.[Job Number] AS 'JobNumber'
, importer.FullName AS 'Importer'
, importer.OrganizationID AS 'ImporterPK'
, CASE WHEN instruction.[Customs Office Override] <> '' THEN instruction.[Customs Office Override] ELSE declaration.[Customs Office] END AS 'CustomsOffice'
, FANMap.FinancialAccountNumber AS 'FinancialAccountNumber'
, instruction.Style AS 'CPCCode'
, instruction.Description AS 'EntryDescription'
, entryHeader.BGMReference AS 'LRNNumber'
, entryNum.EntryNum AS 'MRNNumber'
, declaration.[Master Bill Number] AS 'MasterTransportDocumentNumber'
, declaration.[House Bill Number] AS 'HouseTransportDocumentNumber'

, payInfos.PaymentDate AS 'AccountingDate'
, DutyDeferred AS 'DutyDeferred'
, VATDeferred AS 'VATDeferred'
, DutyCash AS 'DutyCash'
, VATCash AS 'VATCash'
, OtherCash AS 'OtherCash'

, ISNULL(DutyDeferred, 0) + ISNULL(VATDeferred, 0) AS 'TotalDeferred'
, ISNULL(DutyCash, 0) + ISNULL(VATCash, 0) + ISNULL(OtherCash,0) AS 'TotalCash'
, ISNULL(DutyDeferred, 0) + ISNULL(VATDeferred, 0) + ISNULL(DutyCash, 0) + ISNULL(VATCash, 0) + ISNULL(OtherCash, 0) AS 'GrandTotal'
FROM Customs.BAS__EntryHeader entryHeader
LEFT JOIN Customs.BAS__EntryNumber entryNum ON entryNum.EntryHeaderKey = entryHeader.EntryHeaderKey AND entryNum.EntryType = 'MRN' AND entryNum.CountryCode = 'ZA'
LEFT JOIN dbo.vw_ZAJobDeclaration declaration ON entryHeader.DeclarationKey = declaration.[Declaration Key]
LEFT JOIN Organization.BAS__Branch branch ON declaration.[Branch Key] = branch.BranchKey
LEFT JOIN Organization.BAS__OrganizationAddress orgAddress ON declaration.[Declarant Address Key] = orgAddress.OrganizationAddressKey
LEFT JOIN Organization.BAS__Organization importer ON declaration.[Importer Key] = importer.OrganizationKey
LEFT JOIN dbo.vw_ZACusEntryInstruction instruction ON instruction.[Declaration Key] = declaration.[Declaration Key] AND entryHeader.EntryInstructionKey = instruction.[Entry Instruction Key]
INNER JOIN RelatedPayInfos payInfos ON payInfos.EntryHeaderKey = entryHeader.EntryHeaderKey
LEFT JOIN (
	SELECT orgCusCode.CustomsRegNo AS AgentCode, financialAccountNo.OrganizationPK, financialAccountNo.CustomsOfficeCode, financialAccountNo.FinancialAccountNumber
	FROM dbo.FinancialAccountNumberPortMap(@CurrentCompany) financialAccountNo
	LEFT JOIN Organization.BAS__OrgCusCode orgCusCode ON orgCusCode.OrganizationID = financialAccountNo.OrganizationPK
	AND orgCusCode.CodeType = 'AGT'
	AND orgCusCode.CodeCountry = 'ZA'
) AS FANMap ON
FANMap.CustomsOfficeCode = CASE WHEN instruction.[Customs Office Override] <> '' THEN instruction.[Customs Office Override] ELSE declaration.[Customs Office] END
AND (
	declaration.[AGT Code] = FANMap.AgentCode AND FANMap.OrganizationPK = orgAddress.OrganizationID
)

WHERE declaration.[Application Code] = 'BLT'
AND branch.CompanyID = @CurrentCompany
";
			var sqlFunction = new Report_ZACustomsEntryPayment();
			var actualText = sqlFunction.Text;
			AssertEquals("A version of this function exists in the Odyssey database(/CargoWise.DbUpgrader/src/Scripts/Scripts.Definitions/Customs/ZA/Report_ZACustomsEntryPayment.sql), please update it.", expectedText, actualText);
		}

		public void TestFunctionalityOfTheFunction()
		{
			var (orgPk, _) = EDWTestDataCreator.CreateOrganisation("SUPPL1", "SUPPLIER");
			EDWTestDataCreator.CreateOrgCusCode(orgPk, "AGT", "12345", "ZA");
			var (org2Pk, _) = EDWTestDataCreator.CreateOrganisation("SUPPL2", "SUPPLIER");
			EDWTestDataCreator.CreateOrgCusCode(org2Pk, "AGT", "12345", "ZA");

			var sql = $@"
DECLARE @DeclarationKey BIGINT
SELECT @DeclarationKey = ISNULL(MAX(DeclarationKey), 0)
FROM {Db.EdwDatabaseName}.Customs.BAS__Declaration

DECLARE @usDeclarationKey BIGINT, @zaDeclarationKey BIGINT
SET @usDeclarationKey = @DeclarationKey + 1
SET @zaDeclarationKey = @DeclarationKey + 2
INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__Declaration(DeclarationID, DeclarationKey, DataModel, JobNumber, BranchKey, CompanyID, ApplicationCode)
values(NEWID(), @usDeclarationKey, 'ZA', 'BUS100TEST', '{usBranchKey}', '{usCompanyPK}', 'BLT')
INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__Declaration(DeclarationID, DeclarationKey, DataModel, JobNumber, BranchKey, CompanyID, ApplicationCode, CustomsOffice, AddInfo)
values(NEWID(), @zaDeclarationKey, 'ZA', 'BZA100TEST', '{zaBranchKey}', '{zaCompanyPK}', 'BLT', 'AAA', 'AGTCode=12345')

DECLARE @EntryHeaderKey BIGINT
SELECT @EntryHeaderKey = ISNULL(MAX(EntryHeaderKey), 0)
FROM {Db.EdwDatabaseName}.Customs.BAS__EntryHeader

DECLARE @usEntryKey BIGINT, @zaEntryKey BIGINT
SET @usEntryKey = @EntryHeaderKey + 1
SET @zaEntryKey = @EntryHeaderKey + 2
INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__EntryHeader(EntryHeaderID, EntryHeaderKey, DataModel, DeclarationKey, BGMReference)
values
(NEWID(), @usEntryKey, 'US', @usDeclarationKey, 'ENT1'),
(NEWID(), @zaEntryKey, 'ZA', @zaDeclarationKey, 'ENT2');

DECLARE @CusEntryPayInfoKey BIGINT
SELECT @CusEntryPayInfoKey = ISNULL(MAX(CusEntryPayInfoKey), 0)
FROM {Db.EdwDatabaseName}.Customs.BAS__CusEntryPayInfo

INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__CusEntryPayInfo(CusEntryPayInfoID, CusEntryPayInfoKey, EntryHeaderKey, PaymentDate, PaymentAmount, TransactionType, PaymentParty)
values
(newid(), @CusEntryPayInfoKey + 1, @zaEntryKey, '2016-01-01', 1, 'DTY', 'D'),
(newid(), @CusEntryPayInfoKey + 2, @zaEntryKey, '2016-01-01', 1, 'VAT', 'D'),
(newid(), @CusEntryPayInfoKey + 3, @zaEntryKey, '2016-01-01', 1, 'VAT', 'V'),
(newid(), @CusEntryPayInfoKey + 4, @zaEntryKey, '2016-01-01', 1, 'DTY', 'C'),
(newid(), @CusEntryPayInfoKey + 5, @zaEntryKey, '2016-01-01', 1, 'VAT', 'C'),
(newid(), @CusEntryPayInfoKey + 6, @zaEntryKey, '2016-01-01', 1, 'OTH', 'C'),
(newid(), @CusEntryPayInfoKey + 7, @zaEntryKey, '2016-11-01', 2, 'DTY', 'D'),
(newid(), @CusEntryPayInfoKey + 8, @zaEntryKey, '2016-11-01', 2.1, 'DTY', 'D'),
(newid(), @CusEntryPayInfoKey + 9, @zaEntryKey, '2016-11-01', 2.2, 'DTY', 'C'),
(newid(), @CusEntryPayInfoKey + 10, @zaEntryKey, '2016-11-01', 1.1, 'DTY', 'V'),
(newid(), @CusEntryPayInfoKey + 11, @zaEntryKey, '2016-11-01', 2.3, 'VAT', 'C'),
(newid(), @CusEntryPayInfoKey + 12, @zaEntryKey, '2016-11-01', 2.4, 'OTH', 'C'),
(newid(), @CusEntryPayInfoKey + 13, @zaEntryKey, '2016-11-01', 2.5, 'PEN', 'C'),
(newid(), @CusEntryPayInfoKey + 14, @zaEntryKey, '2016-11-01', 2.7, 'PPA', 'C'),
(newid(), @CusEntryPayInfoKey + 15, @zaEntryKey, '2016-12-01', 3.1, 'DTY', 'D'),
(newid(), @CusEntryPayInfoKey + 16, @zaEntryKey, '2016-12-01', 3.2, 'VAT', 'D'),
(newid(), @CusEntryPayInfoKey + 17, @zaEntryKey, '2016-12-01', 3.3, 'VAT', 'V'),
(newid(), @CusEntryPayInfoKey + 18, @zaEntryKey, '2016-12-02', 4, 'DTY', 'D'),
(newid(), @CusEntryPayInfoKey + 19, @zaEntryKey, '2016-12-02', 4, 'VAT', 'D'),
(newid(), @CusEntryPayInfoKey + 20, @zaEntryKey, '2016-12-02', 4, 'VAT', 'V'),
(newid(), @CusEntryPayInfoKey + 21, @zaEntryKey, '2016-12-02', 4, 'DTY', 'C'),
(newid(), @CusEntryPayInfoKey + 22, @zaEntryKey, '2016-12-02', 4, 'OTH', 'C'),
(newid(), @CusEntryPayInfoKey + 23, @usEntryKey, '2016-12-01', 55, 'DTY', 'D'),
(newid(), @CusEntryPayInfoKey + 24, @usEntryKey, '2016-12-01', 55, 'VAT', 'D'),
(newid(), @CusEntryPayInfoKey + 25, @usEntryKey, '2016-12-01', 55, 'VAT', 'V'),
(newid(), @CusEntryPayInfoKey + 26, @usEntryKey, '2016-12-01', 55, 'DTY', 'C'),
(newid(), @CusEntryPayInfoKey + 27, @usEntryKey, '2016-12-01', 55, 'VAT', 'C'),
(newid(), @CusEntryPayInfoKey + 28, @usEntryKey, '2016-12-01', 55, 'OTH', 'C');

INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__CusEntryPayInfo(CusEntryPayInfoID, CusEntryPayInfoKey, EntryHeaderKey, PaymentDate, PaymentAmount, TransactionType, PaymentParty, PaymentReference)
values(newid(), @CusEntryPayInfoKey + 29, @zaEntryKey, '2016-11-01', 2.6, 'PEN', 'C', 'REF1');";

			using (var command = Db.Connection.Command(sql))
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

DECLARE @ZAFinancialAccountNumberPortMapsKey BIGINT
SELECT @ZAFinancialAccountNumberPortMapsKey = ISNULL(MAX(ZAFinancialAccountNumberPortMapsKey), 0) + 1
FROM {Db.EdwDatabaseName}.Customs.BAS__ZAFinancialAccountNumberPortMaps

INSERT INTO {Db.EdwDatabaseName}.Customs.BAS__ZAFinancialAccountNumberPortMaps(ZAFinancialAccountNumberPortMapsID, ZAFinancialAccountNumberPortMapsKey, OwnerID, CompanyKey, Value)
Values (newID(), @ZAFinancialAccountNumberPortMapsKey, '{zaCompanyPK}', {zaCompanyKey}, CONVERT(varbinary(MAX), @registryRawValue))";

			TestConnection.ExecuteNonQuery(insertRegistryValues);

			sql = $"select * FROM {Db.EdwDatabaseName}.dbo.Report_ZACustomsEntryPayment('{zaCompanyPK}', '20161101', '20161203', 'ALL') ORDER BY AccountingDate";
			using (var command = Db.Connection.Command(sql))
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

		protected override void SetUp()
		{
			base.SetUp();
			(usCompanyPK, _) = EDWTestDataCreator.CreateCompany("US#", "USD", "US");
			(_, usBranchKey) = EDWTestDataCreator.CreateBranch(usCompanyPK, "US$", "USMIA");
			(zaCompanyPK, zaCompanyKey) = EDWTestDataCreator.CreateCompany("ZA#", "USD", "ZA");
			(_, zaBranchKey) = EDWTestDataCreator.CreateBranch(zaCompanyPK, "ZA$", "ZAJNB");
		}
		Guid usCompanyPK;
		long usBranchKey;
		Guid zaCompanyPK;
		long zaCompanyKey;
		long zaBranchKey;

		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }
	}
}
