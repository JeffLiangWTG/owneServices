namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLDetailCargoLineCollection : BaseCusSeaManOBLDetailCollection
	{
		public CusSeaManOBLDetailCargoLineCollection(CusSeaManOBLHeaderCargoLine parent)
			: base(parent)
		{
		}

		public new CusSeaManOBLDetailCargoLine this[int index]
		{
			get { return (CusSeaManOBLDetailCargoLine)Elements[index]; }
		}

		public new CusSeaManOBLDetailCargoLine AddNew()
		{
			return (CusSeaManOBLDetailCargoLine)base.AddNew();
		}
	}
}
