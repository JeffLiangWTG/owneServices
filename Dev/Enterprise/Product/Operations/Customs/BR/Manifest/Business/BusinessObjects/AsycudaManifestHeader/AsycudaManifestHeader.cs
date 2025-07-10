using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.BRManifest.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string CustomsOwnNumber = "CustomsOwnNumber";
		}

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;
		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Brazil;
		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);
		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);
		protected override bool RegistrationDetails_ReadOnly => !IsMercante;
		protected override bool AMA_MessageStatus_ReadOnly => !IsMercante;
		protected override ZBool IsDeconsolidatorEnabledCore => IsMercante;

		public AsycudaBill BRMasterBill => (AsycudaBill)MasterBill;

		#region CustomsOwnNumber
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString CustomsOwnNumber
		{
			get { return BRMasterBill?.CustomsOwnNumber ?? ZString.Empty; }
			set
			{
				var oldValue = CustomsOwnNumber;
				var masterBill = BRMasterBill;
				if (masterBill != null)
				{
					masterBill.CustomsOwnNumber = value;
				}
				CustomsOwnNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CustomsOwnNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsOwnNumber); }
		}

		#endregion

		public bool IsMercante => AMA_ManifestType == BRManifestTypes.Codes.MER;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = BRManifestTypes.Codes.MUCR;
		}
#endif

	}
}

