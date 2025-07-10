using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.XmlMapping;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile.Testing
{
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class TxnHeaderFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestImportInvoiceWithMultipleSubAccounts()
		{
			var txnHeaderCollection = new Xsd.TxnHeaderCollection();
			var notifications = new NotificationBuffer();
			var converter = new TxnHeaderFlatFileConverter(notifications, Factory);
			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\InvoiceWithMultipleSubAccounts.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(1, txnHeaderCollection.Count);
			AssertEquals(4, txnHeaderCollection[0].TxnLines.Count);

			AssertEquals("2010.00.00", txnHeaderCollection[0].TxnLines[0].GLAccount);
			AssertEquals(2, txnHeaderCollection[0].TxnLines[0].SubAccounts.Count);
			var importedSubAccount1 = txnHeaderCollection[0].TxnLines[0].SubAccounts[0];
			AssertEquals("ABIGAS", importedSubAccount1.Code);
			AssertEquals(Constants.SubAccountType.Organization, importedSubAccount1.Type.Code);
			var importedSubAccount2 = txnHeaderCollection[0].TxnLines[0].SubAccounts[1];
			AssertEquals("TST", importedSubAccount2.Code);
			AssertEquals(Constants.SubAccountType.StaffAndResources, importedSubAccount2.Type.Code);

			AssertEquals("2020.00.00", txnHeaderCollection[0].TxnLines[1].GLAccount);
			AssertEquals(1, txnHeaderCollection[0].TxnLines[1].SubAccounts.Count);
			var importedSubAccount3 = txnHeaderCollection[0].TxnLines[1].SubAccounts[0];
			Assert(string.IsNullOrEmpty(importedSubAccount3.Code));
			AssertEquals(Constants.SubAccountType.SalesGroup, importedSubAccount3.Type.Code);

			AssertEquals("2020.10.00", txnHeaderCollection[0].TxnLines[2].GLAccount);
			AssertEquals(1, txnHeaderCollection[0].TxnLines[2].SubAccounts.Count);
			var importedSubAccount4 = txnHeaderCollection[0].TxnLines[2].SubAccounts[0];
			Assert(string.IsNullOrEmpty(importedSubAccount4.Code));
			AssertEquals(Constants.SubAccountType.StaffGroup, importedSubAccount4.Type.Code);

			AssertEquals("2020.20.00", txnHeaderCollection[0].TxnLines[3].GLAccount);
			AssertEquals(0, txnHeaderCollection[0].TxnLines[3].SubAccounts.Count);

			AssertEquals(false, notifications.HasErrors);
		}

		public void TestImportInvoiceWithNoSubAccountType()
		{
			var txnHeaderCollection = new Xsd.TxnHeaderCollection();
			var notifications = new NotificationBuffer();
			var converter = new TxnHeaderFlatFileConverter(notifications, Factory);
			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\InvoiceWithMultipleSubAccountsButInvalid.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(1, txnHeaderCollection.Count);
			AssertEquals(2, txnHeaderCollection[0].TxnLines.Count);
			AssertEquals(0, txnHeaderCollection[0].TxnLines[0].SubAccounts.Count);
			AssertEquals(0, txnHeaderCollection[0].TxnLines[1].SubAccounts.Count);

			AssertEquals(true, notifications.HasErrors);
			AssertEquals("2 sub accounts does not have relative line", @"Error: Data type conversion error (There is a sub account which does not belong to any transaction line in this CSV file.)
Error: Data type conversion error (There is a sub account which does not belong to any transaction line in this CSV file.)", notifications.AsString.Trim());
		}

		public void TestImport_ARInvoice()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARInvoice.csv";
			var txnHeader = CheckCommonFunctionalityForInvoicingBase(testFilePath, "AR", "INV");
			AssertEquals("Override system exchange rate", true, txnHeader.OverrideSystemExchangeRate);
			AssertEquals("Header Amount", 200M, txnHeader.LocalInvoiceAmtExclTax.Value);
			AssertEquals("Line 1 Amount", 200M, txnHeader.TxnLines[0].LocalInvoiceAmtExclTax.Value);
			AssertEquals("Line 2 Amount", -200M, txnHeader.TxnLines[1].LocalInvoiceAmtExclTax.Value);
			AssertEquals("Line 3 Amount", 200M, txnHeader.TxnLines[2].LocalInvoiceAmtExclTax.Value);
		}

		public void TestImport_ARInvoiceDisbursement()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARInvoiceDisbursement.csv";
			CheckCommonFunctionalityForInvoicingBase(testFilePath, "AR", "INV", true);
		}

		public void TestImportWhenInvalidTypesAreGiven()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\TransactionWithInvalidTypes.csv";

			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			NotificationBuffer buffer = new NotificationBuffer();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(buffer, Factory);

			using (StreamReader reader = new StreamReader(testFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);

			ZString bufferContents = buffer.AsString;

			ZString expectedMessage1 = $"The Ledger Type you supplied (XX) is not recognized by {BrandingFactory.Instance.ProductName}. " + System.Environment.NewLine;
			expectedMessage1 += "Please use one of the following Ledger Types: AR, AP";

			ZString expectedMessage2 = $"The Transaction Type you supplied (XXX) is not recognized by {BrandingFactory.Instance.ProductName}. " + System.Environment.NewLine;
			expectedMessage2 += "Please use one of the following: INV, CRD, ADJ";

			ZString expectedMessage3 = $"The Job Type you supplied (XXX) is not recognized by {BrandingFactory.Instance.ProductName}. " + System.Environment.NewLine;
			expectedMessage3 += "Please use one of the following Job Types: ";
			foreach (var mapping in TransactionLineConsolOrJobTypeXmlMapping.Instance)
			{
				expectedMessage3 += mapping.ExternalCode + ", ";
			}
			expectedMessage3 = expectedMessage3.TrimEnd(' ', ',');

			ZQuery docTypeQuery = new DocTypeCategoryQuery(Factory, Core.Constants.ReferenceTypes.Accounting);
			ZQuery visibleQuery = new ZQuery(RefDocTypeSchema.RT_IsActive, ZBool.True);
			docTypeQuery.AddToFilter(visibleQuery, JoinCondition.And);
			var docTypes = new RefDocTypeCollection(Factory, docTypeQuery);
			docTypes.ApplySort(RefDocTypeSchema.RT_DocType.Name, ListSortDirection.Ascending);
			string docTypesAsString = new ZStringBuilder(docTypes.Select(docType => docType.RT_DocType + ", ")).ToString().TrimEnd(' ', ',');

			ZString expectedMessage4 = "The Document Type you supplied (XXX) is not recognized by this import process. " + System.Environment.NewLine;
			expectedMessage4 += "You can only use Document Types with a category of 'ACC - Accounting' or 'ALL'. ";
			expectedMessage4 += "Please use one of the following Document Types: " + docTypesAsString;

			AssertContains("Buffer should contain error message", expectedMessage1, bufferContents);
			AssertContains("Buffer should contain error message", expectedMessage2, bufferContents);
			AssertContains("Buffer should contain error message", expectedMessage3, bufferContents);
			AssertContains("Buffer should contain error message", expectedMessage4, bufferContents);
		}

		public void TestARInvoice_CSLJobType_ValidCase()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceForCSLJobType_valid.csv";

			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			NotificationBuffer buffer = new NotificationBuffer();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(buffer, Factory);

			using (StreamReader reader = new StreamReader(testFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);

			AssertEquals("There should be 1 line", 1, txnHeaderCollection[0].TxnLines.Count);

			AssertEquals("Job type is CSL", Xsd.TxnLineConsolOrJobType.CSL, txnHeaderCollection[0].TxnLines[0].ConsolOrJobType);
			AssertEquals("Apportionment Method should be 'SHP' as specified in the CSV file.", Xsd.TxnLineConsolApportionmentMethod.TEU, txnHeaderCollection[0].TxnLines[0].ConsolApportionmentMethod);
		}

		public void TestARInvoice_CSLJobType_DefaultCase()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceForCSLJobType_default.csv";

			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			NotificationBuffer buffer = new NotificationBuffer();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(buffer, Factory);

			using (StreamReader reader = new StreamReader(testFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);

			AssertEquals("There should be 1 line", 1, txnHeaderCollection[0].TxnLines.Count);

			AssertEquals("Job type is CSL", Xsd.TxnLineConsolOrJobType.CSL, txnHeaderCollection[0].TxnLines[0].ConsolOrJobType);
			AssertEquals("Apportionment Method should not be specified.", false, txnHeaderCollection[0].TxnLines[0].ConsolApportionmentMethodSpecified);
		}

		public void TestARInvoice_CSLJobType_InvalidCase()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceForCSLJobType_invalid.csv";

			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			NotificationBuffer buffer = new NotificationBuffer();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(buffer, Factory);

			using (StreamReader reader = new StreamReader(testFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);

			ZString bufferContents = buffer.AsString;

			AssertEquals("There should be 1 line", 1, txnHeaderCollection[0].TxnLines.Count);

			AssertEquals("Job type is CSL", Xsd.TxnLineConsolOrJobType.CSL, txnHeaderCollection[0].TxnLines[0].ConsolOrJobType);

			ZString expectedMessage = "The Apportionment Method you supplied (MAN) is not valid.";
			AssertContains("Buffer should contain error message", expectedMessage, bufferContents);
		}

		public void TestImport_ARCreditNote()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARCreditNote.csv";
			CheckCommonFunctionalityForInvoicingBase(testFilePath, "AR", "CRD");
		}

		public void TestImport_ARAdjustmentNote()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARAdjustmentNote.csv";
			CheckCommonFunctionalityForInvoicingBase(testFilePath, "AR", "ADJ");
		}

		public void TestImport_APInvoice()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoice.csv";
			var txnHeader = CheckCommonFunctionalityForInvoicingBase(testFilePath, "AP", "INV");
			AssertEquals("Header Amount", -200M, txnHeader.LocalInvoiceAmtExclTax.Value);
			AssertEquals("Line 1 Amount", -200M, txnHeader.TxnLines[0].LocalInvoiceAmtExclTax.Value);
			AssertEquals("Line 2 Amount", 200M, txnHeader.TxnLines[1].LocalInvoiceAmtExclTax.Value);
			AssertEquals("Line 3 Amount", -200M, txnHeader.TxnLines[2].LocalInvoiceAmtExclTax.Value);
			AssertEquals("Override system exchange rate", true, txnHeader.OverrideSystemExchangeRate);
		}

		public void TestImport_APCreditNote()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APCreditNote.csv";
			CheckCommonFunctionalityForInvoicingBase(testFilePath, "AP", "CRD");
		}

		public void TestImport_APAdjustmentNote()
		{
			ZString testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APAdjustmentNote.csv";
			CheckCommonFunctionalityForInvoicingBase(testFilePath, "AP", "ADJ");
		}

		Xsd.TxnHeader CheckCommonFunctionalityForInvoicingBase(ZString testFilePath, ZString ledger, ZString txnType)
		{
			return CheckCommonFunctionalityForInvoicingBase(testFilePath, ledger, txnType, false);
		}

		Xsd.TxnHeader CheckCommonFunctionalityForInvoicingBase(ZString testFilePath, ZString ledger, ZString txnType, bool isDisbursement)
		{
			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(null, Factory);

			using (StreamReader reader = new StreamReader(testFilePath))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);
			Xsd.TxnHeader txnHeader = txnHeaderCollection[0];
			AssertEquals("Transaction Ledger", ledger, txnHeader.Ledger.ToString());
			AssertEquals("Transaction Type", txnType, txnHeader.TxnType.ToString());
			AssertEquals("Debtor Code", "EAGDAT_AU", txnHeader.DebtorOrCreditor.EDICode.ToString());
			AssertEquals("Branch Code", "BR1", txnHeader.Branch.ToString());
			AssertEquals("Department Code", "DP1", txnHeader.Department.ToString());
			AssertEquals("Currency Code", "AUD", txnHeader.OsInvoiceAmtInclTax.CurrencyCode.ToString());
			AssertEquals("Disbursement Flag", isDisbursement, txnHeader.DisbursementFlag);
			AssertEquals("File name", "Test.doc", txnHeader.Attachments[0].FileName);
			AssertEquals("File path", "C:\\Test.doc", txnHeader.Attachments[0].FilePath); // This is not hard coded path this is just string for testing
			AssertEquals("Document Type", "MSC", txnHeader.Attachments[0].DocumentType);
			AssertEquals("Due Date", new ZDateTime(2005, 09, 01), txnHeader.DueDate);
			AssertEquals("Invoice date", new ZDateTime(2005, 09, 01), txnHeader.InvoiceDate);
			AssertEquals("Invoice date", new ZDateTime(2005, 09, 01), txnHeader.PostDate);

			ZDecimal multiplier = TxnHeaderMapper.MultiplierForImportAndExport(TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader));

			if (multiplier == 1)
			{
				AssertEquals("Header Amount", 100M, txnHeader.OsInvoiceAmtInclTax.Value);
				AssertEquals("Line 1 Amount", 100M, txnHeader.TxnLines[0].OsInvoiceAmtExclTax.Value);
				AssertEquals("Line 2 Amount", -100M, txnHeader.TxnLines[1].OsInvoiceAmtExclTax.Value);
				AssertEquals("Line 3 Amount", 100M, txnHeader.TxnLines[2].OsInvoiceAmtExclTax.Value);
			}
			else
			{
				AssertEquals("Header Amount", -100M, txnHeader.OsInvoiceAmtInclTax.Value);
				AssertEquals("Line 1 Amount", -100M, txnHeader.TxnLines[0].OsInvoiceAmtExclTax.Value);
				AssertEquals("Line 2 Amount", 100M, txnHeader.TxnLines[1].OsInvoiceAmtExclTax.Value);
				AssertEquals("Line 3 Amount", -100M, txnHeader.TxnLines[2].OsInvoiceAmtExclTax.Value);
			}

			AssertEquals("GL Account Code", "1010.20.11", txnHeader.TxnLines[0].GLAccount);
			AssertEquals("Charge Code", "ChargeCode1", txnHeader.TxnLines[1].ChargeCode);
			AssertEquals("GL Account Code", "1010.20.13", txnHeader.TxnLines[2].GLAccount);
			return txnHeader;
		}

		public void TestImport_AmountIncTaxIsSpecified()
		{
			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(null, Factory);

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARInvoiceAmountInclTax.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);
			Xsd.TxnHeader txnHeader = txnHeaderCollection[0];
			AssertEquals("Transaction Ledger", "AR", txnHeader.Ledger.ToString());
			AssertEquals("Transaction Type", "INV", txnHeader.TxnType.ToString());

			AssertEquals("Line 1 Ex Tax Amount", 100M, txnHeader.TxnLines[0].OsInvoiceAmtExclTax.Value);
			AssertEquals("Line 2 Ex Tax Amount", 100M, txnHeader.TxnLines[1].OsInvoiceAmtExclTax.Value);
			AssertEquals("Line 3 Ex Tax Amount", 100M, txnHeader.TxnLines[2].OsInvoiceAmtExclTax.Value);
			AssertEquals("Header Ex Tax Amount", 300M, txnHeader.OsInvoiceAmtExclTax.Value);

			AssertEquals("Line 1 Tax Amount", 10M, txnHeader.TxnLines[0].OsTaxAmount.Value);
			AssertEquals("Line 2 Tax Amount", 10M, txnHeader.TxnLines[1].OsTaxAmount.Value);
			AssertEquals("Line 3 Tax Amount", 10M, txnHeader.TxnLines[2].OsTaxAmount.Value);
			AssertEquals("Header Tax Amount", 30M, txnHeader.OsTaxAmount.Value);

			AssertEquals("Line 1 Incl Tax Amount", 110M, txnHeader.TxnLines[0].OsInvoiceAmtInclTax.Value);
			AssertEquals("Line 2 Incl Tax Amount", 110M, txnHeader.TxnLines[1].OsInvoiceAmtInclTax.Value);
			AssertEquals("Line 3 Incl Tax Amount", 110M, txnHeader.TxnLines[2].OsInvoiceAmtInclTax.Value);
			AssertEquals("Header Incl Tax Amount", 330M, txnHeader.OsInvoiceAmtInclTax.Value);
		}

		public void TestImport_AmountIncTaxIsSpecifiedAndNoTaxAmount()
		{
			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\ARInvoiceAmountInclTaxNoTaxAmount.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);
			Xsd.TxnHeader txnHeader = txnHeaderCollection[0];

			AssertEquals("Line 1 Ex Tax Amount", 110M, txnHeader.TxnLines[0].OsInvoiceAmtExclTax.Value);
			AssertEquals("Line 2 Ex Tax Amount", 110M, txnHeader.TxnLines[1].OsInvoiceAmtExclTax.Value);
			AssertEquals("Line 3 Ex Tax Amount", 110M, txnHeader.TxnLines[2].OsInvoiceAmtExclTax.Value);
			AssertEquals("Header Ex Tax Amount", 330M, txnHeader.OsInvoiceAmtExclTax.Value);

			AssertEquals("Line 1 Tax Amount", 0M, txnHeader.TxnLines[0].OsTaxAmount.Value);
			AssertEquals("Line 2 Tax Amount", 0M, txnHeader.TxnLines[1].OsTaxAmount.Value);
			AssertEquals("Line 3 Tax Amount", 0M, txnHeader.TxnLines[2].OsTaxAmount.Value);
			AssertEquals("Header Tax Amount", 0M, txnHeader.OsTaxAmount.Value);
		}

		public void TestImportWhenWrongTypeOfRowIsSpecified()
		{
			ZString expectedText = "Warning: Unrecognized row type detected. Only JOBINFO, INVHEAD, INVLINE & ATTACHMENT row types are used for import.The following row will be ignored: GLJHEAD";

			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();

			NotificationBuffer buffer = new NotificationBuffer();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(buffer, Factory);

			AssertNotContains("NotificationSubscriber", expectedText, buffer.ToString());

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\ValidGJL2Journals.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 0, txnHeaderCollection.Count);
			AssertContains("NotifictationSubscriber", expectedText, buffer.AsString);
		}

		public void TestImport_LocalInvoiceAmtExclTaxSpecified_WithDifferentHeaderCurrencyThanLocal()
		{
			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APWithDifferentHeaderCurrencyThanLocal.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);
			Xsd.TxnHeader txnHeader = txnHeaderCollection[0];

			AssertEquals("TxnHeader.TxnLines.Count", 1, txnHeader.TxnLines.Count);

			//line
			AssertEquals("OS Excl Tax Amount on line", -3000M, txnHeader.TxnLines[0].OsInvoiceAmtExclTax.Value);
			AssertEquals("OS Excl Currency on line", "IDR", txnHeader.TxnLines[0].OsInvoiceAmtExclTax.CurrencyCode);

			AssertEquals("OS Incl Tax Amount on line", -3500M, txnHeader.TxnLines[0].OsInvoiceAmtInclTax.Value);
			AssertEquals("OS Incl Currency on line", "IDR", txnHeader.TxnLines[0].OsInvoiceAmtInclTax.CurrencyCode);

			AssertEquals("OS Tax Amount on line", -500M, txnHeader.TxnLines[0].OsTaxAmount.Value);
			AssertEquals("OS Tax Currency on line", "IDR", txnHeader.TxnLines[0].OsTaxAmount.CurrencyCode);

			AssertEquals("Local Excl Tax Amount on line", -0.479M, txnHeader.TxnLines[0].LocalInvoiceAmtExclTax.Value);
			AssertEquals("Local Excl Currency on line", "AUD", txnHeader.TxnLines[0].LocalInvoiceAmtExclTax.CurrencyCode);

			Assert("Local Tax Amount is not specified on line", !txnHeader.TxnLines[0].LocalTaxAmount.IsSpecified);
			Assert("Local Incl Tax Amount is not specified on line", !txnHeader.TxnLines[0].LocalInvoiceAmtInclTax.IsSpecified);

			//header
			AssertEquals("OS Excl Tax Amount on header", -3000M, txnHeader.OsInvoiceAmtExclTax.Value);
			AssertEquals("OS Excl Currency on header", "IDR", txnHeader.OsInvoiceAmtExclTax.CurrencyCode);

			AssertEquals("OS Incl Tax Amount on header", -3500M, txnHeader.OsInvoiceAmtInclTax.Value);
			AssertEquals("OS Incl Currency on header", "IDR", txnHeader.OsInvoiceAmtInclTax.CurrencyCode);

			AssertEquals("OS Tax Amount on header", -500M, txnHeader.OsTaxAmount.Value);
			AssertEquals("OS Tax Currency on header", "IDR", txnHeader.OsTaxAmount.CurrencyCode);

			AssertEquals("Local Excl Tax Amount on header", -0.479M, txnHeader.LocalInvoiceAmtExclTax.Value);
			AssertEquals("Local Excl Currency on header", "AUD", txnHeader.LocalInvoiceAmtExclTax.CurrencyCode);

			Assert("Local Tax Amount is not specified on header", !txnHeader.LocalTaxAmount.IsSpecified);
			Assert("Local Incl Tax Amount is not specified on header", !txnHeader.LocalInvoiceAmtInclTax.IsSpecified);
		}

		public void TestImport_APInvoice_WithDifferentHeaderCurrencyThanLocalAndLocalCurrencyHavingDifferentDecimalValues()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Japan);
			Xsd.TxnHeaderCollection txnHeaderCollection = new Xsd.TxnHeaderCollection();
			TxnHeaderFlatFileConverter converter = new TxnHeaderFlatFileConverter(null, Factory);
			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APWithDifferentHeaderCurrencyThanLocalAndDifferentDecimalValues.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals("TxnHeaderCollection.Count", 1, txnHeaderCollection.Count);
			Xsd.TxnHeader txnHeader = txnHeaderCollection[0];

			AssertEquals("TxnHeader.TxnLines.Count", 2, txnHeader.TxnLines.Count);

			//line 1
			AssertEquals("OS Excl Tax Amount on line", -10M, txnHeader.TxnLines[0].OsInvoiceAmtExclTax.Value);
			AssertEquals("OS Excl Currency on line", "USD", txnHeader.TxnLines[0].OsInvoiceAmtExclTax.CurrencyCode);

			AssertEquals("OS Tax Amount on line", 0M, txnHeader.TxnLines[0].OsTaxAmount.Value);
			AssertEquals("OS Tax Currency on line", "USD", txnHeader.TxnLines[0].OsTaxAmount.CurrencyCode);

			AssertEquals("OS Incl Tax Amount on line", -10M, txnHeader.TxnLines[0].OsInvoiceAmtInclTax.Value);
			AssertEquals("OS Incl Currency on line", "USD", txnHeader.TxnLines[0].OsInvoiceAmtInclTax.CurrencyCode);

			//line 2
			AssertEquals("OS Excl Tax Amount on line", -10.5M, txnHeader.TxnLines[1].OsInvoiceAmtExclTax.Value);
			AssertEquals("OS Excl Currency on line", "USD", txnHeader.TxnLines[1].OsInvoiceAmtExclTax.CurrencyCode);

			AssertEquals("OS Tax Amount on line", 0M, txnHeader.TxnLines[1].OsTaxAmount.Value);
			AssertEquals("OS Tax Currency on line", "USD", txnHeader.TxnLines[1].OsTaxAmount.CurrencyCode);

			AssertEquals("OS Incl Tax Amount on line", -10.5M, txnHeader.TxnLines[1].OsInvoiceAmtInclTax.Value);
			AssertEquals("OS Incl Currency on line", "USD", txnHeader.TxnLines[1].OsInvoiceAmtInclTax.CurrencyCode);

			//header
			AssertEquals("Calculated OS Excl Tax Amount on header from lines is", -20.5M, txnHeader.OsInvoiceAmtExclTax.Value);
		}

		public void TestImport_EmptyInvoiceDate()
		{
			var expectedText = "Error: Invoice Date cannot be empty.";

			var txnHeaderCollection = new Xsd.TxnHeaderCollection();

			var buffer = new NotificationBuffer();
			var converter = new TxnHeaderFlatFileConverter(buffer, Factory);

			AssertNotContains("NotificationSubscriber", expectedText, buffer.ToString());

			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceWithEmptyInvoiceDate.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(true, buffer.HasErrors);
			AssertEquals(expectedText, buffer.AsString.Trim());
		}

		public void TestImport_InvalidInvoiceDateDueDateAndPostDate()
		{
			var expectedText = @"Error: Data type conversion error (PostDate format should be yyyyMMdd.)
Error: Data type conversion error (InvoiceDate format should be yyyyMMdd.)
Error: Data type conversion error (DueDate format should be yyyyMMdd.)";

			var txnHeaderCollection = new Xsd.TxnHeaderCollection();

			var buffer = new NotificationBuffer();
			var converter = new TxnHeaderFlatFileConverter(buffer, Factory);

			AssertNotContains("NotificationSubscriber", expectedText, buffer.ToString());

			using (var reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\APInvoiceWithInvalidInvoiceDateDueDateAndPostDate.csv"))
			{
				converter.ImportFlatFile(txnHeaderCollection, new CsvFlatFileFormat(false), reader);
			}

			AssertEquals(true, buffer.HasErrors);
			AssertEquals(expectedText, buffer.AsString.Trim());
		}

		public void TestImport_AdjustmentNoteShouldNotBeImportedWhenHavingJob()
		{
			AssertAdjustmentNoteWithJobInfo("APAdjustmentNoteWithJobInfo_SHP1.csv"
				, expectedHavingError: true);
			AssertAdjustmentNoteWithJobInfo("APAdjustmentNoteWithJobInfo_SHP2.csv"
				, expectedHavingError: true);
			AssertAdjustmentNoteWithJobInfo("ARAdjustmentNoteWithJobInfo_SHP1.csv"
				, expectedHavingError: true);
			AssertAdjustmentNoteWithJobInfo("ARAdjustmentNoteWithJobInfo_SHP2.csv"
				, expectedHavingError: true);
			AssertAdjustmentNoteWithJobInfo("EdgeCaseForPayable_AdjustmentNoteAfterInvoiceWithSameJobInfoSHP.csv"
				, expectedHavingError: true);

			AssertAdjustmentNoteWithJobInfo("ARAdjustmentNoteWithJobInfo_NIL1.csv"
				, expectedHavingError: false);
			AssertAdjustmentNoteWithJobInfo("ARAdjustmentNoteWithJobInfo_NIL2.csv"
				, expectedHavingError: false);
			AssertAdjustmentNoteWithJobInfo("ARAdjustmentNoteWithJobInfo_WithoutJobInfo.csv"
				, expectedHavingError: false);
			AssertAdjustmentNoteWithJobInfo("APAdjustmentNoteWithJobInfo_NIL1.csv"
				, expectedHavingError: false);
			AssertAdjustmentNoteWithJobInfo("APAdjustmentNoteWithJobInfo_NIL2.csv"
				, expectedHavingError: false);
			AssertAdjustmentNoteWithJobInfo("APAdjustmentNoteWithJobInfo_WithoutJobInfo.csv"
				, expectedHavingError: false);
		}

		void AssertAdjustmentNoteWithJobInfo(string testFile, bool expectedHavingError)
		{
			var expectedError = "Error: Data type conversion error (Adjustments cannot be imported when JOBINFO is not NIL.)";

			var logs = new List<INotification>();
			var mockNotifications = new Mock<INotifications>();
			mockNotifications.Setup(x => x.Add(It.IsAny<INotification>()))
				.Callback<INotification>(log => logs.Add(log));
			var converter = new TxnHeaderFlatFileConverter(mockNotifications.Object, Factory);

			AssertEquals("PreCondition", 0, logs.Count);
			using (var reader = new StreamReader(BaseSourcePath + $@"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\TxnHeader\Invoice\Testing\{testFile}"))
			{
				converter.ImportFlatFile(new Xsd.TxnHeaderCollection(), new CsvFlatFileFormat(false), reader);
			}

			var filteredLogs = logs.Where(x => x.Message == expectedError);
			AssertEquals($"{testFile} expected error number", expectedHavingError ? 1 : 0, filteredLogs.Count());
		}
	}
}
