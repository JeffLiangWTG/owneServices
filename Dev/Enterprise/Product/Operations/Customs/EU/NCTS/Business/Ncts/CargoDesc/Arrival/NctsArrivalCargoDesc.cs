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
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[SystemDefinedValues]
	public class NctsArrivalCargoDesc : NctsCommonCargoDesc
		, Integration.Customs.EU.NCTS.IArrivalCargoDesc
		, IDocAddresses
		, IUNDGDataItemProvider
		, ICusInBondCargoDescTypeProvider
		, IUnloadedStatusSupporter
		, ITariffDescriptionSynchronizerSupporter
	{
		public NctsArrivalCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region GenAddOn
		public static class GenAddOnColumnConstants
		{
			public const string LiabilityTariffColumnName = "LiabilityTariff";
		}
		#endregion

		public static new readonly TypeDecider TypeDecider = new NctsArrivalCargoDescTypeDecider();

		protected override void OnElementChanged()
		{
			base.OnElementChanged();
			RefreshParentBizObj();
		}

		protected override void OnElementReset()
		{
			base.OnElementReset();
			RefreshParentBizObj();
		}

		void RefreshParentBizObj()
		{
			if (MoveHeaderOrBillParent is NctsBill bill && bill.MovementDetail is CusInBondMoveDetail detail && !detail.ShouldValidateOnSave && !IsCopying && !IsValidationSuspended)
			{
				detail.MarkAsNeedingValidation();
			}
		}

		protected override ZString TariffTypeCore => Constants.TariffTypes.Export;

		public override ZDateTime ValuationDate
		{
			get
			{
				var result = Header?.ArrivalMovementHeader?.BM_ValuationDate ?? ZDateTime.Today;
				return result.IsEmpty ? ZDateTime.Today : result;
			}
		}

		protected override bool EnableLightValidationIfAvailable => false;

		[ResourceStringData("5b92221e-0244-43d3-85a8-5084f0ebb790", ShortCaption = "Seq.No.", Caption = "Sequence No.", FullDescription = "Sequence Number")]
		public override ZShort BY_LineNo { get => base.BY_LineNo; set => base.BY_LineNo = value; }

		[MaxLength(9)]
		[List(nameof(Lookups) + "." + nameof(NctsArrivalCargoDescLookups.CusCodeList))]
		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[ResourceStringData("f6b1bbe3-ba79-4569-ae37-4fd55fcf194d", ShortCaption = "CUS Cd", Caption = "CUS Code")]
		public override ZString BY_CusC4Number { get => base.BY_CusC4Number; set => base.BY_CusC4Number = value; }

		[List(nameof(Lookups) + "." + nameof(NctsArrivalCargoDescLookups.UnloadedStates))]
		[ResourceStringData("EU.NCTS.NctsArrivalCargoDesc|BY_UnloadedState", Caption = "Unloaded State", MediumCaption = "Un State", ShortCaption = "State")]
		[ReadOnlyMember(nameof(UnloadedStateIsReadOnly))]
		public override ZString BY_UnloadedState
		{
			get => base.BY_UnloadedState;
			set
			{
				var oldValue = base.BY_UnloadedState;
				base.BY_UnloadedState = value;
				if (value != NctsUnloadedStateListForHouseConsignment.Codes.DIF)
				{
					DeleteUnloadedGoodsItem();
					BY_BY_Commodity = ZGuid.Empty;
				}
				else if (value != oldValue)
				{
					CopyDeclaredValuesToUnloadingValues();
				}

				if (IsLiabilityCalculationForArrivalSupported)
				{
					LiabilityTariff = TariffFromDeclaredOrUnloadedValue;
				}
				else
				{
					RemoveDataFromLiabilityCalculation();
				}
			}
		}

		[ResourceStringData("12930108-CD15-439F-A31A-2C6A4C6D8EAC", Caption = "Commodity", MediumCaption = "Commodity Code", ShortCaption = "Cmdty.", FullDescription = "Commodity Code")]
		[ReadOnlyMember(nameof(HarmonisedTariffIsReadOnly))]
		public override ZString BY_HarmonisedTariff
		{
			get => base.BY_HarmonisedTariff;
			set
			{
				var oldValue = BY_HarmonisedTariff;
				var shouldSynchronizeDescription = TariffDescriptionSynchronizer.ShouldSynchronizeDescription;

				base.BY_HarmonisedTariff = value;
				if (!IsCopying && oldValue != BY_HarmonisedTariff)
				{
					if (shouldSynchronizeDescription)
					{
						TariffDescriptionSynchronizer.SynchronizeDescription();
					}
					if (IsLiabilityCalculationForArrivalSupported)
					{
						LiabilityTariff = BY_HarmonisedTariff;
					}
				}
			}
		}

		TariffDescriptionSynchronizer TariffDescriptionSynchronizer => tariffDescriptionSynchronizer ?? (tariffDescriptionSynchronizer = new TariffDescriptionSynchronizer(this));
		TariffDescriptionSynchronizer tariffDescriptionSynchronizer;

		#region ITariffDescriptionSyncronizerSupporter

		ZString ITariffDescriptionSynchronizerSupporter.CurrentTariffDescription
		{
			get => BY_Description;
			set => BY_Description = value;
		}

		ZString ITariffDescriptionSynchronizerSupporter.OfficialCustomsTariffDescription => UniversalTariff?.FullTariffDescription(ValuationDate, includeSectionHeadings: false, includeChapterHeading: false, useTariffPreferredLanguage: true).Left(BY_DescriptionInfo.MaxLength) ?? ZString.Empty;

		#endregion

		[ReadOnlyMember(nameof(HarmonisedTariffIsReadOnly))]
		public override ZString BY_FormattedHarmonisedTariff
		{
			get => base.BY_FormattedHarmonisedTariff;
			set => base.BY_FormattedHarmonisedTariff = value;
		}

		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[ResourceStringData("0122581d-d28c-44dc-b4af-b8ba7695f19b", ShortCaption = "Descr.", Caption = "Description", FullDescription = "Description of goods")]
		public override ZString BY_Description
		{
			get => base.BY_Description;
			set => base.BY_Description = value;
		}

		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[ResourceStringData("efb8eff4-3d9e-4160-a031-5cf6c7a9c128", ShortCaption = "Gross Wgt.", Caption = "Gross Weight", FullDescription = "Gross Weight of the Goods")]
		public override ZDecimal BY_GrossWeight
		{
			get => base.BY_GrossWeight;
			set => base.BY_GrossWeight = value;
		}

		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[ResourceStringData("03b82514-fcb0-4221-8189-f9858e3392e3", Caption = "Units", FullDescription = "Gross Weight Unit qualifier")]
		public override ZString BY_GrossWeightUnit
		{
			get => base.BY_GrossWeightUnit;
			set => base.BY_GrossWeightUnit = value;
		}

		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[ResourceStringData("aaae6fab-c775-4c84-b112-f7d220acf85b", ShortCaption = "Net Wgt.", Caption = "Net Weight", FullDescription = "Net Weight of the Goods")]
		public override ZDecimal BY_NetWeight
		{
			get => base.BY_NetWeight;
			set => base.BY_NetWeight = value;
		}

		[ReadOnlyMember(nameof(GoodsItemIsReadOnly))]
		[ResourceStringData("82838f6d-0947-4a02-b823-b6b7f0a2ec67", Caption = "Units", FullDescription = "Net Weight Unit qualifier")]
		public override ZString BY_NetWeightUnit
		{
			get => base.BY_NetWeightUnit;
			set => base.BY_NetWeightUnit = value;
		}

		[RelatedBusinessObject(nameof(UnloadedGoodsItem))]
		public override ZGuid BY_BY_Commodity
		{
			get => base.BY_BY_Commodity;
			set => base.BY_BY_Commodity = value;
		}

		[ResourceStringData("dcce3672-c639-46a4-972f-89bdd26c6af7", ShortCaption = "Cmdty.", Caption = "Commodity", FullDescription = "Commodity Code")]
		public override ZString BY_CommodityCode
		{
			get => base.BY_CommodityCode;
			set => base.BY_CommodityCode = value;
		}

		public NctsUnloadedCargoDesc UnloadedGoodsItem
		{
			get
			{
				if (IsUnloadedStateDIF)
				{
					if (unloadedGoodsItem == null || unloadedGoodsItem.IsDeleted)
					{
						unloadedGoodsItem = NctsUnloadedCargoDesc.LoadOrCreate(this);
						RegisterEditableChildObject(unloadedGoodsItem);
					}
					return unloadedGoodsItem;
				}
				else
				{
					RefreshBindingIncludingChildren();
					return null;
				}
			}
		}
		NctsUnloadedCargoDesc unloadedGoodsItem;

		public override void Delete()
		{
			DeleteUnloadedGoodsItem();
			base.Delete();
		}

		public new NctsArrivalCargoDescLookups Lookups => (NctsArrivalCargoDescLookups)base.Lookups;

		public new NctsArrivalCargoDescValidation Validation => (NctsArrivalCargoDescValidation)base.Validation;

		protected override CusInBondCargoDescLookups GetNewLookups() => new NctsArrivalCargoDescLookups(this);

		protected override CusInBondCargoDescValidation GetNewValidation() => new NctsArrivalCargoDescValidation(this);

		protected virtual bool GoodsItemIsReadOnly => !StatusIsNew || IsUnloadingRemarksReadOnly || AreUnloadingRemarksFullyAccepted;

		protected virtual bool HarmonisedTariffIsReadOnly => (!StatusIsNew && !BY_UnloadedState.IsEmpty) || IsUnloadingRemarksReadOnly || AreUnloadingRemarksFullyAccepted;

		protected virtual bool UnloadedStateIsReadOnly => StatusIsNew || IsUnloadingRemarksReadOnly || AreUnloadingRemarksFullyAccepted || IsUnloadedStateMISIncludingParent;

		protected bool StatusIsNew => BY_UnloadedState == NctsUnloadedStateListForHouseConsignment.Codes.NEW;

		protected bool IsUnloadedStateDIF => BY_UnloadedState == NctsUnloadedStateList.Codes.DIF;

		protected bool IsUnloadedStateMISIncludingParent =>
			BY_UnloadedState == NctsUnloadedStateListForHouseConsignment.Codes.MIS &&
			Bill?.MovementDetail != null && Bill.MovementDetail.B9_UnloadedState == NctsUnloadedStateListForHouseConsignment.Codes.MIS;

		public ZString DeclaredNewLabel => BY_UnloadedState == NctsUnloadedStateListForHouseConsignment.Codes.NEW ? ResString.GetMultilingualString("EB798FA6-187E-4A1C-A90B-E81E95F88717", "New Value") : ResString.GetMultilingualString("EBA73E3A-2F9D-450D-83B7-36E2C9281530", "Declared Value");

		public ZBool UnloadedColumsVisible => IsUnloadedStateDIF;

		public bool AreUnloadingRemarksFullyAccepted => Header is NctsHeader header && header.ArrivalMovementHeader.AreUnloadingRemarksFullyAccepted;

		public bool IsUnloadingRemarksReadOnly => Header is NctsHeader header && header.MessageHasBeenSent;

		void DeleteUnloadedGoodsItem()
		{
			NctsUnloadedCargoDesc.Load(this)?.Delete();
		}

		void CopyDeclaredValuesToUnloadingValues()
		{
			UnloadedGoodsItem.BY_HarmonisedTariff = BY_HarmonisedTariff;
			UnloadedGoodsItem.BY_CusC4Number = BY_CusC4Number;
			UnloadedGoodsItem.BY_Description = BY_Description;
			UnloadedGoodsItem.BY_GrossWeight = BY_GrossWeight;
			UnloadedGoodsItem.BY_GrossWeightUnit = BY_GrossWeightUnit;
			UnloadedGoodsItem.BY_NetWeight = BY_NetWeight;
			UnloadedGoodsItem.BY_NetWeightUnit = BY_NetWeightUnit;
		}

		[ChildEditable(true)]
		public INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					previousDocuments = GetPreviousDocuments();
					previousDocuments.SetReadOnlyIncludingChildren(IsPhase5Arrival);
					previousDocuments.Load();
					RegisterEditableChildObject(previousDocuments);
				}
				return previousDocuments;
			}
		}
		INctsPreviousDocumentCollection<NctsPreviousDocument> previousDocuments;

		protected virtual INctsPreviousDocumentCollection<NctsPreviousDocument> GetPreviousDocuments() => new NctsPreviousDocumentCollection<NctsPreviousDocument>(this);

		public override bool CanDelete
		{
			get
			{
				var moveHeader = MoveHeader;
				bool result;
				if (moveHeader != null)
				{
					result = moveHeader.IsUnloadingMovementHeader && BY_LineNo > moveHeader.GoodsItems.Count && base.CanDelete;
				}
				else
				{
					result = UnloadedStateIsReadOnly;
				}
				return result;
			}
		}

		#region JobDocAddress

		JobDocAddressManager jobDocAddressManager;

		public JobDocAddressManager JobDocAddressManager => jobDocAddressManager ?? (jobDocAddressManager = new JobDocAddressManager());

		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		public SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		public JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType) => addressType == DocAddressType.ConsigneeAddress ? ConsigneeDocAddressRequirement : null;

		protected virtual JobDocAddressRequirement GetJobDocAddressRequirement(DocAddressType docAddressType) => new JobDocAddressRequirement(docAddressType);

		public JobDocAddress ConsigneeDocAddress
		{
			get
			{
				if (consigneeDocAddress == null || consigneeDocAddress.IsDeleted)
				{
					consigneeDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsigneeDocAddressRequirement);
				}

				return consigneeDocAddress;
			}
		}
		JobDocAddress consigneeDocAddress;

		public JobDocAddressRequirement ConsigneeDocAddressRequirement
		{
			get
			{
				if (consigneeDocAddressRequirement == null)
				{
					consigneeDocAddressRequirement = GetJobDocAddressRequirement(DocAddressType.ConsigneeAddress);
					JobDocAddressManager.AddRequirement(consigneeDocAddressRequirement);
				}

				return consigneeDocAddressRequirement;
			}
		}
		JobDocAddressRequirement consigneeDocAddressRequirement;

		public void DocAddressChanged(JobDocAddress docAddress)
		{
		}

		public void OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		public void OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		public void AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		public void OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		public bool CanDeleteAddress(JobDocAddress docAddress) => true;

		public OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType) => new OrgHeaderCollection(Factory);

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (jobDocAddressDependentCollection == null)
				{
					jobDocAddressDependentCollection = new JobDocAddressDependentCollection(this);
					jobDocAddressDependentCollection.Load();
					RegisterEditableChildObject(jobDocAddressDependentCollection);
				}

				return jobDocAddressDependentCollection;
			}
		}
		JobDocAddressDependentCollection jobDocAddressDependentCollection;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new DocAddressType[1] { DocAddressType.ConsigneeAddress };

		#endregion

		#region IUNDGDataItemProvider Members

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
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

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		public bool IsUnloadedCommodityCodeRequired => IsUnloadedCommodityCodeRequiredCore;

		protected virtual bool IsUnloadedCommodityCodeRequiredCore => true;

		#endregion

		Type ICusInBondCargoDescTypeProvider.CusInBondCargoDescType => NctsUnloadedCargoDescType;

		internal protected virtual Type NctsUnloadedCargoDescType => typeof(NctsUnloadedCargoDesc);

		public ZString UnloadedStatus
		{
			get => BY_UnloadedState;
			set => BY_UnloadedState = value;
		}

		public IEnumerable<IUnloadedStatusSupporter> RelatedItems => SupportingDocuments.Cast<IUnloadedStatusSupporter>()
			.Union(AdditionalInfos)
			.Union(PreviousDocuments)
			.Union(Packages.Cast<IUnloadedStatusSupporter>());

		[MaxLength(Schema.BY_HarmonisedTariffMaxLength)]
		[List(nameof(Lookups) + "." + nameof(NctsCommonCargoDescLookups.Tariffs))]
		[ResourceStringData("928D629A-DA90-4FA9-9BC2-AB8326170411", Caption = "Commodity Code", MediumCaption = "Commodity", ShortCaption = "Cmdty.")]
		public ZString LiabilityTariff
		{
			get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnConstants.LiabilityTariffColumnName);
			set
			{
				var oldValue = LiabilityTariff;
				if (value != oldValue)
				{
					CheckMaximumLength(LiabilityTariffInfo, value);
					this.SetSystemDefinedValue(GenAddOnColumnConstants.LiabilityTariffColumnName, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateLiabilityTariff();
					}
					LiabilityTariffInfo.RefreshBinding(oldValue);
					DefaultTariffUnitOfMeasures();
					SetDefaultTariffUnitOfMeasuresFromRatesView();
					UpdateAllFeesFromTariffRates();
				}
			}
		}

		public ZPropertyInfo LiabilityTariffInfo => GetZPropertyInfo(nameof(LiabilityTariff));

		ZString TariffFromDeclaredOrUnloadedValue => IsUnloadedStateDIF ? UnloadedGoodsItem.BY_HarmonisedTariff : BY_HarmonisedTariff;

		[MaxLength(Schema.BY_FormattedHarmonisedTariffMaxLength)]
		[BusinessObjectTestExclude]
		[ResourceStringData("81FEAAEB-000A-40A0-91CC-26CDD2D04B82", Caption = "Commodity Code", MediumCaption = "Commodity", ShortCaption = "Cmdty.")]
		public ZString LiabilityFormattedTariff { get => LiabilityFormattedTariffCore; set => LiabilityFormattedTariffCore = value; }

		protected virtual ZString LiabilityFormattedTariffCore
		{
			get
			{
				return TariffFormatter.DisplayFormat(LiabilityTariff);
			}
			set
			{
				var valuedFormatted = TariffFormatter.Format(value);
				if (valuedFormatted.SubstringSafe(0, 8) == TariffFromDeclaredOrUnloadedValue.SubstringSafe(0, 8))
				{
					ZString liabilityFormattedTariff = LiabilityFormattedTariff;
					LiabilityTariff = valuedFormatted;
					LiabilityFormattedTariffInfo.RefreshBinding(liabilityFormattedTariff);
				}
			}
		}

		[ReadOnly(true)]
		public override ZString BY_RX_NKCurrency { get => base.BY_RX_NKCurrency; set => base.BY_RX_NKCurrency = value; }

		public ZPropertyInfo LiabilityFormattedTariffInfo => GetWrappedZPropertyInfo(nameof(LiabilityFormattedTariff), (object x) => LiabilityTariffInfo);

		public ZBool IsLiabilityCalculationForArrivalSupported => (Header?.Configuration.GoodsItemsConfiguration.IsLiabilityCalculationForArrivalSupported() ?? false)
			&& BY_UnloadedState != NctsUnloadedStateList.Codes.MIS && (Header?.ArrivalMovementHeader?.ShouldGuaranteeForArrivalBeVisible ?? false);

		protected override ZString EffectiveTariffForCalculateFields => !LiabilityTariff.IsEmpty ? LiabilityTariff : base.EffectiveTariffForCalculateFields;

		void RemoveDataFromLiabilityCalculation()
		{
			BY_RN_NKCountryOfOrigin = ZString.Empty;
			LiabilityTariff = ZString.Empty;
			BY_CustomsSecondQuantity = ZDecimal.Zero;
			BY_CustomsSecondUnitQty = ZString.Empty;
			BY_CustomsThirdQuantity = ZDecimal.Zero;
			BY_CustomsThirdUnitQty = ZString.Empty;
			BY_CustomsFourthQuantity = ZDecimal.Zero;
			BY_CustomsFourthUnitQty = ZString.Empty;
			BY_MonetaryValue = ZDecimal.Zero;
			AdditionalSupplementaryCodes.RemoveAndDeleteAll();
		}
	}
}
