using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR934MessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImport934Header> importHeaderMock;

		protected override void SetUp()
		{
			base.SetUp();

			importHeaderMock = new Mock<IImport934Header>();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 08, 18)]
		public void TestGenerateDeclarationTypeA()
		{
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("2292620002828M");
			importHeaderMock.Setup(m => m.TotalCustomsValueKRW).Returns(new ZDecimal("6915357"));
			importHeaderMock.Setup(m => m.ProvisionalPricingYN).Returns(ZBool.False);
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("020");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("11");
			importHeaderMock.Setup(m => m.InvoiceNo).Returns("ST2007007");
			importHeaderMock.Setup(m => m.InvoiceDate).Returns(new ZDate("2020-08-06"));
			importHeaderMock.Setup(m => m.EstimatedDateOfFinalPrice).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ContractExpirationDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ProvisionalAdditionRate).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.ProvisionalAdditionalAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ContractNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.ContractDate).Returns(ZDate.Empty);

			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.CompanyName).Returns("(주)엘에스아이엔티");
			payerMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			payerMock.Setup(m => m.BusinessRegNo).Returns("2618125089");
			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var authorMock = new Mock<IValueDeclarationPerson>();
			authorMock.Setup(m => m.DepartmentAndPosition).Returns("관리부");
			authorMock.Setup(m => m.Name).Returns("김국화");
			authorMock.Setup(m => m.TelephoneNumber).Returns("02 543 4040");
			importHeaderMock.Setup(m => m.Author).Returns(authorMock.Object);

			var responsiblePersonMock = new Mock<IValueDeclarationPerson>();
			responsiblePersonMock.Setup(m => m.DepartmentAndPosition).Returns("관리부");
			responsiblePersonMock.Setup(m => m.Name).Returns("김국화");
			responsiblePersonMock.Setup(m => m.TelephoneNumber).Returns("02 543 4040");

			importHeaderMock.Setup(m => m.ResponsiblePerson).Returns(responsiblePersonMock.Object);

			importHeaderMock.Setup(m => m.ValuationMethod).Returns("A");

			var importFormAHeaderMock = new Mock<IImport934FormA>();
			var questions01Mock = MockSetting(PriceQuestionCodeList.Codes._7A, "N", ZString.Empty);
			var questions02Mock = MockSetting(PriceQuestionCodeList.Codes._7B, ZString.Empty, ZString.Empty);
			var questions03Mock = MockSetting(PriceQuestionCodeList.Codes._7C, ZString.Empty, ZString.Empty);
			var questions04Mock = MockSetting(PriceQuestionCodeList.Codes._7D, ZString.Empty, ZString.Empty);
			var questions05Mock = MockSetting(PriceQuestionCodeList.Codes._7E, ZString.Empty, ZString.Empty);
			var questions06Mock = MockSetting(PriceQuestionCodeList.Codes._8A, "N", ZString.Empty);
			var questions07Mock = MockSetting(PriceQuestionCodeList.Codes._8B, "N", ZString.Empty);
			var questions08Mock = MockSetting(PriceQuestionCodeList.Codes._9A, "N", ZString.Empty);
			var questions09Mock = MockSetting(PriceQuestionCodeList.Codes._9B, "N", ZString.Empty);

			importFormAHeaderMock.Setup(x => x.Questions).Returns(new IQuestionAndAnswer[] {
				questions01Mock.Object, questions02Mock.Object, questions03Mock.Object, questions04Mock.Object,
				questions05Mock.Object, questions06Mock.Object, questions07Mock.Object, questions08Mock.Object,
				questions09Mock.Object });

			var method1ValuationDataMock = new Mock<IValuation1_MethodData>();
			method1ValuationDataMock.Setup(m => m.ExchangeRate).Returns(1196.87);
			method1ValuationDataMock.Setup(m => m.BaseAmount).Returns(5623);
			method1ValuationDataMock.Setup(m => m.BaseAmountCurrency).Returns("USD");
			method1ValuationDataMock.Setup(m => m.IndirectPaymentAmount).Returns(ZDecimal.Zero);

			var method1Additions01Mock = new Mock<IChargeAmountKRW>();
			method1Additions01Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.A104);
			method1Additions01Mock.Setup(m => m.Amount).Returns(6730000);

			method1ValuationDataMock.Setup(m => m.Additions).Returns(new IChargeAmountKRW[] { method1Additions01Mock.Object });

			var method1Deductions01Mock = new Mock<IChargeAmountKRW>();
			method1Deductions01Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.A118);
			method1Deductions01Mock.Setup(m => m.Amount).Returns(185357);
			var method1Deductions02Mock = new Mock<IChargeAmountKRW>();
			method1Deductions02Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.A119);
			method1Deductions02Mock.Setup(m => m.Amount).Returns(185357);
			var method1Deductions03Mock = new Mock<IChargeAmountKRW>();
			method1Deductions03Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.A120);
			method1Deductions03Mock.Setup(m => m.Amount).Returns(185357);

			method1ValuationDataMock.Setup(m => m.Deductions).Returns(new IChargeAmountKRW[] { method1Deductions01Mock.Object, method1Deductions02Mock.Object, method1Deductions03Mock.Object });
			importFormAHeaderMock.Setup(m => m.Method1ValuationData).Returns(method1ValuationDataMock.Object);
			importHeaderMock.Setup(m => m.FormAData).Returns(importFormAHeaderMock.Object);

			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR934_TypeIsA.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("02011", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals("2292620002828M", result.Id.Value);
			AssertEquals("20200818", result.IssueDateTime);
			AssertEquals("GOVCBR934", result.TypeCode.Value);
			AssertEquals("A", result.TransactionNatureCode.Value);

			AssertEquals("7A", result.AdditionalInformation[0].StatementCode.Value);
			AssertEquals("N", result.AdditionalInformation[0].StatementTypeCode.Value);
			AssertEquals("8A", result.AdditionalInformation[1].StatementCode.Value);
			AssertEquals("N", result.AdditionalInformation[1].StatementTypeCode.Value);
			AssertEquals("8B", result.AdditionalInformation[2].StatementCode.Value);
			AssertEquals("N", result.AdditionalInformation[2].StatementTypeCode.Value);
			AssertEquals("9A", result.AdditionalInformation[3].StatementCode.Value);
			AssertEquals("N", result.AdditionalInformation[3].StatementTypeCode.Value);
			AssertEquals("9B", result.AdditionalInformation[4].StatementCode.Value);
			AssertEquals("N", result.AdditionalInformation[4].StatementTypeCode.Value);

			AssertEquals(1196.87m, result.CurrencyExchange[0].RateNumeric);
			AssertEquals("A", result.CurrencyExchange[0].RateTypeCode.Value);

			AssertEquals("ST2007007", result.Consignment.AdditionalDocument.Id.Value);
			AssertEquals("20200806", result.Consignment.AdditionalDocument.IssueDateTime);

			AssertEquals(6915357m, result.Consignment.ConsignmentItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);

			AssertEquals("A101", result.DutyTaxFee[0].TypeCode.Value);
			AssertEquals(5623m, result.DutyTaxFee[0].Payment.PaymentAmount.Value);
			AssertEquals("A104", result.DutyTaxFee[1].TypeCode.Value);
			AssertEquals(6730000m, result.DutyTaxFee[1].Payment.PaymentAmount.Value);
			AssertEquals("A117", result.DutyTaxFee[2].TypeCode.Value);
			AssertEquals(6730000m, result.DutyTaxFee[2].Payment.PaymentAmount.Value);
			AssertEquals("A118", result.DutyTaxFee[3].TypeCode.Value);
			AssertEquals(185357m, result.DutyTaxFee[3].Payment.PaymentAmount.Value);
			AssertEquals("A119", result.DutyTaxFee[4].TypeCode.Value);
			AssertEquals(185357m, result.DutyTaxFee[4].Payment.PaymentAmount.Value);
			AssertEquals("A120", result.DutyTaxFee[5].TypeCode.Value);
			AssertEquals(185357m, result.DutyTaxFee[5].Payment.PaymentAmount.Value);
			AssertEquals("A122", result.DutyTaxFee[6].TypeCode.Value);
			AssertEquals(556071m, result.DutyTaxFee[6].Payment.PaymentAmount.Value);

			AssertEquals("김국화", result.Submitter.Name.Value);
			AssertEquals("관리부", result.Submitter.Contact.JobTitle.Value);
			AssertEquals("관리부", result.Submitter.Contact.DepartmentName.Value);
			AssertEquals("김국화", result.Submitter.Contact.RepresentativeName.Value);
			AssertEquals("02 543 4040", result.Submitter.Contact.Communication.Id.Value);
			AssertEquals("02 543 4040", result.Submitter.Communication.Id.Value);
			AssertEquals("2618125089", result.Payer.Id.Value);
			AssertEquals("(주)엘에스아이엔티", result.Payer.Name.Value);
			AssertEquals("04", result.Payer.RoleCode.Value);

			importHeaderMock.VerifyAll();
			payerMock.VerifyAll();
			authorMock.VerifyAll();
			responsiblePersonMock.VerifyAll();
			importFormAHeaderMock.VerifyAll();
			method1ValuationDataMock.VerifyAll();
			method1Additions01Mock.VerifyAll();
			method1Deductions01Mock.VerifyAll();
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
		[TestDate(2020, 12, 29)]
		public void TestGenerateDeclarationTypeB_23()
		{
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("8001720002836M");
			importHeaderMock.Setup(m => m.TotalCustomsValueKRW).Returns(new ZDecimal("21207200"));
			importHeaderMock.Setup(m => m.ProvisionalPricingYN).Returns(ZBool.False);
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("11");
			importHeaderMock.Setup(m => m.InvoiceNo).Returns("2109389752");
			importHeaderMock.Setup(m => m.InvoiceDate).Returns(new ZDate("2020-12-28"));
			importHeaderMock.Setup(m => m.EstimatedDateOfFinalPrice).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ContractExpirationDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ProvisionalAdditionRate).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.ProvisionalAdditionalAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ContractNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.ContractDate).Returns(ZDate.Empty);

			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.CompanyName).Returns("노드슨코리아(주)");
			payerMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			payerMock.Setup(m => m.BusinessRegNo).Returns("1268110513");
			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var authorMock = new Mock<IValueDeclarationPerson>();
			authorMock.Setup(m => m.DepartmentAndPosition).Returns(".");
			authorMock.Setup(m => m.Name).Returns("김호선 김성");
			authorMock.Setup(m => m.TelephoneNumber).Returns("0314999933");
			importHeaderMock.Setup(m => m.Author).Returns(authorMock.Object);

			var responsiblePersonMock = new Mock<IValueDeclarationPerson>();
			responsiblePersonMock.Setup(m => m.DepartmentAndPosition).Returns(".");
			responsiblePersonMock.Setup(m => m.Name).Returns("김호선 김성");
			responsiblePersonMock.Setup(m => m.TelephoneNumber).Returns("0314999933");

			importHeaderMock.Setup(m => m.ResponsiblePerson).Returns(responsiblePersonMock.Object);

			importHeaderMock.Setup(m => m.ValuationMethod).Returns("B");

			var importFormBHeaderMock = new Mock<IImport934FormB>();
			importFormBHeaderMock.Setup(m => m.ExpectedCustomsValue).Returns(ZDecimal.Zero);
			importFormBHeaderMock.Setup(m => m.ValuationMethod).Returns("20");
			importFormBHeaderMock.Setup(m => m.ValuationSupportingDocument1).Returns(ZString.Empty);
			importFormBHeaderMock.Setup(m => m.ValuationSupportingDocument2).Returns(ZString.Empty);

			var useCodes01Mock = new Mock<IValueDeclarationCode>();
			useCodes01Mock.Setup(m => m.Code).Returns("G");
			useCodes01Mock.Setup(m => m.CodeOtherDescription).Returns("기타무상거래");

			importFormBHeaderMock.Setup(m => m.UseCodes).Returns(new IValueDeclarationCode[] { useCodes01Mock.Object });

			var goodsPricingBasis01Mock = new Mock<IValueDeclarationCode>();
			goodsPricingBasis01Mock.Setup(m => m.Code).Returns("D");
			goodsPricingBasis01Mock.Setup(m => m.CodeOtherDescription).Returns(ZString.Empty);

			importFormBHeaderMock.Setup(m => m.GoodsPricingBasis).Returns(new IValueDeclarationCode[] { goodsPricingBasis01Mock.Object });

			var method23ValuationDataValuation1Mock = new Mock<IValuationMethodData>();
			method23ValuationDataValuation1Mock.Setup(m => m.BaseAmount).Returns(19000);
			method23ValuationDataValuation1Mock.Setup(m => m.BaseAmountCurrency).Returns("USD");
			method23ValuationDataValuation1Mock.Setup(m => m.ExchangeRate).Returns(1114.3);

			var method23Additions01Mock = new Mock<IChargeAmountKRW>();
			method23Additions01Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.B309);
			method23Additions01Mock.Setup(m => m.Amount).Returns(3500);
			var method23Additions02Mock = new Mock<IChargeAmountKRW>();
			method23Additions02Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.B310);
			method23Additions02Mock.Setup(m => m.Amount).Returns(3600);

			method23ValuationDataValuation1Mock.Setup(m => m.Additions).Returns(new IChargeAmountKRW[] { method23Additions01Mock.Object, method23Additions02Mock.Object });

			var method23Deductions01Mock = new Mock<IChargeAmountKRW>();
			method23Deductions01Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.B303);
			method23Deductions01Mock.Setup(m => m.Amount).Returns(3700);

			var method23Deductions02Mock = new Mock<IChargeAmountKRW>();
			method23Deductions02Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.B304);
			method23Deductions02Mock.Setup(m => m.Amount).Returns(3800);

			method23ValuationDataValuation1Mock.Setup(m => m.Deductions).Returns(new IChargeAmountKRW[] { method23Deductions01Mock.Object, method23Deductions02Mock.Object });

			importFormBHeaderMock.Setup(m => m.Method2_3ValuationData).Returns(method23ValuationDataValuation1Mock.Object);

			importHeaderMock.Setup(m => m.FormBData).Returns(importFormBHeaderMock.Object);

			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR934_TypeIsB_23.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("01011", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals("8001720002836M", result.Id.Value);
			AssertEquals("20201229", result.IssueDateTime);
			AssertEquals("GOVCBR934", result.TypeCode.Value);
			AssertEquals("B", result.TransactionNatureCode.Value);

			AssertEquals(1114.3m, result.CurrencyExchange[0].RateNumeric);
			AssertEquals("D", result.CurrencyExchange[0].RateTypeCode.Value);

			AssertEquals("2109389752", result.Consignment.AdditionalDocument.Id.Value);
			AssertEquals("20201228", result.Consignment.AdditionalDocument.IssueDateTime);

			AssertEquals(21207200m, result.Consignment.ConsignmentItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);

			AssertEquals("B301", result.DutyTaxFee[0].TypeCode.Value);
			AssertEquals(19000m, result.DutyTaxFee[0].Payment.PaymentAmount.Value);
			AssertEquals("B303", result.DutyTaxFee[1].TypeCode.Value);
			AssertEquals(3700m, result.DutyTaxFee[1].Payment.PaymentAmount.Value);
			AssertEquals("B304", result.DutyTaxFee[2].TypeCode.Value);
			AssertEquals(3800m, result.DutyTaxFee[2].Payment.PaymentAmount.Value);
			AssertEquals("B308", result.DutyTaxFee[3].TypeCode.Value);
			AssertEquals(7500m, result.DutyTaxFee[3].Payment.PaymentAmount.Value);

			AssertEquals("B309", result.DutyTaxFee[4].TypeCode.Value);
			AssertEquals(3500m, result.DutyTaxFee[4].Payment.PaymentAmount.Value);
			AssertEquals("B310", result.DutyTaxFee[5].TypeCode.Value);
			AssertEquals(3600m, result.DutyTaxFee[5].Payment.PaymentAmount.Value);
			AssertEquals("B314", result.DutyTaxFee[6].TypeCode.Value);
			AssertEquals(7100m, result.DutyTaxFee[6].Payment.PaymentAmount.Value);

			AssertEquals("G", result.GoodsShipment.AdditionalInformation[0].StatementCode.Value);
			AssertEquals("기타무상거래", result.GoodsShipment.AdditionalInformation[0].StatementDescription.Value);
			AssertEquals("20", result.GoodsShipment.Consignment.DutyTaxFee.DutyRegimeCode.Value);
			AssertEquals("D", result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalInformation[0].StatementCode.Value);

			AssertEquals("김호선 김성", result.Submitter.Name.Value);
			AssertEquals(".", result.Submitter.Contact.JobTitle.Value);
			AssertEquals(".", result.Submitter.Contact.DepartmentName.Value);
			AssertEquals("김호선 김성", result.Submitter.Contact.RepresentativeName.Value);
			AssertEquals("0314999933", result.Submitter.Contact.Communication.Id.Value);
			AssertEquals("0314999933", result.Submitter.Communication.Id.Value);
			AssertEquals("1268110513", result.Payer.Id.Value);
			AssertEquals("노드슨코리아(주)", result.Payer.Name.Value);
			AssertEquals("04", result.Payer.RoleCode.Value);

			importHeaderMock.VerifyAll();
			payerMock.VerifyAll();
			authorMock.VerifyAll();
			responsiblePersonMock.VerifyAll();
			importFormBHeaderMock.VerifyAll();
			useCodes01Mock.VerifyAll();
			goodsPricingBasis01Mock.VerifyAll();
			method23ValuationDataValuation1Mock.VerifyAll();
			method23Additions01Mock.VerifyAll();
			method23Additions02Mock.VerifyAll();
			method23Deductions01Mock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 02, 02)]
		public void TestGenerateDeclarationTypeB_4()
		{
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("4177721000003M");
			importHeaderMock.Setup(m => m.TotalCustomsValueKRW).Returns(new ZDecimal("26683215"));
			importHeaderMock.Setup(m => m.ProvisionalPricingYN).Returns(ZBool.False);
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("030");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("83");
			importHeaderMock.Setup(m => m.InvoiceNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.InvoiceDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.EstimatedDateOfFinalPrice).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ContractExpirationDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ProvisionalAdditionRate).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.ProvisionalAdditionalAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ContractNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.ContractDate).Returns(ZDate.Empty);

			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.CompanyName).Returns("(주)제이씨월드");
			payerMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			payerMock.Setup(m => m.BusinessRegNo).Returns("8098701450");
			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var authorMock = new Mock<IValueDeclarationPerson>();
			authorMock.Setup(m => m.DepartmentAndPosition).Returns("무역");
			authorMock.Setup(m => m.Name).Returns("김유정");
			authorMock.Setup(m => m.TelephoneNumber).Returns("028810662");
			importHeaderMock.Setup(m => m.Author).Returns(authorMock.Object);

			var responsiblePersonMock = new Mock<IValueDeclarationPerson>();
			responsiblePersonMock.Setup(m => m.DepartmentAndPosition).Returns("무역");
			responsiblePersonMock.Setup(m => m.Name).Returns("김유정");
			responsiblePersonMock.Setup(m => m.TelephoneNumber).Returns("028810662");

			importHeaderMock.Setup(m => m.ResponsiblePerson).Returns(responsiblePersonMock.Object);

			importHeaderMock.Setup(m => m.ValuationMethod).Returns("B");

			var importFormBHeaderMock = new Mock<IImport934FormB>();
			importFormBHeaderMock.Setup(m => m.ExpectedCustomsValue).Returns(26000000);
			importFormBHeaderMock.Setup(m => m.ValuationMethod).Returns("4B");
			importFormBHeaderMock.Setup(m => m.ValuationSupportingDocument1).Returns(ZString.Empty);
			importFormBHeaderMock.Setup(m => m.ValuationSupportingDocument2).Returns(ZString.Empty);

			var method4ValuationDataMock = new Mock<IValuation4_MethodData>();
			method4ValuationDataMock.Setup(m => m.BaseAmount).Returns(19548);
			method4ValuationDataMock.Setup(m => m.BaseAmountCurrency).Returns("EUR");
			method4ValuationDataMock.Setup(m => m.ExchangeRate).Returns(1365.01);
			method4ValuationDataMock.Setup(m => m.AmountInKRW).Returns(26683215);

			var method4Deductions01Mock = new Mock<IChargeAmountKRW>();
			method4Deductions01Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.B404);
			method4Deductions01Mock.Setup(m => m.Amount).Returns(5000);

			var method4Deductions02Mock = new Mock<IChargeAmountKRW>();
			method4Deductions02Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.B405);
			method4Deductions02Mock.Setup(m => m.Amount).Returns(10000);

			method4ValuationDataMock.Setup(m => m.Deductions).Returns(new IChargeAmountKRW[] { method4Deductions01Mock.Object, method4Deductions02Mock.Object });
			importFormBHeaderMock.Setup(m => m.Method4ValuationData).Returns(method4ValuationDataMock.Object);

			importHeaderMock.Setup(m => m.FormBData).Returns(importFormBHeaderMock.Object);

			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR934_TypeIsB_4.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("03083", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals("4177721000003M", result.Id.Value);
			AssertEquals("20210202", result.IssueDateTime);
			AssertEquals("GOVCBR934", result.TypeCode.Value);
			AssertEquals("B", result.TransactionNatureCode.Value);

			AssertEquals(1365.01m, result.CurrencyExchange[0].RateNumeric);
			AssertEquals("G", result.CurrencyExchange[0].RateTypeCode.Value);

			AssertEquals(26683215m, result.Consignment.ConsignmentItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);

			AssertEquals("B401", result.DutyTaxFee[0].TypeCode.Value);
			AssertEquals(19548m, result.DutyTaxFee[0].Payment.PaymentAmount.Value);
			AssertEquals("B403", result.DutyTaxFee[1].TypeCode.Value);
			AssertEquals(26683215m, result.DutyTaxFee[1].Payment.PaymentAmount.Value);
			AssertEquals("B404", result.DutyTaxFee[2].TypeCode.Value);
			AssertEquals(5000m, result.DutyTaxFee[2].Payment.PaymentAmount.Value);
			AssertEquals("B405", result.DutyTaxFee[3].TypeCode.Value);
			AssertEquals(10000m, result.DutyTaxFee[3].Payment.PaymentAmount.Value);
			AssertEquals("B412", result.DutyTaxFee[4].TypeCode.Value);
			AssertEquals(15000m, result.DutyTaxFee[4].Payment.PaymentAmount.Value);

			AssertEquals("4B", result.GoodsShipment.Consignment.DutyTaxFee.DutyRegimeCode.Value);
			AssertEquals(26000000m, result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);

			AssertEquals("김유정", result.Submitter.Name.Value);
			AssertEquals("무역", result.Submitter.Contact.JobTitle.Value);
			AssertEquals("무역", result.Submitter.Contact.DepartmentName.Value);
			AssertEquals("김유정", result.Submitter.Contact.RepresentativeName.Value);
			AssertEquals("028810662", result.Submitter.Contact.Communication.Id.Value);
			AssertEquals("028810662", result.Submitter.Communication.Id.Value);
			AssertEquals("8098701450", result.Payer.Id.Value);
			AssertEquals("(주)제이씨월드", result.Payer.Name.Value);
			AssertEquals("04", result.Payer.RoleCode.Value);

			importHeaderMock.VerifyAll();
			payerMock.VerifyAll();
			authorMock.VerifyAll();
			responsiblePersonMock.VerifyAll();
			importFormBHeaderMock.VerifyAll();
			method4ValuationDataMock.VerifyAll();
			method4Deductions01Mock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2020, 12, 29)]
		public void TestGenerateDeclarationTypeB_56()
		{
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("1090620000473M");
			importHeaderMock.Setup(m => m.TotalCustomsValueKRW).Returns(new ZDecimal("87952918"));
			importHeaderMock.Setup(m => m.ProvisionalPricingYN).Returns(ZBool.False);
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("012");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			importHeaderMock.Setup(m => m.InvoiceNo).Returns("INV190120-011");
			importHeaderMock.Setup(m => m.InvoiceDate).Returns(new ZDate("2020-12-18"));
			importHeaderMock.Setup(m => m.EstimatedDateOfFinalPrice).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ContractExpirationDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ProvisionalAdditionRate).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.ProvisionalAdditionalAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ContractNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.ContractDate).Returns(ZDate.Empty);

			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.CompanyName).Returns("(주)빈티지코리아");
			payerMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			payerMock.Setup(m => m.BusinessRegNo).Returns("2208167491");
			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var authorMock = new Mock<IValueDeclarationPerson>();
			authorMock.Setup(m => m.DepartmentAndPosition).Returns("과장");
			authorMock.Setup(m => m.Name).Returns("윤용강과장");
			authorMock.Setup(m => m.TelephoneNumber).Returns("070-7882-9257");
			importHeaderMock.Setup(m => m.Author).Returns(authorMock.Object);

			var responsiblePersonMock = new Mock<IValueDeclarationPerson>();
			responsiblePersonMock.Setup(m => m.DepartmentAndPosition).Returns("과장");
			responsiblePersonMock.Setup(m => m.Name).Returns("윤용강과장");
			responsiblePersonMock.Setup(m => m.TelephoneNumber).Returns("070-7882-9257");

			importHeaderMock.Setup(m => m.ResponsiblePerson).Returns(responsiblePersonMock.Object);

			importHeaderMock.Setup(m => m.ValuationMethod).Returns("B");

			var importFormBHeaderMock = new Mock<IImport934FormB>();
			importFormBHeaderMock.Setup(m => m.ExpectedCustomsValue).Returns(ZDecimal.Zero);
			importFormBHeaderMock.Setup(m => m.ValuationMethod).Returns("60");
			importFormBHeaderMock.Setup(m => m.ValuationSupportingDocument1).Returns(ZString.Empty);
			importFormBHeaderMock.Setup(m => m.ValuationSupportingDocument2).Returns(ZString.Empty);

			var method56ValuationDataValuationMock = new Mock<IValuationMethodData>();
			method56ValuationDataValuationMock.Setup(m => m.BaseAmount).Returns(87952918);

			var methodAdditional01Mock = new Mock<IChargeAmountKRW>();
			methodAdditional01Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.B501);
			methodAdditional01Mock.Setup(m => m.Amount).Returns(5000);

			var methodAdditional02Mock = new Mock<IChargeAmountKRW>();
			methodAdditional02Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.B502);
			methodAdditional02Mock.Setup(m => m.Amount).Returns(10000);

			method56ValuationDataValuationMock.Setup(m => m.Additions).Returns(new IChargeAmountKRW[] { methodAdditional01Mock.Object, methodAdditional02Mock.Object });
			importFormBHeaderMock.Setup(m => m.Method5_6ValuationData).Returns(method56ValuationDataValuationMock.Object);

			importHeaderMock.Setup(m => m.FormBData).Returns(importFormBHeaderMock.Object);

			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();

			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR934_TypeIsB_56.xml"));
				using (var resultStream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
				{
					AssertXMLEquals(resultStream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("01210", result.DeclarationOfficeId.Value);
			AssertEquals("9", result.FunctionCode.Value);
			AssertEquals("1090620000473M", result.Id.Value);
			AssertEquals("20201229", result.IssueDateTime);
			AssertEquals("GOVCBR934", result.TypeCode.Value);
			AssertEquals("B", result.TransactionNatureCode.Value);

			AssertEquals("INV190120-011", result.Consignment.AdditionalDocument.Id.Value);
			AssertEquals("20201218", result.Consignment.AdditionalDocument.IssueDateTime);

			AssertEquals(87952918m, result.Consignment.ConsignmentItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);

			AssertEquals("B505", result.DutyTaxFee[0].TypeCode.Value);
			AssertEquals(87952918m, result.DutyTaxFee[0].Payment.PaymentAmount.Value);
			AssertEquals("B501", result.DutyTaxFee[1].TypeCode.Value);
			AssertEquals(5000m, result.DutyTaxFee[1].Payment.PaymentAmount.Value);
			AssertEquals("B502", result.DutyTaxFee[2].TypeCode.Value);
			AssertEquals(10000m, result.DutyTaxFee[2].Payment.PaymentAmount.Value);
			AssertEquals("B504", result.DutyTaxFee[3].TypeCode.Value);
			AssertEquals(15000m, result.DutyTaxFee[3].Payment.PaymentAmount.Value);

			AssertEquals("60", result.GoodsShipment.Consignment.DutyTaxFee.DutyRegimeCode.Value);

			AssertEquals("윤용강과장", result.Submitter.Name.Value);
			AssertEquals("과장", result.Submitter.Contact.JobTitle.Value);
			AssertEquals("과장", result.Submitter.Contact.DepartmentName.Value);
			AssertEquals("윤용강과장", result.Submitter.Contact.RepresentativeName.Value);
			AssertEquals("070-7882-9257", result.Submitter.Contact.Communication.Id.Value);
			AssertEquals("070-7882-9257", result.Submitter.Communication.Id.Value);
			AssertEquals("2208167491", result.Payer.Id.Value);
			AssertEquals("(주)빈티지코리아", result.Payer.Name.Value);
			AssertEquals("04", result.Payer.RoleCode.Value);

			importHeaderMock.VerifyAll();
			payerMock.VerifyAll();
			authorMock.VerifyAll();
			responsiblePersonMock.VerifyAll();
			importFormBHeaderMock.VerifyAll();
			method56ValuationDataValuationMock.VerifyAll();
		}

		public void TestNoExceptionWithInvalidCurrency()
		{
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("2292620002828M");
			importHeaderMock.Setup(m => m.TotalCustomsValueKRW).Returns(new ZDecimal("6915357"));
			importHeaderMock.Setup(m => m.ProvisionalPricingYN).Returns(ZBool.False);
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("020");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("11");
			importHeaderMock.Setup(m => m.InvoiceNo).Returns("ST2007007");
			importHeaderMock.Setup(m => m.InvoiceDate).Returns(new ZDate("2020-08-06"));
			importHeaderMock.Setup(m => m.EstimatedDateOfFinalPrice).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ContractExpirationDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ProvisionalAdditionRate).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.ProvisionalAdditionalAmount).Returns(ZDecimal.Zero);
			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(ZDate.Empty);
			importHeaderMock.Setup(m => m.ContractNo).Returns(ZString.Empty);
			importHeaderMock.Setup(m => m.ContractDate).Returns(ZDate.Empty);

			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.CompanyName).Returns("(주)엘에스아이엔티");
			payerMock.Setup(m => m.IsIndividual).Returns(ZBool.False);
			payerMock.Setup(m => m.BusinessRegNo).Returns("2618125089");
			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var authorMock = new Mock<IValueDeclarationPerson>();
			authorMock.Setup(m => m.DepartmentAndPosition).Returns("관리부");
			authorMock.Setup(m => m.Name).Returns("김국화");
			authorMock.Setup(m => m.TelephoneNumber).Returns("02 543 4040");
			importHeaderMock.Setup(m => m.Author).Returns(authorMock.Object);

			var responsiblePersonMock = new Mock<IValueDeclarationPerson>();
			responsiblePersonMock.Setup(m => m.DepartmentAndPosition).Returns("관리부");
			responsiblePersonMock.Setup(m => m.Name).Returns("김국화");
			responsiblePersonMock.Setup(m => m.TelephoneNumber).Returns("02 543 4040");

			importHeaderMock.Setup(m => m.ResponsiblePerson).Returns(responsiblePersonMock.Object);

			importHeaderMock.Setup(m => m.ValuationMethod).Returns("A");

			var importFormAHeaderMock = new Mock<IImport934FormA>();
			var questions01Mock = MockSetting(PriceQuestionCodeList.Codes._7A, "N", ZString.Empty);
			var questions02Mock = MockSetting(PriceQuestionCodeList.Codes._7B, ZString.Empty, ZString.Empty);
			var questions03Mock = MockSetting(PriceQuestionCodeList.Codes._7C, ZString.Empty, ZString.Empty);
			var questions04Mock = MockSetting(PriceQuestionCodeList.Codes._7D, ZString.Empty, ZString.Empty);
			var questions05Mock = MockSetting(PriceQuestionCodeList.Codes._7E, ZString.Empty, ZString.Empty);
			var questions06Mock = MockSetting(PriceQuestionCodeList.Codes._8A, "N", ZString.Empty);
			var questions07Mock = MockSetting(PriceQuestionCodeList.Codes._8B, "N", ZString.Empty);
			var questions08Mock = MockSetting(PriceQuestionCodeList.Codes._9A, "N", ZString.Empty);
			var questions09Mock = MockSetting(PriceQuestionCodeList.Codes._9B, "N", ZString.Empty);

			importFormAHeaderMock.Setup(x => x.Questions).Returns(new IQuestionAndAnswer[] {
				questions01Mock.Object, questions02Mock.Object, questions03Mock.Object, questions04Mock.Object,
				questions05Mock.Object, questions06Mock.Object, questions07Mock.Object, questions08Mock.Object,
				questions09Mock.Object });

			var method1ValuationDataMock = new Mock<IValuation1_MethodData>();
			method1ValuationDataMock.Setup(m => m.ExchangeRate).Returns(1196.87);
			method1ValuationDataMock.Setup(m => m.BaseAmount).Returns(5623);
			method1ValuationDataMock.Setup(m => m.BaseAmountCurrency).Returns(";;;");
			method1ValuationDataMock.Setup(m => m.IndirectPaymentAmount).Returns(ZDecimal.Zero);

			var method1Additions01Mock = new Mock<IChargeAmountKRW>();
			method1Additions01Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.A104);
			method1Additions01Mock.Setup(m => m.Amount).Returns(6730000);

			method1ValuationDataMock.Setup(m => m.Additions).Returns(new IChargeAmountKRW[] { method1Additions01Mock.Object });

			var method1Deductions01Mock = new Mock<IChargeAmountKRW>();
			method1Deductions01Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.A118);
			method1Deductions01Mock.Setup(m => m.Amount).Returns(185357);
			var method1Deductions02Mock = new Mock<IChargeAmountKRW>();
			method1Deductions02Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.A119);
			method1Deductions02Mock.Setup(m => m.Amount).Returns(185357);
			var method1Deductions03Mock = new Mock<IChargeAmountKRW>();
			method1Deductions03Mock.Setup(m => m.Type).Returns(PriceDutyTaxFeeTypeCode.Codes.A120);
			method1Deductions03Mock.Setup(m => m.Amount).Returns(185357);

			method1ValuationDataMock.Setup(m => m.Deductions).Returns(new IChargeAmountKRW[] { method1Deductions01Mock.Object, method1Deductions02Mock.Object, method1Deductions03Mock.Object });
			importFormAHeaderMock.Setup(m => m.Method1ValuationData).Returns(method1ValuationDataMock.Object);
			importHeaderMock.Setup(m => m.FormAData).Returns(importFormAHeaderMock.Object);

			AssertNoExceptionThrown("No exception should be thrown when invalid currency is entered", () => new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage());

			importHeaderMock.VerifyAll();
			payerMock.VerifyAll();
			authorMock.VerifyAll();
			responsiblePersonMock.VerifyAll();
			importFormAHeaderMock.VerifyAll();
			method1ValuationDataMock.VerifyAll();
			method1Additions01Mock.VerifyAll();
			method1Deductions01Mock.VerifyAll();
			method1Deductions02Mock.VerifyAll();
			method1Deductions03Mock.VerifyAll();
		}

		public void TestEmptyReason()
		{
			importHeaderMock = new Mock<IImport934Header>();
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.Reason);
			AssertNull(result.ReasonCode);

			var provisionalPricingReasonsMock = new Mock<IValueDeclarationCode>();
			provisionalPricingReasonsMock.Setup(m => m.Code).Returns(PriceDeclarationItemCodeList.Codes._119);
			importHeaderMock.Setup(m => m.ProvisionalPricingReasons).Returns(new[] { provisionalPricingReasonsMock.Object });
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.Reason);
			AssertNotNull(result.ReasonCode);
		}

		public void TestEmptyAdditionalDocument()
		{
			importHeaderMock = new Mock<IImport934Header>();
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.AdditionalDocument);

			importHeaderMock.Setup(m => m.ProvisionalPricingYN).Returns(true);
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.AdditionalDocument);
		}

		public void TestEmptyAdditionalInformation()
		{
			importHeaderMock = new Mock<IImport934Header>();
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.AdditionalInformation);

			var question = new Mock<IQuestionAndAnswer>();
			question.Setup(m => m.AnswerCode).Returns("Y");
			var formData = new Mock<IImport934FormA>();
			formData.Setup(m => m.Questions).Returns(new[] { question.Object });
			importHeaderMock.Setup(m => m.FormAData).Returns(formData.Object);
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.AdditionalInformation[0].StatementTypeCode);
			AssertNull(result.AdditionalInformation[0].StatementCode);
			AssertNull(result.AdditionalInformation[0].StatementDescription);
			AssertNull(result.AdditionalInformation[0].Content);
		}

		public void TestEmptyAgent()
		{
			importHeaderMock = new Mock<IImport934Header>();
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.Agent);

			var supplier = new Mock<IOrganization>();
			supplier.Setup(m => m.CompanyName).Returns("company");
			importHeaderMock.Setup(m => m.Supplier).Returns(supplier.Object);

			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull("Agent is not used in 934 anymore.", result.Agent);

			var importer = new Mock<IOrganization>();
			importer.Setup(m => m.CompanyName).Returns("company");
			importHeaderMock.Setup(m => m.Importer).Returns(importer.Object);

			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull("Agent is not used in 934 anymore.", result.Agent);
		}

		public void TestEmptyCurrencyExchange()
		{
			importHeaderMock = new Mock<IImport934Header>();
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.CurrencyExchange);

			var valuation1Data = new Mock<IValuation1_MethodData>();
			valuation1Data.Setup(m => m.ExchangeRate).Returns(0.5m);
			var formData = new Mock<IImport934FormA>();
			formData.Setup(m => m.Method1ValuationData).Returns(valuation1Data.Object);
			importHeaderMock.Setup(m => m.FormAData).Returns(formData.Object);
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.CurrencyExchange);
		}

		public void TestEmptyDutyTaxFee()
		{
			importHeaderMock = new Mock<IImport934Header>();
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.DutyTaxFee);

			var valuation1Data = new Mock<IValuation1_MethodData>();
			valuation1Data.Setup(m => m.BaseAmount).Returns(0m);
			valuation1Data.Setup(m => m.IndirectPaymentAmount).Returns(0m);

			var additions = new Mock<IChargeAmountKRW>();
			additions.Setup(m => m.Amount).Returns(10m);
			additions.Setup(m => m.Type).Returns(ZString.Empty);
			valuation1Data.Setup(m => m.Additions).Returns(new[] { additions.Object });

			var deduction = new Mock<IChargeAmountKRW>();
			deduction.Setup(m => m.Amount).Returns(0m);
			deduction.Setup(m => m.Type).Returns(ZString.Empty);
			valuation1Data.Setup(m => m.Deductions).Returns(new[] { deduction.Object });

			var formData = new Mock<IImport934FormA>();
			formData.Setup(m => m.Method1ValuationData).Returns(valuation1Data.Object);
			importHeaderMock.Setup(m => m.FormAData).Returns(formData.Object);
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();

			AssertNotNull(result.DutyTaxFee[0].Payment);
			AssertNull(result.DutyTaxFee[0].RateTypeCode);
			AssertEquals(3, result.DutyTaxFee.Count);
		}

		public void TestEmptyGoodsShipment()
		{
			importHeaderMock = new Mock<IImport934Header>();
			var formData = new Mock<IImport934FormB>();
			importHeaderMock.Setup(m => m.FormBData).Returns(formData.Object);
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.GoodsShipment);

			var useCode = new Mock<IValueDeclarationCode>();
			useCode.Setup(m => m.Code).Returns("1");
			formData.Setup(m => m.UseCodes).Returns(new[] { useCode.Object });

			importHeaderMock.Setup(m => m.FormBData).Returns(formData.Object);
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.GoodsShipment.AdditionalInformation[0].StatementCode);
			AssertNull(result.GoodsShipment.AdditionalInformation[0].StatementDescription);
			AssertNull(result.GoodsShipment.Consignment);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem);

			useCode.Setup(m => m.Code).Returns(ZString.Empty);
			useCode.Setup(m => m.CodeOtherDescription).Returns("desc");
			formData.Setup(m => m.UseCodes).Returns(new[] { useCode.Object });
			formData.Setup(m => m.ValuationMethod).Returns("20");

			importHeaderMock.Setup(m => m.FormBData).Returns(formData.Object);
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.GoodsShipment.AdditionalInformation[0].StatementCode);
			AssertNotNull(result.GoodsShipment.Consignment.DutyTaxFee.DutyRegimeCode);
			AssertNotNull(result.GoodsShipment.AdditionalInformation[0].StatementDescription);

			formData.Setup(m => m.UseCodes).Returns(Array.Empty<IValueDeclarationCode>());

			var goodsPricingBasis = new Mock<IValueDeclarationCode>();
			goodsPricingBasis.Setup(m => m.Code).Returns("1");
			formData.Setup(m => m.GoodsPricingBasis).Returns(new[] { goodsPricingBasis.Object });
			importHeaderMock.Setup(m => m.FormBData).Returns(formData.Object);
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.GoodsShipment.AdditionalInformation);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity);
			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalInformation[0].StatementCode);

			formData.Setup(m => m.ExpectedCustomsValue).Returns(100m);
			formData.Setup(m => m.GoodsPricingBasis).Returns(Array.Empty<IValueDeclarationCode>());
			importHeaderMock.Setup(m => m.FormBData).Returns(formData.Object);
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();

			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalInformation);
			AssertNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.AdditionalInformation);

			formData.Setup(m => m.ValuationSupportingDocument1).Returns("수입물품 증명자료");
			importHeaderMock.Setup(m => m.FormBData).Returns(formData.Object);
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();

			AssertNotNull(result.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.AdditionalInformation[0].Content);
		}

		public void TestEmptyPreviousDocument()
		{
			importHeaderMock = new Mock<IImport934Header>();
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.PreviousDocument);

			importHeaderMock.Setup(m => m.PurchaseOrderNo).Returns("1");
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertEquals(1, result.PreviousDocument.Count);
			AssertNotNull(result.PreviousDocument[0].Id);
			AssertNull(result.PreviousDocument[0].IssueDateTime);

			importHeaderMock.Setup(m => m.ContractNo).Returns("1");
			importHeaderMock.Setup(m => m.PurchaseOrderDate).Returns(new ZDate("2024-04-05"));
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();

			AssertEquals(2, result.PreviousDocument.Count);
			AssertNotNull(result.PreviousDocument[1].Id);
			AssertNull(result.PreviousDocument[1].IssueDateTime);

			importHeaderMock.Setup(m => m.ContractDate).Returns(new ZDate("2024-04-05"));
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.PreviousDocument[1].IssueDateTime);
		}

		public void TestEmptyConsignment()
		{
			importHeaderMock = new Mock<IImport934Header>();
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.Consignment.AdditionalDocument);
			AssertNull(result.Consignment.AdditionalInformation);
			AssertNull(result.Consignment.ConsignmentItem.Commodity.DutyTaxFee.TaxRateNumeric);
			AssertNull(result.Consignment.ConsignmentItem.Commodity.DutyTaxFee.Payment);

			importHeaderMock.Setup(m => m.InvoiceNo).Returns("ST2007007");
			importHeaderMock.Setup(m => m.InvoiceDate).Returns(new ZDate("2024-04-01"));
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.Consignment.AdditionalDocument);

			importHeaderMock.Setup(m => m.EstimatedDateOfFinalPrice).Returns(new ZDate("2024-04-02"));
			importHeaderMock.Setup(m => m.ContractExpirationDate).Returns(new ZDate("2024-04-03"));
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.Consignment.AdditionalInformation);

			importHeaderMock.Setup(m => m.ProvisionalAdditionRate).Returns(10);
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.Consignment.ConsignmentItem.Commodity.DutyTaxFee.TaxRateNumeric);

			importHeaderMock.Setup(m => m.ProvisionalAdditionalAmount).Returns(20m);
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.Consignment.ConsignmentItem.Commodity.DutyTaxFee.Payment.PaymentAmount);
		}

		public void TestEmptyPreviousDocumentContractNo()
		{
			importHeaderMock = new Mock<IImport934Header>();
			importHeaderMock.Setup(m => m.ContractDate).Returns(new ZDate("2024-04-05"));
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.PreviousDocument[0].Id);

			importHeaderMock.Setup(m => m.ContractNo).Returns("1");
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNotNull(result.PreviousDocument[0].Id);
		}

		public void TestEmptyPreviousDocumentContractDate()
		{
			importHeaderMock = new Mock<IImport934Header>();
			importHeaderMock.Setup(m => m.ContractNo).Returns("1");
			importHeaderMock.Setup(m => m.ContractDate).Returns(ZDate.Invalid);
			var result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertNull(result.PreviousDocument[0].IssueDateTime);

			importHeaderMock.Setup(m => m.ContractDate).Returns(new ZDate("2024-04-05"));
			result = new GOVCBR934MessageBuilder(importHeaderMock.Object, MessageFunctions.MessageFunctionCode.Original).GenerateMessage();
			AssertEquals("20240405", result.PreviousDocument[0].IssueDateTime);
		}
	}
}
