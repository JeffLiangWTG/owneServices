using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class BMNCNScheduleValidation : AutoBMNCNScheduleValidation
	{
		public BMNCNScheduleValidation(AutoBMNCNSchedule parent)
			: base(parent)
		{
		}

		protected override void CheckBNC_GB_Branch()
		{
			base.CheckBNC_GB_Branch();

			if (Parent.BNC_IsScaled && IsForRootShape())
			{
				MandatoryValidation.CheckEntered(Parent.BNC_GB_BranchInfo);
			}
		}

		protected override void CheckBNC_GE_Department()
		{
			base.CheckBNC_GE_Department();

			if (Parent.BNC_IsScaled && IsForRootShape())
			{
				MandatoryValidation.CheckEntered(Parent.BNC_GE_DepartmentInfo);
			}
		}

		bool IsForRootShape()
		{
			return ((BMNCNSchedule)Parent).Shape.BNS_BNS_RootShape.IsEmpty;
		}

		public bool IsBranchAndDepartmentValid => !Parent.BNC_GB_BranchInfo.HasErrors() && !Parent.BNC_GE_DepartmentInfo.HasErrors();
	}
}
