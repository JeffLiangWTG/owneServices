namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortCollection : Customs.Business.CusSeaManArrivalPortCollection
	{
		public CusSeaManArrivalPortCollection(CusSeaManTranHead parent)
			: base(parent)
		{
		}

		public new CusSeaManArrivalPort this[int index]
		{
			get { return (CusSeaManArrivalPort)base[index]; }
		}

		public new CusSeaManArrivalPort AddNew()
		{
			return (CusSeaManArrivalPort)base.AddNew();
		}
	}
}
