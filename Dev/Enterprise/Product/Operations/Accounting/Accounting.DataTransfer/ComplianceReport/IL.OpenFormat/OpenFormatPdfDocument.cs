using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.DocumentEngine.PdfBuilder;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.IL.OpenFormat
{
	public class OpenFormatPdfDocument
	{
		public string CompanyVATNumber { get; set; }
		public string CompanyName { get; set; }
		public string FileSavePath { get; set; }
		public string AccountingSoftWareName { get; set; }
		public string AccountingSoftWareLicenseNumber { get; set; }
		public ZDateTime DateFrom { get; set; }
		public ZDateTime DateTo { get; set; }
		public OpenFormatInfo ReportInfo { get; set; }

		public OpenFormatPdfDocument(Stream stream, IPdfBuilder builder)
		{
			outputStream = stream;
			pdfBuilder = builder;
		}

		public void Save(ZDateTime dateStamp)
		{
			var xml = $@"<scheme>
	<TextSection>
		<Texts>
			<Line>Generating files in a open format structure has been completed successfully	{CompanyName}</Line>
			<Line>Authorized dealer number:	{CompanyVATNumber}</Line>
			<Line>name of the business:	{CompanyName}</Line>
			<Line>The data was saved in the following path:	{FileSavePath}</Line>
			<Line>The date range according to which the data was produced:	{DateFrom.ToString(dateFormat, Culture.Invariant)} - {DateTo.ToString(dateFormat, Culture.Invariant)}</Line>
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
				<Line>{ReportInfo.B100Counter}</Line>
			</Row>
			<Row>
				<Line>B110</Line>
				<Line>accounts in the file</Line>
				<Line>{ReportInfo.B110Counter}</Line>
			</Row>
			<Row>
				<Line>C100</Line>
				<Line>Document title</Line>
				<Line>{ReportInfo.C100Counter}</Line>
			</Row>
			<Row>
				<Line>D110</Line>
				<Line>Document details</Line>
				<Line>{ReportInfo.D110Counter}</Line>
			</Row>
			<Row>
				<Line>D120</Line>
				<Line>Receipts (ARpayments) details</Line>
				<Line>{ReportInfo.D120Counter}</Line>
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
				<Line>{ReportInfo.ARInvoiceCounter}</Line>
				<Line>{ReportInfo.ARGSTSum.ToString("0.00")}</Line>
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
				<Line>{ReportInfo.DepositCounter}</Line>
				<Line>{ReportInfo.DepositSum.ToString("0.00")}</Line>
			</Row>
			<Row>
				<Line>420</Line>
				<Line>Bank deposit</Line>
				<Line>{ReportInfo.ARPaymentCounter}</Line>
				<Line>{ReportInfo.ARPaymentGSTSum.ToString("0.00")}</Line>
			</Row>
			<Row>
				<Line>700</Line>
				<Line>Purchase tax invoice (equipment)</Line>
				<Line>{ReportInfo.APInvoiceCounter}</Line>
				<Line>{ReportInfo.APGSTSum.ToString("0.00")}</Line>
			</Row>
		</Rows>
	</TableSection>
	<TextSection>
		<IsCentered>true</IsCentered>
		<Texts>
			<Line>Produced using software {AccountingSoftWareName}</Line>
			<Line>Registration certificate number {AccountingSoftWareLicenseNumber}</Line>
			<Line></Line>
			<Line>on Date:{dateStamp.ToString(dateFormat, Culture.Invariant)} at Time: {dateStamp.ToShortTimeString()}</Line>
		</Texts>
	</TextSection>
</scheme>";

			var elements = PdfBuilderXmlSchemeLoader.Load(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

			pdfBuilder.Build(elements, outputStream);
		}

		readonly Stream outputStream;
		readonly IPdfBuilder pdfBuilder;

		const string dateFormat = "dd/MM/yy";
	}
}
