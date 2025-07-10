namespace Enterprise.Customs.ES.NCTS.Business;

public class NctsBillCollection : EU.NCTS.Business.NctsBillCollection<NctsBill>
{
	public NctsBillCollection(NctsHeader nctsHeader) : base(nctsHeader)
	{
		header = nctsHeader;
	}
	public readonly NctsHeader header;

	protected override bool AllowNew => base.AllowNew && (!header.ArrivalMovementHeader?.IsUnloadingRemarksReadOnlySpain ?? true);
}
