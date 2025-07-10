
using System.Linq;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaPackValidation : ASYCUDA.Business.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent) : base(parent)
		{
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIsHazardous();
		}

		public void ValidateIsHazardous()
		{
			ValidateCalculatedProperty(Parent.IsHazardousInfo);
		}

		protected void CheckIsHazardous()
		{
			if (Parent.UNDGs.Any(x => !x.DI_IMOClass.IsEmpty || !x.DI_DG_NKSubs.IsEmpty) && !Parent.IsHazardous)
			{
				Parent.IsHazardousInfo.AddMessageError(Res.GetString("E048B931-4BD9-4751-A160-E4105E562184", "You have not specified that the item is hazardous."));
			}
		}
	}
}
