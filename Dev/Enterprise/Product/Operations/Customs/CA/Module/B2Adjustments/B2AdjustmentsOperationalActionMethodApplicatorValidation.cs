using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class B2AdjustmentsOperationalActionMethodApplicatorValidation : ZValidation
	{
		public B2AdjustmentsOperationalActionMethodApplicatorValidation(B2AdjustmentsOperationalActionMethodApplicator parent)
			: base(parent)
		{
			this.parent = parent;
			zValidationInternals = this;
		}

		readonly B2AdjustmentsOperationalActionMethodApplicator parent;
		readonly IValidationInternals zValidationInternals;

		public override void ValidateAll()
		{
			ValidateSubmissionDate();
		}

		public void ValidateSubmissionDate()
		{
			zValidationInternals.Validate(parent.SubmissionDateInfo, CheckSubmissionDate);
		}

		protected void CheckSubmissionDate()
		{
			if (parent.SubmissionDate.Date > ZDate.Today)
			{
				parent.SubmissionDateInfo.AddWarning(Res.GetString("439f84b1-d372-4932-abd1-39cf3b6ac4f6", "Submitted Date should not be in the future."));
			}
		}

		public override Type AutoValidationType => typeof(B2AdjustmentsOperationalActionMethodApplicatorValidation);
	}
}
