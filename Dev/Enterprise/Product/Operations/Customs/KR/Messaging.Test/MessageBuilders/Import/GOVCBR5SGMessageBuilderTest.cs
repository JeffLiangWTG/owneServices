using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5SGMessageBuilderTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2014, 01, 01)]
		public void TestGenerateDeclaration()
		{
			var importHeaderMock = new Mock<IImport5SGHeader>();
			importHeaderMock.Setup(m => m.ApplicationNumber).Returns("5SG123452015X000001");
			importHeaderMock.Setup(m => m.SequenceNo).Returns(1);
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importHeaderMock.Setup(m => m.UnipassDeclarantID).Returns("12345");

			var entry1Mock = new Mock<IImport5SGEntry>();
			entry1Mock.Setup(m => m.ImportDeclarationNumber).Returns("000000000000000");
			entry1Mock.Setup(m => m.ExtensionDate).Returns(new ZDate("2014-01-02"));
			entry1Mock.Setup(m => m.ApplicationReason).Returns("연장신청사유");

			var entry2Mock = new Mock<IImport5SGEntry>();
			entry2Mock.Setup(m => m.ImportDeclarationNumber).Returns("000000000000001");
			entry2Mock.Setup(m => m.ExtensionDate).Returns(new ZDate("2014-01-03"));
			entry2Mock.Setup(m => m.ApplicationReason).Returns("연장신청사유1");

			importHeaderMock.Setup(m => m.Entries).Returns(new IImport5SGEntry[] { entry1Mock.Object, entry2Mock.Object });

			var result = new GOVCBR5SGMessageBuilder(importHeaderMock.Object).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5SG.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("010", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals("5SG123452015X000001", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("GOVCBR5SG", result.TypeCode.Value);
			AssertEquals("1", result.VersionId.Value);
			AssertEquals("12345", result.Submitter.Id.Value);

			AssertEquals(2, result.GoodsShipment.Count);

			AssertEquals("000000000000000", result.GoodsShipment[0].AdditionalDocument.Id.Value);
			AssertEquals("연장신청사유", result.GoodsShipment[0].AdditionalInformation.Content.Value);
			AssertEquals("20140102", result.GoodsShipment[0].AdditionalInformation.LimitDateTime);

			AssertEquals("000000000000001", result.GoodsShipment[1].AdditionalDocument.Id.Value);
			AssertEquals("연장신청사유1", result.GoodsShipment[1].AdditionalInformation.Content.Value);
			AssertEquals("20140103", result.GoodsShipment[1].AdditionalInformation.LimitDateTime);

			importHeaderMock.VerifyAll();
			entry1Mock.VerifyAll();
			entry2Mock.VerifyAll();
		}

		public void TestEmptyGoodsShipmentLimitDateTime()
		{
			var importHeaderMock = new Mock<IImport5SGHeader>();
			var entry1Mock = new Mock<IImport5SGEntry>();
			entry1Mock.Setup(m => m.ExtensionDate).Returns(ZDate.Invalid);
			importHeaderMock.Setup(m => m.Entries).Returns(new IImport5SGEntry[] { entry1Mock.Object });
			var result = new GOVCBR5SGMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(ZString.Empty, result.GoodsShipment[0].AdditionalInformation.LimitDateTime);

			entry1Mock.Setup(m => m.ExtensionDate).Returns(new ZDate("2012-01-01"));
			importHeaderMock.Setup(m => m.Entries).Returns(new IImport5SGEntry[] { entry1Mock.Object });
			result = new GOVCBR5SGMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("20120101", result.GoodsShipment[0].AdditionalInformation.LimitDateTime);
		}
	}
}
