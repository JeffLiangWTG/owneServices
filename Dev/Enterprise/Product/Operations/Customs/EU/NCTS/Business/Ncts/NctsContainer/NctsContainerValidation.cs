using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsContainerValidation : Customs.Business.CusInBondContainerValidation
	{
		public NctsContainerValidation(Customs.Business.AutoCusInBondContainer parent) : base(parent)
		{
		}

		protected new NctsContainer Parent => (NctsContainer)base.Parent;

		protected sealed override void CheckBC_ContainerNum()
		{
			if (ValidationEnabled)
			{
				CheckBC_ContainerNumCore();
			}
		}

		protected virtual void CheckBC_ContainerNumCore()
		{
			base.CheckBC_ContainerNum();
			var parent = Parent;

			if (parent.BC_Mode == Constants.ContainerModes.Containerised)
			{
				ContainerNumberValidation.WarnIfInvalid(parent.BC_ContainerNumInfo);
				if (parent.Parent is EnRouteIncident incident)
				{
					CheckBC_ContainerNumCore_Incident(parent, incident);
				}
			}
		}

		protected virtual void CheckBC_ContainerNumCore_Incident(NctsContainer nctsContainer, EnRouteIncident incident) { }

		protected sealed override void CheckBC_Mode()
		{
			if (ValidationEnabled)
			{
				CheckBC_ModeCore();
			}
		}

		protected virtual void CheckBC_ModeCore()
		{
			base.CheckBC_Mode();

			var parent = Parent;
			if (parent.Parent is EnRouteIncident incident)
			{
				CheckBC_ModeCore_Incident(parent, incident);
			}

			ListValidation.ErrorIfInvalidCode(parent.BC_ModeInfo);
		}

		protected virtual void CheckBC_ModeCore_Incident(NctsContainer nctsContainer, EnRouteIncident incident) { }

		protected sealed override void CheckBC_Seal1()
		{
			if (ValidationEnabled)
			{
				CheckBC_Seal1Core();
			}
		}

		protected virtual void CheckBC_Seal1Core()
		{
			base.CheckBC_Seal1();
			var parent = Parent;
			if (parent.Seals.Count > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BC_Seal1Info);
			}

			if (parent.Parent is EnRouteIncident incident)
			{
				CheckBC_Seal1Core_Incident(parent, incident);
			}
		}

		protected virtual void CheckBC_Seal1Core_Incident(NctsContainer nctsContainer, EnRouteIncident incident) { }

		protected sealed override void CheckBC_Seal2()
		{
			if (ValidationEnabled)
			{
				CheckBC_Seal2Core();
			}
		}

		protected virtual void CheckBC_Seal2Core()
		{
			base.CheckBC_Seal2();
			var parent = Parent;
			if (parent.Seals.Count > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.BC_Seal2Info);
			}

			if (parent.Parent is EnRouteIncident incident)
			{
				CheckBC_Seal2Core_Incident(parent, incident);
			}
		}

		protected virtual void CheckBC_Seal2Core_Incident(NctsContainer nctsContainer, EnRouteIncident incident) { }

		protected virtual bool ValidationEnabled => !CusInBondEventSchema.Constants.Prefix.Equals(Parent.BC_ParentTableCode) ||
			!(Parent.Header is NctsHeader nctsHeader) ||
			!nctsHeader.IsArrivalDetailsReadOnly;

		protected bool HasDuplicateSealNumbersOnIncident(ZString sealNumber, EnRouteIncident incident)
			=> !sealNumber.IsEmpty && (incident?.IncidentContainers.HasDuplicatesForSealNumber(sealNumber) ?? false);
	}
}
