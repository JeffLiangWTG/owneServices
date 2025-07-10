using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	class TaiwanTriplicateGUIDetailedReportIntegrationTest : TransactionedTestCase
	{
		public void TestFormatWhenRecordsNotExist()
		{
			InsertHeaders(11, 20);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanTriplicateGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}', '', 'AA', 'TXC'");
			AssertEquals("The report should return nothing.", 0, result.Rows.Count);
		}

		public void TestFormatWhenRecordsLessThan25()
		{
			InsertHeaders(11, 20);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanTriplicateGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}', '', 'TXC', 'AA'");
			AssertEquals("The report should return 25 rows of data, last 5 rows should be blank.", 25, result.Rows.Count);
		}

		public void TestFormatWhenRecordsEqualTo25()
		{
			InsertHeaders(26, 25);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanTriplicateGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}', '', 'TXC', 'AA'");
			AssertEquals("The report should return 25 rows of data.", 25, result.Rows.Count);
		}

		public void TestFormatWhenRecordsEqualTo50()
		{
			InsertHeaders(51, 50);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanTriplicateGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}', '', 'TXC', 'AA'");
			AssertEquals("The report should return 25 rows of data.", 25, result.Rows.Count);
		}

		public void TestFormatWhenRecordsMoreThan50()
		{
			InsertHeaders(61, 70);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanTriplicateGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}', '', 'TXC', 'AA'");
			AssertEquals("The report should return 25 rows of data.", 50, result.Rows.Count);

			// GroupID
			AssertEquals("0", result.Rows[24]["GroupID"].ToString());
			AssertEquals("1", result.Rows[25]["GroupID"].ToString());

			// HasData
			AssertNotNullOrEmpty(result.Rows[34]["DocumentNumberL"].ToString());
			AssertNotNullOrEmpty(result.Rows[44]["DocumentNumberL"].ToString());
			AssertNullOrEmpty(result.Rows[45]["DocumentNumberL"].ToString());

			// Empty Remarks
			AssertEquals(string.Empty, result.Rows[34]["RemarksL"].ToString());
			AssertEquals(string.Empty, result.Rows[34]["RemarksR"].ToString());

			AssertEquals("Empty", result.Rows[35]["RemarksL"].ToString());
			AssertEquals(string.Empty, result.Rows[35]["RemarksR"].ToString());

			// Void Row
			AssertNotNullOrEmpty(result.Rows[4]["DocumentNumberL"].ToString());
			AssertEquals(string.Empty, result.Rows[4]["AmountL"].ToString());
			AssertEquals(string.Empty, result.Rows[4]["TaxAmountL"].ToString());
			AssertEquals(string.Empty, result.Rows[4]["RatedL"].ToString());
			AssertEquals(string.Empty, result.Rows[4]["ZeroRatedL"].ToString());
			AssertEquals(string.Empty, result.Rows[4]["ExemptedL"].ToString());
			AssertEquals("Void", result.Rows[4]["RemarksL"].ToString());

			// Last2Nums
			AssertEquals("01", result.Rows[0]["Last2NumsL"].ToString());
			AssertEquals("26", result.Rows[0]["Last2NumsR"].ToString());

			// Amount
			AssertEquals("100", result.Rows[0]["AmountL"].ToString());
			AssertEquals("100", result.Rows[0]["AmountR"].ToString());

			// Customs Registration Number
			AssertEquals("", result.Rows[0]["CustomsRegNoL"].ToString());
			AssertEquals("", result.Rows[0]["CustomsRegNoR"].ToString());
		}

		public void TestFormatWhenHasMultipleSequencesWithoutPageBreak()
		{
			InsertHeaders(ComplianceSequenceX);
			InsertHeaders(ComplianceSequence);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanTriplicateGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}', '', 'TXC', 'BB'");
			AssertEquals(50, result.Rows.Count);
			AssertNotNullOrEmpty("Rows from 0 to 40 must fill with data.", result.Rows[40]["DocumentNumberL"].ToString());
			AssertNullOrEmpty("Rows from 41 to 49 should be blank.", result.Rows[41]["DocumentNumberL"].ToString());
		}

		public void TestFormatWhenHasMultipleSequencesWithPageBreak()
		{
			InsertHeaders(ComplianceSequence);
			InsertHeaders(ComplianceSequenceX);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanTriplicateGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}', 'Y', 'TXC', 'BB'");
			AssertEquals(75, result.Rows.Count);
			AssertNotNullOrEmpty("Rows from 0 to 10 must fill with data.", result.Rows[10]["DocumentNumberL"].ToString());
			AssertNullOrEmpty("Rows from 12 to 24 should be blank.", result.Rows[12]["DocumentNumberL"].ToString());
			AssertNotNullOrEmpty("Rows from 25 to 54 must fill with data.", result.Rows[25]["DocumentNumberL"].ToString());
			AssertNotNullOrEmpty("Rows from 25 to 54 must fill with data.", result.Rows[54]["DocumentNumberL"].ToString());
			AssertNullOrEmpty("Rows from 55 to 74 should be blank.", result.Rows[55]["DocumentNumberL"].ToString());
		}

		public void TestFormatWhenHasMultipleVoidedRecordsWithoutDebtor()
		{
			InsertHeaders();
			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanTriplicateGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}', 'Y', 'TXC', 'AA'");
			AssertEquals(75, result.Rows.Count);
			AssertNotNullOrEmpty("Rows from 0 to 19 must fill with data.", result.Rows[19]["DocumentNumberL"].ToString());
			AssertNotNullOrEmpty("Row 4 should be voided", result.Rows[4]["DocumentNumberL"].ToString());
			AssertEquals("Row 4 should be voided.", "Void", result.Rows[4]["RemarksL"].ToString());
			AssertNotNullOrEmpty("Rows from 20 to 50 should be voided.", result.Rows[20]["DocumentNumberL"].ToString());
			AssertNotNullOrEmpty("Rows from 20 to 50 should be voided.", result.Rows[50]["DocumentNumberL"].ToString());
			AssertEquals("Rows from 20 to 50 should be voided.", "Void", result.Rows[20]["RemarksL"].ToString());
			AssertEquals("Rows from 20 to 50 should be voided.", "Void", result.Rows[50]["RemarksL"].ToString());
			AssertNotNullOrEmpty("Row 51 should be empty.", result.Rows[51]["DocumentNumberL"].ToString());
			AssertEquals("Row 51 should be empty.", "Empty", result.Rows[51]["RemarksL"].ToString());
			AssertNullOrEmpty("Rows from 52 to 74 should be blank.", result.Rows[52]["DocumentNumberL"].ToString());
			AssertNullOrEmpty("Rows from 52 to 74 should be blank.", result.Rows[74]["DocumentNumberL"].ToString());
			AssertNullOrEmpty("Rows from 52 to 74 should be blank.", result.Rows[52]["RemarksL"].ToString());
			AssertNullOrEmpty("Rows from 52 to 74 should be blank.", result.Rows[74]["RemarksL"].ToString());
			AssertNullOrEmpty("Rows from 52 to 74 should be blank.", result.Rows[52]["DocumentNumberL"].ToString());
			AssertNullOrEmpty("Rows from 52 to 74 should be blank.", result.Rows[74]["DocumentNumberL"].ToString());
		}

		public void TestTaxShouldTickedBasedOnTaxCode()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_SequenceClass = "TXC";
			sequence.XD_Prefix = "AA";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 10;
			sequence.XD_MaximumNumberDigits = 4;
			sequence.XD_NextNumber = 1;
			Factory.Save();

			InsertHeader(001, VAT, sequence, 10, OrgHeader);
			InsertHeader(002, VAT, sequence, 0, OrgHeader);
			InsertHeader(003, CAPVAT, sequence, 10, OrgHeader);
			InsertHeader(004, CAPVAT, sequence, 0, OrgHeader);
			InsertHeader(005, FREEVAT, sequence, 0, OrgHeader);
			InsertHeader(006, EXEMPT, sequence, 0, OrgHeader);
			InsertHeader(007, NOTREPORT, sequence, 0, OrgHeader);
			InsertHeader(008, EXCLUDE, sequence, 0, OrgHeader);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanTriplicateGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}', 'Y', 'TXC', 'AA'");

			//VAT
			AssertEquals("V", result.Rows[0]["RatedL"].ToString());
			AssertEquals("", result.Rows[0]["ZeroRatedL"].ToString());
			AssertEquals("", result.Rows[0]["ExemptedL"].ToString());
			AssertEquals("10", result.Rows[0]["TaxAmountL"].ToString());

			AssertEquals("V", result.Rows[1]["RatedL"].ToString());
			AssertEquals("", result.Rows[1]["ZeroRatedL"].ToString());
			AssertEquals("", result.Rows[1]["ExemptedL"].ToString());
			AssertEquals("0", result.Rows[1]["TaxAmountL"].ToString());

			//CAPVAT
			AssertEquals("V", result.Rows[2]["RatedL"].ToString());
			AssertEquals("", result.Rows[2]["ZeroRatedL"].ToString());
			AssertEquals("", result.Rows[2]["ExemptedL"].ToString());
			AssertEquals("10", result.Rows[2]["TaxAmountL"].ToString());

			AssertEquals("V", result.Rows[3]["RatedL"].ToString());
			AssertEquals("", result.Rows[3]["ZeroRatedL"].ToString());
			AssertEquals("", result.Rows[3]["ExemptedL"].ToString());
			AssertEquals("0", result.Rows[3]["TaxAmountL"].ToString());

			//FREEVAT
			AssertEquals("", result.Rows[4]["RatedL"].ToString());
			AssertEquals("V", result.Rows[4]["ZeroRatedL"].ToString());
			AssertEquals("", result.Rows[4]["ExemptedL"].ToString());
			AssertEquals("0", result.Rows[4]["TaxAmountL"].ToString());

			//EXEMPT
			AssertEquals("", result.Rows[5]["RatedL"].ToString());
			AssertEquals("", result.Rows[5]["ZeroRatedL"].ToString());
			AssertEquals("V", result.Rows[5]["ExemptedL"].ToString());
			AssertEquals("0", result.Rows[5]["TaxAmountL"].ToString());

			//EXCLUDE
			AssertEquals("", result.Rows[6]["RatedL"].ToString());
			AssertEquals("", result.Rows[6]["ZeroRatedL"].ToString());
			AssertEquals("", result.Rows[6]["ExemptedL"].ToString());
			AssertEquals("0", result.Rows[6]["TaxAmountL"].ToString());

			//NOTREPORT
			AssertEquals("", result.Rows[7]["RatedL"].ToString());
			AssertEquals("", result.Rows[7]["ZeroRatedL"].ToString());
			AssertEquals("", result.Rows[7]["ExemptedL"].ToString());
			AssertEquals("0", result.Rows[7]["TaxAmountL"].ToString());
		}

		public void TestRegistrationNumber()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_SequenceClass = "TXC";
			sequence.XD_Prefix = "AA";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 10;
			sequence.XD_MaximumNumberDigits = 4;
			sequence.XD_NextNumber = 1;
			Factory.Save();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.CustomsCodes.AddNew();
			var orgCusCode = orgHeader2.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = "VAT";
			orgCusCode.OK_RN_NKCodeCountry = "TW";
			orgCusCode.OK_CustomsRegNo = "12345675";

			var orgCusCode1 = orgHeader2.CustomsCodes.AddNew();
			orgCusCode1.OK_CodeType = "VAT";
			orgCusCode1.OK_RN_NKCodeCountry = "CN";
			orgCusCode1.OK_CustomsRegNo = "57654321";
			Factory.Save();

			InsertHeader(001, VAT, sequence, 10, orgHeader1);
			InsertHeader(002, VAT, sequence, 0, orgHeader2);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC TaiwanTriplicateGUIDetailedReport 201901, 201901, '{GlbCompany.CurrentCompany.PK}', 'Y', 'TXC', 'AA'");

			AssertEquals("", result.Rows[0]["CustomsRegNoL"].ToString());
			AssertEquals("12345675", result.Rows[1]["CustomsRegNoL"].ToString());
		}

		void InsertHeaders()
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_SequenceClass = "TXC";
			sequence.XD_Prefix = "AA";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = 102;
			sequence.XD_MaximumNumberDigits = 8;
			sequence.XD_NextNumber = 1;
			Factory.Save();

			for (int i = 1; i < 21; i++)
			{
				InsertHeader(i, VAT, sequence, 5, OrgHeader);
			}

			VoidHeader("AA00000005");

			for (int i = 21; i < 102; i++)
			{
				InsertVoidedHeader(i, sequence);
			}

			sequence.XD_NextNumber = 102;
			Factory.Save();
		}

		void InsertHeaders(int nextNumber, int maxNumber)
		{
			var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_SequenceClass = "TXC";
			sequence.XD_Prefix = "AA";
			sequence.XD_StartNumber = 1;
			sequence.XD_EndNumber = maxNumber;
			sequence.XD_MaximumNumberDigits = 4;
			sequence.XD_NextNumber = nextNumber;
			Factory.Save();

			for (int i = 1; i < nextNumber - 6; i++)
			{
				InsertHeader(i, VAT, sequence, 5, OrgHeader);
			}

			InsertHeader(nextNumber - 6, CAPVAT, sequence, 10, OrgHeader);
			InsertHeader(nextNumber - 5, VAT, sequence, 0, OrgHeader);
			InsertHeader(nextNumber - 4, VAT, sequence, 0, OrgHeader);
			InsertHeader(nextNumber - 3, EXEMPT, sequence, 0, OrgHeader);
			InsertHeader(nextNumber - 2, EXEMPT, sequence, 0, OrgHeader);
			InsertHeader(nextNumber - 1, NOTREPORT, sequence, 0, OrgHeader);

			VoidHeader("AA0005");
		}

		void InsertHeaders(AccComplianceSequence sequence)
		{
			for (int i = (int)sequence.XD_StartNumber; i <= sequence.XD_EndNumber; i++)
			{
				InsertHeader((int)sequence.XD_StartNumber, VAT, sequence, 0, OrgHeader);
				sequence.XD_StartNumber++;
			}
		}

		void InsertHeader(int transactionNum, AccTaxRate taxRate, AccComplianceSequence sequence, decimal taxAmount, OrgHeader orgHeader)
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
	'{orgHeader.PK}',
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
	ADH_SystemCreateTimeUtc,
	ADH_SystemLastEditTimeUtc,
	ADH_SystemCreateUser,
	ADH_SystemLastEditUser
)
VALUES
(
	@ADH_PK,
	'AR',
	'TXC',
	'{sequence.PK}',
	CONCAT('{sequence.XD_Prefix}', RIGHT(REPLICATE('0', {sequence.XD_MaximumNumberDigits} - LEN({transactionNum})) + CONVERT(VARCHAR(9), {transactionNum}), {sequence.XD_MaximumNumberDigits})),
	GETDATE(),
	'SET',
	'VAT',
	201901,
	'{orgHeader.PK}',
	'{GlbCompany.CurrentCompany.PK}',
	GETDATE(),
	GETDATE(),
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

		void InsertVoidedHeader(int transactionNum, AccComplianceSequence sequence)
		{
			var script = $@"
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
	ADH_GC_Company,
	ADH_Description,
	ADH_SystemCreateTimeUtc,
	ADH_SystemLastEditTimeUtc,
	ADH_SystemCreateUser,
	ADH_SystemLastEditUser
)
VALUES
(
	NEWID(),
	'AR',
	'TXC',
	'{sequence.PK}',
	CONCAT('{sequence.XD_Prefix}', RIGHT(REPLICATE('0', {sequence.XD_MaximumNumberDigits} -LEN({transactionNum})) +CONVERT(VARCHAR(9), {transactionNum}), {sequence.XD_MaximumNumberDigits})),
	GETDATE(),
	'VOD',
	'VAT',
	201901,
	'{GlbCompany.CurrentCompany.PK}',
	'Sequence number voided via compliance invoice book',
	GETDATE(),
	GETDATE(),
	'~BP',
	'~BP'
)";
			TestConnection.ExecuteNonQuery(script);
		}

		void VoidHeader(string documentNumber)
		{
			var script = $@"UPDATE dbo.AccComplianceDocumentHeader SET ADH_DocumentStatus = 'VOD', ADH_SystemLastEditTimeUtc = GETUTCDATE(), ADH_SystemLastEditUser = 'TST' WHERE ADH_DocumentNumber = '{documentNumber}'";
			TestConnection.ExecuteNonQuery(script);
		}

		#region Setup

		BusinessObjectFactory fFactory;
		public BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}

		AccTaxRate VAT;
		AccTaxRate CAPVAT;
		AccTaxRate EXEMPT;
		AccTaxRate NOTREPORT;
		AccTaxRate FREEVAT;
		AccTaxRate EXCLUDE;
		OrgHeader OrgHeader;
		AccComplianceSequence ComplianceSequence;
		AccComplianceSequence ComplianceSequenceX;

		protected override void SetUp()
		{
			base.SetUp();
			PrepareData();
		}

		void PrepareData()
		{
			var accInvMsg = Factory.NewWithValidTestData<AccInvMsg>();
			accInvMsg.A9_Code = "MSG";

			VAT = Factory.NewWithValidTestData<AccTaxRate>();
			VAT.AT_Code = "VAT";
			VAT.AT_Type = "RAT";
			VAT.AT_PostingGroupId = 0;
			VAT.AT_A9_DefaultVatClass = accInvMsg.PK;

			CAPVAT = Factory.NewWithValidTestData<AccTaxRate>();
			CAPVAT.AT_Code = "CAPVAT";
			CAPVAT.AT_Type = "CAP";
			CAPVAT.AT_PostingGroupId = 0;
			CAPVAT.AT_A9_DefaultVatClass = accInvMsg.PK;

			EXEMPT = Factory.NewWithValidTestData<AccTaxRate>();
			EXEMPT.AT_Code = "EXEMPT";
			EXEMPT.AT_Type = "EXT";
			EXEMPT.AT_PostingGroupId = 1;
			EXEMPT.AT_A9_DefaultVatClass = accInvMsg.PK;

			NOTREPORT = Factory.NewWithValidTestData<AccTaxRate>();
			NOTREPORT.AT_Code = "NOTREPORT";
			NOTREPORT.AT_Type = "NOT";
			NOTREPORT.AT_PostingGroupId = 3;
			NOTREPORT.AT_A9_DefaultVatClass = accInvMsg.PK;

			FREEVAT = Factory.NewWithValidTestData<AccTaxRate>();
			FREEVAT.AT_Code = "FREEVAT";
			FREEVAT.AT_Type = "RAT";
			FREEVAT.AT_PostingGroupId = 0;
			FREEVAT.AT_A9_DefaultVatClass = accInvMsg.PK;

			EXCLUDE = Factory.NewWithValidTestData<AccTaxRate>();
			EXCLUDE.AT_Code = "EXCLUDE";
			EXCLUDE.AT_Type = "EXL";
			EXCLUDE.AT_PostingGroupId = 0;
			EXCLUDE.AT_A9_DefaultVatClass = accInvMsg.PK;

			OrgHeader = Factory.NewWithValidTestData<OrgHeader>();

			ComplianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			ComplianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			ComplianceSequence.XD_SequenceClass = "TXC";
			ComplianceSequence.XD_Prefix = "BB";
			ComplianceSequence.XD_MaximumNumberDigits = 4;
			ComplianceSequence.XD_StartNumber = 1000;
			ComplianceSequence.XD_EndNumber = 1010;
			ComplianceSequence.XD_NextNumber = 1011;

			ComplianceSequenceX = Factory.NewWithValidTestData<AccComplianceSequence>();
			ComplianceSequenceX.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			ComplianceSequenceX.XD_SequenceClass = "TXC";
			ComplianceSequenceX.XD_Prefix = "BB";
			ComplianceSequenceX.XD_MaximumNumberDigits = 4;
			ComplianceSequenceX.XD_StartNumber = 1011;
			ComplianceSequenceX.XD_EndNumber = 1065;
			ComplianceSequenceX.XD_NextNumber = 1066;

			Factory.Save();
		}

		#endregion
	}
}
