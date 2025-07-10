using System;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ProposedProcessHeaderLinkValidation : ZValidation
	{
		public ProposedProcessHeaderLinkValidation(ProposedProcessHeaderLink parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly ProposedProcessHeaderLink parent;

		#region ZValidation Overrides

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		public override void ValidateAll()
		{
			ValidateFP_FH_HeaderFrom();
			ValidateFP_FH_HeaderTo();
			ValidateFP_LinkType();
			ValidateFP_TimeDelayFactor();
			ValidateFP_TimeDelayMinutes();
		}

		#endregion

		#region Property Validation

		public void ValidateFP_FH_HeaderFrom()
		{
			ValidateCalculatedProperty(parent.FP_FH_HeaderFromInfo);
		}

		protected void CheckFP_FH_HeaderFrom()
		{
			MandatoryValidation.CheckEntered(parent.FP_FH_HeaderFromInfo);
			ListValidation.ErrorIfInvalidPK(parent.FP_FH_HeaderFromInfo);
		}

		public void ValidateFP_FH_HeaderTo()
		{
			ValidateCalculatedProperty(parent.FP_FH_HeaderToInfo);
		}

		protected void CheckFP_FH_HeaderTo()
		{
			MandatoryValidation.CheckEntered(parent.FP_FH_HeaderToInfo);
			ListValidation.ErrorIfInvalidPK(parent.FP_FH_HeaderToInfo);
		}

		public void ValidateFP_LinkType()
		{
			ValidateCalculatedProperty(parent.FP_LinkTypeInfo);
		}

		protected void CheckFP_LinkType()
		{
			MandatoryValidation.CheckEntered(parent.FP_LinkTypeInfo);
			ListValidation.ErrorIfInvalidCode(parent.FP_LinkTypeInfo);
		}

		public void ValidateFP_TimeDelayFactor()
		{
			ValidateCalculatedProperty(parent.FP_TimeDelayFactorInfo);
		}

		protected void CheckFP_TimeDelayFactor()
		{
		}

		public void ValidateFP_TimeDelayMinutes()
		{
			ValidateCalculatedProperty(parent.FP_TimeDelayMinutesInfo);
		}

		protected void CheckFP_TimeDelayMinutes()
		{
		}

		#endregion
	}
}
