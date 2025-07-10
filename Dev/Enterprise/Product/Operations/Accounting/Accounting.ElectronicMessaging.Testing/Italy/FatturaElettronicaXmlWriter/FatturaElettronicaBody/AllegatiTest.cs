using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using WTG.TestHelpers.Xml;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class AllegatiTest : TestCaseWithFactory
	{
		public void TestAllegati()
		{
			var document2 = new AttachedDocument();
			var type2 = new DocumentType();
			type2.Code = "EXD";
			type2.Description = "Exporter Documents";
			document2.Type = type2;
			document2.FileName = "TestExporterDocumentsFile.pdf";
			document2.IsPublished = true;
			document2.VisibleBranchCode = GlbBranch.CurrentBranch.GB_Code;
			document2.VisibleCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			document2.VisibleDepartmentCode = GlbDepartment.CurrentDepartment.GE_Code;
			document2.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 9, 8, 7 });
			transaction.AttachedDocumentCollection.Add(document2);

			var expectedXmlResult = $@"
<FatturaElettronicaBody>
  <Allegati>
    <NomeAttachment>TestCartageAdviceFile</NomeAttachment>
    <FormatoAttachment>CAD</FormatoAttachment>
    <DescrizioneAttachment>Cartage Advice</DescrizioneAttachment>
    <Attachment>AQID</Attachment>
  </Allegati>
  <Allegati>
    <NomeAttachment>TestExporterDocumentsFile.pdf</NomeAttachment>
    <FormatoAttachment>pdf</FormatoAttachment>
    <DescrizioneAttachment>Exporter Documents</DescrizioneAttachment>
    <Attachment>CQgH</Attachment>
  </Allegati>
</FatturaElettronicaBody>";

			var actualXmlResult = new XStreamingElement("FatturaElettronicaBody", new Allegati().BuildXML(transaction)).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestAllegatiWithTooLongNomeAttachment()
		{
			document.FileName = string60 + "ijk";

			var expectedXmlResult = $@"
<FatturaElettronicaBody>
  <Allegati>
    <NomeAttachment>{string60}</NomeAttachment>
    <FormatoAttachment>CAD</FormatoAttachment>
    <DescrizioneAttachment>Cartage Advice</DescrizioneAttachment>
    <Attachment>AQID</Attachment>
  </Allegati>
</FatturaElettronicaBody>";

			var actualXmlResult = new XStreamingElement("FatturaElettronicaBody", new Allegati().BuildXML(transaction)).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestAllegatiWithTooLongDescrizioneAttachment()
		{
			type.Description = string100 + "wxyza";

			var expectedXmlResult = $@"
<FatturaElettronicaBody>
  <Allegati>
    <NomeAttachment>TestCartageAdviceFile</NomeAttachment>
    <FormatoAttachment>CAD</FormatoAttachment>
    <DescrizioneAttachment>{string100}</DescrizioneAttachment>
    <Attachment>AQID</Attachment>
  </Allegati>
</FatturaElettronicaBody>";

			var actualXmlResult = new XStreamingElement("FatturaElettronicaBody", new Allegati().BuildXML(transaction)).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		public void TestAllegatiWithSpecialCharactersInLatinStringFields()
		{
			document.FileName = "€€Test";
			type.Description = "Test€€";

			var expectedXmlResult = $@"
<FatturaElettronicaBody>
  <Allegati>
    <NomeAttachment>  Test</NomeAttachment>
    <FormatoAttachment>CAD</FormatoAttachment>
    <DescrizioneAttachment>Test  </DescrizioneAttachment>
    <Attachment>AQID</Attachment>
  </Allegati>
</FatturaElettronicaBody>";

			var actualXmlResult = new XStreamingElement("FatturaElettronicaBody", new Allegati().BuildXML(transaction)).ToString();
			XmlComparison.CompareAndAssertXml(expectedXmlResult, actualXmlResult);
		}

		protected override void SetUp()
		{
			base.SetUp();

			transaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			transaction.DataContext = DataContextFactory.New();
			transaction.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			transaction.Ledger = "AR";
			transaction.TransactionType = TransactionType.INV;
			transaction.Number = "00001234";
			transaction.OSCurrency = new Currency();
			transaction.OSCurrency.Code = "EUR";
			transaction.TransactionDate = new ZDateTime(2018, 10, 15);
			transaction.SetAttachedDocumentCollection(() => new List<AttachedDocument>());

			type = new DocumentType();
			type.Code = "CAD";
			type.Description = "Cartage Advice";

			document = new AttachedDocument();
			document.Type = type;
			document.FileName = "TestCartageAdviceFile";
			document.IsPublished = true;
			document.VisibleBranchCode = GlbBranch.CurrentBranch.GB_Code;
			document.VisibleCompanyCode = GlbCompany.CurrentCompany.GC_Code;
			document.VisibleDepartmentCode = GlbDepartment.CurrentDepartment.GE_Code;
			document.ImageData = (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 });
			transaction.AttachedDocumentCollection.Add(document);
		}

		protected override void TearDown()
		{
			base.TearDown();
			transaction?.Dispose();
		}

		TransactionInfo transaction;
		DocumentType type;
		AttachedDocument document;
		readonly string string60 = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefgh";
		readonly string string100 = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuv";
	}
}
