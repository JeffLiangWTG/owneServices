using System.Linq;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaArrivalLineValidation : ASYCUDA.Business.AsycudaArrivalLineValidation
	{
		public AsycudaArrivalLineValidation(AsycudaArrivalLine parent) : base(parent)
		{
		}

		protected override void CheckATL_APA_AsycudaPack()
		{
			base.CheckATL_APA_AsycudaPack();

			var header = Parent.ArrivalHeader;

			var linesWithSameBillAndPackPK = header.ArrivalDetails.Cast<AsycudaArrivalLine>().Count(x => x.ATL_APA_AsycudaPack == Parent.ATL_APA_AsycudaPack && x.ATL_ABL_AsycudaBill == Parent.ATL_ABL_AsycudaBill);

			if (linesWithSameBillAndPackPK > 1)
			{
				Parent.ATL_APA_AsycudaPackInfo.AddError(ResString.GetMultilingualString("6853EC36-DFB9-4A69-8400-F31D08BE72E4", "Bill plus Pack should be unique under one Arrivals Header"));
			}
		}
	}
}
