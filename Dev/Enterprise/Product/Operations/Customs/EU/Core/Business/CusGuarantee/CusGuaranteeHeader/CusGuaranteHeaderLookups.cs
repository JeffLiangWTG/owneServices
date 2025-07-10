namespace Enterprise.Customs.EU.Business
{
	public class CusGuaranteeHeaderLookups : Customs.Business.CusGuaranteeHeaderLookups
	{
		public CusGuaranteeHeaderLookups(CusGuaranteeHeader parent)
			: base(parent)
		{
		}

		public new CusGuaranteeHeader Parent => (CusGuaranteeHeader)base.Parent;
	}
}
