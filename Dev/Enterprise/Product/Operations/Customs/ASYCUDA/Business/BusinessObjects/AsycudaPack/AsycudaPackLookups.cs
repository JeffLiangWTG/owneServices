using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackLookups : ManifestBase.AsycudaPackLookups
	{
		public AsycudaPackLookups(AsycudaPack parent)
			: base(parent)
		{
		}

		public RefCurrencyCollection LinePriceCurrencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;
	}
}
