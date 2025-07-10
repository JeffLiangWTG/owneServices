using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using AsycudaContainer = Enterprise.Customs.ASYCUDA.Business.AsycudaContainer;

namespace Enterprise.Customs.GB.GVMS
{
	[SystemDefinedValues]
	[UniversalDataContext(DataContextType.GvmsAsycudaManifestHeader)]
	public class AsycudaManifestHeader :
		ASYCUDA.Business.AsycudaManifestHeader,
		Integration.Customs.GB.GBGVMS.IAsycudaManifestHeader,
		Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoAsycudaManifestHeader.Schema
		{
			public const string RouteId = "RouteId";
			public const string IsUnaccompanied = "IsUnaccompanied";
			public const string EmptyVehicle = "EmptyVehicle";
			public const string InspectionLocations = "InspectionLocations";
			public const string InspectionRequired = "InspectionRequired";
			public const string HaulierType = "HaulierType";
		}

		public new ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills => (ASYCUDA.Business.IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>)base.Bills;
		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);
		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;
		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);
		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);
		protected override Type GetBillTypeCore() => typeof(AsycudaBill);
		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.UnitedKingdom;
		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);
		public override bool HasBillsAndPacks => false;
		protected override bool NeedPersonsTabCore => false;
		protected override string HumanReadableNamePrefixCore => Constants.MessageSubTypePreFixes.GVMS;

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes => new Dictionary<ZString, Type>()
		{
			{ CusCodeDataTypeList.Codes.GVI, typeof(GvmsInspectionAtLocationCusCodeData) }
		};

		[ChildEditable(true)]
		public GvmsItemReferenceCollection<GvmsCustomsReference> GvmsCustomsReferenceCollection
		{
			get
			{
				if (gvmsCustomsReferenceCollection == null)
				{
					gvmsCustomsReferenceCollection = new GvmsItemReferenceCollection<GvmsCustomsReference>(this, GvmsItemReferencePartitions.CustomsReferenceCodes);
					gvmsCustomsReferenceCollection.Load();
					RegisterEditableChildObject(gvmsCustomsReferenceCollection);
				}
				return gvmsCustomsReferenceCollection;
			}
		}
		GvmsItemReferenceCollection<GvmsCustomsReference> gvmsCustomsReferenceCollection;

		[ChildEditable(true)]
		public GvmsItemReferenceCollection<GvmsTransitReference> GvmsTransitReferenceCollection
		{
			get
			{
				if (gvmsTransitReferenceCollection == null)
				{
					gvmsTransitReferenceCollection = new GvmsItemReferenceCollection<GvmsTransitReference>(this, GvmsItemReferencePartitions.TransitReferenceCodes);
					gvmsTransitReferenceCollection.Load();
					RegisterEditableChildObject(gvmsTransitReferenceCollection);
				}
				return gvmsTransitReferenceCollection;
			}
		}
		GvmsItemReferenceCollection<GvmsTransitReference> gvmsTransitReferenceCollection;

		[ChildEditable(true)]
		public GvmsItemReferenceCollection<GvmsEidrReference> GvmsEidrAndOralReferenceCollection
		{
			get
			{
				if (gvmsEidrReferenceCollection == null)
				{
					gvmsEidrReferenceCollection = new GvmsItemReferenceCollection<GvmsEidrReference>(this, GvmsItemReferencePartitions.EidrReferenceCodes);
					gvmsEidrReferenceCollection.Load();
					RegisterEditableChildObject(gvmsEidrReferenceCollection);
				}
				return gvmsEidrReferenceCollection;
			}
		}
		GvmsItemReferenceCollection<GvmsEidrReference> gvmsEidrReferenceCollection;

		public IEnumerable<GvmsItemReference> CustomsReferences
		{
			get
			{
				return GvmsCustomsReferenceCollection.Cast<GvmsItemReference>();
			}
		}

		public IEnumerable<GvmsItemReference> CustomsTransitReferences
		{
			get
			{
				return GvmsTransitReferenceCollection.Cast<GvmsItemReference>();
			}
		}

		public IEnumerable<GvmsItemReference> CustomsEidrAndOralReferences
		{
			get
			{
				return GvmsEidrAndOralReferenceCollection.Cast<GvmsItemReference>();
			}
		}

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> { { GvmsItemReference.GvmsItemReferenceType, typeof(GvmsItemReference) } };
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CarrierCodeList))]
		public override ZString AMA_CarrierCode
		{
			get => base.AMA_CarrierCode;
			set
			{
				var oldValue = AMA_CarrierCode;
				base.AMA_CarrierCode = value;
				if (oldValue != AMA_CarrierCode)
				{
					CalculateRouteID();
				}
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;
		}
#endif

		public ZString RegistrationNumberWithoutSpaces => RegistrationNumber.Replace(" ", "");

		public override ZString AMA_RL_NKPortOfLoading
		{
			get => base.AMA_RL_NKPortOfLoading;
			set
			{
				var oldValue = AMA_RL_NKPortOfLoading;
				base.AMA_RL_NKPortOfLoading = value;
				if (oldValue != AMA_RL_NKPortOfLoading)
				{
					CalculateRouteID();
				}
			}
		}

		public override ZString AMA_RL_NKPortOfDischarge
		{
			get => base.AMA_RL_NKPortOfDischarge;
			set
			{
				var oldValue = AMA_RL_NKPortOfDischarge;
				base.AMA_RL_NKPortOfDischarge = value;
				if (oldValue != AMA_RL_NKPortOfDischarge)
				{
					CalculateRouteID();
				}
			}
		}

		GVMSRouteCalculator RouteCalculator => routeCalculator ?? (routeCalculator = new GVMSRouteCalculator(this));

		GVMSRouteCalculator routeCalculator;

		ZBool ShouldCalculateRoute() => !AMA_RL_NKPortOfLoading.IsEmpty && !AMA_RL_NKPortOfDischarge.IsEmpty && !AMA_CarrierCode.IsEmpty;

		void CalculateRouteID()
		{
			calculatedRouteIDCached?.InvalidateCache();

			if (ShouldCalculateRoute())
			{
				if (RouteId.IsEmpty)
				{
					RouteId = CalculatedRouteID;
				}
			}
		}

		public ZString CalculatedRouteID => (calculatedRouteIDCached ?? (calculatedRouteIDCached = new RecalculableCachedValue<ZString>(() => RouteCalculator.CalculateRoute()))).Value;

		RecalculableCachedValue<ZString> calculatedRouteIDCached;

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.GB.GVMS.AsycudaManifestHeader.RouteId", Caption = "Route ID")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.GVMSRoutesList))]
		public ZString RouteId
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.RouteId);
			set
			{
				CheckMaximumLength(RouteIdInfo, value);
				var oldValue = RouteId;
				this.SetSystemDefinedValue(Schema.RouteId, value);
				RouteIdInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo RouteIdInfo => GetZPropertyInfo(Schema.RouteId);

		[ResourceStringData("Enterprise.Customs.GB.GVMS.AsycudaManifestHeader.IsUnaccompanied", Caption = "Is Unaccompanied?")]
		public ZBool IsUnaccompanied
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsUnaccompanied);
			set
			{
				var oldValue = IsUnaccompanied;
				this.SetSystemDefinedValue(Schema.IsUnaccompanied, value);
				IsUnaccompaniedInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo IsUnaccompaniedInfo => GetZPropertyInfo(Schema.IsUnaccompanied);

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.GB.GVMS.AsycudaManifestHeader.EmptyVehicle", Caption = "Empty Vehicle?")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.EmptyVehicleList))]
		public ZString EmptyVehicle
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.EmptyVehicle);
			set
			{
				CheckMaximumLength(EmptyVehicleInfo, value);
				var oldValue = EmptyVehicle;
				this.SetSystemDefinedValue(Schema.EmptyVehicle, value);
				EmptyVehicleInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEmptyVehicle();
				}
			}
		}
		public ZPropertyInfo EmptyVehicleInfo => GetZPropertyInfo(Schema.EmptyVehicle);

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.GB.GVMS.AsycudaManifestHeader.HaulierType", Caption = "Haulier Type")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.HaulierTypeList))]
		public ZString HaulierType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.HaulierType);
			set
			{
				CheckMaximumLength(HaulierTypeInfo, value);
				var oldValue = HaulierType;
				this.SetSystemDefinedValue(Schema.HaulierType, value);
				HaulierTypeInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo HaulierTypeInfo => GetZPropertyInfo(Schema.HaulierType);

		[ChildEditable(false)]
		public GvmsInspectionAtLocationCusCodeDataCollection InspectionLocations
		{
			get
			{
				if (inspectionLocations == null && AMA_ManifestType == GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms)
				{
					inspectionLocations = new GvmsInspectionAtLocationCusCodeDataCollection(this);
					inspectionLocations.Load();
					RegisterEditableChildObject(inspectionLocations);
				}
				return inspectionLocations;
			}
		}
		GvmsInspectionAtLocationCusCodeDataCollection inspectionLocations;

		public ZBool InspectionRequired
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.InspectionRequired);
			set
			{
				var oldValue = InspectionRequired;
				this.SetSystemDefinedValue(Schema.InspectionRequired, value);
				InspectionRequiredInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo InspectionRequiredInfo => GetZPropertyInfo(Schema.InspectionRequired);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_ManifestType = GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;
			EmptyVehicle = GVMSEmptyVehicle.Codes.VehicleIsNotEmpty;
			AMA_TransportMode = Core.Constants.TransportModes.Road;
		}
	}
}
