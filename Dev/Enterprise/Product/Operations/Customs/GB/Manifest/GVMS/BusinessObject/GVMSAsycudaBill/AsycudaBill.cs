using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.GVMS
{
	[DependentBusinessObject(typeof(AsycudaManifestHeader), "Bills")]

	public class AsycudaBill : ASYCUDA.Business.AsycudaBill
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);
	}
}
