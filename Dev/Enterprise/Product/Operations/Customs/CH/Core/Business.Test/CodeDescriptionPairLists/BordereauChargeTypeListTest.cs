using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BordereauChargeTypeList))]
sealed class BordereauChargeTypeListTest : TestCase
{
	public void TestGetChargeTypeTaxationDecisionCustomsDuties() => AssertGetChargeType(BordereauChargeTypeList.Codes.Duties, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties);

	public void TestGetChargeTypeTaxationDecisionVat() => AssertGetChargeType(BordereauChargeTypeList.Codes.Vat, MessageSubTypeCodeList.Codes.TaxationDecisionVat);

	public void TestGetChargeTypeTaxationDecisionReimbursementCustomsDuties() => AssertGetChargeType(BordereauChargeTypeList.Codes.DutiesRefund, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties);

	public void TestGetChargeTypeTaxationDecisionReimbursementVat() => AssertGetChargeType(BordereauChargeTypeList.Codes.VatRefund, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat);

	void AssertGetChargeType(string expectedChargeType, string messageSubType) => AssertEquals(expectedChargeType, BordereauChargeTypeList.GetChargeType(messageSubType));

	public void TestGetMessageSubTypeDuties() => AssertGetMessageSubType(MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties, BordereauChargeTypeList.Codes.Duties);

	public void TestGetMessageSubTypeVat() => AssertGetMessageSubType(MessageSubTypeCodeList.Codes.TaxationDecisionVat, BordereauChargeTypeList.Codes.Vat);

	public void TestGetMessageSubTypeDutiesRefund() => AssertGetMessageSubType(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties, BordereauChargeTypeList.Codes.DutiesRefund);

	public void TestGetMessageSubTypeVatRefund() => AssertGetMessageSubType(MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat, BordereauChargeTypeList.Codes.VatRefund);

	void AssertGetMessageSubType(string expectedMessageSubType, string chargeType) => AssertEquals(expectedMessageSubType, BordereauChargeTypeList.GetMessageSubType(chargeType));
}
