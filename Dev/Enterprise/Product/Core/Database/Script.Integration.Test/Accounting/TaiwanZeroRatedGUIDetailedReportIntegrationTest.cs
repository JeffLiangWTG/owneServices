using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	class TaiwanZeroRatedGUIDetailedReportIntegrationTest : TransactionedTestCase
	{
		public void TestHasADHComplianceSubTypeField()
		{
			InsertHeaders(11);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("Be able to get ADH_ComplianceSubType.", "TXC", result.Rows[0]["ADH_ComplianceSubType"].ToString());
		}

		public void TestFormatWhenRecordsNotExist()
		{
			InsertHeaders(11);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201812, 201212, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 12 rows.", 12, result.Rows.Count);
			AssertEquals("The report should be empty.", false, result.Rows[0]["HasData"]);
		}

		public void TestFormatWhenRecordsLessThan12()
		{
			InsertHeaders(11);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 12 rows.", 12, result.Rows.Count);

			// The header which tax amount is not equal to 0 should not be included in the result
			for (int i = 0; i < 7; i++)
			{
				AssertEquals($"Row {i} should have data.", true, result.Rows[i]["HasData"]);
			}

			// The header which tax type is not equal to FREEVAT should not be included in the result
			for (int i = 7; i < 12; i++)
			{
				AssertEquals($"Row {i} should be empty.", false, result.Rows[i]["HasData"]);
			}
		}

		public void TestFormatWhenRecordsLessThan12WithVoidRecord()
		{
			InsertHeaders(11);
			SetComplianceDocumentHeaderVoid("1");
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 12 rows.", 12, result.Rows.Count);

			for (int i = 0; i < 6; i++)
			{
				AssertEquals($"Row {i} should have data.", true, result.Rows[i]["HasData"]);
			}

			for (int i = 6; i < 12; i++)
			{
				AssertEquals($"Row {i} should be empty.", false, result.Rows[i]["HasData"]);
			}
		}

		public void TestFormatWhenRecordsEqualTo12()
		{
			InsertHeaders(13);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 12 rows.", 12, result.Rows.Count);
			AssertEquals("Row 0 - 8 should have data.", true, result.Rows[8]["HasData"]);
			AssertEquals("Row 9 - 11 should be empty.", false, result.Rows[9]["HasData"]);
			AssertEquals("Row 9 - 11 should be empty.", false, result.Rows[11]["HasData"]);
		}

		public void TestFormatWhenRecordsMoreThan12()
		{
			InsertHeaders(17);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 24 rows.", 24, result.Rows.Count);
			AssertEquals("Row 0 - 12 should have data.", true, result.Rows[0]["HasData"]);
			AssertEquals("Row 0 - 12 should have data.", true, result.Rows[12]["HasData"]);

			AssertEquals("Row 13 - 22 should be empty.", false, result.Rows[13]["HasData"]);
			AssertEquals("Row 13 - 22 should be empty.", false, result.Rows[22]["HasData"]);

			AssertEquals("Rows should order by document number.", "BB0001", result.Rows[0]["ADH_DocumentNumber"].ToString());
			AssertEquals("Rows should order by document number.", "BB0013", result.Rows[12]["ADH_DocumentNumber"].ToString());

			AssertEquals("The full name should be Revengers", "S.H.I.E.L.D", result.Rows[0]["CompanyName"].ToString());
			AssertEquals("The custom registration code should be 12345675", "12345675", result.Rows[0]["OK_CustomsRegNo"].ToString());
			AssertEquals("The supporting reason should be ABC", "ABC", result.Rows[0]["ADH_SupportingReason"].ToString());

			AssertEquals("The non custom related supporting document type should be empty", "", result.Rows[0]["SupportingDocumentType"].ToString());
			AssertEquals("The non custom related supporting document number should be empty", "", result.Rows[0]["SupportingDocumentNumber"].ToString());
			AssertEquals("The non custom related amount should be empty", "", result.Rows[0]["SupportingDocumentNumber"].ToString());

			AssertEquals("The custom related supporting document type should be test DEF", "test DEF", result.Rows[0]["CustomRelatedSupportingDocumentType"].ToString());
			AssertEquals("The custom related supporting document number should be 1", "1", result.Rows[0]["CustomRelatedSupportingDocumentNumber"].ToString());
			AssertEquals("The custom related amount should be 100", "100.0000", result.Rows[0]["CustomRelatedAmount"].ToString());

			AssertEquals("The supporting reason should be AAA", "AAA", result.Rows[12]["ADH_SupportingReason"].ToString());
			AssertEquals("The non custom related supporting document type should be test BBB", "test BBB", result.Rows[12]["SupportingDocumentType"].ToString());
			AssertEquals("The non custom related supporting document number should be 13", "13", result.Rows[12]["SupportingDocumentNumber"].ToString());
			AssertEquals("The non custom related amount should be 100", "100.0000", result.Rows[12]["Amount"].ToString());
		}

		public void TestWhenOrgDontHaveTaiwanVAT()
		{
			InsertHeaders(13);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 12 rows.", 12, result.Rows.Count);
			AssertEquals("Row 0 - 8 should have data.", true, result.Rows[8]["HasData"]);
			AssertEquals("Row 0 - 8 CompanyName should be S.H.I.E.L.D", "S.H.I.E.L.D", result.Rows[8]["CompanyName"]);
			AssertEquals("Row 0 - 8 OK_CustomsRegNo should be 12345675", "12345675", result.Rows[8]["OK_CustomsRegNo"]);

			OrgHeader.CustomsCodes.RemoveAndDeleteAll();
			Factory.Save();

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 12 rows.", 12, result.Rows.Count);
			AssertEquals("Row 0 - 8 should have data.", true, result.Rows[8]["HasData"]);
			AssertEquals("Row 0 - 8 CompanyName should be Revengers", "Revengers", result.Rows[8]["CompanyName"]);
			AssertEquals("Row 0 - 8 OK_CustomsRegNo should be null", DBNull.Value, result.Rows[8]["OK_CustomsRegNo"]);

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "S.H.I.E.L.D";
			var orgCusCode = OrgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = "TES";
			orgCusCode.OK_RN_NKCodeCountry = "AU";
			orgCusCode.OK_CustomsRegNo = "12345675";
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			Factory.Save();

			var orgCusCodeFromDb = Factory.Load<OrgHeader>(OrgHeader.PK).CustomsCodes[0];
			AssertEquals("orgCusCodeFromDb.OK_CodeType should be TES", "TES", orgCusCodeFromDb.OK_CodeType);
			AssertEquals("orgCusCodeFromDb.OK_RN_NKCodeCountry should be AU", "AU", orgCusCodeFromDb.OK_RN_NKCodeCountry);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 12 rows.", 12, result.Rows.Count);
			AssertEquals("Row 0 - 8 should have data.", true, result.Rows[8]["HasData"]);
			AssertEquals("Row 0 - 8 CompanyName should be Revengers", "Revengers", result.Rows[8]["CompanyName"]);
			AssertEquals("Row 0 - 8 OK_CustomsRegNo should be null", DBNull.Value, result.Rows[8]["OK_CustomsRegNo"]);
		}

		public void TestFallBackOfCompanyName()
		{
			InsertHeaders(13);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 12 rows.", 12, result.Rows.Count);
			AssertEquals("Row 0 - 8 should have data.", true, result.Rows[8]["HasData"]);
			AssertEquals("Row 0 - 8 CompanyName should be S.H.I.E.L.D", "S.H.I.E.L.D", result.Rows[8]["CompanyName"]);

			OrgHeader.CustomsCodes.RemoveAndDeleteAll();
			OrgAddress.OA_CompanyNameOverride = "Compliance Document Header Address Company Name";
			Factory.Save();

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 12 rows.", 12, result.Rows.Count);
			AssertEquals("Row 0 - 8 should have data.", true, result.Rows[8]["HasData"]);
			AssertEquals("Row 0 - 8 CompanyName should be Compliance Document Header Address Company Name", "Compliance Document Header Address Company Name", result.Rows[8]["CompanyName"]);

			OrgAddress.OA_CompanyNameOverride = null;
			OrgHeader.OH_FullName = "Company Full Name";
			Factory.Save();

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanZeroRatedGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}'");
			AssertEquals("The report should contain 12 rows.", 12, result.Rows.Count);
			AssertEquals("Row 0 - 8 should have data.", true, result.Rows[8]["HasData"]);
			AssertEquals("Row 0 - 8 CompanyName should be Company Full Name", "Company Full Name", result.Rows[8]["CompanyName"]);
		}

		void InsertHeaders(int nextNumber)
		{
			ComplianceSequence.XD_NextNumber = nextNumber;

			Factory.Save();

			for (int i = 1; i < nextNumber - 4; i++)
			{
				InsertHeader(i, FREEVAT, 0, 1, "ABC", "DEF", i.ToString());
			}

			InsertHeader(nextNumber - 4, FREEVAT, 0, 0, "AAA", "BBB", (nextNumber - 4).ToString());
			InsertHeader(nextNumber - 3, VAT, 0, 1, "ZZZ", "XXX", (nextNumber - 3).ToString());
			InsertHeader(nextNumber - 2, EXEMPT, 0, 1, "ZZZ", "XXX", (nextNumber - 2).ToString());
			InsertHeader(nextNumber - 1, CAPVAT, 5, 1, "ZZZ", "XXX", (nextNumber - 1).ToString());
		}

		void InsertHeader(int transactionNum, AccTaxRate taxRate, decimal taxAmount, int customRelated, string supportingReason, string supportingDocumentType, string supportingDocumentNumber)
		{
			var script = $@"
DECLARE @AH_PK UNIQUEIDENTIFIER = NEWID()
DECLARE @AL_PK UNIQUEIDENTIFIER = NEWID()
DECLARE @ADH_PK UNIQUEIDENTIFIER = NEWID()
DECLARE @ADL_PK UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.AccTransactionHeader
(
	AH_PK, 
	AH_Ledger, 
	AH_TransactionType,
	AH_TransactionNum, 
	AH_GC, 
	AH_GB, 
	AH_GE, 
	AH_OH,
	AH_Desc,
	AH_InvoiceAmount, 
	AH_GSTAmount,
	AH_OSTotal,
	AH_RX_NKTransactionCurrency,
	AH_ExchangeRate,
	AH_InvoiceDate, 
	AH_DueDate,
	AH_SystemCreateTimeUtc,
	AH_SystemCreateUser,
	AH_SystemLastEditTimeUtc,
	AH_SystemLastEditUser
)
VALUES
(
	@AH_PK,
	'AR',
	'INV',
	{transactionNum},
	'{GlbCompany.CurrentCompany.PK}',
	'{GlbBranch.CurrentBranch.PK}',
	'{GlbDepartment.CurrentDepartment.PK}',
	'{OrgHeader.PK}',
	'DESC',
	100,
	{taxAmount},
	100 + {taxAmount},
	'TWD',
	1,
	GETDATE(),
	GETDATE(),
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)

INSERT INTO dbo.Acctransactionlines 
(
	AL_PK, 
	AL_LineAmount,
	AL_OSAmount,
	AL_GSTVAT,
	AL_AH, 
	AL_GB, 
	AL_GC, 
	AL_GE, 
	AL_LineType,
	AL_AT,
	AL_SystemCreateTimeUtc,
	AL_SystemCreateUser,
	AL_SystemLastEditTimeUtc,
	AL_SystemLastEditUser
)
VALUES
(
	@AL_PK,
	100,
	100 + {taxAmount},
	{taxAmount},
	@AH_PK,
	'{GlbBranch.CurrentBranch.PK}',
	'{GlbCompany.CurrentCompany.PK}',
	'{GlbDepartment.CurrentDepartment.PK}',
	'REV',
	'{taxRate.PK}',
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)

INSERT INTO dbo.AccComplianceDocumentHeader
(
	ADH_PK,
	ADH_Ledger,
	ADH_ComplianceSubType,
	ADH_XD_ComplianceBook,
	ADH_DocumentNumber,
	ADH_DocumentDate,
	ADH_DocumentStatus,
	ADH_DocumentType,
	ADH_ReportingPeriod,
	ADH_OH_Organisation,
	ADH_GC_Company,
	ADH_CustomRelated,
	ADH_SupportingReason,
	ADH_SupportingDocumentType,
	ADH_SupportingDocumentNumber,
	ADH_SystemCreateTimeUtc,
	ADH_SystemLastEditTimeUtc,
	ADH_OA_AddressOverride,
	ADH_SystemCreateUser,
	ADH_SystemLastEditUser
)
VALUES
(
	@ADH_PK,
	'AR',
	'TXC',
	'{ComplianceSequence.PK}',
	CONCAT('{ComplianceSequence.XD_Prefix}', RIGHT(REPLICATE('0', {ComplianceSequence.XD_MaximumNumberDigits} - LEN({transactionNum})) + CONVERT(VARCHAR(9), {transactionNum}), {ComplianceSequence.XD_MaximumNumberDigits})),
	GETDATE(),
	'SET',
	'VAT',
	201901,
	'{OrgHeader.PK}',
	'{GlbCompany.CurrentCompany.PK}',
	{customRelated},
	'{supportingReason}',
	'{supportingDocumentType}',
	{supportingDocumentNumber},
	GETDATE(),
	GETDATE(),
	'{OrgAddress.PK}',
	'~BP',
	'~BP'
)

INSERT INTO dbo.AccComplianceDocumentLine
(
	ADL_PK,
	ADL_ADH,
	ADL_Description,
	ADL_Sequence,
	ADL_SystemCreateTimeUtc,
	ADL_SystemCreateUser,
	ADL_SystemLastEditTimeUtc,
	ADL_SystemLastEditUser
)
VALUES
(
	@ADL_PK,
	@ADH_PK,
	'DESC',
	1,
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)

INSERT INTO dbo.AccComplianceDocumentPivot
(
	ADP_PK,
	ADP_AL,
	ADP_ADL,
	ADP_SystemCreateTimeUtc,
	ADP_SystemCreateUser,
	ADP_SystemLastEditTimeUtc,
	ADP_SystemLastEditUser
)
VALUES
(
	NEWID(),
	@AL_PK,
	@ADL_PK,
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)";
			TestConnection.ExecuteNonQuery(script);
		}

		void SetComplianceDocumentHeaderVoid(string supportingDocumentNumber)
		{
			var script = $"UPDATE dbo.AccComplianceDocumentHeader SET ADH_DocumentStatus = 'VOD', ADH_SystemLastEditTimeUtc = GETUTCDATE(), ADH_SystemLastEditUser = 'TST' WHERE ADH_SupportingDocumentNumber = '{supportingDocumentNumber}'";
			TestConnection.ExecuteNonQuery(script);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Factory = new BusinessObjectFactory();

			var accInvMsg = Factory.NewWithValidTestData<AccInvMsg>();
			accInvMsg.A9_Code = "MSG";

			FREEVAT = Factory.NewWithValidTestData<AccTaxRate>();
			FREEVAT.AT_Code = "FREEVAT";
			FREEVAT.AT_Type = "RAT";
			FREEVAT.AT_PostingGroupId = 0;
			FREEVAT.AT_A9_DefaultVatClass = accInvMsg.PK;

			CAPVAT = Factory.NewWithValidTestData<AccTaxRate>();
			CAPVAT.AT_Code = "CAPVAT";
			CAPVAT.AT_Type = "CAP";
			CAPVAT.AT_PostingGroupId = 0;
			CAPVAT.AT_A9_DefaultVatClass = accInvMsg.PK;

			VAT = Factory.NewWithValidTestData<AccTaxRate>();
			VAT.AT_Code = "VAT";
			VAT.AT_Type = "RAT";
			VAT.AT_PostingGroupId = 0;
			VAT.AT_A9_DefaultVatClass = accInvMsg.PK;

			EXEMPT = Factory.NewWithValidTestData<AccTaxRate>();
			EXEMPT.AT_Code = "EXEMPT";
			EXEMPT.AT_Type = "EXT";
			EXEMPT.AT_PostingGroupId = 0;
			EXEMPT.AT_A9_DefaultVatClass = accInvMsg.PK;

			OrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader.OH_FullName = "Revengers";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "S.H.I.E.L.D";
			var orgCusCode = OrgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = "VAT";
			orgCusCode.OK_RN_NKCodeCountry = "TW";
			orgCusCode.OK_CustomsRegNo = "12345675";
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;

			OrgAddress = Factory.NewWithValidTestData<OrgAddress>();

			ComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			ComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			ComplianceSequence.XD_SequenceClass = "TXC";
			ComplianceSequence.XD_Prefix = "BB";
			ComplianceSequence.XD_MaximumNumberDigits = 4;
			ComplianceSequence.XD_StartNumber = 1;
			ComplianceSequence.XD_EndNumber = 100;
			ComplianceSequence.XD_NextNumber = 1;

			var list = new ComplianceDocumentSupportingReasonCollection();
			list.Add(new ComplianceDocumentSupportingReason() { Code = "DEF", EnglishDescription = "test DEF" });
			list.Add(new ComplianceDocumentSupportingReason() { Code = "BBB", EnglishDescription = "test BBB" });
			list.Add(new ComplianceDocumentSupportingReason() { Code = "XXX", EnglishDescription = "test XXX" });
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentSupportingDocumentType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, list);

			Factory.Save();
		}

		AccTaxRate FREEVAT;
		AccTaxRate CAPVAT;
		AccTaxRate VAT;
		AccTaxRate EXEMPT;
		OrgHeader OrgHeader;
		OrgAddress OrgAddress;
		AccComplianceSequence ComplianceSequence;
		BusinessObjectFactory Factory;
	}
}
