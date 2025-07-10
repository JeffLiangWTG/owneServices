namespace Enterprise.Customs.KR.Business
{
	public class CusMiscRequestLineCollection : Customs.Business.CusMiscRequestLineCollection
	{
		public CusMiscRequestLineCollection(CusMiscRequestHeader header) : base(header)
		{
		}

		public new CusMiscRequestLine AddNew() => (CusMiscRequestLine)base.AddNew();
		public new CusMiscRequestLine this[int index] => (CusMiscRequestLine)base[index];

		protected override bool AllowNew => false;
	}
}
