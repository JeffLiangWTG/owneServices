using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Integration
{
	public interface IChargeApplicableForCopy
	{
		ZBool IsApplicable(ICharge charge);
	}
}
