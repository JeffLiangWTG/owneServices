using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSCusContainerValidation : EU.EMCS.Business.EMCSCusContainerValidation
	{
		public EMCSCusContainerValidation(EMCSCusContainer parent)
			: base(parent)
		{
		}

		protected override void CheckCO_ContainerNumber_Mandatory()
		{
			if (!Declaration.IsConsolidatedDocument() && Parent.ZG_UnitCode != EMCSTransportUnitCodeList.Codes.FixedTransportInstallations)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CO_ContainerNumberInfo);
			}
		}

		protected new EMCSCusContainer Parent => (EMCSCusContainer)base.Parent;

		protected EMCSJobDeclaration Declaration => Parent.Declaration;
	}
}
