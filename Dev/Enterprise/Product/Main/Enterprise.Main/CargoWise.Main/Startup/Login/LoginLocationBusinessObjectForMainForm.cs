using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Startup
{
	public class LoginLocationBusinessObjectForMainForm : LoginLocationBusinessObject
	{
		public LoginLocationBusinessObjectForMainForm(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public override ZString CompanyCode
		{
			get { return base.CompanyCode; }
			set
			{
				if (base.CompanyCode != value)
				{
					base.CompanyCode = value;
					if (Branches.Count == 1)
					{
						BranchCode = Branches[0].GB_Code;
					}
				}
			}
		}

		protected override void SetDefaultValues()
		{
			SetDefaultCompanyAndBranch();
			SetDefaultDepartment();
			base.SetDefaultValues();
		}

		void SetDefaultCompanyAndBranch()
		{
			if (Env.CurrentCompany != null)
			{
				CompanyCode = Env.CurrentCompany.Code;
				BranchCode = Env.CurrentBranch.Code;
			}
			else if (!string.IsNullOrEmpty(LoginDirector.Instance.ForceBranch))
			{
				SetBranch(Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, LoginDirector.Instance.ForceBranch)));
			}
			else if (LoginDirector.Instance.AuthenticatedUser.LoginValidated)
			{
				if (!SetBranch(((GlbStaff)LoginDirector.Instance.AuthenticatedUser.User).LastLogonBranch))
				{
					SetBranch(((GlbStaff)LoginDirector.Instance.AuthenticatedUser.User).HomeBranch);
				}
			}

			if (CompanyCode.IsEmpty && Companies.Count > 0)
			{
				CompanyCode = Companies[0].GC_Code;
			}

			if (BranchCode.IsEmpty && Branches.Count > 0)
			{
				BranchCode = Branches[0].GB_Code;
			}
		}

		bool SetBranch(GlbBranch branch)
		{
			if (branch != null)
			{
				CompanyCode = branch.Company.GC_Code;
				BranchCode = branch.GB_Code;
				return true;
			}
			else
			{
				return false;
			}
		}

		void SetDefaultDepartment()
		{
			if (Env.CurrentDepartment != null)
			{
				DepartmentCode = Env.CurrentDepartment.Code;
			}
			else if (!string.IsNullOrEmpty(LoginDirector.Instance.ForceDepartment))
			{
				DepartmentCode = LoginDirector.Instance.ForceDepartment;
			}
			else if (LoginDirector.Instance.AuthenticatedUser.LoginValidated)
			{
				var department = ((GlbStaff)LoginDirector.Instance.AuthenticatedUser.User).LastLogonDepartment ?? ((GlbStaff)LoginDirector.Instance.AuthenticatedUser.User).HomeDepartment;
				if (department != null)
				{
					DepartmentCode = department.GE_Code;
				}
			}

			if (DepartmentCode == ZString.Empty && Departments.Count > 0)
			{
				DepartmentCode = Departments[0].GE_Code;
			}
		}

		protected override GlbStaff LoginUser
		{
			get
			{
				return (GlbStaff)LoginDirector.Instance.AuthenticatedUser.User;
			}
		}

		protected override bool ShouldApplyBranchSecurityFilter
		{
			get { return DataRegistry.Instance.ShowAvailableBranchesOnly && LoginUser != null && !LoginUser.GS_IsController; }
		}

		protected override bool ShouldApplyDepartmentSecurityFilter => DataRegistry.Instance.ShowAvailableDepartmentsOnly && base.ShouldApplyDepartmentSecurityFilter;

		protected override LoginLocationBusinessObjectValidation GetNewValidation()
		{
			return new LoginLocationBusinessObjectForMainFormValidation(this);
		}
	}
}
