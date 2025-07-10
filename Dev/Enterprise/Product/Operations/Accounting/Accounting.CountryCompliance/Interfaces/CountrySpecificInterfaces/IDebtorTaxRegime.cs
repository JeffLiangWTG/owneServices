using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	public interface IDebtorTaxRegime
	{
		ZString GetOrgCusCode();
		CodeDescriptionPairList GetTaxRegimeIdTypes();
	}
}
