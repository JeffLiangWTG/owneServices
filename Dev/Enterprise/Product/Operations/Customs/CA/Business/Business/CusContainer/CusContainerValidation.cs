
namespace Enterprise.Customs.CA.Business
{
	public class CusContainerValidation : Customs.Business.CusContainerValidation
	{
		public CusContainerValidation(CusContainer parent)
			: base(parent)
		{
		}

		public new CusContainer Parent
		{
			get { return (CusContainer)base.Parent; }
		}

		protected override void CheckContainerNoHasPackages()
		{
		}
	}
}
