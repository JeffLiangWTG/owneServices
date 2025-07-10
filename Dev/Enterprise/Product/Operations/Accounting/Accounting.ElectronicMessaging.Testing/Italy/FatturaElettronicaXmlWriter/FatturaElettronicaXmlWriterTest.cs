using System;
using System.IO;
using System.Xml;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Export.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class FatturaElettronicaXmlWriterTest : TestCaseWithFactory
	{
		public void TestXmlDocumentNodesAndAttributes()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var invoicingBatch = CreateInvoicingBatch();
				var exporter = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exporter.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					using (var testStream = new MemoryStream())
					{
						var xmlDocument = CreateFatturaElettronicaXmlDocument(transactionBatch, testStream);
						var childNodes = xmlDocument.ChildNodes;
						AssertEquals(2, childNodes.Count);

						#region XML declaration node

						var declarationNode = childNodes[0];
						AssertEquals(XmlNodeType.XmlDeclaration, declarationNode.NodeType);
						AssertEquals("version=\"1.0\" encoding=\"utf-8\"", declarationNode.Value);

						#endregion

						#region FatturaElettronica 

						var fatturaElettronicaNode = childNodes[1];
						AssertEquals("p:FatturaElettronica", fatturaElettronicaNode.Name);

						var fatturaElettronicaNodeAttributes = fatturaElettronicaNode.Attributes;
						AssertEquals(5, fatturaElettronicaNodeAttributes.Count);
						AssertXmlAttributeNameAndValue(fatturaElettronicaNodeAttributes[0], "versione", "FPR12");
						AssertXmlAttributeNameAndValue(fatturaElettronicaNodeAttributes[1], "xmlns:p", "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2");
						AssertXmlAttributeNameAndValue(fatturaElettronicaNodeAttributes[2], "xmlns:ds", "http://www.w3.org/2000/09/xmldsig#");
						AssertXmlAttributeNameAndValue(fatturaElettronicaNodeAttributes[3], "xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");

						var fatturaElettronicaChildrenNodes = fatturaElettronicaNode.ChildNodes;
						AssertEquals(2, fatturaElettronicaChildrenNodes.Count);

						#region FatturaElettronicaHeader

						var fatturaElettronicaHeaderNode = fatturaElettronicaChildrenNodes[0];
						AssertEquals("FatturaElettronicaHeader", fatturaElettronicaHeaderNode.Name);
						var fatturaElettronicaHeaderChildrenNodes = fatturaElettronicaHeaderNode.ChildNodes;
						AssertEquals(3, fatturaElettronicaHeaderChildrenNodes.Count);

						AssertEquals("DatiTrasmissione", fatturaElettronicaHeaderChildrenNodes[0].Name);
						AssertEquals("CedentePrestatore", fatturaElettronicaHeaderChildrenNodes[1].Name);
						AssertEquals("CessionarioCommittente", fatturaElettronicaHeaderChildrenNodes[2].Name);

						#endregion

						#region FatturaElettronicaBody

						var fatturaElettronicaBodyNode = fatturaElettronicaChildrenNodes[1];
						AssertEquals("FatturaElettronicaBody", fatturaElettronicaBodyNode.Name);
						var fatturaElettronicaBodyChildrenNodes = fatturaElettronicaBodyNode.ChildNodes;
						AssertEquals(4, fatturaElettronicaBodyChildrenNodes.Count);

						AssertEquals("DatiGenerali", fatturaElettronicaBodyChildrenNodes[0].Name);
						AssertEquals("DatiBeniServizi", fatturaElettronicaBodyChildrenNodes[1].Name);
						AssertEquals("DatiPagamento", fatturaElettronicaBodyChildrenNodes[2].Name);
						AssertEquals("Allegati", fatturaElettronicaBodyChildrenNodes[3].Name);

						#endregion

						#endregion
					}
				}
			}
		}

		[TestDate(2020, 10, 14)]
		public void TestOldXsdSchemaAttributeWhenRegistryDateGreaterThanSystemDate()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var invoicingBatch = CreateInvoicingBatch();
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					using (var testStream = new MemoryStream())
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 15)))
					{
						var xmlDocument = CreateFatturaElettronicaXmlDocument(transactionBatch, testStream);
						var fatturaElettronicaNode = xmlDocument.ChildNodes[1];
						var fatturaElettronicaNodeAttributes = fatturaElettronicaNode.Attributes;
						AssertXmlAttributeNameAndValue(fatturaElettronicaNodeAttributes[4], "xsi:schemaLocation", "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2 http://www.fatturapa.gov.it/export/fatturazione/sdi/fatturapa/v1.2/Schema_del_file_xml_FatturaPA_versione_1.2.xsd");
					}
				}
			}
		}

		[TestDate(2020, 10, 14)]
		public void TestNewXsdSchemaAttributeWhenRegistryDateLessOrEqualThanSystemDate()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var invoicingBatch = CreateInvoicingBatch();
				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
				{
					using (var testStream = new MemoryStream())
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 14)))
					{
						var xmlDocument = CreateFatturaElettronicaXmlDocument(transactionBatch, testStream);
						var fatturaElettronicaNode = xmlDocument.ChildNodes[1];
						var fatturaElettronicaNodeAttributes = fatturaElettronicaNode.Attributes;
						AssertXmlAttributeNameAndValue(fatturaElettronicaNodeAttributes[4], "xsi:schemaLocation", "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2 http://www.fatturapa.gov.it/export/fatturazione/sdi/fatturapa/v1.2/Schema_del_file_xml_FatturaPA_versione_1.2.1.xsd");
					}

					using (var testStream = new MemoryStream())
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 13)))
					{
						var xmlDocument = CreateFatturaElettronicaXmlDocument(transactionBatch, testStream);
						var fatturaElettronicaNode = xmlDocument.ChildNodes[1];
						var fatturaElettronicaNodeAttributes = fatturaElettronicaNode.Attributes;
						AssertXmlAttributeNameAndValue(fatturaElettronicaNodeAttributes[4], "xsi:schemaLocation", "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2 http://www.fatturapa.gov.it/export/fatturazione/sdi/fatturapa/v1.2/Schema_del_file_xml_FatturaPA_versione_1.2.1.xsd");
					}
				}
			}
		}

		public void TestTransactionBatchWithoutAnyTransaction()
		{
			var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);

			AssertEquals(0, transactionBatch.TransactionCollection.Count);

			using (var testStream = new MemoryStream())
			{
				var testLogger = new NotificationBuffer();
				AssertExceptionThrown<ArgumentException>("FatturaElettronicaXmlWriter only accepts batches with one transaction", () => new FatturaElettronicaXmlWriter().WriteXmlToStream(transactionBatch, testStream, testLogger));
			}
		}

		public void TestTransactionBatchWithMoreThanOneTransaction()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "charge2", TestObjectCreator.AUD, 20m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 20m, TestObjectCreator.AALSHI);
				Factory.Save();

				var arInvoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				arInvoice1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, arInvoice1.PK));
				var arInvoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				arInvoice2.Lines.Add(TestObjectCreator.CreateRevenueLine(charge2, arInvoice2.PK));
				Factory.Save();

				var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice1, Core.Constants.EInvoicingPivotState.Batched);
				TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice2, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);

				AssertEquals(2, transactionBatch.TransactionCollection.Count);

				using (var testStream = new MemoryStream())
				{
					var testLogger = new NotificationBuffer();
					AssertExceptionThrown<ArgumentException>("FatturaElettronicaXmlWriter only accepts batches with one transaction", () => new FatturaElettronicaXmlWriter().WriteXmlToStream(transactionBatch, testStream, testLogger));
				}
			}
		}

		void AssertXmlAttributeNameAndValue(XmlAttribute xmlAttribute, ZString expectedName, ZString expectedValue)
		{
			AssertEquals(expectedName, xmlAttribute.Name);
			AssertEquals(expectedValue, xmlAttribute.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		AccEInvoicingBatch CreateInvoicingBatch()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();

			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AP001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
			var arInvoiceeDoc = ((IDocManagerSupport)arInvoice).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestInvoiceFile", "INV");
			arInvoiceeDoc.IsPublished = true;
			Factory.Save();

			var invoicingBatch = TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();
			return invoicingBatch;
		}

		XmlDocument CreateFatturaElettronicaXmlDocument(TransactionBatch transactionBatch, MemoryStream testStream)
		{
			var testLogger = new NotificationBuffer();
			new FatturaElettronicaXmlWriter().WriteXmlToStream(transactionBatch, testStream, testLogger);
			testStream.Position = 0;
			var xmlDocument = new XmlDocument();
			xmlDocument.Load(testStream);
			return xmlDocument;
		}

		TestObjectCreator TestObjectCreator;
		BatchExportDataAccess DataAccess;
	}
}
