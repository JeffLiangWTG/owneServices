namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsArrivalCargoDescCollection : EU.NCTS.Business.NctsArrivalCargoDescCollection<NctsArrivalCargoDesc>
{
	public NctsArrivalCargoDescCollection(NctsBill nctsBill) : base(nctsBill)
	{
		this.nctsBill = nctsBill;
	}
	readonly NctsBill nctsBill;

	protected override bool AllowNew
	{
		get
		{
			var arrivalMovement = nctsBill.Header.ArrivalMovementHeader;
			return base.AllowNew && !(arrivalMovement.UnloadingDifferenceDataReadOnly || arrivalMovement.IsUnloadingRemarksReadOnlySpain);
		}
	}
}
