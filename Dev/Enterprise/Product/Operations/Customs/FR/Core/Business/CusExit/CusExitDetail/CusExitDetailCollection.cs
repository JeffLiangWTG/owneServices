namespace Enterprise.Customs.FR.Business
{
	public class CusExitDetailCollection : EU.Business.CusExitDetailCollection
	{
		public CusExitDetailCollection(CusExitControlHeader parent) : base(parent)
		{
		}
		public new CusExitDetail this[int index] => (CusExitDetail)base[index];

		public new CusExitDetail AddNew() => (CusExitDetail)base.AddNew();
	}
}
