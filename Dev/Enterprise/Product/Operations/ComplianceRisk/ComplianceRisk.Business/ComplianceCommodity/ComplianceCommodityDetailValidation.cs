//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoComplianceCommodityDetailValidation
//
//    This class should be used for overriding validation in AutoComplianceCommodityDetailValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using CargoWise.EntityFramework;

namespace Enterprise.ComplianceRisk.Business
{
	public class ComplianceCommodityDetailValidation : AutoComplianceCommodityDetailValidation
	{
		public ComplianceCommodityDetailValidation(AutoComplianceCommodityDetail parent) : base(parent)
		{
			if (ReferenceEquals(parent, null))
			{
				throw new ArgumentNullException(nameof(parent), "Parent cannot be null");
			}
			Parent = (ComplianceCommodityDetail)parent;
			ZValidationInternals = this;
		}

		protected bool RunningValidateAll { get; private set; }

		public override void ValidateAll()
		{
			RunningValidateAll = true;
			base.ValidateAll();
			ValidateCondtionsInfo();
			RunningValidateAll = false;
		}

		public void ValidateCondtionsInfo()
		{
			ZValidationInternals.Validate(Parent.ConditionsInfo, GetConditionsValidationInvoker());
		}

		RunValidationInvoker GetConditionsValidationInvoker()
		{
			return delegate
			{
				CheckConditions();
			};
		}

		public virtual void CheckConditions()
		{
			// sub class
		}

		public virtual void CheckCommodityStatus()
		{
			// sub class
		}

		protected new ComplianceCommodityDetail Parent;
		readonly IValidationInternals ZValidationInternals;
	}
}
