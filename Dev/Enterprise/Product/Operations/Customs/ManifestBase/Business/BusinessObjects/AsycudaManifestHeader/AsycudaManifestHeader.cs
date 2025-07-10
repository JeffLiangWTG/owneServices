using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	[SingleObjectAroundARow]
	public partial class AsycudaManifestHeader : AutoAsycudaManifestHeader
		, Integration.Customs.ManifestBase.IAsycudaManifestHeader
		, IClusterKeyMaster
		, IAuditParent
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly AsycudaManifestHeaderTypeDecider TypeDecider = new AsycudaManifestHeaderTypeDecider();

		[ChildEditable]
		public IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> Bills
		{
			get
			{
				if (bills == null)
				{
					bills = CreateNewAsycudaBillCollection();
					bills.Load();
					RegisterEditableChildObject(bills);
				}
				return bills;
			}
		}
		IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> bills;

		protected virtual IAsycudaBillCollection<AsycudaBill, AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

		[ChildEditable]
		public IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers
		{
			get
			{
				if (containers == null)
				{
					containers = CreateNewAsycudaContainerCollection();
					containers.Load();
					RegisterEditableChildObject(containers);
				}
				return containers;
			}
		}
		IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> containers;

		[ReadOnly(true)]
		[ResourceStringData("D7AEEABA-061D-42FD-BE03-4B52C0D34429", Caption = "Country/Region", ShortCaption = "Ctry/Rgn.")]
		public override ZString AMA_RN_NKCountry { get => base.AMA_RN_NKCountry; set => base.AMA_RN_NKCountry = value; }

		[RelatedBusinessObject(nameof(Vessel))]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.Vessels))]
		public override ZString AMA_VesselName
		{
			get => base.AMA_VesselName;
			set
			{
				var oldValue = AMA_VesselName;
				base.AMA_VesselName = value;
				if (!IsCopying && !AMA_VesselName.IsEmpty && oldValue != AMA_VesselName)
				{
					DefaultVesselValuesIfPropertyChanged(nameof(AMA_VesselName));
				}
			}
		}

		public RefVessel Vessel => Factory.GetValue(ref refVessel, () => new RefVessel.Loader(Factory).LoadUnique(AMA_VesselName, AMA_LloydsNumber, AMA_RadioCallSign, AMA_RN_NKConveyanceNationality));
		CachedProperty<RefVessel> refVessel;

		public override ZString AMA_LloydsNumber
		{
			get => base.AMA_LloydsNumber;
			set
			{
				var oldValue = AMA_LloydsNumber;
				base.AMA_LloydsNumber = value;
				if (!IsCopying && !AMA_LloydsNumber.IsEmpty && oldValue != AMA_LloydsNumber)
				{
					DefaultVesselValuesIfPropertyChanged(nameof(AMA_LloydsNumber));
				}
			}
		}

		public override ZString AMA_RadioCallSign
		{
			get => base.AMA_RadioCallSign;
			set
			{
				var oldValue = AMA_RadioCallSign;
				base.AMA_RadioCallSign = value;
				if (!IsCopying && !AMA_RadioCallSign.IsEmpty && oldValue != AMA_RadioCallSign)
				{
					DefaultVesselValuesIfPropertyChanged(nameof(AMA_RadioCallSign));
				}
			}
		}

		public override ZString AMA_RN_NKConveyanceNationality
		{
			get => base.AMA_RN_NKConveyanceNationality;
			set
			{
				var oldValue = AMA_RN_NKConveyanceNationality;
				base.AMA_RN_NKConveyanceNationality = value;
				if (!IsCopying && !AMA_RN_NKConveyanceNationality.IsEmpty && oldValue != AMA_RN_NKConveyanceNationality)
				{
					DefaultVesselValuesIfPropertyChanged(nameof(AMA_RN_NKConveyanceNationality));
				}
			}
		}

		void DefaultVesselValuesIfPropertyChanged(string vesselRelatedPropertyChanged)
		{
			if (!IsDefaultVesselValuesSuspended)
			{
				var calledFromAMA_VesselName = false;
				var calledFromAMA_LloydsNumber = false;
				var calledFromAMA_RadioCallSign = false;
				var calledFromAMA_RN_NKConveyanceNationality = false;
				switch (vesselRelatedPropertyChanged)
				{
					case nameof(AMA_VesselName):
						calledFromAMA_VesselName = true;
						break;
					case nameof(AMA_LloydsNumber):
						calledFromAMA_LloydsNumber = true;
						break;
					case nameof(AMA_RadioCallSign):
						calledFromAMA_RadioCallSign = true;
						break;
					case nameof(AMA_RN_NKConveyanceNationality):
						calledFromAMA_RN_NKConveyanceNationality = true;
						break;
				}
				DefaultVesselValues(Vessel, !calledFromAMA_VesselName, !calledFromAMA_LloydsNumber, !calledFromAMA_RadioCallSign, !calledFromAMA_RN_NKConveyanceNationality);
			}
		}

		public void DefaultVesselValues(RefVessel vessel) => DefaultVesselValues(vessel, setAMA_VesselName: true, setAMA_LloydsNumber: true, setAMA_RadioCallSign: true, setAMA_RN_NKConveyanceNationality: true);

		void DefaultVesselValues(RefVessel vessel, bool setAMA_VesselName, bool setAMA_LloydsNumber, bool setAMA_RadioCallSign, bool setAMA_RN_NKConveyanceNationality)
		{
			using (SuspendDefaultVesselValues())
			{
				if (vessel == null)
				{
					ClearVesselValuesWhenVesselWasNotMatched(setAMA_VesselName, setAMA_LloydsNumber, setAMA_RadioCallSign, setAMA_RN_NKConveyanceNationality);
				}
				else
				{
					if (setAMA_VesselName)
					{
						AMA_VesselName = vessel.RV_Name;
					}

					if (setAMA_LloydsNumber)
					{
						AMA_LloydsNumber = vessel.RV_LloydsNumber;
					}

					if (setAMA_RadioCallSign)
					{
						AMA_RadioCallSign = vessel.RV_RadioCallSign;
					}

					if (setAMA_RN_NKConveyanceNationality)
					{
						AMA_RN_NKConveyanceNationality = vessel.RV_RN_NKCountryOfReg;
					}
				}
			}
		}

		protected virtual void ClearVesselValuesWhenVesselWasNotMatched(bool setAMA_VesselName, bool setAMA_LloydsNumber, bool setAMA_RadioCallSign, bool setAMA_RN_NKConveyanceNationality) { }

		int suspendDefaultVesselValues;
		bool IsDefaultVesselValuesSuspended => suspendDefaultVesselValues > 0;

		protected IDisposable SuspendDefaultVesselValues() => new CargoWise.Common.DisposableAction(() => suspendDefaultVesselValues++, () => suspendDefaultVesselValues--);

		protected void ClearVesselFields()
		{
			using (SuspendDefaultVesselValues())
			{
				AMA_VesselName = ZString.Empty;
				AMA_LloydsNumber = ZString.Empty;
				AMA_RadioCallSign = ZString.Empty;
				AMA_RN_NKConveyanceNationality = ZString.Empty;
			}
		}

		protected virtual IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);

		protected virtual ZString GetDefaultCountryCode() => ZString.Empty;

		public Type GetArrivalHeaderType() => GetArrivalHeaderTypeCore();
		protected virtual Type GetArrivalHeaderTypeCore() => typeof(AsycudaArrivalHeader);

		public Type GetBillType() => GetBillTypeCore();
		protected virtual Type GetBillTypeCore() => typeof(AsycudaBill);

		public Type GetEUMemberStateCommunicationType() => GetEUMemberStateCommunicationTypeCore();
		protected virtual Type GetEUMemberStateCommunicationTypeCore() => typeof(EUMemberStateCommunication);

		public Type GetContainerType() => GetContainerTypeCore();
		protected virtual Type GetContainerTypeCore() => typeof(AsycudaContainer);

		public bool IsManyPackedItemRelationship => PackedItemRelationship == AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
		public bool IsNonePackedItemRelationship => PackedItemRelationship == AsycudaPackPackedItemPivotCollection.RelationshipType.None;
		public bool IsOnePackedItemRelationship => PackedItemRelationship == AsycudaPackPackedItemPivotCollection.RelationshipType.One;

		public virtual AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship
		{
			get
			{
#if DEBUG
				if (PackedItemRelationshipOverrideForTesting.HasValue)
				{
					return PackedItemRelationshipOverrideForTesting.Value;
				}
#endif
				return AsycudaPackPackedItemPivotCollection.RelationshipType.None;
			}
		}

