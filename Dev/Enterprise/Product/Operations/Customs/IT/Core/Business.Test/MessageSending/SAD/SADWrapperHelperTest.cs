using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Moq;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADWrapperHelperTest : TestCaseWithFactory
{
	public void TestIsItalianCustomsOffice()
	{
		Assert("This is an italian customs office", SADWrapperHelper.IsItalianCustomsOffice("IT123456"));
		Assert("This is NOT an italian customs office", !SADWrapperHelper.IsItalianCustomsOffice("123456"));
		Assert("This is NOT an italian customs office", !SADWrapperHelper.IsItalianCustomsOffice("DE123456"));
		Assert("This is NOT an italian customs office", !SADWrapperHelper.IsItalianCustomsOffice(""));
	}

	public void TestRemoveIsoCodeIfIsItalianCustomsOffice()
	{
		AssertEquals("Reference Number should be", "123456", SADWrapperHelper.RemoveIsoCodeIfIsItalianCustomsOffice("IT123456"));
		AssertEquals("Reference Number should be", "", SADWrapperHelper.RemoveIsoCodeIfIsItalianCustomsOffice("IT"));
		AssertEquals("Reference Number should be", "123456", SADWrapperHelper.RemoveIsoCodeIfIsItalianCustomsOffice("it123456"));
		AssertEquals("Reference Number should be", "DE123456", SADWrapperHelper.RemoveIsoCodeIfIsItalianCustomsOffice("DE123456"));
		AssertEquals("Reference Number should be", "12", SADWrapperHelper.RemoveIsoCodeIfIsItalianCustomsOffice("12"));
		AssertEquals("Reference Number should be", "", SADWrapperHelper.RemoveIsoCodeIfIsItalianCustomsOffice(""));
	}

	public void TestRemoveIsoCode()
	{
		AssertEquals("Reference Number should be", "123456", SADWrapperHelper.RemoveIsoCode("IT123456"));
		AssertEquals("Reference Number should be", "", SADWrapperHelper.RemoveIsoCode("IT"));
		AssertEquals("Reference Number should be", "123456", SADWrapperHelper.RemoveIsoCode("it123456"));
		AssertEquals("Reference Number should be", "123456", SADWrapperHelper.RemoveIsoCode("DE123456"));
		AssertEquals("Reference Number should be", "", SADWrapperHelper.RemoveIsoCode("12"));
		AssertEquals("Reference Number should be", "", SADWrapperHelper.RemoveIsoCodeIfIsItalianCustomsOffice(""));
	}

	public void TestRemoveBlackListChars()
	{
		AssertEquals("", SADWrapperHelper.RemoveBlackListChars(""));
		AssertEquals("this is a string", SADWrapperHelper.RemoveBlackListChars("this is a string"));
		AssertEquals("this is a string", SADWrapperHelper.RemoveBlackListChars("this is\ta string"));
		AssertEquals("this is a string", SADWrapperHelper.RemoveBlackListChars("this is\r\na string"));
		AssertEquals("this is a string", SADWrapperHelper.RemoveBlackListChars("this is\ra string"));
		AssertEquals("this is a string", SADWrapperHelper.RemoveBlackListChars("this is\na string"));
		AssertEquals("this is a super string", SADWrapperHelper.RemoveBlackListChars("this\tis\ra\nsuper\r\nstring"));
	}

	public void TestAppendFEIfElectronicDocument()
	{
		AssertExceptionThrown<ArgumentNullException>("electronicFolderSupporter is required", () => SADWrapperHelper.AppendFEIfElectronicDocument(null, "XYZ"));

		var electronicFolderSupporterMock = new Mock<IElectronicFolderSupporter>();
		electronicFolderSupporterMock.Setup(m => m.UseElectronicFolder).Returns(false);
		AssertEquals(ZString.Empty, SADWrapperHelper.AppendFEIfElectronicDocument(electronicFolderSupporterMock.Object, ZString.Empty));

		electronicFolderSupporterMock.Setup(m => m.UseElectronicFolder).Returns(true);
		AssertEquals("30LOC-FE", SADWrapperHelper.AppendFEIfElectronicDocument(electronicFolderSupporterMock.Object, "30LOC"));

		electronicFolderSupporterMock.Setup(m => m.UseElectronicFolder).Returns(false);
		AssertEquals("30LOC", SADWrapperHelper.AppendFEIfElectronicDocument(electronicFolderSupporterMock.Object, "30LOC"));

		electronicFolderSupporterMock.Setup(m => m.UseElectronicFolder).Returns(true);
		AssertEquals("FE", SADWrapperHelper.AppendFEIfElectronicDocument(electronicFolderSupporterMock.Object, ZString.Empty));
	}

	public void TestGetValueOrNullIfZero()
	{
		AssertNull("When amount is zero", new ZDecimal(0).GetValueOrNullIfZero());
		AssertEquals("When amount is positive", 1m, new ZDecimal(1).GetValueOrNullIfZero());
		AssertEquals("When amount is negative", -1m, new ZDecimal(-1).GetValueOrNullIfZero());
	}

	public void TestGetTrueOrNullIfFalse()
	{
		AssertEquals("When value is false", null, new ZBool(false).GetTrueOrNullIfFalse());
		AssertEquals("When value is true", true, new ZBool(true).GetTrueOrNullIfFalse());
	}

	public void TestIsFeeIncludedInMessageSending()
	{
		var cusEntryLine = Factory.New<CusEntryLine>();
		var fee = cusEntryLine.Fees.AddNew();

		AssertEquals("Fee shouldn't be included when MethodOfPayment is not set", false, SADWrapperHelper.IsFeeIncludedInMessageSending((IFee)fee));
		fee.CF_MethodOfPayment = DutyMethodOfPayment.ImmediatePaymentInCashA;
		AssertEquals("Fee should be included when MethodOfPayment is 'A'", true, SADWrapperHelper.IsFeeIncludedInMessageSending((IFee)fee));
		fee.CF_MethodOfPayment = DutyMethodOfPayment.DeferredPaymentCustomsProcedureF;
		AssertEquals("Fee should be included when MethodOfPayment is 'F'", true, SADWrapperHelper.IsFeeIncludedInMessageSending((IFee)fee));
		fee.CF_MethodOfPayment = DutyMethodOfPayment.DeferredPaymentVatProcedureG;
		AssertEquals("Fee should be included when MethodOfPayment is 'G'", true, SADWrapperHelper.IsFeeIncludedInMessageSending((IFee)fee));
		fee.CF_MethodOfPayment = DutyMethodOfPayment.AgentGeneralGuaranteeAccountT;
		AssertEquals("Fee should be included when MethodOfPayment is 'T'", true, SADWrapperHelper.IsFeeIncludedInMessageSending((IFee)fee));
		fee.CF_MethodOfPayment = "1";
		AssertEquals("Fee shouldn't be included when MethodOfPayment is '1'", false, SADWrapperHelper.IsFeeIncludedInMessageSending((IFee)fee));
		fee.CF_MethodOfPayment = DutyMethodOfPayment.SecurityDepositDeferredPaymentR;
		AssertEquals("Fee shouldn't be included when MethodOfPayment is 'R' and ChargeType different from 'A35' or 'A45'", false, SADWrapperHelper.IsFeeIncludedInMessageSending((IFee)fee));
		fee.CF_ChargeType = RefCusRateCodes.TemporaryAntiDumpingDuty;
		AssertEquals("Fee should be included when MethodOfPayment is 'R' and ChargeType = 'A35'", true, SADWrapperHelper.IsFeeIncludedInMessageSending((IFee)fee));
		fee.CF_ChargeType = RefCusRateCodes.TemportaryCountervailingDuty;
		AssertEquals("Fee should be included when MethodOfPayment is 'R' and ChargeType = 'A45'", true, SADWrapperHelper.IsFeeIncludedInMessageSending((IFee)fee));
	}
}
