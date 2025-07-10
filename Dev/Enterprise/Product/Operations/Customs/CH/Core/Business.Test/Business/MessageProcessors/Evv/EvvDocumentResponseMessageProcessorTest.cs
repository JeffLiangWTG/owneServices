using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EvvDocumentResponseMessageProcessor))]
sealed class EvvDocumentResponseMessageProcessorTest : BaseEvvMessageProcessorTest
{
	protected override ApplicationTypeMessageProcessor GetMessageProcessor() => new EvvDocumentResponseMessageProcessor(Logger);

	protected override string MessageFriendlyName => "Customs eVV Document Message Response Processor";

	public void TestMessageLinkedTaxationDecisionReimbursementVat() => AssertMessageLinked(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, TestingData.InputEvvResponseRefundVAT());

	public void TestMessageLinkedTaxationDecisionReimbursementCustomsDuties() => AssertMessageLinked(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, TestingData.InputEvvResponseRefundCustomsDuties());

	public void TestMessageLinkedTaxationDecisionCustomsDuties() => AssertMessageLinked(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, TestingData.InputEvvResponseDTY());

	public void TestMessageLinkedTaxationDecisionVat() => AssertMessageLinked(MessageSubTypeCodeList.Codes.TaxationDecisionVat, TestingData.InputEvvResponseVAT());

	public void TestApplicationReferenceTaxationDecisionReimbursementVat() => AssertApplicationReference(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, TestingData.InputEvvResponseRefundVAT(), "22CHEI000043143375.1");

	public void TestApplicationReferenceTaxationDecisionReimbursementCustomsDuties() => AssertApplicationReference(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, TestingData.InputEvvResponseRefundCustomsDuties(), "22CHEI000043143375.1");

	public void TestApplicationReferenceTaxationDecisionCustomsDuties() => AssertApplicationReference(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, TestingData.InputEvvResponseDTY(), "22CHEI000043143375.1");

	public void TestApplicationReferenceTaxationDecisionVat() => AssertApplicationReference(MessageSubTypeCodeList.Codes.TaxationDecisionVat, TestingData.InputEvvResponseVAT(), "22CHEI000043143375.1");

	public void TestEventTaxationDecisionReimbursementCustomsDuties() => AssertEvent(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, TestingData.InputEvvResponseRefundCustomsDuties(), $"|STU=Received-OK|TYP={EvvDocumentType.Codes.RefundCustomsDuties}");

	public void TestEventTaxationDecisionReimbursementVat() => AssertEvent(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, TestingData.InputEvvResponseRefundVAT(), $"|STU=Received-OK|TYP={EvvDocumentType.Codes.RefundVAT}");

	public void TestEventTaxationDecisionCustomsDuties() => AssertEvent(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, TestingData.InputEvvResponseDTY(), $"|STU=Received-OK|TYP={EvvDocumentType.Codes.TaxationDecisionCustomsDuties}");

	public void TestEventTaxationDecisionVat() => AssertEvent(MessageSubTypeCodeList.Codes.TaxationDecisionVat, MessageSubTypeCodeList.Codes.TaxationDecisionVat, TestingData.InputEvvResponseVAT(), $"|STU=Received-OK|TYP={EvvDocumentType.Codes.TaxationDecisionVAT}");

