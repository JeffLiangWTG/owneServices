namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManSlotOrgCollection : Customs.Business.CusSeaManSlotOrgCollection
	{
		public CusSeaManSlotOrgCollection(CusSeaManTranHead parent)
			: base(parent)
		{
		}

		public new CusSeaManSlotOrg this[int index]
		{
			get { return (CusSeaManSlotOrg)base[index]; }
		}

		public new CusSeaManSlotOrg AddNew()
		{
			return (CusSeaManSlotOrg)base.AddNew();
		}
	}
}
