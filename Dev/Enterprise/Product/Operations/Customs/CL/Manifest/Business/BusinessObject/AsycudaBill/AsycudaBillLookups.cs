using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public new WarehouseClientCollection GoodsLocations
		{
			get { return new WarehouseClientCollection(Factory); }
		}

		protected override CodeDescriptionPairList PackageTypeListCore
		{
			get
			{
				var isSea = Parent.IsSea;
				return Factory.GetCachedValue("PackageTypeList" + isSea, () =>
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
