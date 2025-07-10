using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public partial class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.CLManifest.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string AMA_IsTramp = "AMA_IsTramp";
			public const string AMA_TransshipmentType = "AMA_TransshipmentType";
			public const int AMA_TransshipmentTypeMaxLength = 3;
		}

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;
		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Chile;
		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);
		protected override bool RegistrationDetails_ReadOnly => AMA_TransportMode == Core.Constants.TransportModes.Sea && Bills.Cast<AsycudaBill>().Any(b => b.ABL_BillStatus == CustomsStatusList.Codes.ACP);
		protected override ASYCUDA.Business.MessageChooser GetNewMessageChooserCore(IEnumerable<ASYCUDA.Business.ISelectionItem> items, string messageSubType, bool showStatus)
			=> (messageSubType != ZString.Empty)
				? new CLMessageChooser(this, items, messageSubType)
				: base.GetNewMessageChooserCore(items, messageSubType, showStatus);
		protected override bool IsAMA_MasterBillReadOnly => AMA_TransportMode == Core.Constants.TransportModes.Sea && Bills.Cast<AsycudaBill>().Any(b => b.ABL_BillStatus == CustomsStatusList.Codes.ACP);
		public new AsycudaArrivalHeaderCollection ArrivalHeaders => (AsycudaArrivalHeaderCollection)base.ArrivalHeaders;
		protected override ASYCUDA.Business.IAsycudaArrivalHeaderCollection<ASYCUDA.Business.AsycudaArrivalHeader> CreateNewAsycudaArrivalHeaderCollection() => new AsycudaArrivalHeaderCollection(this);
		protected override Type GetArrivalHeaderTypeCore() => typeof(AsycudaArrivalHeader);

		#region AMA_IsTramp

		[ResourceStringData("AsycudaManifestHeader.AMA_IsTramp", Caption = "Is Tramp")]
		public ZBool AMA_IsTramp
		{
			get => this.GetSystemDefinedValue<ZBool>(Customs.Business.GenAddOnHelper.AMA_IsTramp);
			set
			{
				var oldValue = AMA_IsTramp;
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.AMA_IsTramp, value);
				AMA_IsTrampInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_IsTrampInfo => GetZPropertyInfo(Schema.AMA_IsTramp);

		#endregion

		#region AMA_TSS_Type

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TSS_Types))]
		[ResourceStringData("AsycudaManifestHeader.AMA_TransshipmentType", Caption = "Transshipment Type")]
		public ZString AMA_TransshipmentType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.AMA_TransshipmentType);
			set
			{
				var oldValue = AMA_TransshipmentType;
				CheckMaximumLength(AMA_TransshipmentTypeInfo, value);

				this.SetSystemDefinedValue(Schema.AMA_TransshipmentType, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAMA_TransshipmentType();
				}

				AMA_TransshipmentTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AMA_TransshipmentTypeInfo => GetZPropertyInfo(Schema.AMA_TransshipmentType);

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = CLManifestTypes.Codes.MAN;
		}
#endif

		public override void OnSaving()
		{
			base.OnSaving();

			if (this.IsAir)
			{
				if (this.ArrivalHeaders.Count == 0)
				{
					ArrivalHeaders.AddNew();
				}
			}
		}
	}
}
