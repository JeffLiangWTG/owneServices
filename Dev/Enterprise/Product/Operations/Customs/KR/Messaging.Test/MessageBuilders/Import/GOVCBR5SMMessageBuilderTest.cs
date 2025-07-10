using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5SMMessageBuilderTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 05, 24)]
		public void TestGenerateDeclarationTypeA()
		{
			Mock<IImport5SMHeader> importHeaderMock;
			importHeaderMock = new Mock<IImport5SMHeader>();

			importHeaderMock.Setup(m => m.ValueDeclarationTemplateNumber).Returns("88888211002U");
			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ValuationMethod).Returns("A");
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("020");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("09");

			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.RepresentativeName).Returns("고영희");
			supplierMock.Setup(m => m.CompanyName).Returns("GUANGZHOU YUANJUN IMP & EXP CO LTD");
			supplierMock.Setup(m => m.AddressLine1).Returns("강원도 춘천시 봉의산길 25");
			supplierMock.Setup(m => m.AddressLine2).Returns("상세주소");
			supplierMock.Setup(m => m.CountryCode).Returns("KR");
			importHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.RepresentativeName).Returns("홍길동");
			importerMock.Setup(m => m.CompanyName).Returns("AMY TEST");
			importerMock.Setup(m => m.AddressLine1).Returns("서울특별시 영등포구 국제금융로 10");
			importerMock.Setup(m => m.AddressLine2).Returns("(여의도동, 서울 국제금융 센터)");
			importHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);

			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.CompanyName).Returns("(주)쎄븐일레븐");
			payerMock.Setup(m => m.IsIndividual).Returns(ZBool.False);

			payerMock.Setup(m => m.BusinessRegNo).Returns("2068160584");
			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var responsiblePersonMock = new Mock<IValueDeclarationPerson>();
			responsiblePersonMock.Setup(m => m.DepartmentAndPosition).Returns("사장");
			responsiblePersonMock.Setup(m => m.Name).Returns("박공순");
			responsiblePersonMock.Setup(m => m.TelephoneNumber).Returns("024534916");
			importHeaderMock.Setup(m => m.ResponsiblePerson).Returns(responsiblePersonMock.Object);

			var authorMock = new Mock<IValueDeclarationPerson>();
			authorMock.Setup(m => m.DepartmentAndPosition).Returns("사장");
			authorMock.Setup(m => m.Name).Returns("박공순");
			authorMock.Setup(m => m.TelephoneNumber).Returns("024534916");
			importHeaderMock.Setup(m => m.Author).Returns(authorMock.Object);

			var questions01Mock = MockSetting(PriceQuestionCodeList.Codes._5A, "N", ZString.Empty);
			var questions02Mock = MockSetting(PriceQuestionCodeList.Codes._5B, ZString.Empty, ZString.Empty);
			var questions03Mock = MockSetting(PriceQuestionCodeList.Codes._5C, ZString.Empty, ZString.Empty);
			var questions04Mock = MockSetting(PriceQuestionCodeList.Codes._5D, ZString.Empty, ZString.Empty);
			var questions05Mock = MockSetting(PriceQuestionCodeList.Codes._5E, ZString.Empty, ZString.Empty);
			var questions06Mock = MockSetting(PriceQuestionCodeList.Codes._6A, "N", ZString.Empty);
			var questions07Mock = MockSetting(PriceQuestionCodeList.Codes._6B, "N", ZString.Empty);
			var questions08Mock = MockSetting(PriceQuestionCodeList.Codes._7A, "N", ZString.Empty);
			var questions09Mock = MockSetting(PriceQuestionCodeList.Codes._7B, "N", ZString.Empty);
			var questions10Mock = MockSetting(PriceQuestionCodeList.Codes._8A, "N", ZString.Empty);
			var questions11Mock = MockSetting(PriceQuestionCodeList.Codes._8B, "N", ZString.Empty);
			var questions12Mock = MockSetting(PriceQuestionCodeList.Codes._8C, "N", ZString.Empty);
			var questions13Mock = MockSetting(PriceQuestionCodeList.Codes._8D, "N", ZString.Empty);
			var questions14Mock = MockSetting(PriceQuestionCodeList.Codes._9A, "N", ZString.Empty);
			var questions15Mock = MockSetting(PriceQuestionCodeList.Codes._9B, "N", ZString.Empty);
			var questions16Mock = MockSetting(PriceQuestionCodeList.Codes._10A, "N", ZString.Empty);
			var questions17Mock = MockSetting(PriceQuestionCodeList.Codes._10B, "N", ZString.Empty);
			var questions18Mock = MockSetting(PriceQuestionCodeList.Codes._10C, "N", ZString.Empty);
			var questions19Mock = MockSetting(PriceQuestionCodeList.Codes._10D, "N", ZString.Empty);
			var questions20Mock = MockSetting(PriceQuestionCodeList.Codes._11A, "N", ZString.Empty);
			var questions21Mock = MockSetting(PriceQuestionCodeList.Codes._11B, "N", ZString.Empty);
			var questions22Mock = MockSetting(PriceQuestionCodeList.Codes._11C, "N", ZString.Empty);
			var questions23Mock = MockSetting(PriceQuestionCodeList.Codes._11D, "N", ZString.Empty);

			var importFormCHeaderMock = new Mock<IImport5SMFormC>();
			importFormCHeaderMock.Setup(x => x.Questions).Returns(new IQuestionAndAnswer[] {
				questions01Mock.Object, questions02Mock.Object, questions03Mock.Object, questions04Mock.Object,
				questions05Mock.Object, questions06Mock.Object, questions07Mock.Object, questions08Mock.Object,
				questions09Mock.Object, questions10Mock.Object, questions11Mock.Object, questions12Mock.Object,
				questions13Mock.Object, questions14Mock.Object, questions15Mock.Object, questions16Mock.Object,
				questions17Mock.Object, questions18Mock.Object, questions19Mock.Object, questions20Mock.Object,
				questions21Mock.Object, questions22Mock.Object, questions23Mock.Object });
			importHeaderMock.Setup(m => m.FormCData).Returns(importFormCHeaderMock.Object);

			var entryLinesHeaderMock1 = new Mock<IImport5SMLine>();
			entryLinesHeaderMock1.Setup(m => m.EntryLineNo).Returns(1);
			entryLinesHeaderMock1.Setup(m => m.HSCode).Returns("9506910000");
			entryLinesHeaderMock1.Setup(m => m.HSDescription).Returns("FOLDING BENCH");
			entryLinesHeaderMock1.Setup(m => m.InvoiceDescription).Returns(ZString.Empty);
			entryLinesHeaderMock1.Setup(m => m.BrandName).Returns(ZString.Empty);
			entryLinesHeaderMock1.Setup(m => m.ItemDescription).Returns(ZString.Empty);
			entryLinesHeaderMock1.Setup(m => m.Ingredient).Returns(ZString.Empty);

			var entryLinesHeaderMock2 = new Mock<IImport5SMLine>();
			entryLinesHeaderMock2.Setup(m => m.EntryLineNo).Returns(2);
			entryLinesHeaderMock2.Setup(m => m.HSCode).Returns("2106909099");
			entryLinesHeaderMock2.Setup(m => m.HSDescription).Returns("FOOD PREPARATION");
			entryLinesHeaderMock2.Setup(m => m.InvoiceDescription).Returns(ZString.Empty);
			entryLinesHeaderMock2.Setup(m => m.BrandName).Returns(ZString.Empty);
			entryLinesHeaderMock2.Setup(m => m.ItemDescription).Returns(ZString.Empty);
			entryLinesHeaderMock2.Setup(m => m.Ingredient).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImport5SMLine[] { entryLinesHeaderMock1.Object, entryLinesHeaderMock2.Object });

			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5SM_TypeIsA.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("02009", result.DeclarationOfficeId.Value);
			AssertEquals("88888211002U", result.Id.Value);
			AssertEquals("20210524", result.IssueDateTime);
			AssertEquals("GOVCBR5SM", result.TypeCode.Value);
			AssertEquals("A", result.TransactionNatureCode.Value);

			AssertEquals("AMY TEST, 홍길동, 서울특별시 영등포구 국제금융로 10, (여의도동, 서울 국제금융 센터)", result.GoodsShipment.Buyer.Name.Value);
			AssertEquals(1m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].SequenceNumeric);
			AssertEquals("FOLDING BENCH", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description.Value);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name);
			AssertEquals("9506910000", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Classification.Id.Value);
			AssertEquals(2m, result.GoodsShipment.GovernmentAgencyGoodsItem[1].SequenceNumeric);
			AssertEquals("FOOD PREPARATION", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Description.Value);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Name);
			AssertEquals("2106909099", result.GoodsShipment.GovernmentAgencyGoodsItem[1].Commodity.Classification.Id.Value);

			AssertEquals("GUANGZHOU YUANJUN IMP & EXP CO LTD, 고영희, 강원도 춘천시 봉의산길 25, 상세주소, KR", result.GoodsShipment.Seller.Name.Value); 
			AssertEquals("박공순", result.Submitter.Name.Value);
			AssertEquals("사장", result.Submitter.Contact.JobTitle.Value);
			AssertEquals("사장", result.Submitter.Contact.DepartmentName.Value);
			AssertEquals("박공순", result.Submitter.Contact.RepresentativeName.Value);
			AssertEquals("024534916", result.Submitter.Contact.Communication.Id.Value);
			AssertEquals("024534916", result.Submitter.Communication.Id.Value);

			AssertEquals("2068160584", result.Payer.Id.Value);
			AssertEquals("(주)쎄븐일레븐", result.Payer.Name.Value);
			AssertEquals("04", result.Payer.RoleCode.Value);

			AssertEquals("5A", result.Requirement[0].ItemCode.Value);
			AssertEquals("N", result.Requirement[0].Details.StatusCode.Value);
			AssertEquals("5B", result.Requirement[1].ItemCode.Value);
			AssertNull("", result.Requirement[1].Details);
			AssertEquals("5C", result.Requirement[2].ItemCode.Value);
			AssertNull("", result.Requirement[2].Details);
			AssertEquals("5D", result.Requirement[3].ItemCode.Value);
			AssertNull("", result.Requirement[3].Details);
			AssertEquals("5E", result.Requirement[4].ItemCode.Value);
			AssertNull(result.Requirement[4].Details);

			importHeaderMock.VerifyAll();
			supplierMock.VerifyAll();
			importerMock.VerifyAll();
			payerMock.VerifyAll();
			responsiblePersonMock.VerifyAll();
			authorMock.VerifyAll();
			importFormCHeaderMock.VerifyAll();
			entryLinesHeaderMock1.VerifyAll();
			entryLinesHeaderMock2.VerifyAll();
		}

		public void TestBuyer()
		{
			Mock<IImport5SMHeader> importHeaderMock;
			importHeaderMock = new Mock<IImport5SMHeader>();

			var importerMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);

			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("", result.GoodsShipment.Buyer.Name.Value);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();

			importerMock.Setup(m => m.RepresentativeName).Returns("홍길동");
			importHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("홍길동", result.GoodsShipment.Buyer.Name.Value);

			importerMock.Setup(m => m.AddressLine1).Returns("서울특별시 영등포구 국제금융로 10");
			importerMock.Setup(m => m.AddressLine2).Returns("(여의도동, 서울 국제금융 센터)");
			importHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("홍길동, 서울특별시 영등포구 국제금융로 10, (여의도동, 서울 국제금융 센터)", result.GoodsShipment.Buyer.Name.Value);

			importerMock.Setup(m => m.CompanyName).Returns("AMY TEST");
			importHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("AMY TEST, 홍길동, 서울특별시 영등포구 국제금융로 10, (여의도동, 서울 국제금융 센터)", result.GoodsShipment.Buyer.Name.Value);
		}

		public void TestSeller()
		{
			Mock<IImport5SMHeader> importHeaderMock;
			importHeaderMock = new Mock<IImport5SMHeader>();

			var supplierMock = new Mock<IOrganization>();
			importHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("", result.GoodsShipment.Seller.Name.Value);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();

			supplierMock.Setup(m => m.RepresentativeName).Returns("홍길동");
			importHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("홍길동", result.GoodsShipment.Seller.Name.Value);

			supplierMock.Setup(m => m.AddressLine1).Returns("서울특별시 영등포구 국제금융로 10");
			supplierMock.Setup(m => m.AddressLine2).Returns("(여의도동, 서울 국제금융 센터)");
			importHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("홍길동, 서울특별시 영등포구 국제금융로 10, (여의도동, 서울 국제금융 센터)", result.GoodsShipment.Seller.Name.Value);

			supplierMock.Setup(m => m.CompanyName).Returns("AMY TEST");
			importHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("AMY TEST, 홍길동, 서울특별시 영등포구 국제금융로 10, (여의도동, 서울 국제금융 센터)", result.GoodsShipment.Seller.Name.Value);

			supplierMock.Setup(m => m.CountryCode).Returns("KR");
			importHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("AMY TEST, 홍길동, 서울특별시 영등포구 국제금융로 10, (여의도동, 서울 국제금융 센터), KR", result.GoodsShipment.Seller.Name.Value);
		}

		Mock<IQuestionAndAnswer> MockSetting(ZString questionCode, ZString answerCode, ZString answerOtherDescription)
		{
			var questionsMock = new Mock<IQuestionAndAnswer>();
			questionsMock.Setup(m => m.QuestionCode).Returns(questionCode);
			questionsMock.Setup(m => m.AnswerCode).Returns(answerCode);
			questionsMock.Setup(m => m.AnswerOtherDescription).Returns(answerOtherDescription);

			return questionsMock;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 05, 24)]
		public void TestGenerateDeclarationTypeB()
		{
			Mock<IImport5SMHeader> importHeaderMock;
			importHeaderMock = new Mock<IImport5SMHeader>();

			importHeaderMock.Setup(m => m.ValueDeclarationTemplateNumber).Returns("88888211003U");
			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ValuationMethod).Returns("B");
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("020");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("09");

			var supplierMock = new Mock<IOrganization>();
			supplierMock.Setup(m => m.CompanyName).Returns("INGREDION THAILAND CO LTD");
			importHeaderMock.Setup(m => m.Supplier).Returns(supplierMock.Object);

			var importerMock = new Mock<IOrganization>();
			importerMock.Setup(m => m.CompanyName).Returns("AMY TEST");
			importHeaderMock.Setup(m => m.Importer).Returns(importerMock.Object);

			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.CompanyName).Returns("인그리디언코리아(유)");
			payerMock.Setup(m => m.BusinessRegNo).Returns("2028162692");
			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var responsiblePersonMock = new Mock<IValueDeclarationPerson>();
			responsiblePersonMock.Setup(m => m.DepartmentAndPosition).Returns("사원");
			responsiblePersonMock.Setup(m => m.Name).Returns("이진희");
			responsiblePersonMock.Setup(m => m.TelephoneNumber).Returns("0234851421");
			importHeaderMock.Setup(m => m.ResponsiblePerson).Returns(responsiblePersonMock.Object);

			var authorMock = new Mock<IValueDeclarationPerson>();
			authorMock.Setup(m => m.DepartmentAndPosition).Returns("사원");
			authorMock.Setup(m => m.Name).Returns("이진희");
			authorMock.Setup(m => m.TelephoneNumber).Returns("0234851421");
			importHeaderMock.Setup(m => m.Author).Returns(authorMock.Object);

			var importFormDHeaderMock = new Mock<IImport934_5SMFormD>();
			importFormDHeaderMock.Setup(m => m.ValuationMethod).Returns("4B");
			importFormDHeaderMock.Setup(m => m.ValuationSupportingDocument1).Returns("료제조원가 자");
			importFormDHeaderMock.Setup(m => m.ValuationSupportingDocument2).Returns(ZString.Empty);

			var useCodes01Mock = new Mock<IValueDeclarationCode>();
			useCodes01Mock.Setup(m => m.Code).Returns("1");
			useCodes01Mock.Setup(m => m.CodeOtherDescription).Returns("301");

			var useCodes02Mock = new Mock<IValueDeclarationCode>();
			useCodes02Mock.Setup(m => m.Code).Returns("1");
			useCodes02Mock.Setup(m => m.CodeOtherDescription).Returns("302");

			var useCodes03Mock = new Mock<IValueDeclarationCode>();
			useCodes03Mock.Setup(m => m.Code).Returns("1");
			useCodes03Mock.Setup(m => m.CodeOtherDescription).Returns("304");

			importFormDHeaderMock.Setup(m => m.UseCodes).Returns(new IValueDeclarationCode[] { useCodes01Mock.Object, useCodes02Mock.Object, useCodes03Mock.Object });

			var goodsPricingBasis01Mock = new Mock<IValueDeclarationCode>();
			goodsPricingBasis01Mock.Setup(m => m.Code).Returns("2");
			goodsPricingBasis01Mock.Setup(m => m.CodeOtherDescription).Returns("401");

			var goodsPricingBasis02Mock = new Mock<IValueDeclarationCode>();
			goodsPricingBasis02Mock.Setup(m => m.Code).Returns("2");
			goodsPricingBasis02Mock.Setup(m => m.CodeOtherDescription).Returns("402");

			var goodsPricingBasis03Mock = new Mock<IValueDeclarationCode>();
			goodsPricingBasis03Mock.Setup(m => m.Code).Returns("2");
			goodsPricingBasis03Mock.Setup(m => m.CodeOtherDescription).Returns("403");

			var goodsPricingBasis04Mock = new Mock<IValueDeclarationCode>();
			goodsPricingBasis04Mock.Setup(m => m.Code).Returns("2");
			goodsPricingBasis04Mock.Setup(m => m.CodeOtherDescription).Returns("404");

			importFormDHeaderMock.Setup(m => m.GoodsPricingBasis).Returns(new IValueDeclarationCode[] { goodsPricingBasis01Mock.Object, goodsPricingBasis02Mock.Object, goodsPricingBasis03Mock.Object, goodsPricingBasis04Mock.Object });
			importHeaderMock.Setup(m => m.FormDData).Returns(importFormDHeaderMock.Object);

			var entryLinesHeaderMock = new Mock<IImport5SMLine>();
			entryLinesHeaderMock.Setup(m => m.EntryLineNo).Returns(1);
			entryLinesHeaderMock.Setup(m => m.HSCode).Returns("2106909099");
			entryLinesHeaderMock.Setup(m => m.HSDescription).Returns("FOOD PREPARATION");
			entryLinesHeaderMock.Setup(m => m.InvoiceDescription).Returns(ZString.Empty);
			entryLinesHeaderMock.Setup(m => m.BrandName).Returns(ZString.Empty);
			entryLinesHeaderMock.Setup(m => m.ItemDescription).Returns(ZString.Empty);
			entryLinesHeaderMock.Setup(m => m.Ingredient).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImport5SMLine[] { entryLinesHeaderMock.Object });

			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5SM_TypeIsB.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("02009", result.DeclarationOfficeId.Value);
			AssertEquals("88888211003U", result.Id.Value);
			AssertEquals("20210524", result.IssueDateTime);
			AssertEquals("GOVCBR5SM", result.TypeCode.Value);
			AssertEquals("B", result.TransactionNatureCode.Value);
			AssertEquals("1", result.AdditionalInformation[0].StatementCode.Value);
			AssertEquals("료제조원가 자", result.AdditionalInformation[0].StatementDescription.Value);

			AssertEquals("AMY TEST", result.GoodsShipment.Buyer.Name.Value);
			AssertEquals("1", result.GoodsShipment.AdditionalInformation[0].StatementCode.Value);
			AssertEquals("301", result.GoodsShipment.AdditionalInformation[0].StatementTypeCode.Value);
			AssertEquals("1", result.GoodsShipment.AdditionalInformation[1].StatementCode.Value);
			AssertEquals("302", result.GoodsShipment.AdditionalInformation[1].StatementTypeCode.Value);
			AssertEquals("1", result.GoodsShipment.AdditionalInformation[2].StatementCode.Value);
			AssertEquals("304", result.GoodsShipment.AdditionalInformation[2].StatementTypeCode.Value);
			AssertEquals("2", result.GoodsShipment.AdditionalInformation[3].StatementCode.Value);
			AssertEquals("401", result.GoodsShipment.AdditionalInformation[3].StatementTypeCode.Value);
			AssertEquals("2", result.GoodsShipment.AdditionalInformation[4].StatementCode.Value);
			AssertEquals("402", result.GoodsShipment.AdditionalInformation[4].StatementTypeCode.Value);
			AssertEquals("2", result.GoodsShipment.AdditionalInformation[5].StatementCode.Value);
			AssertEquals("403", result.GoodsShipment.AdditionalInformation[5].StatementTypeCode.Value);
			AssertEquals("2", result.GoodsShipment.AdditionalInformation[6].StatementCode.Value);
			AssertEquals("404", result.GoodsShipment.AdditionalInformation[6].StatementTypeCode.Value);

			AssertEquals("4B", result.GoodsShipment.DutyTaxFee.DutyRegimeCode.Value);
			AssertEquals(1m, result.GoodsShipment.GovernmentAgencyGoodsItem[0].SequenceNumeric);
			AssertEquals("FOOD PREPARATION", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description.Value);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name);
			AssertEquals("2106909099", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Classification.Id.Value);
			AssertEquals("INGREDION THAILAND CO LTD", result.GoodsShipment.Seller.Name.Value);
			AssertEquals("이진희", result.Submitter.Name.Value);
			AssertEquals("사원", result.Submitter.Contact.JobTitle.Value);
			AssertEquals("사원", result.Submitter.Contact.DepartmentName.Value);
			AssertEquals("이진희", result.Submitter.Contact.RepresentativeName.Value);
			AssertEquals("0234851421", result.Submitter.Contact.Communication.Id.Value);
			AssertEquals("0234851421", result.Submitter.Communication.Id.Value);

			AssertEquals("2028162692", result.Payer.Id.Value);
			AssertEquals("인그리디언코리아(유)", result.Payer.Name.Value);
			AssertEquals("04", result.Payer.RoleCode.Value);
			importHeaderMock.VerifyAll();
			supplierMock.VerifyAll();
			importerMock.VerifyAll();
			payerMock.VerifyAll();
			responsiblePersonMock.VerifyAll();
			authorMock.VerifyAll();
			importFormDHeaderMock.VerifyAll();
			useCodes01Mock.VerifyAll();
			useCodes02Mock.VerifyAll();
			useCodes03Mock.VerifyAll();
			goodsPricingBasis01Mock.VerifyAll();
			goodsPricingBasis02Mock.VerifyAll();
			goodsPricingBasis03Mock.VerifyAll();
			goodsPricingBasis04Mock.VerifyAll();
			entryLinesHeaderMock.VerifyAll();
		}

		public void TestEmptryElement()
		{
			var importHeaderMock = new Mock<IImport5SMHeader>();
			var entryLinesHeaderMock = new Mock<IImport5SMLine>();
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImport5SMLine[] { entryLinesHeaderMock.Object });
			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();

			AssertNotNull(result.DeclarationOfficeId);
			AssertNotNull(result.Id);
			AssertNotNull(result.IssueDateTime);
			AssertNotNull(result.TypeCode);
			AssertNotNull(result.TransactionNatureCode);

			AssertNull(result.AdditionalInformation);

			AssertNotNull(result.GoodsShipment);
			AssertNotNull(result.GoodsShipment.Buyer.Name);
			AssertNull(result.GoodsShipment.AdditionalInformation);
			AssertNull(result.GoodsShipment.DutyTaxFee);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].SequenceNumeric);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CargoDescription);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Classification.Id);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Constituent);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity);

			AssertNotNull(result.GoodsShipment.Seller);

			AssertNull(result.PreviousDocument);

			AssertNotNull(result.Submitter);
			AssertNotNull(result.Submitter.Name);
			AssertNotNull(result.Submitter.Contact.JobTitle);
			AssertNotNull(result.Submitter.Contact.DepartmentName);
			AssertNotNull(result.Submitter.Contact.RepresentativeName);
			AssertNotNull(result.Submitter.Contact.Communication.Id);
			AssertNotNull(result.Submitter.Communication.Id);

			AssertNotNull(result.Payer);
			AssertNotNull(result.Payer.Name);
			AssertNotNull(result.Payer.Id);
			AssertNotNull(result.Payer.RoleCode);

			AssertNull(result.Requirement);
		}

		public void TestEmptyElement_AdditionalInformation()
		{
			var importHeaderMock = new Mock<IImport5SMHeader>();
			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.AdditionalInformation);

			var importFormDHeaderMock = new Mock<IImport934_5SMFormD>();
			importFormDHeaderMock.Setup(m => m.ValuationSupportingDocument1).Returns("Test Data1");
			importHeaderMock.Setup(m => m.FormDData).Returns(importFormDHeaderMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(1, result.AdditionalInformation.Count);
			AssertEquals("Test Data1", result.AdditionalInformation[0].StatementDescription.Value);
			AssertEquals("1", result.AdditionalInformation[0].StatementCode.Value);

			importFormDHeaderMock = new Mock<IImport934_5SMFormD>();
			importFormDHeaderMock.Setup(m => m.ValuationSupportingDocument2).Returns("Test Data2");
			importHeaderMock.Setup(m => m.FormDData).Returns(importFormDHeaderMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(1, result.AdditionalInformation.Count);
			AssertEquals("Test Data2", result.AdditionalInformation[0].StatementDescription.Value);
			AssertEquals("2", result.AdditionalInformation[0].StatementCode.Value);

			importFormDHeaderMock = new Mock<IImport934_5SMFormD>();
			importFormDHeaderMock.Setup(m => m.ValuationSupportingDocument1).Returns("Test Data1");
			importFormDHeaderMock.Setup(m => m.ValuationSupportingDocument2).Returns("Test Data2");
			importHeaderMock.Setup(m => m.FormDData).Returns(importFormDHeaderMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals(2, result.AdditionalInformation.Count);
			AssertEquals("Test Data1", result.AdditionalInformation[0].StatementDescription.Value);
			AssertEquals("1", result.AdditionalInformation[0].StatementCode.Value);
			AssertEquals("Test Data2", result.AdditionalInformation[1].StatementDescription.Value);
			AssertEquals("2", result.AdditionalInformation[1].StatementCode.Value);
		}

		public void TestEmptyElement_GoodsShipmentGovernmentAgencyGoodsItem()
		{
			var importHeaderMock = new Mock<IImport5SMHeader>();
			var entryLinesHeaderMock = new Mock<IImport5SMLine>();
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImport5SMLine[] { entryLinesHeaderMock.Object });
			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].SequenceNumeric);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Description);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Classification.Id);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CargoDescription);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Constituent);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity);

			entryLinesHeaderMock.Setup(m => m.InvoiceDescription).Returns("Test 1");
			entryLinesHeaderMock.Setup(m => m.BrandName).Returns("Test 2");
			entryLinesHeaderMock.Setup(m => m.Ingredient).Returns("Test 3");
			entryLinesHeaderMock.Setup(m => m.ItemDescription).Returns("Test 4");
			importHeaderMock.Setup(m => m.EntryLines).Returns(new IImport5SMLine[] { entryLinesHeaderMock.Object });
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("Test 1", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.CargoDescription.Value);
			AssertEquals("Test 2", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Name.Value);
			AssertEquals("Test 3", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.Constituent.ElementName.Value);
			AssertEquals("Test 4", result.GoodsShipment.GovernmentAgencyGoodsItem[0].Commodity.DetailedCommodity.Description.Value);
		}

		public void TestEmptyElement_GoodsShipmentDutyTaxFee()
		{
			var importHeaderMock = new Mock<IImport5SMHeader>();
			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.DutyTaxFee);

			var importFormDHeaderMock = new Mock<IImport934_5SMFormD>();
			importFormDHeaderMock.Setup(m => m.ValuationMethod).Returns("20");
			importHeaderMock.Setup(m => m.FormDData).Returns(importFormDHeaderMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("20", result.GoodsShipment.DutyTaxFee.DutyRegimeCode.Value);
		}

		public void TestEmptyElement_GoodsShipmentAdditionalInformation()
		{
			var importHeaderMock = new Mock<IImport5SMHeader>();
			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.AdditionalInformation);

			var valueDeclarationCodeMock = new Mock<IValueDeclarationCode>();
			valueDeclarationCodeMock.Setup(m => m.Code).Returns("1");
			valueDeclarationCodeMock.Setup(m => m.CodeOtherDescription).Returns("301");
			var importFormDHeaderMock = new Mock<IImport934_5SMFormD>();
			importFormDHeaderMock.Setup(m => m.UseCodes).Returns(new IValueDeclarationCode[] { valueDeclarationCodeMock.Object });
			importHeaderMock.Setup(m => m.FormDData).Returns(importFormDHeaderMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("1", result.GoodsShipment.AdditionalInformation[0].StatementCode.Value);
			AssertEquals("301", result.GoodsShipment.AdditionalInformation[0].StatementTypeCode.Value);

			importHeaderMock = new Mock<IImport5SMHeader>();
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.GoodsShipment.AdditionalInformation);

			importFormDHeaderMock = new Mock<IImport934_5SMFormD>();
			importFormDHeaderMock.Setup(m => m.GoodsPricingBasis).Returns(new IValueDeclarationCode[] { valueDeclarationCodeMock.Object });
			importHeaderMock.Setup(m => m.FormDData).Returns(importFormDHeaderMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertEquals("1", result.GoodsShipment.AdditionalInformation[0].StatementCode.Value);
			AssertEquals("301", result.GoodsShipment.AdditionalInformation[0].StatementTypeCode.Value);
		}

		public void TestEmptyElement_PreviousDocument()
		{
			var importHeaderMock = new Mock<IImport5SMHeader>();
			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.PreviousDocument);

			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns("1");
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(ZDate.Invalid);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.PreviousDocument);
			AssertEquals("1", result.PreviousDocument.Id.Value);
			AssertNull(result.PreviousDocument.IssueDateTime);

			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(ZDate.Today);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.PreviousDocument);
			AssertNull(result.PreviousDocument.Id);
			AssertEquals(ZDate.Today.ToString(Constants.DateFormatType.Date), result.PreviousDocument.IssueDateTime);

			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns("1");
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(ZDate.Today);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.PreviousDocument);
			AssertEquals("1", result.PreviousDocument.Id.Value);
			AssertEquals(ZDate.Today.ToString(Constants.DateFormatType.Date), result.PreviousDocument.IssueDateTime);
		}

		public void TestEmptyElement_Requirement()
		{
			var importHeaderMock = new Mock<IImport5SMHeader>();
			var result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.Requirement);

			var questionAndAnswerMock = new Mock<IQuestionAndAnswer>();
			questionAndAnswerMock.Setup(m => m.QuestionCode).Returns("1");

			var importFormCHeaderMock = new Mock<IImport5SMFormC>();
			importFormCHeaderMock.Setup(x => x.Questions).Returns(new IQuestionAndAnswer[] { questionAndAnswerMock.Object });
			importHeaderMock.Setup(m => m.FormCData).Returns(importFormCHeaderMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.Requirement);
			AssertEquals("1", result.Requirement[0].ItemCode.Value);
			AssertNull(result.Requirement[0].Details);

			questionAndAnswerMock = new Mock<IQuestionAndAnswer>();
			questionAndAnswerMock.Setup(m => m.AnswerCode).Returns("1");
			importFormCHeaderMock.Setup(x => x.Questions).Returns(new IQuestionAndAnswer[] { questionAndAnswerMock.Object });
			importHeaderMock.Setup(m => m.FormCData).Returns(importFormCHeaderMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.Requirement);
			AssertNull(result.Requirement[0].ItemCode);
			AssertEquals("1", result.Requirement[0].Details.StatusCode.Value);
			AssertNull(result.Requirement[0].Details.Description);

			questionAndAnswerMock = new Mock<IQuestionAndAnswer>();
			questionAndAnswerMock.Setup(m => m.AnswerOtherDescription).Returns("1");
			importFormCHeaderMock.Setup(x => x.Questions).Returns(new IQuestionAndAnswer[] { questionAndAnswerMock.Object });
			importHeaderMock.Setup(m => m.FormCData).Returns(importFormCHeaderMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNull(result.Requirement);

			questionAndAnswerMock = new Mock<IQuestionAndAnswer>();
			questionAndAnswerMock.Setup(m => m.QuestionCode).Returns("5E");
			questionAndAnswerMock.Setup(m => m.AnswerCode).Returns("2");
			questionAndAnswerMock.Setup(m => m.AnswerOtherDescription).Returns("3");
			importFormCHeaderMock.Setup(x => x.Questions).Returns(new IQuestionAndAnswer[] { questionAndAnswerMock.Object });
			importHeaderMock.Setup(m => m.FormCData).Returns(importFormCHeaderMock.Object);
			result = new GOVCBR5SMMessageBuilder(importHeaderMock.Object).GenerateMessage();
			AssertNotNull(result.Requirement);
			AssertEquals("5E", result.Requirement[0].ItemCode.Value);
			AssertEquals("2", result.Requirement[0].Details.StatusCode.Value);
			AssertEquals("3", result.Requirement[0].Details.Description.Value);
		}
	}
}
