using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(GLAccountingJournal))]
	public class GLAccountingJournalTest : AccountingJournalTest
	{
		public void TestGLStandardAccountingJournalWithHighPrecisionExchangeRate()
		{
			AssertAccountingJournalWithHighPrecisionExchangeRate(TransactionTypes.GLStandardJournal, 41.38m, -267.5m, ZDecimal.Zero, ZDecimal.Zero);
		}

		public void TestGLAutoAccountingJournalWithHighPrecisionExchangeRate()
		{
			AssertAccountingJournalWithHighPrecisionExchangeRate(TransactionTypes.GLAutoJournal, 41.38m, 41.38m, -267.5m, -267.5m);
		}

		public void TestGLReversingAccountingJournalWithHighPrecisionExchangeRate()
		{
			AssertAccountingJournalWithHighPrecisionExchangeRate(TransactionTypes.GLReversingJournal, 41.38m, -41.38m, -267.5m, 267.5m);
		}

		void AssertAccountingJournalWithHighPrecisionExchangeRate(ZString journalType, ZDecimal expectedLine1OSExTaxAmount, ZDecimal expectedLine2OSExTaxAmount, ZDecimal expectedLine3OSExTaxAmount, ZDecimal expectedLine4OSExTaxAmount)
		{
			var hongKongCompany = Creator.CreateNewCompany("HKG", Core.Constants.CountryCodes.HongKong);
			hongKongCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.HongKong;
			hongKongCompany.GC_IsReciprocal = true;
			var kowloonBranch = Creator.CreateBranch("KWL", hongKongCompany);
			Factory.Save();

			var exchangeRate = 0.1547m;
			var highPrecisionExchangeRate = 0.154691589m;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, kowloonBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Creator.CreateTestPeriodsForEntireYear(2013);

				var glJournal = Creator.CreateGLJournal(journalType, new ZDateTime(2013, 08, 13), new ZDateTime(2013, 08, 13), new ZDateTime(2013, 09, 13));
				Creator.CreateGLJournalLine(glJournal, 41.38m, DebitCredit.DR, Creator.GLHeader1.PK);
				var lineInUSD = Creator.CreateGLJournalLine(glJournal, 267.5m, DebitCredit.CR, Creator.GLHeader2.PK);
				lineInUSD.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				lineInUSD.AL_ExchangeRate = exchangeRate;
				AssertEquals(41.38m, lineInUSD.UnsignedLocalLineAmount);
				Factory.Save();

				AssertEquals("lineInUSD has High Precision Exchange Rate", highPrecisionExchangeRate, lineInUSD.AL_ExchangeRate);
				AssertAccountingJournalLineOSExTaxAmount(glJournal, expectedLine1OSExTaxAmount, expectedLine2OSExTaxAmount, expectedLine3OSExTaxAmount, expectedLine4OSExTaxAmount);

				lineInUSD.AL_ExchangeRate = exchangeRate;
				Factory.Save();

				AssertEquals("High Precision Exchange Rate is not recalculated for lineInUSD, because it is in DB.", exchangeRate, lineInUSD.AL_ExchangeRate);
				AssertAccountingJournalLineOSExTaxAmount(glJournal, expectedLine1OSExTaxAmount, expectedLine2OSExTaxAmount, expectedLine3OSExTaxAmount, expectedLine4OSExTaxAmount);
			}
		}

		void AssertAccountingJournalLineOSExTaxAmount(GLJournal glJournal, ZDecimal expectedLine1OSExTaxAmount, ZDecimal expectedLine2OSExTaxAmount, ZDecimal expectedLine3OSExTaxAmount, ZDecimal expectedLine4OSExTaxAmount)
		{
			var accountingJournal = new GLAccountingJournal(glJournal, ReadonlyFactory);
			var lines = accountingJournal.Lines.OfType<AccountingJournalLine>().ToArray();
			var expectedAccountingJournalLines = glJournal.AH_TransactionType == TransactionTypes.GLStandardJournal ? 2 : 4;

			AssertEquals(expectedAccountingJournalLines, lines.Length);
			AssertEquals(expectedLine1OSExTaxAmount, lines[0].AL_OSExTaxAmount);
			AssertEquals(expectedLine2OSExTaxAmount, lines[1].AL_OSExTaxAmount);
			if (glJournal.AH_TransactionType != TransactionTypes.GLStandardJournal)
			{
				AssertEquals(expectedLine3OSExTaxAmount, lines[2].AL_OSExTaxAmount);
				AssertEquals(expectedLine4OSExTaxAmount, lines[3].AL_OSExTaxAmount);
			}
		}

		public override void TestAJOptionalFields()
		{
			Creator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
			Factory.Save();

			var autoJournal = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
			Factory.Save();

			var aj = new GLAccountingJournal(autoJournal, ReadonlyFactory);
			AssertEquals("Optional Field Count", 3, aj.ApplicableOptionalFields.Count);
			AssertEquals("POST_TO_PERIOD", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.PostToPeriodText));
			AssertEquals("POST_TO_PERIOD Value", autoJournal.PostPeriod.ToString(), aj.ApplicableOptionalFields[AccountingJournal.PostToPeriodText]);
			AssertEquals("END_PERIOD", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.EndPeriodText));
			AssertEquals("END_PERIOD Value", autoJournal.AgePeriod.ToString(), aj.ApplicableOptionalFields[AccountingJournal.EndPeriodText]);
			AssertEquals("PRESENTATION", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.PresentationText));
			AssertEquals("PRESENTATION Value", string.Empty, aj.ApplicableOptionalFields[AccountingJournal.PresentationText]);

			var revJournal = Creator.CreateGLJournal(TransactionTypes.GLReversingJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
			Factory.Save();

			aj = new GLAccountingJournal(revJournal, ReadonlyFactory);
			AssertEquals("Optional Field Count", 3, aj.ApplicableOptionalFields.Count);
			AssertEquals("POST_TO_PERIOD", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.PostToPeriodText));
			AssertEquals("POST_TO_PERIOD Value", autoJournal.PostPeriod.ToString(), aj.ApplicableOptionalFields[AccountingJournal.PostToPeriodText]);
			AssertEquals("REVERSE_PERIOD", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.ReversePeriodText));
			AssertEquals("REVERSE_PERIOD Value", autoJournal.AgePeriod.ToString(), aj.ApplicableOptionalFields[AccountingJournal.ReversePeriodText]);
			AssertEquals("PRESENTATION", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.PresentationText));
			AssertEquals("PRESENTATION Value", string.Empty, aj.ApplicableOptionalFields[AccountingJournal.PresentationText]);
		}

		public void TestAL_PostPeriodForGL()
		{
			Creator.CreateTestPeriodsForEntireYear(2013);

			var autoJournal = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, new ZDateTime(2013, 08, 13), new ZDateTime(2013, 08, 13), new ZDateTime(2013, 09, 13));
			Creator.CreateGLJournalLine(autoJournal, 250m, DebitCredit.DR, Creator.GLHeader1.PK);
			Creator.CreateGLJournalLine(autoJournal, 250m, DebitCredit.CR, Creator.GLHeader2.PK);

			var revJournal = Creator.CreateGLJournal(TransactionTypes.GLReversingJournal, new ZDateTime(2013, 06, 13), new ZDateTime(2013, 06, 13), new ZDateTime(2013, 07, 13));
			Creator.CreateGLJournalLine(revJournal, 350m, DebitCredit.DR, Creator.GLHeader1.PK);
			Creator.CreateGLJournalLine(revJournal, 350m, DebitCredit.CR, Creator.GLHeader2.PK);

			var stdJournal = Creator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2013, 06, 13), new ZDateTime(2013, 06, 13), new ZDateTime(2013, 07, 13));
			Creator.CreateGLJournalLine(stdJournal, 350m, DebitCredit.DR, Creator.GLHeader1.PK);
			Creator.CreateGLJournalLine(stdJournal, 350m, DebitCredit.CR, Creator.GLHeader2.PK);

			Factory.Save();

			var aj1 = new GLAccountingJournal(stdJournal, ReadonlyFactory);
			AssertEquals("Line Count", 2, aj1.Lines.Count());
			AssertEquals("PostPeriod", aj1.Lines.First().AL_PostPeriod, 201306);
			AssertEquals("PostPeriod", aj1.Lines.Skip(1).First().AL_PostPeriod, 201306);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var gLJournal = Factory.NewWithValidTestData<GLJournal>();
			return new GLAccountingJournal(gLJournal, ReadonlyFactory);
		}

		protected override AccountingJournal CreateTestAccountingJournal()
		{
			var journal = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
			transaction = journal;
			return new GLAccountingJournal(journal, ReadonlyFactory);
		}

		public override void TestAccountingJournalLines()
		{
			var periodCal = new AccountingPeriodCalculator(Factory);
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupSinglePeriod(ZDateTime.Today.Year * 100 + ZDateTime.Today.Month, ZDateTime.Today, ZDateTime.Today.AddDays(30));
			Factory.Save();

			var journal = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
			Creator.CreateGLJournalLine(journal, 250, DebitCredit.DR, Creator.GLHeader1.PK);
			Creator.CreateGLJournalLine(journal, 250, DebitCredit.CR, Creator.GLHeader2.PK);
			Factory.Save();

			var ajFactory = new BusinessObjectFactory();
			var ajournal = new GLAccountingJournal(journal, ReadonlyFactory);
			var ajLines = ajournal.Lines.Cast<AccountingJournalLine>().ToArray();

			AssertAJLine(ajLines[0], journal.Lines[0].AL_AC, journal.Lines[0].AL_AG, "GL AUTO JOURNAL", ZString.Empty, journal.Lines[0].AL_PostDate, periodCal.GetPeriodFromDate(ZDateTime.Today), "IMM", journal.Lines[0].AL_LineAmount, ZString.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			AssertAJLine(ajLines[1], journal.Lines[1].AL_AC, journal.Lines[1].AL_AG, "GL AUTO JOURNAL", ZString.Empty, journal.Lines[1].AL_PostDate, periodCal.GetPeriodFromDate(ZDateTime.Today), "IMM", journal.Lines[1].AL_LineAmount, ZString.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
		}

		public void TestALDescForReportingBook()
		{
			var factory = new ReadOnlyBusinessObjectFactory();
			var reportingBook = factory.New<AccReportingBook>();
			var creator = new TestObjectCreator(factory);
			var dataTable = new System.Data.DataTable("GeneralLedgerTransactionData");
			dataTable.Columns.Add("TransactionHeaderID", typeof(Guid));
			dataTable.Columns.Add("TransactionLineID", typeof(Guid));
			dataTable.Columns.Add("TaxGLMovementKey", typeof(Guid));
			dataTable.Columns.Add("GLType", typeof(string));
			AssertLineDesc(TransactionTypes.GLAutoJournal);
			AssertLineDesc(TransactionTypes.GLNoteJournal);
			AssertLineDesc(TransactionTypes.GLReversingJournal);
			AssertLineDesc(TransactionTypes.GLStandardJournal);

			void AssertLineDesc(string type)
			{
				var glJournal = creator.CreateGLJournal(type, new ZDateTime(2013, 08, 13), new ZDateTime(2013, 08, 13), new ZDateTime(2013, 09, 13));
				var line = creator.CreateGLJournalLine(glJournal, 41.38m, DebitCredit.DR, creator.GLHeader1.PK);
				var row = dataTable.Rows.Add();
				row["TransactionHeaderID"] = glJournal.PK.ToGuid();
				row["TransactionLineID"] = line.PK.ToGuid();
				Factory.Save();

				var accountingJournal = new GLAccountingJournal(glJournal, factory, reportingBook, dataTable);
				Assert(accountingJournal.Lines.All(x => x.AL_Desc == line.AL_Desc));
			}
		}

		public void TestMultiSubAccountTypeCode()
		{
			Creator.CreateTestPeriodsForEntireYear(2013);

			var autoJournal = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, new ZDateTime(2013, 08, 13), new ZDateTime(2013, 08, 13), new ZDateTime(2013, 09, 13));
			Creator.CreateGLJournalLine(autoJournal, 250m, DebitCredit.DR, Creator.GLHeader1.PK);
			Creator.CreateGLJournalLine(autoJournal, 250m, DebitCredit.CR, Creator.GLHeader2.PK);
			var accountingJournalForAJL = GetJournalForMultiSubAccountTypeCode(autoJournal, LedgerTypes.General, TransactionTypes.GLAutoJournal);
			AssertEquals(4, accountingJournalForAJL.Lines.Count());
			AssertEquals(2, accountingJournalForAJL.Lines.Count(x => x.MultiSubAccountTypeCode == "ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1"));

			var revJournal = Creator.CreateGLJournal(TransactionTypes.GLReversingJournal, new ZDateTime(2013, 06, 13), new ZDateTime(2013, 06, 13), new ZDateTime(2013, 07, 13));
			Creator.CreateGLJournalLine(revJournal, 350m, DebitCredit.DR, Creator.GLHeader1.PK);
			Creator.CreateGLJournalLine(revJournal, 350m, DebitCredit.CR, Creator.GLHeader2.PK);
			var accountingJournalForRJL = GetJournalForMultiSubAccountTypeCode(revJournal, LedgerTypes.General, TransactionTypes.GLReversingJournal);
			AssertEquals(4, accountingJournalForRJL.Lines.Count());
			AssertEquals(2, accountingJournalForRJL.Lines.Count(x => x.MultiSubAccountTypeCode == "ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1"));

			var stdJournal = Creator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2013, 06, 13), new ZDateTime(2013, 06, 13), new ZDateTime(2013, 07, 13));
			Creator.CreateGLJournalLine(stdJournal, 350m, DebitCredit.DR, Creator.GLHeader1.PK);
			Creator.CreateGLJournalLine(stdJournal, 350m, DebitCredit.CR, Creator.GLHeader2.PK);
			var accountingJournalForGJL = GetJournalForMultiSubAccountTypeCode(stdJournal, LedgerTypes.General, TransactionTypes.GLStandardJournal);
			AssertEquals(2, accountingJournalForGJL.Lines.Count());
			AssertEquals(1, accountingJournalForGJL.Lines.Count(x => x.MultiSubAccountTypeCode == "ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1"));
		}

		public void TestMultiSubAccountTypeCode_WithAllLines()
		{
			Creator.CreateGLHeaderSubAccount(Creator.GLHeader2, OrgHeaderSchema.Constants.Prefix, false);
			Creator.CreateGLHeaderSubAccount(Creator.GLHeader2, AccGroupsSchema.Constants.Prefix, false);
			Creator.CreateGLHeaderSubAccount(Creator.GLHeader2, GlbStaffSchema.Constants.Prefix, false);
			Creator.CreateGLHeaderSubAccount(Creator.GLHeader2, GlbGroupSchema.Constants.Prefix, false);
			Factory.Save();

			Creator.CreateTestPeriodsForEntireYear(2013);

			var autoJournal = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, new ZDateTime(2013, 08, 13), new ZDateTime(2013, 08, 13), new ZDateTime(2013, 09, 13));
			Creator.CreateGLJournalLine(autoJournal, 250m, DebitCredit.DR, Creator.GLHeader1.PK);
			var lineForAJL = Creator.CreateGLJournalLine(autoJournal, 250m, DebitCredit.CR, Creator.GLHeader2.PK);
			SetupSubAccountForLine(lineForAJL);
			var accountingJournalForAJL = GetJournalForMultiSubAccountTypeCode(autoJournal, LedgerTypes.General, TransactionTypes.GLAutoJournal);
			AssertEquals(4, accountingJournalForAJL.Lines.Count());
			AssertEquals(4, accountingJournalForAJL.Lines.Count(x => x.MultiSubAccountTypeCode == "ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1"));

			var revJournal = Creator.CreateGLJournal(TransactionTypes.GLReversingJournal, new ZDateTime(2013, 06, 13), new ZDateTime(2013, 06, 13), new ZDateTime(2013, 07, 13));
			Creator.CreateGLJournalLine(revJournal, 350m, DebitCredit.DR, Creator.GLHeader1.PK);
			var lineForRJL = Creator.CreateGLJournalLine(revJournal, 350m, DebitCredit.CR, Creator.GLHeader2.PK);
			SetupSubAccountForLine(lineForRJL);
			var accountingJournalForRJL = GetJournalForMultiSubAccountTypeCode(revJournal, LedgerTypes.General, TransactionTypes.GLReversingJournal);
			AssertEquals(4, accountingJournalForRJL.Lines.Count());
			AssertEquals(4, accountingJournalForRJL.Lines.Count(x => x.MultiSubAccountTypeCode == "ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1"));

			var stdJournal = Creator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2013, 06, 13), new ZDateTime(2013, 06, 13), new ZDateTime(2013, 07, 13));
			Creator.CreateGLJournalLine(stdJournal, 350m, DebitCredit.DR, Creator.GLHeader1.PK);
			var lineForGJL = Creator.CreateGLJournalLine(stdJournal, 350m, DebitCredit.CR, Creator.GLHeader2.PK);
			SetupSubAccountForLine(lineForGJL);
			var accountingJournalForGJL = GetJournalForMultiSubAccountTypeCode(stdJournal, LedgerTypes.General, TransactionTypes.GLStandardJournal);
			AssertEquals(2, accountingJournalForGJL.Lines.Count());
			AssertEquals(2, accountingJournalForGJL.Lines.Count(x => x.MultiSubAccountTypeCode == "ORG: ABIGAS, SEG: AR1, STR: GS1, SGP: GG1"));
		}

		void SetupSubAccountForLine(GLJournalLine line)
		{
			line.SubAccounts.RemoveAndDeleteAll();
			SetupSubAccount(line.SubAccounts, AccGroupsSchema.Constants.Prefix, Creator.AR1.PK);
			SetupSubAccount(line.SubAccounts, OrgHeaderSchema.Constants.Prefix, Creator.ABIGAS.PK);
			SetupSubAccount(line.SubAccounts, GlbStaffSchema.Constants.Prefix, Creator.GS1.PK);
			SetupSubAccount(line.SubAccounts, GlbGroupSchema.Constants.Prefix, Creator.GG1.PK);
		}

		public void TestAccountingJournalLinesForNJL()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var periodCal = new AccountingPeriodCalculator(Factory);
				var helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupSinglePeriod(ZDateTime.Today.Year * 100 + ZDateTime.Today.Month, ZDateTime.Today, ZDateTime.Today.AddDays(30));
				Factory.Save();

				var glHeader = Creator.CreateGLHeader();
				glHeader.AG_AccountType = AccountType.Note;

				var journal = Creator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
				var line1 = Creator.CreateGLJournalLine(journal, 250.11m, DebitCredit.DR, glHeader.PK);
				line1.AL_Desc = "GL NOTE JOURNAL";
				var line2 = Creator.CreateGLJournalLine(journal, 250.11m, DebitCredit.CR, glHeader.PK);
				line2.AL_Desc = "GL NOTE JOURNAL";
				Factory.Save();

				var ajournal = new GLAccountingJournal(journal, ReadonlyFactory);
				var ajLines = ajournal.Lines.Cast<AccountingJournalLine>().ToArray();

				AssertAJLine(ajLines[0], line1.AL_AC, line1.AL_AG, "GL NOTE JOURNAL", ZString.Empty, line1.AL_PostDate, periodCal.GetPeriodFromDate(ZDateTime.Today), "IMM", line1.AL_LineAmount, ZString.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				AssertAJLine(ajLines[1], line2.AL_AC, line2.AL_AG, "GL NOTE JOURNAL", ZString.Empty, line2.AL_PostDate, periodCal.GetPeriodFromDate(ZDateTime.Today), "IMM", line2.AL_LineAmount, ZString.Empty, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			}
		}

		public void TestValidTransactionTypes()
		{
			var journal = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
			var glAccountingJournal = new GLAccountingJournalForTest(journal, ReadonlyFactory);
			AssertArrayEqualsByElements(new ZString[] { TransactionTypes.GLStandardJournal, TransactionTypes.GLReversingJournal, TransactionTypes.GLAutoJournal, TransactionTypes.GLNoteJournal }, glAccountingJournal.ValidTransactionTypesForTest.ToArray());
		}

		public void TestLocalCurrencyDecimals()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				var glAccountingJournal = (GLAccountingJournal)GetNewBusinessObject();
				AssertEquals("local currency decimals should be 0",0, glAccountingJournal.LocalCurrencyDecimals);

				var noteJournal = Creator.CreateGLJournal(TransactionTypes.GLNoteJournal, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today);
				var noteGLAccountingJournal = new GLAccountingJournal(noteJournal, ReadonlyFactory);
				AssertEquals("local currency decimals should be 2 for note journal", 2, noteGLAccountingJournal.LocalCurrencyDecimals);
			}
		}

		class GLAccountingJournalForTest : GLAccountingJournal
		{
			public GLAccountingJournalForTest(GLJournal transaction, ReadOnlyBusinessObjectFactory factory)
			: base(transaction, factory)
			{
			}

			public IEnumerable<ZString> ValidTransactionTypesForTest => ValidTransactionTypes;
		}
	}
}
