namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusContainerLookups : Customs.Business.CusContainerLookups
	{
		public CusContainerLookups(CusContainer parent)
			: base(parent)
		{
		}

		public CusContainer Container
		{
			get { return Parent; }
		}

		protected new CusContainer Parent
		{
			get { return (CusContainer)base.Parent; }
		}
	}
}
