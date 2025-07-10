using System;

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class AccQueryClaimLogAdderValidation : ZValidation
	{
		public AccQueryClaimLogAdderValidation(AccQueryClaimLogAdder parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public void ValidateLogComment()
		{
			ValidateCalculatedProperty(Parent.LogCommentInfo);
		}

		protected virtual void CheckLogComment()
		{
			MandatoryValidation.CheckEntered(Parent.LogCommentInfo);
		}

		public override void ValidateAll()
		{
			ValidateLogComment();
		}

		public override Type AutoValidationType
		{
			get { return this.GetType(); }
		}

		public readonly AccQueryClaimLogAdder Parent;
	}
}

