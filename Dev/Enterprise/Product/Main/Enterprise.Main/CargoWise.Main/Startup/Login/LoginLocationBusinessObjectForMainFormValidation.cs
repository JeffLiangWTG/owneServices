using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Startup
{
	public class LoginLocationBusinessObjectForMainFormValidation : LoginLocationBusinessObjectValidation
	{
		public LoginLocationBusinessObjectForMainFormValidation(LoginLocationBusinessObjectForMainForm parent)
			: base(parent)
		{ }

		protected override void CheckCompanyCode()
		{
			base.CheckCompanyCode();
			MandatoryValidation.CheckEntered(Parent.CompanyCodeInfo);
		}

		protected override void CheckBranchCode()
		{
			base.CheckBranchCode();
			MandatoryValidation.CheckEntered(Parent.BranchCodeInfo);
		}

		protected override void CheckDepartmentCode()
		{
			base.CheckDepartmentCode();
			MandatoryValidation.CheckEntered(Parent.DepartmentCodeInfo);
		}
	}
}
