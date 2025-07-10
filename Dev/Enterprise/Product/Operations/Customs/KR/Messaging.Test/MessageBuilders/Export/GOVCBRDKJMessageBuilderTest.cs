using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBRDKJMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IExportCancellationHeader> exportMock;
		Mock<IAmendmentDetails> exportDetailMock;
		protected override void SetUp()
		{
			base.SetUp();
			exportMock = new Mock<IExportCancellationHeader>();
			exportMock.Setup(m => m.ExportDeclarationNumber).Returns("1234520100523X");
			exportMock.Setup(m => m.DeclarationCustomsOffice).Returns("040");
			exportMock.Setup(m => m.DeclarationCustomsDivision).Returns("15");

			var exporterMock = new Mock<IOrganization>();
			exporterMock.Setup(m => m.CompanyName).Returns("(주)레디코리아");

			exporterMock.Setup(m => m.UnipassIDForOrganization).Returns("레디코리1971018");
			exportMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			exportMock.Setup(m => m.UnipassDeclarantID).Returns("신고인부호");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 02, 02)]
		public void TestGenerateCancellationMessage()
		{
			exportDetailMock = new Mock<IAmendmentDetails>();
			exportDetailMock.Setup(m => m.FaultParty).Returns("A");
			exportDetailMock.Setup(m => m.ReasonCode).Returns("21");
			exportDetailMock.Setup(m => m.AmendReasonDescription).Returns("L/C (계약)취소");
			exportDetailMock.Setup(m => m.AmendmentVersionNo).Returns(1);

			var result = new GOVCBRDKJMessageBuilder(exportMock.Object, exportDetailMock.Object).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ExportOutgoingTestFilePath, "GOVCBRDKJ.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("04015", result.DeclarationOfficeId.Value);
			AssertEquals("1234520100523X", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBRDKJ", result.TypeCode.Value);
			AssertEquals("1", result.VersionId.Value);
			AssertEquals("B", result.TransactionNatureCode.Value);
			AssertEquals("A", result.Reason.Value);
			AssertEquals("21", result.AdditionalInformation.StatementCode.Value);
			AssertEquals("L/C (계약)취소", result.AdditionalInformation.StatementDescription.Value);

			AssertEquals("레디코리1971018", result.Exporter.Id.Value);
			AssertEquals("(주)레디코리아", result.Exporter.Name.Value);
			AssertEquals("신고인부호", result.Submitter.Id.Value);
			exportDetailMock.VerifyAll();
			exportMock.VerifyAll();
		}

		public void TestExporterNotNull()
		{
			exportDetailMock = new Mock<IAmendmentDetails>();
			var result = new GOVCBRDKJMessageBuilder(exportMock.Object, exportDetailMock.Object).GenerateMessage();
			AssertNotNull("Exporter should not be null", result.Exporter);
			AssertNotNull("Exporter name not be null", result.Exporter.Name);
		}
	}
}
