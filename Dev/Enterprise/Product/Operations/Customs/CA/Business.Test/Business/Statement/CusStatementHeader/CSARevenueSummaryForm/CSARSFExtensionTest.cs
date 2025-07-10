using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CSARSFExtensionTest : TestCaseWithFactory
	{
		public void TestGetStatementLineDependsOnPaymentType()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			CombineAssertions(() =>
			{
				AssertEquals(header.DebitLine.PK, header.GetStatementLineDependsOnPaymentType(CSARSFPaymentTypes.Codes.Debit).PK);
				AssertEquals(header.CreditLine.PK, header.GetStatementLineDependsOnPaymentType(CSARSFPaymentTypes.Codes.Credit).PK);
				AssertEquals(header.InterimLine.PK, header.GetStatementLineDependsOnPaymentType(CSARSFPaymentTypes.Codes.Interim).PK);
			});
		}

		public void TestGetOrCreateNewPaymentLine()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			var line1 = header.GetOrCreateNewPaymentLine(CSARSFPaymentTypes.Codes.Debit);
			var line2 = header.GetOrCreateNewPaymentLine(CSARSFPaymentTypes.Codes.Credit);
			var line3 = header.GetOrCreateNewPaymentLine(CSARSFPaymentTypes.Codes.Interim);

			CombineAssertions(() =>
			{
				AssertEquals(CSARSFPaymentTypes.Codes.Debit, line1.B3_EntryType);
				AssertEquals(CSARSFPaymentTypes.Codes.Credit, line2.B3_EntryType);
				AssertEquals(CSARSFPaymentTypes.Codes.Interim, line3.B3_EntryType);
			});

			var debitLine = header.DebitLine;
			var creditLine = header.CreditLine;
			var interimLine = header.InterimLine;

			CombineAssertions(() =>
			{
				AssertEquals(line1.PK, debitLine.PK);
				AssertEquals(line2.PK, creditLine.PK);
				AssertEquals(line3.PK, interimLine.PK);
			});
		}

		public void TestIsCalculatedAutomatically()
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			var debitLine = statement.GetStatementLineDependsOnPaymentType(CSARSFPaymentTypes.Codes.Debit);
			var debit1 = new CSARSFPayment(debitLine, CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._490101, CSARSFDebitCodes.Descriptions._490101);
			var debit2 = new CSARSFPayment(debitLine, CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._490102, CSARSFDebitCodes.Descriptions._490102);
			var debit3 = new CSARSFPayment(debitLine, CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._49011, CSARSFDebitCodes.Descriptions._49011);
			var debit4 = new CSARSFPayment(debitLine, CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._491211, CSARSFDebitCodes.Descriptions._491211);
			var debit5 = new CSARSFPayment(debitLine, CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._491212, CSARSFDebitCodes.Descriptions._490102);
			var debit6 = new CSARSFPayment(debitLine, CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._49443, CSARSFDebitCodes.Descriptions._49443);
			var debit7 = new CSARSFPayment(debitLine, CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._49454, CSARSFDebitCodes.Descriptions._49454);
			var debit8 = new CSARSFPayment(debitLine, CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._49475, CSARSFDebitCodes.Descriptions._49475);
			var debit9 = new CSARSFPayment(debitLine, CSARSFPaymentTypes.Codes.Debit, CSARSFDebitCodes.Codes._49555, CSARSFDebitCodes.Descriptions._49555);
			var creditLine = statement.GetStatementLineDependsOnPaymentType(CSARSFPaymentTypes.Codes.Credit);
			var credit1 = new CSARSFPayment(creditLine, CSARSFPaymentTypes.Codes.Credit, CSARSFCreditCodes.Codes._49010, CSARSFCreditCodes.Descriptions._49010);
			var credit2 = new CSARSFPayment(creditLine, CSARSFPaymentTypes.Codes.Credit, CSARSFCreditCodes.Codes._49017, CSARSFCreditCodes.Descriptions._49017);
			var credit3 = new CSARSFPayment(creditLine, CSARSFPaymentTypes.Codes.Credit, CSARSFCreditCodes.Codes._49018, CSARSFCreditCodes.Descriptions._49018);
			var credit4 = new CSARSFPayment(creditLine, CSARSFPaymentTypes.Codes.Credit, CSARSFCreditCodes.Codes._49019, CSARSFCreditCodes.Descriptions._49019);
			var credit5 = new CSARSFPayment(creditLine, CSARSFPaymentTypes.Codes.Credit, CSARSFCreditCodes.Codes._49121, CSARSFCreditCodes.Descriptions._49121);
			var credit6 = new CSARSFPayment(creditLine, CSARSFPaymentTypes.Codes.Credit, CSARSFCreditCodes.Codes._49409, CSARSFCreditCodes.Descriptions._49409);
			var credit7 = new CSARSFPayment(creditLine, CSARSFPaymentTypes.Codes.Credit, CSARSFCreditCodes.Codes._49437, CSARSFCreditCodes.Descriptions._49437);
			var credit8 = new CSARSFPayment(creditLine, CSARSFPaymentTypes.Codes.Credit, CSARSFCreditCodes.Codes._49443, CSARSFCreditCodes.Descriptions._49443);
			var credit9 = new CSARSFPayment(creditLine, CSARSFPaymentTypes.Codes.Credit, CSARSFCreditCodes.Codes._49555, CSARSFCreditCodes.Descriptions._49555);
			var interimLine = statement.GetStatementLineDependsOnPaymentType(CSARSFPaymentTypes.Codes.Interim);
			var interim1 = new CSARSFPayment(interimLine, CSARSFPaymentTypes.Codes.Interim, CSARSFInterimPaymentCodes.Codes._49010, CSARSFInterimPaymentCodes.Descriptions._49010);
			var interim2 = new CSARSFPayment(interimLine, CSARSFPaymentTypes.Codes.Interim, CSARSFInterimPaymentCodes.Codes._49121, CSARSFInterimPaymentCodes.Descriptions._49121);

			CombineAssertions(() =>
			{
				AssertEquals(CSARSFDebitCodes.Descriptions._490101, true, debit1.IsCalculatedAutomatically());
				AssertEquals(CSARSFDebitCodes.Descriptions._490102, true, debit2.IsCalculatedAutomatically());
				AssertEquals(CSARSFDebitCodes.Descriptions._49011, true, debit3.IsCalculatedAutomatically());
				AssertEquals(CSARSFDebitCodes.Descriptions._491211, true, debit4.IsCalculatedAutomatically());
				AssertEquals(CSARSFDebitCodes.Descriptions._491212, true, debit5.IsCalculatedAutomatically());
				AssertEquals(CSARSFDebitCodes.Descriptions._49443, false, debit6.IsCalculatedAutomatically());
				AssertEquals(CSARSFDebitCodes.Descriptions._49454, false, debit7.IsCalculatedAutomatically());
				AssertEquals(CSARSFDebitCodes.Descriptions._49475, true, debit8.IsCalculatedAutomatically());
				AssertEquals(CSARSFDebitCodes.Descriptions._49555, false, debit9.IsCalculatedAutomatically());

				AssertEquals(CSARSFCreditCodes.Descriptions._49010, false, credit1.IsCalculatedAutomatically());
				AssertEquals(CSARSFCreditCodes.Descriptions._49017, true, credit2.IsCalculatedAutomatically());
				AssertEquals(CSARSFCreditCodes.Descriptions._49018, true, credit3.IsCalculatedAutomatically());
				AssertEquals(CSARSFCreditCodes.Descriptions._49019, false, credit4.IsCalculatedAutomatically());
				AssertEquals(CSARSFCreditCodes.Descriptions._49121, false, credit5.IsCalculatedAutomatically());
				AssertEquals(CSARSFCreditCodes.Descriptions._49409, false, credit6.IsCalculatedAutomatically());
				AssertEquals(CSARSFCreditCodes.Descriptions._49437, false, credit7.IsCalculatedAutomatically());
				AssertEquals(CSARSFCreditCodes.Descriptions._49443, false, credit8.IsCalculatedAutomatically());
				AssertEquals(CSARSFCreditCodes.Descriptions._49555, false, credit9.IsCalculatedAutomatically());

				AssertEquals(CSARSFInterimPaymentCodes.Descriptions._49010, false, interim1.IsCalculatedAutomatically());
				AssertEquals(CSARSFInterimPaymentCodes.Descriptions._49121, false, interim2.IsCalculatedAutomatically());
			});
		}

		public void TestGetPaymentCodeList()
		{
			var list = CSARSFExtension.GetPaymentCodeList(Factory, CSARSFPaymentTypes.Codes.Debit);
			Assert(list is CSARSFDebitCodes);

			list = CSARSFExtension.GetPaymentCodeList(Factory, CSARSFPaymentTypes.Codes.Credit);
			Assert(list is CSARSFCreditCodes);

			list = CSARSFExtension.GetPaymentCodeList(Factory, CSARSFPaymentTypes.Codes.Interim);
			Assert(list is CSARSFInterimPaymentCodes);

			list = CSARSFExtension.GetPaymentCodeList(Factory, ZString.Empty);
			Assert(list is CodeDescriptionPairList);
		}

		public void TestGetCSARSFDeclarationQuery()
		{
			var org = Factory.New<OrgHeader>();
			var rsf = Factory.New<CusStatementHeader>();
			rsf.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			rsf.B2_PeriodStartDate = new ZDate(2020, 10, 01);
			rsf.B2_PeriodEndDate = new ZDate(2020, 10, 31);
			rsf.B2_OH_Importer = org.PK;
			var query = rsf.GetCSARSFDeclarationQuery();
			AssertMultilineASCIIEquals(string.Format(@"JE_GB IN 
(
	SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{0}'
)
AND
JE_OH_Importer = '{1}' 
AND
(
	(
		JE_MessageType = 'B3X' 
		AND
		(
			JE_PK IN 
			(
				SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'CA_B2AcceptedDate' 
				AND
				XA_Data >= '2020-10-01 00:00:00.000' 
				AND
				XA_Data <= '2020-10-31 00:00:00.000'
			)
		)
	)
	OR
	(
		JE_MessageType = 'IMP' 
		AND
		JE_EntryAuthorisationDate <= '2020-10-31 23:59:00.000' 
		AND
		(
			JE_PK IN 
			(
				SELECT CH_JE FROM dbo.CusEntryHeader WHERE 
				(
					CH_MessageType in 
					(
						'B3C', 'CAD'
					)
				)
				AND
				CH_EntryReleaseDate >= '2020-10-19 00:00:00.000' 
				AND
				CH_EntryReleaseDate <= '2020-11-18 23:59:00.000'
			)
		)
	)
	OR
	(
		JE_MessageType = 'IMP' 
		AND
		JE_EntryAuthorisationDate <= '2020-10-18 23:59:00.000' 
		AND
		(
			JE_PK IN 
			(
				SELECT CH_JE FROM dbo.CusEntryHeader WHERE 
				(
					CH_MessageType in 
					(
						'B3C', 'CAD'
					)
				)
				AND
				CH_EntryReleaseDate >= '2020-10-01 00:00:00.000' 
				AND
				CH_EntryReleaseDate <= '2020-10-31 23:59:00.000'
			)
		)
	)
)", rsf.Company.PK, org.PK), query.LiteralTextSqlFormatted);
		}

		public void TestGetCSARSFDeclarationQuery_Option1()
		{
			var org = Factory.New<OrgHeader>();
			var orgImp = OrgImpAddInfo.Get(org);
			orgImp.ZO_AccountingTimeOption = CSARSFAccountingOptionList.Codes.Option1;
			var rsf = Factory.New<CusStatementHeader>();
			rsf.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			rsf.B2_PeriodStartDate = new ZDate(2020, 10, 01);
			rsf.B2_PeriodEndDate = new ZDate(2020, 10, 31);
			rsf.B2_OH_Importer = org.PK;
			var query = rsf.GetCSARSFDeclarationQuery();
			AssertMultilineASCIIEquals(string.Format(@"JE_GB IN 
(
	SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{0}'
)
AND
JE_OH_Importer = '{1}' 
AND
(
	(
		JE_MessageType = 'B3X' 
		AND
		(
			JE_PK IN 
			(
				SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'CA_B2AcceptedDate' 
				AND
				XA_Data >= '2020-10-01 00:00:00.000' 
				AND
				XA_Data <= '2020-10-31 00:00:00.000'
			)
		)
	)
	OR
	(
		JE_MessageType = 'IMP' 
		AND
		JE_EntryAuthorisationDate <= '2020-10-31 23:59:00.000' 
		AND
		(
			JE_PK IN 
			(
				SELECT CH_JE FROM dbo.CusEntryHeader WHERE 
				(
					CH_MessageType in 
					(
						'B3C', 'CAD'
					)
				)
				AND
				CH_EntryReleaseDate >= '2020-10-19 00:00:00.000' 
				AND
				CH_EntryReleaseDate <= '2020-11-18 23:59:00.000'
			)
		)
	)
)", rsf.Company.PK, org.PK), query.LiteralTextSqlFormatted);
		}

		public void TestGetCSARSFDeclarationQuery_Option2()
		{
			var org = Factory.New<OrgHeader>();
			var orgImp = OrgImpAddInfo.Get(org);
			orgImp.ZO_AccountingTimeOption = CSARSFAccountingOptionList.Codes.Option2;
			var rsf = Factory.New<CusStatementHeader>();
			rsf.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			rsf.B2_PeriodStartDate = new ZDate(2020, 10, 01);
			rsf.B2_PeriodEndDate = new ZDate(2020, 10, 31);
			rsf.B2_OH_Importer = org.PK;
			var query = rsf.GetCSARSFDeclarationQuery();
			AssertMultilineASCIIEquals(string.Format(@"JE_GB IN 
(
	SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = '{0}'
)
AND
JE_OH_Importer = '{1}' 
AND
(
	(
		JE_MessageType = 'B3X' 
		AND
		(
			JE_PK IN 
			(
				SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_Name = 'CA_B2AcceptedDate' 
				AND
				XA_Data >= '2020-10-01 00:00:00.000' 
				AND
				XA_Data <= '2020-10-31 00:00:00.000'
			)
		)
	)
	OR
	(
		JE_MessageType = 'IMP' 
		AND
		JE_EntryAuthorisationDate <= '2020-10-18 23:59:00.000' 
		AND
		(
			JE_PK IN 
			(
				SELECT CH_JE FROM dbo.CusEntryHeader WHERE 
				(
					CH_MessageType in 
					(
						'B3C', 'CAD'
					)
				)
				AND
				CH_EntryReleaseDate >= '2020-10-01 00:00:00.000' 
				AND
				CH_EntryReleaseDate <= '2020-10-31 23:59:00.000'
			)
		)
	)
)", rsf.Company.PK, org.PK), query.LiteralTextSqlFormatted);
		}
	}
}
