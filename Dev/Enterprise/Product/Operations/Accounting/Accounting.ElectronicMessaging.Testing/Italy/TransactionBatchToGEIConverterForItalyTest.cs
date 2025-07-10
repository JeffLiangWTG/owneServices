using System;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class TransactionBatchToGEIConverterForItalyTest : TestCaseWithFactory
	{
		public void TestConversion()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var invoicingBatch = CreateInvoicingBatch();

				using (var converter = new TransactionBatchToGEIConverterForItaly())
				{
					var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);

					var expectedErrors = @"The 'IdCodice' element is invalid - The value '' is invalid according to its datatype 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2:CodiceType' - The actual length is less than the MinLength value.
The 'Comune' element is invalid - The value '' is invalid according to its datatype 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2:String60LatinType' - The Pattern constraint failed.
The 'Comune' element is invalid - The value '' is invalid according to its datatype 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2:String60LatinType' - The Pattern constraint failed.
The 'AliquotaIVA' element is invalid - The value '' is invalid according to its datatype 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2:RateType' - The Pattern constraint failed.
The 'AliquotaIVA' element is invalid - The value '' is invalid according to its datatype 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2:RateType' - The Pattern constraint failed.
The 'ModalitaPagamento' element is invalid - The value '' is invalid according to its datatype 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2:ModalitaPagamentoType' - The actual length is not equal to the specified length.";

					AssertMultilineASCIIEquals("Validation Errors", expectedErrors, validationErrors);

					AssertEquals("BatchNumber", invoicingBatch.AIB_BatchNumber.ToString(), eInvoice.Header.ElectronicInvoiceBatchRequest.BatchNumber.ToString());
					AssertEquals("MessageType", "REQ", eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType);
					AssertEquals("MessagingSystem", "Italy electronic invoicing system", eInvoice.Header.ElectronicInvoiceBatchRequest.MessagingSystem);

					using (var testStream = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(eInvoice.Payload)))
					{
						var testLogger = new NotificationBuffer();
						testStream.Position = 0;
						var xmlDocument = new XmlDocument();
						xmlDocument.Load(testStream);
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
				using (var converter = new TransactionBatchToGEIConverterForItaly())
				{
					using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 15)))
					{
						var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);
						using (var testStream = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(eInvoice.Payload)))
						{
							var testLogger = new NotificationBuffer();
							testStream.Position = 0;
							var xmlDocument = new XmlDocument();
							xmlDocument.Load(testStream);
							var fatturaElettronicaNode = xmlDocument.ChildNodes[1];
							var fatturaElettronicaNodeAttributes = fatturaElettronicaNode.Attributes;
							AssertXmlAttributeNameAndValue(fatturaElettronicaNodeAttributes[4], "xsi:schemaLocation", "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2 http://www.fatturapa.gov.it/export/fatturazione/sdi/fatturapa/v1.2/Schema_del_file_xml_FatturaPA_versione_1.2.xsd");
						}
					}
				}
			}
		}

		[TestDate(2020, 10, 14)]
		public void TestNewXsdSchemaAttributeWhenRegistryDateLessThanSystemDate()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var invoicingBatch = CreateInvoicingBatch();
				using (var converter = new TransactionBatchToGEIConverterForItaly())
				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 13)))
				{
					var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);
					using (var testStream = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(eInvoice.Payload)))
					{
						var testLogger = new NotificationBuffer();
						testStream.Position = 0;
						var xmlDocument = new XmlDocument();
						xmlDocument.Load(testStream);
						var fatturaElettronicaNode = xmlDocument.ChildNodes[1];
						var fatturaElettronicaNodeAttributes = fatturaElettronicaNode.Attributes;
						AssertXmlAttributeNameAndValue(fatturaElettronicaNodeAttributes[4], "xsi:schemaLocation", "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2 http://www.fatturapa.gov.it/export/fatturazione/sdi/fatturapa/v1.2/Schema_del_file_xml_FatturaPA_versione_1.2.1.xsd");
					}
				}
			}
		}

		[TestDate(2020, 10, 14)]
		public void TestNewXsdSchemaAttributeWhenRegistryDateEqualToSystemDate()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var invoicingBatch = CreateInvoicingBatch();
				using (var converter = new TransactionBatchToGEIConverterForItaly())
				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 14)))
				{
					var (eInvoice, validationErrors, validationWarnings) = converter.Convert(invoicingBatch);
					using (var testStream = new MemoryStream(MessageEncoding.UTF8WithoutBOM.GetBytes(eInvoice.Payload)))
					{
						var testLogger = new NotificationBuffer();
						testStream.Position = 0;
						var xmlDocument = new XmlDocument();
						xmlDocument.Load(testStream);
						var fatturaElettronicaNode = xmlDocument.ChildNodes[1];
						var fatturaElettronicaNodeAttributes = fatturaElettronicaNode.Attributes;
						AssertXmlAttributeNameAndValue(fatturaElettronicaNodeAttributes[4], "xsi:schemaLocation", "http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2 http://www.fatturapa.gov.it/export/fatturazione/sdi/fatturapa/v1.2/Schema_del_file_xml_FatturaPA_versione_1.2.1.xsd");
					}
				}
			}
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

		void AssertXmlAttributeNameAndValue(XmlAttribute xmlAttribute, ZString expectedName, ZString expectedValue)
		{
			AssertEquals(expectedName, xmlAttribute.Name);
			AssertEquals(expectedValue, xmlAttribute.Value);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
