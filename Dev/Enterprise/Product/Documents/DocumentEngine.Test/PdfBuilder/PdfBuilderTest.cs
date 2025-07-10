using System.IO;
using System.Text;
using CargoWise.IO;
using NUnit.Framework;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.IO;

namespace Enterprise.DocumentEngine.PdfBuilder.Testing
{
	sealed class ConfigurableSectionTypeListTest : TestCase
	{
		public void TestPdfGenerating()
		{
			// Arrange
			var resourceRetriever = new EmbeddedResourceRetriever();
			var expected = resourceRetriever.GetBytes(@"Enterprise.DocumentEngine.Test.PdfBuilder.Testing.Sample.pdf");

			// Act
			using var stream = new MemoryStream();
			using var schemeStream = new MemoryStream(Encoding.UTF8.GetBytes(ReportScheme));
			var pdfBuilder = new PdfBuilder();
			pdfBuilder.Build(PdfBuilderXmlSchemeLoader.Load(schemeStream), stream);
			var actual = stream.ToByteArray();

			using (var expectedDoc = PdfReader.Open(new MemoryStream(expected), PdfDocumentOpenMode.ReadOnly))
			using (var actualDoc = PdfReader.Open(new MemoryStream(actual), PdfDocumentOpenMode.ReadOnly))
			{
				AssertEquals("Output PDF has the same number of pages", expectedDoc.Pages.Count, actualDoc.Pages.Count);

				for (int i = 0; i < expectedDoc.Pages.Count; i++)
				{
					var expectedPage = expectedDoc.Pages[i];
					var actualPage = actualDoc.Pages[i];

					AssertEquals("Output PDF has the same content", PdfTestHelper.GetTextContent(expectedPage), PdfTestHelper.GetTextContent(actualPage));

					AssertEquals(ContentReader.ReadContent(expectedPage).ToContent(), ContentReader.ReadContent(actualPage).ToContent());
				}
			}
		}

		const string ReportScheme = @"
<scheme>
    <TextSection>
        <Texts>
            <Line>Generating files in a open format structure has been completed successfully	Cargowise</Line>
            <Line>Authorized dealer number:	23456789</Line>
            <Line>name of the business:	Cargowise</Line>
            <Line>The data was saved in the following path:	c:\temp</Line>
            <Line>The date range according to which the data was produced:	01/01/25 - 31/12/25</Line>
        </Texts>
    </TextSection>
    <TableSection>
        <Widths>
            <Value>100</Value>
            <Value>200</Value>
            <Value>100</Value>
        </Widths>
        <Headers>
            <Line>Record code</Line>
            <Line>Record description</Line>
            <Line>total records</Line>
        </Headers>
        <Rows>
            <Row>
                <Line>A100</Line>
                <Line>Opening Record</Line>
                <Line>1</Line>
            </Row>    
            <Row>
                <Line>B100</Line>
                <Line>Transactions in accounting</Line>
                <Line>10</Line>
            </Row>    
            <Row>
                <Line>B110</Line>
                <Line>accounts in the file</Line>
                <Line>123</Line>
            </Row>    
            <Row>
                <Line>C100</Line>
                <Line>Document title</Line>
                <Line>0</Line>
            </Row>    
            <Row>
                <Line>D110</Line>
                <Line>Document details</Line>
                <Line>23</Line>
            </Row>    
            <Row>
                <Line>D120</Line>
                <Line>Receipts (ARpayments) details</Line>
                <Line>12</Line>
            </Row>    
            <Row>
                <Line>M100</Line>
                <Line>Items in stock</Line>
                <Line>0</Line>
            </Row>    
            <Row>
                <Line>Z900</Line>
                <Line>End record</Line>
                <Line>1</Line>
            </Row>    
        </Rows>
    </TableSection>
    <TableSection>
        <Widths>
            <Value>100</Value>
            <Value>100</Value>
            <Value>100</Value>
            <Value>100</Value>
        </Widths>
        <Headers>
            <Line>Document type</Line>
            <Line>Description</Line>
            <Line>Amount of documents</Line>
            <Line>Amount in NIS</Line>
        </Headers>
        <Alignments>
            <Alignment>Left</Alignment>
            <Alignment>Left</Alignment>
            <Alignment>Right</Alignment>
            <Alignment>Right</Alignment>
        </Alignments>
        <Rows>
            <Row>
                <Line>305</Line>
                <Line>Tax invoice (ARinvoices)</Line>
                <Line>5</Line>
                <Line>300.00</Line>
            </Row>    
            <Row>
                <Line>330</Line>
                <Line>Tax invoice - credit</Line>
                <Line></Line>
                <Line></Line>
            </Row>    
            <Row>
                <Line>400</Line>
                <Line>Receipts (ARpayments)</Line>
                <Line>2</Line>
                <Line>400.00</Line>
            </Row>    
            <Row>
                <Line>420</Line>
                <Line>Bank deposit</Line>
                <Line>5</Line>
                <Line>23300.99</Line>
            </Row>    
            <Row>
                <Line>700</Line>
                <Line>Purchase tax invoice (equipment)</Line>
                <Line>12</Line>
                <Line>123300.00</Line>
            </Row>    
        </Rows>
    </TableSection>
	<TextSection>
		<IsCentered>true</IsCentered>
        <Texts>
            <Line>Produced using software Cargowise</Line>
            <Line>Registration certificate number 223-4456</Line>
            <Line></Line>
			<Line>on  Date:01/01/25 at Time: 08:00</Line>
        </Texts>
    </TextSection>
</scheme>";
	}
}
