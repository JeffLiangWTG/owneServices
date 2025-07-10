namespace Enterprise.Customs.CH.Business;

public class ActiveCusEntryHeaderCollection : Customs.Business.ActiveCusEntryHeaderCollection
{
	public ActiveCusEntryHeaderCollection(JobDeclaration declaration)
		: base(declaration)
	{
	}

	public new CusEntryHeader this[int index] => (CusEntryHeader)base[index];

	public new CusEntryHeader AddNew() => (CusEntryHeader)base.AddNew();
}
