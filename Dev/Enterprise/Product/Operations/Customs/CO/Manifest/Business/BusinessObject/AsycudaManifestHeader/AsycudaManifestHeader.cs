using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public partial class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, Integration.Customs.ASYCUDA.COManifest.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string TravelDocumentType = "TravelDocumentType";
			public const int TravelDocumentTypeMaxLength = 3;
			public const string CargoDisposition = "CargoDisposition";
			public const int CargoDispositionMaxLength = 3;
			public const string Precursors = "Precursors";
			public const int DeliveryModeMaxLength = 1;
			public const string DeliveryMode = "DeliveryMode";
		}

		protected override void OnFactorySaving()
		{
			FactorySaving?.Invoke(this, new EventArgs());
			base.OnFactorySaving();
		}

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;
		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Colombia;
		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);
		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;
		protected override bool AMA_MessageStatus_ReadOnly => false;
		protected override bool RegistrationDetails_ReadOnly => false;
		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source)
			=> new AsycudaManifestHeaderSynchroniser(this, source);

		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				var oldValue = AMA_TransportMode;
				base.AMA_TransportMode = value;
				if (!IsCopying && oldValue != AMA_TransportMode)
				{
					Containers.MarkAsNeedingValidation();
					DeliveryModeInfo.ClearValue();
				}
			}
		}

		#region TravelDocumentType

		[MaxLength(Schema.TravelDocumentTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TravelDocumentTypes))]
		[ResourceStringData("AsycudaManifestHeader.TravelDocumentType", Caption = "Travel Document Type", ShortCaption = "Travel Doc. Type")]
		public ZString TravelDocumentType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TravelDocumentType);
			set
			{
				var oldValue = TravelDocumentType;
				CheckMaximumLength(TravelDocumentTypeInfo, value);
				this.SetSystemDefinedValue(Schema.TravelDocumentType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTravelDocumentType();
				}
				TravelDocumentTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TravelDocumentTypeInfo => GetZPropertyInfo(Schema.TravelDocumentType);

		#endregion

		#region CargoDisposition

		[MaxLength(Schema.CargoDispositionMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CargoDispositionList))]
		[ResourceStringData("AsycudaManifestHeader.CargoDisposition", Caption = "Cargo Disposition")]
		public ZString CargoDisposition
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CargoDisposition);
			set
			{
				var oldValue = CargoDisposition;
				CheckMaximumLength(CargoDispositionInfo, value);
				this.SetSystemDefinedValue(Schema.CargoDisposition, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCargoDisposition();
				}
				CargoDispositionInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CargoDispositionInfo => GetZPropertyInfo(Schema.CargoDisposition);

		#endregion

		#region Multimodal

		[ResourceStringData("AsycudaManifestHeader.Multimodal", Caption = "Multimodal")]
		public ZBool Multimodal
		{
			get => MasterBill.Multimodal;
			set
			{
				MasterBill.Multimodal = value;
				MultimodalInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo MultimodalInfo => GetZPropertyInfo(nameof(Multimodal));

		#endregion

		#region Precursors

		[ResourceStringData("AsycudaManifestHeader.Precursors", Caption = "Precursors")]
		public ZBool Precursors
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.Precursors);
			set
			{
				var oldValue = Precursors;
				this.SetSystemDefinedValue(Schema.Precursors, value);
				PrecursorsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo PrecursorsInfo => GetZPropertyInfo(Schema.Precursors);

		#endregion

		#region CarriersLiability

		[ResourceStringData("AsycudaManifestHeader.CarriersLiability", Caption = "Carrier's Liability")]
		public ZBool CarriersLiability
		{
			get => MasterBill.CarriersLiability;
			set
			{
				MasterBill.CarriersLiability = value;
				CarriersLiabilityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CarriersLiabilityInfo => GetZPropertyInfo(nameof(CarriersLiability));

		#endregion

		#region DeliveryMode

		[MaxLength(Schema.DeliveryModeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.DeliveryModeList))]
		[ResourceStringData("AsycudaManifestHeader.DeliveryMode", Caption = "Delivery Mode")]
		public ZString DeliveryMode
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.DeliveryMode);
			set
			{
				var oldValue = DeliveryMode;
				CheckMaximumLength(DeliveryModeInfo, value);
				this.SetSystemDefinedValue(Schema.DeliveryMode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDeliveryMode();
				}
				DeliveryModeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DeliveryModeInfo => GetZPropertyInfo(Schema.DeliveryMode);

		#endregion

		internal event EventHandler FactorySaving;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = COManifestTypes.Codes.MAN;
		}
#endif

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateNumberPropertyIfRequired(AMA_ManifestNumberInfo, GetManifestNumber);
		}

		ZString GetManifestNumber(BusinessObjectFactory factory)
		{
			return Env.NumberFountains.GetOutgoingCOCustomsMessageNumber().GetNextFormatted(factory);
		}

		#region Consume Document ID Mutex

		ZGlobalMutex ConsumeDocumentIDMutex => fConsumeDocumentIDMutex ?? (fConsumeDocumentIDMutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, PK.ToString()));
		ZGlobalMutex fConsumeDocumentIDMutex;

		public bool LockConsumeDocumentIDMutex() => ConsumeDocumentIDMutex.Lock();

		public void UnlockConsumeDocumentIDMutex()
		{
			if (fConsumeDocumentIDMutex != null && fConsumeDocumentIDMutex.IsLocked && fConsumeDocumentIDMutex.HasLock)
			{
				ConsumeDocumentIDMutex.Unlock();
			}
		}

		public string GetConsumeDocumentIDMutexInfo() => ConsumeDocumentIDMutex.GetMutexLockByInfo();

		#endregion

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				UnlockConsumeDocumentIDMutex();
			}
		}

		public override void Delete()
		{
			base.Delete();
			UnlockConsumeDocumentIDMutex();
		}

		public List<CusTransactionNumber> GetValidDocumentIDs(int count)
		{
			var query = new ZDBOnlyQuery(typeof(CusTransactionNumber))
			{
				MaximumRows = count,
				OrderBy = CusTransactionNumberSchema.Constants.TN_TransactionReference + OrderByClause.Ascending
			};
			query.AddToFilter(CusTransactionNumberSchema.TN_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(CusTransactionNumberSchema.TN_Type, CusTransactionNumberTypeList.Codes.ColombiaManifest);
			query.AddToFilter(CusTransactionNumberSchema.TN_IsUsed, ZBool.False);
			return Factory.Load<CusTransactionNumber>(query).ToList();
		}
	}
}
