using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaPackLookups : ASYCUDA.Business.AsycudaPackLookups
	{
		public AsycudaPackLookups(ASYCUDA.Business.AsycudaPack parent) : base(parent)
		{
		}

		public new AsycudaPack Parent => (AsycudaPack)base.Parent;

		public override CodeDescriptionPairList PackUQList
		{
			get
			{
				var isSea = Parent.Bill.IsSea;
				return Factory.GetCachedValue("PackUQList" + isSea, () =>
				{
					var result = new CodeDescriptionPairList(Factory.GetPackageTypeList());
					if (isSea)
					{
						result.AddRange(new SeaPackageTypeList());
						result.SortByDescription();
					}
					return result;
				});
			}
		}
	}
}
