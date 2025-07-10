using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public sealed class PGARequirementValidation
	{
		public PGARequirementValidation(PGARequirement parent)
		{
			requirement = parent;
		}

		readonly PGARequirement requirement;

		public void ValidatePGARequired()
		{
			requirement.ClearAllNotifications();

			if (requirement.IsEffective && requirement.Indicator == YesNoList.Codes.Yes && !IsPGARequired)
			{
				var warning = Res.GetString("6add2d74-fa44-454f-83e5-371ea86f94fc", "The Tariff does not indicate that this PGA reporting is required.");
				requirement.AddRowWarning(warning);
			}
		}

		bool IsPGARequired => CARefTariffDataLoader.DoesTariffHasPGAType(requirement.Factory, requirement.Provider.TariffNo, requirement.AgencyCode, requirement.Provider.TariffEffectiveDate);
	}
}
