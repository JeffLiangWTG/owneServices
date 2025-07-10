namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLDetailCollection : BaseCusSeaManOBLDetailCollection
	{
		public CusSeaManOBLDetailCollection(BaseCusSeaManOBLHeader parent)
			: base(parent)
		{
		}

		public new CusSeaManOBLDetail this[int index]
		{
			get { return (CusSeaManOBLDetail)base[index]; }
		}

		public new CusSeaManOBLDetail AddNew()
		{
			return (CusSeaManOBLDetail)base.AddNew();
		}
	}
}
