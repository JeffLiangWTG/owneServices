using System;
using System.Collections.Specialized;
using System.IO;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataConverters.Accounting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.Accounting
{
	class ConverterTest : AccountingConverterBaseTest
	{
		public void TestBlankFileNameValidation()
		{
			var converter = GetConverter();

			converter.ImportFile("", true, ZDateTime.Now);
			AssertEquals("Blank File Name", true, converter.Errors.Contains("Please enter the file location"));

			AssertEquals(1, ErrorReceived.Count);
			Assert(ErrorReceived.Contains("Please enter the file location"));
		}

		Converter GetConverter()
		{
			var converter = new Converter();
			converter.ErrorOccurred += new EventHandler(Converter_OnError);
			return converter;
		}

		[TestDate(2003, 06, 06)]
		public void TestPostDateValidationBeforeImport()
		{
			var testFileName = Env.TempPath + "test.csv";
			try
			{
				using (var writer = new StreamWriter(testFileName))
				{
					WriteHeaderLine(writer);
					writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
				}

				var converter = new Converter();
				var postDate = ZDateTime.MinSmallDateTimeValue.AddHours(-1);
				converter.ImportFile(testFileName, false, postDate);
				AssertContains("[Accounts Payable Journal] Invoice Post Date: This date does not fall into a valid date range", converter.Errors[0]);
				converter.ClearErrors();

				postDate = ZDateTime.MaxSmallDateTime.AddHours(1);
				converter.ImportFile(testFileName, false, postDate);
				AssertContains("[Accounts Payable Journal] Invoice Post Date: This date does not fall into a valid date range", converter.Errors[0]);
				converter.ClearErrors();

				postDate = ZDateTime.MaxSmallDateTime.AddDays(-1);
				converter.ImportFile(testFileName, false, postDate);
				AssertContains("[Accounts Payable Journal] Invoice Post Date: This date does not fall into a valid accounting period’s date range.", converter.Errors[0]);
				converter.ClearErrors();

				postDate = Helper.PreviousSubLedgerClosedPeriod.AM_StartDate.AddDays(5);
				converter.ImportFile(testFileName, false, postDate);
				AssertContains("[Accounts Payable Journal] Invoice Post Date: This date falls into a period where the sub-ledger is closed", converter.Errors[0]);
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		AccountingPeriodTestHelper Helper
		{
			get
			{
				return fHelper ?? (fHelper = new AccountingPeriodTestHelper());
			}
		}
		AccountingPeriodTestHelper fHelper;

		public void TestFileExistValidation()
		{
			var testFileName = "DummyFile";
			var converter = GetConverter();

			converter.ImportFile(testFileName, true, ZDateTime.Now);
			AssertEquals("Invalid file name", true, converter.Errors.Contains("File " + testFileName + " doesn't exist!"));
			AssertEquals(1, ErrorReceived.Count);
			Assert(ErrorReceived.Contains("File " + testFileName + " doesn't exist!"));
		}

		public void TestPostDateValidation()
		{
			var testFileName = Env.TempPath + "test.csv";

			try
			{
				using (var writer = new StreamWriter(testFileName))
				{
					var converter = GetConverter();

					converter.ImportFile(testFileName, true, ZDateTime.Invalid);
					AssertEquals("Invalid post date", true, converter.Errors.Contains("Please enter a valid Post Date"));
					AssertEquals(1, ErrorReceived.Count);
					Assert(ErrorReceived.Contains("Please enter a valid Post Date"));
				}
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		public void TestFileHeaderFormatValidation()
		{
			var fileName = Env.TempPath + "test.csv";

			using (var writer = new StreamWriter(fileName))
			{
				writer.WriteLine("This is the header for the file.");
				writer.WriteLine("-------------------");
			}

			try
			{
				var converter = GetConverter();

				converter.ImportFile("rubbish", true, ZDateTime.Now);
				AssertEquals("File not found", 1, converter.Errors.Count);
				AssertEquals(1, ErrorReceived.Count);
				Assert(ErrorReceived.Contains("File rubbish doesn't exist!"));
			}
			finally
			{
				File.Delete(fileName);
			}
		}

		[TestDate(2003, 06, 05)]
		public void TestImportInvalidDatesAndEmptyLines()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			var currCompanyBranch = GetNewBranchInDB(GlbCompany.CurrentCompany);

			var creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, creator.GLHeader1.PK.ToGuid());

			var testFileName = Env.TempPath + "test.csv";

			var code = "TestCode";
			Organisation.OH_Code = code;
			Organisation.OH_IsDebtor = true;
			Factory.Save();
			try
			{
				using (var writer = new StreamWriter(testFileName))
				{
					WriteHeaderLine(writer);
					writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction2\",\"20030606\",\"20030705\",\"USD\",\"210.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction3\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction4\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction5\",\"20040219\",\"20040229\",\"AUD\",\"500.11\",\"500.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction7\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine("");
					writer.WriteLine(" ");
					writer.WriteLine("\r\n");

					// These transactions should fail.
					writer.WriteLine(string.Format("\"{0}\",\"NonExistDate1\",\"20030606\",\"20030745\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"NonExistDate1\",\"20030606\",\"20035563\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"OutofBoundsDate\",\"21030606\",\"21030609\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"BadTransaction\",\"35022004\",\"14032004\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
				}
				var converter = new Converter();
				var createdJournals = converter.ImportFile(testFileName, true, ZDateTime.Now);
				AssertEquals(6, createdJournals.Length);
				AssertEquals("Error count should be 4 because blank lines are consumed", 4, converter.Errors.Count);

				AssertContains("Should have invalid due date error", "Due Date: Please enter a Due Date", converter.Errors[0]);
				AssertContains("Should have invalid due date error", "Due Date: Please enter a Due Date", converter.Errors[1]);
				AssertContains("Should have invalid due date error", "[Accounts Receivable Journal] Due Date: The date '09-Jun-2103' is later than '06-Jun-2079', the limit for this field.", converter.Errors[2]);
				AssertContains("Should have invalid due date error", "Due Date: Please enter a Due Date", converter.Errors[3]);

				AssertContains("Should have invalid invoice date error", "The date '06-Jun-2103' is later than '06-Jun-2079', the limit for this field", converter.Errors[2]);
				AssertContains("Should have invalid invoice date error", "[Accounts Receivable Journal] Invoice Date: Please enter an Invoice Date.", converter.Errors[3]);
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		[TestDate(2003, 06, 05)]
		public void TestImportARData()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			var currCompanyBranch = GetNewBranchInDB(GlbCompany.CurrentCompany);

			var creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, creator.GLHeader1.PK.ToGuid());

			var testFileName = Env.TempPath + "test.csv";

			var code = "TestCode";
			Organisation.OH_Code = code;
			Organisation.OH_IsDebtor = true;
			Factory.Save();
			try
			{
				using (var writer = new StreamWriter(testFileName))
				{
					WriteHeaderLine(writer);
					writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction2\",\"20030606\",\"20030705\",\"USD\",\"210.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction3\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction4\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction5\",\"20040219\",\"20040229\",\"AUD\",\"500.11\",\"500.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction7\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction8\",\"20040206\",\"20040214\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));

					// These transactions should fail.
					writer.WriteLine(string.Format("\"{0}\",\"BadTransaction\",\"35022004\",\"14032004\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"OrgCode\",\"InvalidOrg\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction6\",\"20040210\",\"20040331\",\"\",\"0.00\",\"400.44\"", Organisation.OH_Code));

					//Branch & Department Lines
					writer.WriteLine(string.Format("\"{0}\",\"Transaction9\",\"20040219\",\"20040229\",\"AUD\",\"500.11\",\"500.11\",\"{1}\",\"{2}\"", Organisation.OH_Code, currCompanyBranch.GB_Code, ""));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction10\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\",\"{1}\",\"{2}\"", Organisation.OH_Code, currCompanyBranch.GB_Code, TestDeparment.GE_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction11\",\"20040206\",\"20040214\",\"USD\",\"200.22\",\"400.44\",\"{1}\",\"{2}\"", Organisation.OH_Code, "", TestDeparment.GE_Code));
				}

				var converter = new Converter();
				var createdJournals = converter.ImportFile(testFileName, true, ZDateTime.Now);
				AssertEquals(10, createdJournals.Length);

				Assert(createdJournals[0] is ARJournal);
				Assert(createdJournals[1] is ARJournal);

				var j1 = (ARJournal)createdJournals[0];
				AssertEquals(LedgerTypes.AccountsReceivable, j1.AH_Ledger);
				AssertEquals(TransactionTypes.Journal, j1.AH_TransactionType);
				AssertEquals(100.11m, j1.AH_LocalExTaxAmount);
				AssertEquals(100.11m, j1.AH_OSTotalAmount);
				AssertEquals(1m, j1.AH_ExchangeRate);
				AssertEquals("Transaction1", j1.AH_Desc);
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, j1.AH_RX_NKTransactionCurrency);
				AssertEquals("Due Date", new ZDateTime(2003, 7, 5).Date, j1.AH_DueDate.Date);

				//new PeriodManagement.PeriodCalculator().GetPeriodFromDate(J1.AH_DueDate.Date);
				// TODO: Need to fix this soon

				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j1.AH_GB);
				AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, j1.AH_GE);
				AssertEquals("GL Account", creator.GLHeader1.PK, j1.AH_AG);
				AssertEquals("Oustanding Amount", 100.11m, j1.AH_OutstandingAmount);

				var j2 = (ARJournal)createdJournals[1];
				AssertEquals(LedgerTypes.AccountsReceivable, j2.AH_Ledger);
				AssertEquals(TransactionTypes.Journal, j2.AH_TransactionType);
				AssertEquals(400.44m, j2.AH_LocalExTaxAmount);
				AssertEquals(210.22m, j2.AH_OSTotalAmount);
				AssertEquals(0.524973m, j2.AH_ExchangeRate);
				AssertEquals("Transaction2", j2.AH_Desc);
				AssertEquals(USD.RX_Code, j2.AH_RX_NKTransactionCurrency);
				AssertEquals("Due Date", new ZDateTime(2003, 7, 5).Date, j2.AH_DueDate.Date);

				//new PeriodManagement.PeriodCalculator().GetPeriodFromDate(J2.AH_DueDate.Date);
				// TODO: Need to fix this soon

				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j2.AH_GB);
				AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, j2.AH_GE);
				AssertEquals("GL Account", creator.GLHeader1.PK, j2.AH_AG);
				AssertEquals("Oustanding Amount", 400.44m, j2.AH_OutstandingAmount);

				var j3 = (ARJournal)createdJournals[7];
				AssertEquals("Branch", currCompanyBranch.PK, j3.AH_GB);
				AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, j3.AH_GE);
				AssertEquals("Oustanding Amount", 500.11m, j3.AH_OutstandingAmount);

				var j4 = (ARJournal)createdJournals[8];
				AssertEquals("Branch", currCompanyBranch.PK, j4.AH_GB);
				AssertEquals("Department", TestDeparment.PK, j4.AH_GE);
				AssertEquals("Oustanding Amount", 400.44m, j4.AH_OutstandingAmount);

				var j5 = (ARJournal)createdJournals[9];
				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j5.AH_GB);
				AssertEquals("Department", TestDeparment.PK, j5.AH_GE);
				AssertEquals("Oustanding Amount", 400.44m, j5.AH_OutstandingAmount);
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		[TestDate(2003, 06, 05)]
		public void TestImportARDataWithReciprocalCountry()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, creator.GLHeader1.PK.ToGuid());

			var testFileName = Env.TempPath + "test.csv";

			var code = "TestCode";
			Organisation.OH_Code = code;
			Organisation.OH_IsDebtor = true;
			Factory.Save();

			try
			{
				using (var writer = new StreamWriter(testFileName))
				{
					WriteHeaderLine(writer);
					writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction2\",\"20030606\",\"20030705\",\"USD\",\"210.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction3\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction4\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction5\",\"20040219\",\"20040229\",\"AUD\",\"500.11\",\"500.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction7\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction8\",\"20040206\",\"20040214\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));

					// These transactions should fail.
					writer.WriteLine(string.Format("\"{0}\",\"BadTransaction\",\"35022004\",\"14032004\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"OrgCode\",\"InvalidOrg\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction6\",\"20040210\",\"20040331\",\"\",\"0.00\",\"400.44\"", Organisation.OH_Code));

					//Branch & Department Lines
					writer.WriteLine(string.Format("\"{0}\",\"Transaction9\",\"20040219\",\"20040229\",\"AUD\",\"500.11\",\"500.11\",\"{1}\",\"{2}\"", Organisation.OH_Code, GlbBranch.CurrentBranch.GB_Code, ""));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction10\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\",\"{1}\",\"{2}\"", Organisation.OH_Code, GlbBranch.CurrentBranch.GB_Code, TestDeparment.GE_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction11\",\"20040206\",\"20040214\",\"USD\",\"200.22\",\"400.44\",\"{1}\",\"{2}\"", Organisation.OH_Code, "", TestDeparment.GE_Code));
				}

				var converter = new Converter();
				var createdJournals = converter.ImportFile(testFileName, true, ZDateTime.Now);
				AssertEquals(10, createdJournals.Length);

				Assert(createdJournals[0] is ARJournal);
				Assert(createdJournals[1] is ARJournal);

				var j1 = (ARJournal)createdJournals[0];
				AssertEquals(LedgerTypes.AccountsReceivable, j1.AH_Ledger);
				AssertEquals(TransactionTypes.Journal, j1.AH_TransactionType);
				AssertEquals(100.11m, j1.AH_LocalExTaxAmount);
				AssertEquals(100.11m, j1.AH_OSTotalAmount);
				AssertEquals(1m, j1.AH_ExchangeRate);
				AssertEquals("Transaction1", j1.AH_Desc);
				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, j1.AH_RX_NKTransactionCurrency);
				AssertEquals("Due Date", new ZDateTime(2003, 7, 5).Date, j1.AH_DueDate.Date);

				//new PeriodManagement.PeriodCalculator().GetPeriodFromDate(J1.AH_DueDate.Date);
				// TODO: Need to fix this soon

				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j1.AH_GB);
				AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, j1.AH_GE);
				AssertEquals("GL Account", creator.GLHeader1.PK, j1.AH_AG);
				AssertEquals("Oustanding Amount", 100.11m, j1.AH_OutstandingAmount);

				var j2 = (ARJournal)createdJournals[1];
				AssertEquals(LedgerTypes.AccountsReceivable, j2.AH_Ledger);
				AssertEquals(TransactionTypes.Journal, j2.AH_TransactionType);
				AssertEquals(400.44m, j2.AH_LocalExTaxAmount);
				AssertEquals(210.22m, j2.AH_OSTotalAmount);
				AssertEquals(1.904862m, j2.AH_ExchangeRate);
				AssertEquals("Transaction2", j2.AH_Desc);
				AssertEquals(USD.RX_Code, j2.AH_RX_NKTransactionCurrency);
				AssertEquals("Due Date", new ZDateTime(2003, 7, 5).Date, j2.AH_DueDate.Date);

				//new PeriodManagement.PeriodCalculator().GetPeriodFromDate(J2.AH_DueDate.Date);
				// TODO: Need to fix this soon

				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j2.AH_GB);
				AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, j2.AH_GE);
				AssertEquals("GL Account", creator.GLHeader1.PK, j2.AH_AG);
				AssertEquals("Oustanding Amount", 400.44m, j2.AH_OutstandingAmount);

				var j3 = (ARJournal)createdJournals[7];
				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j3.AH_GB);
				AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, j3.AH_GE);
				AssertEquals("Oustanding Amount", 500.11m, j3.AH_OutstandingAmount);

				var j4 = (ARJournal)createdJournals[8];
				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j4.AH_GB);
				AssertEquals("Department", TestDeparment.PK, j4.AH_GE);
				AssertEquals("Oustanding Amount", 400.44m, j4.AH_OutstandingAmount);

				var j5 = (ARJournal)createdJournals[9];
				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j5.AH_GB);
				AssertEquals("Department", TestDeparment.PK, j5.AH_GE);
				AssertEquals("Oustanding Amount", 400.44m, j5.AH_OutstandingAmount);
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		[TestDate(2003, 06, 05)]
		public void TestImportAPData()
		{
			var creator = new TestObjectCreator(Factory);
			var currCompanyBranch = GetNewBranchInDB(GlbCompany.CurrentCompany);
			AccountingConfigurationRegistry.Instance.APJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, creator.GLHeader1.PK.ToGuid());

			var testFileName = Env.TempPath + "test.csv";
			var code = "TestCode";
			Organisation.OH_Code = code;
			Organisation.OH_IsCreditor = true;
			Factory.Save();

			using (var writer = new StreamWriter(testFileName))
			{
				WriteHeaderLine(writer);
				writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
				writer.WriteLine(string.Format("\"{0}\",\"Transaction2\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
				//Branch & Department Lines
				writer.WriteLine(string.Format("\"{0}\",\"Transaction3\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\",\"{1}\",\"{2}\"", Organisation.OH_Code, currCompanyBranch.GB_Code, ""));
				writer.WriteLine(string.Format("\"{0}\",\"Transaction4\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\",\"{1}\",\"{2}\"", Organisation.OH_Code, currCompanyBranch.GB_Code, TestDeparment.GE_Code));
				writer.WriteLine(string.Format("\"{0}\",\"Transaction5\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\",\"{1}\",\"{2}\"", Organisation.OH_Code, "", TestDeparment.GE_Code));
				//
				writer.WriteLine(string.Format("\"{0}\",\"Transaction6\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", Organisation.OH_Code));
				writer.WriteLine(string.Format("\"{0}\",\"Transaction7\",\"20030606\",\"20030705\",\"USD\",\"200.22\",\"400.44\"", Organisation.OH_Code));
			}

			var converter = new Converter();
			var createdJournals = converter.ImportFile(testFileName, false, ZDateTime.Now);
			AssertEquals(7, createdJournals.Length);

			Assert(createdJournals[0] is APJournal);
			Assert(createdJournals[1] is APJournal);

			var j1 = (APJournal)createdJournals[0];
			AssertEquals(LedgerTypes.AccountsPayable, j1.AH_Ledger);
			AssertEquals(TransactionTypes.Journal, j1.AH_TransactionType);
			AssertEquals(100.11m, j1.AH_LocalExTaxAmount);
			AssertEquals(100.11m, j1.AH_OSTotalAmount);

			AssertEquals(-100.11m, j1.AH_InvoiceAmount);
			AssertEquals(-100.11m, j1.AH_OSTotal);

			AssertEquals(1m, j1.AH_ExchangeRate);
			AssertEquals("Transaction1", j1.AH_Desc);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, j1.AH_RX_NKTransactionCurrency);
			AssertEquals("Due Date", new ZDateTime(2003, 7, 5).Date, j1.AH_DueDate.Date);

			//new PeriodManagement.PeriodCalculator().GetPeriodFromDate(J1.AH_DueDate.Date);
			// TODO: Need to fix this soon

			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j1.AH_GB);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, j1.AH_GE);
			AssertEquals("GL Account", creator.GLHeader1.PK, j1.AH_AG);
			AssertEquals("Oustanding Amount", -100.11m, j1.AH_OutstandingAmount);

			var j2 = (APJournal)createdJournals[1];
			AssertEquals(LedgerTypes.AccountsPayable, j2.AH_Ledger);
			AssertEquals(TransactionTypes.Journal, j2.AH_TransactionType);
			AssertEquals(400.44m, j2.AH_LocalExTaxAmount);
			AssertEquals(200.22m, j2.AH_OSTotalAmount);
			AssertEquals(0.5m, j2.AH_ExchangeRate);
			AssertEquals("Transaction2", j2.AH_Desc);
			AssertEquals(USD.RX_Code, j2.AH_RX_NKTransactionCurrency);
			AssertEquals("Due Date", new ZDateTime(2003, 7, 5).Date, j2.AH_DueDate.Date);

			//new PeriodManagement.PeriodCalculator().GetPeriodFromDate(J2.AH_DueDate.Date);
			// TODO: Need to fix this soon

			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j2.AH_GB);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, j2.AH_GE);
			AssertEquals("GL Account", creator.GLHeader1.PK, j2.AH_AG);
			AssertEquals("Oustanding Amount", -400.44m, j2.AH_OutstandingAmount);

			// OutstandingAmount should not be set from DataConverters, it is set automatically 
			// in Accounting when you set LocalExTaxAmount. 
			// OutstandingAmount should be the opposite of LocalExTax and OSTotal Amounts as it 
			// is a DB Field and is not affected by the multiplier.

			var j3 = (APJournal)createdJournals[2];
			AssertEquals(LedgerTypes.AccountsPayable, j3.AH_Ledger);
			AssertEquals(TransactionTypes.Journal, j3.AH_TransactionType);
			AssertEquals(100.11m, j3.AH_LocalExTaxAmount);
			AssertEquals(100.11m, j3.AH_OSTotalAmount);
			AssertEquals(1m, j3.AH_ExchangeRate);
			AssertEquals("Transaction3", j3.AH_Desc);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, j3.AH_RX_NKTransactionCurrency);
			AssertEquals("Due Date", new ZDateTime(2003, 7, 5).Date, j3.AH_DueDate.Date);
			AssertEquals("Branch", currCompanyBranch.PK, j3.AH_GB);
			AssertEquals("Department", GlbDepartment.CurrentDepartment.PK, j3.AH_GE);
			AssertEquals("GL Account", creator.GLHeader1.PK, j3.AH_AG);
			AssertEquals("InvoiceAmount", -100.11m, j3.AH_OutstandingAmount);
			AssertEquals("Oustanding Amount", -100.11m, j3.AH_OutstandingAmount);

			var j4 = (APJournal)createdJournals[3];
			AssertEquals("Branch", currCompanyBranch.PK, j4.AH_GB);
			AssertEquals("Department", TestDeparment.PK, j4.AH_GE);

			var j5 = (APJournal)createdJournals[4];
			AssertEquals("Branch", GlbBranch.CurrentBranch.PK, j5.AH_GB);
			AssertEquals("Department", TestDeparment.PK, j5.AH_GE);
			File.Delete(testFileName);
		}

		[TestDate(2003, 06, 05)]
		public void TestTransactionNumberOrder()
		{
			var testFileName = Env.TempPath + "test.csv";
			var code = "TestCode";
			Organisation.OH_Code = code;
			Organisation.OH_IsCreditor = true;
			Factory.Save();

			try
			{
				using (var writer = new StreamWriter(testFileName))
				{
					WriteHeaderLine(writer);

					writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.00\",\"100.00\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction2\",\"20030605\",\"20030705\",\"AUD\",\"100.00\",\"100.00\"", Organisation.OH_Code));
					writer.WriteLine(string.Format("\"{0}\",\"Transaction3\",\"20030605\",\"20030705\",\"AUD\",\"100.00\",\"100.00\"", Organisation.OH_Code));
				}

				var converter = new Converter();
				var createdJournals = converter.ImportFile(testFileName, false, ZDateTime.Now);
				AssertEquals(3, createdJournals.Length);

				var nextJournalNumber = Env.NumberFountains.APJournalNo.GetTodaysPeriodFountain().PeekPreliminary(Factory);

				converter.SaveChanges();

				var j1 = (APJournal)createdJournals[0];
				var j2 = (APJournal)createdJournals[1];
				var j3 = (APJournal)createdJournals[2];

				j1 = Factory.Load<APJournal>(j1.PK);
				j2 = Factory.Load<APJournal>(j2.PK);
				j3 = Factory.Load<APJournal>(j3.PK);

				AssertEquals("Should be " + nextJournalNumber, nextJournalNumber, ZInt.Parse(j1.AH_TransactionNum));

				nextJournalNumber++;
				AssertEquals("Should be " + nextJournalNumber, nextJournalNumber, ZInt.Parse(j2.AH_TransactionNum));

				nextJournalNumber++;
				AssertEquals("Should be " + nextJournalNumber, nextJournalNumber, ZInt.Parse(j3.AH_TransactionNum));
			}
			finally
			{
				File.Delete(testFileName);
			}
		}

		public void TestClearingAccountValidationAR()
		{
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			TestClearingAccountValidationCore(true);
		}

		public void TestClearingAccountValidationAP()
		{
			AccountingConfigurationRegistry.Instance.APJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			TestClearingAccountValidationCore(false);
		}

		[TestDate(2003, 06, 05)]
		public void TestConverterDoesNotAddJournalImporterFactoryToBusinessObjectFactoryListWhenImporterHasErrors()
		{
			var testARClearingAccount = Factory.NewWithValidTestData<AccGLHeader>();
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testARClearingAccount.PK.ToGuid());

			var fileWithInvalidData = Env.TempPath + "incorrectfile.csv";
			var fileWithValidData = Env.TempPath + "correctfile.csv";

			Organisation.OH_Code = "TestOrg";
			Organisation.OH_IsDebtor = true;
			Factory.Save();

			try
			{
				CreateTestFileContent(fileWithInvalidData, Organisation.OH_Code, true);

				var converter = new Converter();
				var createdJournals = converter.ImportFile(fileWithInvalidData, true, ZDateTime.Now);
				AssertEquals(1, createdJournals.Length);
				AssertEquals(1, converter.Errors.Count);
				AssertContains("As Currency and Local Currency are same,  Invoice Amount and Foreign Amount must be equal.", converter.Errors[0]);
				AssertEquals("Importer Factory should not be added because there are errors", 0, converter.BusinessObjectFactoryList.Count);
				converter.ClearErrors(); //Once import is completed, errors are cleaned up by event handler method in the main form.

				CreateTestFileContent(fileWithValidData, Organisation.OH_Code, false);

				createdJournals = converter.ImportFile(fileWithValidData, true, ZDateTime.Now);
				AssertEquals(2, createdJournals.Length);
				AssertEquals(0, converter.Errors.Count);
				AssertEquals("Importer Factory should be added because there are no errors", 1, converter.BusinessObjectFactoryList.Count);

				converter.SaveChanges();

				AssertJournalNotDuplicatedInDB("Transaction1");
				AssertJournalNotDuplicatedInDB("Transaction2");
			}
			finally
			{
				File.Delete(fileWithInvalidData);
				File.Delete(fileWithValidData);
			}
		}

		public void TestTotalAmountIsCorrectWhenLedgerIsAP()
		{
			Organisation.OH_Code = "TestCode";
			Organisation.OH_IsCreditor = true;
			Factory.Save();
			AssertTotalAmountIsCorrect(false);
		}

		public void TestTotalAmountIsCorrectWhenLedgerIsAR()
		{
			Organisation.OH_Code = "TestCode";
			Organisation.OH_IsDebtor = true;
			Factory.Save();
			AssertTotalAmountIsCorrect(true);
		}

		void AssertTotalAmountIsCorrect(bool isDebtor)
		{
			var totalAmount = 0M;
			var converter = new Converter();
			converter.ProgressChanged += Converter_ProgressChanged;

			ImportData(100, 200);
			AssertEquals("Total Amount 1", 300M, totalAmount);

			ImportData(100, -200);
			AssertEquals("Total Amount 2", -100M, totalAmount);

			ImportData(-100, 200);
			AssertEquals("Total Amount 3", 100M, totalAmount);

			ImportData(-100, -200);
			AssertEquals("Total Amount 4", -300M, totalAmount);

			void ImportData(decimal amount1, decimal amount2)
			{
				var fileName = Env.TempPath + "test.csv";
				try
				{
					using (var writer = new StreamWriter(fileName))
					{
						WriteHeaderLine(writer);
						writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20250605\",\"20250605\",\"AUD\",\"{1}\",\"{1}\"", Organisation.OH_Code, amount1));
						writer.WriteLine(string.Format("\"{0}\",\"Transaction2\",\"20250605\",\"20250605\",\"AUD\",\"{1}\",\"{1}\"", Organisation.OH_Code, amount2));
					}
					converter.ImportFile(fileName, isDebtor, ZDateTime.Now);
				}
				finally
				{
					File.Delete(fileName);
				}
			}

			void Converter_ProgressChanged(object sender, EventArgs e)
			{
				var data = (ConverterData)sender;
				totalAmount = data.TotalAmountImported;
			}
		}

		#region Helpers

		void CreateTestFileContent(string fileName, string organisationCode, bool shouldIncludeInvalidLine)
		{
			using (var writer = new StreamWriter(fileName))
			{
				WriteHeaderLine(writer);
				writer.WriteLine(string.Format("\"{0}\",\"Transaction1\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"{1}\"", organisationCode, shouldIncludeInvalidLine ? "0" : "100.11"));
				writer.WriteLine(string.Format("\"{0}\",\"Transaction2\",\"20030605\",\"20030705\",\"AUD\",\"100.11\",\"100.11\"", organisationCode));
			}
		}

		void AssertJournalNotDuplicatedInDB(ZString journalDescription)
		{
			var sqlQuery = string.Format(@"SELECT COUNT(*) FROM dbo.AccTransactionHeader WHERE AH_Ledger = 'AR' and AH_TransactionType = 'JNL' and 
 AH_Desc = '{0}'", journalDescription);
			using (var command = Db.Connection.Command(sqlQuery))
			{
				var result = command.ExecuteScalar();
				AssertNotNull(result);
				AssertEquals(1, (int)result);
			}
		}

		#endregion

		void TestClearingAccountValidationCore(bool isDebtor)
		{
			var testFileName = Env.TempPath + "test.csv";

			using (var writer = new StreamWriter(testFileName))
			{
				WriteHeaderLine(writer);
			}
			// debtors
			var converter = new Converter();
			converter.ImportFile(testFileName, isDebtor, ZDateTime.Now);

			AssertEquals("Error should read as follows: ", "The " + converter.Ledger
				+ " Journal Clearing Account cannot be empty.  Please enter GL Account for this in the registry", converter.Errors[0]);

			File.Delete(testFileName);
		}

		protected RefCurrency USD;
		protected OrgHeader Organisation;
		protected GlbDepartment TestDeparment;
		protected StringCollection ErrorReceived;

		protected override void SetUp()
		{
			base.SetUp();
			USD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			Organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			TestDeparment = Factory.LoadTop1<GlbDepartment>(new ZQuery());

			Helper.SetupPeriods();
		}

		GlbBranch GetNewBranchInDB(GlbCompany company)
		{
			var newFactory = new BusinessObjectFactory();
			var branch = newFactory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			newFactory.Save();

			return branch;
		}

		void Converter_OnError(object sender, EventArgs e)
		{
			if (ErrorReceived == null)
			{
				ErrorReceived = new StringCollection();
			}

			ErrorReceived.Add(sender.ToString());
		}
	}
}
