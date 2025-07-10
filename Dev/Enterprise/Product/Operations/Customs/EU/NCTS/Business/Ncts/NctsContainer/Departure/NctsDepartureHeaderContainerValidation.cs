using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsDepartureHeaderContainerValidation : CusInBondContainerValidation
	{
		public NctsDepartureHeaderContainerValidation(AutoCusInBondContainer parent) : base(parent)
		{
		}

		public new NctsDepartureHeaderContainer Parent => (NctsDepartureHeaderContainer)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTotalSealCount();
		}

		public void ValidateTotalSealCount() => ValidateCalculatedProperty(Parent.TotalSealCountInfo);

		protected virtual void CheckTotalSealCount()
		{
		}
	}
}
