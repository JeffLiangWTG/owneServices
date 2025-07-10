using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CO.Manifest.Business
{
	[UserDefinedValues]
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
		, Integration.Customs.ASYCUDA.COManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new class Schema : ASYCUDA.Business.AsycudaPack.Schema
		{
			public const string ContactPK = "ContactPK";
			public const string IsHazardous = "IsHazardous";
		}

		[List(nameof(UNDGs) + "." + nameof(UNDGDataItemCollection.Contacts))]
		public ZGuid ContactPK
		{
			get { return ContactManager.Value; }
			set { ContactManager.Value = value; }
		}

		[ResourceStringData("AsycudaPack.IsHazardous", Caption = "Is Hazardous")]
		public ZBool IsHazardous
		{
			get
			{
				return this.GetSystemDefinedValue<ZBool>(Schema.IsHazardous);
			}
			set
			{
				this.SetSystemDefinedValue(Schema.IsHazardous, value);
				IsHazardousInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsHazardousInfo => GetZPropertyInfo(Schema.IsHazardous);

		public MultipleItemManagerGUID<OrgContact> ContactManager => contactManager ?? (contactManager = UNDGs.UNDGContactManager);
		MultipleItemManagerGUID<OrgContact> contactManager;

		[ChildEditable(true)]
		public override MasterFiles.Business.UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		protected override bool IsPackUQNeedToConvertCore => false;

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			return Bill.GetWarningBeforeBeingDeleted();
		}

		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;

		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);
	}
}
