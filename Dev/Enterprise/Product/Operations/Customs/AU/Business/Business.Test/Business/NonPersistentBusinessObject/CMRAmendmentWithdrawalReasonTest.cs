using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRAmendmentWithdrawalReason))]
	sealed class CMRAmendmentWithdrawalReasonTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMaxLength()
		{
			CMRAmendmentWithdrawalReason reason = new CMRAmendmentWithdrawalReason();
			AssertEquals("Max length for reason as in Message spec", 512, reason.ReasonTextInfo.MaxLength);
		}

		public void TestValidateChangeWithdrawalReason()
		{
			CMRAmendmentWithdrawalReason reason = new CMRAmendmentWithdrawalReason();
			reason.RunPreSaveValidation();
			AssertEquals("Should have been validated", true, reason.ReasonTextInfo.HasErrors());
		}

		public void TestValidateLengthOfChangeReason()
		{
			CMRAmendmentWithdrawalReason reason = new CMRAmendmentWithdrawalReason();
			reason.ReasonText = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"123456789012345678901234567890123456789012345678901234567890";
			reason.RunPreSaveValidation();
			AssertHasMessageErrors("Too long", reason.ReasonTextInfo);

			reason.ReasonText = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890" +
				"12345678901234567890123456789012345678901234567890";
			reason.RunPreSaveValidation();
			AssertNoMessageErrors("Not too long", reason.ReasonTextInfo);
		}
	}
}
