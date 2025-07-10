using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EvvRejectionMessageProcessor))]
sealed class EvvRejectionMessageProcessorTest : BaseEvvMessageProcessorTest
{
	protected override ApplicationTypeMessageProcessor GetMessageProcessor() => new EvvRejectionMessageProcessor(Logger);

	protected override string MessageFriendlyName => "Customs eVV Rejection Message Response Processor";

	public void TestMessageLinkedRuleError() => AssertMessageLinked(MessageSubTypeCodeList.Codes.RuleError, TestingData.InputEvvResponseRuleErrors);

	public void TestMessageLinkedXmlSchemaError() => AssertMessageLinked(MessageSubTypeCodeList.Codes.XmlSchemaError, TestingData.InputEvvResponseXMLSchemaErrors);

	public void TestApplicationReferenceRuleError() => AssertApplicationReference(MessageSubTypeCodeList.Codes.RuleError, TestingData.InputEvvResponseRuleErrors, "XXX");

	public void TestApplicationReferenceXmlSchemaError() => AssertApplicationReference(MessageSubTypeCodeList.Codes.XmlSchemaError, TestingData.InputEvvResponseXMLSchemaErrors, "XXX");

	public void TestEventRuleErrorTaxationDecisionCustomsDuties() => AssertEvent(MessageSubTypeCodeList.Codes.RuleError, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, TestingData.InputEvvResponseRuleErrors, $"|STU=Received-ERROR|TYP={EvvDocumentType.Codes.TaxationDecisionCustomsDuties}");

	public void TestEventRuleErrorTaxationDecisionVat() => AssertEvent(MessageSubTypeCodeList.Codes.RuleError, MessageSubTypeCodeList.Codes.TaxationDecisionVat, TestingData.InputEvvResponseRuleErrors, $"|STU=Received-ERROR|TYP={EvvDocumentType.Codes.TaxationDecisionVAT}");

	public void TestEventXmlSchemaErrorTaxationDecisionCustomsDuties() => AssertEvent(MessageSubTypeCodeList.Codes.XmlSchemaError, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, TestingData.InputEvvResponseXMLSchemaErrors, $"|STU=Received-ERROR|TYP={EvvDocumentType.Codes.TaxationDecisionCustomsDuties}");

	public void TestEventXmlSchemaErrorTaxationDecisionVat() => AssertEvent(MessageSubTypeCodeList.Codes.XmlSchemaError, MessageSubTypeCodeList.Codes.TaxationDecisionVat, TestingData.InputEvvResponseXMLSchemaErrors, $"|STU=Received-ERROR|TYP={EvvDocumentType.Codes.TaxationDecisionVAT}");

	public void TestShipmentRuleErrorTaxationDecisionCustomsDuties() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, TestingData.InputEvvResponseRuleErrors, EvvDocumentType.Codes.TaxationDecisionCustomsDuties, "ERROR");

	public void TestShipmentRuleErrorTaxationDecisionVat() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionVat, TestingData.InputEvvResponseRuleErrors, EvvDocumentType.Codes.TaxationDecisionVAT, "ERROR");

	public void TestShipmentRuleErrorTaxationDecisionReimbursementCustomsDuties() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, TestingData.InputEvvResponseRuleErrors, EvvDocumentType.Codes.RefundCustomsDuties, "ERROR");

	public void TestShipmentRuleErrorTaxationDecisionReimbursementVat() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, TestingData.InputEvvResponseRuleErrors, EvvDocumentType.Codes.RefundVAT, "ERROR");

	public void TestShipmentInputEvvResponseXMLSchemaErrorTaxationDecisionCustomsDuties() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, TestingData.InputEvvResponseXMLSchemaErrors, EvvDocumentType.Codes.TaxationDecisionCustomsDuties, "ERROR");

	public void TestShipmentInputEvvResponseXMLSchemaErrorTaxationDecisionVat() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionVat, TestingData.InputEvvResponseXMLSchemaErrors, EvvDocumentType.Codes.TaxationDecisionVAT, "ERROR");

	public void TestShipmentInputEvvResponseXMLSchemaErrorTaxationDecisionReimbursementCustomsDuties() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, TestingData.InputEvvResponseXMLSchemaErrors, EvvDocumentType.Codes.RefundCustomsDuties, "ERROR");
	
	public void TestShipmentInputEvvResponseXMLSchemaErrorTaxationDecisionReimbursementVat() => AssertProcessing(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, TestingData.InputEvvResponseXMLSchemaErrors, EvvDocumentType.Codes.RefundVAT, "ERROR");

	public void TestStatementLineStatusRuleError() => AssertStatementLineStatus(BordereauReceivedStatusList.Codes.RejectedByCustoms, MessageSubTypeCodeList.Codes.RuleError, "MRN001.2", TestingData.InputEvvResponseRuleErrors);

	public void TestStatementLineStatusXmlSchemaError() => AssertStatementLineStatus(BordereauReceivedStatusList.Codes.RejectedByCustoms, MessageSubTypeCodeList.Codes.XmlSchemaError, "MRN001.2", TestingData.InputEvvResponseXMLSchemaErrors);
}
