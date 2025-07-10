using System;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	[TestedType(typeof(XmlAccountingTransactionExporter))]
	public class XmlAccountingTransactionExporterTest : AccountingTransactionsDataExporterTestCase
	{
		public void TestExportedXmlIsValid()
		{
			Exporter.FilterProvider.CurrentBatchNo = 0;
			Exporter.FilterProvider.IncludeAccrualsPosting = true;
			Exporter.FilterProvider.IncludeAccrualsReversing = true;
			Exporter.FilterProvider.IncludeAPAdjustmentNotes = true;
			Exporter.FilterProvider.IncludeAPCreditNotes = true;
			Exporter.FilterProvider.IncludeAPInvoices = true;
			Exporter.FilterProvider.IncludeARAdjustmentNotes = true;
			Exporter.FilterProvider.IncludeARCreditNotes = true;
			Exporter.FilterProvider.IncludeARInvoices = true;
			Exporter.FilterProvider.IncludeWIPsPosting = true;
			Exporter.FilterProvider.IncludeWIPsReversing = true;

			XmlValidator validator = new XmlValidator(AccountingXmlSchemaDefinitions.Instance.FinancialTransactionsSchema);

			using (TempFile exportedXmlFile = TempFile.New())
			{
				using (FileStream exportedXmlFileStream = new FileStream(exportedXmlFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					ExporterWithoutInterchange.Export(exportedXmlFileStream);
				}

				XmlDocument doc = new XmlDocument();

				using (FileStream exportedXmlFileStream = new FileStream(exportedXmlFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					doc.Load(exportedXmlFileStream);
				}

				NotificationBuffer notify = new NotificationBuffer();
				validator.Validate(doc.InnerXml, notify);

				Assert("The expected Document was invalid", !notify.HasErrors);
			}
		}

		public void TestExportOnFileClosed()
		{
			using (TempFile tempXmlFile = TempFile.New())
			{
				using (FileStream exportedXmlFileStream = new FileStream(tempXmlFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					exportedXmlFileStream.SafeFileHandle.Close();
					AssertNoExceptionThrown(() => Exporter.Export(exportedXmlFileStream));
				}
			}
		}

		public void TestInterchange()
		{
			const string XmlDocumentHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n";

			using (TempFile xmlFile = TempFile.New())
			{
				XmlAccountingTransactionExporterWithOnlyInterchange exporter = new XmlAccountingTransactionExporterWithOnlyInterchange(Factory);

				using (FileStream xmlFileStream = new FileStream(xmlFile.Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				{
					exporter.Export(xmlFileStream);
				}

				ZString xmlDocString = String.Empty;
				using (StreamReader reader = new StreamReader(xmlFile.Filename))
				{
					xmlDocString = reader.ReadToEnd();
				}

				Assert("Doc must not be empty", xmlDocString != String.Empty);

				Assert("Should start with xml header", xmlDocString.StartsWith(XmlDocumentHeader));
				xmlDocString = xmlDocString.Replace(XmlDocumentHeader, String.Empty);
				Assert("Should start with <XmlInterchange", xmlDocString.StartsWith("<XmlInterchange "));
				Assert("Should end with </XmlInterchange>", xmlDocString.EndsWith("</Payload>\r\n</XmlInterchange>"));
			}
		}

		#region AccountingTransactionsDataExporterTestBase

		protected override AccountingTransactionsDataExporter NewExporter
		{
			get { return Exporter; }
		}

		protected override bool ShouldTestMessagesToDisplayWhenExportIsFinished()
		{
			return true;
		}

		#endregion

		#region NonPersistenBusinessObjectTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return new XmlAccountingTransactionExporter(Factory);
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Exporter = new XmlAccountingTransactionExporter(Factory);
			ExporterWithoutInterchange = new XmlAccountingTransactionExporterWithoutInterchange(Factory);
		}

		XmlAccountingTransactionExporter Exporter;
		XmlAccountingTransactionExporterWithoutInterchange ExporterWithoutInterchange;

		class XmlAccountingTransactionExporterWithoutInterchange : XmlAccountingTransactionExporter
		{
			public XmlAccountingTransactionExporterWithoutInterchange(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override void EnsureDocumentStreamPositionForPayload()
			{
			}

			protected override void WriteEndOfDocument()
			{
			}
		}

		class XmlAccountingTransactionExporterWithOnlyInterchange : XmlAccountingTransactionExporter
		{
			public XmlAccountingTransactionExporterWithOnlyInterchange(BusinessObjectFactory factory) : base(factory)
			{
			}

			protected override void ExportObjectsToEndPoint(BusinessObject bizObj, IValueObjectDataAdapter dataAdapter, ZString status)
			{
				//do nothing :)
			}

			protected override void InitialiseDocumentWriter(Stream documentStream)
			{
				Document = documentStream;
				InitialiseXmlDocumentWriter();
			}
		}

		#endregion
	}
}
