using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module
{
	public class SubAccountModuleFilterValidation : ModuleFilterValidation
	{
		public SubAccountModuleFilterValidation(SubAccountFilter parent)
			: base(parent)
		{
			Parent = parent;
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}

		public override void ValidateAll()
		{
			ValidateSubAccountType();
			ValidateSubAccount();
		}

		public void ValidateSubAccountType()
		{
			ValidateCalculatedProperty(Parent.SubAccountTypeInfo);
		}

		protected void CheckSubAccountType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.SubAccountTypeInfo);
		}

		public void ValidateSubAccount()
		{
			ValidateCalculatedProperty(Parent.SubAccountInfo);
		}

		protected void CheckSubAccount()
		{
			ListValidation.ErrorIfInvalidPK(Parent.SubAccountInfo);
		}

		protected readonly SubAccountFilter Parent;
	}
}
