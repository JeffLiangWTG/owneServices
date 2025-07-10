using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class GroupCompanyChargeValidation : ZValidation
	{
		public GroupCompanyChargeValidation(GroupCompanyCharge parent)
			: base(parent)
		{
			Parent = parent;
		}

		GroupCompanyCharge Parent { get; }

		public override Type AutoValidationType => typeof(GroupCompanyCharge);

		public override void ValidateAll()
		{
			ValidateMappedChargeCode();
		}

		#region MappedChargeCode

		public void ValidateMappedChargeCode()
		{
			((IValidationInternals)this).Validate(Parent.CostCompanyChargeCodePKInfo, GetCheckMappedChargeCodeValidationInvoker());
		}

		RunValidationInvoker GetCheckMappedChargeCodeValidationInvoker()
		{
			return CheckMappedChargeCode;
		}

		protected virtual void CheckMappedChargeCode()
		{
			if (Parent.CostCompanyChargeCodePK.IsEmpty)
			{
				Parent.CostCompanyChargeCodePKInfo.AddError(Res.GetString("d6092532-13e4-4e0a-be67-7f24c00f7da1", "No Charge Code could be mapped."));
			}
			else if (Parent.CostCompanyChargeCodePK == Parent.JR_AC)
			{
				Parent.CostCompanyChargeCodePKInfo.AddError(Res.GetString("a2e68d98-3226-4797-8f08-30198de600e9", "Charge Code {0} in Company {1} could not be mapped to any Global Charge Code.", Parent.GroupCompanySellCharge.ChargeCode.AC_Code, Parent.GroupCompanySellCharge.Company.GC_Code));
			}
			else if (Parent.CostCompanyChargeCode.IsGlobal)
			{
				Parent.CostCompanyChargeCodePKInfo.AddError(Res.GetString("e0d4a376-642e-4890-895e-926e557230e3", "No mapping was found for Global Charge Code {0} to any Local Charge Codes in this Company.", Parent.CostCompanyChargeCode.AC_Code));
			}
		}

		#endregion
	}
}