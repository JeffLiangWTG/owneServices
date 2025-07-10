using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsBillCollection : NctsBillCollection<NctsBill>
{
	public NctsBillCollection(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override int MaxCount => 1999;

	protected override bool AllowNew => !nctsHeader.IsPhase5Arrival;
}

