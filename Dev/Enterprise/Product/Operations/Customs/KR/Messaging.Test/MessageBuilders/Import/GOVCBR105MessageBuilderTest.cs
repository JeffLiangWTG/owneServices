using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR105MessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImportFTAAmendmentHeader> import105Mock;
		Mock<IAmendmentDetails> import105DetailMock;

		protected override void SetUp()
		{
			import105Mock = new Mock<IImportFTAAmendmentHeader>();
			import105Mock.Setup(m => m.EntryReleaseDate).Returns(new ZDate("2008-06-01"));
			import105Mock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			import105Mock.Setup(m => m.DeclarationCustomsDivision).Returns("30");
			import105Mock.Setup(m => m.ImportDeclarationNumber).Returns("000000000000000");
			import105Mock.Setup(m => m.LawCode).Returns("1");
			import105Mock.Setup(m => m.StatementNumber5WN).Returns("040112050000001");

			var submitterMock = new Mock<IOrganization>();
			submitterMock.Setup(m => m.CompanyName).Returns("㈜관세청");
			submitterMock.Setup(m => m.RepresentativeName).Returns("홍길동");
			submitterMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			submitterMock.Setup(m => m.CountryCode).Returns("KR");

			import105Mock.Setup(m => m.Declarant).Returns(submitterMock.Object);

			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.IsIndividual).Returns(false);
			importerMock.Setup(m => m.CompanyName).Returns("모나리자㈜");
			importerMock.Setup(m => m.RepresentativeName).Returns("홍나리");
			importerMock.Setup(m => m.AddressLine1).Returns("강원도 춘천시 봉의산길 25");
			importerMock.Setup(m => m.AddressLine2).Returns("상세주소");
			importerMock.Setup(m => m.Postcode).Returns("302111");
			importerMock.Setup(m => m.RoadNameCode).Returns("421104454294");
			importerMock.Setup(m => m.BuildingNumber).Returns("020120");
			importerMock.Setup(m => m.CountryCode).Returns("020120");
			importerMock.Setup(m => m.PhoneNumber).Returns("010-1111-2222");
			importerMock.Setup(m => m.Email).Returns("id@domain.com");
			importerMock.Setup(m => m.MobileNumber).Returns("020120");
			importerMock.Setup(m => m.FaxNumber).Returns("000-000-0000");
			importerMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			importerMock.Setup(m => m.CountryCode).Returns("KR");
			importerMock.Setup(m => m.KoreanRegNoForResident).Returns("주민등록번호");
			importerMock.Setup(m => m.PassportNo).Returns("여권번호");
			importerMock.Setup(m => m.KoreanRegNoForForeigner).Returns("외국인등록번호");
			importerMock.Setup(m => m.BusinessRegNo).Returns("사업자등록번호");
			importerMock.Setup(m => m.UnipassIDForOrganization).Returns("통관고유부호");

			import105Mock.Setup(m => m.Importer).Returns(importerMock.Object);

			import105DetailMock = new Mock<IAmendmentDetails>();
			import105DetailMock.Setup(m => m.AmendmentType).Returns("UXX");
			import105DetailMock.Setup(m => m.AmendReasonDescription).Returns("신청사유");
			import105DetailMock.Setup(m => m.AmendmentVersionNo).Returns(123);

			var amendmentItemMock1 = new Mock<IImportFTAAmendmentItem>();
			amendmentItemMock1.Setup(m => m.SequenceNo).Returns(123);
			amendmentItemMock1.Setup(m => m.EntryLineNo).Returns(1);
			amendmentItemMock1.Setup(m => m.AmendType).Returns("99D");
			amendmentItemMock1.Setup(m => m.DataItemID).Returns("1A");
			amendmentItemMock1.Setup(m => m.BeforeDescription).Returns("홍나리");
			amendmentItemMock1.Setup(m => m.AfterDescription).Returns("개나리");

			var amendmentItemMock2 = new Mock<IImportFTAAmendmentItem>();
			amendmentItemMock2.Setup(m => m.SequenceNo).Returns(1234);
			amendmentItemMock2.Setup(m => m.EntryLineNo).Returns(12);
			amendmentItemMock2.Setup(m => m.AmendType).Returns("99D");
			amendmentItemMock2.Setup(m => m.DataItemID).Returns("2A");
			amendmentItemMock2.Setup(m => m.BeforeDescription).Returns("1홍나리");
			amendmentItemMock2.Setup(m => m.AfterDescription).Returns("1개나리");

			import105Mock.Setup(m => m.Items).Returns(new IImportFTAAmendmentItem[] { amendmentItemMock1.Object, amendmentItemMock2.Object });
		}
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 02, 02)]
		public void TestGenerateDeclaration()
		{
			var result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR105_Result.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("01030", result.DeclarationOfficeId.Value);
			AssertEquals("20080601", result.AuthenticationDateTime);

			AssertEquals("1", result.AdditionalDocument.TypeCode.Value);
			AssertEquals("040112050000001", result.AdditionalDocument.Id.Value);
			AssertEquals("UXX", result.AdditionalInformation.StatementCode.Value);
			AssertEquals("신청사유", result.Reason.Value);
			AssertEquals("123", result.VersionId.Value);

			AssertEquals(1M, result.Consignment[0].SequenceNumeric);
			AssertEquals(1234M, result.Consignment[1].AdditionalDocument.SequenceNumeric);
			AssertEquals("99D", result.Consignment[0].AdditionalInformation.StatementCode.Value);
			AssertEquals("1홍나리", result.Consignment[1].Amendment.StatementDescription.Value);
			AssertEquals("개나리", result.Consignment[0].Amendment.AdjustmentDescription.Value);
			AssertEquals("2A", result.Consignment[1].Amendment.Pointer.TagId.Value);

			AssertEquals("사업자등록번호", result.Importer.Id[0].Value);
			AssertEquals("통관고유부호", result.Importer.Id[1].Value);
			AssertEquals("모나리자㈜", result.Importer.Name.Value);
			AssertEquals("04", result.Importer.RoleCode.Value);
			AssertEquals("421104454294", result.Importer.Address.CountrySubDivisionId.Value);
			AssertEquals("상세주소", result.Importer.Address.Line.Value);
			AssertEquals("302111", result.Importer.Address.PostcodeId.Value);
			AssertEquals("020120", result.Importer.Address.BuildingNumber.Value);
			AssertEquals("강원도 춘천시 봉의산길 25", result.Importer.Address.Description.Value);
			AssertEquals("홍나리", result.Importer.Contact.Name.Value);
			AssertEquals("010-1111-2222", result.Importer.Communication[0].Id.Value);
			AssertEquals("000-000-0000", result.Importer.Communication[1].Id.Value);
			AssertEquals("id@domain.com", result.Importer.Communication[2].Id.Value);
		}

		public void TestEmptyDeclaration()
		{
			import105Mock = new Mock<IImportFTAAmendmentHeader>();
			var submitter = new Mock<IOrganization>();
			import105Mock.Setup(m => m.Declarant).Returns(submitter.Object);
			import105DetailMock = new Mock<IAmendmentDetails>();
			var result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();
			AssertNotNull(result.DeclarationOfficeId);
			AssertNotNull(result.FunctionCode);
			AssertNotNull(result.Id);
			AssertNotNull(result.IssueDateTime);
			AssertNotNull(result.TypeCode);
			AssertNotNull(result.VersionId);
			AssertNotNull(result.Reason);
			AssertNotNull(result.AdditionalInformation);
			AssertNotNull(result.Consignment);
			AssertNotNull(result.Importer);
			AssertNotNull(result.Submitter);
		}

		public void TestEmptyAuthenticationDateTime()
		{
			import105Mock = new Mock<IImportFTAAmendmentHeader>();
			var submitter = new Mock<IOrganization>();
			import105Mock.Setup(m => m.Declarant).Returns(submitter.Object);
			import105DetailMock = new Mock<IAmendmentDetails>();
			var result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();
			AssertNull(result.AuthenticationDateTime);

			import105Mock.Setup(m => m.EntryReleaseDate).Returns(ZDate.Invalid);
			result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();
			AssertNull(result.AuthenticationDateTime);

			import105Mock.Setup(m => m.EntryReleaseDate).Returns(new ZDate("2021-12-31"));
			result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();
			AssertNotNull(result.AuthenticationDateTime);
		}

		public void TestEmptyConsignment()
		{
			import105Mock = new Mock<IImportFTAAmendmentHeader>();
			var submitter = new Mock<IOrganization>();
			import105Mock.Setup(m => m.Declarant).Returns(submitter.Object);
			var item = new Mock<IImportFTAAmendmentItem>();
			import105Mock.Setup(m => m.Items).Returns(new[] { item.Object });
			import105DetailMock = new Mock<IAmendmentDetails>();
			var result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();

			AssertNotNull(result.Consignment[0].AdditionalDocument.SequenceNumeric);
			AssertNull(result.Consignment[0].SequenceNumeric);
			AssertNull(result.Consignment[0].AdditionalInformation);
			AssertNull(result.Consignment[0].Amendment);

			item.Setup(m => m.BeforeDescription).Returns("홍나리");
			import105Mock.Setup(m => m.Items).Returns(new[] { item.Object });
			result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();

			AssertNotNull(result.Consignment[0].Amendment.StatementDescription);
			AssertNull(result.Consignment[0].Amendment.AdjustmentDescription);
			AssertNull(result.Consignment[0].Amendment.Pointer);

			item.Setup(m => m.BeforeDescription).Returns(ZString.Empty);
			item.Setup(m => m.AfterDescription).Returns("개나리");
			import105Mock.Setup(m => m.Items).Returns(new[] { item.Object });

			result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();
			AssertNull(result.Consignment[0].Amendment.StatementDescription);
			AssertNotNull(result.Consignment[0].Amendment.AdjustmentDescription);
		}

		public void TestEmptyImporter()
		{
			import105Mock = new Mock<IImportFTAAmendmentHeader>();
			var submitter = new Mock<IOrganization>();
			import105Mock.Setup(m => m.Declarant).Returns(submitter.Object);
			import105DetailMock = new Mock<IAmendmentDetails>();

			var importer = new Mock<IOrganization>();
			import105Mock.Setup(m => m.Importer).Returns(importer.Object);
			var result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();
			AssertNotNull(result.Importer.Id);
			AssertNotNull(result.Importer.Name);
			AssertNotNull(result.Importer.RoleCode);
			AssertNotNull(result.Importer.Address.PostcodeId);
			AssertNotNull(result.Importer.Address.Description);
			AssertNotNull(result.Importer.Contact.Name);
			AssertNotNull(result.Importer.Communication);
			AssertEquals(3, result.Importer.Communication.Count);

			AssertNull(result.Importer.Address.BuildingNumber);
			AssertNull(result.Importer.Address.Line);
			AssertNull(result.Importer.Address.CountrySubDivisionId);
		}

		public void TestEmptyAdditionalDocument()
		{
			import105Mock = new Mock<IImportFTAAmendmentHeader>();
			var submitter = new Mock<IOrganization>();
			import105Mock.Setup(m => m.Declarant).Returns(submitter.Object);
			import105DetailMock = new Mock<IAmendmentDetails>();

			var result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();

			AssertNull(result.AdditionalDocument);

			import105Mock.Setup(m => m.LawCode).Returns("4");
			result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();

			AssertNotNull(result.AdditionalDocument.TypeCode);
			AssertNull(result.AdditionalDocument.Id);

			import105Mock.Setup(m => m.LawCode).Returns(ZString.Empty);
			import105Mock.Setup(m => m.StatementNumber5WN).Returns("040112050000001");
			result = new GOVCBR105MessageBuilder(import105Mock.Object, import105DetailMock.Object).GenerateMessage();

			AssertNull(result.AdditionalDocument.TypeCode);
			AssertNotNull(result.AdditionalDocument.Id);
		}
	}
}
