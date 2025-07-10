using System.IO;
using System.Text;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.PdfBuilder.Testing
{
	sealed class PdfBuilderXmlSchemeLoaderTest : TestCase
	{
		const string ReportScheme = @"
<scheme>
	<TextSection>
		<IsCentered>true</IsCentered>
		<Texts>
			<Line>Authorized dealer number:23456789</Line>
			<Line>name of the business:Cargowise</Line>
		</Texts>
	</TextSection>
	<TableSection>
		<Widths>
			<Value>100</Value>
			<Value>200</Value>
		</Widths>
		<Headers>
			<Line>Record code</Line>
			<Line>Record description</Line>
		</Headers>
		<Alignments>
			<Alignment>Left</Alignment>
			<Alignment>Right</Alignment>
		</Alignments>
		<Rows>
			<Row>
				<Line>A100</Line>
				<Line>Opening Record</Line>
			</Row>
			<Row>
				<Line>B100</Line>
				<Line>Transactions in accounting</Line>
			</Row>
		</Rows>
	</TableSection>
</scheme>";

		PdfElement[] GetReportElements()
		{
			var textSection = new PdfTextSection { IsCentered = true };
			textSection.Texts.AddRange(new string[]
			{
				(NoResString)"Authorized dealer number:23456789",
				(NoResString)"name of the business:Cargowise"
			});

			var tableSection = new PdfTableSection();
			tableSection.Widths.AddRange(new[]
			{
				100,
				200
			});
			tableSection.Headers.AddRange(new[]
			{
				"Record code",
				"Record description"
			});
			tableSection.Alignments.AddRange(new[]
			{
				ColumnAlignment.Left,
				ColumnAlignment.Right
			});
			tableSection.Rows.AddRange(new[]
			{
				new []
				{
					"A100",
					"Opening Record"
				},
				new []
				{
					"B100",
					"Transactions in accounting"
				}
			});

			return new PdfElement[] { textSection, tableSection };
		}

		public void TestLoading()
		{
			// Arrange
			using var schemeStream = new MemoryStream(Encoding.UTF8.GetBytes(ReportScheme));

			// Act
			var actual = PdfBuilderXmlSchemeLoader.Load(schemeStream);
			var expected = GetReportElements();

			// Assert
			AssertContainsExactElementsInExactOrder(actual, expected);
		}
	}
}
