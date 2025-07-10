//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccApportionmentTemplateLinesValidation
//
//    This class should be used for overriding validation in AutoAccApportionmentTemplateLinesValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	using Enterprise.MasterFiles.Business;

	public class AccApportionmentTemplateLinesValidation : AutoAccApportionmentTemplateLinesValidation
	{
		public AccApportionmentTemplateLinesValidation(AutoAccApportionmentTemplateLines parent) : base(parent)
		{
		}

		protected override void CheckY0_Percentage()
		{
			base.CheckY0_Percentage();
			//Parent.Y0_PercentageInfo.ClearAllNotifications();
			if ((Parent.Y0_Percentage <= 0.0M) || (Parent.Y0_Percentage > 100.0M))
			{
				Parent.Y0_PercentageInfo.AddError(Res.GetString("e3ef10f7-dfe7-4e23-b68a-53659f073184", "Please enter a valid Percentage"));
			}
		}

		protected override void CheckY0_GE()
		{
			base.CheckY0_GE();
			GlbBranchCombinationValidation.CheckBranchDepartmentCombination(Parent.Y0_GEInfo, Parent.Branch, Parent.Department);
		}
	}
}