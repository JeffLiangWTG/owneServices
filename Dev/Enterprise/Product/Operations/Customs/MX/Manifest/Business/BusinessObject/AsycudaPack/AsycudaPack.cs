using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
		, Integration.Customs.ASYCUDA.MXManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;
		public new AsycudaContainer Container => (AsycudaContainer)base.Container;
		protected new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;
		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		#region Overrided properties

		[ChildEditable(true)]
		public override UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					Factory.Validation.MainGroup.UnregisterValidationType(typeof(UNDGDataItem), typeof(UndgValidation));
					Factory.Validation.MainGroup.RegisterValidationType(typeof(UNDGDataItem), typeof(UndgValidation));
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		#endregion

	}
}
