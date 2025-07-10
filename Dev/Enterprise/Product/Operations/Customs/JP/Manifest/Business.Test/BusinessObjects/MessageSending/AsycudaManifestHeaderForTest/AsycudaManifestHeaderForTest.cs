using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderForTest : AsycudaManifestHeader
	{
		public AsycudaManifestHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool CreateMessageErrorForTest { get; set; }

		public bool CreateErrorForTest { get; set; }

		public bool CreateWarningForTest { get; set; }

		protected override void RunPreSaveValidationCore() => Validation.ValidateAll();

		public new AsycudaManifestHeaderValidationForTest Validation => GetNewValidation();

		protected new AsycudaManifestHeaderValidationForTest GetNewValidation() => new (this);

		public new AsycudaBillCollectionForTest Bills => (AsycudaBillCollectionForTest)base.Bills;

		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollectionForTest(this);

		protected override Type GetBillTypeCore() => typeof(AsycudaBillForTest);
	}
}
