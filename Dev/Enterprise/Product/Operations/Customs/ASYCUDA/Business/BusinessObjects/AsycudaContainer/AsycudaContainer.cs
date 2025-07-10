using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.ASYCUDA.Business
{
	[SystemDefinedValues]
	[CodeProperty(Schema.ACN_ContainerNumber), DescriptionProperty("Description")]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class AsycudaContainer : ManifestBase.AsycudaContainer
		, Integration.Customs.ASYCUDA.IAsycudaContainer
		, ISynchableContainer
		, ISynchroniserReadOnlyMembersProvider
		, ISailingSynchronisationTarget<BillOfLadingContainer>
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly AsycudaContainerTypeDecider TypeDecider = new AsycudaContainerTypeDecider();

		public new AsycudaContainerValidation Validation => (AsycudaContainerValidation)base.Validation;

		protected override ManifestBase.AsycudaContainerValidation GetNewValidation() => new AsycudaContainerValidation(this);

		public new AsycudaContainerLookups Lookups => (AsycudaContainerLookups)base.Lookups;

		protected override ManifestBase.AsycudaContainerLookups GetNewLookups() => new AsycudaContainerLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ACN_GoodsWeightUQ = Core.Constants.Weight.Kilograms;
		}

		public ZString Description
		{
			get { return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0} {1}pk {2}", ACN_ContainerNumber, ACN_NumberOfPackages, ACN_CommodityCode); }
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.EmptyFullList))]
		public override ZString ACN_EmptyFullIndicator
		{
			get { return base.ACN_EmptyFullIndicator; }
			set { base.ACN_EmptyFullIndicator = value; }
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.SealingPartyList))]
		public override ZString ACN_SealingPartyType
		{
			get { return base.ACN_SealingPartyType; }
			set { base.ACN_SealingPartyType = value; }
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.SealingPartyList))]
		public override ZString ACN_SealingPartyType2
		{
			get => base.ACN_SealingPartyType2;
			set => base.ACN_SealingPartyType2 = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.SealingPartyList))]
		public override ZString ACN_SealingPartyType3
		{
			get => base.ACN_SealingPartyType3;
			set => base.ACN_SealingPartyType3 = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.SealTypeList))]
		public override ZString ACN_SealType1
		{
			get => base.ACN_SealType1;
			set => base.ACN_SealType1 = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.SealTypeList))]
		public override ZString ACN_SealType2
		{
			get => base.ACN_SealType2;
			set => base.ACN_SealType2 = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.SealTypeList))]
		public override ZString ACN_SealType3
		{
			get => base.ACN_SealType3;
			set => base.ACN_SealType3 = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.UnloadingStatesList))]
		public override ZString ACN_Seal1UnloadingState { get => base.ACN_Seal1UnloadingState; set => base.ACN_Seal1UnloadingState = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.UnloadingStatesList))]
		public override ZString ACN_Seal2UnloadingState { get => base.ACN_Seal2UnloadingState; set => base.ACN_Seal2UnloadingState = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.UnloadingStatesList))]
		public override ZString ACN_Seal3UnloadingState { get => base.ACN_Seal3UnloadingState; set => base.ACN_Seal3UnloadingState = value; }

		ZPropertyInfo ISynchableContainer.ContainerNumInfo
		{
			get { return ACN_ContainerNumberInfo; }
		}

		bool ISynchableContainer.IsNonContainerized
		{
			get { return false; }
		}

		ZPropertyInfo ISynchableContainer.Seal1Info
		{
			get { return ACN_Seal1Info; }
		}

		ZPropertyInfo ISynchableContainer.Seal2Info
		{
			get { return ACN_Seal2Info; }
		}

		bool ISynchableContainer.Seal2Supported
		{
			get { return true; }
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		ZPropertyInfo ISynchableContainer.Seal3Info
		{
			get { return ACN_Seal3Info; }
		}

		bool ISynchableContainer.Seal3Supported
		{
			get { return true; }
		}

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		[ResourceStringData("024774cc-3ce5-41c3-9e8d-ec266141d33d", Caption = "Commodity Code")]
		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.CommodityCodes))]
		public override ZString ACN_CommodityCode
		{
			get { return base.ACN_CommodityCode; }
			set { base.ACN_CommodityCode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.WeightCodes))]
		public override ZString ACN_GoodsWeightUQ
		{
			get { return base.ACN_GoodsWeightUQ; }
			set { base.ACN_GoodsWeightUQ = value; }
		}

		[ResourceStringData("24d0be62-cd67-4aa3-b865-7768c7f64d0f", Caption = "Stow Location")]
		public override ZString ACN_StowageLocation
		{
			get => base.ACN_StowageLocation;
			set => base.ACN_StowageLocation = value;
		}

		[ResourceStringData("b21f70ad-3ecf-429b-8dd9-2e09ebad9564", Caption = "Packages")]
		public override ZInt ACN_NumberOfPackages
		{
			get => base.ACN_NumberOfPackages;
			set => base.ACN_NumberOfPackages = value;
		}

		public ZDecimal ACN_GoodsWeightInKilos
		{
			get { return Core.Constants.Weight.ConvertSafe(ACN_GoodsWeight, ACN_GoodsWeightUQ, Core.Constants.Weight.Kilograms); }
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = "Manifest Container";

				if (!ACN_ContainerNumber.IsEmpty)
				{
					result += " " + ACN_ContainerNumber;
				}

				return result;
			}
		}

		#region ISailingSynchronisationTarget

		bool ISailingSynchronisationTarget<BillOfLadingContainer>.IsMatched(BillOfLadingContainer container)
		{
			return IsMatched(container);
		}

		bool IsMatched(BillOfLadingContainer container)
		{
			var billOfLading = Header?.BillOfLadingForSync;
			return IsMatched(container, billOfLading);
		}

		bool IsMatched(BillOfLadingContainer container, BillOfLading billOfLading)
		{
			return billOfLading != null && billOfLading.IsContainerised && billOfLading.RealContainers.Contains(container) && ACN_ContainerNumber == container.JC_ContainerNum;
		}

		void ISailingSynchronisationTarget<BillOfLadingContainer>.Set(BillOfLadingContainer container)
		{
			using (GetValidationSuspender())
			{
				ACN_ContainerNumber = container.JC_ContainerNum;
			}
		}

		void ISailingSynchronisationTarget<BillOfLadingContainer>.Synchronise()
		{
			var source = SailingSynchronisationSource;
			if (source != null)
			{
				using (GetValidationDataSuspender())
				{
					ACN_RC_ContainerType = source.JC_RC;
					ACN_Seal1 = source.JC_SealNum;
					ACN_Seal2 = source.JC_AdditionalSealNum;
					ACN_Seal3 = source.JC_Additional2SealNum;
					ACN_CommodityCode = source.JC_RH_NKContainerCommodityCode.SubstringSafe(0, ACN_CommodityCodeInfo.MaxLength);
					ACN_GoodsWeight = source.JC_GrossWeight;
					ACN_GoodsWeightUQ = source.JC_GrossWeightUQ;
					ACN_StowageLocation = source.JC_StowagePosition.SubstringSafe(0, ACN_StowageLocationInfo.MaxLength);

					if (source.JC_IsEmptyContainer)
					{
						ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.EmptyContainer;
					}
					else if (source.JC_ContainerMode.EqualsIgnoringCase(Core.Constants.ContainerModes.FCL))
					{
						ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.FullContainerLoad;
					}
					else if (source.JC_ContainerMode.EqualsIgnoringCase(Core.Constants.ContainerModes.LCL))
					{
						ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.LessThanFullContainerLoad;
					}
					else
					{
						ACN_EmptyFullIndicator = ZString.Empty;
					}
				}
			}
		}

		BillOfLadingContainer ISailingSynchronisationTarget<BillOfLadingContainer>.Source => SailingSynchronisationSource;

		BillOfLadingContainer SailingSynchronisationSource
		{
			get
			{
				var billOfLading = Header.BillOfLadingForSync;

				return sailingSynchronisationSource != null && IsMatched(sailingSynchronisationSource)
					? sailingSynchronisationSource
					: billOfLading != null
						? sailingSynchronisationSource = billOfLading.RealContainers.Cast<BillOfLadingContainer>().FirstOrDefault(x => IsMatched(x, billOfLading))
						: null;
			}
		}
		BillOfLadingContainer sailingSynchronisationSource;

		#endregion
	}
}
