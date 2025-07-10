using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaArrivalLineValidation : ManifestBase.AsycudaArrivalLineValidation
	{
		public AsycudaArrivalLineValidation(AsycudaArrivalLine parent) : base(parent)
		{
		}

		public new AsycudaArrivalLine Parent => (AsycudaArrivalLine)base.Parent;

		protected override void CheckATL_WeightUQ()
		{
			base.CheckATL_WeightUQ();

			ListValidation.MessageErrorIfInvalidCode(Parent.ATL_WeightUQInfo);
			if (!Parent.ATL_Weight.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.ATL_WeightUQInfo);
			}
		}
	}
}
