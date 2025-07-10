using System;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	[TestedType(typeof(GLJournalDataAdapter))]
	sealed class GLJournalDataAdapterTest : BaseAccountingDataAdapterTest<GLJournal, Xsd.GLJournal>
	{
		public void TestImport_InvalidJournalXML()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("InvalidGLJournal.xml")))
			{
				DoImport(stream);
			}

			AssertEquals(GLJournalDataAdapter.InvalidXmlFileErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImport_MorethenOneJournals()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidGJL2Journals.xml")))
			{
				DoImport(stream);
			}

			AssertEquals(GLJournalDataAdapter.MoreThanOneJournalErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImport_ValidJournalXML()
		{
			AssertImport_ValidJournalXML(new GLJournalDataAdapterSettings { useJournalNumber = false });
		}

		public void TestImport_ValidJournalXML_UseJobNumber()
		{
			AssertImport_ValidJournalXML(new GLJournalDataAdapterSettings { useJournalNumber = true });
		}

		public void TestImport_UsingPredefinedFactory()
		{
			var predefinedFactory = new BusinessObjectFactory();
			var importedJournal = AssertImport_ValidJournalXML(new GLJournalDataAdapterSettings { factoryForNewJournal = predefinedFactory });
			AssertEquals("journal Factory", predefinedFactory, importedJournal.Factory);
		}

		GLJournal AssertImport_ValidJournalXML(GLJournalDataAdapterSettings settings)
		{
			var expectedOrg = TestObjectCreator.TestOrganisation;
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidGJLJournal.xml")))
			{
				DoImport(stream, settings);
			}

			AssertEquals("performance checkpoint, it should be only 4 calls per importing Journal", 4, Journal.GetSumOfLinesCallAmount_ForTestOnly);

			AssertEquals(settings.useJournalNumber ? "1234" : "", Journal.AH_TransactionNum);
			AssertEquals("BBB", Journal.AH_TransactionCategory);
			AssertEquals(200501, Journal.PostPeriod);
			AssertEquals(0, Journal.AgePeriod);
			AssertEquals(2, Journal.GLJournalLines.Count);

			AssertEquals("2010.00.00", Journal.GLJournalLines[0].GLHeader.AccountNum);
			AssertEquals("BNE", Journal.GLJournalLines[0].Branch.GB_Code);
			AssertEquals("BRN", Journal.GLJournalLines[0].Department.GE_Code);
			AssertEquals("AUD", Journal.GLJournalLines[0].AL_RX_NKTransactionCurrency);
			AssertEquals(1m, Journal.GLJournalLines[0].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[0].UnsignedOSLineAmount);
			AssertEquals(new ZString("DR"), Journal.GLJournalLines[0].DebitCreditSign);
			AssertEquals(expectedOrg.PK, Journal.GLJournalLines[0].AL_OH);

			AssertEquals("2020.00.00", Journal.GLJournalLines[1].GLHeader.AccountNum);
			AssertEquals("SYD", Journal.GLJournalLines[1].Branch.GB_Code);
			AssertEquals("CIA", Journal.GLJournalLines[1].Department.GE_Code);
			AssertEquals("Test Line 2", Journal.GLJournalLines[1].AL_Desc);
			AssertEquals("AUD", Journal.GLJournalLines[1].AL_RX_NKTransactionCurrency);
			AssertEquals(1m, Journal.GLJournalLines[1].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[1].UnsignedOSLineAmount);
			AssertEquals(new ZString("CR"), Journal.GLJournalLines[1].DebitCreditSign);

			return Journal;
		}

		public void TestImport_ValidJournalXMLWithCurrency()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidGJLJournalWithCurrency.xml")))
			{
				DoImport(stream);
			}

			AssertEquals(200501, Journal.PostPeriod);
			AssertEquals(0, Journal.AgePeriod);
			AssertEquals(5, Journal.GLJournalLines.Count);

			int i = 0;
			AssertEquals("2010.00.00", Journal.GLJournalLines[i].GLHeader.AccountNum);
			AssertEquals("BNE", Journal.GLJournalLines[i].Branch.GB_Code);
			AssertEquals("BRN", Journal.GLJournalLines[i].Department.GE_Code);
			AssertEquals("USD", Journal.GLJournalLines[i].AL_RX_NKTransactionCurrency);
			AssertEquals(11m, Journal.GLJournalLines[i].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[i].UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(110), Journal.GLJournalLines[i].UnsignedOSLineAmount);
			AssertEquals(new ZString("DR"), Journal.GLJournalLines[i].DebitCreditSign);

			i++;
			AssertEquals("2020.00.00", Journal.GLJournalLines[i].GLHeader.AccountNum);
			AssertEquals("SYD", Journal.GLJournalLines[i].Branch.GB_Code);
			AssertEquals("CIA", Journal.GLJournalLines[i].Department.GE_Code);
			AssertEquals("Test Line 2", Journal.GLJournalLines[i].AL_Desc);
			AssertEquals("GBP", Journal.GLJournalLines[i].AL_RX_NKTransactionCurrency);
			AssertEquals(22m, Journal.GLJournalLines[i].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[i].UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(220), Journal.GLJournalLines[i].UnsignedOSLineAmount);
			AssertEquals(new ZString("CR"), Journal.GLJournalLines[i].DebitCreditSign);

			i++;
			AssertEquals("2030.00.00", Journal.GLJournalLines[i].GLHeader.AccountNum);
			AssertEquals("SYD", Journal.GLJournalLines[i].Branch.GB_Code);
			AssertEquals("CIA", Journal.GLJournalLines[i].Department.GE_Code);
			AssertEquals("Test Line 3", Journal.GLJournalLines[i].AL_Desc);
			AssertEquals("AUD", Journal.GLJournalLines[i].AL_RX_NKTransactionCurrency);
			AssertEquals(1m, Journal.GLJournalLines[i].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[i].UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[i].UnsignedOSLineAmount);
			AssertEquals(new ZString("DR"), Journal.GLJournalLines[i].DebitCreditSign);

			i++;
			AssertEquals("2030.00.00", Journal.GLJournalLines[i].GLHeader.AccountNum);
			AssertEquals("SYD", Journal.GLJournalLines[i].Branch.GB_Code);
			AssertEquals("CIA", Journal.GLJournalLines[i].Department.GE_Code);
			AssertEquals("Test Line 4", Journal.GLJournalLines[i].AL_Desc);
			AssertEquals("AUD", Journal.GLJournalLines[i].AL_RX_NKTransactionCurrency);
			AssertEquals(1m, Journal.GLJournalLines[i].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[i].UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[i].UnsignedOSLineAmount);
			AssertEquals(new ZString("CR"), Journal.GLJournalLines[i].DebitCreditSign);

			i++;
			AssertEquals("2030.00.00", Journal.GLJournalLines[i].GLHeader.AccountNum);
			AssertEquals("SYD", Journal.GLJournalLines[i].Branch.GB_Code);
			AssertEquals("CIA", Journal.GLJournalLines[i].Department.GE_Code);
			AssertEquals("Test Line 5", Journal.GLJournalLines[i].AL_Desc);
			AssertEquals("USD", Journal.GLJournalLines[i].AL_RX_NKTransactionCurrency);
			AssertEquals(4m, Journal.GLJournalLines[i].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[i].UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(40), Journal.GLJournalLines[i].UnsignedOSLineAmount);
			AssertEquals(new ZString("CR"), Journal.GLJournalLines[i].DebitCreditSign);
		}

		public void TestImport_JournalXMLWithInvalidCurrency()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("GJLJournalWithInvalidCurrency.xml")))
			{
				DoImport(stream);
			}

			AssertEquals(200501, Journal.PostPeriod);
			AssertEquals(0, Journal.AgePeriod);
			AssertEquals(2, Journal.GLJournalLines.Count);

			AssertEquals("2010.00.00", Journal.GLJournalLines[0].GLHeader.AccountNum);
			AssertEquals("BNE", Journal.GLJournalLines[0].Branch.GB_Code);
			AssertEquals("BRN", Journal.GLJournalLines[0].Department.GE_Code);
			AssertEquals("US", Journal.GLJournalLines[0].AL_RX_NKTransactionCurrency);
			AssertEquals(11m, Journal.GLJournalLines[0].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[0].UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(110), Journal.GLJournalLines[0].UnsignedOSLineAmount);
			AssertEquals(new ZString("DR"), Journal.GLJournalLines[0].DebitCreditSign);

			AssertEquals("2020.00.00", Journal.GLJournalLines[1].GLHeader.AccountNum);
			AssertEquals("SYD", Journal.GLJournalLines[1].Branch.GB_Code);
			AssertEquals("CIA", Journal.GLJournalLines[1].Department.GE_Code);
			AssertEquals("Test Line 2", Journal.GLJournalLines[1].AL_Desc);
			AssertEquals("GBP", Journal.GLJournalLines[1].AL_RX_NKTransactionCurrency);
			AssertEquals(22m, Journal.GLJournalLines[1].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[1].UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(220), Journal.GLJournalLines[1].UnsignedOSLineAmount);
			AssertEquals(new ZString("CR"), Journal.GLJournalLines[1].DebitCreditSign);
		}

		public void TestImport_ValidJournalXMLWithoutCurrency()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidGJLJournalWithoutCurrency.xml")))
			{
				DoImport(stream);
			}

			AssertEquals(201509, Journal.PostPeriod);
			AssertEquals(0, Journal.AgePeriod);
			AssertEquals(2, Journal.GLJournalLines.Count);

			var line = Journal.GLJournalLines[0];
			AssertEquals("1010.10.10", line.GLHeader.AccountNum);
			AssertEquals("SYD", line.Branch.GB_Code);
			AssertEquals("FIS", line.Department.GE_Code);
			AssertEquals("AUD", line.AL_RX_NKTransactionCurrency);
			AssertEquals(1m, line.AL_ExchangeRate);
			AssertEquals(new ZDecimal(300), line.UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(300), line.UnsignedOSLineAmount);
			AssertEquals(new ZString("CR"), line.DebitCreditSign);

			line = Journal.GLJournalLines[1];
			AssertEquals("3020.00.00", line.GLHeader.AccountNum);
			AssertEquals("SYD", line.Branch.GB_Code);
			AssertEquals("BRN", line.Department.GE_Code);
			AssertEquals("EPC003424  /CCFCLF/1/3210-6-30-30", line.AL_Desc);
			AssertEquals("AUD", line.AL_RX_NKTransactionCurrency);
			AssertEquals(1m, line.AL_ExchangeRate);
			AssertEquals(new ZDecimal(300), line.UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(300), line.UnsignedOSLineAmount);
			AssertEquals(new ZString("DR"), line.DebitCreditSign);
		}

		public void TestImport_ValidJournalXMLWithdifferentDecimalPlaces()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidGJLJournalWithDifferentDecimalPlaces.xml")))
			{
				DoImport(stream);
			}

			AssertEquals(201509, Journal.PostPeriod);
			AssertEquals(0, Journal.AgePeriod);
			AssertEquals(2, Journal.GLJournalLines.Count);

			var line = Journal.GLJournalLines[0];
			AssertEquals("AUD", line.AL_RX_NKTransactionCurrency);
			AssertEquals(1m, line.AL_ExchangeRate);
			AssertEquals("value should be rounded to currency decimal places", new ZDecimal(300.63), line.UnsignedLocalLineAmount);
			AssertEquals("value should be rounded to currency decimal places", new ZDecimal(300.63), line.UnsignedOSLineAmount);
			AssertEquals("value should be rounded to currency decimal places", new ZDecimal(300.63), line.AL_LineAmount);
			AssertEquals("value should be rounded to currency decimal places", new ZDecimal(300.63), line.AL_OSExTaxAmount);
			AssertNoExceptionThrown(CriticalValidationMessageTemplate.TransactionLineLocalAmountNotEqualForeignWithExRate1ErrorMessage, () => { Factory.Save(); });
		}

		public void TestImport_FCB()
		{
			var expectedNumber = "2010.00.00";
			var glAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, expectedNumber));
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glAccount.PK.ToGuid());

			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidFCBJournal.xml")))
			{
				DoImport(stream);
			}

			AssertType<FCBAdjustmentJournal>(Journal);
			AssertEquals("GJL", ReceiptTypes.ForeignCurrencyBalance, Journal.AH_ReceiptType);
			AssertEquals("GJL", Journal.AH_TransactionType);
			AssertEquals(0, Journal.AgePeriod);
			AssertEquals(GLJournalDataAdapter.FCBJournalDescription, Journal.AH_Desc);
			AssertEquals(GLJournalDataAdapter.FCBJournalDescription, Journal.GLJournalLines[0].AL_Desc);
			AssertEquals("Test Line 2", Journal.GLJournalLines[1].AL_Desc);

			AssertEquals(2, Journal.GLJournalLines.Count);

			AssertEquals(expectedNumber, Journal.GLJournalLines[0].GLHeader.AccountNum);
			AssertEquals("BNE", Journal.GLJournalLines[0].Branch.GB_Code);
			AssertEquals("BRN", Journal.GLJournalLines[0].Department.GE_Code);
			AssertEquals("USD", Journal.GLJournalLines[0].AL_RX_NKTransactionCurrency);
			AssertEquals(11m, Journal.GLJournalLines[0].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[0].UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(110), Journal.GLJournalLines[0].UnsignedOSLineAmount);
			AssertEquals(new ZString("DR"), Journal.GLJournalLines[0].DebitCreditSign);

			AssertEquals("2020.00.00", Journal.GLJournalLines[1].GLHeader.AccountNum);
			AssertEquals("SYD", Journal.GLJournalLines[1].Branch.GB_Code);
			AssertEquals("CIA", Journal.GLJournalLines[1].Department.GE_Code);
			AssertEquals("Test Line 2", Journal.GLJournalLines[1].AL_Desc);
			AssertEquals("GBP", Journal.GLJournalLines[1].AL_RX_NKTransactionCurrency);
			AssertEquals(22m, Journal.GLJournalLines[1].AL_ExchangeRate);
			AssertEquals(new ZDecimal(10), Journal.GLJournalLines[1].UnsignedLocalLineAmount);
			AssertEquals(new ZDecimal(0), Journal.GLJournalLines[1].UnsignedOSLineAmount);
			AssertEquals(new ZString("DR"), Journal.GLJournalLines[1].DebitCreditSign);
		}

		public void TestImport_GJL()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidGJLJournal.xml")))
			{
				DoImport(stream);
			}

			AssertEquals("GJL", Journal.AH_TransactionType);
			AssertEquals(0, Journal.AgePeriod);
			AssertEquals(GLJournalDataAdapter.GeneralJournalDescription, Journal.AH_Desc);
			var lines = Journal.GLJournalLines;
			AssertEquals(GLJournalDataAdapter.GeneralJournalDescription, lines[0].AL_Desc);
			AssertEquals("Test Line 2", lines[1].AL_Desc);
		}

		public void TestImport_AJL()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidAJLJournal.xml")))
			{
				DoImport(stream);
			}
			AssertEquals("AJL", Journal.AH_TransactionType);
			AssertEquals(200502, Journal.AgePeriod);
			AssertEquals(GLJournalDataAdapter.AutoJournalDescription, Journal.AH_Desc);
			var lines = Journal.GLJournalLines;
			AssertEquals(GLJournalDataAdapter.AutoJournalDescription, lines[0].AL_Desc);
			AssertEquals("Test Line 2", lines[1].AL_Desc);
		}

		public void TestImport_RJL()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidRJLJournal.xml")))
			{
				DoImport(stream);
			}

			AssertEquals("RJL", Journal.AH_TransactionType);
			AssertEquals(200502, Journal.AgePeriod);
			AssertEquals(new ZDateTime(2004, 07, 31, 23, 59, 0), Journal.AH_PostDate);
			AssertEquals(new ZDateTime(2004, 08, 01, 0, 0, 0), Journal.AH_DueDate);
			AssertEquals(GLJournalDataAdapter.ReverseJournalDescription, Journal.AH_Desc);
			var lines = Journal.GLJournalLines;
			AssertEquals(GLJournalDataAdapter.ReverseJournalDescription, lines[0].AL_Desc);
			AssertEquals("Test Line 2", lines[1].AL_Desc);
		}

		public void TestImport_NJL()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidNJLJournal.xml")))
			{
				DoImport(stream);
			}

			AssertEquals("NJL", Journal.AH_TransactionType);
			AssertEquals(0, Journal.AgePeriod);
			AssertEquals(GLJournalDataAdapter.NoteJournalDescription, Journal.AH_Desc);
			var lines = Journal.GLJournalLines;
			AssertEquals(GLJournalDataAdapter.NoteJournalDescription, lines[0].AL_Desc);
			AssertEquals("Test Line 2", lines[1].AL_Desc);
		}

		public void TestImport_BranchDepartmentInGLHeader()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidGJLJournaBrnDept.xml")))
			{
				DoImport(stream);
			}

			AssertEquals("BNE", Journal.GLJournalLines[0].Branch.GB_Code);
			AssertEquals("BRN", Journal.GLJournalLines[0].Department.GE_Code);
		}

		public void TestImport_DefaultBranchDepartment()
		{
			using (StreamReader stream = new StreamReader(Retriever.GetStream("ValidGJLJournaBrnDept.xml")))
			{
				DoImport(stream);
			}

			AssertEquals(GlbBranch.CurrentBranch.GB_Code, Journal.GLJournalLines[0].Branch.GB_Code);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, Journal.GLJournalLines[0].Department.GE_Code);
		}

		public void TestGLJournalExportToValueObjectCoreWithNullReference()
		{
			var glAccount = TestObjectCreator.CreateGLHeader("TestGLAcc");
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, glAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, glAccount.PK);
			Factory.Save();

			glAccount.Delete();
			var request = Factory.New<GLJournalApprovalRequest>();
			AssertNoExceptionThrown(() => request.Initialize(journal));

			journal.Lines.Cast<GLJournalLine>().ForEach(x => x.AL_GE = ZGuid.Empty);
			AssertNoExceptionThrown(() => request.Initialize(journal));

			journal.Lines.Cast<GLJournalLine>().ForEach(x => x.AL_GB = ZGuid.Empty);
			AssertNoExceptionThrown(() => request.Initialize(journal));
		}

		public void TestCsvImportMultipleSubAccounts()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var staff = testObjectCreator.CreateStaff("TST");

			var glHeader1 = testObjectCreator.GetGLAccountFromDB("2010.00.00");
			testObjectCreator.CreateGLHeaderSubAccount(glHeader1, OrgHeaderSchema.Constants.Prefix, false);
			testObjectCreator.CreateGLHeaderSubAccount(glHeader1, GlbStaffSchema.Constants.Prefix, false);
			var glHeader2 = testObjectCreator.GetGLAccountFromDB("2020.00.00");
			testObjectCreator.CreateGLHeaderSubAccount(glHeader2, AccGroupsSchema.Constants.Prefix, false);
			var glHeader3 = testObjectCreator.GetGLAccountFromDB("2020.10.00");
			testObjectCreator.CreateGLHeaderSubAccount(glHeader3, GlbGroupSchema.Constants.Prefix, false);

			Factory.Save();

			using (var stream = new StreamReader(Retriever.GetStream("GJLJournalWithMultipleSubAccounts.xml")))
			{
				DoImport(stream);
			}

			AssertEquals(4, Journal.GLJournalLines.Count);

			// XML import follows line sequence in source file. Hence use collection index here.
			AssertEquals("2010.00.00", Journal.GLJournalLines[0].GLHeader.AccountNum);
			AssertEquals(2, Journal.GLJournalLines[0].SubAccounts.Count);
			var subAccount1 = Journal.GLJournalLines[0].SubAccounts[0];
			var subAccount2 = Journal.GLJournalLines[0].SubAccounts[1];
			AssertEquals(OrgHeaderSchema.Constants.Prefix, subAccount1.AL1_SubClassParentTableCode);
			AssertEquals(testObjectCreator.ABIGAS.PK, subAccount1.AL1_SubClassParentId);
			AssertEquals(GlbStaffSchema.Constants.Prefix, subAccount2.AL1_SubClassParentTableCode);
			AssertEquals(staff.PK, subAccount2.AL1_SubClassParentId);

			AssertEquals("2020.00.00", Journal.GLJournalLines[1].GLHeader.AccountNum);
			AssertEquals(1, Journal.GLJournalLines[1].SubAccounts.Count);
			var subAccount3 = Journal.GLJournalLines[1].SubAccounts[0];
			AssertEquals(AccGroupsSchema.Constants.Prefix, subAccount3.AL1_SubClassParentTableCode);
			AssertEquals("Sub Account with empty code could be imported, but with empty parent ID.", Guid.Empty, subAccount3.AL1_SubClassParentId);

			AssertEquals("2020.10.00", Journal.GLJournalLines[2].GLHeader.AccountNum);
			AssertEquals(1, Journal.GLJournalLines[2].SubAccounts.Count);
			var subAccount4 = Journal.GLJournalLines[2].SubAccounts[0];
			AssertEquals(GlbGroupSchema.Constants.Prefix, subAccount4.AL1_SubClassParentTableCode);
			AssertEquals(Guid.Empty, subAccount4.AL1_SubClassParentId);

			AssertEquals("2020.20.00", Journal.GLJournalLines[3].GLHeader.AccountNum);
			AssertEquals(0, Journal.GLJournalLines[3].SubAccounts.Count);
		}

		public void TestExportToValueObjectCoreWithSubAccounts()
		{
			var testGLJournalDataAdapter = new TestGLJournalDataAdapter();

			var glHeader = TestObjectCreator.CreateGLHeaderWithSubAccount(OrgHeaderSchema.Constants.Prefix, false);

			var glJournalXML = new Xsd.GLJournal();

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			var line = (GLJournalLine)journal.Lines.AddNew();
			line.AL_AG = glHeader.PK;
			line.SubAccounts.FirstSubAccount.AL1_SubClassParentId = TestObjectCreator.Creditor1.PK;

			AssertEquals(0, glJournalXML.JournalLines.Count);

			testGLJournalDataAdapter.ExportToValueObjectCoreTest(journal, glJournalXML, null);

			AssertEquals(1, glJournalXML.JournalLines.Count);
			AssertEquals(1, glJournalXML.JournalLines[0].SubAccounts.Count);

			var subAccount = glJournalXML.JournalLines[0].SubAccounts[0];
			AssertEquals("ZCreditor1", subAccount.Code);
			AssertEquals("ORG", subAccount.Type.Code);
			AssertEquals(AccountingMasterFilesConstants.SubAccountTypeList.Organization.Description, subAccount.Type.Description);
		}

		[TestDate(2020, 5, 21)]
		public void TestExportToValueObjectForNoteJournal()
		{
			var today = ZDateTime.Today;
			var glHeader = TestObjectCreator.CreateGLHeader();
			glHeader.AG_AccountNum = "2020.10.00";
			glHeader.AG_AccountType = Constants.AccountType.Note;
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLNoteJournal, today, today.AddMonths(-1), today.AddMonths(3));
			journal.PostPeriod = 202005;
			var line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, glHeader.PK);
			line.AL_Desc = "GL NOTE JOURNAL";

			TestExportToValueObject(new BusinessObjectAndExpectedOutputFileName(journal, Retriever.SaveResourceToFile("PopulatedNoteJournal.xml"), ValidationKind.None, "Populate Note Journal"));
		}

		[TestDate(2022, 8, 22)]
		public void TestExportToValueObjectForReversingJournal()
		{
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var today = ZDateTime.Today;
				var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLReversingJournal, today, today, today.AddDays(35));

				var line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
				line.AL_Desc = "GL REVERSING JOURNAL";
				var clearLine = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
				clearLine.AL_Desc = "Clearing line";

				TestExportToValueObject(new BusinessObjectAndExpectedOutputFileName(journal, Retriever.SaveResourceToFile("PopulatedReversingJournal.xml"), ValidationKind.None, "Populate Reversing Journal"));
			}
		}

		#region Base Tests

		[TestDate(2010, 09, 04)]
		public override void TestExportToValueObject_ForEmptyBizO()
		{
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				base.TestExportToValueObject_ForEmptyBizO();
			}
		}

		[TestDate(2010, 09, 04)]
		public override void TestExportToValueObject_ForFullyPopulatedBizO()
		{
			base.TestExportToValueObject_ForFullyPopulatedBizO();
		}

		[TestDate(2010, 09, 04)]
		public override void TestTestCoverageOfValueObject()
		{
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				base.TestTestCoverageOfValueObject();
			}
		}

		[TestDate(2010, 09, 04)]
		public new void TestExportToValueObject_ForMiscSamples()
		{
			base.TestExportToValueObject_ForMiscSamples();
		}

		[TestDate(2010, 09, 04)]
		public override void TestExportToValueObject_ForPopulatedBizObjWithEmptyFields()
		{
			base.TestExportToValueObject_ForPopulatedBizObjWithEmptyFields();
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return true; }
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return true; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return fGLJournalDataAdapter.RootCollectionElementName; }
		}

		protected override string ExpectedRootElementName
		{
			get { return fGLJournalDataAdapter.RootElementName; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var journal = Factory.New<GLJournal>();
			journal.AH_PostDate = new ZDateTime(2010, 09, 30);
			return new BusinessObjectAndExpectedOutputFileName(journal, Retriever.SaveResourceToFile("EmptyJournal.xml"), ValidationKind.None, "Empty Journal");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLAutoJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			var line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);

			return new BusinessObjectAndExpectedOutputFileName(journal, Retriever.SaveResourceToFile("SemiPopulatedJournal.xml"), ValidationKind.None, "Semi Populated Journal");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLReversingJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			journal.AH_TransactionNum = "SomeNumber";
			journal.AH_TransactionCategory = "AAA";
			var line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.GBP.Code;
			line.AL_ExchangeRate = 0.5;

			var org = TestObjectCreator.CreateOrgHeader("TSTORG1", true, true);

			line.SubAccounts.Add(TestObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.Organization, TestObjectCreator.ABIGAS.PK));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, "", org.PK.ToGuid()));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, AccountingMasterFilesConstants.NAV.Code));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, AccountingMasterFilesConstants.LFOCodes.LOC));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, AccountingMasterFilesConstants.LFECodes.LOC));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, AccountingMasterFilesConstants.TICCodes.STI));
			line.AccTransactionLineDissectionAttributes.Add(TestObjectCreator.CreateTransactionLineDissectionAttribtues(line.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, AccountingMasterFilesConstants.SPRCodes.SPR));

			line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			line.AL_ExchangeRate = 2.5;
			line.AL_OH = TestObjectCreator.TestOrganisation.PK;

			return new BusinessObjectAndExpectedOutputFileName(journal, Retriever.SaveResourceToFile("FullyPopulatedJournal.xml"), ValidationKind.None, "Fully Populated Journal");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			AdapterSettings = new GLJournalDataAdapterSettings() { useJournalNumber = true };

			var journal = TestObjectCreator.CreateFCBJournal(ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			journal.AH_TransactionNum = "SomeNumber";
			var line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.GBP.Code;
			line.AL_ExchangeRate = 0.5;
			line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			line.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			line.AL_ExchangeRate = 2.5;

			return new[]
				{
					new BusinessObjectAndExpectedOutputFileName(journal, Retriever.SaveResourceToFile("FullyPopulatedFCBJournal.xml"), ValidationKind.None, "Fully Populated FCB Journal")
				};
		}

		GLJournalDataAdapterSettings? AdapterSettings;

		protected override ValueObjectDataAdapter<GLJournal, Xsd.GLJournal> GetNewBizObjXmlDataAdapter()
		{
			var adapter = new GLJournalDataAdapter();
			if (AdapterSettings.HasValue)
			{
				adapter.Initialize(AdapterSettings.Value);
			}

			return adapter;
		}

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert("Shouldn't be imported twice for the same Journal", true);
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new[]
				{
					"GLDetail/JournalNumber", //this is field for internal usage only. There is a test for it in AccountingBusiness project for GLJournalApprovalRequest or GLJournalApprovalRequestDetails.
					"JournalLines/Organisation/OrganisationDetails", //organisation field is tested, this is just some additional fields for fully populated object
					"JournalLines/Organisation/Notes", //organisation field is tested, this is just some additional fields for fully populated object
					"JournalLines/PK" //this is field for internal usage only. There is a test in GLJournalApprovalRequestTest that covers its usage and it should fail if PK field is exported/imported incorrectly
				};
			}
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			retriever?.Dispose();
			retriever = null;
		}

		EmbeddedResourceRetriever Retriever => retriever ??= new ();
		EmbeddedResourceRetriever retriever;

		GLJournal Journal;
		TestGLJournalDataAdapter Adapter;
		JournalXmlDataTransferDirectorTestClass Director;
		GLJournalDataAdapter fGLJournalDataAdapter;

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		void DoImport(StreamReader stream, GLJournalDataAdapterSettings? settings = null)
		{
			Adapter = new TestGLJournalDataAdapter();
			if (settings.HasValue)
			{
				Adapter.Initialize(settings.Value);
			}
			Director = new JournalXmlDataTransferDirectorTestClass(new TestGLJournalDataAdapter(), false);

			XmlDocument xmlDoc = Director.LoadXmlDoc(stream.BaseStream);
			Xsd.GLJournal journalNode = Director.ExtractJournalNode(xmlDoc);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			Journal = Adapter.CreateOrUpdateFromValueObject(journalNode, context);
		}

		protected override void SetUp()
		{
			fGLJournalDataAdapter = new GLJournalDataAdapter();
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
		}

		class TestGLJournalDataAdapter : GLJournalDataAdapter
		{
			public void ImportFromValueObjectForTest(GLJournal bizObj, Xsd.GLJournal value, ValueObjectImportContext context)
			{
				ImportFromValueObjectCore(bizObj, value, context);
			}

			public void ExportToValueObjectCoreTest(GLJournal bizObj, Xsd.GLJournal constructedValueObject, IValueObjectExportContext context)
			{
				ExportToValueObjectCore(bizObj, constructedValueObject, context);
			}
		}

		class JournalXmlDataTransferDirectorTestClass : GlJournalXmlDataTransferDirector
		{
			public JournalXmlDataTransferDirectorTestClass(GLJournalDataAdapter adapter, bool hasLicence)
				: base(adapter, hasLicence)
			{
			}

			public XmlDocument LoadXmlDoc(Stream xmlDocStream)
			{
				return base.LoadXmlFileIntoXmlDoc(xmlDocStream);
			}

			public Xsd.GLJournal ExtractJournalNode(XmlDocument journalXMLDocument)
			{
				return base.ExtractJournalNodeFromXml(journalXMLDocument);
			}
		}

		#endregion
	}
}
