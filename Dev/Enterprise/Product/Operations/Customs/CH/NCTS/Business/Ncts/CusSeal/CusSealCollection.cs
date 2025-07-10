namespace Enterprise.Customs.CH.NCTS.Business;

public class CusSealCollection : EU.NCTS.Business.CusSealCollection
{
	public CusSealCollection(NctsArrivalHeaderContainer master) : base(master)
	{
	}

	public CusSealCollection(NctsHeader master) : base(master)
	{ }

	public new CusSeal AddNew() => (CusSeal)base.AddNew();

	public new CusSeal this[int index] => (CusSeal)base[index];
}