	public void TestDocuments() => CombineAssertions(() =>
	{
		AssertDocument(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, TestingData.InputEvvResponseRefundCustomsDuties(), "CustomsDuties", true);
		AssertDocument(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, TestingData.InputEvvResponseRefundVAT(), "VAT", true);
		AssertDocument(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, TestingData.InputEvvResponseDTY(), "CustomsDuties", false);
		AssertDocument(MessageSubTypeCodeList.Codes.TaxationDecisionVat, TestingData.InputEvvResponseVAT(), "VAT", false);

		void AssertDocument(string messageSubType, string incommingMessageContent, string expectedDocType, bool isRefund)
		{
			var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(new BusinessObjectFactory(), ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EVV, messageSubType, messageSubType, incommingMessageContent);
			MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(), ediMessage);

			var assertionMessage = $"MessageSubType={messageSubType}: ";

			var docManagerSupport = entryHeader as IDocManagerSupport;
			AssertEquals(assertionMessage + "eDoc.Count", 2, docManagerSupport.DocManagerInfo.AllEDocs.Count);

			var eDoc = docManagerSupport.DocManagerInfo.AllEDocs[0];
			AssertEquals(assertionMessage + "eVV.FileName", isRefund ? $"e-dec_receiptResponse_receipt_refund{expectedDocType}_22CHEI000043143375_1_CHE326684996.pdf" : $"e-dec_receiptResponse_receipt_taxationDecision{expectedDocType}_22CHEI000043143375_1_CHE326684996.pdf", eDoc?.FileName);
			AssertEquals(assertionMessage + "eVV.DocType", RefDocTypes.MiscellaneousDocument, eDoc?.DocType);
			AssertEquals(assertionMessage + "eVV.ImageData", "%PDF", eDoc.ImageData.ToAscii().SubstringSafe(0, 4));

			eDoc = docManagerSupport.DocManagerInfo.AllEDocs[1];
			AssertEquals(assertionMessage + "VLD.FileName", isRefund ? $"e-dec_receiptResponse_receipt_refund{expectedDocType}_22CHEI000043143375_1_CHE326684996_validationReport.pdf" : $"e-dec_receiptResponse_receipt_taxationDecision{expectedDocType}_22CHEI000043143375_1_CHE326684996_validationReport.pdf", eDoc?.FileName);
			AssertEquals(assertionMessage + "VLD.DocType", RefDocTypes.MiscellaneousDocument, eDoc?.DocType);
			AssertEquals(assertionMessage + "VLD.ImageData", "%PDF", eDoc.ImageData.ToAscii().SubstringSafe(0, 4));
		}
	});

	public void TestShipmentTaxationDecisionCustomsDuties() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, TestingData.InputEvvResponseDTY(documentNumber: "25CH202503130001J2", documentVersion: "7"), EvvDocumentType.Codes.TaxationDecisionCustomsDuties, "OK", "25CH202503130001J2.7");

	public void TestShipmentTaxationDecisionVat() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionVat, TestingData.InputEvvResponseDTY(documentNumber: "25CH202503130001J2", documentVersion: "7"), EvvDocumentType.Codes.TaxationDecisionVAT, "OK", "25CH202503130001J2.7");

	public void TestShipmentTaxationDecisionReimbursementCustomsDuties() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, TestingData.InputEvvResponseRefundCustomsDuties(documentNumber: "25CH202503130001J2", documentVersion: "7"), EvvDocumentType.Codes.RefundCustomsDuties, "OK", "25CH202503130001J2.7");

	public void TestShipmentTaxationDecisionReimbursementVat() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, TestingData.InputEvvResponseRefundVAT(documentNumber: "25CH202503130001J2", documentVersion: "7"), EvvDocumentType.Codes.RefundVAT, "OK", "25CH202503130001J2.7");

	public void TestStatementLineStatusTaxationDecisionReimbursementVat() => AssertStatementLineStatus(BordereauReceivedStatusList.Codes.Received, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, "MRN001.2", TestingData.InputEvvResponseRefundVAT(documentNumber: "MRN001", documentVersion: "2"));

	public void TestStatementLineStatusTaxationDecisionReimbursementCustomsDuties() => AssertStatementLineStatus(BordereauReceivedStatusList.Codes.Received, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, "MRN001.2", TestingData.InputEvvResponseRefundCustomsDuties(documentNumber: "MRN001", documentVersion: "2"));

	public void TestStatementLineStatusTaxationDecisionCustomsDuties() => AssertStatementLineStatus(BordereauReceivedStatusList.Codes.Received, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, "MRN001.2", TestingData.InputEvvResponseDTY(documentNumber: "MRN001", documentVersion: "2"));

	public void TestStatementLineStatusTaxationDecisionVat() => AssertStatementLineStatus(BordereauReceivedStatusList.Codes.Received, MessageSubTypeCodeList.Codes.TaxationDecisionVat, "MRN001.2", TestingData.InputEvvResponseVAT(documentNumber: "MRN001", documentVersion: "2"));

	protected override void SetUp()
	{
		base.SetUp();
		RefCusCodeTestHelper.CreateSwissLocalLanguages(Factory);
	}
}
