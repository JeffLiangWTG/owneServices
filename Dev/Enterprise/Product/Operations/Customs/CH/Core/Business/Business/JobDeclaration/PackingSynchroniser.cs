using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class PackingSynchroniser : Customs.Business.PackingSynchroniser
{
	public PackingSynchroniser(JobDeclarationSynchroniser parentSynchroniser, BaseJobDeclaration declaration) : base(parentSynchroniser, declaration)
	{
	}

	protected override ZString GetConvertedPackUQ(ZString freightPackType)
	{
		return JobDeclarationHelper.GetTwoCharacterUnitType(freightPackType);
	}
}
