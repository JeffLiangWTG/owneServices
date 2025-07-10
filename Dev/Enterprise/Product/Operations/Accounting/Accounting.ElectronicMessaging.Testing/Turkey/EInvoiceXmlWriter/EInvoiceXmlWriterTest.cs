using System;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.Export.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.CountryCompliance.TurkeyComplianceInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	sealed class EInvoiceXmlWriterTest : TestCaseWithFactory
	{
		readonly TurkeyEInvoiceTestHelper Helper = new TurkeyEInvoiceTestHelper();

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestTurkeyEInvoiceStructureAndContentForLocalCurrency()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EIN);
				AssertXMLWriterTests(arInvoice, 65, false, 4, TurkeyEInvoiceExportedXmlLocalCurrency);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestTurkeyEInvoiceStructureAndContentForLocalCurrency_EAR()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.TRY, "AR001", ComplianceSubTypeCodes.EAR);
				AssertXMLWriterTests(arInvoice, 65, false, 5, TurkeyEInvoiceExportedXmlLocalCurrency_EAR);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestTurkeyEInvoiceStructureAndContentForOSCurrency()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "euro sent");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.EUR, "AR001", ComplianceSubTypeCodes.EIN, addCommentCharges: true);
				AssertXMLWriterTests(arInvoice, 70, true, 4, TurkeyEInvoiceExportedXmlOSCurrency);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestTurkeyEInvoiceStructureAndContentForOSCurrency_EAR()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "euro sent");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateARInvoiceWithShipment(Helper.TestObjectCreator.CreditorTR, Helper.TestObjectCreator.EUR, "AR001", ComplianceSubTypeCodes.EAR);
				AssertXMLWriterTests(arInvoice, 67, true, 5, TurkeyEInvoiceExportedXmlOSCurrency_EAR);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestTurkeyEInvoiceStructureAndContentForWithHoldingTaxInvoice()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18W5.PK, "AR001", 1034m, Helper.TestObjectCreator.TRY);
				AssertXMLWriterTests(arInvoice, 34, false, 4, TurkeyEInvoiceWithholdingTaxXML);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestTurkeyEInvoiceStructureAndContentForWithHoldingTaxInvoice_EAR()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18W5.PK, "AR001", 1034m, Helper.TestObjectCreator.TRY, true);
				AssertXMLWriterTests(arInvoice, 34, false, 5, TurkeyEInvoiceWithholdingTaxXML_EAR);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestTurkeyEInvoiceStructureAndContentForWithTaxExemptionInvoice()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.FREEVAT.PK, "AR001", 1034m, Helper.TestObjectCreator.TRY);
				AssertXMLWriterTests(arInvoice, 32, false, 4, TurkeyEInvoiceWithTaxExemptionXML);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestTurkeyEInvoiceXMLDoesNotContainNegativeValuesForPayablesCancellationWithWithHoldingTaxInvoice_DAR()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var apCreditNote = Helper.CreateTestAPCreditNote(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.KDV18W5.PK, "AP001", 1034m, Helper.TestObjectCreator.TRY, ComplianceSubTypeCodes.DAR, hasTaxMessage: true);
				AssertXMLWriterTests(apCreditNote, 35, false, 5, TurkeyEInvoiceXMLWithholdingTax_DAR);
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestTurkeyEInvoiceStructureAndContentForWithTaxExemptionInvoice_EAR()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", Helper.TestObjectCreator.FREEVAT.PK, "AR001", 1034m, Helper.TestObjectCreator.TRY, true);
				AssertXMLWriterTests(arInvoice, 32, false, 5, TurkeyEInvoiceWithTaxExemptionXML_EAR);
			}
		}

		void AssertXMLWriterTests(InvoicingBase arInvoice, int invoiceNodeCount, bool checkPricingExchangeRate, int invoiceInfoChildrenNodeCount, string expectedXML)
		{
			var invoicingBatch = Helper.CreateInvoiceBatch(arInvoice);
			var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			using (var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace))
			{
				var uInvoice = transactionBatch.TransactionCollection.First();
				Helper.SetTransactionShipment1(uInvoice);
				Helper.SetTransactionShipment2(uInvoice);

				using (var testStream = (SubStreamableStream)new MemoryStream())
				{
					new EInvoiceXmlWriter().WriteXmlToStream(transactionBatch, GlbCompany.CurrentCompany, testStream);
					testStream.Position = 0;
					var xmlDocument = new XmlDocument();
					xmlDocument.Load(testStream);

					AssertCreateTurkeyEInvoiceXml(xmlDocument, expectedXML);
					AssertXmlDocumentNodesAndAttributes(xmlDocument, invoiceNodeCount, checkPricingExchangeRate, invoiceInfoChildrenNodeCount);
				}
			}
		}

		void AssertXmlDocumentNodesAndAttributes(XmlDocument xmlDocument, int invoiceNodeCount, bool checkPricingExchangeRate, int invoiceInfoChildrenNodeCount)
		{
			var childNodes = xmlDocument.ChildNodes;
			AssertEquals(2, childNodes.Count);

			#region XML declaration node

			var declarationNode = childNodes[0];
			AssertEquals(XmlNodeType.XmlDeclaration, declarationNode.NodeType);
			AssertEquals("version=\"1.0\" encoding=\"utf-8\"", declarationNode.Value);

			#endregion

			#region Invoices

			var invoicesNode = childNodes[1];
			AssertEquals("ns0:invoices", invoicesNode.Name);

			var invoicesNodeAttributes = invoicesNode.Attributes;
			AssertEquals(1, invoicesNodeAttributes.Count);
			AssertXmlAttributeNameAndValue(invoicesNodeAttributes[0], "xmlns:ns0", "http://tempuri.org/");

			var invoicesNodeChildrenNodes = invoicesNode.ChildNodes;
			AssertEquals(1, invoicesNodeChildrenNodes.Count);

			#region InvoiceInfo

			var invoiceInfoNode = invoicesNodeChildrenNodes[0];
			AssertEquals("InvoiceInfo", invoiceInfoNode.Name);
			var invoiceInfoChildrenNodes = invoiceInfoNode.ChildNodes;
			AssertEquals(invoiceInfoChildrenNodeCount, invoiceInfoChildrenNodes.Count);

			#endregion

			#region Invoice

			var invoiceNode = invoiceInfoChildrenNodes[0];
			AssertEquals("ns0:Invoice", invoiceNode.Name);

			AssertEquals(invoiceNodeCount, invoiceNode.ChildNodes.Count);

			AssertExistNode(invoiceNode.ChildNodes, "ns3:UBLVersionID");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:CustomizationID");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:ProfileID");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:ID");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:CopyIndicator");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:UUID");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:IssueDate");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:InvoiceTypeCode");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:Note");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:DocumentCurrencyCode");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:AccountingCost");
			AssertExistNode(invoiceNode.ChildNodes, "ns3:LineCountNumeric");
			AssertNotExistNode(invoiceNode.ChildNodes, "ns3:OrderReference");
			AssertExistNode(invoiceNode.ChildNodes, "ns4:AccountingSupplierParty");
			AssertExistNode(invoiceNode.ChildNodes, "ns4:AccountingCustomerParty");
			AssertExistNode(invoiceNode.ChildNodes, "ns4:PaymentMeans");
			if (checkPricingExchangeRate)
			{
				AssertExistNode(invoiceNode.ChildNodes, "ns4:PricingExchangeRate");
			}
			AssertExistNode(invoiceNode.ChildNodes, "ns4:TaxTotal");
			AssertExistNode(invoiceNode.ChildNodes, "ns4:LegalMonetaryTotal");

			#region InvoiceLines

			AssertExistNode(invoiceNode.ChildNodes, "ns4:InvoiceLine");

			AssertExistNode(invoiceNode.ChildNodes, "ns4:InvoiceLine");
			for (int i = 0; i < invoiceNode.ChildNodes.Count - 1; i++)
			{
				if ("ns4:InvoiceLine" == invoiceNode.ChildNodes[i].Name)
				{
					AssertInvoiceLine(invoiceNode.ChildNodes[i].ChildNodes);
				}
			}

			#endregion

			#endregion

			#endregion
		}

		void AssertCreateTurkeyEInvoiceXml(XmlDocument xmlDocument, string turkeyEInvoiceExpectedXml)
		{
			var exportedXml = xmlDocument.OuterXml;
			var xmlDocumentFile = new XmlDocument();
			xmlDocumentFile.LoadXml(turkeyEInvoiceExpectedXml);
			var expectedXml = xmlDocumentFile.OuterXml;

			AssertMultilineASCIIEquals("Exported XML should match XML file", expectedXml, exportedXml);
			AssertEquals("Exported XML should match XML file", expectedXml, exportedXml);
		}

		void AssertExistNode(XmlNodeList childNodes, string nodeName)
		{
			var nodeFound = false;
			for (int i = 0; i < childNodes.Count; i++)
			{
				if (childNodes[i].Name == nodeName)
				{
					nodeFound = true;
					break;
				}
			}

			AssertEquals(nodeName + " node is not exist", nodeFound, true);
		}

		void AssertNotExistNode(XmlNodeList childNodes, string nodeName)
		{
			var nodeFound = false;
			for (int i = 0; i < childNodes.Count - 1; i++)
			{
				if (childNodes[i].Name == nodeName)
				{
					nodeFound = true;
				}
			}

			AssertEquals(nodeName + " node is exist", nodeFound, false);
		}

		void AssertInvoiceLine(XmlNodeList invoiceLines)
		{
			AssertEquals(7, invoiceLines.Count);
			AssertEquals("ns3:ID", invoiceLines[0].Name);
			AssertEquals("ns3:Note", invoiceLines[1].Name);
			AssertEquals("ns3:InvoicedQuantity", invoiceLines[2].Name);
			AssertEquals("ns3:LineExtensionAmount", invoiceLines[3].Name);
			AssertContains("ns4:TaxTotal", invoiceLines[4].Name);
			AssertEquals("ns4:Item", invoiceLines[5].Name);
			AssertEquals("ns4:Price", invoiceLines[6].Name);
		}

		void AssertXmlAttributeNameAndValue(XmlAttribute xmlAttribute, ZString expectedName, ZString expectedValue)
		{
			AssertEquals(expectedName, xmlAttribute.Name);
			AssertEquals(expectedValue, xmlAttribute.Value);
		}

		public void TestTransactionBatchWithoutAnyTransaction()
		{
			var invoicingBatch = Helper.TestObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
			var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);

			AssertEquals(0, transactionBatch.TransactionCollection.Count);

			using (var testStream = new MemoryStream())
			{
				var testLogger = new NotificationBuffer();
				AssertExceptionThrown<ArgumentException>("EInvoiceXmlWriter only accepts batches with one transaction", () => new EInvoiceXmlWriter().WriteXmlToStream(transactionBatch, GlbCompany.CurrentCompany, testStream)); //testLogger
			}
		}

		public void TestTransactionBatchWithMoreThanOneTransaction()
		{
			using (TestObjectCreator.SetTemporaryControlAccounts())
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				var job = testObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge1", testObjectCreator.AUD, 10m, testObjectCreator.Creditor1, testObjectCreator.AUD, 10m, testObjectCreator.AALSHI);
				var charge2 = testObjectCreator.CreateCharge(job, testObjectCreator.CC2, "charge2", testObjectCreator.AUD, 20m, testObjectCreator.Creditor1, testObjectCreator.AUD, 20m, testObjectCreator.AALSHI);
				Factory.Save();

				var arInvoice1 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", testObjectCreator.AUD, 1m, testObjectCreator.AALSHI);
				arInvoice1.Lines.Add(testObjectCreator.CreateRevenueLine(charge1, arInvoice1.PK));
				var arInvoice2 = testObjectCreator.CreateInvoice(typeof(ARInvoice), "AR002", testObjectCreator.AUD, 1m, testObjectCreator.AALSHI);
				arInvoice2.Lines.Add(testObjectCreator.CreateRevenueLine(charge2, arInvoice2.PK));
				Factory.Save();

				var invoicingBatch = testObjectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
				testObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice1, Core.Constants.EInvoicingPivotState.Batched);
				testObjectCreator.CreateEInvoicingTransactionPivot(invoicingBatch, arInvoice2, Core.Constants.EInvoicingPivotState.Batched);
				Factory.Save();

				var exportor = new TransactionBatchExporter(DataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
				var transactionBatch = exportor.CreateTransactionBatch(invoicingBatch, SchemaVersionManager.Current.Namespace);

				AssertEquals(2, transactionBatch.TransactionCollection.Count);

				using (var testStream = new MemoryStream())
				{
					var testLogger = new NotificationBuffer();
					AssertExceptionThrown<ArgumentException>("EInvoiceXmlWriter only accepts batches with one transaction", () => new EInvoiceXmlWriter().WriteXmlToStream(transactionBatch, GlbCompany.CurrentCompany, testStream)); //testLogger
				}
			}
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestInvoiceXmlTaxMessageWithTaxGroup_Exemption()
		{
			AssertInvoiceXmlTaxMessageWithTaxGroup(Helper.TestObjectCreator.FREEVATTG.PK, 32, TurkeyEInvoiceExportedXmlTaxGroupMapped_Exemption);
		}

		[TestDate(2020, 1, 29, 23, 8, 32)]
		public void TestInvoiceXmlTaxMessageWithTaxGroup_Withholding()
		{
			AssertInvoiceXmlTaxMessageWithTaxGroup(Helper.TestObjectCreator.KDV18W5TG.PK, 34, TurkeyEInvoiceExportedXmlTaxGroupMapped_Withholding);
		}

		void AssertInvoiceXmlTaxMessageWithTaxGroup(ZGuid taxRatePK, int invoiceNodeCount, string expectedXml)
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			using (TestObjectCreator.SetTemporaryControlAccounts())
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, Helper.TurkeyBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var arInvoice = Helper.CreateTestARInvoice(Helper.TestObjectCreator.CC14, "charge1", taxRatePK, "AR001", 1034m, Helper.TestObjectCreator.TRY);
				AssertXMLWriterTests(arInvoice, invoiceNodeCount, false, 4, expectedXml);
			}
		}

		string TurkeyEInvoiceExportedXmlTaxGroupMapped_Withholding => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(TurkeyEInvoiceExportedXmlTaxGroupMapped_Withholding)}.xml");

		string TurkeyEInvoiceExportedXmlTaxGroupMapped_Exemption => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString($"{nameof(TurkeyEInvoiceExportedXmlTaxGroupMapped_Exemption)}.xml");

		string TurkeyEInvoiceExportedXmlLocalCurrency => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceExportedXml.xml");

		string TurkeyEInvoiceExportedXmlLocalCurrency_EAR => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceExportedXml_EAR.xml");

		string TurkeyEInvoiceExportedXmlOSCurrency => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceExportedXmlOS.xml");

		string TurkeyEInvoiceExportedXmlOSCurrency_EAR => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceExportedXmlOS_EAR.xml");

		string TurkeyEInvoiceWithholdingTaxXML => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceXMLWithholdingTax.xml");

		string TurkeyEInvoiceWithholdingTaxXML_EAR => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceXMLWithholdingTax_EAR.xml");

		string TurkeyEInvoiceXMLWithholdingTax_DAR => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceXMLWithholdingTax_DAR.xml");

		string TurkeyEInvoiceWithTaxExemptionXML => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceXMLWithTaxExemption.xml");

		string TurkeyEInvoiceWithTaxExemptionXML_EAR => TurkeyEInvoiceTestHelper.GetEmbeddedResourceAsString("TurkeyEInvoiceXMLWithTaxExemption_EAR.xml");

		protected override void SetUp()
		{
			base.SetUp();

			var connection = ((IDbConnectionInternals)base.TestConnection).ADOConnection;
			var transaction = ((IDbConnectionInternals)base.TestConnection).ADOTransaction;
			DataAccess = new BatchExportDataAccess(connection, transaction);
		}

		BatchExportDataAccess DataAccess;
	}
}
