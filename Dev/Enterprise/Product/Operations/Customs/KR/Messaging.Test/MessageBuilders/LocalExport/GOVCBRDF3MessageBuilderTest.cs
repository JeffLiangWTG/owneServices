using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBRDF3MessageBuilderTest : TestCaseWithFactory
	{
		Mock<ILocalExportAmendEntryHeader> localExportHeaderMock;

		protected override void SetUp()
		{
			localExportHeaderMock = new Mock<ILocalExportAmendEntryHeader>();
			localExportHeaderMock.Setup(m => m.CustomsReceiptNumber).Returns("03033151234562");
			localExportHeaderMock.Setup(m => m.DeclarationCustomsOfficeAndDivision).Returns("01020");
			localExportHeaderMock.Setup(m => m.LoadingDate).Returns(new ZDateTime("2015-08-24 12:30"));

			var localExportStevedoreMock1 = new Mock<ILocalExportStevedore>();
			localExportStevedoreMock1.Setup(m => m.SequenceNo).Returns(1);
			localExportStevedoreMock1.Setup(m => m.CompanyName).Returns("AA㈜");
			localExportStevedoreMock1.Setup(m => m.FullName).Returns("성명");
			localExportStevedoreMock1.Setup(m => m.PhoneNumber).Returns("0514621354");
			localExportStevedoreMock1.Setup(m => m.MobileNumber).Returns("01011112222");

			var localExportStevedoreMock2 = new Mock<ILocalExportStevedore>();
			localExportStevedoreMock2.Setup(m => m.SequenceNo).Returns(2);
			localExportStevedoreMock2.Setup(m => m.CompanyName).Returns("BB㈜");
			localExportStevedoreMock2.Setup(m => m.FullName).Returns("박보람");
			localExportStevedoreMock2.Setup(m => m.PhoneNumber).Returns("0212345678");
			localExportStevedoreMock2.Setup(m => m.MobileNumber).Returns("01012345678");

			localExportHeaderMock.Setup(m => m.Stevedores).Returns(new ILocalExportStevedore[] { localExportStevedoreMock1.Object, localExportStevedoreMock2.Object });
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 02, 03)]
		public void TestGenerateDeclaration()
		{
			var result = new GOVCBRDF3MessageBuilder(localExportHeaderMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.LocalExportOutgoingTestFilePath, "GOVCBRDF3_0.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("01020", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals("03033151234562", result.Id.Value);
			AssertEquals(ZDate.Today.ToString(DateFormatType.Date), result.IssueDateTime);
			AssertEquals("GOVCBRDF3", result.TypeCode.Value);
			AssertEquals("1", result.Submitter.RoleCode.Value);
			AssertNull(result.Agent);
			AssertEquals(1m, result.BorderTransportMeans[0].PersonOnBoard.SequenceNumeric);
			AssertEquals("성명", result.BorderTransportMeans[0].PersonOnBoard.GivenName.Value);
			AssertEquals("TE", result.BorderTransportMeans[0].PersonOnBoard.Communication[0].TypeId.Value);
			AssertEquals("0514621354", result.BorderTransportMeans[0].PersonOnBoard.Communication[0].Id.Value);
			AssertEquals("CE", result.BorderTransportMeans[0].PersonOnBoard.Communication[1].TypeId.Value);
			AssertEquals("01011112222", result.BorderTransportMeans[0].PersonOnBoard.Communication[1].Id.Value);
			AssertEquals("AA㈜", result.BorderTransportMeans[0].PersonOnBoard.Contact.DepartmentName.Value);
			AssertEquals(2m, result.BorderTransportMeans[1].PersonOnBoard.SequenceNumeric);
			AssertEquals("박보람", result.BorderTransportMeans[1].PersonOnBoard.GivenName.Value);
			AssertEquals("TE", result.BorderTransportMeans[1].PersonOnBoard.Communication[0].TypeId.Value);
			AssertEquals("0212345678", result.BorderTransportMeans[1].PersonOnBoard.Communication[0].Id.Value);
			AssertEquals("CE", result.BorderTransportMeans[1].PersonOnBoard.Communication[1].TypeId.Value);
			AssertEquals("01012345678", result.BorderTransportMeans[1].PersonOnBoard.Communication[1].Id.Value);
			AssertEquals("BB㈜", result.BorderTransportMeans[1].PersonOnBoard.Contact.DepartmentName.Value);
			AssertEquals("201508241230", result.LoadingLocation.LoadingDateTime);
			localExportHeaderMock.VerifyAll();
		}
	}
}
