using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5SIDataProvidersTest : XMLMessageTestHelper<GOVCBR5SIDataProvidersTest>
	{
		[TestDate(2019, 07, 24)]
		public void TestSerialisationAndDeserialisationRealData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "037";
			declaration.JE_CustomsDivision = "10";

			var invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "4163419073087M";

			var parcels = invoice.Parcels.AddNew();
			parcels.CSI_ReferenceNumber = "03723191558592";
			parcels.CSI_ReferenceNumber2 = "CD256851653JP";
			parcels.CSI_Code = "A";

			var parcels2 = invoice.Parcels.AddNew();
			parcels2.CSI_ReferenceNumber = "03723191558612";
			parcels2.CSI_ReferenceNumber2 = "CD256855006JP";
			parcels2.CSI_Code = "A";

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();

			var parcels3 = invoice2.Parcels.AddNew();
			parcels3.CSI_ReferenceNumber = "03723191558602";
			parcels3.CSI_ReferenceNumber2 = "CD256851640JP";
			parcels3.CSI_Code = "A";

			var parcels4 = invoice2.Parcels.AddNew();
			parcels4.CSI_ReferenceNumber = "03723191558622";
			parcels4.CSI_ReferenceNumber2 = "CD256851667JP";
			parcels4.CSI_Code = "A";
			invoiceLine.JI_CL = entry.MergedLines.AddNew().PK;
			invoiceLine2.JI_CL = entry.MergedLines.AddNew().PK;

			var import5SIHeader = new Import5SIHeaderCreator().Create(entry);
			Factory.Save();

			var result = new GOVCBR5SIMessageBuilder(import5SIHeader).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5SIDataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5SI_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		[TestDate(2014, 05, 06)]
		public void TestSerialisationAndDeserialisation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "010";
			declaration.JE_CustomsDivision = "20";

			var invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "9999907000001X";

			var parcels = invoice.Parcels.AddNew();
			parcels.CSI_ReferenceNumber = "0100254646646";
			parcels.CSI_ReferenceNumber2 = "EM123456789KR";
			parcels.CSI_Code = "A";

			var invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();

			var parcels2 = invoice2.Parcels.AddNew();
			parcels2.CSI_ReferenceNumber = "0100254747747";
			parcels2.CSI_ReferenceNumber2 = "EM123457789KR";
			parcels2.CSI_Code = "B";
			invoiceLine.JI_CL = entry.MergedLines.AddNew().PK;
			invoiceLine2.JI_CL = entry.MergedLines.AddNew().PK;

			var import5SIHeader = new Import5SIHeaderCreator().Create(entry);
			Factory.Save();

			var result = new GOVCBR5SIMessageBuilder(import5SIHeader).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5SIDataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5SI_D2.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
