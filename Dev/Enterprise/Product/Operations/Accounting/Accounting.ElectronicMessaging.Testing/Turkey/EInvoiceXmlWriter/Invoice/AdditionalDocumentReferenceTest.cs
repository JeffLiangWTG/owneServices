using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Export.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class AdditionalDocumentReferenceTest : TestCaseWithFactory
	{
		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestAdditionalDocumentReference()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var fileData = new ZBlob(Encoding.UTF8.GetBytes("This is test data."));
				var templateFile = Helper.CreateTemplateFile(Env.CurrentCompanyPK, "TST", true, fileData, externalReference: ZGuid.Empty);
				var configuration = Helper.CreateNewTemplateFileConfiguration(templateFile, "ALL", "ALL", ZGuid.Empty, ZString.Empty);
				var invoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "A00001", 1000m, Helper.TestObjectCreator.TRY);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(2, eInvoice.Invoice.AdditionalDocumentReference.Length);
					var additionalDocReference1 = eInvoice.Invoice.AdditionalDocumentReference[0];
					var binaryObject = additionalDocReference1.Attachment.EmbeddedDocumentBinaryObject;
					var additionalDocumentReference2 = eInvoice.Invoice.AdditionalDocumentReference[1];

					AssertEquals(TestDateAttribute.Date, additionalDocReference1.IssueDate.Value);
					AssertEquals("application/xml", binaryObject.mimeCode);
					AssertEquals("Base64", binaryObject.encodingCode);
					AssertEquals("UTF-8", binaryObject.characterSetCode);
					AssertEquals("Default.xslt", binaryObject.filename);
					AssertEquals(fileData, binaryObject.Value);
					AssertEquals("00001000", additionalDocumentReference2.ID.Value);
					AssertEquals("TRANSACTION_NUMBER", additionalDocumentReference2.DocumentType.Value);
					AssertEquals(TestDateAttribute.Date, additionalDocumentReference2.IssueDate.Value);
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestAdditionalDocumentReference_OrgRelatedConfigFirst()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var fileData = new ZBlob(Encoding.UTF8.GetBytes("This is test data for Company."));
				var fileDataOrg = new ZBlob(Encoding.UTF8.GetBytes("This is test data for Organization."));
				var templateFile = Helper.CreateTemplateFile(Env.CurrentCompanyPK, "TSC", true, fileData, ZGuid.Empty, "CompanyDefault.xslt", "Company Default");
				var templateFileOrg = Helper.CreateTemplateFile(Env.CurrentCompanyPK, "TSO", true, fileDataOrg, ZGuid.Empty, "OrganizationDefault.xslt", "Organization Default");
				var configuration = Helper.CreateNewTemplateFileConfiguration(templateFile, "ALL", "ALL", ZGuid.Empty, ZString.Empty);
				var configurationOrg = Helper.CreateNewTemplateFileConfiguration(templateFileOrg, "ALL", "ALL", Helper.TestObjectCreator.DebtorTR.PK, OrgHeaderSchema.Constants.Prefix);
				var invoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "A00001", 1000m, Helper.TestObjectCreator.TRY);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(2, eInvoice.Invoice.AdditionalDocumentReference.Length);
					var additionalDocReference1 = eInvoice.Invoice.AdditionalDocumentReference[0];
					var binaryObject = additionalDocReference1.Attachment.EmbeddedDocumentBinaryObject;
					var additionalDocumentReference2 = eInvoice.Invoice.AdditionalDocumentReference[1];

					AssertEquals(TestDateAttribute.Date, additionalDocReference1.IssueDate.Value);
					AssertEquals("application/xml", binaryObject.mimeCode);
					AssertEquals("Base64", binaryObject.encodingCode);
					AssertEquals("UTF-8", binaryObject.characterSetCode);
					AssertEquals("OrganizationDefault.xslt", binaryObject.filename);
					AssertEquals(fileDataOrg, binaryObject.Value);
					AssertEquals("00001000", additionalDocumentReference2.ID.Value);
					AssertEquals("TRANSACTION_NUMBER", additionalDocumentReference2.DocumentType.Value);
					AssertEquals(TestDateAttribute.Date, additionalDocumentReference2.IssueDate.Value);
				}
			}
		}

		[TestDate(2020, 1, 29, 13, 8, 32)]
		public void TestAdditionalDocumentReference_CorrectConfigForConsolJob()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var fileData = new ZBlob(Encoding.UTF8.GetBytes("This is test data for Company."));
				var fileDataOrg = new ZBlob(Encoding.UTF8.GetBytes("This is test data for Organization."));
				var templateFile = Helper.CreateTemplateFile(Env.CurrentCompanyPK, "TSC", true, fileData, ZGuid.Empty, "CompanyDefault.xslt", "Company Default");
				var templateFileOrg = Helper.CreateTemplateFile(Env.CurrentCompanyPK, "TSO", true, fileDataOrg, ZGuid.Empty, "OrganizationDefault.xslt", "Organization Default");
				var configuration = Helper.CreateNewTemplateFileConfiguration(templateFile, "SHP", "ALL", ZGuid.Empty, ZString.Empty);
				var configurationOrg = Helper.CreateNewTemplateFileConfiguration(templateFileOrg, "SHP", "AIR", Helper.TestObjectCreator.DebtorTR.PK, OrgHeaderSchema.Constants.Prefix);

				var consol = Helper.TestObjectCreator.CreateConsol("TRIST", "NZAKL", "C001001");
				var shipment = Helper.TestObjectCreator.CreateShipment("S001001", consol);
				var job = Helper.TestObjectCreator.CreateJob(shipment, createWithMutex: false, localClientOrg: Helper.TestObjectCreator.DebtorTR);

				var invoice = Helper.TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", Helper.TestObjectCreator.USD, 1m, Helper.TestObjectCreator.DebtorTR);
				invoice.AH_TransactionReference = "ABC2020000000001";
				invoice.AH_JH = job.PK;

				invoice.Lines.Add(Helper.TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, Helper.TestObjectCreator.CC14, Helper.TestObjectCreator.USD, 1m, "ARInvoiceLine1", 1000m, Helper.TestObjectCreator.KDV18.PK));
				invoice.Lines.Add(Helper.TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, job, Helper.TestObjectCreator.CC14, Helper.TestObjectCreator.USD, 1m, "ARInvoiceLine2", 2000m, Helper.TestObjectCreator.KDV18W5.PK));

				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(2, eInvoice.Invoice.AdditionalDocumentReference.Length);
					var additionalDocReference1 = eInvoice.Invoice.AdditionalDocumentReference[0];
					var binaryObject = additionalDocReference1.Attachment.EmbeddedDocumentBinaryObject;
					var additionalDocumentReference2 = eInvoice.Invoice.AdditionalDocumentReference[1];

					AssertEquals(TestDateAttribute.Date, additionalDocReference1.IssueDate.Value);
					AssertEquals("application/xml", binaryObject.mimeCode);
					AssertEquals("Base64", binaryObject.encodingCode);
					AssertEquals("UTF-8", binaryObject.characterSetCode);
					AssertEquals("OrganizationDefault.xslt", binaryObject.filename);
					AssertEquals(fileDataOrg, binaryObject.Value);
					AssertEquals("00001000", additionalDocumentReference2.ID.Value);
					AssertEquals("TRANSACTION_NUMBER", additionalDocumentReference2.DocumentType.Value);
					AssertEquals(TestDateAttribute.Date,	additionalDocumentReference2.IssueDate.Value);
				}
			}
		}

		public void TestInactiveAdditionalDocumentReferencesOnCompanyLevelNotBeingUsed()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var fileData = new ZBlob(Encoding.UTF8.GetBytes("This is test data."));
				var templateFile = Helper.CreateTemplateFile(Env.CurrentCompanyPK, "TST", true, fileData, externalReference: ZGuid.Empty);
				templateFile.TFS_IsActive = false;
				Helper.Factory.Save();
				var configuration = Helper.CreateNewTemplateFileConfiguration(templateFile, "ALL", "ALL", ZGuid.Empty, ZString.Empty);
				var invoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "A00001", 1000m, Helper.TestObjectCreator.TRY);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				AssertInactiveTemplateFilesAreNotUsed(invoiceBatch);
			}
		}

		public void TestInactiveAdditionalDocumentReferenceOnOrganizationLevelNotBeingUsed()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var fileData = new ZBlob(Encoding.UTF8.GetBytes("This is test data."));
				var templateFileOrg = Helper.CreateTemplateFile(Env.CurrentCompanyPK, "TSO", true, fileData, ZGuid.Empty, "OrganizationDefault.xslt", "Organization Default");
				templateFileOrg.TFS_IsActive = false;
				Helper.Factory.Save();
				var configurationOrg = Helper.CreateNewTemplateFileConfiguration(templateFileOrg, "ALL", "ALL", Helper.TestObjectCreator.DebtorTR.PK, OrgHeaderSchema.Constants.Prefix);
				var invoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "A00001", 1000m, Helper.TestObjectCreator.TRY);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				AssertInactiveTemplateFilesAreNotUsed(invoiceBatch);
			}
		}

		public void TestXSLTFilesUseGUIDWhenItExists()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var fileData = new ZBlob(Encoding.UTF8.GetBytes("This is test data."));
				var templateFile = Helper.CreateTemplateFile(Env.CurrentCompanyPK, "TSO", true, fileData, ZGuid.Empty, "TEST.xslt", "Organization Default");
				templateFile.TFS_ExternalReference = ZGuid.NewZGuid();
				Helper.Factory.Save();

				//Asserting template has both data and guid
				Assert(!templateFile.TFS_ExternalReference.IsEmpty);
				Assert(!templateFile.TFS_FileData.IsEmpty);
				Assert(!templateFile.TFS_FileName.IsEmpty);

				var configuration = Helper.CreateNewTemplateFileConfiguration(templateFile, "ALL", "ALL", ZGuid.Empty, ZString.Empty);
				var invoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18.PK.ToGuid(), "A00001", 1000m, Helper.TestObjectCreator.TRY);
				var invoiceBatch = Helper.CreateInvoiceBatch(invoice);
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
				{
					var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

					AssertEquals(2, eInvoice.Invoice.AdditionalDocumentReference.Length);
					var additionalDocumnetReference1 = eInvoice.Invoice.AdditionalDocumentReference[0];
					var additionalDocumentReference2 = eInvoice.Invoice.AdditionalDocumentReference[1];

					//asserting gei message only has guid, not data
					AssertEquals("xslt", additionalDocumnetReference1.DocumentType.Value);
					AssertEquals(templateFile.TFS_ExternalReference.ToString(), additionalDocumnetReference1.DocumentTypeCode.Value);
					AssertNull(additionalDocumnetReference1.Attachment);
					AssertEquals("00001000", additionalDocumentReference2.ID.Value);
					AssertEquals("TRANSACTION_NUMBER", additionalDocumentReference2.DocumentType.Value);
				}
			}
		}

		void AssertInactiveTemplateFilesAreNotUsed(AccEInvoicingBatch invoiceBatch)
		{
			var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			using (var transactionBatch = exporter.CreateTransactionBatch(invoiceBatch, SchemaVersionManager.Current.Namespace))
			{
				var eInvoice = Helper.EInvoiceInitializer(transactionBatch);

				AssertNotNull(eInvoice.Invoice.AdditionalDocumentReference);
				AssertEquals(1, eInvoice.Invoice.AdditionalDocumentReference.Length);
				var additionalDocumentReference = eInvoice.Invoice.AdditionalDocumentReference[0];

				AssertNotEquals("xslt", additionalDocumentReference.DocumentType.Value);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
			Helper = new TurkeyEInvoiceTestHelper();
		}

		BatchExportDataAccess DataAccess;
		TurkeyEInvoiceTestHelper Helper;
	}
}
