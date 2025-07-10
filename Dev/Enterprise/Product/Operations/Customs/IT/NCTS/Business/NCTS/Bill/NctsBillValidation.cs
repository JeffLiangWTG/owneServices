using CargoWise.EntityFramework;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsBillValidation : EU.NCTS.Business.NctsBillValidation
{
	public NctsBillValidation(NctsBill parent) : base(parent)
	{
	}

	protected override void CheckB0_BillStatus()
	{
		base.CheckB0_BillStatus();
		ListValidation.ErrorIfInvalidCode(Parent.B0_BillStatusInfo);
	}

	protected void CheckInlandTransportModeAtDeparture()
	{
		var parent = Parent;
		if (parent.AreAllDepartureTransportMeansFieldsEmpty || parent.IsTransportDepartureReadOnly
			|| (parent.Header is NctsHeader header && (header.IsInPhase5TransitionPeriod || !header.AllBillsHaveSameDepartureTransportMeans)))
		{
			return;
		}

		parent.InlandTransportModeAtDepartureInfo.AddWarning(ValidationCaptions.NctsBill.AllDepartureTransportMeansAreSame);
	}

	public void ValidateInlandTransportModeAtDeparture()
	{
		ValidateCalculatedProperty(Parent.InlandTransportModeAtDepartureInfo);
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateInlandTransportModeAtDeparture();
	}
}
