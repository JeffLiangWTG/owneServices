namespace Enterprise.Customs.CN.Business
{
	public class CusContainerLookups : Customs.Business.CusContainerLookups
	{
		public CusContainerLookups(CusContainer parent)
			: base(parent)
		{
		}

		public CusContainer Container => Parent;

		protected new CusContainer Parent => (CusContainer)base.Parent;
	}
}
