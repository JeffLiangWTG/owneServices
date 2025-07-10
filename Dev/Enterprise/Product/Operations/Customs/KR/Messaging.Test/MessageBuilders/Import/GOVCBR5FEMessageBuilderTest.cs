using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class GOVCBR5FEMessageBuilderTest : TestCaseWithFactory
	{
		Mock<IImport5FEHeader> importHeaderMock;
		Mock<IAmendmentDetails> importAmendMock;

		protected override void SetUp()
		{
			base.SetUp();

			#region IImportEntryHeader
			importHeaderMock = new Mock<IImport5FEHeader>();
			importHeaderMock.Setup(m => m.ImportDeclarationNumber).Returns("4062001070010U");
			importHeaderMock.Setup(m => m.DeclarationCustomsOffice).Returns("010");
			importHeaderMock.Setup(m => m.DeclarationCustomsDivision).Returns("10");
			importHeaderMock.Setup(m => m.DeclarationDate).Returns(new ZDate("2020-12-23"));
			importHeaderMock.Setup(m => m.DeclarantType).Returns("01");
			importHeaderMock.Setup(m => m.TotalAmendedItemCount).Returns(50);
			importHeaderMock.Setup(m => m.TotalAmendedTaxCount).Returns(1);
			importHeaderMock.Setup(m => m.DomesticTaxPenaltyType).Returns("01");
			importHeaderMock.Setup(m => m.DutyPenaltyReducedYN).Returns("Y");
			importHeaderMock.Setup(m => m.PenaltyExemptionIndicator).Returns("Y");
			importHeaderMock.Setup(m => m.PenaltyExemptionReasonCode).Returns("B5");
			importHeaderMock.Setup(m => m.PenaltyExemptionReason).Returns("가산세면제정당한사유");
			importHeaderMock.Setup(m => m.DutyPenaltyExemption5UASequenceNumber).Returns(3);
			importHeaderMock.Setup(m => m.PenaltyExemptionAmount).Returns(99m);
			importHeaderMock.Setup(m => m.BeforeTotalDutyTaxAmount).Returns(999999999999m);
			importHeaderMock.Setup(m => m.AfterTotalDutyTaxAmount).Returns(999999999999m);
			importHeaderMock.Setup(m => m.DutyTaxDifference).Returns(999999999999m);
			importHeaderMock.Setup(m => m.BeforeCustomsValue).Returns(999999999999m);
			importHeaderMock.Setup(m => m.AfterCustomsValue).Returns(999999999999m);
			importHeaderMock.Setup(m => m.CustomsValueDifference).Returns(999999999999m);
			importHeaderMock.Setup(m => m.DutyPenaltyType).Returns("01");
			#endregion

			#region Declarant, Payer
			var declarantMock = new Mock<IOrganization>();

			declarantMock.Setup(m => m.CompanyName).Returns("상호");
			declarantMock.Setup(m => m.RepresentativeName).Returns("성명");

			importHeaderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);

			var payerMock = new Mock<IOrganization>();

			payerMock.Setup(m => m.CompanyName).Returns("상호");
			payerMock.Setup(m => m.RepresentativeName).Returns("성명");

			importHeaderMock.Setup(m => m.Payer).Returns(payerMock.Object);
			#endregion

			#region IAmendmentDetails
			importAmendMock = new Mock<IAmendmentDetails>();
			importAmendMock.Setup(m => m.AmendmentType).Returns("AD");
			importAmendMock.Setup(m => m.AmendmentVersionNo).Returns(3);
			importAmendMock.Setup(m => m.FaultParty).Returns("A");
			importAmendMock.Setup(m => m.FaultPartyOtherDescription).Returns("귀책사유상세사유");
			importAmendMock.Setup(m => m.ReasonCode).Returns("01");
			importAmendMock.Setup(m => m.AmendReasonDescription).Returns("정정사유");
			importAmendMock.Setup(m => m.PenaltyPaymentReasonCode).Returns("AAA");
			#endregion

			#region IImport5FEItem
			var importItem1Mock = new Mock<IImport5FEItem>();
			importItem1Mock.Setup(m => m.EntryLineNo).Returns(001);
			importItem1Mock.Setup(m => m.AmendType).Returns("01");
			importItem1Mock.Setup(m => m.InvoiceLineNo).Returns(new ZInt(ZString.Empty));
			importItem1Mock.Setup(m => m.GARequirementApprovalNumber).Returns(ZString.Empty);
			importItem1Mock.Setup(m => m.NonGASequnceNo).Returns(new ZInt(ZString.Empty));
			importItem1Mock.Setup(m => m.ExportDeclarationNumber).Returns("99999999");
			importItem1Mock.Setup(m => m.ExportDeclarationEntryLineNo).Returns(1);
			importItem1Mock.Setup(m => m.ExportDeclarationInvoiceLineNo).Returns(01);
			importItem1Mock.Setup(m => m.AmendDataItemID).Returns("B101");
			importItem1Mock.Setup(m => m.BeforeDescription).Returns(ZString.Empty);
			importItem1Mock.Setup(m => m.AfterDescription).Returns("OF BOVINE (INCLUDING BUFFALO) OR EQUINE ANIMALS");

			var importItem2Mock = new Mock<IImport5FEItem>();
			importItem2Mock.Setup(m => m.EntryLineNo).Returns(002);
			importItem2Mock.Setup(m => m.AmendType).Returns("01");
			importItem2Mock.Setup(m => m.InvoiceLineNo).Returns(01);
			importItem2Mock.Setup(m => m.GARequirementApprovalNumber).Returns(ZString.Empty);
			importItem2Mock.Setup(m => m.NonGASequnceNo).Returns(new ZInt(ZString.Empty));
			importItem2Mock.Setup(m => m.ExportDeclarationNumber).Returns("99999999");
			importItem2Mock.Setup(m => m.ExportDeclarationEntryLineNo).Returns(1);
			importItem2Mock.Setup(m => m.ExportDeclarationInvoiceLineNo).Returns(01);
			importItem2Mock.Setup(m => m.AmendDataItemID).Returns("C102");
			importItem2Mock.Setup(m => m.BeforeDescription).Returns(ZString.Empty);
			importItem2Mock.Setup(m => m.AfterDescription).Returns("M0505JWEVLADY DIOR MI GOAM46E TU");

			var importItem3Mock = new Mock<IImport5FEItem>();
			importItem3Mock.Setup(m => m.EntryLineNo).Returns(003);
			importItem3Mock.Setup(m => m.AmendType).Returns("01");
			importItem3Mock.Setup(m => m.InvoiceLineNo).Returns(new ZInt(ZString.Empty));
			importItem3Mock.Setup(m => m.GARequirementApprovalNumber).Returns(ZString.Empty);
			importItem3Mock.Setup(m => m.NonGASequnceNo).Returns(new ZInt(ZString.Empty));
			importItem3Mock.Setup(m => m.ExportDeclarationNumber).Returns("99999999");
			importItem3Mock.Setup(m => m.ExportDeclarationEntryLineNo).Returns(1);
			importItem3Mock.Setup(m => m.ExportDeclarationInvoiceLineNo).Returns(01);
			importItem3Mock.Setup(m => m.AmendDataItemID).Returns("E103");
			importItem3Mock.Setup(m => m.BeforeDescription).Returns(ZString.Empty);
			importItem3Mock.Setup(m => m.AfterDescription).Returns("71");

			var importItem4Mock = new Mock<IImport5FEItem>();
			importItem4Mock.Setup(m => m.EntryLineNo).Returns(004);
			importItem4Mock.Setup(m => m.AmendType).Returns("01");
			importItem4Mock.Setup(m => m.InvoiceLineNo).Returns(01);
			importItem4Mock.Setup(m => m.GARequirementApprovalNumber).Returns("A");
			importItem4Mock.Setup(m => m.NonGASequnceNo).Returns(new ZInt(ZString.Empty));
			importItem4Mock.Setup(m => m.ExportDeclarationNumber).Returns("99999999");
			importItem4Mock.Setup(m => m.ExportDeclarationEntryLineNo).Returns(1);
			importItem4Mock.Setup(m => m.ExportDeclarationInvoiceLineNo).Returns(01);
			importItem4Mock.Setup(m => m.AmendDataItemID).Returns("D103");
			importItem4Mock.Setup(m => m.BeforeDescription).Returns(ZString.Empty);
			importItem4Mock.Setup(m => m.AfterDescription).Returns("01");

			var importItem5Mock = new Mock<IImport5FEItem>();
			importItem5Mock.Setup(m => m.EntryLineNo).Returns(005);
			importItem5Mock.Setup(m => m.AmendType).Returns("02");
			importItem5Mock.Setup(m => m.InvoiceLineNo).Returns(07);
			importItem5Mock.Setup(m => m.GARequirementApprovalNumber).Returns(ZString.Empty);
			importItem5Mock.Setup(m => m.NonGASequnceNo).Returns(new ZInt(ZString.Empty));
			importItem5Mock.Setup(m => m.ExportDeclarationNumber).Returns("99999999");
			importItem5Mock.Setup(m => m.ExportDeclarationEntryLineNo).Returns(1);
			importItem5Mock.Setup(m => m.ExportDeclarationInvoiceLineNo).Returns(01);
			importItem5Mock.Setup(m => m.AmendDataItemID).Returns("C109");
			importItem5Mock.Setup(m => m.BeforeDescription).Returns("N");
			importItem5Mock.Setup(m => m.AfterDescription).Returns("Y");

			var importItem6Mock = new Mock<IImport5FEItem>();
			importItem6Mock.Setup(m => m.EntryLineNo).Returns(006);
			importItem6Mock.Setup(m => m.AmendType).Returns("02");
			importItem6Mock.Setup(m => m.InvoiceLineNo).Returns(new ZInt(ZString.Empty));
			importItem6Mock.Setup(m => m.GARequirementApprovalNumber).Returns(ZString.Empty);
			importItem6Mock.Setup(m => m.NonGASequnceNo).Returns(new ZInt(ZString.Empty));
			importItem6Mock.Setup(m => m.ExportDeclarationNumber).Returns("99999999");
			importItem6Mock.Setup(m => m.ExportDeclarationEntryLineNo).Returns(1);
			importItem6Mock.Setup(m => m.ExportDeclarationInvoiceLineNo).Returns(01);
			importItem6Mock.Setup(m => m.AmendDataItemID).Returns("B111");
			importItem6Mock.Setup(m => m.BeforeDescription).Returns("N");
			importItem6Mock.Setup(m => m.AfterDescription).Returns("Y");

			var importItem7Mock = new Mock<IImport5FEItem>();
			importItem7Mock.Setup(m => m.EntryLineNo).Returns(007);
			importItem7Mock.Setup(m => m.AmendType).Returns("02");
			importItem7Mock.Setup(m => m.InvoiceLineNo).Returns(new ZInt(ZString.Empty));
			importItem7Mock.Setup(m => m.GARequirementApprovalNumber).Returns(ZString.Empty);
			importItem7Mock.Setup(m => m.NonGASequnceNo).Returns(new ZInt("1"));
			importItem7Mock.Setup(m => m.ExportDeclarationNumber).Returns("99999999");
			importItem7Mock.Setup(m => m.ExportDeclarationEntryLineNo).Returns(1);
			importItem7Mock.Setup(m => m.ExportDeclarationInvoiceLineNo).Returns(01);
			importItem7Mock.Setup(m => m.AmendDataItemID).Returns("E106");
			importItem7Mock.Setup(m => m.BeforeDescription).Returns("N");
			importItem7Mock.Setup(m => m.AfterDescription).Returns("Y");

			var importItem8Mock = new Mock<IImport5FEItem>();
			importItem8Mock.Setup(m => m.EntryLineNo).Returns(008);
			importItem8Mock.Setup(m => m.AmendType).Returns("02");
			importItem8Mock.Setup(m => m.InvoiceLineNo).Returns(01);
			importItem8Mock.Setup(m => m.GARequirementApprovalNumber).Returns("QQEQWE");
			importItem8Mock.Setup(m => m.NonGASequnceNo).Returns(new ZInt(ZString.Empty));
			importItem8Mock.Setup(m => m.ExportDeclarationNumber).Returns("99999999");
			importItem8Mock.Setup(m => m.ExportDeclarationEntryLineNo).Returns(1);
			importItem8Mock.Setup(m => m.ExportDeclarationInvoiceLineNo).Returns(01);
			importItem8Mock.Setup(m => m.AmendDataItemID).Returns("D109");
			importItem8Mock.Setup(m => m.BeforeDescription).Returns("N");
			importItem8Mock.Setup(m => m.AfterDescription).Returns("Y");

			var importItem9Mock = new Mock<IImport5FEItem>();
			importItem9Mock.Setup(m => m.EntryLineNo).Returns(009);
			importItem9Mock.Setup(m => m.AmendType).Returns("03");
			importItem9Mock.Setup(m => m.InvoiceLineNo).Returns(new ZInt(ZString.Empty));
			importItem9Mock.Setup(m => m.GARequirementApprovalNumber).Returns(ZString.Empty);
			importItem9Mock.Setup(m => m.NonGASequnceNo).Returns(new ZInt(ZString.Empty));
			importItem9Mock.Setup(m => m.ExportDeclarationNumber).Returns("99999999");
			importItem9Mock.Setup(m => m.ExportDeclarationEntryLineNo).Returns(1);
			importItem9Mock.Setup(m => m.ExportDeclarationInvoiceLineNo).Returns(01);
			importItem9Mock.Setup(m => m.AmendDataItemID).Returns("B301");
			importItem9Mock.Setup(m => m.BeforeDescription).Returns("22805083");
			importItem9Mock.Setup(m => m.AfterDescription).Returns("20929274");

			var importItem10Mock = new Mock<IImport5FEItem>();
			importItem10Mock.Setup(m => m.EntryLineNo).Returns(010);
			importItem10Mock.Setup(m => m.AmendType).Returns("03");
			importItem10Mock.Setup(m => m.InvoiceLineNo).Returns(new ZInt(ZString.Empty));
			importItem10Mock.Setup(m => m.GARequirementApprovalNumber).Returns(ZString.Empty);
			importItem10Mock.Setup(m => m.NonGASequnceNo).Returns(new ZInt(ZString.Empty));
			importItem10Mock.Setup(m => m.ExportDeclarationNumber).Returns("99999999");
			importItem10Mock.Setup(m => m.ExportDeclarationEntryLineNo).Returns(1);
			importItem10Mock.Setup(m => m.ExportDeclarationInvoiceLineNo).Returns(01);
			importItem10Mock.Setup(m => m.AmendDataItemID).Returns("A825");
			importItem10Mock.Setup(m => m.BeforeDescription).Returns("28159458");
			importItem10Mock.Setup(m => m.AfterDescription).Returns("28166182");

			importHeaderMock.Setup(x => x.AmendedItems).Returns(new IImport5FEItem[] { importItem1Mock.Object, importItem2Mock.Object,
																		importItem3Mock.Object, importItem4Mock.Object,
																		importItem5Mock.Object, importItem6Mock.Object,
																		importItem7Mock.Object, importItem8Mock.Object,
																		importItem9Mock.Object, importItem10Mock.Object });
			#endregion

			#region IImport5FETaxItem
			var importTaxItem1Mock = new Mock<IImport5FETaxItem>();
			importTaxItem1Mock.Setup(m => m.DutyTaxType).Returns("CUD");
			importTaxItem1Mock.Setup(m => m.BeforeAmount).Returns(10000m);
			importTaxItem1Mock.Setup(m => m.AfterAmount).Returns(20000m);
			importTaxItem1Mock.Setup(m => m.AmountDifference).Returns(10000m);

			var importTaxItem2Mock = new Mock<IImport5FETaxItem>();
			importTaxItem2Mock.Setup(m => m.DutyTaxType).Returns("IND");
			importTaxItem2Mock.Setup(m => m.BeforeAmount).Returns(15000m);
			importTaxItem2Mock.Setup(m => m.AfterAmount).Returns(30000m);
			importTaxItem2Mock.Setup(m => m.AmountDifference).Returns(15000m);

			var importTaxItem3Mock = new Mock<IImport5FETaxItem>();
			importTaxItem3Mock.Setup(m => m.DutyTaxType).Returns("ENV");
			importTaxItem3Mock.Setup(m => m.BeforeAmount).Returns(20000m);
			importTaxItem3Mock.Setup(m => m.AfterAmount).Returns(40000m);
			importTaxItem3Mock.Setup(m => m.AmountDifference).Returns(20000m);

			importHeaderMock.Setup(m => m.TaxItems).Returns(new IImport5FETaxItem[] { importTaxItem1Mock.Object, importTaxItem2Mock.Object, importTaxItem3Mock.Object });
			#endregion

		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2014, 05, 06)]
		public void TestGenerateDeclaration()
		{
			importHeaderMock.Setup(m => m.RefundRequestNumber).Returns("0000000000000");

			var result = new GOVCBR5FEMessageBuilder(importHeaderMock.Object, importAmendMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5FE_0.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			#region IImportEntryHeader
			AssertEquals("01010", result.DeclarationOfficeId.Value);
			AssertEquals(50m, result.GoodsItemQuantity.Value);
			AssertEquals("4062001070010U", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("20201223", result.JurisdictionDateTime);
			AssertEquals(1m, result.LoadingListQuantity.Value);
			AssertEquals("GOVCBR5FE", result.TypeCode.Value);
			AssertEquals("Y", result.AdditionalCode.AttachmentSubmissionTypeCode.Value);
			AssertEquals(99m, result.AdditionalDocument.AmountAmount.Value);
			AssertEquals("0000000000000", result.AdditionalDocument.Id.Value);
			AssertEquals("가산세면제정당한사유", result.AdditionalInformation.Content.Value);
			AssertEquals("Y", result.AdditionalInformation.StatementCode.Value);
			AssertEquals("B5", result.AdditionalInformation.StatementTypeCode.Value);
			AssertEquals("3", result.AdditionalInformation.StatementDescription.Value);
			AssertEquals(999999999999m, result.GoodsShipment.Consignment.DutyTaxFee.AdValoremTaxBaseAmount.Value);
			AssertEquals("01", result.GoodsShipment.Consignment.DutyTaxFee.AdditionalTaxTypeCode.Value);
			AssertEquals("Y", result.GoodsShipment.Consignment.DutyTaxFee.AdditionalTaxReductionCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.Consignment.DutyTaxFee.TotalTaxAmount.Value);
			AssertEquals("1", result.GoodsShipment.DutyTaxFee[0].RequestOverrideCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.DutyTaxFee[0].Payment.TaxAssessedAmount.Value);
			AssertEquals(999999999999m, result.GoodsShipment.DutyTaxFee[0].Payment.AssessmentAmount.Value);
			AssertEquals("2", result.GoodsShipment.DutyTaxFee[1].RequestOverrideCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.DutyTaxFee[1].Payment.TaxAssessedAmount.Value);
			AssertEquals(999999999999m, result.GoodsShipment.DutyTaxFee[1].Payment.AssessmentAmount.Value);
			AssertEquals("01", result.GovernmentInformation.TypeCode.Value);
			AssertEquals("01", result.Submitter.RoleCode.Value);
			#endregion

			#region Declarant, Payer
			AssertEquals("상호", result.Submitter.Name.Value);
			AssertEquals("성명", result.Submitter.Contact.Name.Value);

			AssertEquals("상호", result.Payer.Name.Value);
			AssertEquals("성명", result.Payer.Contact.Name.Value);
			#endregion

			#region IAmendmentDetails
			AssertEquals("3", result.VersionId.Value);
			AssertEquals("AD", result.TransactionNatureCode.Value);
			AssertEquals("귀책사유상세사유", result.Reason.Value);
			AssertEquals("A", result.ReasonCode.Value);
			AssertEquals("AAA", result.AdditionalInformation.AdditionalPaymentCode.Value);
			AssertEquals("01", result.Amendment.ChangeReasonCode.Value);
			AssertEquals("정정사유", result.Amendment.Content.Value);
			#endregion

			#region IImport5FEItem
			AssertEquals(001m, result.Consignment[0].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[0].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[0].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[0].AdditionalDocument.LineNumeric);
			AssertEquals("01", result.Consignment[0].Amendment.ChangeReasonCode.Value);
			AssertEquals("OF BOVINE (INCLUDING BUFFALO) OR EQUINE ANIMALS", result.Consignment[0].Amendment.AdjustmentDescription.Value);
			AssertEquals("B101", result.Consignment[0].Amendment.Pointer.TagId.Value);

			AssertEquals(002m, result.Consignment[1].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[1].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[1].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[1].AdditionalDocument.LineNumeric);
			AssertEquals("01", result.Consignment[1].Amendment.ChangeReasonCode.Value);
			AssertEquals("M0505JWEVLADY DIOR MI GOAM46E TU", result.Consignment[1].Amendment.AdjustmentDescription.Value);
			AssertEquals("1", result.Consignment[1].Amendment.Pointer.DocumentSectionCode.Value);
			AssertEquals("C102", result.Consignment[1].Amendment.Pointer.TagId.Value);

			AssertEquals(003m, result.Consignment[2].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[2].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[2].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[2].AdditionalDocument.LineNumeric);
			AssertEquals("01", result.Consignment[2].Amendment.ChangeReasonCode.Value);
			AssertEquals("71", result.Consignment[2].Amendment.AdjustmentDescription.Value);
			AssertEquals("E103", result.Consignment[2].Amendment.Pointer.TagId.Value);

			AssertEquals(004m, result.Consignment[3].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[3].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[3].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[3].AdditionalDocument.LineNumeric);
			AssertEquals("01", result.Consignment[3].Amendment.ChangeReasonCode.Value);
			AssertEquals("01", result.Consignment[3].Amendment.AdjustmentDescription.Value);
			AssertEquals("1", result.Consignment[3].Amendment.Pointer.DocumentSectionCode.Value);
			AssertEquals("D103", result.Consignment[3].Amendment.Pointer.TagId.Value);
			AssertEquals("A", result.Consignment[3].PreviousDocument.Id.Value);

			AssertEquals(005m, result.Consignment[4].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[4].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[4].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[4].AdditionalDocument.LineNumeric);
			AssertEquals("02", result.Consignment[4].Amendment.ChangeReasonCode.Value);
			AssertEquals("N", result.Consignment[4].Amendment.StatementDescription.Value);
			AssertEquals("Y", result.Consignment[4].Amendment.AdjustmentDescription.Value);
			AssertEquals("7", result.Consignment[4].Amendment.Pointer.DocumentSectionCode.Value);
			AssertEquals("C109", result.Consignment[4].Amendment.Pointer.TagId.Value);

			AssertEquals(006m, result.Consignment[5].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[5].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[5].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[5].AdditionalDocument.LineNumeric);
			AssertEquals("02", result.Consignment[5].Amendment.ChangeReasonCode.Value);
			AssertEquals("N", result.Consignment[5].Amendment.StatementDescription.Value);
			AssertEquals("Y", result.Consignment[5].Amendment.AdjustmentDescription.Value);
			AssertEquals("B111", result.Consignment[5].Amendment.Pointer.TagId.Value);

			AssertEquals(007m, result.Consignment[6].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[6].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[6].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[6].AdditionalDocument.LineNumeric);
			AssertEquals("02", result.Consignment[6].Amendment.ChangeReasonCode.Value);
			AssertEquals("N", result.Consignment[6].Amendment.StatementDescription.Value);
			AssertEquals("Y", result.Consignment[6].Amendment.AdjustmentDescription.Value);
			AssertEquals("E106", result.Consignment[6].Amendment.Pointer.TagId.Value);
			AssertEquals(1m, result.Consignment[6].PreviousDocument.SequenceNumeric);

			AssertEquals(008m, result.Consignment[7].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[7].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[7].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[7].AdditionalDocument.LineNumeric);
			AssertEquals("02", result.Consignment[7].Amendment.ChangeReasonCode.Value);
			AssertEquals("N", result.Consignment[7].Amendment.StatementDescription.Value);
			AssertEquals("Y", result.Consignment[7].Amendment.AdjustmentDescription.Value);
			AssertEquals("1", result.Consignment[7].Amendment.Pointer.DocumentSectionCode.Value);
			AssertEquals("D109", result.Consignment[7].Amendment.Pointer.TagId.Value);
			AssertEquals("QQEQWE", result.Consignment[7].PreviousDocument.Id.Value);

			AssertEquals(009m, result.Consignment[8].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[8].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[8].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[8].AdditionalDocument.LineNumeric);
			AssertEquals("03", result.Consignment[8].Amendment.ChangeReasonCode.Value);
			AssertEquals("22805083", result.Consignment[8].Amendment.StatementDescription.Value);
			AssertEquals("20929274", result.Consignment[8].Amendment.AdjustmentDescription.Value);
			AssertEquals("B301", result.Consignment[8].Amendment.Pointer.TagId.Value);

			AssertEquals(010m, result.Consignment[9].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[9].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[9].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[9].AdditionalDocument.LineNumeric);
			AssertEquals("03", result.Consignment[9].Amendment.ChangeReasonCode.Value);
			AssertEquals("28159458", result.Consignment[9].Amendment.StatementDescription.Value);
			AssertEquals("28166182", result.Consignment[9].Amendment.AdjustmentDescription.Value);
			AssertEquals("A825", result.Consignment[9].Amendment.Pointer.TagId.Value);
			#endregion

			#region IImport5FETaxItem
			AssertEquals("CUD", result.DutyTaxFee[0].TypeCode.Value);
			AssertEquals(10000m, result.DutyTaxFee[0].DifferenceAmount.Value);
			AssertEquals(10000m, result.DutyTaxFee[0].Payment.TaxAssessedAmount.Value);
			AssertEquals(20000m, result.DutyTaxFee[0].Payment.PaymentAmount.Value);

			AssertEquals("IND", result.DutyTaxFee[1].TypeCode.Value);
			AssertEquals(15000m, result.DutyTaxFee[1].DifferenceAmount.Value);
			AssertEquals(15000m, result.DutyTaxFee[1].Payment.TaxAssessedAmount.Value);
			AssertEquals(30000m, result.DutyTaxFee[1].Payment.PaymentAmount.Value);

			AssertEquals("ENV", result.DutyTaxFee[2].TypeCode.Value);
			AssertEquals(20000m, result.DutyTaxFee[2].DifferenceAmount.Value);
			AssertEquals(20000m, result.DutyTaxFee[2].Payment.TaxAssessedAmount.Value);
			AssertEquals(40000m, result.DutyTaxFee[2].Payment.PaymentAmount.Value);
			#endregion
			importHeaderMock.VerifyAll();
			importAmendMock.VerifyAll();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2015, 06, 07)]
		public void TestEmptyData()
		{
			importHeaderMock.Setup(m => m.RefundRequestNumber).Returns("");

			var result = new GOVCBR5FEMessageBuilder(importHeaderMock.Object, importAmendMock.Object).GenerateMessage();

			var testFile = File.OpenRead(Path.Combine(MessageBuilderTestHelper.ImportOutgoingTestFilePath, "GOVCBR5FE_EmptyData.xml"));
			using (var stream = new CargoWise.IO.Shim.SubStreamableStream(testFile))
			{
				using (var makeStream = KRXmlObjectSerializer.Serialize(result))
				{
					var readerSource = new TextReaderSource(makeStream);
					var serialisedXml = readerSource.GetReader().ReadToEnd();

					AssertXMLEquals(stream.WriteToString(), serialisedXml);
				}
			}

			#region IImportEntryHeader
			AssertEquals("01010", result.DeclarationOfficeId.Value);
			AssertEquals(50m, result.GoodsItemQuantity.Value);
			AssertEquals("4062001070010U", result.Id.Value);
			AssertEquals(ZDate.Today.ToString("yyyyMMdd"), result.IssueDateTime);
			AssertEquals("20201223", result.JurisdictionDateTime);
			AssertEquals(1m, result.LoadingListQuantity.Value);
			AssertEquals("GOVCBR5FE", result.TypeCode.Value);
			AssertEquals("N", result.AdditionalCode.AttachmentSubmissionTypeCode.Value);
			AssertEquals(99m, result.AdditionalDocument.AmountAmount.Value);
			AssertNull(result.AdditionalDocument.Id);
			AssertEquals("가산세면제정당한사유", result.AdditionalInformation.Content.Value);
			AssertEquals("Y", result.AdditionalInformation.StatementCode.Value);
			AssertEquals("B5", result.AdditionalInformation.StatementTypeCode.Value);
			AssertEquals("3", result.AdditionalInformation.StatementDescription.Value);
			AssertEquals(999999999999m, result.GoodsShipment.Consignment.DutyTaxFee.AdValoremTaxBaseAmount.Value);
			AssertEquals("01", result.GoodsShipment.Consignment.DutyTaxFee.AdditionalTaxTypeCode.Value);
			AssertEquals("Y", result.GoodsShipment.Consignment.DutyTaxFee.AdditionalTaxReductionCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.Consignment.DutyTaxFee.TotalTaxAmount.Value);
			AssertEquals("1", result.GoodsShipment.DutyTaxFee[0].RequestOverrideCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.DutyTaxFee[0].Payment.TaxAssessedAmount.Value);
			AssertEquals(999999999999m, result.GoodsShipment.DutyTaxFee[0].Payment.AssessmentAmount.Value);
			AssertEquals("2", result.GoodsShipment.DutyTaxFee[1].RequestOverrideCode.Value);
			AssertEquals(999999999999m, result.GoodsShipment.DutyTaxFee[1].Payment.TaxAssessedAmount.Value);
			AssertEquals(999999999999m, result.GoodsShipment.DutyTaxFee[1].Payment.AssessmentAmount.Value);
			AssertEquals("01", result.GovernmentInformation.TypeCode.Value);
			AssertEquals("01", result.Submitter.RoleCode.Value);
			#endregion

			#region Declarant, Payer
			AssertEquals("상호", result.Submitter.Name.Value);
			AssertEquals("성명", result.Submitter.Contact.Name.Value);

			AssertEquals("상호", result.Payer.Name.Value);
			AssertEquals("성명", result.Payer.Contact.Name.Value);
			#endregion

			#region IAmendmentDetails
			AssertEquals("3", result.VersionId.Value);
			AssertEquals("AD", result.TransactionNatureCode.Value);
			AssertEquals("귀책사유상세사유", result.Reason.Value);
			AssertEquals("A", result.ReasonCode.Value);
			AssertEquals("AAA", result.AdditionalInformation.AdditionalPaymentCode.Value);
			AssertEquals("01", result.Amendment.ChangeReasonCode.Value);
			AssertEquals("정정사유", result.Amendment.Content.Value);
			#endregion

			#region IImport5FEItem
			AssertEquals(001m, result.Consignment[0].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[0].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[0].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[0].AdditionalDocument.LineNumeric);
			AssertEquals("01", result.Consignment[0].Amendment.ChangeReasonCode.Value);
			AssertEquals("OF BOVINE (INCLUDING BUFFALO) OR EQUINE ANIMALS", result.Consignment[0].Amendment.AdjustmentDescription.Value);
			AssertEquals("B101", result.Consignment[0].Amendment.Pointer.TagId.Value);

			AssertEquals(002m, result.Consignment[1].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[1].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[1].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[1].AdditionalDocument.LineNumeric);
			AssertEquals("01", result.Consignment[1].Amendment.ChangeReasonCode.Value);
			AssertEquals("M0505JWEVLADY DIOR MI GOAM46E TU", result.Consignment[1].Amendment.AdjustmentDescription.Value);
			AssertEquals("1", result.Consignment[1].Amendment.Pointer.DocumentSectionCode.Value);
			AssertEquals("C102", result.Consignment[1].Amendment.Pointer.TagId.Value);

			AssertEquals(003m, result.Consignment[2].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[2].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[2].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[2].AdditionalDocument.LineNumeric);
			AssertEquals("01", result.Consignment[2].Amendment.ChangeReasonCode.Value);
			AssertEquals("71", result.Consignment[2].Amendment.AdjustmentDescription.Value);
			AssertEquals("E103", result.Consignment[2].Amendment.Pointer.TagId.Value);

			AssertEquals(004m, result.Consignment[3].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[3].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[3].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[3].AdditionalDocument.LineNumeric);
			AssertEquals("01", result.Consignment[3].Amendment.ChangeReasonCode.Value);
			AssertEquals("01", result.Consignment[3].Amendment.AdjustmentDescription.Value);
			AssertEquals("1", result.Consignment[3].Amendment.Pointer.DocumentSectionCode.Value);
			AssertEquals("D103", result.Consignment[3].Amendment.Pointer.TagId.Value);
			AssertEquals("A", result.Consignment[3].PreviousDocument.Id.Value);

			AssertEquals(005m, result.Consignment[4].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[4].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[4].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[4].AdditionalDocument.LineNumeric);
			AssertEquals("02", result.Consignment[4].Amendment.ChangeReasonCode.Value);
			AssertEquals("N", result.Consignment[4].Amendment.StatementDescription.Value);
			AssertEquals("Y", result.Consignment[4].Amendment.AdjustmentDescription.Value);
			AssertEquals("7", result.Consignment[4].Amendment.Pointer.DocumentSectionCode.Value);
			AssertEquals("C109", result.Consignment[4].Amendment.Pointer.TagId.Value);

			AssertEquals(006m, result.Consignment[5].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[5].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[5].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[5].AdditionalDocument.LineNumeric);
			AssertEquals("02", result.Consignment[5].Amendment.ChangeReasonCode.Value);
			AssertEquals("N", result.Consignment[5].Amendment.StatementDescription.Value);
			AssertEquals("Y", result.Consignment[5].Amendment.AdjustmentDescription.Value);
			AssertEquals("B111", result.Consignment[5].Amendment.Pointer.TagId.Value);

			AssertEquals(007m, result.Consignment[6].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[6].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[6].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[6].AdditionalDocument.LineNumeric);
			AssertEquals("02", result.Consignment[6].Amendment.ChangeReasonCode.Value);
			AssertEquals("N", result.Consignment[6].Amendment.StatementDescription.Value);
			AssertEquals("Y", result.Consignment[6].Amendment.AdjustmentDescription.Value);
			AssertEquals("E106", result.Consignment[6].Amendment.Pointer.TagId.Value);
			AssertEquals(1m, result.Consignment[6].PreviousDocument.SequenceNumeric);

			AssertEquals(008m, result.Consignment[7].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[7].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[7].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[7].AdditionalDocument.LineNumeric);
			AssertEquals("02", result.Consignment[7].Amendment.ChangeReasonCode.Value);
			AssertEquals("N", result.Consignment[7].Amendment.StatementDescription.Value);
			AssertEquals("Y", result.Consignment[7].Amendment.AdjustmentDescription.Value);
			AssertEquals("1", result.Consignment[7].Amendment.Pointer.DocumentSectionCode.Value);
			AssertEquals("D109", result.Consignment[7].Amendment.Pointer.TagId.Value);
			AssertEquals("QQEQWE", result.Consignment[7].PreviousDocument.Id.Value);

			AssertEquals(009m, result.Consignment[8].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[8].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[8].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[8].AdditionalDocument.LineNumeric);
			AssertEquals("03", result.Consignment[8].Amendment.ChangeReasonCode.Value);
			AssertEquals("22805083", result.Consignment[8].Amendment.StatementDescription.Value);
			AssertEquals("20929274", result.Consignment[8].Amendment.AdjustmentDescription.Value);
			AssertEquals("B301", result.Consignment[8].Amendment.Pointer.TagId.Value);

			AssertEquals(010m, result.Consignment[9].SequenceNumeric);
			AssertEquals("99999999", result.Consignment[9].AdditionalDocument.Id.Value);
			AssertEquals(1m, result.Consignment[9].AdditionalDocument.SequenceNumeric);
			AssertEquals(01m, result.Consignment[9].AdditionalDocument.LineNumeric);
			AssertEquals("03", result.Consignment[9].Amendment.ChangeReasonCode.Value);
			AssertEquals("28159458", result.Consignment[9].Amendment.StatementDescription.Value);
			AssertEquals("28166182", result.Consignment[9].Amendment.AdjustmentDescription.Value);
			AssertEquals("A825", result.Consignment[9].Amendment.Pointer.TagId.Value);
			#endregion

			#region IImport5FETaxItem
			AssertEquals("CUD", result.DutyTaxFee[0].TypeCode.Value);
			AssertEquals(10000m, result.DutyTaxFee[0].DifferenceAmount.Value);
			AssertEquals(10000m, result.DutyTaxFee[0].Payment.TaxAssessedAmount.Value);
			AssertEquals(20000m, result.DutyTaxFee[0].Payment.PaymentAmount.Value);

			AssertEquals("IND", result.DutyTaxFee[1].TypeCode.Value);
			AssertEquals(15000m, result.DutyTaxFee[1].DifferenceAmount.Value);
			AssertEquals(15000m, result.DutyTaxFee[1].Payment.TaxAssessedAmount.Value);
			AssertEquals(30000m, result.DutyTaxFee[1].Payment.PaymentAmount.Value);

			AssertEquals("ENV", result.DutyTaxFee[2].TypeCode.Value);
			AssertEquals(20000m, result.DutyTaxFee[2].DifferenceAmount.Value);
			AssertEquals(20000m, result.DutyTaxFee[2].Payment.TaxAssessedAmount.Value);
			AssertEquals(40000m, result.DutyTaxFee[2].Payment.PaymentAmount.Value);
			#endregion
			importHeaderMock.VerifyAll();
			importAmendMock.VerifyAll();
		}

		readonly Mock<IImport5FEHeader> emptyMock = new Mock<IImport5FEHeader>();
		readonly Mock<IAmendmentDetails> emptyAmendmentMock = new Mock<IAmendmentDetails>();

		public void TestNotCreateElementWhenNoMandatoryFirstNode()
		{
			var result = new GOVCBR5FEMessageBuilder(emptyMock.Object, emptyAmendmentMock.Object).GenerateMessage();

			AssertNotNull(result.DeclarationOfficeId);
			AssertNotNull(result.GoodsItemQuantity);
			AssertNotNull(result.Id);
			AssertNotNull(result.IssueDateTime);
			AssertNotNull(result.JurisdictionDateTime);
			AssertNull(result.LoadingListQuantity);
			AssertNotNull(result.TypeCode);
			AssertNotNull(result.VersionId);
			AssertNotNull(result.TransactionNatureCode);
			AssertNull(result.Reason);
			AssertNotNull(result.ReasonCode);
			AssertNotNull(result.AdditionalCode);
			AssertNull(result.AdditionalDocument);
			AssertNotNull(result.AdditionalInformation);
			AssertNotNull(result.Amendment);
			AssertNotNull(result.Consignment);
			AssertNull(result.DutyTaxFee);
			AssertNull(result.GoodsShipment);
			AssertNull(result.GovernmentInformation);
			AssertNotNull(result.Submitter);
			AssertNotNull(result.Payer);
		}

		public void TestNotCreateElementWhenNoMandatoryAdditionalInformation()
		{
			var result = new GOVCBR5FEMessageBuilder(emptyMock.Object, emptyAmendmentMock.Object).GenerateMessage();

			AssertNull(result.AdditionalInformation.Content);
			AssertNotNull(result.AdditionalInformation.StatementCode);
			AssertNull(result.AdditionalInformation.StatementTypeCode);
			AssertNull(result.AdditionalInformation.StatementDescription);
			AssertNull(result.AdditionalInformation.AdditionalPaymentCode);
		}

		public void TestNotCreateElementWhenNoMandatoryAmendment()
		{
			var result = new GOVCBR5FEMessageBuilder(emptyMock.Object, emptyAmendmentMock.Object).GenerateMessage();

			AssertNotNull(result.Amendment.ChangeReasonCode);
			AssertNull(result.Amendment.Content);
		}

		public void TestNotCreateElementWhenNoMandatoryConsignment()
		{
			var importItem1Mock = new Mock<IImport5FEItem>();
			importItem1Mock.Setup(m => m.EntryLineNo).Returns(001);
			emptyMock.Setup(x => x.AmendedItems).Returns(new IImport5FEItem[] { importItem1Mock.Object });

			var result = new GOVCBR5FEMessageBuilder(emptyMock.Object, emptyAmendmentMock.Object).GenerateMessage();

			AssertNotNull(result.Consignment[0].SequenceNumeric);
			AssertNull(result.Consignment[0].AdditionalDocument);

			AssertNotNull(result.Consignment[0].Amendment);
			AssertNotNull(result.Consignment[0].Amendment.ChangeReasonCode);
			AssertNull(result.Consignment[0].Amendment.StatementDescription);
			AssertNull(result.Consignment[0].Amendment.AdjustmentDescription);
			AssertNotNull(result.Consignment[0].Amendment.Pointer);

			AssertNull(result.Consignment[0].Amendment.Pointer.DocumentSectionCode);
			AssertNotNull(result.Consignment[0].Amendment.Pointer.TagId);
			AssertNull(result.Consignment[0].PreviousDocument);
		}

		public void TestEmptyJurisdictionDateTime()
		{
			importHeaderMock.Setup(m => m.DeclarationDate).Returns(ZDate.Invalid);
			var result = new GOVCBR5FEMessageBuilder(importHeaderMock.Object, importAmendMock.Object).GenerateMessage();
			AssertEquals(ZString.Empty, result.JurisdictionDateTime);

			importHeaderMock.Setup(m => m.DeclarationDate).Returns(new ZDate("2012-01-01"));
			result = new GOVCBR5FEMessageBuilder(importHeaderMock.Object, importAmendMock.Object).GenerateMessage();
			AssertEquals("20120101", result.JurisdictionDateTime);
		}
	}
}
