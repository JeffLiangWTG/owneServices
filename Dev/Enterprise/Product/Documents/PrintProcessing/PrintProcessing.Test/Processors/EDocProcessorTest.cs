using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.FlexCelInterface.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineCore.Registry.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class EDocProcessorTest : MergedPrintGroupProcessorTestCase
	{
		#region estCopyToDocManagerForStmReport

		public void TestCopyToDocManagerForStmReport_MultipleStmPrintJobsWithSameFileNameDifferentSubjectLine()
		{
			CreateDocTypeForStmReportRun("AAA");

			using (var file = TempFile.NewWithExtension("CSV"))
			{
				File.AppendAllText(file.Filename, "Test");

				var reportRun = Factory.NewWithValidTestData<StmReportRun>();

				var printJob1 = CreateStmPrintJobForStmReportRun(reportRun, "Test Jerry 1 (CSV)", "CSV", file.Filename);
				var printJob2 = CreateStmPrintJobForStmReportRun(reportRun, "Test Jerry 2 (CSV)", "CSV", file.Filename);
				var printJob3 = CreateStmPrintJobForStmReportRun(reportRun, "Test Jerry 3 (CSV)", "CSV", file.Filename);

				Factory.Save();
				var processor = new EDocProcessorForTesting(null);
				processor.CopyToDocManagerAndSaveForTesting(printJob1);
				processor.CopyToDocManagerAndSaveForTesting(printJob2);
				processor.CopyToDocManagerAndSaveForTesting(printJob3);

				var eDocs = reportRun.DocManagerInfo().EDocsView;
				AssertEquals("Should have 3 file", 3, eDocs.Count);

				AssertEquals("Test Jerry 1 (CSV)", eDocs[0].Description);
				AssertEquals("Test.csv", eDocs[0].FileName);

				AssertEquals("Test Jerry 2 (CSV)", eDocs[1].Description);
				AssertEquals("Test[2].csv", eDocs[1].FileName);

				AssertEquals("Test Jerry 3 (CSV)", eDocs[2].Description);
				AssertEquals("Test[3].csv", eDocs[2].FileName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerForStmReport_CSVWithOtherFiles()
		{
			CreateDocTypeForStmReportRun("AAA");

			using (var file = TempFile.NewWithExtension("CSV"))
			{
				File.AppendAllText(file.Filename, "Test");

				var reportRun = Factory.NewWithValidTestData<StmReportRun>();

				var printJob1 = CreateStmPrintJobForStmReportRun(reportRun, "CSV", file.Filename);
				var printJob2 = CreateStmPrintJobForStmReportRun(reportRun, "XLS", PrintProcessingConstants.Small);
				var printJob3 = CreateStmPrintJobForStmReportRun(reportRun, "PDF", PrintProcessingConstants.Small);
				var printJob4 = CreateStmPrintJobForStmReportRun(reportRun, "PDF", PrintProcessingConstants.Small);

				Factory.Save();
				var processor = new EDocProcessorForTesting(null);
				processor.CopyToDocManagerAndSaveForTesting(printJob1);
				processor.CopyToDocManagerAndSaveForTesting(printJob2);
				processor.CopyToDocManagerAndSaveForTesting(printJob3);
				processor.CopyToDocManagerAndSaveForTesting(printJob4);

				var eDocs = reportRun.DocManagerInfo().EDocsView;
				AssertEquals("Should have 3 file", 3, eDocs.Count);
				AssertEquals("Test (CSV)", eDocs[0].Description);
				AssertEquals("Test (XLS)", eDocs[1].Description);
				AssertEquals("Test (PDF)", eDocs[2].Description);
			}
		}

		public void TestCopyToDocManagerForJobSubmittedBy()
		{
			CreateDocTypeForStmReportRun("AAA");

			using (var file = TempFile.NewWithExtension("CSV"))
			{
				var user = Factory.NewWithValidTestData<GlbStaff>();
				user.GS_Code = "AA";
				user.GS_LoginName = "AALogin";
				Factory.Save();

				File.AppendAllText(file.Filename, "Test");

				var reportRun = Factory.NewWithValidTestData<StmReportRun>();
				var printJob1 = CreateStmPrintJobForStmReportRun(reportRun, "CSV", file.Filename);
				printJob1.SP_GS_NKJobSubmittedBy = user.GS_Code;
				Factory.Save();

				var processor = new EDocProcessorForTesting(null);
				processor.CopyToDocManagerAndSaveForTesting(printJob1);

				var eDocs = reportRun.DocManagerInfo().EDocsView;
				AssertEquals("The last edit user should be the user in JobSubmittedBy", user.GS_Code, (eDocs[0] as IStorageFile).LastEditedUser);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerForStmReport_XLSFile() => AssertCopyToDocManagerForStmReportRun("XLS");

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerForStmReport_TIFFile() => AssertCopyToDocManagerForStmReportRun("TIF");

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerForStmReport_PDFFile() => AssertCopyToDocManagerForStmReportRun("PDF");

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerForStmReport_XMLFile() => AssertCopyToDocManagerForStmReportRun("XML");

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerForStmReport_HTMLFile() => AssertCopyToDocManagerForStmReportRun("HTML");

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerForStmReport_HTMFFile() => AssertCopyToDocManagerForStmReportRun("HTMF");

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerForStmReport_TXTFile() => AssertCopyToDocManagerForStmReportRun("TXT");

		void AssertCopyToDocManagerForStmReportRun(string formatType)
		{
			CreateDocTypeForStmReportRun("AAA");

			var reportRun = Factory.NewWithValidTestData<StmReportRun>();

			var printJob = CreateStmPrintJobForStmReportRun(reportRun, formatType, PrintProcessingConstants.Small);
			Factory.Save();

			var processor = new EDocProcessorForTesting(null);
			processor.CopyToDocManagerAndSaveForTesting(printJob);

			var eDocs = reportRun.DocManagerInfo().EDocsView;
			AssertEquals("Should have 1 file", 1, eDocs.Count);
			AssertEquals("Test", eDocs[0].FileNameOnly);
			AssertEquals($"Test ({formatType})", eDocs[0].Description);
			AssertEquals("AAA", eDocs[0].DocType);
		}

		StmPrintJob CreateStmPrintJobForStmReportRun(StmReportRun reportRun, string formatType, string filePath) => CreateStmPrintJobForStmReportRun(reportRun, $"Test ({formatType})", formatType, filePath);

		StmPrintJob CreateStmPrintJobForStmReportRun(StmReportRun reportRun, string subjectLine, string formatType, string filePath)
		{
			var extension = formatType == "CSV" || formatType == "XML" || formatType == "TXT" ? formatType : "XLS";
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "DDS";
			printJob.SP_DocumentName = "Test";
			printJob.SP_CustomProperties = File.ReadAllBytes(filePath);
			printJob.SP_EmailAttachmentFormat = formatType;
			printJob.SP_EmailAttachments = $"Test.{extension}";
			printJob.SP_EmailSubjectLine = subjectLine;
			printJob.SP_ParentTableName = "StmReportRun";
			printJob.SP_ParentGuid = reportRun.PK;
			printJob.SP_RelatedBusinessContext = "RTS";
			printJob.SP_DocumentType = "AAA";
			printJob.SP_RunDateTime = ZDateTime.Now;
			printJob.SP_SB_DeliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory).PK;
			return printJob;
		}

		void CreateDocTypeForStmReportRun(string type)
		{
			var refDocType = Factory.New<RefDocType>();
			refDocType.RT_SE_NKDocumentReceivedEvent = Events.ExportReceivalAdvisePrinted.Code;
			refDocType.RT_DocType = type;
			refDocType.RT_ReferenceType = "ALL";
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManager()
		{
			AssertCopyToDocManager("test.XLS");

			AssertEquals("LogProgress messages", "Document 'Subject' of type 'CIV' was allocated, but not registered with business object.", loggedMessages[1]);
			AssertEquals("LogProgress messages", "Finished allocating document \"Test\" of type \"CIV\"", loggedMessages[2]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerXLSX()
		{
			AssertCopyToDocManager("test.XLSX");

			AssertEquals("LogProgress messages", "Document 'Subject' of type 'CIV' was allocated, but not registered with business object.", loggedMessages[1]);
			AssertEquals("LogProgress messages", "Finished allocating document \"Test\" of type \"CIV\"", loggedMessages[2]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerPdf()
		{
			AssertCopyToDocManager("test.PDF");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerWithInvalidFileName()
		{
			AssertCopyToDocManager("test|<>?*_file.XLSX");
			AssertEquals("LogProgress messages", "Document 'Subject' of type 'CIV' was allocated, but not registered with business object.", loggedMessages[1]);
			AssertEquals("LogProgress messages", "Finished allocating document \"Test\" of type \"CIV\"", loggedMessages[2]);
		}

		void AssertCopyToDocManager(string emailAttachments)
		{
			var testParentGuid = ZGuid.NewZGuid();
			var printJob = CreatePrintJob(testParentGuid, File.ReadAllBytes(PrintProcessingConstants.Small), emailAttachments: emailAttachments);
			Factory.Save();

			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var queryForShipment = new ZQuery(StorageMainSchema.SM_ParentFK, testParentGuid);
			var matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Precondition: Should not be any docs for this parent", 0, matchingStorageMains.Length);

			var processor = new EDocProcessorForTesting(null);
			processor.CopyToDocManagerAndSaveForTesting(printJob);

			matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Should have created a parent", 1, matchingStorageMains.Length);
			AssertEquals("Parent should have a reference to the Shipment Parent Guid", testParentGuid, matchingStorageMains[0][StorageMainSchema.Constants.SM_ParentFK]);

			int databaseNumber = (ZInt)matchingStorageMains[0][StorageMainSchema.SM_DB.Name];
			BusinessObjectFactory factoryOne = ((IDocumentFactory)documentFactory).GetFactory(databaseNumber);
			var parentQuery = new ZQuery(StorageDocsSchema.SC_SM, matchingStorageMains[0].PK);
			var matchingStorageDocs = (BusinessObject[])factoryOne.Load<IStorageDocs>(parentQuery);
			AssertEquals("Should have created one document", 1, matchingStorageDocs.Length);
			AssertEquals("Should have document type", "CIV", matchingStorageDocs[0][StorageDocsSchema.SC_DocType.Name]);
			AssertEquals(Path.GetFileNameWithoutExtension(PathValidation.GetSafeFilename(emailAttachments)), matchingStorageDocs[0][StorageDocsSchema.SC_FileName.Name]);
		}

		public void TestQuotationDefaultDocType()
		{
			using (var file = TempFile.NewWithExtension("CSV"))
			{
				File.AppendAllText(file.Filename, "Test");

				var reportRun = Factory.NewWithValidTestData<StmReportRun>();

				var printJob1 = CreateStmPrintJobForStmReportRun(reportRun, "Test Quotation", "CSV", file.Filename);
				printJob1.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.Quotation;

				Factory.Save();
				var processor = new EDocProcessorForTesting(null);
				processor.CopyToDocManagerAndSaveForTesting(printJob1);
								
				var eDocs = reportRun.DocManagerInfo().EDocsView.Cast<IeDocBase>().ToList();
				AssertContainsExactElementsInAnyOrder
				(
					new[] { "Test Quotation - SQTE" },
					eDocs.Select(eDoc => $"{eDoc.Description} - {eDoc.DocType}")
				);
			}
		}

		public void TestOneOffQuoteDefaultDocType()
		{
			using (var file = TempFile.NewWithExtension("CSV"))
			{
				File.AppendAllText(file.Filename, "Test");

				var reportRun = Factory.NewWithValidTestData<StmReportRun>();

				var printJob1 = CreateStmPrintJobForStmReportRun(reportRun, "Test Quotation", "CSV", file.Filename);
				printJob1.SP_DocumentType = Core.Constants.RefDocTypes.Quotation;
				printJob1.SP_RelatedBusinessContext = Core.Constants.DocManagerCodes.OneOffQuote;

				Factory.Save();
				var processor = new EDocProcessorForTesting(null);
				processor.CopyToDocManagerAndSaveForTesting(printJob1);

				var eDocs = reportRun.DocManagerInfo().EDocsView.Cast<IeDocBase>().ToList();
				AssertContainsExactElementsInAnyOrder
				(
					new[] { "Test Quotation - QUO" },
					eDocs.Select(eDoc => $"{eDoc.Description} - {eDoc.DocType}")
				);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManager_XLSFile_ShouldCopyAsPDFByDefault()
		{
			CopyToDocManagerAndAssertExpectedFileFormat(Core.Constants.FileFormats.PDF);

			AssertEquals("LogProgress messages", "Document 'Subject' of type 'CIV' was allocated, but not registered with business object.", loggedMessages[1]);
			AssertEquals("LogProgress messages", "Finished allocating document \"Test\" of type \"CIV\"", loggedMessages[2]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManager_XLSFile_OverridenRegistry_ShouldCopyAsTIF()
		{
			SystemDataRegistry.Instance.EDocImportFileFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.FileFormats.TIF);
			CopyToDocManagerAndAssertExpectedFileFormat(Core.Constants.FileFormats.TIF);

			AssertEquals("LogProgress messages", "Document 'Subject' of type 'CIV' was allocated, but not registered with business object.", loggedMessages[1]);
			AssertEquals("LogProgress messages", "Finished allocating document \"Test\" of type \"CIV\"", loggedMessages[2]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManager_XLSFile_OverridenRegistry_ShouldCopyAsPDFA()
		{
			var fileExtension = Core.Constants.FileFormats.PDF; // PDFA documents are just a different type of pdf, they're still saved as a .pdf
			SystemDataRegistry.Instance.EDocImportFileFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.FileFormats.PDFA);
			CopyToDocManagerAndAssertExpectedFileFormat(fileExtension);

			AssertEquals("LogProgress messages", "Document 'Subject' of type 'CIV' was allocated, but not registered with business object.", loggedMessages[1]);
			AssertEquals("LogProgress messages", "Finished allocating document \"Test\" of type \"CIV\"", loggedMessages[2]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManager_XLSFile_OverridenRegistry_ShouldSignPdfIfNeeded()
		{
			byte[] data = null;
			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(Path.Combine(UnitTestingConstants.TestDocumentDigitalSignatureFilePath, "TestSignXlsDocument.xls"));
				using (var ms = new MemoryStream())
				{
					xlInterface.SaveToStream(ms);
					data = ms.ToArray();
				}
			}

			var testParentGuid = ZGuid.NewZGuid();
			var printJob = CreatePrintJob(testParentGuid, data);
			printJob.SP_SignBy = DocumentsSignBy.PFX;
			Factory.Save();

			var documentFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
			var queryForShipment = new ZQuery(StorageMainSchema.SM_ParentFK, testParentGuid);
			var matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Precondition: Should not be any docs for this parent", 0, matchingStorageMains.Length);

			DocumentsDataRegistry.Instance.DigitalSignature.SetValue(Guid.Empty,
				Guid.Empty,
				Guid.Empty,
				DigitalSignatureTestHelper.GetDigitalSignatureRegistry()
			);

			var processor = new EDocProcessorForTesting(null);
			processor.CopyToDocManagerAndSaveForTesting(printJob);

			matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Should have created a parent", 1, matchingStorageMains.Length);
			AssertEquals("Parent should have a reference to the Shipment Parent Guid", testParentGuid, matchingStorageMains[0][StorageMainSchema.Constants.SM_ParentFK]);

			int databaseNumber = (ZInt)matchingStorageMains[0][StorageMainSchema.SM_DB.Name];
			var factoryOne = ((IDocumentFactory)documentFactory).GetFactory(databaseNumber);
			var parentQuery = new ZQuery(StorageDocsSchema.SC_SM, matchingStorageMains[0].PK);
			var matchingStorageDocs = (BusinessObject[])factoryOne.Load<IStorageDocs>(parentQuery);
			AssertEquals("Should have created one document", 1, matchingStorageDocs.Length);

			var actualPdfData = (ZBlob)matchingStorageDocs[0][StorageDocsSchema.SC_ImageData.Name];
			var expectedSignature = new List<(string key, string value)>()
				{
					(("Location", "Sango")),
					(("Reason", $@"{BrandingFactory.Instance.CompanyBrandingName} \(WTG\) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.")),
					(("ContactInfo", "Sango@Sango.com"))
				};
			var actualSignature = ImageToPDFConverterTest.GetSignatureFromPDF(actualPdfData);
			AssertContainsExactElementsInAnyOrder(expectedSignature, actualSignature);
		}

		void CopyToDocManagerAndAssertExpectedFileFormat(string expectedFileFormat)
		{
			var testParentGuid = ZGuid.NewZGuid();
			var printJob = CreatePrintJob(testParentGuid, File.ReadAllBytes(PrintProcessingConstants.Small));
			Factory.Save();

			var documentFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
			var queryForShipment = new ZQuery(StorageMainSchema.SM_ParentFK, testParentGuid);
			var matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Precondition: Should not be any docs for this parent", 0, matchingStorageMains.Length);

			var processor = new EDocProcessorForTesting(null);
			processor.CopyToDocManagerAndSaveForTesting(printJob);

			matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Should have created a parent", 1, matchingStorageMains.Length);
			AssertEquals("Parent should have a reference to the Shipment Parent Guid", testParentGuid, matchingStorageMains[0][StorageMainSchema.Constants.SM_ParentFK]);

			int databaseNumber = (ZInt)matchingStorageMains[0][StorageMainSchema.SM_DB.Name];
			var factoryOne = ((IDocumentFactory)documentFactory).GetFactory(databaseNumber);
			var parentQuery = new ZQuery(StorageDocsSchema.SC_SM, matchingStorageMains[0].PK);
			var matchingStorageDocs = (BusinessObject[])factoryOne.Load<IStorageDocs>(parentQuery);
			AssertEquals("Should have created one document", 1, matchingStorageDocs.Length);
			AssertEquals("Should have document type", "CIV", matchingStorageDocs[0][StorageDocsSchema.SC_DocType.Name]);
			AssertEndsWith("Should be a " + expectedFileFormat, expectedFileFormat, matchingStorageDocs[0]["SC_FileNameWithExtension"].ToString().ToUpperInvariant());
			AssertEquals("Should set file name from attachment", "test", matchingStorageDocs[0][StorageDocsSchema.SC_FileName.Name]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerWithNoLogDocType()
		{
			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_LogSystemCreatedDocsToEDocs = false;
			docType.RT_DocType = "ZZZ";
			docType.RT_Desc = "This is a test doctype";
			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;

			ZGuid testParentGuid = ZGuid.NewZGuid();
			var printJob = CreatePrintJob(testParentGuid, File.ReadAllBytes(PrintProcessingConstants.Small), documentType: "ZZZ");
			Factory.Save();

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			ZQuery queryForShipment = new ZQuery(StorageMainSchema.SM_ParentFK, testParentGuid);
			BusinessObject[] matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Precondition: Should not be any docs for this parent", 0, matchingStorageMains.Length);

			EDocProcessorForTesting processor = new EDocProcessorForTesting(null);
			processor.CopyToDocManagerAndSaveForTesting(printJob);

			matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Should not create document as DocType.RT_LogSystemCreatedDocsToEDocs = false", 0, matchingStorageMains.Length);

			printJob.SP_SendToEDocs = true;
			Factory.Save();

			processor.CopyToDocManagerAndSaveForTesting(printJob);

			matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Should have created a parent as SP_SendToEDocs = true", 1, matchingStorageMains.Length);
			AssertEquals("Parent should have a reference to the Shipment Parent Guid", testParentGuid, matchingStorageMains[0][StorageMainSchema.Constants.SM_ParentFK]);

			int databaseNumber = (ZInt)matchingStorageMains[0][StorageMainSchema.SM_DB.Name];
			BusinessObjectFactory factoryOne = ((IDocumentFactory)documentFactory).GetFactory(databaseNumber);
			ZQuery parentQuery = new ZQuery(StorageDocsSchema.SC_SM, matchingStorageMains[0].PK);
			BusinessObject[] matchingStorageDocs = (BusinessObject[])factoryOne.Load<IStorageDocs>(parentQuery);
			AssertEquals("Should have created one document", 1, matchingStorageDocs.Length);
			AssertEquals("Should have document type", "ZZZ", matchingStorageDocs[0][StorageDocsSchema.SC_DocType.Name]);
			AssertEquals("Should set file name from attachment", "test", matchingStorageDocs[0][StorageDocsSchema.SC_FileName.Name]);

			AssertEquals("LogProgress messages", "Cannot allocate document 'Subject': Document Type 'ZZZ' with Category 'SCL' is not allowed to save copies on eDocs tab", loggedMessages[1]);
			AssertEquals("LogProgress messages", "Document \"Test\" of type \"ZZZ\" was not allocated to eDocs", loggedMessages[2]);
			AssertEquals("LogProgress messages", "Document 'Subject' of type 'ZZZ' was allocated, but not registered with business object.", loggedMessages[4]);
			AssertEquals("LogProgress messages", "Finished allocating document \"Test\" of type \"ZZZ\"", loggedMessages[5]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerWithEmptyDocType()
		{
			var testParentGuid = ZGuid.NewZGuid();
			var printJob = CreatePrintJob(testParentGuid, File.ReadAllBytes(PrintProcessingConstants.Small), documentType: ZString.Empty);
			Factory.Save();

			var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			var documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			var queryForShipment = new ZQuery(StorageMainSchema.SM_ParentFK, testParentGuid);
			var matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Precondition: Should not be any docs for this parent", 0, matchingStorageMains.Length);

			var processor = new EDocProcessorForTesting(null);
			processor.CopyToDocManagerAndSaveForTesting(printJob);

			matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Should have created a parent", 1, matchingStorageMains.Length);

			var databaseNumber = (ZInt)matchingStorageMains[0][StorageMainSchema.SM_DB.Name];
			var factoryOne = ((IDocumentFactory)documentFactory).GetFactory(databaseNumber);
			var parentQuery = new ZQuery(StorageDocsSchema.SC_SM, matchingStorageMains[0].PK);
			var matchingStorageDocs = factoryOne.Load(ObjectFactory.GetType(typeof(IStorageDocs)), parentQuery);
			AssertEquals("Should have created one document", 1, matchingStorageDocs.Length);
			AssertEquals("Doc type should be msc", "MSC", matchingStorageDocs[0][StorageDocsSchema.SC_DocType.Name]);
			AssertEquals("Should set file name from attachment", "test", matchingStorageDocs[0][StorageDocsSchema.SC_FileName.Name]);

			AssertEquals("LogProgress messages", "Document 'Subject' of type 'MSC' was allocated, but not registered with business object.", loggedMessages[1]);
			AssertEquals("LogProgress messages", "Finished allocating document \"Test\" of type \"\"", loggedMessages[2]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerWithSqlErrorSendsEmail()
		{
			ZGuid testParentGuid = ZGuid.NewZGuid();
			var printJob = CreatePrintJob(testParentGuid, File.ReadAllBytes(PrintProcessingConstants.Small));
			printJob.JobSubmittedBy.GS_EmailAddress = "test@test.com";
			Factory.Save();

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory1 = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			new BusinessObjectFactory("XYZ");
			ZQuery queryForShipment = new ZQuery(StorageMainSchema.SM_ParentFK, testParentGuid);
			BusinessObject[] matchingStorageMains = (BusinessObject[])documentFactory1.Load<IStorageMain>(queryForShipment);
			AssertEquals("Precondition: Should not be any docs for this parent", 0, matchingStorageMains.Length);

			EDocProcessorForTesting processor = new EDocProcessorForTesting(null);
			processor.CopyToDocManagerAndSaveForTesting(printJob);

			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
			foreach (BusinessObject bizO in documentFactory.Load<IStorageMain>(new ZQuery()))
			{
				bizO[StorageMainSchema.SM_DB.Name] = 999;
			}
			documentFactory.Save();

			try
			{
				new EDocProcessorForTesting(null).CopyToDocManagerAndSaveForTesting(printJob);

				Fail("Exception expected here");
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}
			}

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated[0].Attachments.Count);
			AssertEquals("Failure to create eDoc", Env.OutgoingMailManager.EmailsCreated[0].Subject);

			AssertEquals("LogProgress messages", "Document 'Subject' of type 'CIV' was allocated, but not registered with business object.", loggedMessages[1]);
			AssertEquals("LogProgress messages", "Finished allocating document \"Test\" of type \"CIV\"", loggedMessages[2]);
			Assert(loggedMessages[4], loggedMessages[4].StartsWith("Error allocating document \"Test\" with subject \"Subject\" to eDocs."));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManager_DocumentType_SOA()
		{
			var testParentGuid = ZGuid.NewZGuid();
			var printJob = CreatePrintJob(
				testParentGuid,
				File.ReadAllBytes(PrintProcessingConstants.Small),
				documentName: Core.Constants.RefDocTypeDescriptions.CollectionLetter,
				relatedBusinessContext: "CSO",
				documentType: Core.Constants.StatementCollectionLetterType.StatementOfAccount);
			Factory.Save();

			var documentFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
			var queryForShipment = new ZQuery(StorageMainSchema.SM_ParentFK, testParentGuid);
			var matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Precondition: Should not be any docs for this parent", 0, matchingStorageMains.Length);

			var processor = new EDocProcessorForTesting(null);
			processor.CopyToDocManagerAndSaveForTesting(printJob);

			matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
			AssertEquals("Should have created a parent", 1, matchingStorageMains.Length);
			AssertEquals("Parent should have a reference to the Shipment Parent Guid", testParentGuid, matchingStorageMains[0][StorageMainSchema.Constants.SM_ParentFK]);

			int databaseNumber = (ZInt)matchingStorageMains[0][StorageMainSchema.SM_DB.Name];
			var factoryOne = ((IDocumentFactory)documentFactory).GetFactory(databaseNumber);
			var parentQuery = new ZQuery(StorageDocsSchema.SC_SM, matchingStorageMains[0].PK);
			var matchingStorageDocs = (BusinessObject[])factoryOne.Load<IStorageDocs>(parentQuery);
			AssertEquals("Should have created one document", 1, matchingStorageDocs.Length);
			AssertEquals("Should be Collection Letter", Core.Constants.StatementCollectionLetterType.CollectionLetter, matchingStorageDocs[0][StorageDocsSchema.SC_DocType.Name]);
			AssertEquals("Should be Collection Letter", Core.Constants.RefDocTypeDescriptions.CollectionLetter, matchingStorageDocs[0]["SC_DocType_Description"].ToString());
			AssertEquals("Should set file name from attachment", "test", matchingStorageDocs[0][StorageDocsSchema.SC_FileName.Name]);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManager_BranchForCopy()
		{
			// Create job user, and a branch for the user
			var staffOnPrintJob = Factory.New(typeof(GlbStaff)) as GlbStaff;
			staffOnPrintJob.GS_Code = "PRJ";
			staffOnPrintJob.GS_FullName = "Jane Doe";
			staffOnPrintJob.GS_EmailAddress = "jane@example";
			staffOnPrintJob.GS_LoginName = "janedoe";

			var company1 = Factory.New(typeof(GlbCompany)) as GlbCompany;
			company1.GC_Code = "CP1";
			company1.GC_Name = "A Company";

			var userBranch = Factory.New(typeof(GlbBranch)) as GlbBranch;
			userBranch.GB_Code = "BP1";
			userBranch.GB_GC = company1.PK;
			userBranch.GB_BranchName = "A Branch";
			staffOnPrintJob.GS_GB_HomeBranch = userBranch.PK;

			// Create the job branch and assign it to the job
			var company2 = Factory.New(typeof(GlbCompany)) as GlbCompany;
			company2.GC_Code = "CP2";
			company2.GC_Name = "B Company";

			var jobBranch = Factory.New(typeof(GlbBranch)) as GlbBranch;
			jobBranch.GB_Code = "BP2";
			jobBranch.GB_GC = company2.PK;
			jobBranch.GB_BranchName = "B Branch";

			var testParentGuid = ZGuid.NewZGuid();
			var jobWithBranch = CreatePrintJob(testParentGuid, File.ReadAllBytes(PrintProcessingConstants.Small));
			jobWithBranch.SP_GS_NKJobSubmittedBy = staffOnPrintJob.GS_Code;
			jobWithBranch.SP_GB = jobBranch.PK;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(jobWithBranch.ProperBranchPK.ToGuid()))
			{
				var processor = new EDocProcessorForTesting(null);
				processor.CopyToDocManagerAndSaveForTesting(jobWithBranch);
				AssertEquals("Job branch should be used to copy eDoc", jobBranch.PK, processor.BranchUsedForCopyEDoc.PK);
			}

			var jobWithoutBranch = CreatePrintJob(testParentGuid, File.ReadAllBytes(PrintProcessingConstants.Small));
			jobWithoutBranch.SP_GS_NKJobSubmittedBy = staffOnPrintJob.GS_Code;
			jobWithoutBranch.SP_GB = ZGuid.Empty;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(jobWithoutBranch.ProperBranchPK.ToGuid()))
			{
				var processor = new EDocProcessorForTesting(null);
				processor.CopyToDocManagerAndSaveForTesting(jobWithoutBranch);
				AssertEquals("Current branch should be used to copy eDoc", GlbBranch.CurrentBranch.PK, processor.BranchUsedForCopyEDoc.PK);
			}
		}

		StmPrintJob CreatePrintJob(
			ZGuid testParentGuid,
			byte[] data,
			string documentName = "Test",
			string emailAttachments = "test.XLS",
			string relatedBusinessContext = "SHP",
			string documentType = "CIV")
		{
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "TIF";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = documentName;
			printJob.SP_CustomProperties = data;
			printJob.SP_EmailAttachments = emailAttachments;
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = relatedBusinessContext;
			printJob.SP_DocumentType = documentType;
			printJob.SP_RunDateTime = ZDateTime.Now;
			printJob.SP_SB_DeliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory).PK;

			return printJob;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerAllocatePasswordProtectedExcelSpreadsheetsUseRegistryPassword()
		{
			var testParentGuid = ZGuid.NewZGuid();
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = Core.Constants.RefDocTypeDescriptions.CollectionLetter;
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "CSO";
			printJob.SP_DocumentType = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			printJob.SP_RunDateTime = ZDateTime.Now;
			printJob.SP_SB_DeliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory).PK;
			printJob.SP_ExcelEncryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt("123");
			Factory.Save();

			using (SystemDataRegistry.Instance.AllocatePasswordProtectedExcelSpreadsheets.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
				var queryForShipment = new ZQuery(StorageMainSchema.SM_ParentFK, testParentGuid);
				var processor = new EDocProcessorForTesting(null);
				processor.CopyToDocManagerAndSaveForTesting(printJob);

				var matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
				int databaseNumber = (ZInt)matchingStorageMains[0][StorageMainSchema.SM_DB.Name];
				var factoryOne = ((IDocumentFactory)documentFactory).GetFactory(databaseNumber);
				var parentQuery = new ZQuery(StorageDocsSchema.SC_SM, matchingStorageMains[0].PK);
				var matchingStorageDocs = (BusinessObject[])factoryOne.Load<IStorageDocs>(parentQuery);

				AssertEquals("Should have one file allocate to eDocs", 1, matchingStorageDocs.Length);

				var storageDoc = matchingStorageDocs[0] as IeDocBase;
				AssertEquals("Should set file name from attachment", "test.xls", storageDoc.FileName);

				using (var xLInterface = new ExcelInterface())
				using (var ms = new MemoryStream(storageDoc.ImageData))
				{
					AssertExceptionThrown<DocumentEngine.Exceptions.ExcelInterfaceException>("should open with password",
						"The selected file could not be loaded. It may be damaged or an unsupported format. Please check the file and try again.",
						() => xLInterface.LoadExcelFile(ms));

					xLInterface.Xls.Protection.OpenPassword = Env.Registry.ExcelPasswordForOpening;
					AssertNoExceptionThrown(() => xLInterface.LoadExcelFile(ms));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyToDocManagerAllocatePasswordProtectedExcelSpreadsheets_KeepLatestVersion()
		{
			var testParentGuid = ZGuid.NewZGuid();
			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_JobType = "EML";
			printJob.SP_EmailAttachmentFormat = "XLS";
			printJob.SP_Destination = "example@example.com";
			printJob.SP_DocumentName = Core.Constants.RefDocTypeDescriptions.CollectionLetter;
			printJob.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob.SP_EmailAttachments = "test.XLS";
			printJob.SP_EmailSubjectLine = "Subject";
			printJob.SP_ParentTableName = "JobShipment";
			printJob.SP_ParentGuid = testParentGuid;
			printJob.SP_RelatedBusinessContext = "CSO";
			printJob.SP_DocumentType = RefDocTypes.ScheduledReport;
			printJob.SP_RunDateTime = ZDateTime.Now;
			printJob.SP_SB_DeliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory).PK;
			printJob.SP_ExcelEncryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt("123");
			Factory.Save();

			using (SystemDataRegistry.Instance.AllocatePasswordProtectedExcelSpreadsheets.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
				var queryForShipment = new ZQuery(StorageMainSchema.SM_ParentFK, testParentGuid);
				var processor = new EDocProcessorForTesting(null);
				processor.CopyToDocManagerAndSaveForTesting(printJob);

				var matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
				int databaseNumber = (ZInt)matchingStorageMains[0][StorageMainSchema.SM_DB.Name];
				var factoryOne = ((IDocumentFactory)documentFactory).GetFactory(databaseNumber);
				var parentQuery = new ZQuery(StorageDocsSchema.SC_SM, matchingStorageMains[0].PK);
				var matchingStorageDocs = (BusinessObject[])factoryOne.Load<IStorageDocs>(parentQuery);

				AssertEquals("Should have one file allocate to eDocs", 1, matchingStorageDocs.Length);

				var storageDoc = matchingStorageDocs[0] as IeDocBase;
				AssertEquals("Should set file name from attachment", "test.xls", storageDoc.FileName);

				using (var xLInterface = new ExcelInterface())
				using (var ms = new MemoryStream(storageDoc.ImageData))
				{
					AssertExceptionThrown<DocumentEngine.Exceptions.ExcelInterfaceException>("should open with password",
						"The selected file could not be loaded. It may be damaged or an unsupported format. Please check the file and try again.",
						() => xLInterface.LoadExcelFile(ms));

					xLInterface.Xls.Protection.OpenPassword = Env.Registry.ExcelPasswordForOpening;
					AssertNoExceptionThrown(() => xLInterface.LoadExcelFile(ms));
				}
			}

			var printJob1 = Factory.New<StmPrintJob>();
			printJob1.SP_JobType = "EML";
			printJob1.SP_EmailAttachmentFormat = "XLS";
			printJob1.SP_Destination = "example@example.com";
			printJob1.SP_DocumentName = Core.Constants.RefDocTypeDescriptions.CollectionLetter;
			printJob1.SP_CustomProperties = File.ReadAllBytes(PrintProcessingConstants.Small);
			printJob1.SP_EmailAttachments = "test.XLS";
			printJob1.SP_EmailSubjectLine = "Subject";
			printJob1.SP_ParentTableName = "JobShipment";
			printJob1.SP_ParentGuid = testParentGuid;
			printJob1.SP_RelatedBusinessContext = "CSO";
			printJob1.SP_DocumentType = RefDocTypes.ScheduledReport;
			printJob1.SP_RunDateTime = ZDateTime.Now;
			printJob1.SP_SB_DeliveryGroup = TestAssistant.CreateNewDeliveryGroup(Factory).PK;
			printJob1.SP_ExcelEncryptedPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt("345");
			Factory.Save();

			using (SystemDataRegistry.Instance.AllocatePasswordProtectedExcelSpreadsheets.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var documentFactory = (BusinessObjectFactory)ObjectFactory.Get<IDocumentFactoryProvider>().GetFactory(Factory);
				var queryForShipment = new ZQuery(StorageMainSchema.SM_ParentFK, testParentGuid);
				var processor = new EDocProcessorForTesting(null);
				processor.CopyToDocManagerAndSaveForTesting(printJob1);

				var matchingStorageMains = (BusinessObject[])documentFactory.Load<IStorageMain>(queryForShipment);
				int databaseNumber = (ZInt)matchingStorageMains[0][StorageMainSchema.SM_DB.Name];
				var factoryOne = ((IDocumentFactory)documentFactory).GetFactory(databaseNumber);
				var parentQuery = new ZQuery(StorageDocsSchema.SC_SM, matchingStorageMains[0].PK);
				var matchingStorageDocs = (BusinessObject[])factoryOne.Load<IStorageDocs>(parentQuery);

				AssertEquals("Should have one file allocate to eDocs", 1, matchingStorageDocs.Length);

				var storageDoc = matchingStorageDocs[0] as IeDocBase;
				AssertEquals("Should set file name from attachment", "test[2].xls", storageDoc.FileName);

				using (var xLInterface = new ExcelInterface())
				using (var ms = new MemoryStream(storageDoc.ImageData))
				{
					AssertExceptionThrown<DocumentEngine.Exceptions.ExcelInterfaceException>("should open with password",
						"The selected file could not be loaded. It may be damaged or an unsupported format. Please check the file and try again.",
						() => xLInterface.LoadExcelFile(ms));

					xLInterface.Xls.Protection.OpenPassword = Env.Registry.ExcelPasswordForOpening;
					AssertNoExceptionThrown(() => xLInterface.LoadExcelFile(ms));
				}
			}
		}

		#region Implementation

		internal override MergedPrintGroupProcessor GetProcessor(StmPrintJobMergedCollection printGroup)
		{
			return new EDocProcessor(printGroup, new PrintJobManager.ProgressDelegate((eventType, statusMessage) => { loggedMessages.Add(statusMessage); }));
		}

		class EDocProcessorForTesting : EDocProcessor
		{
			public EDocProcessorForTesting(StmPrintJobMergedCollection mergedPrintGroup)
				: base(mergedPrintGroup, new PrintJobManager.ProgressDelegate((eventType, statusMessage) => { loggedMessages.Add(statusMessage); }))
			{
			}

			internal void CopyToDocManagerAndSaveForTesting(StmPrintJob printJob)
			{
				ProcessIndividualItemCore(printJob);
			}

			protected override ITransactionParticipant CopyToDocManager(StmPrintJob printJob)
			{
				BranchUsedForCopyEDoc = GlbBranch.CurrentBranch;
				return base.CopyToDocManager(printJob);
			}

			internal GlbBranch BranchUsedForCopyEDoc { get; set; }
		}

		static readonly List<String> loggedMessages = new List<String>();

		protected override void TearDown()
		{
			loggedMessages.Clear();
			base.TearDown();
		}

		#endregion
	}
}