#if DEBUG
		public AsycudaPackPackedItemPivotCollection.RelationshipType? PackedItemRelationshipOverrideForTesting;
#endif
		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaManifestHeaderFetchStrategy(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_GB = GlbBranch.CurrentBranch.PK;
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchStrategy.FetchForDelete();
				Bills.RemoveAndDeleteAll();
				Containers.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		#region IClusterKeyMaster

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)AMA_ClusterKeyInfo;

		[LightValidationTestExempt]
		public override ZInt AMA_ClusterKey
		{
			get => base.AMA_ClusterKey;
			set
			{
				if (base.AMA_ClusterKey != value)
				{
					this.CheckCanSetMasterClusterKey();
					base.AMA_ClusterKey = value;
				}
			}
		}

		#endregion

		#region IAuditParent

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new(AsycudaArrivalHeaderSchema.ATH_ClusterKey, null);
				yield return new(AsycudaArrivalLineSchema.ATL_ClusterKey, null);
				yield return new(AsycudaBillSchema.ABL_ClusterKey, null);
				yield return new(AsycudaBillScreeningSchema.ASR_ClusterKey, null);
				yield return new(AsycudaContainerSchema.ACN_ClusterKey, null);
				yield return new(AsycudaContainerBillOrPackageLinkSchema.APC_ClusterKey, null);
				yield return new(AsycudaPackSchema.APA_ClusterKey, null);
				yield return new(AsycudaPackedItemSchema.API_ClusterKey, null);
				yield return new(AsycudaTaxSchema.AET_ClusterKey, null);
				yield return new(AsycudaTransferBillSchema.ATB_ClusterKey, null);
				yield return new(AsycudaTransferHeaderSchema.ATF_ClusterKey, null);
				yield return new(AsycudaPackPackedItemPivotSchema.APP_ClusterKey, null);
			}
		}

		#endregion
	}
}
