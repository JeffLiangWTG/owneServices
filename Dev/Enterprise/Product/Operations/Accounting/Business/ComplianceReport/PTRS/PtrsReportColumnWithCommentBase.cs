using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public abstract class PtrsReportColumnWithCommentBase : PtrsReportColumnWrapperBase, IHaveCommentRequiredValidation
	{
		protected PtrsReportColumnWithCommentBase(AccTaxReturnColumn column) : base(column)
		{
			CommentInfo.AdditionalValidation += ValidateCommentCore;
		}

		public ZString Comment { get => column.ATC_Comment; set => column.ATC_Comment = value; }

		public ZPropertyInfo CommentInfo => GetWrappedZPropertyInfo(nameof(Comment), _ => column.ATC_CommentInfo);

		void ValidateCommentCore()
		{
			if (HasCommentRequiredCheck && Comment.Trim().IsEmpty
				&& CommentRequiredCheck())
			{
				CommentInfo.AddError(Res.GetString("71e87261-3467-4263-8db1-309e6c8188d4", "Reason must be entered when values calculated by compliance report are overridden."));
			}
		}

		protected void ValidateComment()
		{
			if (!IsValidationSuspended && HasCommentRequiredCheck)
			{
				column.Validation.ValidateATC_Comment();
			}
		}

		void IHaveCommentRequiredValidation.ValidateComment() => ValidateComment();

		void IHaveCommentRequiredValidation.SetIsCommentRequiredCheck(Func<bool> isCommentRequired)
		{
			CommentRequiredCheck = isCommentRequired;
		}

		Func<bool> CommentRequiredCheck { get; set; }

		public bool HasCommentRequiredCheck => CommentRequiredCheck != null;
	}
}
