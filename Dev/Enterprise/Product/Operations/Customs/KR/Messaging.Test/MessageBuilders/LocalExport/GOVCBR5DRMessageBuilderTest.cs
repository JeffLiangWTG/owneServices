using System.IO;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5DRMessageBuilderTest : TestCaseWithFactory
	{
		Mock<ILocalExportAmendEntryHeader> localExportHeaderMock;
		Mock<IAmendmentDetails> localExportAmendMock;

		protected override void SetUp()
		{
			localExportHeaderMock = new Mock<ILocalExportAmendEntryHeader>();
			localExportHeaderMock.Setup(m => m.DeclarationNumber).Returns("1083699012345");
			localExportHeaderMock.Setup(m => m.DeclarationCustomsOfficeAndDivision).Returns("01022");
			localExportHeaderMock.Setup(m => m.CustomsReceiptNumber).Returns("00000000000000");

			#region Supplier
			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.BusinessRegNo).Returns("9999999999");
			supplierMock.Setup(m => m.UnipassIDForOrganization).Returns("99999999");

			localExportHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);
			#endregion

			#region IAmendmentDetails
			localExportAmendMock = new Mock<IAmendmentDetails>();
			localExportAmendMock.Setup(m => m.ReasonCode).Returns("1");
			#endregion
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 02, 09)]
		public void TestGenerateDeclaration()
		{
			#region AmendedItems
			var localExportLinerMock1 = new Mock<ILocalExportAmendItem>();
			localExportLinerMock1.Setup(m => m.DataItemNo).Returns("16");
			localExportLinerMock1.Setup(m => m.ItemSequenceNumber).Returns(10);
			localExportLinerMock1.Setup(m => m.AmendType).Returns("3");
			localExportLinerMock1.Setup(m => m.BeforeValue).Returns("1168");
			localExportLinerMock1.Setup(m => m.AfterValue).Returns("1165");

			var localExportLinerMock2 = new Mock<ILocalExportAmendItem>();
			localExportLinerMock2.Setup(m => m.DataItemNo).Returns(ZString.Empty);
			localExportLinerMock2.Setup(m => m.ItemSequenceNumber).Returns(312);
			localExportLinerMock2.Setup(m => m.AmendType).Returns("2");
			localExportLinerMock2.Setup(m => m.BeforeValue).Returns(ZString.Empty);
			localExportLinerMock2.Setup(m => m.AfterValue).Returns(ZString.Empty);

			localExportHeaderMock.Setup(m => m.AmendedItems).Returns(new ILocalExportAmendItem[] { localExportLinerMock1.Object, localExportLinerMock2.Object });
			#endregion

			#region IAmendmentDetails
			localExportAmendMock.Setup(m => m.AmendReasonDescription).Returns("사유내용");
			#endregion

			var result = new GOVCBR5DRMessageBuilder(localExportHeaderMock.Object, localExportAmendMock.Object, MessageFunctions.MessageFunctionCode.Amendment).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.LocalExportOutgoingTestFilePath, "GOVCBR5DR_0.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("01022", result.DeclarationOfficeId.Value);
			AssertEquals("1083699012345", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5DR", result.TypeCode.Value);
			AssertEquals(MessageSubTypeLocalExport.Amendment, result.TransactionNatureCode.Value);
			AssertEquals("사유내용", result.Reason.Value);
			AssertEquals("1", result.ReasonCode.Value);
			AssertEquals("00000000000000", result.AdditionalDocument.Id.Value);

			AssertEquals("3", result.Amendment[0].ChangeReasonCode.Value);
			AssertEquals("1168", result.Amendment[0].StatementDescription.Value);
			AssertEquals("1165", result.Amendment[0].AdjustmentDescription.Value);
			AssertEquals(10m, result.Amendment[0].Pointer.SequenceNumeric);
			AssertEquals("16", result.Amendment[0].Pointer.TagId.Value);

			AssertEquals("2", result.Amendment[1].ChangeReasonCode.Value);
			AssertEquals(null, result.Amendment[1].StatementDescription);
			AssertEquals(null, result.Amendment[1].AdjustmentDescription);
			AssertEquals(312m, result.Amendment[1].Pointer.SequenceNumeric);
			AssertEquals(null, result.Amendment[1].Pointer.TagId);

			AssertEquals("99999999", result.Submitter[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Submitter[0].SchemeAgencyId);
			AssertEquals("9999999999", result.Submitter[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Ktx, result.Submitter[1].SchemeAgencyId);
			localExportHeaderMock.VerifyAll();
			localExportAmendMock.VerifyAll();
			localExportLinerMock1.VerifyAll();
			localExportLinerMock2.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 02, 09)]
		public void Testwithdraw()
		{
			#region AmendedItems
			var localExportLinerMock1 = new Mock<ILocalExportAmendItem>();
			localExportLinerMock1.Setup(m => m.DataItemNo).Returns(ZString.Empty);
			localExportLinerMock1.Setup(m => m.ItemSequenceNumber).Returns(10);
			localExportLinerMock1.Setup(m => m.AmendType).Returns("3");
			localExportLinerMock1.Setup(m => m.BeforeValue).Returns(ZString.Empty);
			localExportLinerMock1.Setup(m => m.AfterValue).Returns(ZString.Empty);

			localExportHeaderMock.Setup(m => m.AmendedItems).Returns(new ILocalExportAmendItem[] { localExportLinerMock1.Object });
			#endregion

			#region IAmendmentDetails
			localExportAmendMock.Setup(m => m.AmendReasonDescription).Returns("사유내용");
			#endregion

			var result = new GOVCBR5DRMessageBuilder(localExportHeaderMock.Object, localExportAmendMock.Object, MessageFunctions.MessageFunctionCode.Cancellation).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.LocalExportOutgoingTestFilePath, "GOVCBR5DR_1.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("01022", result.DeclarationOfficeId.Value);
			AssertEquals("1083699012345", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5DR", result.TypeCode.Value);
			AssertEquals(MessageSubTypeLocalExport.Cancellation, result.TransactionNatureCode.Value);
			AssertEquals("사유내용", result.Reason.Value);
			AssertEquals("1", result.ReasonCode.Value);
			AssertEquals("00000000000000", result.AdditionalDocument.Id.Value);

			AssertEquals("3", result.Amendment[0].ChangeReasonCode.Value);
			AssertEquals(null, result.Amendment[0].StatementDescription);
			AssertEquals(null, result.Amendment[0].AdjustmentDescription);
			AssertEquals(10m, result.Amendment[0].Pointer.SequenceNumeric);
			AssertEquals(null, result.Amendment[0].Pointer.TagId);

			AssertEquals("99999999", result.Submitter[0].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Item380, result.Submitter[0].SchemeAgencyId);
			AssertEquals("9999999999", result.Submitter[1].Value);
			AssertEquals(AgencyIdentificationCodeContentType.Ktx, result.Submitter[1].SchemeAgencyId);
			localExportHeaderMock.VerifyAll();
			localExportAmendMock.VerifyAll();
			localExportLinerMock1.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 02, 09)]
		public void TestwithoutData()
		{
			localExportAmendMock.Setup(m => m.AmendReasonDescription).Returns(ZString.Empty);
			localExportHeaderMock.Setup(m => m.Supplier).Returns(new Mock<IOrganization>().Object);
			var result = new GOVCBR5DRMessageBuilder(localExportHeaderMock.Object, localExportAmendMock.Object, MessageFunctions.MessageFunctionCode.Cancellation).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.LocalExportOutgoingTestFilePath, "GOVCBR5DR_2.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("01022", result.DeclarationOfficeId.Value);
			AssertEquals("1083699012345", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5DR", result.TypeCode.Value);
			AssertEquals(MessageSubTypeLocalExport.Cancellation, result.TransactionNatureCode.Value);
			AssertEquals("2", result.TransactionNatureCode.Value);
			AssertEquals(null, result.Reason);
			AssertEquals("1", result.ReasonCode.Value);
			AssertEquals("00000000000000", result.AdditionalDocument.Id.Value);

			AssertEquals(null, result.Amendment);

			AssertEquals(0, result.Submitter.Count);
			localExportHeaderMock.VerifyAll();
			localExportAmendMock.VerifyAll();
		}
	}
}
