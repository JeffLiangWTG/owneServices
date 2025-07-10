namespace Enterprise.Customs.CH.Business;

public class BillContainerCollection : Customs.Business.BaseBillContainerCollection
{
	public BillContainerCollection(Bill bill)
		: base(bill)
	{
	}

	public new CusContainer this[int index] => (CusContainer)base[index];

	public new CusContainer AddNew() => (CusContainer)base.AddNew();
}
