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
	sealed class GOVCBR5ULMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImport5ULHeader> import5ULMock;

		protected override void SetUp()
		{
			import5ULMock = new Mock<IImport5ULHeader>();
			import5ULMock.Setup(m => m.RefundDeclarationNumber).Returns("229262000043U");
			import5ULMock.Setup(m => m.RefundType).Returns("B");
			import5ULMock.Setup(m => m.RefundCauseCode).Returns(ZString.Empty);
			import5ULMock.Setup(m => m.RefundReasonCode).Returns(ZString.Empty);
			import5ULMock.Setup(m => m.Are5FE_5ULToBeSentTogether).Returns(false);
			import5ULMock.Setup(m => m.DeclarationCustomsOffice).Returns("040");
			import5ULMock.Setup(m => m.DeclarationCustomsDivision).Returns("64");
			import5ULMock.Setup(m => m.BankAccountNumber).Returns("13450662247");
			import5ULMock.Setup(m => m.BankCode).Returns("027");
			import5ULMock.Setup(m => m.TotalRefundAmount).Returns(1592350m);
			import5ULMock.Setup(m => m.TaxOfficeCode).Returns(ZString.Empty);

			var entryLine1Mock = new Mock<IImport5ULEntryLine>();
			entryLine1Mock.Setup(m => m.ImportDeclarationNumber).Returns("2292620001198M");
			entryLine1Mock.Setup(m => m.SoABillNumber).Returns("040112007080760");
			entryLine1Mock.Setup(m => m.VATDecisionDate).Returns(new ZDate("2020-04-13"));
			entryLine1Mock.Setup(m => m.VersionNumber5WN).Returns(1);
			entryLine1Mock.Setup(m => m.TotalOtherTaxItemAmount).Returns(205600m);
			entryLine1Mock.Setup(m => m.RefundLineNo).Returns(1);
			entryLine1Mock.Setup(m => m.ImportEntryLineNo).Returns(3);
			entryLine1Mock.Setup(m => m.CancelReasonCode).Returns("A");
			entryLine1Mock.Setup(m => m.ExportDeclarationNumber).Returns("2292620002334X");
			entryLine1Mock.Setup(m => m.ExportEntryLineNo).Returns(1);
			entryLine1Mock.Setup(m => m.DisposalNumber).Returns("A");
			entryLine1Mock.Setup(m => m.DisposalDate).Returns(new ZDate("2014-01-01"));
			entryLine1Mock.Setup(m => m.GoodsLocationDescription).Returns("환급시장치장소");
			entryLine1Mock.Setup(m => m.ResidualSubstanceDescription).Returns("TEST");
			entryLine1Mock.Setup(m => m.DamageSituation).Returns("참고사항");
			entryLine1Mock.Setup(m => m.TotalOtherTaxItemAmount).Returns(205600m);

			var taxItem1Mock = new Mock<IImport5ULTaxItem>();
			taxItem1Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes._5AB);
			taxItem1Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem1Mock.Setup(m => m.Tax).Returns(0m);
			var taxItem2Mock = new Mock<IImport5ULTaxItem>();
			taxItem2Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes.CAP);
			taxItem2Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem2Mock.Setup(m => m.Tax).Returns(0m);
			var taxItem3Mock = new Mock<IImport5ULTaxItem>();
			taxItem3Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes.VAT);
			taxItem3Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem3Mock.Setup(m => m.Tax).Returns(205600m);
			var taxItem4Mock = new Mock<IImport5ULTaxItem>();
			taxItem4Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes.ACT);
			taxItem4Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem4Mock.Setup(m => m.Tax).Returns(0m);
			var taxItem5Mock = new Mock<IImport5ULTaxItem>();
			taxItem5Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes.IND);
			taxItem5Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem5Mock.Setup(m => m.Tax).Returns(0m);
			var taxItem6Mock = new Mock<IImport5ULTaxItem>();
			taxItem6Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes.ENV);
			taxItem6Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem6Mock.Setup(m => m.Tax).Returns(0m);

			entryLine1Mock.Setup(m => m.TaxItems).Returns(new IImport5ULTaxItem[] { taxItem1Mock.Object, taxItem2Mock.Object, taxItem3Mock.Object, taxItem4Mock.Object, taxItem5Mock.Object, taxItem6Mock.Object });

			var otherTaxItem1Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem1Mock.Setup(m => m.TaxItem).Returns("5CQ");
			otherTaxItem1Mock.Setup(m => m.Tax).Returns(2056000m);
			var otherTaxItem2Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem2Mock.Setup(m => m.TaxItem).Returns("5CR");
			otherTaxItem2Mock.Setup(m => m.Tax).Returns(0m);
			var otherTaxItem3Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem3Mock.Setup(m => m.TaxItem).Returns("5AC");
			otherTaxItem3Mock.Setup(m => m.Tax).Returns(0m);
			var otherTaxItem4Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem4Mock.Setup(m => m.TaxItem).Returns("5AY");
			otherTaxItem4Mock.Setup(m => m.Tax).Returns(0m);
			var otherTaxItem5Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem5Mock.Setup(m => m.TaxItem).Returns("5CT");
			otherTaxItem5Mock.Setup(m => m.Tax).Returns(0m);
			var otherTaxItem6Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem6Mock.Setup(m => m.TaxItem).Returns("5CS");
			otherTaxItem6Mock.Setup(m => m.Tax).Returns(0m);

			entryLine1Mock.Setup(m => m.OtherTaxItems).Returns(new IImport5ULTaxItem[] { otherTaxItem1Mock.Object, otherTaxItem2Mock.Object, otherTaxItem3Mock.Object, otherTaxItem4Mock.Object, otherTaxItem5Mock.Object, otherTaxItem6Mock.Object });

			var invoiceLine11Mock = new Mock<IImport5ULInvoiceLine>();
			invoiceLine11Mock.Setup(m => m.InvoiceLineNo).Returns(32);
			invoiceLine11Mock.Setup(m => m.HSDescription).Returns("HANDBAGS");
			invoiceLine11Mock.Setup(m => m.ItemDescription).Returns("M9203UTZQ30 MONTAIGNE M CANVAM928  TU   ITALY");
			invoiceLine11Mock.Setup(m => m.RefundQuantity).Returns(1);
			invoiceLine11Mock.Setup(m => m.InvoiceQuantity).Returns(1);
			invoiceLine11Mock.Setup(m => m.UnitPrice).Returns(1135802m);
			var invoiceLine12Mock = new Mock<IImport5ULInvoiceLine>();
			invoiceLine12Mock.Setup(m => m.InvoiceLineNo).Returns(39);
			invoiceLine12Mock.Setup(m => m.HSDescription).Returns("HANDBAGS");
			invoiceLine12Mock.Setup(m => m.ItemDescription).Returns("M1296ZRIWDIOR BOOK TOTE S CANM928  TU   ITALY");
			invoiceLine12Mock.Setup(m => m.RefundQuantity).Returns(1);
			invoiceLine12Mock.Setup(m => m.InvoiceQuantity).Returns(1);
			invoiceLine12Mock.Setup(m => m.UnitPrice).Returns(884164m);

			entryLine1Mock.Setup(m => m.InvoiceLines).Returns(new IImport5ULInvoiceLine[] { invoiceLine11Mock.Object, invoiceLine12Mock.Object });

			var entryLine2Mock = new Mock<IImport5ULEntryLine>();
			entryLine2Mock.Setup(m => m.ImportDeclarationNumber).Returns("2292620001198M");
			entryLine2Mock.Setup(m => m.SoABillNumber).Returns("040112007080760");
			entryLine2Mock.Setup(m => m.VATDecisionDate).Returns(new ZDate("2020-04-13"));
			entryLine2Mock.Setup(m => m.VersionNumber5WN).Returns(1);
			entryLine2Mock.Setup(m => m.RefundLineNo).Returns(2);
			entryLine2Mock.Setup(m => m.ImportEntryLineNo).Returns(3);
			entryLine2Mock.Setup(m => m.CancelReasonCode).Returns("A");
			entryLine2Mock.Setup(m => m.ExportDeclarationNumber).Returns("2292620002334X");
			entryLine2Mock.Setup(m => m.ExportEntryLineNo).Returns(7);
			entryLine2Mock.Setup(m => m.DisposalNumber).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.DisposalDate).Returns(ZDate.Empty);
			entryLine2Mock.Setup(m => m.GoodsLocationDescription).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.ResidualSubstanceDescription).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.DamageSituation).Returns(ZString.Empty);
			entryLine2Mock.Setup(m => m.TotalOtherTaxItemAmount).Returns(94220m);

			var taxItem2_1Mock = new Mock<IImport5ULTaxItem>();
			taxItem2_1Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes._5AB);
			taxItem2_1Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem2_1Mock.Setup(m => m.Tax).Returns(0m);
			var taxItem2_2Mock = new Mock<IImport5ULTaxItem>();
			taxItem2_2Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes.CAP);
			taxItem2_2Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem2_2Mock.Setup(m => m.Tax).Returns(0m);
			var taxItem2_3Mock = new Mock<IImport5ULTaxItem>();
			taxItem2_3Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes.VAT);
			taxItem2_3Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem2_3Mock.Setup(m => m.Tax).Returns(94220m);
			var taxItem2_4Mock = new Mock<IImport5ULTaxItem>();
			taxItem2_4Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes.ACT);
			taxItem2_4Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem2_4Mock.Setup(m => m.Tax).Returns(0m);
			var taxItem2_5Mock = new Mock<IImport5ULTaxItem>();
			taxItem2_5Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes.IND);
			taxItem2_5Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem2_5Mock.Setup(m => m.Tax).Returns(0m);
			var taxItem2_6Mock = new Mock<IImport5ULTaxItem>();
			taxItem2_6Mock.Setup(m => m.TaxItem).Returns(EntryTaxTypeList.Codes.ENV);
			taxItem2_6Mock.Setup(m => m.PenaltyAmount).Returns(0m);
			taxItem2_6Mock.Setup(m => m.Tax).Returns(0m);

			entryLine2Mock.Setup(m => m.TaxItems).Returns(new IImport5ULTaxItem[] { taxItem2_1Mock.Object, taxItem2_2Mock.Object, taxItem2_3Mock.Object, taxItem2_4Mock.Object, taxItem2_5Mock.Object, taxItem2_6Mock.Object });

			var otherTaxItem2_1Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem2_1Mock.Setup(m => m.TaxItem).Returns("5CQ");
			otherTaxItem2_1Mock.Setup(m => m.Tax).Returns(942200m);
			var otherTaxItem2_2Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem2_2Mock.Setup(m => m.TaxItem).Returns("5CR");
			otherTaxItem2_2Mock.Setup(m => m.Tax).Returns(0m);
			var otherTaxItem2_3Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem2_3Mock.Setup(m => m.TaxItem).Returns("5AC");
			otherTaxItem2_3Mock.Setup(m => m.Tax).Returns(0m);
			var otherTaxItem2_4Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem2_4Mock.Setup(m => m.TaxItem).Returns("5AY");
			otherTaxItem2_4Mock.Setup(m => m.Tax).Returns(0m);
			var otherTaxItem2_5Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem2_5Mock.Setup(m => m.TaxItem).Returns("5CT");
			otherTaxItem2_5Mock.Setup(m => m.Tax).Returns(0m);
			var otherTaxItem2_6Mock = new Mock<IImport5ULTaxItem>();
			otherTaxItem2_6Mock.Setup(m => m.TaxItem).Returns("5CS");
			otherTaxItem2_6Mock.Setup(m => m.Tax).Returns(0m);

			entryLine2Mock.Setup(m => m.OtherTaxItems).Returns(new IImport5ULTaxItem[] { otherTaxItem2_1Mock.Object, otherTaxItem2_2Mock.Object, otherTaxItem2_3Mock.Object, otherTaxItem2_4Mock.Object, otherTaxItem2_5Mock.Object, otherTaxItem2_6Mock.Object });

			var invoiceLine21Mock = new Mock<IImport5ULInvoiceLine>();
			invoiceLine21Mock.Setup(m => m.InvoiceLineNo).Returns(26);
			invoiceLine21Mock.Setup(m => m.HSDescription).Returns("HANDBAGS");
			invoiceLine21Mock.Setup(m => m.ItemDescription).Returns("M1286ZRIWDIOR BOOK TOTE L CANM928  TU   ITALY");
			invoiceLine21Mock.Setup(m => m.RefundQuantity).Returns(1);
			invoiceLine21Mock.Setup(m => m.InvoiceQuantity).Returns(1);
			invoiceLine21Mock.Setup(m => m.UnitPrice).Returns(925712m);
			entryLine2Mock.Setup(m => m.InvoiceLines).Returns(new IImport5ULInvoiceLine[] { invoiceLine21Mock.Object });

			import5ULMock.Setup(m => m.EntryLines).Returns(new IImport5ULEntryLine[] { entryLine1Mock.Object, entryLine2Mock.Object });
		}
		Mock<IOrganization> PayerSetting(bool isIndividual)
		{
			var payerMock = new Mock<IOrganization>();
			payerMock.Setup(m => m.CompanyName).Returns("신고인상호");
			payerMock.Setup(m => m.RepresentativeName).Returns("성명");
			payerMock.Setup(m => m.AddressLine1).Returns("기본주소");
			payerMock.Setup(m => m.AddressLine2).Returns("상세주소");
			payerMock.Setup(m => m.Postcode).Returns("12345");
			payerMock.Setup(m => m.RoadNameCode).Returns("14124312");
			payerMock.Setup(m => m.BuildingNumber).Returns("15");
			payerMock.Setup(m => m.KoreanRegNoForResident).Returns("주민등록번호");
			payerMock.Setup(m => m.IsIndividual).Returns(isIndividual);
			if (!isIndividual)
			{
				payerMock.Setup(m => m.BusinessRegNo).Returns("사업자등록번호");
				payerMock.Setup(m => m.UnipassIDForOrganization).Returns("업체통관고유부호");
			}
			else
			{
				payerMock.Setup(m => m.UnipassIDForIndividual).Returns("개인통관고유부호");
			}
			return payerMock;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateDeclarationWhenPayerIsIndividualIsFalse()
		{
			var payerMock = PayerSetting(false);
			import5ULMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5UL_1.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			AssertEquals("업체통관고유부호", result.Submitter.Id.Value);
			AssertEquals("신고인상호", result.Submitter.Name.Value);
			AssertEquals("01", result.Submitter.RoleCode.Value);
			AssertEquals("14124312", result.Submitter.Address.CountrySubDivisionId.Value);
			AssertEquals("상세주소", result.Submitter.Address.Line.Value);
			AssertEquals("12345", result.Submitter.Address.PostcodeId.Value);
			AssertEquals("15", result.Submitter.Address.BuildingNumber.Value);
			AssertEquals("기본주소", result.Submitter.Address.Description.Value);
			AssertEquals("성명", result.Submitter.Contact.RepresentativeName.Value);
			import5ULMock.VerifyAll();
		}
		public void TestGenerateDeclarationPayerIsIndividualIsTrue()
		{
			var payerMock = PayerSetting(true);
			import5ULMock.Setup(m => m.Payer).Returns(payerMock.Object);

			var result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();
			using (var stream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(stream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();
			}

			AssertEquals(1, result.Agent.Count);
			AssertEquals("주민등록번호", result.Agent[0].Value);
			AssertEquals("개인통관고유부호", result.Submitter.Agent.Id.Value);
			import5ULMock.VerifyAll();
		}

		public void TestEmptyDeclaration()
		{
			import5ULMock = new Mock<IImport5ULHeader>();
			var result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNotNull(result.DeclarationOfficeId);
			AssertNotNull(result.Id);
			AssertNotNull(result.InvoiceAmount);
			AssertNotNull(result.TypeCode);
			AssertNotNull(result.TransactionNatureCode);
			AssertNotNull(result.AdditionalInformation);
			AssertNotNull(result.Agent);
			AssertNotNull(result.BankAccount);
			AssertNotNull(result.GoodsShipment);
			AssertNotNull(result.Submitter);
			AssertNull(result.FunctionCode);
			AssertNull(result.ReasonCode);
		}

		public void TestEmptyAdditionalInformation()
		{
			import5ULMock = new Mock<IImport5ULHeader>();
			var result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();
			AssertNotNull(result.AdditionalInformation.StatementCode);
			AssertNull(result.AdditionalInformation.Pointer);
		}

		public void TestEmptyBankAccount()
		{
			import5ULMock = new Mock<IImport5ULHeader>();
			var result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNotNull(result.BankAccount.Id);
			AssertNotNull(result.BankAccount.ReferenceId);
			AssertNull(result.BankAccount.Name);
		}

		public void TestEmptyGoodsShipment()
		{
			import5ULMock = new Mock<IImport5ULHeader>();
			var entryLine = new Mock<IImport5ULEntryLine>();
			import5ULMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			var result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].AdditionalDocument);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.AdditionalDocument.IssueDateTime);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.Description);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.PreviousDocument);
			AssertNull(result.GoodsShipment[0].Warehouse);
		}

		public void TestEmptyGoodsShipmentAdditionalInformation()
		{
			import5ULMock = new Mock<IImport5ULHeader>();
			var entryLine = new Mock<IImport5ULEntryLine>();
			entryLine.Setup(m => m.DamageSituation).Returns("참고사항");
			import5ULMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			var result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.AdditionalInformation.Content);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.AdditionalInformation.StatementCode);

			entryLine.Setup(m => m.DamageSituation).Returns(ZString.Empty);
			entryLine.Setup(m => m.CancelReasonCode).Returns("A");
			import5ULMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.AdditionalInformation.Content);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.AdditionalInformation.StatementCode);

			entryLine.Setup(m => m.CancelReasonCode).Returns(ZString.Empty);
			import5ULMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.AdditionalInformation);
		}

		public void TestEmptyDetailedCommodity()
		{
			import5ULMock = new Mock<IImport5ULHeader>();
			var entryLine = new Mock<IImport5ULEntryLine>();
			var invoiceLine = new Mock<IImport5ULInvoiceLine>();

			invoiceLine.Setup(m => m.HSDescription).Returns("Handbags");
			invoiceLine.Setup(m => m.InvoiceQuantity).Returns(1);
			invoiceLine.Setup(m => m.ItemDescription).Returns("M9203UTZQ30 MONTAIGNE M CANVAM928  TU   ITALY");
			invoiceLine.Setup(m => m.UnitPrice).Returns(10000000);
			invoiceLine.Setup(m => m.RefundQuantity).Returns(1);

			entryLine.Setup(m => m.InvoiceLines).Returns(new[] { invoiceLine.Object });
			import5ULMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			var result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].CargoDescription);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].CountQuantity);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].Description);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].UnitPriceAmount);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].DetailedCountQuantity);

			invoiceLine.Setup(m => m.HSDescription).Returns(ZString.Empty);
			entryLine.Setup(m => m.InvoiceLines).Returns(new[] { invoiceLine.Object });
			import5ULMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].CargoDescription);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].CountQuantity);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].Description);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].UnitPriceAmount);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].DetailedCountQuantity);

			invoiceLine.Setup(m => m.InvoiceQuantity).Returns(0);
			entryLine.Setup(m => m.InvoiceLines).Returns(new[] { invoiceLine.Object });
			import5ULMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].CargoDescription);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].CountQuantity);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].Description);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].UnitPriceAmount);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].DetailedCountQuantity);

			invoiceLine.Setup(m => m.ItemDescription).Returns(ZString.Empty);
			entryLine.Setup(m => m.InvoiceLines).Returns(new[] { invoiceLine.Object });
			import5ULMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].CargoDescription);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].CountQuantity);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].Description);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].UnitPriceAmount);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].DetailedCountQuantity);

			invoiceLine.Setup(m => m.UnitPrice).Returns(0);
			entryLine.Setup(m => m.InvoiceLines).Returns(new[] { invoiceLine.Object });
			import5ULMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].CargoDescription);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].CountQuantity);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].Description);
			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].UnitPriceAmount);
			AssertNotNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity[0].DetailedCountQuantity);

			invoiceLine.Setup(m => m.RefundQuantity).Returns(0);
			entryLine.Setup(m => m.InvoiceLines).Returns(Array.Empty<IImport5ULInvoiceLine>());
			import5ULMock.Setup(m => m.EntryLines).Returns(new[] { entryLine.Object });
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DetailedCommodity);
		}

		public void TestEmptySubmitter()
		{
			import5ULMock = new Mock<IImport5ULHeader>();
			var payer = new Mock<IOrganization>();
			payer.Setup(m => m.RoadNameCode).Returns("도로명주소");
			payer.Setup(m => m.AddressLine2).Returns("상세주소");
			payer.Setup(m => m.Postcode).Returns("우편번호");
			payer.Setup(m => m.BuildingNumber).Returns("10435");
			import5ULMock.Setup(m => m.Payer).Returns(payer.Object);
			var result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.Submitter.Id);
			AssertNull(result.Submitter.Contact);
			AssertNull(result.Submitter.Agent);

			AssertNotNull(result.Submitter.Address.Description);
			AssertNotNull(result.Submitter.Address.CountrySubDivisionId);
			AssertNotNull(result.Submitter.Address.Line);
			AssertNotNull(result.Submitter.Address.PostcodeId);
			AssertNotNull(result.Submitter.Address.BuildingNumber);

			payer.Setup(m => m.RoadNameCode).Returns(ZString.Empty);
			import5ULMock.Setup(m => m.Payer).Returns(payer.Object);
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.Submitter.Address.CountrySubDivisionId);
			AssertNotNull(result.Submitter.Address.Line);
			AssertNotNull(result.Submitter.Address.PostcodeId);
			AssertNotNull(result.Submitter.Address.BuildingNumber);

			payer.Setup(m => m.AddressLine2).Returns(ZString.Empty);
			import5ULMock.Setup(m => m.Payer).Returns(payer.Object);
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.Submitter.Address.CountrySubDivisionId);
			AssertNull(result.Submitter.Address.Line);
			AssertNotNull(result.Submitter.Address.PostcodeId);
			AssertNotNull(result.Submitter.Address.BuildingNumber);

			payer.Setup(m => m.Postcode).Returns(ZString.Empty);
			import5ULMock.Setup(m => m.Payer).Returns(payer.Object);
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.Submitter.Address.CountrySubDivisionId);
			AssertNull(result.Submitter.Address.Line);
			AssertNull(result.Submitter.Address.PostcodeId);
			AssertNotNull(result.Submitter.Address.BuildingNumber);

			payer.Setup(m => m.BuildingNumber).Returns(ZString.Empty);
			import5ULMock.Setup(m => m.Payer).Returns(payer.Object);
			result = new GOVCBR5ULMessageBuilder(import5ULMock.Object).GenerateMessage();

			AssertNull(result.Submitter.Address.CountrySubDivisionId);
			AssertNull(result.Submitter.Address.Line);
			AssertNull(result.Submitter.Address.PostcodeId);
			AssertNull(result.Submitter.Address.BuildingNumber);
		}
	}
}
