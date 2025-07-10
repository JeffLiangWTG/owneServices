namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsArrivalHeaderContainerCollection : EU.NCTS.Business.NctsArrivalHeaderContainerCollection
{
	public NctsArrivalHeaderContainerCollection(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	public new NctsArrivalHeaderContainer AddNew() => (NctsArrivalHeaderContainer)base.AddNew();

	public new NctsArrivalHeaderContainer this[int index] => (NctsArrivalHeaderContainer)base[index];

	protected override bool AllowNewCore => false;
}
