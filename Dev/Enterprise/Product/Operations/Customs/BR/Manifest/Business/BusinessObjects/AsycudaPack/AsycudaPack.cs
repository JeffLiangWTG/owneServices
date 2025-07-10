using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
		, Integration.Customs.ASYCUDA.BRManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaPack.Schema
		{
			public const string BulkType = "BulkType";
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		protected new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;

		public new AsycudaPackLookups Lookups => (AsycudaPackLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackLookups(this);

		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		public new AsycudaContainer Container => (AsycudaContainer)base.Container;

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			return Bill.GetWarningBeforeBeingDeleted();
		}

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(AsycudaPackLookups.BulkTypes))]
		[ResourceStringData("AsycudaPacks.BulkType", Caption = "Bulk Type")]
		public ZString BulkType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.BulkType);
			set
			{
				var oldValue = BulkType;
				CheckMaximumLength(BulkTypeInfo, value);

				this.SetSystemDefinedValue(Schema.BulkType, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateBulkType();
				}

				BulkTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BulkTypeInfo => GetZPropertyInfo(Schema.BulkType);
	}
}
