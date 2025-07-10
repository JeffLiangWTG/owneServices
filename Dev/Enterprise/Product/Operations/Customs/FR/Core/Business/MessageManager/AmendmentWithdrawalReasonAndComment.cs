using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business
{
	public class AmendmentWithdrawalReasonAndComment : Customs.Business.AmendmentWithdrawalReason
	{
		[MaxLength(280)]
		public override ZString ReasonText
		{
			get { return base.ReasonText; }
			set { base.ReasonText = value; }
		}

		[MaxLength(280)]
		public ZString CommentText
		{
			get { return fCommentText; }
			set
			{
				var shortenedCommentText = value.Left(CommentTextInfo.MaxLength);
				CheckMaximumLength(CommentTextInfo, shortenedCommentText);
				SetNonPersistentPropertyValue(CommentTextInfo, ref fCommentText, shortenedCommentText);
				RefreshBinding();
			}
		}
		ZString fCommentText;

		public ZPropertyInfo CommentTextInfo
		{
			get { return GetZPropertyInfo(nameof(CommentText)); }
		}
	}
}
