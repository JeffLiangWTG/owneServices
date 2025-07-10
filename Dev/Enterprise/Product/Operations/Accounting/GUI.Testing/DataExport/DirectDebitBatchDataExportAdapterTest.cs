using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.DataTransfer.DataExport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.DataExport.Testing
{
	sealed class DirectDebitBatchDataExportAdapterTest : TestCaseWithFactory
	{
		[TestDate(2019, 11, 1)]
		public void TestExportWizardUseASCIIEncoding()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);

			string errorMsg = null;
			using (var file = TempFile.NewWithExtension("csv"))
			{
				var result = adapter.CreateFile(file.Filename, out errorMsg);
				AssertEquals(true, result);
				Assert(string.IsNullOrEmpty(errorMsg));

				using (var stream = File.OpenRead(file.Filename))
				{
					var actual = new byte[(int)stream.Length];
					stream.Read(actual, 0, (int)stream.Length);
					var expected = Encoding.ASCII.GetBytes(File.ReadAllText(file.Filename));
					AssertEquals(expected, actual);
				}
			}
		}

		[TestDate(2019, 11, 1)]
		public void TestCustomDirectDebitExportIncludesEventLogAndEdoc()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);

			string errorMsg = null;
			using (var tempFile = TempFile.NewWithExtension("csv"))
			{
				var result = adapter.CreateFile(tempFile.Filename, out errorMsg);
				AssertEquals("CreateFile() should return true", true, result);
				Assert("CreateFile() should have no error message", string.IsNullOrEmpty(errorMsg));
				Assert("CreateFile() should create a file!", File.Exists(tempFile.Filename));

				var ddrEventLogs = header.Logs.Find(l => l.Event.SE_Code == Events.EditedARecord.Code && l.ReferenceFreeText == "DDR File Generated.");
				AssertEquals("Exactly one Event Log for DDR file should be created.", 1, ddrEventLogs.Count());
				Assert("Event Log should be saved.", ddrEventLogs.First().IsInDatabase);

				var storageFile = (BusinessObject)header.DocManagerInfo.Files[0];
				var docType = (RefDocType)storageFile["DocType"];
				Assert("StorageFile should be saved", storageFile.IsInDatabase);
				AssertEquals("RefDocType should exist and be of the correct code", "DDR", docType.RT_DocType);
				AssertGreaterThan("StorageFile should be larger than zero bytes.", ((Enterprise.Integration.DocumentEngine.IDeliveryEmailAttachment)storageFile).FileSizeInBytes, 0);
			}
		}

		[TestDate(2019, 11, 1)]
		public void TestCreateFileDoesNotWriteToUnmappedPath_WhenUnexpectedExceptionBeforeAttachedToEdocs()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);
			adapter.CreateFile_ErrorAction_ForTestOnly = (finalPath) =>
			{
				throw new ApplicationException("Oh My! This was not expected");
			};

			var outputPath = Path.Combine(Env.TempPath, Guid.NewGuid().ToString());
			try
			{
				AssertExceptionThrown<ApplicationException>("Precondition: Unexpected exception should be thrown during CreateFile()", () => adapter.CreateFile(outputPath, out _));
				Assert("Final output path should not exist when an unexpected exception occurs before eDoc attached.", !File.Exists(outputPath));
			}
			finally
			{
				AccountingUtils.DeleteFileSafe(outputPath);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 11, 1)]
		public void TestCreateFileDoesNotWriteToUnmappedPath_WhenUnexpectedExceptionBeforeAttachedToEdocs_AndFileNameExpressionUsed()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);
			adapter.CreateFile_ErrorAction_ForTestOnly = (finalPath) =>
			{
				throw new ApplicationException("Oh My! This was not expected");
			};

			var exportWizard = GetAdapterExportWizard(adapter);
			var filePath = BaseTestFilePath + @"DataExport\Settings\yusen.xml";
			exportWizard.Setting = "YUSENTEST";
			exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));
			exportWizard.FileNameExpression = "\"Test.txt\"";
			exportWizard.FileNameExpressionObject = header;
			exportWizard.SaveSettings();
			var blob = ZBlob.FromAscii(exportWizard.Setting);

			var setting = Factory.NewWithValidTestData<StmData>();
			setting.SD_Owner = header.BankAccount.PK;
			setting.SD_Name = "DDRBatchExportSetting";
			setting.SD_BinaryValue = blob;

			var expectedExpressionPath = Path.Combine(new FileMapper().GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Test.txt");
			try
			{
				AssertExceptionThrown<ApplicationException>("Precondition: Unexpected exception should be thrown during CreateFile()", () => adapter.CreateFile(string.Empty, out _));
				Assert("Final output path should not exist when an unexpected exception occurs before eDoc attached.", !File.Exists(expectedExpressionPath));
			}
			finally
			{
				AccountingUtils.DeleteFileSafe(expectedExpressionPath);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 11, 1)]
		public void TestCreateFile_ShowsErrorMessage_WhenInvalidFileNameExpression()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);

			var exportWizard = GetAdapterExportWizard(adapter);
			var filePath = BaseTestFilePath + @"DataExport\Settings\yusen.xml";
			exportWizard.Setting = "YUSENTEST";
			exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));
			exportWizard.FileNameExpression = "\"\\\\SOMESERVER.client.local\\SHARE\\Folder\\NL_SEPA\\TheFile_\" + DateTime.Now.ToString(\"yyyyMMdd_HHmmss\") + \".txt\"";
			exportWizard.FileNameExpressionObject = header;
			exportWizard.SaveSettings();
			var blob = ZBlob.FromAscii(exportWizard.Setting);

			var setting = Factory.NewWithValidTestData<StmData>();
			setting.SD_Owner = header.BankAccount.PK;
			setting.SD_Name = "DDRBatchExportSetting";
			setting.SD_BinaryValue = blob;

			var expectedErrorMessage = $@"Unable to evaluate Filename Expression from Data Export Wizard. Please check your configuration in Actions > Customize Export.
  SystemException: 'unicodeescape' codec can't decode bytes in position 97: malformed \N character escape";

			// As the FileNameExpression refers to an invalid path, no file should be written to disk. Any attempt to write a file will throw.
			adapter.CreateFile(string.Empty, out var errorMessage);
			AssertEquals("Error message should be set when unable to evaluate FileNameExpression", expectedErrorMessage, errorMessage);
		}

		#region TestYusenExport

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 11, 1)]
		public void TestYusenExport()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);
			adapter.Sort(header);
			var businessObjects = adapter.GetBusinessObjectsForExport(header);
			var exportWizard = GetAdapterExportWizard(adapter);
			var filePath = BaseTestFilePath + @"DataExport\Settings\yusen.xml";
			exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));
			var results = new List<string[]>(exportWizard.ExportCollection(businessObjects, int.MaxValue));
			AssertYusenExport(results);
		}

		void AssertYusenExport(List<string[]> results)
		{
			int rowCount = 0;
			AssertYusenPayment(results[0], ++rowCount, "00001000", "87654321", "AUD", "100.00", "ACCOUNT NAME", "01234567", "9876", "543", "123456");
			AssertYusenPayment(results[1], ++rowCount, "00001000", "87654321", "AUD", "220.00", "ACCOUNT NAME", "01234567", "9876", "543", "987654");
			AssertYusenPaidTransaction(results[2], ++rowCount, "00001000/PI/CL/AP INVOICE/AUD/220.00");
			AssertYusenPayment(results[3], ++rowCount, "00001000", "87654321", "AUD", "330.00", "ACCOUNT NAME", "01234567", "9876", "543", "555");
			AssertYusenPaidTransaction(results[4], ++rowCount, "00001000/PC/CL/DISCOUNT RELATING TO MATCH NO M00001005/AUD/-110.00");
			AssertYusenPaidTransaction(results[5], ++rowCount, "00001000/PI/CL/AP INVOICE/AUD/440.00");
			AssertYusenPayment(results[6], ++rowCount, "00001000", "87654321", "AUD", "11100.00", "JOHN SMITH", "88884321", "4321", "765", "111222");
		}

		void AssertYusenPayment(string[] line, int rowCount, string valueDate, string accountNumber, string currency, string amount, string accountTitle,
			string beneficiaryAccountNumber, string beneficiaryBankCode, string beneficiaryBranchCode, string reference)
		{
			AssertEquals(string.Format("Line {0} Length", rowCount), 41, line.Length);
			AssertEquals("", line[0]);
			AssertEquals("GI", line[1]);
			AssertEquals("MIS", line[2]);
			AssertEquals("T", line[3]);
			AssertEquals(valueDate, line[4]);
			AssertEquals(accountNumber, line[5]);
			AssertEquals(currency, line[6]);
			AssertEquals(amount, line[7]);
			AssertEquals(accountTitle, line[8]);
			AssertEquals("", line[9]);
			AssertEquals("", line[10]);
			AssertEquals("", line[11]);
			AssertEquals("", line[12]);
			AssertEquals("", line[13]);
			AssertEquals("", line[14]);
			AssertEquals("", line[15]);
			AssertEquals(beneficiaryAccountNumber, line[16]);
			AssertEquals("", line[17]);
			AssertEquals(beneficiaryBankCode, line[18]);
			AssertEquals("", line[19]);
			AssertEquals(beneficiaryBranchCode, line[20]);
			AssertEquals("", line[21]);
			AssertEquals("", line[22]);
			AssertEquals("", line[23]);
			AssertEquals("", line[24]);
			AssertEquals("", line[25]);
			AssertEquals("", line[26]);
			AssertEquals("", line[27]);
			AssertEquals("", line[28]);
			AssertEquals("", line[29]);
			AssertEquals("", line[30]);
			AssertEquals("", line[31]);
			AssertEquals("", line[32]);
			AssertEquals("", line[33]);
			AssertEquals("", line[34]);
			AssertEquals("", line[35]);
			AssertEquals(reference, line[36]);
			AssertEquals("", line[37]);
			AssertEquals("", line[38]);
			AssertEquals("", line[39]);
			AssertEquals("", line[40]);
		}

		void AssertYusenPaidTransaction(string[] line, int rowCount, string additionalDescription)
		{
			AssertEquals(string.Format("Line {0} Length", rowCount), 5, line.Length);
			AssertEquals("", line[0]);
			AssertEquals("GI", line[1]);
			AssertEquals("MIS", line[2]);
			AssertEquals("A", line[3]);
			AssertEquals(additionalDescription, line[4]);
		}

		#endregion

		[TestDate(2019, 11, 1)]
		public void TestGetBusinessObjectsForExport()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);
			adapter.Sort(header);
			var list = new List<BusinessObject>(adapter.GetBusinessObjectsForExport(header));

			var count = 0;
			AssertEquals("List Count", 24, list.Count);
			AssertEquals("DataExportFileHeader", typeof(DataExportFileHeader), list[count++].GetType());
			AssertEquals("DataExportDirectDebitBatchHeader", typeof(DataExportDirectDebitBatchHeader), list[count++].GetType());
			AssertEquals("DataExportPaymentHeader", typeof(DataExportPaymentHeader), list[count++].GetType());
			AssertEquals("DataExportPaymentFooter", typeof(DataExportPaymentFooter), list[count++].GetType());
			AssertEquals("DataExportPaymentHeader", typeof(DataExportPaymentHeader), list[count++].GetType());
			AssertEquals("DataExportPaidTransaction", typeof(DataExportPaidTransaction), list[count++].GetType());
			AssertEquals("DataExportPaymentFooter", typeof(DataExportPaymentFooter), list[count++].GetType());
			AssertEquals("DataExportPaymentHeader", typeof(DataExportPaymentHeader), list[count++].GetType());
			AssertEquals("DataExportPaidTransaction", typeof(DataExportPaidTransaction), list[count++].GetType());
			AssertEquals("DataExportPaidTransaction", typeof(DataExportPaidTransaction), list[count++].GetType());
			AssertEquals("DataExportPaymentFooter", typeof(DataExportPaymentFooter), list[count++].GetType());
			AssertEquals("DataExportPaymentHeader", typeof(DataExportPaymentHeader), list[count++].GetType());
			AssertEquals("DataExportPaymentFooter", typeof(DataExportPaymentFooter), list[count++].GetType());
			AssertEquals("DataExportDirectDebitBatchFooter", typeof(DataExportDirectDebitBatchFooter), list[count++].GetType());
			AssertEquals("DataExportFileFooter", typeof(DataExportFileFooter), list[count++].GetType());
			AssertEquals("DataExportNordeaFormatSpacingFooter", typeof(DataExportNordeaFormatSpacingFooter), list[count++].GetType());
			AssertEquals("DataExportNordeaFormatSpacingFooter", typeof(DataExportNordeaFormatSpacingFooter), list[count++].GetType());
			AssertEquals("DataExportNordeaFormatSpacingFooter", typeof(DataExportNordeaFormatSpacingFooter), list[count++].GetType());
			AssertEquals("DataExportNordeaFormatSpacingFooter", typeof(DataExportNordeaFormatSpacingFooter), list[count++].GetType());
			AssertEquals("DataExportNordeaFormatSpacingFooter", typeof(DataExportNordeaFormatSpacingFooter), list[count++].GetType());
			AssertEquals("DataExportNordeaFormatSpacingFooter", typeof(DataExportNordeaFormatSpacingFooter), list[count++].GetType());
			AssertEquals("DataExportNordeaFormatSpacingFooter", typeof(DataExportNordeaFormatSpacingFooter), list[count++].GetType());
			AssertEquals("DataExportNordeaFormatSpacingFooter", typeof(DataExportNordeaFormatSpacingFooter), list[count++].GetType());
			AssertEquals("DataExportNordeaFormatSpacingFooter", typeof(DataExportNordeaFormatSpacingFooter), list[count++].GetType());
		}

		[TestDate(2019, 11, 1)]
		public void TestGetMultiTypeCollectionInfo()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);
			adapter.Sort(header);
			var collectionInfo = adapter.GetMultiTypeCollectionInfo(header);

			var names = new List<string>() { "File Header", "Direct Debit Batch Header", "Payment Header", "Paid Transaction", "Payment Footer", "Direct Debit Batch Footer", "File Footer", "Nordea Format Spacing Footer" };
			var types = new List<Type>() { typeof(DataExportFileHeader), typeof(DataExportDirectDebitBatchHeader), typeof(DataExportPaymentHeader), typeof(DataExportPaidTransaction),
												  typeof(DataExportPaymentFooter), typeof(DataExportDirectDebitBatchFooter), typeof(DataExportFileFooter), typeof(DataExportNordeaFormatSpacingFooter) };

			for (int i = 0; i < names.Count; i++)
			{
				var rowTypes = new List<RowType>(collectionInfo.RowTypes);

				AssertEquals("Name", names[i], rowTypes[i].Name);
				AssertEquals("Type", types[i], rowTypes[i].Type);

				var properties = new List<IImportPropertyInfo>(rowTypes[i].Properties);
				var schemaType = types[i].BaseType.GetNestedType("Schema") ?? types[i].BaseType.BaseType.GetNestedType("Schema");

				foreach (var info in schemaType.GetFields())
				{
					if (info.FieldType.Name == "String" && !info.Name.EndsWith("MaxLength"))
					{
						IImportPropertyInfo importPropertyInfo = null;

						foreach (var info2 in properties)
						{
							if (info2.MappingName == ZCustomTypeDescriptor.GetProperties(types[i])[info.Name].Name)
							{
								importPropertyInfo = info2;
								break;
							}
						}
						AssertNotNull(string.Format("Property not found: {0}.", info.Name), importPropertyInfo);
					}
				}
			}
		}

		#region TestGenericAlmostEmptyExport

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 11, 1)]
		public void TestGenericAlmostEmptyExport()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);
			adapter.Sort(header);
			var businessObjects = adapter.GetBusinessObjectsForExport(header);
			var collectionInfo = adapter.GetMultiTypeCollectionInfo(businessObjects);
			var exportWizard = new ExportWizard(collectionInfo, null, new FileMapper());
			var filePath = BaseTestFilePath + @"DataExport\Settings\GenericAlmostEmptyTest.xml";
			exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));
			var results = new List<string[]>(exportWizard.ExportCollection(businessObjects, int.MaxValue));
			AssertGenericAlmostEmptyExport(results);
		}

		void AssertGenericAlmostEmptyExport(List<string[]> results)
		{
			int rowCount = 0;
			AssertEquals("Number of rows", 4, results.Count);
			AssertGenericAlmostEmptyPaymentHeader(results[rowCount], ++rowCount, "100.00", "AUD");
			AssertGenericAlmostEmptyPaymentHeader(results[rowCount], ++rowCount, "220.00", "AUD");
			AssertGenericAlmostEmptyPaymentHeader(results[rowCount], ++rowCount, "330.00", "AUD");
			AssertGenericAlmostEmptyPaymentHeader(results[rowCount], ++rowCount, "11100.00", "AUD");
		}

		void AssertGenericAlmostEmptyPaymentHeader(string[] line, int rowCount, string paymentAmount, string paymentCurrency)
		{
			AssertEquals(string.Format("Line {0} column count", rowCount), 2, line.Length);
			AssertEquals(paymentAmount, line[0]);
			AssertEquals(paymentCurrency, line[1]);
		}

		#endregion

		#region TestGenericExport

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 11, 1)]
		public void TestGenericExport()
		{
			/*
			 * This test is a generic export which should test all properties and expression functions used in the DataExportWizard export mappings.
			 * If you need to add more fields to the unit test:
			 *		1. Import the DataExport\Settings\GenericTest.xml
			 *		2. Make the modification to the mapping
			 *		3. Export the mapping
			 *		4. Update DataExport\Settings\GenericTest.xml with the exported mapping
			 *		5. Alter the below assertions
			 */

			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);
			adapter.Sort(header);

			var businessObjects = adapter.GetBusinessObjectsForExport(header);
			var collectionInfo = adapter.GetMultiTypeCollectionInfo(businessObjects);

			var exportWizard = new ExportWizard(collectionInfo, null, new FileMapper());
			var filePath = BaseTestFilePath + @"DataExport\Settings\GenericTest.xml";
			exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));

			var results = new List<string[]>(exportWizard.ExportCollection(businessObjects, int.MaxValue));
			AssertGenericExport(results);
		}

		void AssertGenericExport(List<string[]> results)
		{
			int rowCount = 0;
			AssertGenericFileHeader(results[rowCount], ++rowCount, "11750.00", "11750.00", "00001000", "00001000", "4/11/2009", "AUD", "AAA", "NRB", "BNE", "BRN",
				"4", "1", "0", "0", "Eagle Datamation International", "0000000000", "0087654321", "greater", "ABC", "DEF", "CDEF", "11750.0000", "11750.00", "11750",
				"20091104", "091104", "ERROR: 'DataExportFileHeader' object has no attribute 'BogusProperty'");
			AssertGenericDirectDebitBatchHeader(results[rowCount], ++rowCount, "11750.00", "11750.00", "00001000", "00001000", "4/11/2009", "AUD", "AAA", "NRB", "BNE", "BRN",
				"4", "1", "0", "0", "Eagle Datamation International", "Eagle Datamation", "87654321", "");
			AssertGenericPaymentHeader(results[rowCount], ++rowCount, "100.00", "AUD", "100.00", "AUD", "00001000", "4/11/2009", "AP", "PAY", "DDR", "123456", "00001000",
				"ACCOUNT NAME", "9876543", "01234567", "ABC Branch", "ABC Add1", "ABC Add2", "ABC Add3", "A.A.L. SHIPPING AGENCIES P/L", "PO BOX 10446", "ADELAIDE ST, BRISBANE QLD",
				"AU", "", "AALSHI", "AAA", "12345678", "87654321", "", "", "", "", "1", "CMSI", "00001000", "100.00", "9876", "543", "", "0000010000", "000000000000001");
			AssertGenericPaymentFooter(results[rowCount], ++rowCount, "100.00", "AUD", "100.00", "AUD", "00001000", "4/11/2009", "AP", "PAY", "DDR", "123456", "00001000",
				"ACCOUNT NAME", "9876543", "01234567", "ABC Branch", "ABC Add1", "ABC Add2", "ABC Add3", "A.A.L. SHIPPING AGENCIES P/L", "PO BOX 10446", "ADELAIDE ST, BRISBANE QLD",
				"AU", "", "AALSHI", "AAA", "12345678", "87654321", "", "", "", "", "1");
			AssertGenericPaymentHeader(results[rowCount], ++rowCount, "220.00", "AUD", "220.00", "AUD", "00001001", "4/11/2009", "AP", "PAY", "DDR", "987654", "00001000",
				"ACCOUNT NAME", "9876543", "01234567", "ABC Branch", "ABC Add1", "ABC Add2", "ABC Add3", "A.A.L. SHIPPING AGENCIES P/L", "PO BOX 10446", "ADELAIDE ST, BRISBANE QLD",
				"AU", "", "AALSHI", "AAA", "12345678", "87654321", "004", "", "", "", "2", "CMSI", "00001000", "220.00", "9876", "543", "", "0000022000", "000000000000002");
			AssertGenericPaidTransaction(results[rowCount], ++rowCount, "004", "4/11/2009", "4/11/2009", "4/11/2009", "AALSHI", "AUD", "AP INVOICE", "200.00", "20.00", "220.00", "1", "220.00", "220.00", "220.00", "00001001", "AALSHI", "A.A.L. SHIPPING AGENCIES P/L", "00001000/PI/CL/AP INVOICE/AUD/220.00");
			AssertGenericPaymentFooter(results[rowCount], ++rowCount, "220.00", "AUD", "220.00", "AUD", "00001001", "4/11/2009", "AP", "PAY", "DDR", "987654", "00001000",
				"ACCOUNT NAME", "9876543", "01234567", "ABC Branch", "ABC Add1", "ABC Add2", "ABC Add3", "A.A.L. SHIPPING AGENCIES P/L", "PO BOX 10446", "ADELAIDE ST, BRISBANE QLD",
				"AU", "", "AALSHI", "AAA", "12345678", "87654321", "004", "", "", "", "2");
			AssertGenericPaymentHeader(results[rowCount], ++rowCount, "330.00", "AUD", "330.00", "AUD", "00001002", "4/11/2009", "AP", "PAY", "DDR", "555", "00001000",
				"ACCOUNT NAME", "9876543", "01234567", "ABC Branch", "ABC Add1", "ABC Add2", "ABC Add3", "A.A.L. SHIPPING AGENCIES P/L", "PO BOX 10446", "ADELAIDE ST, BRISBANE QLD",
				"AU", "", "AALSHI", "AAA", "12345678", "87654321", "005", "", "", "", "3", "CMSI", "00001000", "330.00", "9876", "543", "", "0000033000", "000000000000003");
			AssertGenericPaidTransaction(results[rowCount], ++rowCount, "00001000", "4/11/2009", "4/11/2009", "4/11/2009", "AALSHI", "AUD", "DISCOUNT RELATING TO MATCH NO M00001005", "110.00", "", "110.00", "1", "-110.00", "-110.00", "-110.00", "00001002", "AALSHI", "A.A.L. SHIPPING AGENCIES P/L", "00001000/PC/CL/DISCOUNT RELATING TO MATCH NO M00001005/AUD/-110.00");
			AssertGenericPaidTransaction(results[rowCount], ++rowCount, "005", "4/11/2009", "4/11/2009", "4/11/2009", "AALSHI", "AUD", "AP INVOICE", "400.00", "40.00", "440.00", "1", "440.00", "440.00", "440.00", "00001002", "AALSHI", "A.A.L. SHIPPING AGENCIES P/L", "00001000/PI/CL/AP INVOICE/AUD/440.00");
			AssertGenericPaymentFooter(results[rowCount], ++rowCount, "330.00", "AUD", "330.00", "AUD", "00001002", "4/11/2009", "AP", "PAY", "DDR", "555", "00001000",
				"ACCOUNT NAME", "9876543", "01234567", "ABC Branch", "ABC Add1", "ABC Add2", "ABC Add3", "A.A.L. SHIPPING AGENCIES P/L", "PO BOX 10446", "ADELAIDE ST, BRISBANE QLD",
				"AU", "", "AALSHI", "AAA", "12345678", "87654321", "005", "", "", "", "3");
			AssertGenericPaymentHeader(results[rowCount], ++rowCount, "11100.00", "AUD", "11000.00", "AUD", "00001000", "4/11/2009", "CB", "DPY", "DDR", "111222",
				"00001000", "JOHN SMITH", "4321765", "88884321", "", "", "", "", "", "", "", "", "", "", "AAA", "12345678", "87654321", "", "", "", "", "4", "CMSI", "00001000",
				"11100.00", "4321", "765", "", "0001110000", "000000000000004");
			AssertGenericPaymentFooter(results[rowCount], ++rowCount, "11100.00", "AUD", "11000.00", "AUD", "00001000", "4/11/2009", "CB", "DPY", "DDR", "111222",
				"00001000", "JOHN SMITH", "4321765", "88884321", "", "", "", "", "", "", "", "", "", "", "AAA", "12345678", "87654321", "", "", "", "", "4");
			AssertGenericDirectDebitBatchFooter(results[rowCount], ++rowCount, "11750.00", "11750.00", "00001000", "00001000", "4/11/2009", "AUD", "AAA", "NRB", "BNE", "BRN", "4", "1", "0", "0", "Eagle Datamation International", "000004", "0000011750");
			AssertGenericFileFooter(results[rowCount], ++rowCount, "11750.00", "11750.00", "00001000", "00001000", "4/11/2009", "AUD", "AAA", "NRB", "BNE", "BRN", "4", "1", "0", "0", "Eagle Datamation International", "00000004");
		}

		void AssertGenericFileHeader(string[] line, int rowCount, string batchAmount, string batchAmountInLocalCurrency, string bankReferenceNumber,
			string batchNumber, string postDate, string currency, string bankAccount, string paymentType, string branch, string department,
			string numberOfPayments, string fileNumber, string fileID, string sequenceNumberOffset, string companyName, string swift, string accountNum,
			string iif, string left, string right, string mid, string numberFormat4, string numberFormat2, string numberFormat0, string yyyyMMdd,
			string yyMMdd, string bogusProperty)
		{
			AssertEquals(string.Format("Line {0} column count", rowCount), 33, line.Length);
			AssertEquals("RowType", "FileHeader", line[0]);
			AssertEquals("BatchAmount", batchAmount, line[1]);
			AssertEquals("BatchAmountInLocalCurrency", batchAmountInLocalCurrency, line[2]);
			AssertEquals("BankReferenceNumber", bankReferenceNumber, line[3]);
			AssertEquals("BatchNumber", batchNumber, line[4]);
			AssertEquals("PostDate", postDate, line[5]);
			AssertEquals("Currency", currency, line[6]);
			AssertEquals("BankAccount", bankAccount, line[7]);
			AssertEquals("PaymentType", paymentType, line[8]);
			AssertEquals("Branch", branch, line[9]);
			AssertEquals("Department", department, line[10]);
			AssertEquals("NumberOfPayments", numberOfPayments, line[11]);
			AssertEquals("FileNumber", fileNumber, line[12]);
			AssertEquals("FileID", fileID, line[13]);
			AssertEquals("SequenceNumberOffset", sequenceNumberOffset, line[14]);
			AssertEquals("CompanyName", companyName, line[15]);
			AssertEquals("Empty Column", string.Empty, line[16]);
			AssertEquals("SWIFT", swift, line[17]);
			AssertEquals("AccountNum", accountNum, line[18]);
			AssertEquals("FileID", fileID, line[19]);
			AssertEquals("FileID", fileID, line[20]);
			AssertEquals("BatchNumber", batchNumber, line[21]);
			AssertEquals("BatchNumber", batchNumber, line[22]);
			AssertEquals("iif", iif, line[23]);
			AssertEquals("left", left, line[24]);
			AssertEquals("right", right, line[25]);
			AssertEquals("mid", mid, line[26]);
			AssertEquals("numberFormat", numberFormat4, line[27]);
			AssertEquals("numberFormat", numberFormat2, line[28]);
			AssertEquals("numberFormat", numberFormat0, line[29]);
			AssertEquals("PostDate", yyyyMMdd, line[30]);
			AssertEquals("PostDate", yyMMdd, line[31]);
			AssertEquals("BogusProperty", bogusProperty, line[32]);
		}

		void AssertGenericDirectDebitBatchHeader(string[] line, int rowCount, string batchAmount, string batchAmountInLocalCurrency,
			string bankReferenceNumber, string batchNumber, string postDate, string currency, string bankAccount, string paymentType, string branch,
			string department, string numberOfPayments, string fileNumber, string fileID, string sequenceNumberOffset, string companyName,
			string partCompanyName, string accountNum, string swift)
		{
			AssertEquals(string.Format("Line {0} column count", rowCount), 19, line.Length);
			AssertEquals("RowType", "DirectDebitBatchHeader", line[0]);
			AssertEquals("BatchAmount", batchAmount, line[1]);
			AssertEquals("BatchAmountInLocalCurrency", batchAmountInLocalCurrency, line[2]);
			AssertEquals("BankReferenceNumber", bankReferenceNumber, line[3]);
			AssertEquals("BatchNumber", batchNumber, line[4]);
			AssertEquals("PostDate", postDate, line[5]);
			AssertEquals("Currency", currency, line[6]);
			AssertEquals("BankAccount", bankAccount, line[7]);
			AssertEquals("PaymentType", paymentType, line[8]);
			AssertEquals("Branch", branch, line[9]);
			AssertEquals("Department", department, line[10]);
			AssertEquals("NumberOfPayments", numberOfPayments, line[11]);
			AssertEquals("FileNumber", fileNumber, line[12]);
			AssertEquals("FileID", fileID, line[13]);
			AssertEquals("SequenceNumberOffset", sequenceNumberOffset, line[14]);
			AssertEquals("CompanyName", companyName, line[15]);
			AssertEquals("PartCompanyName", partCompanyName, line[16]);
			AssertEquals("AccountNum", accountNum, line[17]);
			AssertEquals("SWIFT", swift, line[18]);
		}

		void AssertGenericPaymentHeader(string[] line, int rowCount, string paymentAmount, string paymentCurrency, string paymentAmountInLocalCurrency,
			string localCurrency, string transactionNumber, string postDate, string ledger, string transactionType, string paymentType, string paymentReference,
			string directDebitBatchNumber, string payeeName, string payeeBankBSB, string payeeBankAccountNumber, string payeeBankBranchName,
			string payeeBankAddress1, string payeeBankAddress2, string payeeBankAddress3, string payeeAddress1, string payeeAddress2,
			string payeeAddress3, string payeeCountryCode, string payeeSWIFTID, string organization, string bankAccount, string bankBSB,
			string bankAccountNumber, string relatedInvoiceNumbers, string relatedJobNumbers, string relatedHouseBillNumbers, string relatedMasterBillNumbers,
			string sequenceNumber, string cMSI, string reference, string numberFormat, string payeeBankBSBPart1, string payeeBankBSBPart2, string sWIFT,
			string oSTotalAmount, string paddedSequenceNumber)
		{
			AssertEquals(string.Format("Line {0} column count", rowCount), 41, line.Length);
			int currentLineIndex = 0;

			NextLineCheck("RowType", "PaymentHeader");
			NextLineCheck("PaymentAmount", paymentAmount);
			NextLineCheck("PaymentCurrency", paymentCurrency);
			NextLineCheck("PaymentAmountInLocalCurrency", paymentAmountInLocalCurrency);
			NextLineCheck("LocalCurrency", localCurrency);
			NextLineCheck("TransactionNumber", transactionNumber);
			NextLineCheck("PostDate", postDate);
			NextLineCheck("Ledger", ledger);
			NextLineCheck("TransactionType", transactionType);
			NextLineCheck("PaymentType", paymentType);
			NextLineCheck("PaymentReference", paymentReference);
			NextLineCheck("DirectDebitBatchNumber", directDebitBatchNumber);
			NextLineCheck("PayeeName", payeeName);
			NextLineCheck("PayeeBankBSB", payeeBankBSB);
			NextLineCheck("PayeeBankAccountNumber", payeeBankAccountNumber);
			NextLineCheck("PayeeBankBranchName", payeeBankBranchName);
			NextLineCheck("PayeeBankAddress1", payeeBankAddress1);
			NextLineCheck("PayeeBankAddress2", payeeBankAddress2);
			NextLineCheck("PayeeBankAddress3", payeeBankAddress3);
			NextLineCheck("PayeeAddress1", payeeAddress1);
			NextLineCheck("PayeeAddress2", payeeAddress2);
			NextLineCheck("PayeeAddress3", payeeAddress3);
			NextLineCheck("PayeeCountryCode", payeeCountryCode);
			NextLineCheck("PayeeSWIFTID", payeeSWIFTID);
			NextLineCheck("Organization", organization);
			NextLineCheck("BankAccount", bankAccount);
			NextLineCheck("BankBSB", bankBSB);
			NextLineCheck("BankAccountNumber", bankAccountNumber);
			NextLineCheck("RelatedInvoiceNumbers", relatedInvoiceNumbers);
			NextLineCheck("RelatedJobNumbers", relatedJobNumbers);
			NextLineCheck("RelatedHouseBillNumbers", relatedHouseBillNumbers);
			NextLineCheck("RelatedMasterBillNumbers", relatedMasterBillNumbers);
			NextLineCheck("SequenceNumber", sequenceNumber);
			NextLineCheck("CMSI", cMSI);
			NextLineCheck("Reference", reference);
			NextLineCheck("NumberFormat", numberFormat);
			NextLineCheck("PayeeBankBSBPart1", payeeBankBSBPart1);
			NextLineCheck("PayeeBankBSBPart2", payeeBankBSBPart2);
			NextLineCheck("SWIFT", sWIFT);
			NextLineCheck("OSTotalAmount", oSTotalAmount);
			NextLineCheck("PaddedSequenceNumber", paddedSequenceNumber);

			void NextLineCheck(string message, string expectedValue)
			{
				AssertEquals(message, expectedValue, line[currentLineIndex++]);
			}
		}

		void AssertGenericPaidTransaction(string[] line, int rowCount, string transactionNumber, string invoiceDate, string postDate, string dueDate,
			string organization, string currency, string description, string transactionExTaxAmount, string transactionTaxAmount, string transactionTotalAmount,
			string exchangeRate, string matchedAmountInLocalCurrency, string matchedAmountInTransactionCurrency, string matchedAmountInPaymentCurrency,
			string paymentTransactionNumber, string organizationCode, string organizationFullName, string concatenation)
		{
			AssertEquals(string.Format("Line {0} column count", rowCount), 19, line.Length);
			AssertEquals("RowType", "PaidTransaction", line[0]);
			AssertEquals("TransactionNumber", transactionNumber, line[1]);
			AssertEquals("InvoiceDate", invoiceDate, line[2]);
			AssertEquals("PostDate", postDate, line[3]);
			AssertEquals("DueDate", dueDate, line[4]);
			AssertEquals("Organization", organization, line[5]);
			AssertEquals("Currency", currency, line[6]);
			AssertEquals("Description", description, line[7]);
			AssertEquals("TransactionExTaxAmount", transactionExTaxAmount, line[8]);
			AssertEquals("TransactionTaxAmount", transactionTaxAmount, line[9]);
			AssertEquals("TransactionTotalAmount", transactionTotalAmount, line[10]);
			AssertEquals("ExchangeRate", exchangeRate, line[11]);
			AssertEquals("MatchedAmountInLocalCurrency", matchedAmountInLocalCurrency, line[12]);
			AssertEquals("MatchedAmountInTransactionCurrency", matchedAmountInTransactionCurrency, line[13]);
			AssertEquals("MatchedAmountInPaymentCurrency", matchedAmountInPaymentCurrency, line[14]);
			AssertEquals("PaymentTransactionNumber", paymentTransactionNumber, line[15]);
			AssertEquals("OrganizationCode", organizationCode, line[16]);
			AssertEquals("OrganizationFullName", organizationFullName, line[17]);
			AssertEquals("Concatenation", concatenation, line[18]);
		}

		void AssertGenericPaymentFooter(string[] line, int rowCount, string paymentAmount, string paymentCurrency, string paymentAmountInLocalCurrency,
			string localCurrency, string transactionNumber, string postDate, string ledger, string transactionType, string paymentType, string paymentReference,
			string directDebitBatchNumber, string payeeName, string payeeBankBSB, string payeeBankAccountNumber, string payeeBankBranchName,
			string payeeBankAddress1, string payeeBankAddress2, string payeeBankAddress3, string payeeAddress1, string payeeAddress2,
			string payeeAddress3, string payeeCountryCode, string payeeSWIFTID, string organization, string bankAccount, string bankBSB,
			string bankAccountNumber, string relatedInvoiceNumbers, string relatedJobNumbers, string relatedHouseBillNumbers, string relatedMasterBillNumbers,
			string sequenceNumber)
		{
			AssertEquals(string.Format("Line {0} column count", rowCount), 33, line.Length);
			int currentLineIndex = 0;

			NextLineCheck("RowType", "PaymentFooter");
			NextLineCheck("PaymentAmount", paymentAmount);
			NextLineCheck("PaymentCurrency", paymentCurrency);
			NextLineCheck("PaymentAmountInLocalCurrency", paymentAmountInLocalCurrency);
			NextLineCheck("LocalCurrency", localCurrency);
			NextLineCheck("TransactionNumber", transactionNumber);
			NextLineCheck("PostDate", postDate);
			NextLineCheck("Ledger", ledger);
			NextLineCheck("TransactionType", transactionType);
			NextLineCheck("PaymentType", paymentType);
			NextLineCheck("PaymentReference", paymentReference);
			NextLineCheck("DirectDebitBatchNumber", directDebitBatchNumber);
			NextLineCheck("PayeeName", payeeName);
			NextLineCheck("PayeeBankBSB", payeeBankBSB);
			NextLineCheck("PayeeBankAccountNumber", payeeBankAccountNumber);
			NextLineCheck("PayeeBankBranchName", payeeBankBranchName);
			NextLineCheck("PayeeBankAddress1", payeeBankAddress1);
			NextLineCheck("PayeeBankAddress2", payeeBankAddress2);
			NextLineCheck("PayeeBankAddress3", payeeBankAddress3);
			NextLineCheck("PayeeAddress1", payeeAddress1);
			NextLineCheck("PayeeAddress2", payeeAddress2);
			NextLineCheck("PayeeAddress3", payeeAddress3);
			NextLineCheck("PayeeCountryCode", payeeCountryCode);
			NextLineCheck("PayeeSWIFTID", payeeSWIFTID);
			NextLineCheck("Organization", organization);
			NextLineCheck("BankAccount", bankAccount);
			NextLineCheck("BankBSB", bankBSB);
			NextLineCheck("BankAccountNumber", bankAccountNumber);
			NextLineCheck("RelatedInvoiceNumbers", relatedInvoiceNumbers);
			NextLineCheck("RelatedJobNumbers", relatedJobNumbers);
			NextLineCheck("RelatedHouseBillNumbers", relatedHouseBillNumbers);
			NextLineCheck("RelatedMasterBillNumbers", relatedMasterBillNumbers);
			NextLineCheck("SequenceNumber", sequenceNumber);

			void NextLineCheck(string message, string expectedValue)
			{
				AssertEquals(message, expectedValue, line[currentLineIndex++]);
			}
		}

		void AssertGenericDirectDebitBatchFooter(string[] line, int rowCount, string batchAmount, string batchAmountInLocalCurrency,
			string bankReferenceNumber, string batchNumber, string postDate, string currency, string bankAccount, string paymentType, string branch,
			string department, string numberOfPayments, string fileNumber, string fileID, string sequenceNumberOffset, string companyName,
			string paddedNumberOfPayments, string paddedOSExTaxAmount)
		{
			AssertEquals(string.Format("Line {0} column count", rowCount), 18, line.Length);
			AssertEquals("RowType", "DirectDebitBatchFooter", line[0]);
			AssertEquals("BatchAmount", batchAmount, line[1]);
			AssertEquals("BatchAmountInLocalCurrency", batchAmountInLocalCurrency, line[2]);
			AssertEquals("BankReferenceNumber", bankReferenceNumber, line[3]);
			AssertEquals("BatchNumber", batchNumber, line[4]);
			AssertEquals("PostDate", postDate, line[5]);
			AssertEquals("Currency", currency, line[6]);
			AssertEquals("BankAccount", bankAccount, line[7]);
			AssertEquals("PaymentType", paymentType, line[8]);
			AssertEquals("Branch", branch, line[9]);
			AssertEquals("Department", department, line[10]);
			AssertEquals("NumberOfPayments", numberOfPayments, line[11]);
			AssertEquals("FileNumber", fileNumber, line[12]);
			AssertEquals("FileID", fileID, line[13]);
			AssertEquals("SequenceNumberOffset", sequenceNumberOffset, line[14]);
			AssertEquals("CompanyName", companyName, line[15]);
			AssertEquals("PaddedNumberOfPayments", paddedNumberOfPayments, line[16]);
			AssertEquals("PaddedOSExTaxAmount", paddedOSExTaxAmount, line[17]);
		}

		void AssertGenericFileFooter(string[] line, int rowCount, string batchAmount, string batchAmountInLocalCurrency,
			string bankReferenceNumber, string batchNumber, string postDate, string currency, string bankAccount, string paymentType, string branch,
			string department, string numberOfPayments, string fileNumber, string fileID, string sequenceNumberOffset, string companyName,
			string paddedNumberOfPayments)
		{
			AssertEquals(string.Format("Line {0} column count", rowCount), 17, line.Length);
			AssertEquals("RowType", "FileFooter", line[0]);
			AssertEquals("BatchAmount", batchAmount, line[1]);
			AssertEquals("BatchAmountInLocalCurrency", batchAmountInLocalCurrency, line[2]);
			AssertEquals("BankReferenceNumber", bankReferenceNumber, line[3]);
			AssertEquals("BatchNumber", batchNumber, line[4]);
			AssertEquals("PostDate", postDate, line[5]);
			AssertEquals("Currency", currency, line[6]);
			AssertEquals("BankAccount", bankAccount, line[7]);
			AssertEquals("PaymentType", paymentType, line[8]);
			AssertEquals("Branch", branch, line[9]);
			AssertEquals("Department", department, line[10]);
			AssertEquals("NumberOfPayments", numberOfPayments, line[11]);
			AssertEquals("FileNumber", fileNumber, line[12]);
			AssertEquals("FileID", fileID, line[13]);
			AssertEquals("SequenceNumberOffset", sequenceNumberOffset, line[14]);
			AssertEquals("CompanyName", companyName, line[15]);
			AssertEquals("PaddedNumberOfPayments", paddedNumberOfPayments, line[16]);
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 11, 1)]
		public void TestIsFileNameExpressionUsed()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);
			adapter.Sort(header);

			var exportWizard = GetAdapterExportWizard(adapter);
			var filePath = BaseTestFilePath + @"DataExport\Settings\yusen.xml";
			exportWizard.Setting = "YUSENTEST";
			exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));
			exportWizard.SaveSettings();

			var data = Factory.New<StmData>();
			data.SD_Name = "DDRBatchExportSetting";
			data.SD_Owner = header.BankAccount.PK;
			var blob = ZBlob.FromAscii(exportWizard.Setting);
			data.SD_BinaryValue = blob;

			AssertEquals("IsFileNameExpressionUsed", false, adapter.IsFileNameExpressionUsed);

			exportWizard.FileNameExpression = "\"yusen.csv\"";
			exportWizard.FileNameExpressionObject = header;

			AssertEquals("IsFileNameExpressionUsed", true, adapter.IsFileNameExpressionUsed);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2019, 11, 1)]
		public void TestLastExportedFullFileName()
		{
			var header = GetDirectDebitBatchHeader();
			var adapter = new DirectDebitBatchDataExportAdapter(Factory, header);
			adapter.Sort(header);

			var exportWizard = GetAdapterExportWizard(adapter);
			var filePath = BaseTestFilePath + @"DataExport\Settings\yusen.xml";
			exportWizard.Setting = "YUSENTEST";
			exportWizard.SetSettings(DataExportWizardSettings.FromXml<DataExportWizardSettings>(File.ReadAllText(filePath)));
			exportWizard.SaveSettings();

			AssertEquals("Precondition: LastExportedFullFileName should be blank", string.Empty, adapter.LastExportedFullFileName);

			var filename = Guid.NewGuid().ToString() + ".csv";
			exportWizard.FileNameExpression = "\"" + filename + "\"";
			exportWizard.FileNameExpressionObject = header;

			var createFileResult = adapter.CreateFile(string.Empty, out var errorMessage);

			Assert("CreateFile() should return success", createFileResult);
			AssertNullOrEmpty("CreateFile() errorMessage should be blank", errorMessage);
			var fullFilePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), filename);
			AssertEquals("LastExportedFullFileName should be the expected export path", fullFilePath, adapter.LastExportedFullFileName);

			if (File.Exists(fullFilePath))
			{
				File.Delete(fullFilePath);
			}
		}

		#region Implementation

		string BaseTestFilePath => BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\";

		DirectDebitBatchHeader GetDirectDebitBatchHeader()
		{
			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupSinglePeriod(200911, new ZDateTime(2009, 11, 01), new ZDateTime(2009, 11, 30));

			var gLHeader = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "6110.10.10");
			AssertNotNull("GLHeader", gLHeader);

			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "AAA";
			bankAccount.AB_Desc = "AAA BANK ACCOUNT";
			bankAccount.AB_AG = gLHeader.PK;
			bankAccount.AB_BankName = "AAA BANK";
			bankAccount.AB_BankAddress = "123 SOME STREET, SYDNEY, NSW, 2000";
			bankAccount.AB_BankAccountName = "EAGLE DATAMATION INTERNATIONAL";
			bankAccount.AB_BSB = "12345678";
			bankAccount.AB_AccountNum = "87654321";
			bankAccount.AB_BankAbbreviation = "AAA";
			bankAccount.AB_RX_NKAccountCurrency = "AUD";
			bankAccount.AB_AllowAutoDDR = true;
			bankAccount.AB_AutoDDRFormat = "BTM";
			bankAccount.AB_DetailedDepositSlip = true;
			bankAccount.AB_IsDefaultReceiptBankAccount = false;

			var setting = Factory.NewWithValidTestData<StmData>();
			setting.SD_Owner = bankAccount.PK;
			setting.SD_Name = "DDRBatchExportSetting";

			var orgHeader = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
			AssertNotNull("OrgHeader", orgHeader);

			var accountDetails = orgHeader.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_AccountName = "ACCOUNT NAME";
			accountDetails.A1_BankName = "BANK NAME";
			accountDetails.A1_BankAccount = "01234567";
			accountDetails.A1_BankBranchName = "ABC Branch";
			accountDetails.A1_BankAddress1 = "ABC Add1";
			accountDetails.A1_BankAddress2 = "ABC Add2";
			accountDetails.A1_BankAddress3 = "ABC Add3";
			accountDetails.A1_BankBsb = "9876543";
			accountDetails.A1_IsDefaultAccount = true;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "John Smith";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Payables.ToString();
			document.OD_FilterShipmentMode = Core.Constants.TransportModes.All;
			document.OD_FilterDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;

			orgHeader.Addresses.MainAddress.OA_City = "BRISBANE";
			orgHeader.Addresses.MainAddress.OA_State = "QLD";

			Factory.Save();

			AssertEquals("orgHeader should have no errors: " + orgHeader.NotificationsIncludingChildren.ToUniqueMessageListString(), false, orgHeader.HasErrors);

			Env.Security.NewPayablesPaymentDirectDebit.IsAllowed = true;

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var date = new ZDateTime(2009, 11, 4);

			//1 - Unmatched Payment

			var unmatchedPayment = Factory.New<APPayment>();
			unmatchedPayment.AH_InvoiceDate = date;
			unmatchedPayment.AH_PostDate = date;
			unmatchedPayment.AH_OH = orgHeader.PK;
			unmatchedPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			unmatchedPayment.AH_AB = bankAccount.PK;
			unmatchedPayment.AH_ChequeOrReference = "123456";
			unmatchedPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			unmatchedPayment.AH_LocalExTaxAmount = 100.00m;
			unmatchedPayment.AH_OSExTaxAmount = 100.00m;

			AssertEquals("unmatched payment should have no errors: " + unmatchedPayment.NotificationsIncludingChildren.ToUniqueMessageListString(), false, unmatchedPayment.HasErrors);

			Factory.Save();

			//2 - Matched Payment & Invoice

			var invoice1 = Factory.New<APInvoice>();
			invoice1.AH_OH = orgHeader.PK;
			invoice1.AH_PostDate = date;
			invoice1.AH_InvoiceDate = date;
			invoice1.AH_DueDate = date;
			invoice1.AH_TransactionNum = "004";

			var line1 = (APInvoiceLine)invoice1.Lines.AddNew();

			var gLHeader2 = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "7100.40.10");
			AssertNotNull("GLHeader2", gLHeader2);

			line1.GenericCharge = gLHeader2.PK;
			line1.AL_LocalExTaxAmount = 200.00m;
			line1.AL_OSExTaxAmount = 200.00m;

			var query = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(AccTaxRateSchema.AT_Code, "GST");
			var gSTtaxRate = Factory.LoadTop1<AccTaxRate>(query);
			gSTtaxRate.SetRateNumerator_ForTestOnly(10);
			line1.AL_AT = gSTtaxRate.PK;
			invoice1.AH_OutstandingAmount = 0m;
			invoice1.AH_FullyPaidDate = date;

			var matchedPayment1 = Factory.New<APPayment>();
			matchedPayment1.AH_PostDate = date;
			matchedPayment1.AH_InvoiceDate = date;
			matchedPayment1.AH_OH = orgHeader.PK;
			matchedPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			matchedPayment1.AH_AB = bankAccount.PK;
			matchedPayment1.AH_ChequeOrReference = "987654";
			matchedPayment1.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			matchedPayment1.AH_LocalExTaxAmount = 220.00m;
			matchedPayment1.AH_OSExTaxAmount = 220.00m;
			matchedPayment1.AH_OutstandingAmount = 0m;
			matchedPayment1.AH_FullyPaidDate = date;

			AssertEquals("matchedPayment1 should have no errors: " + matchedPayment1.NotificationsIncludingChildren.ToUniqueMessageListString(), false, matchedPayment1.HasErrors);

			var matchLink1 = ((IMatching)invoice1).CurrentMatchGroup.AddNew();
			matchLink1.AP_AH = invoice1.PK;
			matchLink1.AP_MatchGroupNum = "98765";
			matchLink1.AP_Amount = invoice1.AH_InvoiceAmount + invoice1.AH_GSTAmount;

			var matchLink2 = ((IMatching)invoice1).CurrentMatchGroup.AddNew();
			matchLink2.AP_AH = matchedPayment1.PK;
			matchLink2.AP_MatchGroupNum = matchLink1.AP_MatchGroupNum;
			matchLink2.AP_Amount = matchedPayment1.AH_InvoiceAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice1);

			Factory.Save();

			//3 - Direct Payment

			var directPayment = Factory.New<DirectPayment>();
			directPayment.AH_TransactionNum = "00001000";
			directPayment.AH_InvoiceDate = date;
			directPayment.AH_PostDate = date;
			directPayment.AH_AB = bankAccount.PK;
			directPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			directPayment.AH_ChequeOrReference = "111222";
			directPayment.AH_ChequeDrawer = "JOHN SMITH";
			directPayment.AH_DrawerBank = "88884321";
			directPayment.AH_DrawerBranch = "4321765";

			var gLHeader3 = Factory.LoadFromNaturalKey<AccGLHeader>(AccGLHeaderSchema.AG_AccountNum, "3510.00.00");
			AssertNotNull("GLHeader3", gLHeader3);

			var directPaymentLine1 = (DirectPaymentLine)directPayment.Lines.AddNew();
			directPaymentLine1.AL_AG = gLHeader2.PK;
			directPaymentLine1.AL_OSExTaxAmount = 1000.00m;
			directPaymentLine1.AL_LocalWHTAmount = 0.00m;
			directPaymentLine1.AL_AT = gSTtaxRate.PK;

			query = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
			query.AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			query.AddToFilter(AccTaxRateSchema.AT_Code, "FREEGST");
			var fREEGST = Factory.LoadTop1<AccTaxRate>(query);

			var directPaymentLine2 = (DirectPaymentLine)directPayment.Lines.AddNew();
			directPaymentLine2.AL_AG = gLHeader3.PK;
			directPaymentLine2.AL_OSExTaxAmount = 10000.00m;
			directPaymentLine2.AL_LocalWHTAmount = 0.00m;
			directPaymentLine2.AL_AT = fREEGST.PK;

			Factory.Save();

			//4 - Matched Payment, Invoice & Discount

			var invoice2 = Factory.New<APInvoice>();
			invoice2.AH_OH = orgHeader.PK;
			invoice2.AH_PostDate = date;
			invoice2.AH_InvoiceDate = date;
			invoice2.AH_DueDate = date;
			invoice2.AH_TransactionNum = "005";

			line1 = (APInvoiceLine)invoice2.Lines.AddNew();

			line1.GenericCharge = gLHeader3.PK;
			line1.AL_LocalExTaxAmount = 400.00m;
			line1.AL_OSExTaxAmount = 400.00m;

			line1.AL_AT = gSTtaxRate.PK;
			invoice2.AH_OutstandingAmount = 0m;
			invoice2.AH_FullyPaidDate = date;

			var matchedPayment2 = Factory.New<APPayment>();
			matchedPayment2.AH_InvoiceDate = date;
			matchedPayment2.AH_PostDate = date;
			matchedPayment2.AH_OH = orgHeader.PK;
			matchedPayment2.AH_ReceiptType = ReceiptTypes.DirectDebit;
			matchedPayment2.AH_AB = bankAccount.PK;
			matchedPayment2.AH_ChequeOrReference = "555";
			matchedPayment2.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			matchedPayment2.AH_LocalExTaxAmount = 330.00m;
			matchedPayment2.AH_OSExTaxAmount = 330.00m;
			matchedPayment2.AH_OutstandingAmount = 0m;
			matchedPayment2.AH_FullyPaidDate = date;

			var discount = Factory.New<APDiscount>();
			discount.AH_TransactionNum = "00001000";
			discount.AH_Desc = "DISCOUNT RELATING TO MATCH NO M00001005";
			discount.AH_InvoiceDate = date;
			discount.AH_PostDate = date;
			discount.AH_DueDate = date;
			discount.AH_OH = orgHeader.PK;
			discount.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
			discount.AH_LocalExTaxAmount = 110.00m;
			discount.AH_OSExTaxAmount = 110.00m;
			discount.AH_OutstandingAmount = 0m;
			discount.AH_FullyPaidDate = date;

			AssertEquals("matchedPayment2 should have no errors: " + matchedPayment2.NotificationsIncludingChildren.ToUniqueMessageListString(), false, matchedPayment2.HasErrors);
			AssertEquals("invoice2 should have no errors: " + invoice2.NotificationsIncludingChildren.ToUniqueMessageListString(), false, invoice2.HasErrors);
			AssertEquals("discount should have no errors: " + discount.NotificationsIncludingChildren.ToUniqueMessageListString(), false, discount.HasErrors);

			matchLink2 = ((IMatching)invoice2).CurrentMatchGroup.AddNew();
			matchLink2.AP_AH = matchedPayment2.PK;
			matchLink2.AP_MatchGroupNum = "12365";
			matchLink2.AP_Amount = matchedPayment2.AH_InvoiceAmount;

			var matchLink3 = ((IMatching)invoice2).CurrentMatchGroup.AddNew();
			matchLink3.AP_AH = discount.PK;
			matchLink3.AP_MatchGroupNum = "12365";
			matchLink3.AP_Amount = discount.AH_InvoiceAmount;

			matchLink1 = ((IMatching)invoice2).CurrentMatchGroup.AddNew();
			matchLink1.AP_AH = invoice2.PK;
			matchLink1.AP_MatchGroupNum = "12365";
			matchLink1.AP_Amount = invoice2.AH_InvoiceAmount + invoice2.AH_GSTAmount;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice2);

			Factory.Save();

			var batchHeader = Factory.New<DirectDebitBatchHeader>();
			batchHeader.AH_InvoiceDate = date;
			batchHeader.AH_PostDate = date;
			batchHeader.AH_AB = bankAccount.PK;
			batchHeader.AH_OSExTaxAmount = 11750.00m;
			batchHeader.AH_LocalExTaxAmount = 11750.00m;
			batchHeader.AH_LocalTaxAmount = 0.00m;
			batchHeader.AH_LocalWHTAmount = 0.00m;

			batchHeader.Lines.Add(matchedPayment2);                     //1st
			batchHeader.Lines.Add(directPayment);                       //2nd
			batchHeader.Lines.Add(matchedPayment1);                     //3rd
			batchHeader.Lines.Add(unmatchedPayment);                    //4th

			Factory.Save();

			return batchHeader;
		}

		ExportWizard GetAdapterExportWizard(DirectDebitBatchDataExportAdapter adapter)
		{
			var exportWizard = adapter.GetType()
								.GetProperty("ExportWizard", BindingFlags.NonPublic | BindingFlags.Instance)
								.GetValue(adapter, null);
			return (ExportWizard)exportWizard;
		}

		#endregion
	}
}
