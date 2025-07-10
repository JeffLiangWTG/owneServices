using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.Declaration;

public partial class ImportJobDeclarationValidation
{
	protected override void CheckJE_RegionOfDestination()
	{
		base.CheckJE_RegionOfDestination();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_RegionOfDestinationInfo);
	}
}
