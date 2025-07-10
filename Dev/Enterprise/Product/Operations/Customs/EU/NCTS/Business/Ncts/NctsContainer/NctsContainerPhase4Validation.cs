using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsContainerPhase4Validation : NctsContainerValidation
	{
		public NctsContainerPhase4Validation(NctsContainer parent) : base(parent)
		{
		}

		protected override void CheckBC_ContainerNumCore_Incident(NctsContainer nctsContainer, EnRouteIncident incident)
		{
			nctsContainer.CheckContainerNumberIsUnique(incident.IncidentContainers, ZString.Empty);
		}

		protected override void CheckBC_ModeCore_Incident(NctsContainer nctsContainer, EnRouteIncident incident)
		{
			if (!nctsContainer.BC_ContainerNum.IsEmpty || !nctsContainer.BC_Seal1.IsEmpty || !nctsContainer.BC_Seal2.IsEmpty)
			{
				MandatoryValidation.CheckEntered(nctsContainer.BC_ModeInfo);
			}
		}

		protected override void CheckBC_Seal1Core_Incident(NctsContainer nctsContainer, EnRouteIncident incident)
		{
			if (HasDuplicateSealNumbersOnIncident(nctsContainer.BC_Seal1, incident))
			{
				nctsContainer.BC_Seal1Info.AddWarning(Res.GetString("E05B6341-8EED-4C9C-9CFC-6601470C8621", "Duplicate Seal 1 Number entered."));
			}
		}

		protected override void CheckBC_Seal2Core_Incident(NctsContainer nctsContainer, EnRouteIncident incident)
		{
			if (HasDuplicateSealNumbersOnIncident(nctsContainer.BC_Seal2, incident))
			{
				nctsContainer.BC_Seal2Info.AddWarning(Res.GetString("B7D0839E-BB79-4022-9B54-05F723094EBE", "Duplicate Seal 2 Number entered."));
			}
		}
	}
}
