using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaArrivalLineLookups : ManifestBase.AsycudaArrivalLineLookups
	{
		public AsycudaArrivalLineLookups(AsycudaArrivalLine parent) : base(parent)
		{
		}

		public new AsycudaArrivalLine Parent => (AsycudaArrivalLine)base.Parent;

		public CodeDescriptionPairList WeightUQList => Factory.GetWeightUQList();

		public IAsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => Parent.Bill?.Packs ?? new AsycudaPackCollection<AsycudaPack, AsycudaBill>(Factory.GetNull<AsycudaBill>());
	}
}
