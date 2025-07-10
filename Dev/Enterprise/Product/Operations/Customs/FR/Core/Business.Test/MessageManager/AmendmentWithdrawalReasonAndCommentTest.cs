using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessageManager.Testing
{
	[TestedType(typeof(AmendmentWithdrawalReasonAndComment))]
	class AmendmentWithdrawalReasonAndCommentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMaxLengthReasonText()
		{
			var amendmentWithdrawalReasonAndComment = new AmendmentWithdrawalReasonAndComment();
			AssertEquals("ReasonText maxlength is 280", 280, amendmentWithdrawalReasonAndComment.ReasonTextInfo.MaxLength);
		}

		public void TestMaxLengthCommentText()
		{
			var amendmentWithdrawalReasonAndComment = new AmendmentWithdrawalReasonAndComment();
			AssertEquals("CommentText maxlength is 280", 280, amendmentWithdrawalReasonAndComment.CommentTextInfo.MaxLength);
		}

		[ExpectNoExceptions]
		public void TestMaxLength()
		{
			var comment = (AmendmentWithdrawalReasonAndComment)GetNewBusinessObject();
			comment.CommentText = new ZString(new string('*', comment.CommentTextInfo.MaxLength + 10));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AmendmentWithdrawalReasonAndComment();
		}

		public void TestValidateReasonText()
		{
			var reason = GetAmendmentWithdrawalReason();
			reason.RunPreSaveValidation();
			AssertEquals("Should have validated", true, reason.ReasonTextInfo.HasErrors());
		}

		public void TestEmptyReasonIsAnError()
		{
			var reason = GetAmendmentWithdrawalReason();
			reason.ReasonText = "";
			AssertEquals("Should have validated", true, reason.ReasonTextInfo.HasErrors());

			reason.ReasonText = "BLAH";
			AssertEquals("Should have validated", false, reason.ReasonTextInfo.HasErrors());
		}

		protected AmendmentWithdrawalReasonAndComment GetAmendmentWithdrawalReason()
		{
			return new AmendmentWithdrawalReasonAndComment();
		}
	}
}
