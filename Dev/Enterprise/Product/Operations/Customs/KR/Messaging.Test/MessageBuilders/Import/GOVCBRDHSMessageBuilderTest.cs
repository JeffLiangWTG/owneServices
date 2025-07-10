using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBRDHSMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImportFTAAmendmentHeader> importHeaderMock;
		Mock<IDHSAmendmentDetails> detailsHeaderMock;

		protected override void SetUp()
		{
			importHeaderMock = new Mock<IImportFTAAmendmentHeader>();
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("000000000000000");
			importHeaderMock.Setup(m => m.LawCode).Returns("1");
			importHeaderMock.Setup(m => m.StatementNumber5WN).Returns("040112050000001");

			importHeaderMock.Setup(m => m.EntryReleaseDate).Returns(new ZDate("2008-06-01"));
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("30");

			var submitterMock = new Mock<IOrganization>();
			submitterMock.Setup(m => m.CompanyName).Returns("㈜관세청");
			submitterMock.Setup(m => m.RepresentativeName).Returns("홍길동");
			importHeaderMock.Setup(m => m.Declarant).Returns(submitterMock.Object);

			detailsHeaderMock = new Mock<IDHSAmendmentDetails>();
			detailsHeaderMock.Setup(m => m.AmendmentType).Returns("UXI");
			detailsHeaderMock.Setup(m => m.AmendReasonDescription).Returns("신청사유");
			detailsHeaderMock.Setup(m => m.AmendmentVersionNo).Returns(123);
			detailsHeaderMock.Setup(m => m.AmendmentTypeForInvoiceLine).Returns("CXX");
		}
		Mock<IOrganization> ImporterMockSetValue(Mock<IOrganization> importerMock, string typeValue)
		{
			importerMock.Setup(m => m.CompanyName).Returns("모나리자㈜");
			importerMock.Setup(m => m.RepresentativeName).Returns("홍나리");
			importerMock.Setup(m => m.AddressLine1).Returns("강원도 춘천시 봉의산길 25");
			importerMock.Setup(m => m.Postcode).Returns("302111");
			importerMock.Setup(m => m.PhoneNumber).Returns("010-1111-2222");
			importerMock.Setup(m => m.Email).Returns("id@domain.com");
			importerMock.Setup(m => m.FaxNumber).Returns("000-000-0000");
			importerMock.Setup(m => m.IsIndividual).Returns(ZBool.False);

			importerMock.Setup(m => m.AddressLine2).Returns(typeValue == "NotEmpty" ? "상세주소" : "");
			importerMock.Setup(m => m.RoadNameCode).Returns(typeValue == "NotEmpty" ? "421104454294" : "");
			importerMock.Setup(m => m.BuildingNumber).Returns(typeValue == "NotEmpty" ? "020120" : "");
			importerMock.Setup(m => m.BusinessRegNo).Returns("사업자등록번호");
			importerMock.Setup(m => m.UnipassIDForOrganization).Returns("통관고유부호");

			return importerMock;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 02, 02)]
		public void TestGenerateDeclaration()
		{
			var importerMock = new Mock<IOrganization>();
			var importer = ImporterMockSetValue(importerMock, "NotEmpty");
			importHeaderMock.Setup(m => m.Importer).Returns(importer.Object);

			var amendmentItemMock1 = new Mock<IImportFTAAmendmentItem>();
			amendmentItemMock1.Setup(m => m.SequenceNo).Returns(123);
			amendmentItemMock1.Setup(m => m.EntryLineNo).Returns(001);
			amendmentItemMock1.Setup(m => m.AmendType).Returns("99D");
			amendmentItemMock1.Setup(m => m.DataItemID).Returns("1A");
			amendmentItemMock1.Setup(m => m.BeforeDescription).Returns("홍나리");
			amendmentItemMock1.Setup(m => m.InvoiceLineNo).Returns(01);
			amendmentItemMock1.Setup(m => m.AfterDescription).Returns("개나리");

			var amendmentItemMock2 = new Mock<IImportFTAAmendmentItem>();
			amendmentItemMock2.Setup(m => m.SequenceNo).Returns(1234);
			amendmentItemMock2.Setup(m => m.EntryLineNo).Returns(002);
			amendmentItemMock2.Setup(m => m.AmendType).Returns("99D");
			amendmentItemMock2.Setup(m => m.DataItemID).Returns("2A");
			amendmentItemMock2.Setup(m => m.BeforeDescription).Returns("1홍나리");
			amendmentItemMock2.Setup(m => m.InvoiceLineNo).Returns(02);
			amendmentItemMock2.Setup(m => m.AfterDescription).Returns("1개나리");
			importHeaderMock.Setup(m => m.Items).Returns(new IImportFTAAmendmentItem[] { amendmentItemMock1.Object, amendmentItemMock2.Object });

			var result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBRDHS_Result.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}
			AssertEquals("01030", result.DeclarationOfficeId.Value);
			AssertEquals("20080601", result.AuthenticationDateTime);
			AssertEquals("1", result.AdditionalDocument.TypeCode.Value);
			AssertEquals("040112050000001", result.AdditionalDocument.Id.Value);
			AssertEquals("UXI", result.AdditionalInformation.StatementCode.Value);
			AssertEquals("CXX", result.Amendment.ChangeReasonCode.Value);
			AssertEquals("신청사유", result.Reason.Value);
			AssertEquals("123", result.VersionId.Value);
			AssertEquals(123M, result.Consignment[0].SequenceNumeric);
			AssertEquals("99D", result.Consignment[0].AdditionalInformation.StatementCode.Value);
			AssertEquals("1홍나리", result.Consignment[1].Amendment.StatementDescription.Value);
			AssertEquals("개나리", result.Consignment[0].Amendment.AdjustmentDescription.Value);
			AssertEquals("1", result.Consignment[0].ConsignmentItem.Commodity.IdentityQualifierCode.Value);
			AssertEquals("2", result.Consignment[1].ConsignmentItem.Commodity.SequenceId.Value);
			AssertEquals("2A", result.Consignment[1].Amendment.Pointer.TagId.Value);
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

			importHeaderMock.VerifyAll();
			detailsHeaderMock.VerifyAll();
			importerMock.VerifyAll();
			amendmentItemMock1.VerifyAll();
			amendmentItemMock2.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 02, 02)]
		public void TestWithEmpty()
		{
			detailsHeaderMock.Setup(m => m.AmendmentType).Returns("");
			detailsHeaderMock.Setup(m => m.AmendmentTypeForInvoiceLine).Returns("");

			importHeaderMock.Setup(m => m.LawCode).Returns("");
			importHeaderMock.Setup(m => m.StatementNumber5WN).Returns("");
			importHeaderMock.Setup(m => m.EntryReleaseDate).Returns(new ZDate(""));

			var importerMock = new Mock<IOrganization>();
			var importer = ImporterMockSetValue(importerMock, "Empty");
			importHeaderMock.Setup(m => m.Importer).Returns(importer.Object);

			var amendmentItemMock1 = new Mock<IImportFTAAmendmentItem>();
			amendmentItemMock1.Setup(m => m.SequenceNo).Returns(123);
			amendmentItemMock1.Setup(m => m.EntryLineNo).Returns(0);
			amendmentItemMock1.Setup(m => m.AmendType).Returns("");
			amendmentItemMock1.Setup(m => m.DataItemID).Returns("");
			amendmentItemMock1.Setup(m => m.BeforeDescription).Returns("");
			amendmentItemMock1.Setup(m => m.InvoiceLineNo).Returns(0);
			amendmentItemMock1.Setup(m => m.AfterDescription).Returns("");

			var amendmentItemMock2 = new Mock<IImportFTAAmendmentItem>();
			amendmentItemMock2.Setup(m => m.SequenceNo).Returns(1234);
			amendmentItemMock2.Setup(m => m.EntryLineNo).Returns(0);
			amendmentItemMock2.Setup(m => m.AmendType).Returns("");
			amendmentItemMock2.Setup(m => m.DataItemID).Returns("");
			amendmentItemMock2.Setup(m => m.BeforeDescription).Returns("");
			amendmentItemMock2.Setup(m => m.InvoiceLineNo).Returns(0);
			amendmentItemMock2.Setup(m => m.AfterDescription).Returns("");

			importHeaderMock.Setup(m => m.Items).Returns(new IImportFTAAmendmentItem[] { amendmentItemMock1.Object, amendmentItemMock2.Object });

			var result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBRDHS_Result_Empty.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}
			AssertEquals("01030", result.DeclarationOfficeId.Value);
			AssertEquals("신청사유", result.Reason.Value);
			AssertEquals("123", result.VersionId.Value);
			AssertEquals(123M, result.Consignment[0].SequenceNumeric);
			AssertEquals("모나리자㈜", result.Importer.Name.Value);
			AssertEquals("04", result.Importer.RoleCode.Value);
			AssertEquals("302111", result.Importer.Address.PostcodeId.Value);
			AssertEquals("강원도 춘천시 봉의산길 25", result.Importer.Address.Description.Value);
			AssertEquals("홍나리", result.Importer.Contact.Name.Value);
			AssertEquals("010-1111-2222", result.Importer.Communication[0].Id.Value);
			AssertEquals("000-000-0000", result.Importer.Communication[1].Id.Value);
			AssertEquals("id@domain.com", result.Importer.Communication[2].Id.Value);

			importHeaderMock.VerifyAll();
			detailsHeaderMock.VerifyAll();
			importerMock.VerifyAll();
			amendmentItemMock1.VerifyAll();
			amendmentItemMock2.VerifyAll();
		}

		public void TestEmptyDeclaration()
		{
			importHeaderMock = new Mock<IImportFTAAmendmentHeader>();
			detailsHeaderMock = new Mock<IDHSAmendmentDetails>();
			var result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();
			AssertNull(result.AuthenticationDateTime);
			AssertNull(result.AdditionalDocument);
			AssertNull(result.AdditionalInformation);
			AssertNull(result.Amendment);
		}

		public void TestEmptyConsignment()
		{
			importHeaderMock = new Mock<IImportFTAAmendmentHeader>();
			detailsHeaderMock = new Mock<IDHSAmendmentDetails>();
			var items = new Mock<IImportFTAAmendmentItem>();
			items.Setup(m => m.AmendType).Returns("99D");
			items.Setup(m => m.BeforeDescription).Returns("홍나리");
			items.Setup(m => m.AfterDescription).Returns("개나리");
			items.Setup(m => m.DataItemID).Returns("1A");
			items.Setup(m => m.InvoiceLineNo).Returns(1);
			items.Setup(m => m.EntryLineNo).Returns(1);

			importHeaderMock.Setup(m => m.Items).Returns(new[] { items.Object });
			var result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();

			AssertEquals(1, result.Consignment.Count);
			AssertNotNull(result.Consignment[0].AdditionalInformation.StatementCode);

			AssertNotNull(result.Consignment[0].Amendment.StatementDescription);
			AssertNotNull(result.Consignment[0].Amendment.AdjustmentDescription);
			AssertNotNull(result.Consignment[0].Amendment.Pointer.TagId);

			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.IdentityQualifierCode);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.SequenceId);

			items.Setup(m => m.AmendType).Returns(ZString.Empty);
			items.Setup(m => m.BeforeDescription).Returns(ZString.Empty);
			items.Setup(m => m.InvoiceLineNo).Returns(0);

			importHeaderMock.Setup(m => m.Items).Returns(new[] { items.Object });
			result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();

			AssertNull(result.Consignment[0].AdditionalInformation);

			AssertNull(result.Consignment[0].Amendment.StatementDescription);
			AssertNotNull(result.Consignment[0].Amendment.AdjustmentDescription);
			AssertNotNull(result.Consignment[0].Amendment.Pointer.TagId);

			AssertNull(result.Consignment[0].ConsignmentItem.Commodity.IdentityQualifierCode);
			AssertNotNull(result.Consignment[0].ConsignmentItem.Commodity.SequenceId);

			items.Setup(m => m.AfterDescription).Returns(ZString.Empty);
			items.Setup(m => m.EntryLineNo).Returns(0);

			importHeaderMock.Setup(m => m.Items).Returns(new[] { items.Object });
			result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();

			AssertNull(result.Consignment[0].Amendment.StatementDescription);
			AssertNull(result.Consignment[0].Amendment.AdjustmentDescription);
			AssertNotNull(result.Consignment[0].Amendment.Pointer.TagId);

			AssertNull(result.Consignment[0].ConsignmentItem);

			items.Setup(m => m.DataItemID).Returns(ZString.Empty);

			importHeaderMock.Setup(m => m.Items).Returns(new[] { items.Object });
			result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();

			AssertNull(result.Consignment[0].Amendment);
		}

		public void TestEmptyImporterAddress()
		{
			importHeaderMock = new Mock<IImportFTAAmendmentHeader>();
			detailsHeaderMock = new Mock<IDHSAmendmentDetails>();
			var importer = new Mock<IOrganization>();
			importer.Setup(m => m.RoadNameCode).Returns("101010");
			importer.Setup(m => m.AddressLine2).Returns("상세주소");
			importer.Setup(m => m.BuildingNumber).Returns("11011");

			importHeaderMock.Setup(m => m.Importer).Returns(importer.Object);
			var result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.Importer.Address.CountrySubDivisionId);
			AssertNotNull(result.Importer.Address.Line);
			AssertNotNull(result.Importer.Address.BuildingNumber);

			importer.Setup(m => m.RoadNameCode).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.Importer).Returns(importer.Object);
			result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();

			AssertNull(result.Importer.Address.CountrySubDivisionId);
			AssertNotNull(result.Importer.Address.Line);
			AssertNotNull(result.Importer.Address.BuildingNumber);

			importer.Setup(m => m.AddressLine2).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.Importer).Returns(importer.Object);
			result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();

			AssertNull(result.Importer.Address.CountrySubDivisionId);
			AssertNull(result.Importer.Address.Line);
			AssertNotNull(result.Importer.Address.BuildingNumber);

			importer.Setup(m => m.BuildingNumber).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.Importer).Returns(importer.Object);
			result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();

			AssertNull(result.Importer.Address.CountrySubDivisionId);
			AssertNull(result.Importer.Address.Line);
			AssertNull(result.Importer.Address.BuildingNumber);
		}

		public void TestEmptyEntryReleaseDate()
		{
			importHeaderMock.Setup(m => m.EntryReleaseDate).Returns(new ZDate(""));
			var result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();
			AssertNull(result.AuthenticationDateTime);

			importHeaderMock.Setup(m => m.EntryReleaseDate).Returns(ZDate.Invalid);
			result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();
			AssertNull(result.AuthenticationDateTime);

			importHeaderMock.Setup(m => m.EntryReleaseDate).Returns(ZDate.Today);
			result = new GOVCBRDHSMessageBuilder(importHeaderMock.Object, detailsHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.AuthenticationDateTime);
		}
	}
}
