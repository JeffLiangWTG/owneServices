using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using CusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;
using JobDeclaration = Enterprise.Customs.IT.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class ExportCusEntryHeaderCustomsMessageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new ExportCusEntryHeaderCustomsMessageWrapper(entryHeader: null));
		AssertNoExceptionThrown(() => new ExportCusEntryHeaderCustomsMessageWrapper(entryHeader));
	}

	public void TestExportTermsOfDelivery()
	{
		var wrapper = GetNewWrapper();
		var termOfDeliveryWrapper = wrapper.TermOfDelivery;
		AssertSame(nameof(ICusEntryHeaderCustomsMessageWrapper.TermOfDelivery), termOfDeliveryWrapper, wrapper.TermOfDelivery);
	}

	public void TestDeferredPayment()
	{
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var feeLine = entryHeader.MergedLines.AddNew().Fees.AddNew();

		CombineAssertions(() =>
		{
			var wrapper = GetNewWrapper();
			AssertNullOrEmpty(nameof(wrapper.DeferredPayment), wrapper.DeferredPayment);

			declaration.JE_DefermentAccountNumber = "123456A";
			wrapper = GetNewWrapper();
			AssertEquals("JE_PaymentMethod is empty and CF_MethodOfPayment is empty", ZString.Empty, wrapper.DeferredPayment);

			declaration.JE_PaymentMethod = "A";
			declaration.JE_DefermentAccountNumber = "123456A";
			wrapper = GetNewWrapper();
			AssertEquals("JE_PaymentMethod is not empty and CF_MethodOfPayment is empty", ZString.Empty, wrapper.DeferredPayment);

			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_DefermentAccountNumber = "123456A";
			feeLine.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.OthersD;
			wrapper = GetNewWrapper();
			AssertEquals($"JE_PaymentMethod is empty and CF_MethodOfPayment is {feeLine.CF_MethodOfPayment}", "123456A", wrapper.DeferredPayment);

			feeLine.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE;
			wrapper = GetNewWrapper();
			AssertEquals($"JE_PaymentMethod is empty and CF_MethodOfPayment is {feeLine.CF_MethodOfPayment}", "123456A", wrapper.DeferredPayment);

			feeLine.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentVatProcedureG;
			wrapper = GetNewWrapper();
			AssertEquals($"JE_PaymentMethod is empty and CF_MethodOfPayment is {feeLine.CF_MethodOfPayment}", "123456A", wrapper.DeferredPayment);

			feeLine.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.AgentGeneralGuaranteeAccountT;
			wrapper = GetNewWrapper();
			AssertEquals("JE_PaymentMethod is empty and CF_MethodOfPayment is Not in [D, E, G]", ZString.Empty, wrapper.DeferredPayment);

			declaration.JE_PaymentMethod = "A";
			declaration.JE_DefermentAccountNumber = "123456A";
			feeLine.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.OthersD;
			wrapper = GetNewWrapper();
			AssertEquals($"JE_PaymentMethod is A and CF_MethodOfPayment is {feeLine.CF_MethodOfPayment}", "123456", wrapper.DeferredPayment);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	ICusEntryHeaderCustomsMessageWrapper GetNewWrapper() => new ExportCusEntryHeaderCustomsMessageWrapper(entryHeader);
}
