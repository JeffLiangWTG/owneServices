using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureHeaderContainer : EU.NCTS.Business.NctsDepartureHeaderContainer, Integration.Customs.IT.INctsDepartureHeaderContainer
{
	public NctsDepartureHeaderContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public ZString EffectiveContainerNumber
		=> BC_Mode == Core.Constants.ContainerModes.NonContainerised ? ZString.Empty : BC_ContainerNum;

	protected override ZString HumanReadableNameCore => Res.GetString("AE92D4C7-4033-489C-B654-C7DCA04CFB74", "Container");

	protected override CusInBondContainerValidation GetNewPhase5Validation() => new NctsDepartureHeaderContainerPhase5Validation(this);
}
