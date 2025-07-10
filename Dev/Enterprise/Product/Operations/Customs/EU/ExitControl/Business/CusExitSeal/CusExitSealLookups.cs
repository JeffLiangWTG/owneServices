using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitSealLookups : CusSealLookups
{
	public CusExitSealLookups(AutoCusSeal parent) : base(parent)
	{
	}

	public CodeDescriptionPairList StatusList => StatusListCore;
	protected virtual CodeDescriptionPairList StatusListCore => new();
}
