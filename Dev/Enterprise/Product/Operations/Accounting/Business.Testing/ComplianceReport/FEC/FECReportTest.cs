using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.FEC;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using StorageDocsBase = Enterprise.DocumentScanning.Business.StorageDocsBase;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.FEC
{
	public class FECReportTest : TestCaseWithFactory
	{
		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		#region FECReport tests

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestReportGeneration()
		{
			// test purpose is to test if the main entry function FECReport.ExportData() can be called without errors and a simple output file is attached to the compliance report
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			Db.Connection.BeginTransaction();
			var complianceReport = CreateFECReport(new ZDate(2022, 11, 1), new ZDate(2022, 11, 30));

			using var disposableService = complianceReport.Factory.AddDisposableService();
			complianceReport.Company.OrgProxy.CustomsCodes.AddNew("TVA", "FR123456789", "FR");
			var serviceLogger = new DummyLogger();

			(_, var apInvoiceLine, _) = CreateAPInvoice(Creator.ABIGAS, "12345678", Creator.USD, 1.5m);
			apInvoiceLine.AL_AG = Creator.CreateGLAccountAndLocalAccountMapping("1111.11.11", Constants.Languages.French, Constants.CountryCodes.France, "P&L").PK;

			var retainedEarningsAccount = Creator.CreateRetainedEarningsAccount();
			Creator.CreateLocalAccountMappingForGLHeader(retainedEarningsAccount, Constants.Languages.French, Constants.CountryCodes.France);
			CreateAccGLAggregate(apInvoiceLine.AL_AG, 100.05, 202208, complianceReport.Company.PK);

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV**Rev-", null, apInvoiceLine); // uses GL account of transaction line

			Db.Connection.CommitTransaction();

			var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("Expected no usage records for report status changes.", 0, ediMessages.Length);

			do
			{
				complianceReport.GenerateFECFromQueue(serviceLogger, null);
			} while (complianceReport.ACR_Status == "QUE");

			var docManager = ((IDocManagerSupport)complianceReport).DocManagerInfo;
			AssertEquals("One eDoc added", 1, docManager.AllEDocs.Count);
			AssertEquals("Filename", "123456789FEC20221231.zip", docManager.AllEDocs[0].FileName.ToString());

			using (TempDirectory tempDirectory = new TempDirectory())
			{
				var zipFilename = Path.Combine(tempDirectory, docManager.AllEDocs[0].FileName.ToString());
				File.WriteAllBytes(zipFilename, docManager.AllEDocs[0].ImageData);

				ZipFile.ExtractToDirectory(zipFilename, tempDirectory);
				AssertASCIIFileSameAsString(Path.Combine(tempDirectory, "123456789FEC20221231.csv"), GetEmbeddedResourceAsZString("fec2022.csv").ToASCII());
			}

			AssertEquals("Report status", "GEN", complianceReport.ACR_Status);

			// This calls AccComplianceReport.ChangeAndApplyStatusCore() which starts the usage collector.
			// Test usage data written to DATABASE.
			ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("Expected two usage records for report status changes:  ADD -> QUE -> GEN", 2, ediMessages.Length);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		[TestDate(2022, 11, 7, 0, 0, 0)]
		public void TestGenerateChunks()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			using var currency = GlbCompany.CurrentCompany.TemporarilySetCurrency(Constants.CurrencyCodes.France);
			Db.Connection.BeginTransaction();
			var complianceReport = CreateFECReport(new ZDate(2022, 10, 1), new ZDate(2022, 12, 31));

			using var disposableService = complianceReport.Factory.AddDisposableService();
			complianceReport.Company.OrgProxy.CustomsCodes.AddNew("TVA", "FR123456789", "FR");
			var serviceLogger = new DummyLogger();
			var fecReport = new FECReport();

			// AP INV 1: partially paid
			var (aPInvoice1, apInvoiceLine11, _) = CreateAPInvoice(Creator.ABIGAS, "12345678", Creator.USD, 1.5m);
			var paymentDate = new DateTime(2022, 12, 6);
			var aPPayment1 = Creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice1, Creator.USDBankAccount.PK, paymentDate, -200m, matchGroupNumber: "M001", exchangeRate: 1.5m,
				useLocalPaidAmount: false);
			Creator.CreateLocalAccountMappingForGLHeader(Creator.USDBankAccount.GLHeader, Constants.Languages.French, Constants.CountryCodes.France);

			// AP INV 2: fully paid (plus a misc. transaction)
			var (aPInvoice2, apInvoiceLine21, apInvoiceLine22) = CreateAPInvoice(Creator.ABIGAS, "12345679", Creator.EUR, exRate: 1.0m);
			// GL account mapping
			apInvoiceLine11.AL_AG = apInvoiceLine21.AL_AG =
				apInvoiceLine22.AL_AG = Creator.CreateGLAccountAndLocalAccountMapping("1111.11.11", Constants.Languages.French, Constants.CountryCodes.France, "P&L").PK;
			var aPPayment21 = Creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice2, Creator.EURBankAccount.PK, paymentDate, -200, matchGroupNumber: "M0021", exchangeRate: 1.0m,
				useLocalPaidAmount: false);
			Creator.CreateLocalAccountMappingForGLHeader(Creator.EURBankAccount.GLHeader, Constants.Languages.French, Constants.CountryCodes.France);
			var aPOverPayment2 = Creator.CreateAndMatchMiscellaneousTransaction(aPInvoice2, paymentDate, -64, "M0022");
			Factory.Save();
			var aPPayment22 = Creator.CreateAndMatchAPPaymentForAPInvoice(aPInvoice2, Creator.EURBankAccount.PK, paymentDate, null, matchGroupNumber: "M0023", exchangeRate: 1.0m,
				useLocalPaidAmount: false);

			// AR INV 1: partially apid
			var (aRInvoice1, arInvoiceLine11, arInvoiceLine12) = CreateARInvoice(new DateTime(2022, 10, 20));
			arInvoiceLine11.AL_AG = arInvoiceLine12.AL_AG = Creator.CreateGLAccountAndLocalAccountMapping("2222.22.22", Constants.Languages.French, Constants.CountryCodes.France, "P&L").PK;
			var receiptDate = new DateTime(2022, 10, 25);
			var aRReceipt1 = Creator.CreateAndMatchARReceiptForARInvoice(aRInvoice1, Creator.EURBankAccount.PK, matchingDate: receiptDate, 150.0m, matchGroupNumber: "M003",
				postAndDueDate: receiptDate);

			// AR INV 2: fully paid (a misc. transaction)
			var (aRInvoice2, arInvoiceLine21, arInvoiceLine22) = CreateARInvoice(new DateTime(2022, 10, 22));
			arInvoiceLine21.AL_AG = arInvoiceLine22.AL_AG = arInvoiceLine12.AL_AG;
			receiptDate = new DateTime(2022, 10, 27);
			var aRReceipt21 = Creator.CreateAndMatchARReceiptForARInvoice(aRInvoice2, Creator.EURBankAccount.PK, matchingDate: receiptDate, 180.0m, matchGroupNumber: "M0041",
				postAndDueDate: receiptDate);
			Factory.Save();
			var aROverPayment2 = Creator.CreateAndMatchMiscellaneousTransaction(aRInvoice2, receiptDate, 30, "M0042");
			Factory.Save();
			var aRReceipt22 = Creator.CreateAndMatchARReceiptForARInvoice(aRInvoice2, Creator.EURBankAccount.PK, matchingDate: receiptDate, null, matchGroupNumber: "M0043",
				postAndDueDate: receiptDate);

			// AR INV 3: not paid at all
			var (aRInvoice3, arInvoiceLine31, arInvoiceLine32) = CreateARInvoice(new DateTime(2022, 10, 25));
			arInvoiceLine31.AL_AG = arInvoiceLine32.AL_AG = arInvoiceLine12.AL_AG;

			// create a WIP with Post date inside the report date range and reverse date outside the date range.
			(var wip1, _) = CreateShipmentJobAndWip("S002000", postDate: new DateTime(2022, 12, 11), false);
			wip1.AL_AG = apInvoiceLine22.AL_AG;

			// create an AP/AR contra
			var contra = CreateContra(new DateTime(2022, 12, 30));

			var retainedEarningsAccount = Creator.CreateRetainedEarningsAccount();
			Creator.CreateLocalAccountMappingForGLHeader(retainedEarningsAccount, Constants.Languages.French, Constants.CountryCodes.France);
			CreateAccGLAggregate(apInvoiceLine22.AL_AG, 100.05, 202208, complianceReport.Company.PK);

			var aRControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
			Creator.CreateLocalAccountMappingForGLHeader(aRControlAccount, Constants.Languages.French, Constants.CountryCodes.France);
			var aPControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			Creator.CreateLocalAccountMappingForGLHeader(aPControlAccount, Constants.Languages.French, Constants.CountryCodes.France);
			var ovpControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.OverpaymentsAccount.Value);
			Creator.CreateLocalAccountMappingForGLHeader(ovpControlAccount, Constants.Languages.French, Constants.CountryCodes.France);

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV**Rev-", null, apInvoiceLine11); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aPInvoice1 }, (_) => new ZDate(2022, 11, 25), (_) => "*AP*INV*APCtrl*Total");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aPPayment1 }, (_) => new ZDate(2022, 12, 6), (_) => "*AP*PAY*APCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aPPayment1 }, (_) => new ZDate(2022, 12, 6), (_) => "*AP*PAY*Bank*-");
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV**Rev-", null, apInvoiceLine21); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aPInvoice2 }, (_) => new ZDate(2022, 11, 25), (_) => "*AP*INV*APCtrl*Total");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aPPayment21 }, (_) => new ZDate(2022, 12, 6), (_) => "*AP*PAY*APCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aPPayment21 }, (_) => new ZDate(2022, 12, 6), (_) => "*AP*PAY*Bank*-");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aPPayment22 }, (_) => new ZDate(2022, 12, 6), (_) => "*AP*PAY*APCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aPPayment22 }, (_) => new ZDate(2022, 12, 6), (_) => "*AP*PAY*Bank*-");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aPOverPayment2 }, (_) => new ZDate(2022, 12, 6), (_) => "*AP*OVP*APCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aPOverPayment2 }, (_) => new ZDate(2022, 12, 6), (_) => "*AP*OVP**-");
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV**Rev-", null, arInvoiceLine11); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV**Rev-", null, arInvoiceLine12); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aRInvoice1 }, (_) => new ZDate(2022, 10, 20), (_) => "*AR*INV*ARCtrl*Total");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aRReceipt1 }, (_) => new ZDate(2022, 10, 25), (_) => "*AR*REC*ARCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aRReceipt1 }, (_) => new ZDate(2022, 10, 25), (_) => "*AR*REC*Bank*-");

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV**Rev-", null, arInvoiceLine21); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV**Rev-", null, arInvoiceLine22); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aRInvoice2 }, (_) => new ZDate(2022, 10, 22), (_) => "*AR*INV*ARCtrl*Total");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aRReceipt21 }, (_) => new ZDate(2022, 10, 27), (_) => "*AR*REC*ARCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aRReceipt21 }, (_) => new ZDate(2022, 10, 27), (_) => "*AR*REC*Bank*-");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aRReceipt22 }, (_) => new ZDate(2022, 10, 27), (_) => "*AR*REC*ARCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aRReceipt22 }, (_) => new ZDate(2022, 10, 27), (_) => "*AR*REC*Bank*-");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aROverPayment2 }, (_) => new ZDate(2022, 10, 27), (_) => "*AR*OVP*ARCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aROverPayment2 }, (_) => new ZDate(2022, 10, 27), (_) => "*AR*OVP**-");

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV**Rev-", null, arInvoiceLine31); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV**Rev-", null, arInvoiceLine32); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { aRInvoice3 }, (_) => new ZDate(2022, 10, 25), (_) => "*AR*INV*ARCtrl*Total");

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP**", null, wip1); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { contra.APRow }, (_) => new ZDate(2022, 12, 30), (_) => "*AP*CTR*APCtrl*");
			Creator.CreateComplianceReportQueueEntry(complianceReport, new List<TransactionHeader>() { contra.ARRow }, (_) => new ZDate(2022, 12, 30), (_) => "*AR*CTR*ARCtrl*");

			Db.Connection.CommitTransaction();

			for (var period = 10; period <= 12; period++)
			{
				var newStatus = fecReport.ExportData(complianceReport, serviceLogger);
				AssertEquals($"Report status after exporting period #{period}", "QUE", newStatus);
			}

			var docManager = ((IDocManagerSupport)complianceReport).DocManagerInfo;
			AssertEquals("3 files have been added to eDoc", 3, docManager.AllEDocs.Count);
			for (var period = 10; period <= 12; period++)
			{
				var index = period - 10;
				AssertEquals($"Filename for period #{period}", $"PERIOD2022{period:D2}.zip", docManager.AllEDocs[index].FileName.ToString());

				using TempDirectory tempDirectory = new TempDirectory();
				var zipFilename = Path.Combine(tempDirectory, docManager.AllEDocs[index].FileName.ToString());
				File.WriteAllBytes(zipFilename, docManager.AllEDocs[index].ImageData);

				ZipFile.ExtractToDirectory(zipFilename, tempDirectory);
				AssertASCIIFileSameAsString(Path.ChangeExtension(zipFilename, ".csv"), GetEmbeddedResourceAsZString("fec2022" + period + ".csv").ToASCII());
			}
		}

		public void TestCombineChunksIntoSingleFileNoCSVFile()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 3, 31));

			using var disposableService = complianceReport.Factory.AddDisposableService();
			complianceReport.Company.OrgProxy.CustomsCodes.AddNew("TVA", "FR333222111", "FR");
			complianceReport.NextProcessingStepFromDate = new ZDate(2022, 4, 1); // this means that all period chunks have been created and the next step is to combine them into a single file
			var serviceLogger = new DummyLogger();

			// add 3 system generated ZIP files
			AttachZipFileToeDocs("PERIOD202101.txt", complianceReport);
			AttachZipFileToeDocs("PERIOD202102.csv", complianceReport);
			AttachZipFileToeDocs("PERIOD202103.csv", complianceReport);

			var docManager = complianceReport.DocManagerInfo();

			var fecReport = new FECReport();
			var result = fecReport.CombineChunksIntoSingleFile(complianceReport, serviceLogger, null);

			AssertNullOrEmpty("CombineChunksIntoSingleFile()", result);
			Assert("Status not set to Error.", complianceReport.ACR_Status == AccComplianceReport.Status.ReportError);
			var notes = complianceReport.GetNotes().GetAllNotes();
			AssertEquals("Number of notes attached to report", 1, notes.Count);
			var firstNote = notes.Cast<StmNote>().First();
			AssertEquals("Note text",
				$"The final ZIP file cannot be created because one of the PERIOD*.ZIP files doesn't contain a CSV file.\r\nPlease re-queue this FEC report.\r\nFile: PERIOD202101.zip",
				firstNote.ST_NoteDataAsText);
			AssertEquals("Status not set to ERR.", AccComplianceReport.Status.ReportError, complianceReport.ACR_Status);
			AssertEquals("Status message not set correctly.", "Combining all periods into one ZIP file is not possible. See Notes tab for details.", complianceReport.ACR_StatusMessage);

			var fecFileIndex = docManager.AllEDocs.Cast<IeDocBase>().IndexOf(v => v.FileName == "333222111FEC20221231.zip");
			AssertEquals("333222111FEC20221231.zip has been added", -1, fecFileIndex);
		}

		public void TestCombineChunksIntoSingleFileDuplicateFile()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 3, 31));

			using var disposableService = complianceReport.Factory.AddDisposableService();
			complianceReport.Company.OrgProxy.CustomsCodes.AddNew("TVA", "FR333222111", "FR");
			complianceReport.NextProcessingStepFromDate = new ZDate(2022, 4, 1); // this means that all period chunks have been created and the next step is to combine them into a single file
			var serviceLogger = new DummyLogger();

			// add 3 system generated ZIP files
			AttachZipFileToeDocs("PERIOD202101.csv", complianceReport);
			// this one will be attached as PERIOD202101[2] under eDocs because default for overwriteExistingFileIfNotImageFile: false
			AttachZipFileToeDocs("PERIOD202101.csv", complianceReport);
			AttachZipFileToeDocs("PERIOD202102.csv", complianceReport);

			var docManager = complianceReport.DocManagerInfo();

			var fecReport = new FECReport();
			var result = fecReport.CombineChunksIntoSingleFile(complianceReport, serviceLogger, null);

			AssertNullOrEmpty("CombineChunksIntoSingleFile()", result);
			Assert("Status not set to Error.", complianceReport.ACR_Status == AccComplianceReport.Status.ReportError);
			var notes = complianceReport.GetNotes().GetAllNotes();
			AssertEquals("Number of notes attached to report", 1, notes.Count);
			var firstNote = notes.Cast<StmNote>().First();
			AssertEquals("Note text",
				$"The final ZIP file cannot be created because duplicate PERIODxxxxxx.zip files are attached under eDocs.\r\nThis might lead to duplicated data in the final FEC file.\r\nPlease re-queue this FEC report.\r\nFile: PERIOD202101[2].zip",
				firstNote.ST_NoteDataAsText);
			AssertEquals("Status not set to ERR.", AccComplianceReport.Status.ReportError, complianceReport.ACR_Status);
			AssertEquals("Status message not set correctly.", "Combining all periods into one ZIP file is not possible. See Notes tab for details.", complianceReport.ACR_StatusMessage);

			var fecFileIndex = docManager.AllEDocs.Cast<IeDocBase>().IndexOf(v => v.FileName == "333222111FEC20221231.zip");
			AssertEquals("333222111FEC20221231.zip has been added", -1, fecFileIndex);
		}

		public void TestCombineChunksIntoSingleFileIgnoreUserGeneratedFileAndDeleteOldNotes()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 3, 31));
			var docManager = complianceReport.DocManagerInfo();

			using var disposableService = complianceReport.Factory.AddDisposableService();
			complianceReport.Company.OrgProxy.CustomsCodes.AddNew("TVA", "FR333222111", "FR");
			complianceReport.NextProcessingStepFromDate = new ZDate(2022, 4, 1); // this means that all period chunks have been created and the next step is to combine them into a single file
			var serviceLogger = new DummyLogger();

			// Add a failure note to simulate a prior unsuccessful run
			var noteDescription = "Combining all periods into 1 ZIP file not possible";
			complianceReport.AddNote(noteDescription, "Any text, only the description matters.");

			AttachZipFileToeDocs("PERIOD202101.csv", complianceReport, isUserGeneratedFile: true);
			// Intentional naming collision, file will be attached as PERIOD202101[2] under eDocs because default for overwriteExistingFileIfNotImageFile: false
			AttachZipFileToeDocs("PERIOD202101.csv", complianceReport);
			AttachZipFileToeDocs("PERIOD202102.csv", complianceReport);
			AttachZipFileToeDocs("PERIOD202103.csv", complianceReport);

			var hasFailureNote = complianceReport.GetNotes().FindByDescription(noteDescription).Any();
			Assert("Report should have a failure note", hasFailureNote);

			var fecReport = new FECReport();
			var combinedFilename = fecReport.CombineChunksIntoSingleFile(complianceReport, serviceLogger, null);
			AssertNotNullOrEmpty("CombineChunksIntoSingleFile()", combinedFilename);

			hasFailureNote = complianceReport.GetNotes().FindByDescription(noteDescription).Any();
			Assert("Report should have no failure note", !hasFailureNote);

			var tempDirectory = complianceReport.Factory.SubscribeForDispose(new TempDirectory());
			ZipFile.ExtractToDirectory(combinedFilename, tempDirectory);
			AssertASCIIFileSameAsString(Path.Combine(tempDirectory, "333222111FEC20221231.csv"), GetEmbeddedResourceAsZString("combinedfiles.csv"));

			var userGeneratedFileStillExists = docManager.AllEDocs.Cast<IeDocBase>().Any(v => v.FileName == "PERIOD202101.zip" && !v.IsSystemGenerated);
			Assert("User-generated file has been deleted", userGeneratedFileStillExists);
		}

		void AttachZipFileToeDocs(ZString csvFileName, AccComplianceReport complianceReport, bool isUserGeneratedFile = false)
		{
			using var csvTempDir = new TempDirectory();
			var testData = csvFileName + "\r\n";
			var csvTempFullFilename = Path.Combine(csvTempDir, csvFileName);
			File.WriteAllText(csvTempFullFilename, testData);

			var zipTempDir = complianceReport.Factory.SubscribeForDispose(new TempDirectory());
			var zipFileName = Path.ChangeExtension(csvFileName, "zip");
			var zipTempFullFilename = Path.Combine(zipTempDir, zipFileName);

			if (ZipCompression.Zip(csvTempDir, zipTempFullFilename))
			{
				var eDoc = complianceReport.AttachFileToEdoc(zipTempFullFilename, "Test file");

				if (isUserGeneratedFile)
				{
					((StorageDocsBase)eDoc).SC_IsSystemGenerated = false;
				}
			}
		}

		void AttachFileToeDocs(ZString csvFileName, AccComplianceReport complianceReport)
		{
			var csvTempDir = complianceReport.Factory.SubscribeForDispose(new TempDirectory());
			var testData = csvFileName + "\r\n";
			var csvTempFullFilename = Path.Combine(csvTempDir, csvFileName);

			File.WriteAllText(csvTempFullFilename, testData);

			complianceReport.AttachFileToEdoc(csvTempFullFilename, "Test file");
		}

		public void TestCombineChunksIntoSingleFile()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 3, 31));
			var docManager = complianceReport.DocManagerInfo();

			using var disposableService = complianceReport.Factory.AddDisposableService();
			complianceReport.Company.OrgProxy.CustomsCodes.AddNew("TVA", "FR333222111", "FR");
			complianceReport.NextProcessingStepFromDate = new ZDate(2022, 4, 1); // this means that all period chunks have been created and the next step is to combine them into a single file
			var serviceLogger = new DummyLogger();

			// add 3 system generated ZIP files
			AttachZipFileToeDocs("PERIOD202101.csv", complianceReport);
			AttachZipFileToeDocs("PERIOD202102.csv", complianceReport);
			AttachZipFileToeDocs("PERIOD202103.csv", complianceReport);

			// add 1 user file
			AttachFileToeDocs("userfile.csv", complianceReport);

			var fecReport = new FECReport();

			var combinedFilename = fecReport.CombineChunksIntoSingleFile(complianceReport, serviceLogger, null); // This method only creates a combined file. It should be added to eDocs after by calling complianceReport.AttachFileToEdoc().

			AssertNotNullOrEmpty("CombineChunksIntoSingleFile()", combinedFilename);

			var tempDirectory = complianceReport.Factory.SubscribeForDispose(new TempDirectory());
			ZipFile.ExtractToDirectory(combinedFilename, tempDirectory);
			AssertASCIIFileSameAsString(Path.Combine(tempDirectory, "333222111FEC20221231.csv"), GetEmbeddedResourceAsZString("combinedfiles.csv"));

			AssertEquals("1 file exists in eDocs", 1, docManager.AllEDocs.Count);
			var userfileIndex = docManager.AllEDocs.Cast<IeDocBase>().IndexOf(v => v.FileName == "userfile.csv");
			AssertEquals("User file has not been deleted", true, userfileIndex >= 0);
		}

		public void TestAddOnColumnsSets_MaxLineNum()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			//Arrange: Set MaxLineNum = 0
			var fromDate = new ZDate(2022, 1, 1);
			var toDate = new ZDate(2022, 12, 31);
			var complianceReport = CreateFECReport(fromDate, toDate);

			//Act
			var reportTotal = complianceReport.ReportTotals;

			//Assert
			var query = new ZQuery(GenAddOnColumnSchema.XA_Name, AccComplianceReport.Schema.MaxLineNum);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentID, complianceReport.PK);
			var addOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);

			AssertEquals("Precondition: addOnColumn has not been created", null, addOnColumn);
			AssertEquals(ZInt.Zero, complianceReport.MaxLineNum);

			//Arrange: Set MaxLineNum from 0 to 1
			addOnColumn = Factory.New<GenAddOnColumn>();
			addOnColumn.XA_ParentID = complianceReport.PK;
			addOnColumn.XA_Name = AccComplianceReport.Schema.MaxLineNum;
			addOnColumn.XA_Type = AddOnColumnDataType.Codes.Integer;
			addOnColumn.XA_Data = "1";
			Factory.Save();

			//Act //Assert
			AssertEquals("MaxLineNum was not set from AddOnColumns", 1, complianceReport.MaxLineNum);

			//Arrange: Set MaxLineNum from 1 to 0
			addOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);
			addOnColumn.XA_Data = "0";

			//Act //Assert
			AssertEquals("MaxLineNum was not set from AddOnColumns", 0, complianceReport.MaxLineNum);
		}

		public void TestAddOnColumnsSets_SetAddOnColumnDecimal()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			//Arrange: SetAddOnColumnDecimal(GeneralLedgerAmountDR) = 0
			var fromDate = new ZDate(2022, 1, 1);
			var toDate = new ZDate(2022, 12, 31);
			var complianceReport = CreateFECReport(fromDate, toDate);

			//Act
			var reportTotal = complianceReport.ReportTotals;

			//Assert
			var query = new ZQuery(GenAddOnColumnSchema.XA_Name, AccComplianceReport.Schema.GeneralLedgerAmountDR);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentID, complianceReport.PK);
			var addOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);

			AssertEquals("Precondition: addOnColumn has not been created", null, addOnColumn);
			AssertEquals(ZDecimal.Zero, complianceReport.GeneralLedgerAmountDR);

			//Arrange: SetAddOnColumnDecimal(GeneralLedgerAmountDR) from 0 to 1
			addOnColumn = Factory.New<GenAddOnColumn>();
			addOnColumn.XA_ParentID = complianceReport.PK;
			addOnColumn.XA_Name = AccComplianceReport.Schema.GeneralLedgerAmountDR;
			addOnColumn.XA_Type = AddOnColumnDataType.Codes.Decimal;
			addOnColumn.XA_Data = "1.0";
			Factory.Save();

			//Act //Assert
			AssertEquals("SetAddOnColumnDecimal was not set from AddOnColumns", 1m, complianceReport.GeneralLedgerAmountDR);

			//Arrange: SetAddOnColumnDecimal(GeneralLedgerAmountDR) from 1 to 0
			addOnColumn = Factory.LoadTop1<GenAddOnColumn>(query);
			addOnColumn.XA_Data = "0.0";

			//Act //Assert
			AssertEquals("SetAddOnColumnDecimal was not set from AddOnColumns", 0m, complianceReport.GeneralLedgerAmountDR);
		}

		#endregion

		#region FECFileWriter tests

		public void TestGetFilename()
		{
			// test FEC output filename generation with different years and business registration numbers
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			for (var year = 2019; year < 2023; year++)
			{
				var complianceReport = CreateFECReport(new ZDate(year, 1, 1), new ZDate(year, 12, 31));
				Creator.CreateTestPeriods(new ZDateTime(year + 1, 1, 1));

				var fecWriter = new FECFileWriter(complianceReport,
					complianceReport.ACR_DateTo.AddDays(1)); // pass a date after the report to simulate the step than combines the chunks into a single file
				for (var vatRegnoRun = 0; vatRegnoRun < 20; vatRegnoRun++)
				{
					var siren = RandomString(9);
					var vatRegno = $"{RandomString(random.Next(1, 4))}{siren}";
					complianceReport.Company.OrgProxy.CustomsCodes.RemoveAll();
					complianceReport.NextProcessingStepFromDate = new ZDate(year + 1, 1, 1);
					complianceReport.Company.OrgProxy.CustomsCodes.AddNew("TVA", vatRegno, "FR");
					var actualFilename = fecWriter.GetFilename();
					AssertEquals("FEC filename", $"{siren}FEC{year}1231.csv", actualFilename);
				}
			}
		}

		public void TestWriteFECHeader()
		{
			// quick test if the FEC file header is correctly written to the output file (the header is a constant value)
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 12, 31));

			using var stream = new MemoryStream();
			using var fecWriter = new FECFileWriter(complianceReport, complianceReport.ACR_DateFrom);
			fecWriter.RedirectOutput(stream, true);
			fecWriter.WriteFECHeader();

			stream.Position = 0;

			var streamReader = new StreamReader(stream);
			var actualData = streamReader.ReadToEnd();

			AssertEquals("Column header row",
				"JournalCode|JournalLib|EcritureNum|EcritureDate|CompteNum|CompteLib|CompAuxNum|CompAuxLib|PieceRef|PieceDate|EcritureLib|Debit|Credit|EcritureLet|DateLet|ValidDate|Montantdevise|Idevise\r\n",
				actualData);
		}

		public void TestWriteFECOpeningBalances()
		{
			// quick test to verify the format of the open balance record in the FEC file by writing a single open balance line (correctness of open balances is tested in TestRetrieveOpenBalances)
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 1, 31));

			var account = Creator.CreateGLAccountAndLocalAccountMapping("2100.00.00", Constants.Languages.French, Constants.CountryCodes.France, "BSH");
			Creator.CreateAccGLAggregate(100, 202112, account.PK, GlbBranch.CurrentBranch.PK, complianceReport.Company.PK, GlbDepartment.CurrentDepartment.PK, "");
			Factory.Save();

			using var stream = new MemoryStream();
			using var fecWriter = new FECFileWriter(complianceReport, complianceReport.ACR_DateFrom);
			fecWriter.RedirectOutput(stream, true);

			fecWriter.WriteFECOpeningBalances();

			stream.Position = 0;

			var streamReader = new StreamReader(stream);
			var actualData = streamReader.ReadToEnd();

			AssertEquals("Open balance row",
				"OPENING|Opening balance of the posting account|OPENING0001|20220101|FR-2100.00.00|FR-2100.00.00 - Description|||Opening Balance|20220101|Opening Balance as at 010122|100,00|0,00|||20220101||\r\n",
				actualData);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestWriteFECTransactions()
		{
			// quick test to verify the format of the transaction record in the FEC file by writing a single transaction line (correctness of transaction details is tested in TestCreateTransactionAsCSV)
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			Db.Connection.BeginTransaction();
			var complianceReport = CreateFECReport(new ZDate(2022, 11, 1), new ZDate(2022, 11, 30));

			(_, var apInvoiceLine, _) = CreateAPInvoice(Creator.ABIGAS, "12345678", Creator.USD, 1.5m);
			apInvoiceLine.AL_AG = Creator.CreateGLAccountAndLocalAccountMapping("1111.11.11", Constants.Languages.French, Constants.CountryCodes.France, "P&L").PK;

			Factory.Save();
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV**Rev-", null, apInvoiceLine);
			Db.Connection.CommitTransaction();

			using var stream = new MemoryStream();
			using var fecWriter = new FECFileWriter(complianceReport, complianceReport.ACR_DateFrom);
			fecWriter.RedirectOutput(stream, true);

			fecWriter.WriteFECTransactions();

			stream.Position = 0;

			var streamReader = new StreamReader(stream);
			var actualData = streamReader.ReadToEnd();

			AssertEquals("Transaction row",
				"APINV|AP Invoice|APINV00001000|20221125|FR-1111.11.11|\"FR-1111.11.11 - Description\"|ABIGAS      |\"ABI GAS & TOOLS\"|12345678|20221125|\"Test Invoice\"|240,00|0,00|||20221125|360,00|USD\r\n",
				actualData);
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestNoWriteFECZeroTransactions()
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			Db.Connection.BeginTransaction();
			var complianceReport = CreateFECReport(new ZDate(2022, 11, 1), new ZDate(2022, 11, 30));

			var apInvoiceLine = CreateZeroAPInvoice();

			Factory.Save();
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV**Rev-", null, apInvoiceLine);
			Db.Connection.CommitTransaction();

			using var fecWriter = new FECFileWriter(complianceReport, complianceReport.ACR_DateFrom);
			using var stream = new MemoryStream();
			fecWriter.RedirectOutput(stream, true);

			fecWriter.WriteFECTransactions();

			stream.Position = 0;

			var streamReader = new StreamReader(stream);
			var actualData = streamReader.ReadToEnd();

			AssertEquals("Transaction row", string.Empty, actualData);
		}

		public void TestCreateTransactionAsCSV()
		{
			// test FEC representation of different transaction types
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var apControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			Creator.CreateLocalAccountMappingForGLHeader(apControlAccount, Constants.Languages.French, Constants.CountryCodes.France);

			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 12, 31));
			var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, SharedConstants.Languages.French);
			var fecWriter = new FECFileWriter(complianceReport, complianceReport.ACR_DateFrom, accountMappingHelper);

			AssertEquals("Credit transaction",
				"APINV|AP Invoice|APINV00001015|20220525|FR-8210.00.00|\"FR-8210.00.00 - Description\"|OH_Code|\"OH_FullName\"|00000001|20220526|\"AP Invoice 0001\"|0,00|135,80|||20220525|-162,96|USD",
				fecWriter.CreateTransactionAsCSV(new FECDataPerLine("AP", "INV", new ZDateTime(2022, 5, 25), ZGuid.Empty, "OH_Code", "OH_FullName", "00000001", new ZDateTime(2022, 5, 26),
					"AP Invoice 0001", -123.45, -12.35, new ZDateTime(2022, 5, 25), "USD", -162.96, 1.2, "*AP*INV*APCtrl*Total", "CST", "A", 0, 1, "00001015")));

			AssertEquals("Debit transaction",
				"ARCRD|AR Credit Note|ARCRD00000002|20220527|FR-8210.00.00|\"FR-8210.00.00 - Description\"|OH_Code|\"OH_FullName\"|00000002|20220527|\"AR Invoice 0002\"|225,54|0,00|||20220527||",
				fecWriter.CreateTransactionAsCSV(new FECDataPerLine("AR", "CRD", new ZDateTime(2022, 5, 27), ZGuid.Empty, "OH_Code", "OH_FullName", "00000002", new ZDateTime(2022, 5, 27),
					"AR Invoice 0002", 225.54, 25.06, new ZDateTime(2022, 5, 27), "EUR", 250.60, 1.0, "*AP*CTR*APCtrl*", "CST", "A", 0, 1, "")));

			AssertEquals("Zero debit and credit transaction",
				null,
				fecWriter.CreateTransactionAsCSV(new FECDataPerLine("AR", "CRD", new ZDateTime(2022, 5, 27), ZGuid.Empty, "OH_Code", "OH_FullName", "00000002", new ZDateTime(2022, 5, 27),
					"AR Invoice 0002", 0.0, 25.06, new ZDateTime(2022, 5, 27), "EUR", 250.60, 1.0, "*AP*CTR*APCtrl*", "CST", "A", 0, 1, "")));
		}

		public void TestCreateTransactionAsCSVWithLineBreaks()
		{
			// test FEC representation of different transaction types
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var apControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			Creator.CreateLocalAccountMappingForGLHeader(apControlAccount, Constants.Languages.French, Constants.CountryCodes.France);

			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 12, 31));
			var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, SharedConstants.Languages.French);
			var fecWriter = new FECFileWriter(complianceReport, complianceReport.ACR_DateFrom, accountMappingHelper);

			AssertEquals("Transaction with line breaks '\\r\\n'",
				"APINV|AP Invoice|APINV00001015|20220525|FR-8210.00.00|\"FR-8210.00.00 - Description\"|OH_Code|\"OH_FullName\"|00000001|20220526|\"AP Invoice  0001\"|0,00|135,80|||20220525|-162,96|USD",
				fecWriter.CreateTransactionAsCSV(new FECDataPerLine("AP", "INV", new ZDateTime(2022, 5, 25), ZGuid.Empty, "OH_Code", "OH_FullName", "00000001", new ZDateTime(2022, 5, 26),
					"AP Invoice \r\n0001", -123.45, -12.35, new ZDateTime(2022, 5, 25), "USD", -162.96, 1.2, "*AP*INV*APCtrl*Total", "CST", "A", 0, 1, "00001015")));

			AssertEquals("Transaction with line breaks '\\r'",
				"APINV|AP Invoice|APINV00001015|20220525|FR-8210.00.00|\"FR-8210.00.00 - Description\"|OH_Code|\"OH_FullName\"|00000001|20220526|\"AP Invoice  0001\"|0,00|135,80|||20220525|-162,96|USD",
				fecWriter.CreateTransactionAsCSV(new FECDataPerLine("AP", "INV", new ZDateTime(2022, 5, 25), ZGuid.Empty, "OH_Code", "OH_FullName", "00000001", new ZDateTime(2022, 5, 26),
					"AP Invoice \r0001", -123.45, -12.35, new ZDateTime(2022, 5, 25), "USD", -162.96, 1.2, "*AP*INV*APCtrl*Total", "CST", "A", 0, 1, "00001015")));

			AssertEquals("Transaction with line breaks '\\n'",
				"APINV|AP Invoice|APINV00001015|20220525|FR-8210.00.00|\"FR-8210.00.00 - Description\"|OH_Code|\"OH_FullName\"|00000001|20220526|\"AP Invoice  0001\"|0,00|135,80|||20220525|-162,96|USD",
				fecWriter.CreateTransactionAsCSV(new FECDataPerLine("AP", "INV", new ZDateTime(2022, 5, 25), ZGuid.Empty, "OH_Code", "OH_FullName", "00000001", new ZDateTime(2022, 5, 26),
					"AP Invoice \n0001", -123.45, -12.35, new ZDateTime(2022, 5, 25), "USD", -162.96, 1.2, "*AP*INV*APCtrl*Total", "CST", "A", 0, 1, "00001015")));
		}

		public void TestCreateTransactionAsCSVWithQuotes()
		{
			// test FEC representation of different transaction types
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var apControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			Creator.CreateLocalAccountMappingForGLHeader(apControlAccount, Constants.Languages.French, Constants.CountryCodes.France);

			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 12, 31));
			var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, SharedConstants.Languages.French);
			var fecWriter = new FECFileWriter(complianceReport, complianceReport.ACR_DateFrom, accountMappingHelper);

			AssertEquals("Transaction with double quotation marks",
				"APINV|AP Invoice|APINV00001015|20220525|FR-8210.00.00|\"FR-8210.00.00 - Description\"|OH_Code|\"OH_FullName\"|00000001|20220526|\"AP 'Invoice' 0001\"|0,00|135,80|||20220525|-162,96|USD",
				fecWriter.CreateTransactionAsCSV(new FECDataPerLine("AP", "INV", new ZDateTime(2022, 5, 25), ZGuid.Empty, "OH_Code", "OH_FullName", "00000001", new ZDateTime(2022, 5, 26),
					"AP \"Invoice\" 0001", -123.45, -12.35, new ZDateTime(2022, 5, 25), "USD", -162.96, 1.2, "*AP*INV*APCtrl*Total", "CST", "A", 0, 1, "00001015")));
		}

		public void TestCreateTransactionAsCSVWithLineBreaksAndQuotes()
		{
			// test FEC representation of different transaction types
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var apControlAccount = Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			Creator.CreateLocalAccountMappingForGLHeader(apControlAccount, Constants.Languages.French, Constants.CountryCodes.France);

			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 12, 31));
			var accountMappingHelper = new GLAccountToLocalAccountMapping(complianceReport, SharedConstants.Languages.French);
			var fecWriter = new FECFileWriter(complianceReport, complianceReport.ACR_DateFrom, accountMappingHelper);

			AssertEquals("Transaction with line breaks and double quotation marks",
				"APINV|AP Invoice|APINV00001015|20220525|FR-8210.00.00|\"FR-8210.00.00 - Description\"|OH_Code|\"OH_FullName\"|00000001|20220526|\"AP 'Invoice  0001'\"|0,00|135,80|||20220525|-162,96|USD",
				fecWriter.CreateTransactionAsCSV(new FECDataPerLine("AP", "INV", new ZDateTime(2022, 5, 25), ZGuid.Empty, "OH_Code", "OH_FullName", "00000001", new ZDateTime(2022, 5, 26),
					"AP \"Invoice \r\n0001\"", -123.45, -12.35, new ZDateTime(2022, 5, 25), "USD", -162.96, 1.2, "*AP*INV*APCtrl*Total", "CST", "A", 0, 1, "00001015")));
		}

		public void TestCalculateAmounts()
		{
			// test amount calculation (applying tax and exchanging to foreign currency)
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 12, 31));
			var fecWriter = new FECFileWriter(complianceReport, complianceReport.ACR_DateFrom);

			var zero = "0,00";
			// transaction in local currency
			AssertAmounts("*AP*INV*APCtrl*Total", Constants.CurrencyCodes.France, -100.00, -10.00, -110.00, 1.00, zero, "110,00", string.Empty, string.Empty);
			AssertAmounts("*AP*INV*APSusp*-", Constants.CurrencyCodes.France, -100.00, -10.00, -110.00, 1.00, "100,00", zero, string.Empty, string.Empty);
			AssertAmounts("*AP*INV*GSTIn*-", Constants.CurrencyCodes.France, -100.00, -10.00, -110.00, 1.00, "10,00", zero, string.Empty, string.Empty);
			AssertAmounts("*AP*INV**Rev-", Constants.CurrencyCodes.France, -100.00, -10.00, -110.00, 1.00, "100,00", zero, string.Empty, string.Empty);
			AssertAmounts("*AP*INV*APSusp*Rev", Constants.CurrencyCodes.France, -100.00, -10.00, -110.00, 1.00, zero, "100,00", string.Empty, string.Empty);

			// transaction in foreign currency
			AssertAmounts("*AR*INV*ARCtrl*Total", Constants.CurrencyCodes.UnitedStates, 100.00, 10.00, 165.00, 1.50, "110,00", zero, "165,00", Constants.CurrencyCodes.UnitedStates);
			AssertAmounts("*AR*INV*ARSusp*-", Constants.CurrencyCodes.UnitedStates, 100.00, 10.00, 165, 1.50, zero, "100,00", "-150,00", Constants.CurrencyCodes.UnitedStates);
			AssertAmounts("*AR*INV*GSTOut*-", Constants.CurrencyCodes.UnitedStates, 100.00, 10.00, 165.00, 1.50, zero, "10,00", "-15,00", Constants.CurrencyCodes.UnitedStates);
			AssertAmounts("*AR*INV**Rev-", Constants.CurrencyCodes.UnitedStates, 100.00, 10.00, 165.00, 1.50, zero, "100,00", "-150,00", Constants.CurrencyCodes.UnitedStates);
			AssertAmounts("*AR*INV*ARSusp*Rev", Constants.CurrencyCodes.UnitedStates, 100.00, 10.00, 165.00, 1.50, "100,00", zero, "150,00", Constants.CurrencyCodes.UnitedStates);

			void AssertAmounts(ZString reportSubCode, ZString foreignCurrency, ZDecimal netAmountLocal, ZDecimal gstamountLocal, ZDecimal grossAmountForeign, ZDecimal exchangeRate,
				ZString debitAmountExpected, ZString creditAmountExpected, ZString montantDeviseExpected, ZString ideviseExpected)
			{
				(ZString debitAmount, ZString creditAmount, ZString montantDevise, ZString idevise) = fecWriter.CalculateAmounts(netAmountLocal, gstamountLocal, reportSubCode, lineType: "CST",
					vatBasis: "A", 1.00, foreignCurrency, grossAmountForeign, exchangeRate);
				AssertEquals($"Debit amount for {reportSubCode} in {foreignCurrency}", debitAmountExpected, debitAmount);
				AssertEquals($"Credit amount for {reportSubCode} in {foreignCurrency}", creditAmountExpected, creditAmount);
				AssertEquals($"MontantDevise for {reportSubCode} in {foreignCurrency}", montantDeviseExpected, montantDevise);
				AssertEquals($"Idevise for {reportSubCode} in {foreignCurrency}", ideviseExpected, idevise);
			}
		}

		public void TestGetJournalData()
		{
			// test generation of values for FEC fields "JournalCode", "JournalLib", and "EcritureNum" which depend on ledger type, transaction type,
			// transaction number and registry flags "ShareSequentialInvoiceTransactionNumbers" and "ShareSequentialInvoiceReferenceNumbers"
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 12, 31));
			var fecWriter = new FECFileWriter(complianceReport, complianceReport.ACR_DateFrom);
			var allTransactionTypes = new CodeDescriptionPairList(OLookUpEditType.TransactionTypes);
			var invoicePrintingTransactionTypes = new CodeDescriptionPairList(OLookUpEditType.InvoicePrintingTransactionTypes);

			foreach (ICodeDescription transactionType in allTransactionTypes)
			{
				AssertJournalData("AR", transactionType.Code, "1111", "0011", false, false, $"{(transactionType.Code == "CTR" ? string.Empty : "AR")}{transactionType.Code}",
					$"{(transactionType.Code == "CTR" ? string.Empty : "AR ")}{transactionType.GetMultilingualDescription()}",
					$"{(transactionType.Code == "CTR" ? string.Empty : "AR")}{transactionType.Code}1111");
				AssertJournalData("AP", transactionType.Code, "2222", "0022", false, false, $"{(transactionType.Code == "CTR" ? string.Empty : "AP")}{transactionType.Code}",
					$"{(transactionType.Code == "CTR" ? string.Empty : "AP ")}{transactionType.GetMultilingualDescription()}",
					$"{(transactionType.Code == "CTR" ? string.Empty : "AP")}{transactionType.Code}{(invoicePrintingTransactionTypes.ContainsCode(transactionType.Code) ? "0022" : "2222")}");
				AssertJournalData("GL", transactionType.Code, "3333", "0033", false, false, $"GL{transactionType.Code}", $"GL {transactionType.GetMultilingualDescription()}",
					$"GL{transactionType.Code}3333");
				AssertJournalData("CB", transactionType.Code, "4444", "0044", false, false, $"CB{transactionType.Code}", $"CB {transactionType.GetMultilingualDescription()}",
					$"CB{transactionType.Code}4444");
				AssertJournalData("JC", transactionType.Code, "5555", "0055", false, false, $"JC{transactionType.Code}", $"JC {transactionType.GetMultilingualDescription()}",
					$"JC{transactionType.Code}5555");
			}

			foreach (ICodeDescription transactionType in allTransactionTypes)
			{
				if (invoicePrintingTransactionTypes.ContainsCode(transactionType.Code))
				{
					AssertJournalData("AR", transactionType.Code, "1111", "0011", true, false, "ARINVCRDADJ", "AR Invoice, Credit Note, Adjustment Note", "ARINVCRDADJ1111");
				}
				else
				{
					AssertJournalData("AR", transactionType.Code, "1111", "0011", true, false, $"{(transactionType.Code == "CTR" ? string.Empty : "AR")}{transactionType.Code}",
						$"{(transactionType.Code == "CTR" ? string.Empty : "AR ")}{transactionType.GetMultilingualDescription()}",
						$"{(transactionType.Code == "CTR" ? string.Empty : "AR")}{transactionType.Code}1111");
				}

				AssertJournalData("AP", transactionType.Code, "2222", "0022", true, false, $"{(transactionType.Code == "CTR" ? string.Empty : "AP")}{transactionType.Code}",
					$"{(transactionType.Code == "CTR" ? string.Empty : "AP ")}{transactionType.GetMultilingualDescription()}",
					$"{(transactionType.Code == "CTR" ? string.Empty : "AP")}{transactionType.Code}{(invoicePrintingTransactionTypes.ContainsCode(transactionType.Code) ? "0022" : "2222")}");
				AssertJournalData("GL", transactionType.Code, "3333", "0033", true, false, $"GL{transactionType.Code}", $"GL {transactionType.GetMultilingualDescription()}",
					$"GL{transactionType.Code}3333");
				AssertJournalData("CB", transactionType.Code, "4444", "0044", true, false, $"CB{transactionType.Code}", $"CB {transactionType.GetMultilingualDescription()}",
					$"CB{transactionType.Code}4444");
				AssertJournalData("JC", transactionType.Code, "5555", "0055", true, false, $"JC{transactionType.Code}", $"JC {transactionType.GetMultilingualDescription()}",
					$"JC{transactionType.Code}5555");
			}

			foreach (ICodeDescription transactionType in allTransactionTypes)
			{
				AssertJournalData("AR", transactionType.Code, "1111", "0011", false, true, $"{(transactionType.Code == "CTR" ? string.Empty : "AR")}{transactionType.Code}",
					$"{(transactionType.Code == "CTR" ? string.Empty : "AR ")}{transactionType.GetMultilingualDescription()}",
					$"{(transactionType.Code == "CTR" ? string.Empty : "AR")}{transactionType.Code}1111");
				if (invoicePrintingTransactionTypes.ContainsCode(transactionType.Code))
				{
					AssertJournalData("AP", transactionType.Code, "2222", "0022", false, true, "APINVCRDADJ", "AP Invoice, Credit Note, Adjustment Note", "APINVCRDADJ0022");
				}
				else
				{
					AssertJournalData("AP", transactionType.Code, "2222", "0022", false, true, $"{(transactionType.Code == "CTR" ? string.Empty : "AP")}{transactionType.Code}",
						$"{(transactionType.Code == "CTR" ? string.Empty : "AP ")}{transactionType.GetMultilingualDescription()}",
						$"{(transactionType.Code == "CTR" ? string.Empty : "AP")}{transactionType.Code}2222");
				}

				AssertJournalData("GL", transactionType.Code, "3333", "0033", false, true, $"GL{transactionType.Code}", $"GL {transactionType.GetMultilingualDescription()}",
					$"GL{transactionType.Code}3333");
				AssertJournalData("CB", transactionType.Code, "4444", "0044", false, true, $"CB{transactionType.Code}", $"CB {transactionType.GetMultilingualDescription()}",
					$"CB{transactionType.Code}4444");
				AssertJournalData("JC", transactionType.Code, "5555", "0055", false, true, $"JC{transactionType.Code}", $"JC {transactionType.GetMultilingualDescription()}",
					$"JC{transactionType.Code}5555");
			}

			foreach (ICodeDescription transactionType in allTransactionTypes)
			{
				if (invoicePrintingTransactionTypes.ContainsCode(transactionType.Code))
				{
					AssertJournalData("AR", transactionType.Code, "1111", "0011", true, true, "ARINVCRDADJ", "AR Invoice, Credit Note, Adjustment Note", "ARINVCRDADJ1111");
					AssertJournalData("AP", transactionType.Code, "2222", "0022", true, true, "APINVCRDADJ", "AP Invoice, Credit Note, Adjustment Note", "APINVCRDADJ0022");
				}
				else
				{
					AssertJournalData("AR", transactionType.Code, "1111", "0011", true, true, $"{(transactionType.Code == "CTR" ? string.Empty : "AR")}{transactionType.Code}",
						$"{(transactionType.Code == "CTR" ? string.Empty : "AR ")}{transactionType.GetMultilingualDescription()}",
						$"{(transactionType.Code == "CTR" ? string.Empty : "AR")}{transactionType.Code}1111");
					AssertJournalData("AP", transactionType.Code, "2222", "0022", true, true, $"{(transactionType.Code == "CTR" ? string.Empty : "AP")}{transactionType.Code}",
						$"{(transactionType.Code == "CTR" ? string.Empty : "AP ")}{transactionType.GetMultilingualDescription()}",
						$"{(transactionType.Code == "CTR" ? string.Empty : "AP")}{transactionType.Code}2222");
				}

				AssertJournalData("GL", transactionType.Code, "3333", "0033", true, true, $"GL{transactionType.Code}", $"GL {transactionType.GetMultilingualDescription()}",
					$"GL{transactionType.Code}3333");
				AssertJournalData("CB", transactionType.Code, "4444", "0044", true, true, $"CB{transactionType.Code}", $"CB {transactionType.GetMultilingualDescription()}",
					$"CB{transactionType.Code}4444");
				AssertJournalData("JC", transactionType.Code, "5555", "0055", true, true, $"JC{transactionType.Code}", $"JC {transactionType.GetMultilingualDescription()}",
					$"JC{transactionType.Code}5555");
			}

			void AssertJournalData(ZString ledger, ZString transactionType, ZString transactionNum, ZString internalReference, bool arShareSequentialInvoiceTransactionNumbers,
				bool apShareSequentialInvoiceTransactionNumbers, ZString expectedJournalCode, ZString exptectedJournalLib, ZString expectedEcritureNum)
			{
				var actual = fecWriter.GetJournalData(ledger, transactionType, transactionNum, arShareSequentialInvoiceTransactionNumbers, apShareSequentialInvoiceTransactionNumbers,
					internalReference);
				AssertEquals(
					$"Journal Code for {ledger} {transactionType} #{transactionNum} with ARShare:{arShareSequentialInvoiceTransactionNumbers} and APShare:{apShareSequentialInvoiceTransactionNumbers}",
					expectedJournalCode, actual.journalCode);
				AssertEquals(
					$"Journal Lib for {ledger} {transactionType} #{transactionNum} with ARShare:{arShareSequentialInvoiceTransactionNumbers} and APShare:{apShareSequentialInvoiceTransactionNumbers}",
					exptectedJournalLib, actual.journalLib);
				AssertEquals(
					$"Ecriture Num for {ledger} {transactionType} #{transactionNum} with ARShare:{arShareSequentialInvoiceTransactionNumbers} and APShare:{apShareSequentialInvoiceTransactionNumbers}",
					expectedEcritureNum, actual.ecritureNum);
			}
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestReportGenerationWithMissingLocalMapping()
		{
			// test purpose is to test if the FEC report fails with failing local mapping
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			Db.Connection.BeginTransaction();
			var complianceReport = CreateFECReport(new ZDate(2022, 11, 1), new ZDate(2022, 11, 30));

			using var disposableService = complianceReport.Factory.AddDisposableService();
			complianceReport.Company.OrgProxy.CustomsCodes.AddNew("TVA", "FR123456789", "FR");
			var serviceLogger = new DummyLogger();

			(_, var apInvoiceLine, _) = CreateAPInvoice(Creator.ABIGAS, "12345678", Creator.USD, 1.5m);
			apInvoiceLine.AL_AG = Creator.CreateGLHeader("1111.11.11", "P&L").PK;

			var retainedEarningsAccount = Creator.CreateRetainedEarningsAccount();
			Creator.CreateLocalAccountMappingForGLHeader(retainedEarningsAccount, Constants.Languages.French, Constants.CountryCodes.France);

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV**Rev-", null, apInvoiceLine); // uses GL account of transaction line

			Db.Connection.CommitTransaction();

			complianceReport.GenerateFECFromQueue(serviceLogger, null);

			var reloadedComplianceReport = ReloadFECReport(complianceReport);

			var notes = reloadedComplianceReport.GetNotes().GetAllNotes();
			AssertEquals("Number of notes attached to report", 1, notes.Count);
			var docManager = ((IDocManagerSupport)reloadedComplianceReport).DocManagerInfo;
			AssertEquals("No eDoc were added", 0, docManager.AllEDocs.Count);

			AssertEquals("Report status", "ERR", reloadedComplianceReport.ACR_Status);
		}
		#endregion

		#region FECDataProvider tests

		public void TestOpenTransactionsDbCommand()
		{
			// test if the DB command used to retrieve the FEC relevant transactions includes the columns that we need for report generation
			var complianceReport = CreateFECReport(new ZDate(2022, 1, 1), new ZDate(2022, 1, 31));
			var fecDataProvider = new FECDataProvider(complianceReport);

			using (var dbCommand = fecDataProvider.OpenTransactionsDbCommand(complianceReport.ACR_DateFrom, complianceReport.ACR_DateTo.AddDays(1), ((IDbConnected)Factory).Connection))
			using (var dataReader = dbCommand.ExecuteReader())
			{
				var columnList = new ZStringBuilder();
				for (var columnNumber = 0; columnNumber < dataReader.FieldCount; columnNumber++)
				{
					columnList.Append(dataReader.GetName(columnNumber));
				}

				AssertEquals("Transactions DB command column list",
					"AH_Ledger,AH_TransactionType,EcritureDate,CompAuxNum,CompAuxLib,NetAmountLocal,ValidDate,TaxAmountLocal,ACQ_ReportSubCode,LineType,VATBasis,InputVatRecoverable,AccountPK,AccountNum,PieceDate,PieceRef,EcritureLib,GrossAmountForeign,IDevise,ExchangeRate,AL_Sequence,AL_PK,InternalReference,EcritureLet,DateLet",
					columnList.ToStringWithDelimiterBetweenAppends(","));
			}
		}

		public void TestTransactionsRetrievalOrder()
		{
			// test if the transaction are retrieved in the correct order
			// French fiscal authorities requires them in chronological order
			// for better readability we also sorted them by ledger, transaction type, line type, sequence number, and report-subcode
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France);
			var complianceReport = CreateFECReport(new ZDate(2022, 9, 1), new ZDate(2022, 12, 31));
			var fecDataProvider = new FECDataProvider(complianceReport);

			// create an AP invoice
			var (_, apInvoiceLine1, apInvoiceLine2) = CreateAPInvoice(Creator.ABIGAS, "12345678", Creator.USD, 1.5m);
			var (_, apInvoiceLine3, apInvoiceLine4) = CreateAPInvoice(Creator.Creditor1, "12345679", Creator.USD, 1.5m);
			apInvoiceLine4.AL_AG = apInvoiceLine3.AL_AG = apInvoiceLine2.AL_AG =
				apInvoiceLine1.AL_AG = Creator.CreateGLAccountAndLocalAccountMapping("1111.11.11", Constants.Languages.French, Constants.CountryCodes.France, "P&L").PK;

			var (_, arInvoiceLine1, arInvoiceLine2) = CreateARInvoice(new DateTime(2022, 11, 25));
			arInvoiceLine1.AL_AG = arInvoiceLine2.AL_AG = Creator.CreateGLAccountAndLocalAccountMapping("2222.22.22", Constants.Languages.French, Constants.CountryCodes.France, "P&L").PK;

			// create a WIP with a PostDate and a ReverseDate outside the report date range. Reverse Date is always set to the current date somewhere in saving process. (OnFactorySaving) 
			var (wip1, wip2) = CreateShipmentJobAndWip("S001000", postDate: new DateTime(2023, 01, 11), false);
			wip1.AL_AG = wip2.AL_AG = apInvoiceLine1.AL_AG;

			// create another WIP with reverse date inside the report date range and post date outside the date range.
			// GUIDs are passed to guarantee the correct sort order.
			var (wip3, wip4) = CreateShipmentJobAndWip("S002000", postDate: new DateTime(2022, 08, 25), reverse: true, reverseDate: new DateTime(2022, 09, 11),
				new Guid("61380BB1-35EC-4E95-BCD2-DB67080745EB"), new Guid("990BB7B1-A7F6-42E6-B59C-4DB288E90B09"));
			wip3.AL_AG = wip4.AL_AG = apInvoiceLine1.AL_AG;

			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*APCtrl*Total", null, apInvoiceLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*APSusp*-", null, apInvoiceLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*GSTIn*-", null, apInvoiceLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV**Rev-", null, apInvoiceLine1); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*APSusp*Rev", null, apInvoiceLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*APSusp*-", null, apInvoiceLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV**Rev-", null, apInvoiceLine2); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*APSusp*Rev", null, apInvoiceLine2);

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*APCtrl*Total", null, apInvoiceLine3);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*APSusp*-", null, apInvoiceLine3);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*GSTIn*-", null, apInvoiceLine3);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV**Rev-", null, apInvoiceLine3); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*APSusp*Rev", null, apInvoiceLine3);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*APSusp*-", null, apInvoiceLine4);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV**Rev-", null, apInvoiceLine4); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AP*INV*APSusp*Rev", null, apInvoiceLine4);

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV*ARCtrl*Total", null, arInvoiceLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV*ARSusp*-", null, arInvoiceLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV*PenGSTOut*-", null, arInvoiceLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV**Rev-", null, arInvoiceLine1); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV*ARSusp*Rev", null, arInvoiceLine1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV*ARSusp*-", null, arInvoiceLine2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV**Rev-", null, arInvoiceLine2); // uses GL account of transaction line
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*AR*INV*ARSusp*Rev", null, arInvoiceLine2);

			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP**", null, wip1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP*WIPCtrl*-", null, wip1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP**Rev-", wip1.AL_ReverseDate, wip1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP*WIPCtrl*Rev", wip1.AL_ReverseDate, wip1);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP**", null, wip2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP*WIPCtrl*-", null, wip2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP**Rev-", wip2.AL_ReverseDate, wip2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP*WIPCtrl*Rev", wip2.AL_ReverseDate, wip2);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP**", null, wip3);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP*WIPCtrl*-", null, wip3);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP**Rev-", wip3.AL_ReverseDate, wip3);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP*WIPCtrl*Rev", wip3.AL_ReverseDate, wip3);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP**", null, wip4);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP*WIPCtrl*-", null, wip4);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP**Rev-", wip4.AL_ReverseDate, wip4);
			Creator.CreateComplianceReportQueueEntry(complianceReport, "*JC*WIP*WIPCtrl*Rev", wip4.AL_ReverseDate, wip4);

			var actualData = new ZStringBuilder();
			using (var dbCommand = fecDataProvider.OpenTransactionsDbCommand(complianceReport.ACR_DateFrom, complianceReport.ACR_DateTo.AddDays(1), ((IDbConnected)Factory).Connection))
			using (var dataReader = dbCommand.ExecuteReader())
			{
				while (dataReader.Read())
				{
					var dataPerLine = new FECDataPerLine(dataReader);
					actualData.AppendLine($"{dataPerLine.PostDate:yyyyMMdd},{dataPerLine.Ledger},{dataPerLine.TransactionType},{dataPerLine.InternalReference}," +
										  $"{dataPerLine.PieceRef},{dataPerLine.LineType},{dataPerLine.SequenceNumber},{dataPerLine.EcritureLib},{dataPerLine.NetAmountLocal}," +
										  $"{dataPerLine.TaxAmountLocal},{dataPerLine.ReportSubCode},{dataPerLine.ValidDate:yyyyMMdd},{dataPerLine.PieceDate:yyyyMMdd}");
				}
			}

			var expectedData = GetEmbeddedResourceAsZString("fec2022OrderBy.csv");
			AssertMultilineASCIIEquals("Order By", expectedData, actualData.ToString());

			// The last empty line in the expected Data file cannot be eliminated using VS. It is always added again when saving the file.
			// Therefore, subtract 1 from the counter.
			AssertEquals("Number of rows", 28, expectedData.Split("\n").Length - 1);
		}

		public void TestRetrieveOpenBalances()
		{
			// test correctness of retrieved open balances
			var complianceReport = CreateFECReport(new ZDate(2022, 5, 1), new ZDate(2022, 12, 31));

			var (accountPK1, accountPK2, accountPK3, accountPK4) = CreateGLAggregates(complianceReport.Company.PK);
			var germanyCompany = Creator.CreateCompanyAndBranch("DEHAM");
			var (accountPK5, accountPK6, accountPK7, accountPK8) = CreateGLAggregates(germanyCompany.PK);
			var retainedEarningsAccount = Creator.CreateRetainedEarningsAccount();
			Creator.CreateLocalAccountMappingForGLHeader(retainedEarningsAccount, Constants.Languages.French, Constants.CountryCodes.France);

			Factory.Save();

			var fecDataProvider = new FECDataProvider(complianceReport);
			var result = fecDataProvider.RetrieveOpenBalances();

			AssertEquals("Number of OB accounts", 3, result.Count);
			var amount = result.FirstOrDefault(x => x.AccountPK == accountPK1).Balance;
			AssertEquals("OB of accountPK1", (decimal)25.05, amount);
			AssertEquals("OB of accountPK2 exists", false, result.Select(x => x.AccountPK).Contains(accountPK2));
			AssertEquals("OB of accountPK3 exists", false, result.Select(x => x.AccountPK).Contains(accountPK3));
			amount = result.FirstOrDefault(x => x.AccountPK == accountPK4).Balance;
			AssertEquals("OB of accountPK4", (decimal)177.77, amount);

			amount = result.FirstOrDefault(x => x.AccountPK == retainedEarningsAccount.PK).Balance;
			AssertEquals("OB of retainedEarningsAccount", (decimal)234.16, amount);

			AssertEquals("OB of accountPK5 (DE) exists", false, result.Select(x => x.AccountPK).Contains(accountPK5));
			AssertEquals("OB of accountPK6 (DE) exists", false, result.Select(x => x.AccountPK).Contains(accountPK6));
			AssertEquals("OB of accountPK7 (DE) exists", false, result.Select(x => x.AccountPK).Contains(accountPK7));
			AssertEquals("OB of accountPK8 (DE) exists", false, result.Select(x => x.AccountPK).Contains(accountPK8));

			// amounts sum up to zero and shall be excluded
			var glAccount = Creator.CreateGLAccountAndLocalAccountMapping("2222.11.11", Constants.Languages.French, Constants.CountryCodes.France, "BSH");
			CreateAccGLAggregate(glAccount.PK, 200, 202105, complianceReport.Company.PK);
			CreateAccGLAggregate(glAccount.PK, -200, 202107, complianceReport.Company.PK);
			// amounts for retained earnings now sum up to zero and shall be excluded
			CreateAccGLAggregate(accountPK2, -234.16, 202103, complianceReport.Company.PK);
			Factory.Save();

			result = fecDataProvider.RetrieveOpenBalances();
			AssertEquals("Number of OB accounts", 2, result.Count);
			amount = result.FirstOrDefault(x => x.AccountPK == accountPK1).Balance;
			AssertEquals("OB of accountPK1", (decimal)25.05, amount);
			AssertEquals("OB of glAccount 2222.11.11 exists", false, result.Select(x => x.AccountPK).Contains(glAccount.PK));
			AssertEquals("OB of retained earnings account exists", false, result.Select(x => x.AccountPK).Contains(retainedEarningsAccount.PK));
			amount = result.FirstOrDefault(x => x.AccountPK == accountPK4).Balance;
			AssertEquals("OB of accountPK4", (decimal)177.77, amount);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			CreateFECReportConfiguration();
			base.SetUp();
		}

		(APInvoice header, InvoicingLineBase line1, InvoicingLineBase line2) CreateAPInvoice(OrgHeader creditor, ZString transactionNumber, RefCurrency currency, decimal exRate)
		{
			var aPInvoice = Creator.CreateAPInvoice<APInvoice>(transactionNumber, currency, exRate, 240m * exRate, 24m * exRate, 0m, 240m, 24m, 0m, creditor);
			var postDate = new DateTime(2022, 11, 25);
			aPInvoice.AH_PostDate = postDate;
			aPInvoice.AH_InvoiceDate = postDate;
			var apInvoiceLine1 = aPInvoice.Lines[0];
			apInvoiceLine1.AL_AT = Creator.GST1.PK;
			var apInvoiceLine2 = Creator.CreateInvoiceLine(aPInvoice, currency, exRate, 200m * exRate, 0m, 0m);

			return (aPInvoice, apInvoiceLine1, apInvoiceLine2);
		}

		InvoicingLineBase CreateZeroAPInvoice()
		{
			var apInvoice = Creator.CreateAPInvoice<APInvoice>("321654987", Creator.USD, 1.5m, 360m, 36m, 0m, 0m, 24m, 0m, Creator.ABIGAS);
			var postDate = new DateTime(2022, 11, 25);
			apInvoice.AH_PostDate = postDate;
			apInvoice.AH_InvoiceDate = postDate;
			var apInvoiceLine1 = apInvoice.Lines[0];
			apInvoiceLine1.AL_AT = Creator.GST1.PK;
			apInvoiceLine1.AL_AG = Creator.CreateGLAccountAndLocalAccountMapping("1111.11.11", Constants.Languages.French, Constants.CountryCodes.France, "P&L").PK;

			return apInvoiceLine1;
		}

		(ARInvoice, InvoicingLineBase line1, InvoicingLineBase line2) CreateARInvoice(ZDateTime postDate)
		{
			// add an AR invoice with a different date and 2 positions
			var arInvoice = Creator.CreateARInvoice<ARInvoice>("987654321", Creator.EUR, 1m, Creator.Debtor);
			arInvoice.AH_PostDate = postDate;
			arInvoice.AH_InvoiceDate = postDate;
			var arInvoiceLine1 = Creator.CreateInvoiceLine(arInvoice, Creator.EUR, 1m, 100m, 10m, 0m);
			arInvoiceLine1.AL_AT = Creator.GST1.PK;
			var arInvoiceLine2 = Creator.CreateInvoiceLine(arInvoice, Creator.EUR, 1m, 200m, 0m, 0m);
			return (arInvoice, arInvoiceLine1, arInvoiceLine2);
		}

		Contra CreateContra(ZDateTime postDate)
		{
			// create an AP contra
			var contra = Creator.CreateContra(100m, postDate, Creator.Debtor.PK, Creator.ABIGAS.PK);
			contra.APRow.AH_PostDate = postDate;
			contra.APRow.AH_InvoiceDate = postDate;
			contra.APRow.AH_Desc = "AP CONTRA";

			contra.ARRow.AH_PostDate = postDate;
			contra.ARRow.AH_InvoiceDate = postDate;
			contra.ARRow.AH_Desc = "RECEIVABLE AND PAYABLE CONTRA";

			return contra;
		}

		void CreateFECReportConfiguration()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "FEC";
			reportConfig.ReportTitle = "Test Compliance Report";
			reportConfig.Country = "FR";
			reportConfig.TaxRegistrationType = "TVA";
			reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
			reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBook;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);
		}

		(ZGuid accountPK1, ZGuid accountPK2, ZGuid accountPK3, ZGuid accountPK4) CreateGLAggregates(ZGuid companyPK)
		{
			var account1 = Factory.NewWithValidTestData<AccGLHeader>();
			account1.AG_AccountType = "BSH";
			var account2 = Factory.NewWithValidTestData<AccGLHeader>();
			account2.AG_AccountType = "P&L";
			var account3 = Factory.NewWithValidTestData<AccGLHeader>();
			account3.AG_AccountType = "P&L";
			var account4 = Factory.NewWithValidTestData<AccGLHeader>();
			account4.AG_AccountType = "BSH";
			CreateAccGLAggregate(account2.PK, 200.05, 202202, companyPK); // Feb
			CreateAccGLAggregate(account1.PK, 100.05, 202103, companyPK); // Mar
			CreateAccGLAggregate(account4.PK, 77.77, 202103, companyPK);
			CreateAccGLAggregate(account1.PK, -75, 202204, companyPK); // Apr
			CreateAccGLAggregate(account2.PK, -77, 202204, companyPK);
			CreateAccGLAggregate(account4.PK, 100, 202104, companyPK);
			CreateAccGLAggregate(account3.PK, 111.11, 202204, companyPK);
			CreateAccGLAggregate(account1.PK, 20.1, 202205, companyPK); // May
			CreateAccGLAggregate(account3.PK, 33.3, 202206, companyPK); // June
			CreateAccGLAggregate(account1.PK, -44, 202207, companyPK); // July
			CreateAccGLAggregate(account4.PK, -10, 202207, companyPK);
			return (account1.PK, account2.PK, account3.PK, account4.PK);
		}

		void CreateAccGLAggregate(ZGuid accountPK, ZDecimal amount, int period, ZGuid companyPK)
		{
			var glAggregate = Factory.NewWithValidTestData<AccGLAggregate>();
			glAggregate.AA_AG = accountPK;
			glAggregate.AA_Amount = amount;
			glAggregate.AA_Period = period;
			glAggregate.AA_GC = companyPK;
			return;
		}

		(WIP wip1, WIP wip2) CreateShipmentJobAndWip(string shipmentNumber, DateTime postDate, bool reverse, ZDateTime? reverseDate = null, Guid? pkWip1 = null, Guid? pkWip2 = null)
		{
			// Insert a Shipment -> Job -> WIP
			var shipment1 = Creator.CreateShipment(shipmentNumber, false);
			var job1 = Creator.CreateJob(shipment1, false, false);

			var wip1 = Creator.CreateWIP(job1, pkWip1 ?? new Guid());
			wip1.AL_PostDate = postDate;
			wip1.AL_LineAmount = 111m;
			wip1.AL_AT = Creator.GST1.PK;
			wip1.AL_GSTVAT = 11.10m;
			wip1.AL_OSAmount = 122.10m;
			wip1.AL_Desc = "Pallet Rental";

			var wip2 = Creator.CreateWIP(job1, pkWip2 ?? new Guid());
			wip2.AL_PostDate = postDate;
			wip2.AL_LineAmount = 222m;
			wip2.AL_AT = Creator.GST1.PK;
			wip2.AL_GSTVAT = 22.20m;
			wip2.AL_OSAmount = 244.20m;
			wip2.AL_Desc = "Redelivery";

			if (reverse && reverseDate != null)
			{
				wip1.Reverse();
				wip1.AL_ReverseDate = reverseDate ?? ZDateTime.UtcNow;
				wip2.Reverse();
				wip2.AL_ReverseDate = reverseDate ?? ZDateTime.UtcNow;
			}

			return (wip1, wip2);
		}

		AccComplianceReport CreateFECReport(ZDate dateFrom, ZDate dateTo)
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.FEC;
			complianceReport.ACR_DateFrom = dateFrom;
			complianceReport.ACR_DateTo = dateTo;
			Creator.CreateTestPeriods(new ZDateTime(dateFrom.Year, 1, 1));
			return complianceReport;
		}

		AccComplianceReport ReloadFECReport(AccComplianceReport originalReport)
		{
			var query = new ZQuery(AccComplianceReportSchema.PK, originalReport.PK);

			return originalReport.Factory.LoadTop1<AccComplianceReport>(query);
		}

		static readonly Random random = new Random();

		static string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());
		}

		ZString GetEmbeddedResourceAsZString(string filename)
		{
			ZString filecontent = ZString.Empty;
			using (Stream stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Accounting.Business.Testing.ComplianceReport.FEC.Testfiles." + filename))
			using (StreamReader sr = new StreamReader(stream))
			{
				filecontent = sr.ReadToEnd();
			}

			return filecontent;
		}

		#endregion
	}
}
