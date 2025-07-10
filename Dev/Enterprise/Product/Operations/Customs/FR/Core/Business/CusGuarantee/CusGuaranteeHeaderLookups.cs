namespace Enterprise.Customs.FR.Business
{
	public class CusGuaranteeHeaderLookups : EU.Business.CusGuaranteeHeaderLookups
	{
		public CusGuaranteeHeaderLookups(CusGuaranteeHeader parent)
			: base(parent)
		{
		}
		public new CusGuaranteeHeader Parent => (CusGuaranteeHeader)base.Parent;
	}
}
