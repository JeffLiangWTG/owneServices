using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.NctsAndDeclarationIntegration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using RefCusCodeList = Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[SystemDefinedValues]
	public class NctsDepartureCargoDesc : NctsCommonCargoDesc
		, Integration.Customs.EU.NCTS.IDepartureCargoDesc
		, IDocAddresses
		, IUNDGDataItemProvider
		, ITariffDescriptionSynchronizerSupporter
		, ICusReferenceTypeSupporter
		, IWarehouseProductLine
	{
		public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : NctsCommonCargoDesc.Schema
		{
			public const int BY_CustomsThirdQuantityDecimalPlacesPhase4 = 3;
		}

		public readonly static new TypeDecider TypeDecider = new NctsDepartureCargoDescTypeDecider();

		public new IDepartureCargoDescLookups Lookups => (IDepartureCargoDescLookups)base.Lookups;

		public new NctsDepartureCargoDescValidation Validation => (NctsDepartureCargoDescValidation)base.Validation;

		public new NctsDepartureMovementHeader MoveHeader => Header?.MovementHeader;

		protected override bool SupportsCloneCore() => true;

		public override ZString BY_ParentTableCode
		{
			get => base.BY_ParentTableCode;
			set
			{
				var oldValue = BY_ParentTableCode;
				base.BY_ParentTableCode = value;
				if (!IsCopying && oldValue != BY_ParentTableCode)
				{
					var header = Header;
					if (header != null && header.IsPluggedIn)
					{
						Consignee.MakePersistentEvenIfEmpty();
						Consignor.MakePersistentEvenIfEmpty();
					}
					Bill?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid BY_ParentID
		{
			get => base.BY_ParentID;
			set
			{
				var oldValue = BY_ParentID;
				base.BY_ParentID = value;
				if (!IsCopying && oldValue != BY_ParentID)
				{
					Bill?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("8D1F7981-43D2-48EB-8672-6DD69193344A", Caption = "Is Main Pack?", MediumCaption = "Main Pack?")]
		public override ZBool BY_IsMainPack
		{
			get => base.BY_IsMainPack;
			set => base.BY_IsMainPack = value;
		}

		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.DeclarationTypeList))]
		[ResourceStringData("E0B5A9FA-7FC1-462D-B2A3-997A7A55A225", Caption = "[1] Declaration Type", ShortCaption = "Type", MediumCaption = "Decl. Type", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("753841A8-5B7B-40C9-8057-C953E09191ED", Caption = "Declaration Type", ShortCaption = "Type", MediumCaption = "Dec. Type", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ReadOnlyMember(nameof(DeclarationTypeReadOnly))]
		public override ZString BY_Type
		{
			get => base.BY_Type;
			set
			{
				var oldValue = BY_Type;
				base.BY_Type = value;
				if (oldValue != BY_Type && !IsCopying)
				{
					var moveHeader = MoveHeader;
					if (moveHeader != null)
					{
						if (AllLinesHaveMixedValidDeclarationTypesAndNotBlank && !value.IsEmpty)
						{
							moveHeader.SetMixedConsignment();
						}
					}
				}
			}
		}

		public override SupplementaryCodeCollection AdditionalSupplementaryCodes
		{
			get
			{
				var additionalSupplementaryCodes = base.AdditionalSupplementaryCodes;

				if (Header != null && additionalSupplementaryCodes.ReadOnly != Header.IsDepartureTabReadOnly)
				{
					additionalSupplementaryCodes.SetReadOnlyIncludingChildren(Header.IsDepartureTabReadOnly);
				}

				return additionalSupplementaryCodes;
			}
		}

		public bool DeclarationTypeReadOnly
		{
			get
			{
				var result = false;
				var departureMovementHeader = MoveHeader;
				if (departureMovementHeader != null)
				{
					result = !departureMovementHeader.BM_InBondEntryType.IsEmpty
							&& !departureMovementHeader.IsMixedConsignment
							&& Header.DepartureGoodsItems.All(i => i.BY_Type.IsEmpty);
				}
				return result;
			}
		}

		bool AllLinesHaveMixedValidDeclarationTypesAndNotBlank
		{
			get
			{
				var query = Header.DepartureGoodsItems.Select(i => i.BY_Type).Distinct().ToArray();
				return query.Length > 1 && !query.Except(new ZString[] {
					NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure,
					NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure,
					NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories,
					NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino }).Any();
			}
		}

		protected virtual ZDecimal VATRateForEmptyTaxType => HighestVATRate;

		[ResourceStringData("EU.NCTS.NctsDepartureCargoDesc|ExciseAmount", Caption = "Excise Amount", MediumCaption = "Excise Amount", ShortCaption = "Excise Amount", FullDescription = "Total amount of Excise Taxes calculated in Euros")]
		public ZDecimal ExciseAmount => GetFeeTotalAmount(ChargeType.Excise);

		public ZPropertyInfo ExciseAmountInfo => GetZPropertyInfo(nameof(ExciseAmount));

		protected virtual IEnumerable<RateView> ExciseRates => null;

		List<FeeData> FeeDataForVAT
		{
			get
			{
				var baseValue = BY_MonetaryValue + DutyAmount + AntiDumpingDutyAmount + CountervailingDutyAmount + ExciseAmount;
				var vatRate = BY_ZZF_NKTaxType.IsEmpty ? VATRateForEmptyTaxType : VATRate;
				var result = new List<FeeData>();
				if (vatRate != ZDecimal.Zero)
				{
					result.Add(new FeeData(baseValue, "%", baseValue * vatRate, vatRate));
				}
				return result;
			}
		}

		public override void UpdateAllFeesFromTariffRates()
		{
			base.UpdateAllFeesFromTariffRates();
			UpdateFeesFromTariffRates(updateExcise: true);
		}

		void UpdateFeesForType(ZString feeType, IEnumerable<RateView> rateViews)
		{
			var newFees = new List<FeeData>();
			if (rateViews != null)
			{
				foreach (var rateView in rateViews)
				{
					AddFeesForType(newFees, rateView);
				}
			}
			UpdateFeesForType(feeType, newFees);
		}

		internal protected void UpdateFeesFromTariffRates(bool updateExcise = false)
		{
			if (updateExcise)
			{
				UpdateFeesForType(ChargeType.Excise, ExciseRates);
			}
			UpdateFeesForType(NctsCommonCargoDesc.ChargeType.VAT, FeeDataForVAT);
			GetZPropertyInfo(nameof(ExciseAmount)).RefreshBinding();
			GetZPropertyInfo(nameof(VatAmount)).RefreshBinding();
		}

		public void RefreshBindingsWhenValuationDateChanged()
		{
			UpdateAllFeesFromTariffRates();
		}

		public override ZDateTime ValuationDate
		{
			get
			{
				var mrnNumber = Header?.GetMovementReferenceEntryNumberCore(false);
				var result = (MoveHeader?.BM_ValuationDate ?? ZDateTime.Empty)
					.IfEmptyUse(() => mrnNumber?.CE_IssueDate ?? ZDateTime.Empty)
					.IfEmptyUse(() => mrnNumber?.CE_SystemCreateTimeUtc ?? ZDateTime.Empty)
					.IfEmptyUse(() => Header?.BH_SystemCreateTimeUtc ?? ZDateTime.Empty)
					.IfEmptyUse(() => ZDateTime.Today);
				return result;
			}
		}

		public ZDecimal VATRate
		{
			get
			{
				var effectiveDate = ValuationDate;
				var dataGroupingCode = DataGroupingCode;
				return Factory.GetCachedValue(FormattableString.Invariant($"EU.NCTS.NctsDepartureCargoDesc.VATRate_{dataGroupingCode}_{BY_ZZF_NKTaxType}_{effectiveDate}"), () =>
				{
					var taxesOrFees = new RefCusTaxOrFee.Loader(Factory).LoadTaxOrFeeFromCodeDate(dataGroupingCode, BY_ZZF_NKTaxType, effectiveDate);
					return taxesOrFees?.FirstOrDefault()?.ZZF_Value ?? ZDecimal.Zero;
				});
			}
		}

		public ZDecimal HighestVATRate
		{
			get
			{
				var result = ZDecimal.Zero;
				var tariff = BY_HarmonisedTariff;
				if (!tariff.IsEmpty)
				{
					var effectiveDate = ValuationDate;
					var dataGroupingCode = DataGroupingCode;
					var taxType = BY_ZZF_NKTaxType;
					result = Factory.GetCachedValue(FormattableString.Invariant($"EU.NCTS.NctsDepartureCargoDesc.HighestVATRate_{dataGroupingCode}_{taxType}_{tariff}_{effectiveDate}"),
						() => UniversalTariff?.GetHighestVATRateByTaxOrFeeCode(dataGroupingCode, taxType, effectiveDate) ?? ZDecimal.Zero);
				}
				return result;
			}
		}

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
					DefaultTariffUnitOfMeasures();
					SetDefaultTaxType();
					SetDefaultTariffUnitOfMeasuresFromRatesView();
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

		[ChildEditable]
		public ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences
		{
			get
			{
				if (cusSupplyChainActorReferences == null)
				{
					cusSupplyChainActorReferences = GetCusSupplyChainActorReferences();
					cusSupplyChainActorReferences.Load();
					RegisterEditableChildObject(cusSupplyChainActorReferences);
				}
				return cusSupplyChainActorReferences;
			}
		}
		ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> cusSupplyChainActorReferences;

		protected virtual ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> GetCusSupplyChainActorReferences() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);

		public HashSet<ZString> ContainersSelected => Factory.GetValue(ref containersSelectedCached, () => IsPhase5 ?
			Packages.Cast<NctsPackage>()
			.SelectMany(x => x.ContainersSelected)
			.Distinct()
			.ToHashSet()
			:
			ContainersPivots.Cast<NonPersistentDepartureContainerPivot>()
			.Where(container => container.ContainerSelected && !container.ContainerNumber.IsEmpty)
			.Select(container => container.ContainerNumber)
			.ToHashSet());
		CachedProperty<HashSet<ZString>> containersSelectedCached;

		public IEnumerable<NctsCusInBondContainer> SelectedContainers => IsPhase5
			? Packages
				.SelectMany(x => x.ContainersPivot)
				.Select(x => x.Container)
				.Distinct()
			: ContainersPivots.Cast<NonPersistentDepartureContainerPivot>()
				.Where(container => container.ContainerSelected)
				.Select(container => container.Container)
				.Distinct();

		public void DeleteAllContainerPivots()
		{
			if (ContainersPivots.Count > 0)
			{
				ContainersPivots.RemoveAll();
			}
		}

		[ChildEditable(true)]
		public INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					previousDocuments = GetPreviousDocuments();
					previousDocuments.Load();
					RegisterEditableChildObject(previousDocuments);
				}
				return previousDocuments;
			}
		}
		INctsPreviousDocumentCollection<NctsPreviousDocument> previousDocuments;

		protected virtual INctsPreviousDocumentCollection<NctsPreviousDocument> GetPreviousDocuments() => new NctsPreviousDocumentCollection<NctsPreviousDocument>(this);

		public ZInt NCTSPreviousDocumentsCount => Factory.GetCached(ref nctsPreviousDocumentsCount, () => PreviousDocuments.Count(x => x.IsNCTSPreviousDocument));
		CachedProperty<ZInt> nctsPreviousDocumentsCount;

		public ZInt AllNCTSPreviousDocumentsCount => Factory.GetCached(ref allNCTSPreviousDocumentsCount, () => NCTSPreviousDocumentsCount + (Bill?.NCTSPreviousDocumentsCount ?? ZInt.Zero) + Header.NCTSPreviousDocumentsCount);
		CachedProperty<ZInt> allNCTSPreviousDocumentsCount;

		[ResourceStringData("EU.NCTS.NctsDepartureCargoDesc|BY_ZZF_NKTaxType", Caption = "Tax Type", ShortCaption = "Tax")]
		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.TaxOrFeeCodeList))]
		public override ZString BY_ZZF_NKTaxType
		{
			get => base.BY_ZZF_NKTaxType;
			set => base.BY_ZZF_NKTaxType = value.ToUpper();
		}

		[MeasureUnit(nameof(BY_CustomsUnitQty), MeasureUnitType.Unknown)]
		public override ZDecimal BY_CustomsQuantity
		{
			get => base.BY_CustomsQuantity;
			set => base.BY_CustomsQuantity = value;
		}

		[ReadOnlyMember(nameof(BY_DescriptionReadOnly))]
		public override ZString BY_Description { get => base.BY_Description; set => base.BY_Description = value; }

		protected bool BY_DescriptionReadOnly => Header != null && Header.IsDepartureTabReadOnly;

		[ReadOnly(true)]
		public override ZDecimal BY_MonetaryValue
		{
			get => base.BY_MonetaryValue;
			set => base.BY_MonetaryValue = value;
		}

		[ReadOnlyMember(nameof(BY_RN_NKCountryOfDispatchReadOnly))]
		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.CountryOfDispatchList))]
		[ResourceStringData("9BF5FC83-97F1-43B8-8E8D-7BBBD7CB1D78", Caption = "[15A] Dispatch Country/Region", ShortCaption = "Disp.", MediumCaption = "Disp. Ctry./Rgn.", FullDescription = "Country/Region of Dispatch", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("1934CDB0-9229-47B8-8691-41BBF4EC897E", Caption = "Country/Region of Dispatch", ShortCaption = "Disp. Ctry./Rgn.", MediumCaption = "Dispatch Ctry./Rgn.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BY_RN_NKCountryOfDispatch
		{
			get => base.BY_RN_NKCountryOfDispatch;
			set
			{
				var oldValue = BY_RN_NKCountryOfDispatch;
				base.BY_RN_NKCountryOfDispatch = value;
				if (!IsCopying && oldValue != BY_RN_NKCountryOfDispatch && Header is NctsHeader header)
				{
					header.BH_RL_NKImportLoadPortInfo.RefreshBinding();
				}
			}
		}
		protected bool BY_RN_NKCountryOfDispatchReadOnly => !IsPhase5 && BY_RN_NKCountryOfDispatch.IsEmpty && Header is NctsHeader header && !header.BH_RL_NKImportLoadPort.IsEmpty;

		[ReadOnlyMember(nameof(BY_RN_NKCountryOfDestinationReadOnly))]
		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.CountryOfDestinationList))]
		[ResourceStringData("64E66F3C-3356-4E85-82C5-59F0D189D013", Caption = "[17A] Destination Country/Region", ShortCaption = "Dest.", MediumCaption = "Dest. Ctry./Rgn.", FullDescription = "Country/Region of Destination", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("9C6A668E-243A-49F1-A7A0-FEFB32DCE8DE", Caption = "Country/Region of Destination", ShortCaption = "Destin. Ctry./Rgn.", MediumCaption = "Destination Ctry./Rgn.", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BY_RN_NKCountryOfDestination
		{
			get => base.BY_RN_NKCountryOfDestination;
			set
			{
				var oldValue = BY_RN_NKCountryOfDestination;
				base.BY_RN_NKCountryOfDestination = value;
				if (!IsCopying && oldValue != BY_RN_NKCountryOfDestination && MoveHeader is NctsDepartureMovementHeader moveHeader)
				{
					moveHeader.BM_RL_NKDestinationPortInfo.RefreshBinding();
				}
			}
		}
		protected bool BY_RN_NKCountryOfDestinationReadOnly => !IsPhase5 && BY_RN_NKCountryOfDestination.IsEmpty && MoveHeader is NctsDepartureMovementHeader moveHeader && !moveHeader.BM_RL_NKDestinationPort.IsEmpty;

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.CustomsUnitOfQuantityList))]
		public override ZString BY_CustomsUnitQty
		{
			get => base.BY_CustomsUnitQty;
			set => base.BY_CustomsUnitQty = value.ToUpper();
		}

		[MaxLength(1)]
		[ReadOnlyMember(nameof(BY_TransportChargesMethodOfPaymentReadOnly))]
		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.TransportChargesModeOfPaymentList))]
		[ResourceStringData("0B836C80-1A88-4B14-B6D9-8C4F9FD9961A", Caption = "Transport Charges / Method of Payment", MediumCaption = "Trans. Chg. MoP", ShortCaption = "MoP", FullDescription = "Method of Payment of Transport Charges", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("5039E9D3-0D54-47D9-8623-CAF6925B4486", Caption = "Transport Charges MoP", MediumCaption = "Transport Chg. MoP", ShortCaption = "Trans. Chg. MoP", FullDescription = "Method of Payment of Transport Charges", MultipleKey = NctsHeader.Phase5CaptionKey)]
		public override ZString BY_TransportChargesMethodOfPayment
		{
			get => base.BY_TransportChargesMethodOfPayment;
			set => base.BY_TransportChargesMethodOfPayment = value;
		}

		protected bool BY_TransportChargesMethodOfPaymentReadOnly => Bill?.IsRuleC0337Applied ?? false;

		protected int MaxLengthOfReferenceNumber => Header is { IsPhase5: true } ? 35 : AutoCusInBondCargoDesc.Schema.BY_CommercialReferenceNumberMaxLength;

		[MaxLength(nameof(MaxLengthOfReferenceNumber))]
		[ResourceStringData("590AAE1A-0BA0-4220-B3BB-446086B7621B", Caption = "Commercial Reference", MediumCaption = "Comm. Ref.", ShortCaption = "Ref.", MultipleKey = NctsHeader.Phase4CaptionKey)]
		[ResourceStringData("8FF79B92-E7EA-43C3-A50E-7018231DA7E1", Caption = "Commercial Reference", MediumCaption = "Commercial Ref.", ShortCaption = "Reference", MultipleKey = NctsHeader.Phase5CaptionKey)]
		[ResourceStringData("D5BBADCB-502B-4BC5-A28C-935839937465", Caption = "Reference Number / UCR", ShortCaption = "Ref. No. / UCR", FullDescription = "Indicate the Reference Number / Unique Consignment Reference (UCR)", MultipleKey = NctsHeader.Phase5DepartureCaptionKey)]
		public override ZString BY_CommercialReferenceNumber
		{
			get => base.BY_CommercialReferenceNumber;
			set => base.BY_CommercialReferenceNumber = value;
		}

		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.CusCodeList))]
		[ResourceStringData("C475BCBC-DB94-4DCD-BE00-01AA11143BFA", Caption = "CUS-Code")]
		public override ZString BY_CusC4Number
		{
			get => base.BY_CusC4Number;
			set => base.BY_CusC4Number = value;
		}

		public override ZDecimal BY_NetWeight
		{
			get => base.BY_NetWeight;
			set
			{
				var oldValue = BY_NetWeight;
				base.BY_NetWeight = value;
				if (!IsCopying && oldValue != BY_NetWeight)
				{
					CalculateCustomsFirstQtyFromNetWeight();
				}
			}
		}

		public override ZString BY_NetWeightUnit
		{
			get => base.BY_NetWeightUnit;
			set
			{
				var oldValue = BY_NetWeightUnit;
				base.BY_NetWeightUnit = value.ToUpper();
				if (oldValue != BY_NetWeightUnit && !IsCopying)
				{
					CalculateCustomsFirstQtyFromNetWeight();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.Parts))]
		[ResourceStringData("4F8BB872-21A7-4E96-A615-C79DD32B9E4B", Caption = "Product Code", MediumCaption = "Prod. Code", ShortCaption = "Product")]
		[ReadOnlyMember(nameof(IsDeclarationSentToWarehouse))]
		public override ZGuid BY_OP_Part
		{
			get => base.BY_OP_Part;
			set => base.BY_OP_Part = value;
		}

		[ResourceStringData("915D08D3-19A0-4200-BA36-593362BFD11F", Caption = "Warehouse Quantity", MediumCaption = "Warehouse Qty.", ShortCaption = "Whs. Qty.")]
		[ReadOnlyMember(nameof(IsDeclarationSentToWarehouse))]
		public override ZDecimal BY_BondedWhsQuantity
		{
			get => base.BY_BondedWhsQuantity;
			set => base.BY_BondedWhsQuantity = value;
		}

		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.BondedWhsUnitQtyList))]
		[ResourceStringData("A93B3C14-8B9F-48BB-AF54-CAC0094B8074", Caption = "Warehouse Unit of Quantity")]
		[ReadOnlyMember(nameof(IsDeclarationSentToWarehouse))]
		public override ZString BY_BondedWhsUnitQty
		{
			get => base.BY_BondedWhsUnitQty;
			set => base.BY_BondedWhsUnitQty = value;
		}

		[ResourceStringData("8B272162-12B5-459D-A3F5-5E84589A8D15", Caption = "Previous Entry Number", MediumCaption = "Prev. Entry No.", ShortCaption = "Prev. Entry")]
		[ReadOnlyMember(nameof(IsDeclarationSentToWarehouse))]
		public override ZString BY_WarehouseEntryNumber
		{
			get => base.BY_WarehouseEntryNumber;
			set => base.BY_WarehouseEntryNumber = value;
		}

		[ResourceStringData("FFF988DA-371A-4D95-9BA6-D524AA715082", Caption = "Previous Entry Line", MediumCaption = "Prev. Entry Line", ShortCaption = "Prev. Line")]
		[ReadOnlyMember(nameof(IsDeclarationSentToWarehouse))]
		public override ZShort BY_WarehouseEntryLineNo
		{
			get => base.BY_WarehouseEntryLineNo;
			set => base.BY_WarehouseEntryLineNo = value;
		}

		[ResourceStringData("91521067-717F-4E1A-A261-5717AA6DAA5A", Caption = "Warehouse Order Number", MediumCaption = "Whs. Order No.", ShortCaption = "Whs. Order")]
		[ReadOnlyMember(nameof(IsDeclarationSentToWarehouse))]
		public override ZString BY_BondedWHSOrderNumber
		{
			get => base.BY_BondedWHSOrderNumber;
			set => base.BY_BondedWHSOrderNumber = value;
		}

		[ResourceStringData("558F6D79-4397-4C26-9E97-31CD490DC84E", Caption = "Warehouse Order Line", MediumCaption = "Whs. Order Line", ShortCaption = "Whs. Line")]
		[ReadOnlyMember(nameof(IsDeclarationSentToWarehouse))]
		public override ZShort BY_BondedWHSOrderLineNumber
		{
			get => base.BY_BondedWHSOrderLineNumber;
			set => base.BY_BondedWHSOrderLineNumber = value;
		}

		[ResourceStringData("04D20383-A382-4D8F-B5BE-93730ED28E95", Caption = "Invoice or Commercial Price of Line Item", MediumCaption = "Line Price", ShortCaption = "Price")]
		public override ZDecimal BY_LinePrice
		{
			get => base.BY_LinePrice;
			set
			{
				var oldValue = base.BY_LinePrice;
				base.BY_LinePrice = value;
				if (oldValue != BY_LinePrice && !IsCopying)
				{
					UpdateMonetaryValue();
				}
			}
		}

		[ResourceStringData("23273CF4-5B56-4A0C-9C61-9B18A9C09E8A", Caption = "Line Price Currency", ShortCaption = "Currency")]
		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.LinePriceCurrencies))]
		public override ZString BY_RX_NKLinePriceCurrency
		{
			get => base.BY_RX_NKLinePriceCurrency;
			set
			{
				var oldValue = base.BY_RX_NKLinePriceCurrency;
				base.BY_RX_NKLinePriceCurrency = value;
				if (oldValue != BY_RX_NKLinePriceCurrency && !IsCopying)
				{
					UpdateMonetaryValue();
				}
			}
		}

		public void UpdateMonetaryValue()
		{
			if (IsPhase5Departure && Bill != null && Bill.Header.MovementHeader.BM_ValuationDate.IsEmpty && !IsUpdateMonetaryValueSuspended)
			{
				if (BY_LinePrice.IsEmpty || BY_LinePrice <= 0 || LinePriceCurrency is null)
				{
					BY_MonetaryValue = 0;
				}
				else
				{
					var currencyConverter = CurrencyConverter.New(Factory, ZDateTime.Today, ExchangeRateType.Customs, false);
					BY_MonetaryValue = currencyConverter.ConvertExact(new Money(BY_LinePrice, LinePriceCurrency), Bill.Header.Company.CustomsCurrency).Amount;
				}
			}
		}

		bool IsUpdateMonetaryValueSuspended => suspendUpdateMonetaryValue != 0;

		public IDisposable SuspendUpdateMonetaryValue()
		{
			return new DisposableAction(() => suspendUpdateMonetaryValue++, () => suspendUpdateMonetaryValue--);
		}
		byte suspendUpdateMonetaryValue;

		public JobDocAddress Consignor
		{
			get
			{
				if (consignorJobDocAddress == null || consignorJobDocAddress.IsDeleted)
				{
					consignorJobDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsignorJobDocAddressRequirement);
				}
				return consignorJobDocAddress;
			}
		}
		JobDocAddress consignorJobDocAddress;

		public JobDocAddress Consignee
		{
			get
			{
				if (consigneeJobDocAddress == null || consigneeJobDocAddress.IsDeleted)
				{
					if (consigneeJobDocAddress != null)
					{
						consigneeJobDocAddress.DocAddressChanged -= ConsigneeJobDocAddressChanged;
					}
					consigneeJobDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsigneeJobDocAddressRequirement);
					consigneeJobDocAddress.DocAddressChanged += ConsigneeJobDocAddressChanged;
					consigneeJobDocAddress.IgnoreValidationStatusError = true;
					consigneeJobDocAddress.SetReadOnlyIncludingChildren(IsConsigneeReadOnly);
				}
				return consigneeJobDocAddress;
			}
		}
		JobDocAddress consigneeJobDocAddress;

		bool IsConsigneeReadOnly => IsPhase5 && !IsInPhase5TransitionPeriod;

		protected virtual void ConsigneeJobDocAddressChanged(object sender, EventArgs e)
		{
		}

		public JobDocAddress SecurityConsignor
		{
			get
			{
				if (securityConsignorJobDocAddress == null || securityConsignorJobDocAddress.IsDeleted)
				{
					securityConsignorJobDocAddress = DocAddresses.FindOrCreateWithRequirement(SecurityConsignorJobDocAddressRequirement);
				}
				return securityConsignorJobDocAddress;
			}
		}
		JobDocAddress securityConsignorJobDocAddress;

		public JobDocAddress SecurityConsignee
		{
			get
			{
				if (securityConsigneeJobDocAddress == null || securityConsigneeJobDocAddress.IsDeleted)
				{
					securityConsigneeJobDocAddress = DocAddresses.FindOrCreateWithRequirement(SecurityConsigneeJobDocAddressRequirement);
				}
				return securityConsigneeJobDocAddress;
			}
		}
		JobDocAddress securityConsigneeJobDocAddress;

		#region IDocAddresses Implementation

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => true;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

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

		Security.SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Environment.Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.ConsignorDocumentaryAddress:
					return ConsignorJobDocAddressRequirement;
				case DocAddressType.ConsigneeAddress:
					return ConsigneeJobDocAddressRequirement;
				case DocAddressType.NotifyParty2:
					return SecurityConsignorJobDocAddressRequirement;
				case DocAddressType.NotifyParty3:
					return SecurityConsigneeJobDocAddressRequirement;
				default:
					return null;
			}
		}

		public JobDocAddressRequirement ConsignorJobDocAddressRequirement
		{
			get
			{
				if (consignorJobDocAddressRequirement == null)
				{
					consignorJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.Consignor)
					{
						CanOverride = ConsignorJobDocAddressCanOverride,
						ValidateOrganisationPK = ValidateConsignor,
					};
					JobDocAddressManager.AddRequirement(consignorJobDocAddressRequirement);
				}
				return consignorJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement consignorJobDocAddressRequirement;

		protected virtual bool ConsignorJobDocAddressCanOverride => false;

		protected virtual void ValidateConsignor(JobDocAddressValidation validation)
		{
		}

		public JobDocAddressRequirement ConsigneeJobDocAddressRequirement
		{
			get
			{
				if (consigneeJobDocAddressRequirement == null)
				{
					consigneeJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsigneeAddress, ContactType.NoContactType)
					{
						ValidateOrganisationPK = ValidateConsignee,
						CanOverride = ConsigneeJobDocAddressCanOverride,
						ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address = ValidateConsigneeAddress
					};
					JobDocAddressValidationHelper.ApplyRequirementForOverriddenValidation(consigneeJobDocAddressRequirement);
					JobDocAddressValidationHelper.ApplyRequirementForContactWorkPhoneValidation_TR0079(consigneeJobDocAddressRequirement);
					JobDocAddressManager.AddRequirement(consigneeJobDocAddressRequirement);
				}
				return consigneeJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement consigneeJobDocAddressRequirement;

		protected virtual bool ConsigneeJobDocAddressCanOverride => false;

		protected virtual void ValidateConsignee(JobDocAddressValidation validation)
		{
			SilentlyReportErrorIfHeaderIsNull(Header);

			var consignee = validation.Parent;
			var organisationPKInfo = consignee.OrganisationPKInfo;
			CheckConditionB1820_1Phase5(this, organisationPKInfo, consignee);
			CheckConditionB1820_2Phase5(this, organisationPKInfo);
			CheckConditionB1822(this, organisationPKInfo);
			CheckConditionB2400_1ForConsigneePhase5(this, organisationPKInfo);
			CheckConditionC002(this, organisationPKInfo);
			Header.CheckConditionC0505(consignee.OrganisationPKInfo, consignee);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "WTG internal only captions")]
		void SilentlyReportErrorIfHeaderIsNull(NctsHeader header)
		{
			if (header is null)
			{
				const string errorKey = "EU.NctsDepartureCargoDesc.ValidateConsignee: could not retrieve the 'Header' for this goods item (goodsItem.Header is null).";

				var errorMessageBuilder = new ZStringBuilder()
					.Append("See below debug information:")
					.AppendFormat("BY_ParentTableCode = '{0}'", BY_ParentTableCode)
					.AppendFormat("BY_ParentID = '{0}'", BY_ParentID.ToString())
					.AppendFormat("Is MoveHeaderOrBillParent null? {0}", MoveHeaderOrBillParent is null ? "Yes" : "No")
					.AppendFormat("Is MoveHeader null? {0}", MoveHeader is null ? "Yes" : "No")
					.AppendFormat("Is Bill null? {0}", Bill is null ? "Yes" : "No");

				ErrorReporter.ReportOnce(errorKey, errorMessageBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		void CheckConditionB1820_1Phase5(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info, JobDocAddress consignee)
		{
			if (goodsItem.Header is NctsHeader header
				&& header.Configuration.ValidationRuleConfiguration.IsRuleB1820_1Active
				&& goodsItem.IsPhase5
				&& header.Consignee.IsEmpty
				&& IsInPhase5TransitionPeriod
				&& (consignee.IsEmpty || consignee.IsOverridenButEmpty))
			{
				var movementHeader = header.MovementHeader;
				var codeListC0009 = ZZRefCusCodeListCombined.GetUniqueCodes(header.Factory, header.DefaultDataGroupingCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, ZDateTime.Today);
				var destinationIsC0009Code = codeListC0009.Contains(movementHeader.BM_RL_NKDestinationPort) || codeListC0009.Contains(goodsItem.BY_RN_NKCountryOfDestination);
				if (destinationIsC0009Code || (movementHeader.IsCombinedWithExit && !movementHeader.Has30600AdditionalInformation))
				{
					info.AddMessageError($"[{ValidationRuleCodeConstants.B1820_1}] {MandatoryValidation.YouHaveNotEnteredMessage(info.Description)}");
				}
			}
		}

		void CheckConditionB1820_2Phase5(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info)
		{
			if (goodsItem.Header is NctsHeader header
				&& IsRuleB1820_2Applicable
				&& goodsItem.IsPhase5
				&& !goodsItem.Consignee.IsEmpty)
			{
				var messageError = Res.GetString("6AA216F2-F10F-420B-ABAA-C40F283EA875", "[B1820-2] Consignee field must be empty");

				var movementHeader = header.MovementHeader;
				var codeListC0009 = UniversalLookupsHelper.GetCountryC0009List(header.Factory, header.DefaultDataGroupingCode);
				if (!header.Consignee.IsEmpty)
				{
					CheckConditionB1820_2ConsigneeEnteredAtConsignment(goodsItem, info, messageError);
				}

				if (!codeListC0009.ContainsCode(movementHeader.BM_RL_NKDestinationPort)
					&& !codeListC0009.ContainsCode(goodsItem.BY_RN_NKCountryOfDestination)
					&& !movementHeader.IsSecurityTypeNONOrENT
					&& Has30600AdditionalInfo(goodsItem))
				{
					info.AddMessageError(messageError);
				}
			}
		}

		bool IsRuleB1820_2Applicable => Header is NctsHeader header && header.Configuration.ValidationRuleConfiguration.IsRuleB1820_2Active && header.IsInPhase5TransitionPeriod;

		bool Has30600AdditionalInfo(NctsDepartureCargoDesc goodsItem)
			=> goodsItem.Header.AdditionalDocuments.Concat(goodsItem.AdditionalInfos).Has30600AdditionalInformation();

		void CheckConditionB1820_2ConsigneeEnteredAtConsignment(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info, string messageError)
		{
			var header = goodsItem.Header;
			var movementHeader = header.MovementHeader;
			var codeListC0009 = UniversalLookupsHelper.GetCountryC0009List(movementHeader.Factory, movementHeader.DefaultDataGroupingCode);
			if (codeListC0009.ContainsCode(movementHeader.BM_RL_NKDestinationPort)
				|| (!codeListC0009.ContainsCode(goodsItem.BY_RN_NKCountryOfDestination)
					&& (movementHeader.IsSecurityTypeNONOrENT || !Has30600AdditionalInfo(goodsItem))))
			{
				info.AddMessageError(messageError);
			}
		}

		void CheckConditionB1822(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info)
		{
			if (goodsItem?.Consignee.Address is OrgAddress address
				&& address.Postcode.IsEmpty
				&& goodsItem.MoveHeader is NctsDepartureMovementHeader moveHeader
				&& moveHeader.Header is NctsHeader header
				&& header.Configuration.ValidationRuleConfiguration.IsRuleB1822Active
				&& IsInPhase5TransitionPeriod
				&& !header.GetCountryCL505List().Contains(address.Country))
			{
				info.AddMessageError(Res.GetString("06DFE3A2-BA03-411F-A91A-4132704ED28C", "Consignment Item Consignee's post code is mandatory if the country is not from CL505."));
			}
		}

		void CheckConditionB2400_1ForConsigneePhase5(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info)
		{
			if (goodsItem.IsPhase5
				&& ValidationDecider is INctsDepartureCargoDescPhase5ValidationDecider validationDecider
				&& validationDecider.IsRuleB2400_1Active
				&& !IsInPhase5TransitionPeriod
				&& !info.Value.IsEmpty)
			{
				info.AddMessageError(Res.GetString("705AA588-CA8E-40D1-BE04-0E23AD007870", "[B2400-1] Consignee field must be empty"));
			}
		}

		void CheckConditionC002(NctsDepartureCargoDesc goodsItem, ZPropertyInfo info)
		{
			var countryOfDestination = goodsItem.BY_RN_NKCountryOfDestination;
			if (!IsPhase5
				&& !countryOfDestination.IsEmpty
				&& goodsItem.Consignee.OrganisationPK.IsEmpty
				&& NctsHeaderValidationHelper.IsCommonTransitCountry(countryOfDestination)
				&& goodsItem.MoveHeader is NctsDepartureMovementHeader moveHeader
				&& (moveHeader.Header?.Consignee.OrganisationPK.IsEmpty ?? false)
				)
			{
				info.AddMessageError(NctsHeaderValidationHelper.GetRuleExplanation(Rules_C_Conditions.Codes.C002, Res.GetString("F7C9C064-B823-4D3C-B628-DE7FEC9CFC6C", "Consignee Trader is required for this movement.")));
			}
		}

		protected virtual void ValidateConsigneeAddress(JobDocAddressValidation validation)
		{
			Header.CheckConditionE1102(Consignee.E2_OA_AddressInfo, Consignee, NctsConstants.Traders.ConsigneeCaption);
			new NctsTraderAddressPhase5DepartureValidator(Consignee, NctsConstants.Traders.ConsigneeCaption)
				.CheckRuleE1104_1(Header, Consignee.E2_OA_AddressInfo);
		}

		public JobDocAddressRequirement SecurityConsignorJobDocAddressRequirement
		{
			get
			{
				if (securityConsignorJobDocAddressRequirement == null)
				{
					securityConsignorJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.NotifyParty2, ContactType.Consignor)
					{
						ValidateOrganisationPK = ValidateSecurityConsignorTrader
					};
					JobDocAddressManager.AddRequirement(securityConsignorJobDocAddressRequirement);
				}
				return securityConsignorJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement securityConsignorJobDocAddressRequirement;

		void ValidateSecurityConsignorTrader(JobDocAddressValidation validation)
		{
			if (Header != null)
			{
				Header.SecurityConsignor.Validation.ValidateOrganisationPK();
			}
		}

		public JobDocAddressRequirement SecurityConsigneeJobDocAddressRequirement
		{
			get
			{
				if (securityConsigneeJobDocAddressRequirement == null)
				{
					securityConsigneeJobDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.NotifyParty3, ContactType.Consignee);
					JobDocAddressManager.AddRequirement(securityConsigneeJobDocAddressRequirement);
				}
				return securityConsigneeJobDocAddressRequirement;
			}
		}
		JobDocAddressRequirement securityConsigneeJobDocAddressRequirement;

		public JobDocAddressManager JobDocAddressManager => jobDocAddressManager ?? (jobDocAddressManager = new JobDocAddressManager());
		JobDocAddressManager jobDocAddressManager;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return new OrgHeaderCollection(Factory);
		}

		ZString IDocAddresses.HumanReadableName => ZString.Empty;

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new NctsJobDocAddressValidation(addressToValidate, this);
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new[]
		{
			DocAddressType.ConsignorDocumentaryAddress,
			DocAddressType.ConsigneeAddress,
			DocAddressType.NotifyParty2,
			DocAddressType.NotifyParty3
		};

		#endregion

		#region IUNDGDataItemProvider Members

		[ChildEditable(true)]
		public NctsUNDGDataItemCollection UNDGs
		{
			get
			{
				if (undgs == null)
				{
					undgs = new NctsUNDGDataItemCollection(this);
					InitialiseUNDGs();
					RegisterEditableChildObject(undgs);
				}

				if (Header != null && undgs.ReadOnly != Header.IsDepartureTabReadOnly)
				{
					undgs.SetReadOnlyIncludingChildren(Header.IsDepartureTabReadOnly);
				}

				return undgs;
			}
		}
		NctsUNDGDataItemCollection undgs;

		UNDGDataItemCollection IUNDGDataItemProvider.UNDGs => UNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		protected virtual void InitialiseUNDGs()
		{
			((IBindingList)undgs).ListChanged += delegate
			{
				UNDGsAsStringInfo.RefreshBinding();
			};
		}

		[ResourceStringData("24bdcb96-c50b-4a56-9d8c-815a7030f62b", Caption = "DG Substance", MediumCaption = "DG Subs.", ShortCaption = "DG")]
		public ZString UNDGsAsString => UNDGs.AsString;

		public ZPropertyInfo UNDGsAsStringInfo => GetZPropertyInfo(nameof(UNDGsAsString));

		[ResourceStringData("D34A0886-2C69-4926-9A93-84314375F2B0", Caption = "Dangerous Goods Substance")]
		public ZString UNDangerousGoodsCode => UNDGs.Count > 0 && UNDGs.FirstOrDefault()?.UNDGSubstance is UNDGSubstance substance ? substance.DG_Code : ZString.Empty;

		public ZString UNDangerousGoodsStandard => UNDGs.Count > 0 && UNDGs.FirstOrDefault()?.UNDGSubstance is UNDGSubstance substance ? substance.DG_Standard : ZString.Empty;

		#endregion

		#region ICusReferenceTypeSupporter Members

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
		{
			{ CusReferenceTypeList.Codes.SupplyChainActor, SupplyChainActorType },
		};

		protected virtual Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

		#endregion

		public override void Delete()
		{
			if (IsInDatabase)
			{
				Header.ResetGoodsItemNumbersWhenPhase5();
			}
			DeleteAllContainerPivots();
			base.Delete();
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (BY_DeclarationGoodsItemNumber.IsEmpty)
			{
				Header?.ResetGoodsItemNumbersWhenPhase5();
			}
		}

		public bool Has30600AdditionalInformation => Factory.GetValue(ref has30600AdditionalInformation, AdditionalInfos.Has30600AdditionalInformation);
		CachedProperty<bool> has30600AdditionalInformation;

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return base.GetPropertiesToExcludeFromCloning().Concat(new[] { Schema.BY_DeclarationGoodsItemNumber });
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newGoodsItem = (NctsDepartureCargoDesc)base.CloneInternal(args);

			newGoodsItem.PreviousDocuments.AddCloneFrom(PreviousDocuments, args);
			CloneUNDGs(newGoodsItem);
			CloneAdditionalSupplementaryCodes(newGoodsItem);
			CloneFees(newGoodsItem);
			CloneCusSupplyChainActorReferences(newGoodsItem);
			return newGoodsItem;
		}

		void CloneUNDGs(NctsDepartureCargoDesc newGoodsItem)
		{
			foreach (var undg in UNDGs)
			{
				var newUnDg = (UNDGDataItem)new NctsDeepCloneStrategy(undg, newGoodsItem.PK).Clone();
				newGoodsItem.UNDGs.Add(newUnDg);
			}
		}

		void CloneAdditionalSupplementaryCodes(NctsDepartureCargoDesc newGoodsItem)
		{
			foreach (var sourceSupplementaryCode in AdditionalSupplementaryCodes)
			{
				var newSupplementaryCode = (SupplementaryCode)new NctsDeepCloneStrategy(sourceSupplementaryCode, newGoodsItem.PK).Clone();
				newGoodsItem.AdditionalSupplementaryCodes.Add(newSupplementaryCode);
			}
		}

		void CloneFees(NctsDepartureCargoDesc newGoodsItem)
		{
			newGoodsItem.Fees.DeleteAll();
			foreach (var sourceFee in Fees)
			{
				var newFee = (NctsCargoDescFee)new NctsDeepCloneStrategy(sourceFee, newGoodsItem.PK).Clone();
				newGoodsItem.Fees.Add(newFee);
			}
		}

		void CloneCusSupplyChainActorReferences(NctsDepartureCargoDesc newGoodsItem)
		{
			foreach (var supplyChainActorReference in CusSupplyChainActorReferences)
			{
				var newCusSupplyChainActorReference = (CusSupplyChainActorReference)new NctsDeepCloneStrategy(supplyChainActorReference, newGoodsItem.PK).Clone();
				newGoodsItem.CusSupplyChainActorReferences.Add(newCusSupplyChainActorReference);
			}
		}

		#region CustomsFirstQuantity

		[ResourceStringData("8C1F176C-1190-404F-9E49-8D25282E8EB7", Caption = "[38] Customs Qty", MediumCaption = "Customs Qty", ShortCaption = "Cus. Qty")]
		[DecimalPlaces(6)]
		[MeasureUnit(nameof(CustomsFirstUnitQtyKilograms), MeasureUnitType.Unknown)]
		public ZDecimal CustomsFirstQuantityInKilograms
		{
			get => BY_CustomsQuantity;
			set
			{
				var oldValue = CustomsFirstQuantityInKilograms;
				BY_CustomsQuantity = value;
				BY_CustomsUnitQty = value == 0 ? string.Empty : RefCusCodeList.CustomsUq.Weight.Kilogram;
				CustomsFirstQuantityInKilogramsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CustomsFirstQuantityInKilogramsInfo => GetZPropertyInfo(nameof(CustomsFirstQuantityInKilograms));

		[List(nameof(Lookups) + "." + nameof(IDepartureCargoDescLookups.CustomsUnitOfQuantityList))]
		public ZString CustomsFirstUnitQtyKilograms => BY_CustomsUnitQty.IsEmpty ? (ZString)RefCusCodeList.CustomsUq.Weight.Kilogram : BY_CustomsUnitQty;

		public ZPropertyInfo CustomsFirstUnitQtyKilogramsInfo => GetZPropertyInfo(nameof(CustomsFirstUnitQtyKilograms));

		void CalculateCustomsFirstQtyFromNetWeight()
		{
			if (BY_NetWeight > 0m && Core.Constants.Weight.ContainsCode(BY_NetWeightUnit))
			{
				CustomsFirstQuantityInKilogramsInfo.Value = NetMassInKilograms.Round(6);
			}
		}

		#endregion

		#region CustomsThirdQuantity
		[DecimalPlaces(nameof(BY_CustomsThirdQuantityNumberOfDecimalPlaces))]
		public override ZDecimal BY_CustomsThirdQuantity
		{
			get => base.BY_CustomsThirdQuantity;
			set
			{
				base.BY_CustomsThirdQuantity = value;
				if (!IsPhase5)
				{
					UpdateAllFeesFromTariffRates();
				}
			}
		}

		int BY_CustomsThirdQuantityNumberOfDecimalPlaces => IsPhase5 ? Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForIndividualUnits : Schema.BY_CustomsThirdQuantityDecimalPlacesPhase4;

		protected override void DefaultUOMsExtraThirdUnitQty(ZString thirdUnit)
		{
			BY_CustomsThirdUnitQtyInfo.Value = thirdUnit.SubstringSafe(0, BY_CustomsThirdUnitQtyInfo.MaxLength);
		}
		#endregion

		public ITemporaryStorageRegisterTransactionDataProvider TemporaryStorageRegisterTransactionDataProvider => temporaryStorageRegisterTransactionDataProvider ??= GetNewTemporaryStorageRegisterTransactionDataProvider();
		ITemporaryStorageRegisterTransactionDataProvider temporaryStorageRegisterTransactionDataProvider;

		protected virtual ITemporaryStorageRegisterTransactionDataProvider GetNewTemporaryStorageRegisterTransactionDataProvider() => new NctsDepartureCargoDescPhase5TemporaryStorageRegisterTransactionDataProvider(this);

		public IEntryNumberFormatterForNctsAndDeclarationIntegration GetEntryNumberFormatter() => GetEntryNumberFormatterCore();
		protected virtual IEntryNumberFormatterForNctsAndDeclarationIntegration GetEntryNumberFormatterCore() => new EntryNumberFormatterForNctsAndDeclarationIntegration();

		public void SetDefaultTaxType() => SetDefaultTaxTypeCore();

		protected virtual void SetDefaultTaxTypeCore()
		{
			if (UniversalTariff == null)
			{
				BY_ZZF_NKTaxType = ZString.Empty;
			}
			else if (!BY_HarmonisedTariff.IsEmpty)
			{
				var effectiveTaxes = GetEffectiveTaxes();
				var defaultTaxOrFeeCode = effectiveTaxes.IsNullOrEmpty() ? UniversalTariff.GetDefaultTaxOrFeeCode(ValuationDate, DataGroupingCode) : effectiveTaxes.MaxBy(x => x.ZZF_Value).ZZF_Code;
				if (!defaultTaxOrFeeCode.IsEmpty)
				{
					BY_ZZF_NKTaxType = defaultTaxOrFeeCode;
				}
			}
		}

		protected virtual IEnumerable<RefCusTaxOrFee> GetEffectiveTaxes()
		{
			var effectiveVATApplicabilities = UniversalTariff?.GetEffectiveVATApplicabilities(ValuationDate).Where(x => x.ZX5_ZZZ_NKDataGrouping == DataGroupingCode).Select(x => x.ZX5_ZZF_NKTaxOrFeeCode).Distinct().ToArray();
			return RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(Factory, DataGroupingCode, ZString.Empty, ValuationDate).Where(x => effectiveVATApplicabilities?.Contains(x.ZZF_Code) ?? false);
		}

		protected override IValueSetStrategy GetValueSetStrategy() => valueSetStrategy ?? (valueSetStrategy = new NctsDepartureCargoDescValueSetStrategy(this));
		IValueSetStrategy valueSetStrategy;

		protected sealed override CusInBondCargoDescLookups GetNewLookups() => IsPhase5 ? GetNewPhase5Lookups() : GetNewPhase4Lookups();

		protected virtual NctsCommonCargoDescLookups GetNewPhase4Lookups() => new NctsDepartureCargoDescPhase4Lookups(this);

		protected virtual NctsCommonCargoDescLookups GetNewPhase5Lookups() => new NctsDepartureCargoDescPhase5Lookups(this);

		protected sealed override CusInBondCargoDescValidation GetNewValidation() => IsPhase5 ? GetNewPhase5Validation() : GetNewPhase4Validation();

		protected virtual NctsDepartureCargoDescPhase5Validation GetNewPhase5Validation() => new NctsDepartureCargoDescPhase5Validation(this);

		protected virtual NctsDepartureCargoDescPhase4Validation GetNewPhase4Validation() => new NctsDepartureCargoDescPhase4Validation(this);

		protected override IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			foreach (var fetchStrategy in base.GetAdditionalBusinessObjectFetchStrategies())
			{
				yield return fetchStrategy;
			}
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new NctsDepartureCargoDescFetchStrategy(this);

		#region tariff

		protected override ZDecimal OtherFeesAmount => base.OtherFeesAmount + GetFeeTotalAmount(ChargeType.Excise);

		public IReadOnlyCollection<SelectionStyle> GetTariffNomenclatureSelectionModes() => new[] { SelectionStyle.Subheading, SelectionStyle.EightCharNomenclature, SelectionStyle.Tariff };

		public new static class ChargeType
		{
			public static readonly ZString Excise = "EXC";
		}

		public bool IsDeclarationSentToWarehouse =>
			ReadOnlyWarehouseStatusCodes.Contains(MoveHeader?.BM_WarehouseTransactionStatus);

		IList<string> ReadOnlyWarehouseStatusCodes => new[]
		{
			WarehouseTransactionStatusList.Codes.OutwardCreated,
			WarehouseTransactionStatusList.Codes.OutwardCreatedPending,
			WarehouseTransactionStatusList.Codes.OutwardHolding,
			WarehouseTransactionStatusList.Codes.OutwardUpdated,
			WarehouseTransactionStatusList.Codes.OutwardUpdatedPending,
		};
	}

	#endregion

}
