using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack, Integration.Customs.ASYCUDA.ARManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;
		protected new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;
		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);
	}
}
