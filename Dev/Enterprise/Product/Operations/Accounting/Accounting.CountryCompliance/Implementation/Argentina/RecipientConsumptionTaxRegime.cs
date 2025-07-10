using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using static Enterprise.Accounting.CountryCompliance.Implementation.Argentina.Constants;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Argentina
{
	class RecipientConsumptionTaxRegime : IRecipientConsumptionTaxRegime
	{
		ZString[] IRecipientConsumptionTaxRegime.GetOrgCusCodes() => new ZString[]
			{
				OrgCusCodes.IVE,
				OrgCusCodes.IVF,
				OrgCusCodes.IVI,
				OrgCusCodes.IVM,
				OrgCusCodes.IVN,
				OrgCusCodes.IVP,
				OrgCusCodes.IVR,
				OrgCusCodes.IVS,
				OrgCusCodes.IVX,
			};
	}
}
