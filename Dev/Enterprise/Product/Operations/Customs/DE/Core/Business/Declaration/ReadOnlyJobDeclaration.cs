using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public partial class JobDeclaration
	{
		public ZBool IsWarehouseAdjustment => JE_MessageType == Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;

		public ZBool IsInwardProcessingAVABR => CustomsEntryInstructions.Any(i => i.CEI_Style == ImportDeclarationTypeList.Codes.AVABR);

		internal bool IsWarehouseAdjustmentOrInwardProcessingAVABR => IsWarehouseAdjustment || IsInwardProcessingAVABR;

		public new bool JE_TransportMode_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR || base.JE_TransportMode_ReadOnly;

		[ReadOnlyMember(nameof(ZG_BorderTransportMeans_ReadOnly))]
		public override ZString ZG_BorderTransportMeans
		{
			get => base.ZG_BorderTransportMeans;
			set => base.ZG_BorderTransportMeans = value;
		}
		ZBool ZG_BorderTransportMeans_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(ZG_SpecificCircumstanceIndicator_ReadOnly))]
		public override ZString ZG_SpecificCircumstanceIndicator
		{
			get => base.ZG_SpecificCircumstanceIndicator;
			set => base.ZG_SpecificCircumstanceIndicator = value;
		}
		ZBool ZG_SpecificCircumstanceIndicator_ReadOnly => IsWarehouseAdjustment;

		[ReadOnlyMember(nameof(JE_RN_NKTransportNationality_ReadOnly))]
		public override ZString JE_RN_NKTransportNationality
		{
			get => base.JE_RN_NKTransportNationality;
			set => base.JE_RN_NKTransportNationality = value;
		}
		ZBool JE_RN_NKTransportNationality_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_RL_NKPortOfLoading_ReadOnly))]
		public override ZString JE_RL_NKPortOfLoading
		{
			get => base.JE_RL_NKPortOfLoading;
			set => base.JE_RL_NKPortOfLoading = value;
		}
		ZBool JE_RL_NKPortOfLoading_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_ExportDate_ReadOnly))]
		public override ZDateTime JE_ExportDate
		{
			get => base.JE_ExportDate;
			set => base.JE_ExportDate = value;
		}
		ZBool JE_ExportDate_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_RL_NKPortOfFirstArrival_ReadOnly))]
		public override ZString JE_RL_NKPortOfFirstArrival
		{
			get => base.JE_RL_NKPortOfFirstArrival;
			set => base.JE_RL_NKPortOfFirstArrival = value;
		}
		ZBool JE_RL_NKPortOfFirstArrival_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_DateOfFirstArrival_ReadOnly))]
		public override ZDateTime JE_DateOfFirstArrival
		{
			get => base.JE_DateOfFirstArrival;
			set => base.JE_DateOfFirstArrival = value;
		}
		ZBool JE_DateOfFirstArrival_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_RL_NKPortOfArrival_ReadOnly))]
		public override ZString JE_RL_NKPortOfArrival
		{
			get => base.JE_RL_NKPortOfArrival;
			set => base.JE_RL_NKPortOfArrival = value;
		}
		ZBool JE_RL_NKPortOfArrival_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_DateOfArrival_ReadOnly))]
		public override ZDateTime JE_DateOfArrival
		{
			get => base.JE_DateOfArrival;
			set => base.JE_DateOfArrival = value;
		}
		ZBool JE_DateOfArrival_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(ZG_Box18TransportID_ReadOnly))]
		public override ZString ZG_Box18TransportID
		{
			get => base.ZG_Box18TransportID;
			set => base.ZG_Box18TransportID = value;
		}
		ZBool ZG_Box18TransportID_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(ZG_Box18TransportNationality_ReadOnly))]
		public override ZString ZG_Box18TransportNationality
		{
			get => base.ZG_Box18TransportNationality;
			set => base.ZG_Box18TransportNationality = value;
		}
		ZBool ZG_Box18TransportNationality_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		protected new bool JE_HouseBill_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR || base.JE_HouseBill_ReadOnly;

		[ReadOnlyMember(nameof(JE_RL_NKOrigin_ReadOnly))]
		public override ZString JE_RL_NKOrigin
		{
			get => base.JE_RL_NKOrigin;
			set => base.JE_RL_NKOrigin = value;
		}
		ZBool JE_RL_NKOrigin_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_GoodsOrigin_ReadOnly))]
		public override ZString JE_GoodsOrigin
		{
			get => base.JE_GoodsOrigin;
			set => base.JE_GoodsOrigin = value;
		}
		ZBool JE_GoodsOrigin_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_DateAtOrigin_ReadOnly))]
		public override ZDateTime JE_DateAtOrigin
		{
			get => base.JE_DateAtOrigin;
			set => base.JE_DateAtOrigin = value;
		}
		ZBool JE_DateAtOrigin_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_DateAtFinalDestination_ReadOnly))]
		public override ZDateTime JE_DateAtFinalDestination
		{
			get => base.JE_DateAtFinalDestination;
			set => base.JE_DateAtFinalDestination = value;
		}
		ZBool JE_DateAtFinalDestination_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_GoodsDescription_ReadOnly))]
		public override ZString JE_GoodsDescription
		{
			get => base.JE_GoodsDescription;
			set => base.JE_GoodsDescription = value;
		}
		ZBool JE_GoodsDescription_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_TotalNoOfPacks_ReadOnly))]
		public override ZInt JE_TotalNoOfPacks
		{
			get => base.JE_TotalNoOfPacks;
			set => base.JE_TotalNoOfPacks = value;
		}
		ZBool JE_TotalNoOfPacks_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_TotalNoOfPacksPackType_ReadOnly))]
		public override ZString JE_TotalNoOfPacksPackType
		{
			get => base.JE_TotalNoOfPacksPackType;
			set => base.JE_TotalNoOfPacksPackType = value;
		}
		ZBool JE_TotalNoOfPacksPackType_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_TotalWeight_ReadOnly))]
		public override ZDecimal JE_TotalWeight
		{
			get => base.JE_TotalWeight;
			set => base.JE_TotalWeight = value;
		}
		ZBool JE_TotalWeight_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_TotalWeightUnit_ReadOnly))]
		public override ZString JE_TotalWeightUnit
		{
			get => base.JE_TotalWeightUnit;
			set => base.JE_TotalWeightUnit = value;
		}
		ZBool JE_TotalWeightUnit_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_ShipmentIncoTerm_ReadOnly))]
		public override ZString JE_ShipmentIncoTerm
		{
			get => base.JE_ShipmentIncoTerm;
			set => base.JE_ShipmentIncoTerm = value;
		}
		ZBool JE_ShipmentIncoTerm_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_TotalVolume_ReadOnly))]
		public override ZDecimal JE_TotalVolume
		{
			get => base.JE_TotalVolume;
			set => base.JE_TotalVolume = value;
		}
		ZBool JE_TotalVolume_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_TotalVolumeUnit_ReadOnly))]
		public override ZString JE_TotalVolumeUnit
		{
			get => base.JE_TotalVolumeUnit;
			set => base.JE_TotalVolumeUnit = value;
		}
		ZBool JE_TotalVolumeUnit_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_EntryStyle_ReadOnly))]
		public override ZString JE_EntryStyle
		{
			get { return base.JE_EntryStyle; }
			set { base.JE_EntryStyle = value; }
		}
		ZBool JE_EntryStyle_ReadOnly => IsWarehouseAdjustment;

		[ReadOnlyMember(nameof(JE_GoodsDestination_ReadOnly))]
		public override ZString JE_GoodsDestination
		{
			get => base.JE_GoodsDestination;
			set => base.JE_GoodsDestination = value;
		}
		ZBool JE_GoodsDestination_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		protected override ZBool JE_ShipmentIncoTermPlace_ReadOnlyCore => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ResourceStringData("DEJobDeclaration|ZG_AgreedPlaceCode", Caption = "Incoterm Place Code", MediumCaption = "Inco. Place Code", ShortCaption = "Inco. Place Code")]
		[ReadOnlyMember(nameof(ZG_AgreedPlaceCode_ReadOnly))]
		public override ZString ZG_AgreedPlaceCode
		{
			get => base.ZG_AgreedPlaceCode;
			set => base.ZG_AgreedPlaceCode = value;
		}
		ZBool ZG_AgreedPlaceCode_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		protected override bool AgentsReferenceReadOnly => IsWarehouseAdjustment;

		protected override bool UCRReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		protected override void SupplierDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
		{
			base.SupplierDocAddressRequirement_ValidateOrganisationPK(validation);

			var parent = validation.Parent;
			if (parent != null && parent.Address == null && IsWarehouseAdjustment && AtLeastOneEntryInstructionHasFromWarehouse)
			{
				parent.OrganisationPKInfo.AddMessageError(SupplierDocumentaryAddressIsRequiredForBondedWarehousing);
			}
		}

		protected override bool IsImporterDocumentaryAddressRequiredForWarehouseValidationCore => !IsWarehouseAdjustment && base.IsImporterDocumentaryAddressRequiredForWarehouseValidationCore;

		public override JobDocAddress SupplierDocumentaryAddress
		{
			get
			{
				var supplierDocumentaryAddress = base.SupplierDocumentaryAddress;
				supplierDocumentaryAddress.SetReadOnlyIncludingChildren(IsInwardProcessingAVABR);
				return supplierDocumentaryAddress;
			}
		}

		public override JobDocAddress DefermentPartyDocAddress
		{
			get
			{
				var defermentPartyDocAddress = base.DefermentPartyDocAddress;
				defermentPartyDocAddress.SetReadOnlyIncludingChildren(IsInwardProcessingAVABR);
				return defermentPartyDocAddress;
			}
		}

		[ReadOnlyMember(nameof(JE_DeclarantType_ReadOnly))]
		public override ZString JE_DeclarantType
		{
			get => base.JE_DeclarantType;
			set => base.JE_DeclarantType = value;
		}

		ZBool JE_DeclarantType_ReadOnly => IsInwardProcessingAVABR;

		ZBool ZG_MethodOfPayment_ReadOnly => IsInwardProcessingAVABR;

		[ReadOnlyMember(nameof(ZG_CTStatusID_ReadOnly))]
		public override ZString ZG_CTStatusID
		{
			get => base.ZG_CTStatusID;
			set => base.ZG_CTStatusID = value;
		}

		ZBool ZG_CTStatusID_ReadOnly => IsInwardProcessingAVABR;

		ZBool ZG_IsHighValueOvrd_ReadOnly => IsInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JE_CustomsOffice_ReadOnly))]
		public override ZString JE_CustomsOffice
		{
			get => base.JE_CustomsOffice;
			set => base.JE_CustomsOffice = value;
		}

		ZBool JE_CustomsOffice_ReadOnly => IsInwardProcessingAVABR;

		ZBool JE_OA_Representative_ReadOnly => IsInwardProcessingAVABR;

		public override bool JE_OwnerRef_ReadOnly
		{
			get => IsInwardProcessingAVABR || base.JE_OwnerRef_ReadOnly;
		}

		void ClearFieldsIfNeeded()
		{
			if (IsWarehouseAdjustment)
			{
				ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
				JE_EntryStyle = ZString.Empty;
				JE_TransportMode = ZString.Empty;
				ZG_BorderTransportMeans = ZString.Empty;
				JE_ContainerMode = ZString.Empty;
				ZG_SpecificCircumstanceIndicator = ZString.Empty;

				JE_MasterBill = ZString.Empty;
				JE_VoyageFlightNo = ZString.Empty;
				JE_VesselName = ZString.Empty;
				JE_LloydsIMO = ZString.Empty;
				JE_RN_NKTransportNationality = ZString.Empty;
				JE_RL_NKPortOfLoading = ZString.Empty;
				JE_ExportDate = ZDateTime.Empty;
				JE_RL_NKPortOfFirstArrival = ZString.Empty;
				JE_DateOfFirstArrival = ZDateTime.Empty;
				JE_RL_NKPortOfArrival = ZString.Empty;
				JE_DateOfArrival = ZDateTime.Empty;
				ZG_Box18TransportID = ZString.Empty;
				ZG_Box18TransportNationality = ZString.Empty;
				JE_TransportModeInland = ZString.Empty;
				InlandTransports.RemoveAndDeleteAll();

				JE_HouseBill = ZString.Empty;
				JE_RL_NKOrigin = ZString.Empty;
				JE_GoodsOrigin = ZString.Empty;
				JE_DateAtOrigin = ZDateTime.Empty;
				JE_RL_NKFinalDestination = ZString.Empty;
				JE_GoodsDestination = ZString.Empty;
				JE_DateAtFinalDestination = ZDateTime.Empty;

				JE_GoodsDescription = ZString.Empty;
				JE_LocationOfGoods = ZString.Empty;
				JE_TotalNoOfPacks = ZInt.Zero;
				JE_TotalNoOfPacksPackType = ZString.Empty;
				JE_TotalWeight = ZDecimal.Zero;
				JE_TotalWeightUnit = ZString.Empty;
				JE_ShipmentIncoTerm = ZString.Empty;
				JE_TotalVolume = ZDecimal.Zero;
				JE_TotalVolumeUnit = ZString.Empty;
				JE_ShipmentIncoTermPlace = ZString.Empty;
				ZG_AgreedPlaceCode = ZString.Empty;

				JE_AgentsReference = ZString.Empty;
				JE_UCR = ZString.Empty;

				ZG_PresentationStartDate = ZDate.Empty;
				ZG_PresentationEndDate = ZDate.Empty;
			}
		}
	}
}
