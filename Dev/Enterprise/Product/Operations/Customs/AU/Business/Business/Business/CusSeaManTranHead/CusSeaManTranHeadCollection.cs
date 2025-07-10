using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManTranHeadCollection : Customs.Business.CusSeaManTranHeadCollection
	{
		public CusSeaManTranHeadCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new CusSeaManTranHead AddNew()
		{
			return (CusSeaManTranHead)base.AddNew();
		}

		public new CusSeaManTranHead this[int index]
		{
			get { return (CusSeaManTranHead)base[index]; }
		}
	}
}
