using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public partial class JobDeclarationValidation
	{
		protected override void CheckJE_SpecificCircumstanceIndicator()
		{
			base.CheckJE_SpecificCircumstanceIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_SpecificCircumstanceIndicatorInfo);
		}

		protected override void CheckJE_RegionOfDestination()
		{
			var parent = Parent;
			var targetInfo = parent.ZG_RegionOfDestinationInfo;
			if (parent is JobDeclaration declaration)
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}
	}
}
