using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5ASMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IExportAmendmentHeader> exportMock;
		Mock<IAmendmentDetails> exportDetailMock;
		protected override void SetUp()
		{
			base.SetUp();
			exportMock = new Mock<IExportAmendmentHeader>();
			exportMock.Setup(m => m.ExportDeclarationNumber).Returns("1234520100523X");
			exportMock.Setup(m => m.DeclarationCustomsOffice).Returns("040");
			exportMock.Setup(m => m.DeclarationCustomsDivision).Returns("15");

			var exporterMock = new Mock<IOrganization>();
			exporterMock.Setup(m => m.CompanyName).Returns("(주)레디코리아");

			exporterMock.Setup(m => m.UnipassIDForOrganization).Returns("레디코리1971018");
			exportMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			exportMock.Setup(m => m.UnipassDeclarantID).Returns("신고인부호");
		}
		Mock<IExportAmendmentHeader> EntryDataByThreeAmendType(string amendType)
		{
			var exportItem1 = new Mock<IExport5ASItem>();
			var exportItem2 = new Mock<IExport5ASItem>();

			if (amendType == "Amendment")
			{
				exportItem1.Setup(m => m.EntryLineNo).Returns("001");
				exportItem1.Setup(m => m.LineAmendType).Returns("01");
				exportItem1.Setup(m => m.AmendDataItemID).Returns("321");
				exportItem1.Setup(m => m.LineDetailNo).Returns("01");
				exportItem1.Setup(m => m.ContainerSequenceNo).Returns("01");
				exportItem1.Setup(m => m.SequenceNo).Returns(01);
				exportItem1.Setup(m => m.VINSequenceNo).Returns("001");
				exportItem1.Setup(m => m.BeforeDescription).Returns("정정전1");
				exportItem1.Setup(m => m.AfterDescription).Returns("정정후1");
				exportItem1.Setup(m => m.VINSequenceNo).Returns("001");
				exportItem1.Setup(m => m.RegulationCategorySequnceNo).Returns("01");

				exportItem2.Setup(m => m.EntryLineNo).Returns("002");
				exportItem2.Setup(m => m.LineAmendType).Returns("02");
				exportItem2.Setup(m => m.AmendDataItemID).Returns("123");
				exportItem2.Setup(m => m.LineDetailNo).Returns("02");
				exportItem2.Setup(m => m.ContainerSequenceNo).Returns("02");
				exportItem2.Setup(m => m.SequenceNo).Returns(02);
				exportItem2.Setup(m => m.VINSequenceNo).Returns("002");
				exportItem2.Setup(m => m.BeforeDescription).Returns("정정전2");
				exportItem2.Setup(m => m.AfterDescription).Returns("정정후2");
				exportItem2.Setup(m => m.VINSequenceNo).Returns("002");
				exportItem2.Setup(m => m.RegulationCategorySequnceNo).Returns("01");
				exportMock.Setup(m => m.AmendmentItems).Returns(new IExport5ASItem[] { exportItem1.Object, exportItem2.Object });
			}
			else if (amendType == "Extend")
			{
				exportItem1.Setup(m => m.EntryLineNo).Returns("001");
				exportItem1.Setup(m => m.LineAmendType).Returns("");
				exportItem1.Setup(m => m.AmendDataItemID).Returns("321");
				exportItem1.Setup(m => m.LineDetailNo).Returns("01");
				exportItem1.Setup(m => m.ContainerSequenceNo).Returns(ZString.Empty);
				exportItem1.Setup(m => m.SequenceNo).Returns(new ZInt(""));
				exportItem1.Setup(m => m.BeforeDescription).Returns("정정전1");
				exportItem1.Setup(m => m.AfterDescription).Returns("정정후1");
				exportItem1.Setup(m => m.RegulationCategorySequnceNo).Returns(ZString.Empty);
				exportItem1.Setup(m => m.VINSequenceNo).Returns(ZString.Empty);

				exportItem2.Setup(m => m.EntryLineNo).Returns("002");
				exportItem2.Setup(m => m.LineAmendType).Returns("");
				exportItem2.Setup(m => m.AmendDataItemID).Returns("123");
				exportItem2.Setup(m => m.LineDetailNo).Returns("02");
				exportItem2.Setup(m => m.ContainerSequenceNo).Returns(ZString.Empty);
				exportItem2.Setup(m => m.SequenceNo).Returns(new ZInt(""));
				exportItem2.Setup(m => m.BeforeDescription).Returns("정정전2");
				exportItem2.Setup(m => m.AfterDescription).Returns("정정후2");
				exportItem2.Setup(m => m.RegulationCategorySequnceNo).Returns(ZString.Empty);
				exportItem2.Setup(m => m.VINSequenceNo).Returns(ZString.Empty);

				exportMock.Setup(m => m.AmendmentItems).Returns(new IExport5ASItem[] { exportItem1.Object, exportItem2.Object });
			}
			return exportMock;
		}

		public void TestGenerateDeclarationForDateOfFinalPrice()
		{
			var export = EntryDataByThreeAmendType(nameof(MessageFunctions.MessageFunctionCode.Amendment));

			exportDetailMock = new Mock<IAmendmentDetails>();
			var result = new GOVCBR5ASMessageBuilder(export.Object, exportDetailMock.Object, MessageFunctions.MessageFunctionCode.Amendment).GenerateMessage();

			AssertEquals(null, result.AcceptanceDateTime);

			exportDetailMock.Setup(m => m.DateOfFinalPrice).Returns(new ZDate("2022-11-01"));
			result = new GOVCBR5ASMessageBuilder(export.Object, exportDetailMock.Object, MessageFunctions.MessageFunctionCode.Amendment).GenerateMessage();
			AssertEquals("20221101", result.AcceptanceDateTime);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 02, 02)]
		public void TestGenerateDeclarationWhenAmendTypeIsA()
		{
			var export = EntryDataByThreeAmendType(nameof(MessageFunctions.MessageFunctionCode.Amendment));

			exportDetailMock = new Mock<IAmendmentDetails>();
			exportDetailMock.Setup(m => m.FaultParty).Returns("E");
			exportDetailMock.Setup(m => m.ReasonCode).Returns("11");
			exportDetailMock.Setup(m => m.AmendReasonDescription).Returns("인도조건 정정");
			exportDetailMock.Setup(m => m.AmendmentVersionNo).Returns(1);

			var result = new GOVCBR5ASMessageBuilder(export.Object, exportDetailMock.Object, MessageFunctions.MessageFunctionCode.Amendment).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ExportOutgoingTestFilePath, "GOVCBR5AS_EM_TypeIsA.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals(null, result.AcceptanceDateTime);
			AssertEquals("04015", result.DeclarationOfficeId.Value);
			AssertEquals("1234520100523X", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5AS", result.TypeCode.Value);
			AssertEquals("1", result.VersionId.Value);
			AssertEquals("A", result.TransactionNatureCode.Value);
			AssertEquals("E", result.Reason.Value);
			AssertEquals("11", result.AdditionalInformation.StatementCode.Value);
			AssertEquals("인도조건 정정", result.AdditionalInformation.StatementDescription.Value);

			AssertEquals("01", result.Consignment[0].AdditionalDocument.CriteriaConformanceId.Value);
			AssertEquals("02", result.Consignment[1].Amendment.ChangeReasonCode.Value);
			AssertEquals("정정전1", result.Consignment[0].Amendment.StatementDescription.Value);
			AssertEquals("정정후2", result.Consignment[1].Amendment.AdjustmentDescription.Value);
			AssertEquals("001", result.Consignment[0].Amendment.Pointer.SequenceNumeric.Value);
			AssertEquals("02", result.Consignment[1].Amendment.Pointer.DocumentSectionCode.Value);
			AssertEquals("321", result.Consignment[0].Amendment.Pointer.TagId.Value);

			AssertEquals("001", result.Consignment[0].ConsignmentItem.Commodity.Id.Value);
			AssertEquals("02", result.Consignment[1].TransportEquipment.Id.Value);

			AssertEquals("레디코리1971018", result.Exporter.Id.Value);
			AssertEquals("(주)레디코리아", result.Exporter.Name.Value);
			AssertEquals("신고인부호", result.Submitter.Id.Value);
			exportDetailMock.VerifyAll();
			exportMock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 02, 02)]
		public void TestGenerateDeclarationWhenAmendTypeIsC()
		{
			var export = EntryDataByThreeAmendType(nameof(MessageFunctions.MessageFunctionCode.Extend));

			exportDetailMock = new Mock<IAmendmentDetails>();
			exportDetailMock.Setup(m => m.FaultParty).Returns("C");
			exportDetailMock.Setup(m => m.ReasonCode).Returns("23");
			exportDetailMock.Setup(m => m.AmendReasonDescription).Returns("기간연장 신청");
			exportDetailMock.Setup(m => m.AmendmentVersionNo).Returns(1);

			var result = new GOVCBR5ASMessageBuilder(exportMock.Object, exportDetailMock.Object, MessageFunctions.MessageFunctionCode.Extend).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ExportOutgoingTestFilePath, "GOVCBR5AS_EM_TypeIsC.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("04015", result.DeclarationOfficeId.Value);
			AssertEquals("1234520100523X", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5AS", result.TypeCode.Value);
			AssertEquals("1", result.VersionId.Value);
			AssertEquals("C", result.TransactionNatureCode.Value);
			AssertEquals("C", result.Reason.Value);
			AssertEquals("23", result.AdditionalInformation.StatementCode.Value);
			AssertEquals("기간연장 신청", result.AdditionalInformation.StatementDescription.Value);

			AssertEquals(null, result.Consignment[1].Amendment.ChangeReasonCode);
			AssertEquals("정정전1", result.Consignment[0].Amendment.StatementDescription.Value);
			AssertEquals("정정후2", result.Consignment[1].Amendment.AdjustmentDescription.Value);
			AssertEquals("001", result.Consignment[0].Amendment.Pointer.SequenceNumeric.Value);
			AssertEquals("02", result.Consignment[1].Amendment.Pointer.DocumentSectionCode.Value);
			AssertEquals("321", result.Consignment[0].Amendment.Pointer.TagId.Value);

			AssertEquals("레디코리1971018", result.Exporter.Id.Value);
			AssertEquals("(주)레디코리아", result.Exporter.Name.Value);
			AssertEquals("신고인부호", result.Submitter.Id.Value);
			exportDetailMock.VerifyAll();
			exportMock.VerifyAll();
		}

		public void TestConditionalValuesEmpty()
		{
			var export = EntryDataByThreeAmendType(nameof(MessageFunctions.MessageFunctionCode.Amendment));
			var exporterMock = new Mock<IOrganization>();

			exportMock.Setup(m => m.Exporter).Returns(exporterMock.Object);
			exportMock.Setup(m => m.UnipassDeclarantID).Returns("");
			var exportItem1 = new Mock<IExport5ASItem>();
			exportItem1.Setup(m => m.EntryLineNo).Returns("001");
			exportMock.Setup(m => m.AmendmentItems).Returns(new IExport5ASItem[] { exportItem1.Object });

			exportDetailMock = new Mock<IAmendmentDetails>();
			var result = new GOVCBR5ASMessageBuilder(export.Object, exportDetailMock.Object, MessageFunctions.MessageFunctionCode.Amendment).GenerateMessage();

			AssertEquals(null, result.Exporter.Id);
			AssertEquals(null, result.Submitter);
			AssertEquals(null, result.AcceptanceDateTime);
			AssertEquals(null, result.Consignment[0].Amendment.ChangeReasonCode);
			AssertEquals(null, result.Consignment[0].TransportEquipment);
			AssertEquals(null, result.Consignment[0].AdditionalDocument);
			AssertEquals(null, result.Consignment[0].ConsignmentItem);
		}

		public void TestExporterNotNull()
		{
			var export = EntryDataByThreeAmendType(nameof(MessageFunctions.MessageFunctionCode.Amendment));
			exportDetailMock = new Mock<IAmendmentDetails>();
			var result = new GOVCBR5ASMessageBuilder(export.Object, exportDetailMock.Object, MessageFunctions.MessageFunctionCode.Amendment).GenerateMessage();
			AssertNotNull("Exporter should not be null", result.Exporter);
			AssertNotNull("Exporter name not be null", result.Exporter.Name);
		}
	}
}
