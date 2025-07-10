using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobComInvoiceHeader
		: AutoJobComInvoiceHeader
		, Integration.Customs.DE.IJobComInvoiceHeader
		, IPreviousDocumentParentProvider
		, ISupportingDocumentMaster
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.Business.Declaration.JobComInvoiceHeader.Schema
		{
			public const string JZ_FreeOfCharge = nameof(JobComInvoiceHeader.JZ_FreeOfCharge);
			public const string AdditionalInfoDescription = nameof(JobComInvoiceHeader.AdditionalInfoDescription);
		}

		public new AddInfoJobComInvoiceHeader AddInfo => (AddInfoJobComInvoiceHeader)base.AddInfo;

		public new AddInfoJobComInvoiceHeaderLookups AddInfoLookups => (AddInfoJobComInvoiceHeaderLookups)base.AddInfoLookups;

		public new AddInfoJobComInvoiceHeaderValidation AddInfoValidation => (AddInfoJobComInvoiceHeaderValidation)base.AddInfoValidation;

		public new JobComInvoiceGroupHeader GroupHeader => (JobComInvoiceGroupHeader)base.GroupHeader;

		public new JobComInvoiceLineViewCollection JobComInvoiceLines => (JobComInvoiceLineViewCollection)base.JobComInvoiceLines;

		public new JobComInvoiceLineViewCollection InvoiceLines => JobComInvoiceLines;

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		public new PreviousDocumentCollection PreviousDocuments => (PreviousDocumentCollection)base.PreviousDocuments;

		public new SupportingDocumentCollection SupportingDocuments => (SupportingDocumentCollection)base.SupportingDocuments;

		public new JobComInvoiceHeaderLookups Lookups => (JobComInvoiceHeaderLookups)base.Lookups;

		public new JobComInvoiceHeaderValidation Validation => (JobComInvoiceHeaderValidation)GetNewValidation();

		public new JobComInvApportionedChargeCollection<InvoiceApportionCharge> GroupCharges => (JobComInvApportionedChargeCollection<InvoiceApportionCharge>)base.GroupCharges;

		public new InvoiceChargeCollection<InvoiceCharge> Charges => (InvoiceChargeCollection<InvoiceCharge>)base.Charges;

		#region Properties

		public override ZGuid JZ_JE
		{
			get => base.JZ_JE;
			set
			{
				if (!IsCopying && JZ_JE != value)
				{
					base.JZ_JE = value;
					SetDefaultJZ_UCR(IsExport);
				}
			}
		}

		public override ZGuid JZ_OA_ConsigneeAddress
		{
			get => base.JZ_OA_ConsigneeAddress;
			set
			{
				bool hasChanged = JZ_OA_ConsigneeAddress != value;
				base.JZ_OA_ConsigneeAddress = value;
				if (hasChanged)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(IsWarehouseAdjustment))]
		[ResourceStringData("0C3D2ECF-9D2E-4392-B825-4C6006FCF5C7", Caption = "Free of Charge")]
		public ZBool JZ_FreeOfCharge
		{
			get => AddInfo.ZG_FreeOfCharge;
			set => AddInfo.ZG_FreeOfCharge = value;
		}

		public ZPropertyInfo JZ_FreeOfChargeInfo => GetWrappedZPropertyInfo(Schema.JZ_FreeOfCharge, c => AddInfo.ZG_FreeOfChargeInfo);

		[ReadOnlyMember(nameof(JZ_Weight_ReadOnly))]
		public override ZDecimal JZ_Weight
		{
			get => base.JZ_Weight;
			set
			{
				var oldValue = JZ_Weight;
				base.JZ_Weight = value;
				if (!IsCopying && oldValue != JZ_Weight)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		bool JZ_Weight_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[MaxLength(nameof(AdditionalInfoDescriptionMaxLength))]
		public ZString AdditionalInfoDescription
		{
			get { return AdditionalInfos.Count == 0 ? ZString.Empty : AdditionalInfos[0].CSI_Description; }
			set
			{
				var oldValue = AdditionalInfoDescription;
				CheckMaximumLength(AdditionalInfoDescriptionInfo, value);
				if (oldValue != value)
				{
					if (value.IsEmpty)
					{
						AdditionalInfos.RemoveAndDeleteAll();
					}
					else
					{
						var additionalInfo = AdditionalInfos.Cast<AdditionalInfo>().FirstOrDefault() ?? AdditionalInfos.AddNew();
						additionalInfo.CSI_Description = value;
					}
				}
				AdditionalInfoDescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AdditionalInfoDescriptionInfo => GetZPropertyInfo(Schema.AdditionalInfoDescription);

		int AdditionalInfoDescriptionMaxLength => IsImport ? 300 : AdditionalInfo.Schema.DescriptionMaxLength;

		internal ZBool IsHighValueOvrd => JobDeclaration?.ZG_IsHighValueOvrd ?? ZBool.False;

		[ReadOnlyMember(nameof(JZ_InvoiceDate_ReadOnly))]
		public override ZDateTime JZ_InvoiceDate { get => base.JZ_InvoiceDate; set => base.JZ_InvoiceDate = value; }

		bool JZ_InvoiceDate_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(IsWarehouseAdjustment))]
		public override ZDecimal JZ_InvoiceAmount { get => base.JZ_InvoiceAmount; set => base.JZ_InvoiceAmount = value; }

		[ReadOnlyMember(nameof(IsWarehouseAdjustment))]
		public override ZString JZ_RX_NKInvoice_Currency
		{
			get => base.JZ_RX_NKInvoice_Currency;
			set
			{
				var oldValue = JZ_RX_NKInvoice_Currency;
				base.JZ_RX_NKInvoice_Currency = value;
				if (!IsCopying && oldValue != JZ_RX_NKInvoice_Currency)
				{
					InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(IsWarehouseAdjustment))]
		public override ZString JZ_IncoTerm
		{
			get => base.JZ_IncoTerm;
			set
			{
				base.JZ_IncoTerm = value;
				SetDefaultAgreedPlaceCodeIfNecessary(value);
			}
		}

		[ResourceStringData("19ACEA12-F5E6-4FDB-AC4A-91E04396C2F6", Caption = "Agreed Place")]
		[ReadOnlyMember(nameof(JZ_IncoTermPlace_ReadOnly))]
		public override ZString JZ_IncoTermPlace { get => base.JZ_IncoTermPlace; set => base.JZ_IncoTermPlace = value; }

		protected override bool JZ_IncoTermPlace_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR || base.JZ_IncoTermPlace_ReadOnly;

		[ReadOnlyMember(nameof(JZ_IncoTermDescription_ReadOnly))]
		[ResourceStringData("0359AA18-41DF-42A5-BAF2-C7CD4DB9E1E7", Caption = "Incoterm Description", MediumCaption = "Incoterm Desc.", ShortCaption = "Inco. Desc.")]
		public ZString JZ_IncoTermDescription
		{
			get => base.ZG_IncoTermDescription;
			set
			{
				if (JZ_IncoTermDescription != value)
				{
					base.ZG_IncoTermDescription = value;
					JZ_IncoTermDescriptionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo JZ_IncoTermDescriptionInfo => GetWrappedZPropertyInfo(nameof(JZ_IncoTermDescription), c => AddInfo.ZG_IncoTermDescriptionInfo);

		bool JZ_IncoTermDescription_ReadOnly => IsInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JZ_ValuationCode_ReadOnly))]
		public override ZString JZ_ValuationCode
		{
			get => base.JZ_ValuationCode;
			set
			{
				var oldValue = JZ_ValuationCode;
				base.JZ_ValuationCode = value;
				if (!IsCopying && JZ_ValuationCode != oldValue)
				{
					JobComInvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		bool JZ_ValuationCode_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JZ_WeightUQ_ReadOnly))]
		public override ZString JZ_WeightUQ { get => base.JZ_WeightUQ; set => base.JZ_WeightUQ = value; }

		bool JZ_WeightUQ_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR; 

		[ReadOnlyMember(nameof(JZ_NetWeight_ReadOnly))]
		public override ZDecimal JZ_NetWeight { get => base.JZ_NetWeight; set => base.JZ_NetWeight = value; }

		bool JZ_NetWeight_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JZ_NetWeightUQ_ReadOnly))]
		public override ZString JZ_NetWeightUQ { get => base.JZ_NetWeightUQ; set => base.JZ_NetWeightUQ = value; }

		bool JZ_NetWeightUQ_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(IsWarehouseAdjustment))]
		public override ZString ZG_TransportChargesMethodOfPayment { get => base.ZG_TransportChargesMethodOfPayment; set => base.ZG_TransportChargesMethodOfPayment = value; }

		[ReadOnlyMember(nameof(JZ_NoOfPacks_ReadOnly))]
		public override ZDecimal JZ_NoOfPacks { get => base.JZ_NoOfPacks; set => base.JZ_NoOfPacks = value; }

		bool JZ_NoOfPacks_ReadOnly => IsWarehouseAdjustmentOrInwardProcessingAVABR;

		[ReadOnlyMember(nameof(NoOfPacksPackType_ReadOnly))]
		public override ZString NoOfPacksPackType => base.NoOfPacksPackType;

		bool NoOfPacksPackType_ReadOnly => IsInwardProcessingAVABR;

		internal bool IsWarehouseAdjustment => JobDeclaration?.IsWarehouseAdjustment ?? false;

		internal bool IsInwardProcessingAVABR => JobDeclaration?.IsInwardProcessingAVABR ?? false;

		internal bool IsWarehouseAdjustmentOrInwardProcessingAVABR => IsWarehouseAdjustment || IsInwardProcessingAVABR;

		protected override bool IsJZ_InvoiceCurrExRateUserEnterable_ReadOnly => base.IsJZ_InvoiceCurrExRateUserEnterable_ReadOnly || IsInwardProcessingAVABR;

		public bool IsStockMovement => JobDeclaration?.IsStockMovement ?? false;

		static readonly ImmutableDictionary<ZString, ZString> AgreedPlaceCodeByIncoTerm = new Dictionary<ZString, ZString>()
		{
			{ IncotermA1840CodeList.Codes.CFR, UniversalReferenceConstants.AgreedPlaceCodes._3 },
			{ IncotermA1840CodeList.Codes.CIF, UniversalReferenceConstants.AgreedPlaceCodes._3 },
			{ IncotermA1840CodeList.Codes.CIP, UniversalReferenceConstants.AgreedPlaceCodes._3 },
			{ IncotermA1840CodeList.Codes.CPT, UniversalReferenceConstants.AgreedPlaceCodes._3 },
			{ IncotermA1840CodeList.Codes.DAP, UniversalReferenceConstants.AgreedPlaceCodes._3 },
			{ IncotermA1840CodeList.Codes.DAT, UniversalReferenceConstants.AgreedPlaceCodes._3 },
			{ IncotermA1840CodeList.Codes.DDP, UniversalReferenceConstants.AgreedPlaceCodes._3 },
			{ IncotermA1840CodeList.Codes.DPU, UniversalReferenceConstants.AgreedPlaceCodes._3 },
			{ IncotermA1840CodeList.Codes.EXW, UniversalReferenceConstants.AgreedPlaceCodes._1 },
			{ IncotermA1840CodeList.Codes.FAS, UniversalReferenceConstants.AgreedPlaceCodes._1 },
			{ IncotermA1840CodeList.Codes.FCA, UniversalReferenceConstants.AgreedPlaceCodes._1 },
			{ IncotermA1840CodeList.Codes.FOB, UniversalReferenceConstants.AgreedPlaceCodes._1 },
			{ Core.Constants.IncoTerms.Other, string.Empty }
		}.ToImmutableDictionary();

		#endregion

		#region Default Values

		protected override void RefreshDefaultsWhenInvoiceAttachedToDeclarationCore()
		{
			base.RefreshDefaultsWhenInvoiceAttachedToDeclarationCore();
			var declaration = JobDeclaration;
			if (declaration != null)
			{
				DefaultValuesBasedOnDeclarationMessageType();
			}
		}

		public void DefaultValuesBasedOnDeclarationMessageType()
		{
			SetDefaultJZ_UCR(IsExport);
		}

		void SetDefaultJZ_UCR(ZBool isExport)
		{
			if (isExport)
			{
				if (JZ_UCR.IsEmpty)
				{
					JZ_UCR = JobDeclaration?.JE_UCR ?? ZString.Empty;
				}
			}
			else
			{
				JZ_UCR = ZString.Empty;
			}
		}

		void SetDefaultAgreedPlaceCodeIfNecessary(ZString incoTerm)
		{
			if (IsImport && AgreedPlaceCodeByIncoTerm.TryGetValue(incoTerm, out var agreedPlaceCode))
			{
				ZG_AgreedPlaceCode = agreedPlaceCode;
			}
		}

		#endregion

		#region Override

		protected override BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
		{
			var dec = JobDeclaration;
			return dec != null ? new JobComInvoiceLineViewCollection(this, dec.InvoiceLines) : null;
		}

		protected override ZString LocalCurrencyCodeCore => JobDeclaration.LocalCurrencyConstantCode;

		protected override BaseJobComInvoiceLineViewCollection CreateNewInvoiceLineCollectionWhenDeclarationIsNull()
		{
			var collection = new InvoiceLineDependentCollection(this);
			collection.Load();
			return new JobComInvoiceLineViewCollection(this, collection);
		}

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		protected override EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this, false);

		protected override EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = base.GetCusSupportingInfoTypes();
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			return result;
		}

		protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups() => new JobComInvoiceHeaderLookups(this);

		protected override EU.Business.Declaration.AddInfoJobComInvoiceHeader GetNewAddInfo() => new AddInfoJobComInvoiceHeader(JZ_AddInfoInfo);

		public PreviousDocumentMaster PreviousDocumentMaster => previousDocumentMaster ?? (previousDocumentMaster = new PreviousDocumentMaster(Factory, this));
		PreviousDocumentMaster previousDocumentMaster;

		protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
		{
			JobComInvoiceHeaderValidation result;
			if (IsStockMovement)
			{
				result = new StockMovementJobComInvoiceHeaderValidation(this);
			}
			else if (IsImport)
			{
				result = new ImportJobComInvoiceHeaderValidation(this);
			}
			else if (IsWarehouseAdjustment)
			{
				return new WarehouseAdjustmentJobComInvoiceHeaderValidation(this);
			}
			else if (IsExport)
			{
				result = new ExportJobComInvoiceHeaderValidation(this);
			}
			else
			{
				result = new JobComInvoiceHeaderValidation(this);
			}

			return result;
		}

		JobDeclaration IPreviousDocumentParentProvider.JobDeclaration => (JobDeclaration)base.JobDeclaration;

		HugeSequenceNumberGenerator ISupportingDocumentMaster.LineNumberGenerator => lineNumberGenerator ?? (lineNumberGenerator = new HugeSequenceNumberGenerator(() => new TypedEnumerable<IHugeSequenceNumberLine>(SupportingDocuments)));
		HugeSequenceNumberGenerator lineNumberGenerator;

		protected override IJobComInvApportionedChargeCollection<BaseApportionedCharge> CreateNewApportionedChargeCollection() => new JobComInvApportionedChargeCollection<InvoiceApportionCharge>(this);

		protected override IJobComInvChargeCollection<BaseInvoiceCharge> CreateNewJobComInvHeaderCharges() => new InvoiceChargeCollection<InvoiceCharge>(this);

		protected override string GetStandaloneIncoTermAndChargeFactoryCountryContext() => base.GetStandaloneIncoTermAndChargeFactoryCountryContext() + this.GetIncoTermChargeFactoryCacheKey();

		protected override ZBool NeedAtLeastOneInvoiceSupportingDocumentCore => base.NeedAtLeastOneInvoiceSupportingDocumentCore && !IsStockMovement;

		protected override ZAddress GetNewJZ_OA_ConsigneeAddress_ZAddress()
		{
			var result = base.GetNewJZ_OA_ConsigneeAddress_ZAddress();
			result.GetDefaultAddress = new ZAddress.DefaultAddressHandler(GetDefaultAddress);
			return result;

			ZGuid GetDefaultAddress(IOrgHeader orgHeader)
			{
				var addressPK = ZGuid.Empty;
				if (IsExport && orgHeader is OrgHeader organisation)
				{
					addressPK = organisation.MainAddress.PK;
				}
				return addressPK;
			}
		}

		#endregion

		[ReadOnlyMember(nameof(ZG_AgreedPlaceCode_ReadOnly))]
		public override ZString ZG_AgreedPlaceCode
		{
			get => base.ZG_AgreedPlaceCode;
			set
			{
				var oldValue = ZG_AgreedPlaceCode;
				base.ZG_AgreedPlaceCode = value;
				if (oldValue != ZG_AgreedPlaceCode
					&& !ZG_AgreedPlaceCode.IsEmpty
					&& !oldValue.IsEmpty)
				{
					JobDeclaration.InvHeaderZG_AgreedPlaceCodeValueChanged(this, EventArgs.Empty);
				}
			}
		}

		bool ZG_AgreedPlaceCode_ReadOnly => IsInwardProcessingAVABR;

		[ReadOnlyMember(nameof(JZ_InvoiceCurrLandedCostExRate_ReadOnly))]
		public override ZDecimal JZ_InvoiceCurrLandedCostExRate { get => base.JZ_InvoiceCurrLandedCostExRate; set => base.JZ_InvoiceCurrLandedCostExRate = value; }

		bool JZ_InvoiceCurrLandedCostExRate_ReadOnly => IsInwardProcessingAVABR;

		protected override CustomsValuationCalculator GetValuationCalculatorCore() => new DeCustomsValuationCalculator(this);

		internal ZZRefCusCodeListCombined ValuationRefCusCodeList
		{
			get
			{
				var cacheKey = string.Join("|", "DE.JobComInvoiceHeader.ValuationRefCusCodeList", JZ_ValuationCode, JZ_MessageType);
				return Factory.GetCachedValue(cacheKey, GetValuationRefCusCodeList);
			}
		}

		ZZRefCusCodeListCombined GetValuationRefCusCodeList()
		{
			var attributeFilters = new[] { new RefCusCodeListAttributeFilter(UniversalReferenceConstants.RefCusCodeListAttributes.Name.C0091, JoinCondition.Or, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes),
											new RefCusCodeListAttributeFilter(UniversalReferenceConstants.RefCusCodeListAttributes.Name.A1150, JoinCondition.Or, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes) };
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, JZ_ValuationCode, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, ZDateTime.Today, attributeFilters: attributeFilters, includeParentDataGrouping: false);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new[] { nameof(JZ_ValuationDateOverride) });
			return base.CloneInternal(args);
		}

		public void CopyConsigneeAddressFromDeclarationImporter()
		{
			var declaration = JobDeclaration;
			if (declaration.IsExport)
			{
				var importer = declaration.ImporterDocumentaryAddress;
				var importerAddress = importer.E2_OA_Address;
				if (importerAddress.IsValid || importerAddress.IsEmpty)
				{
					JZ_OA_ConsigneeAddress = importerAddress;
					ConsigneeAddressOrgPK = importer.OrganisationPK;
				}
			}
		}

		public override ZBool AddingSupportingDocumentAutomaticallyEnabled => !IsInwardProcessingAVABR ? base.AddingSupportingDocumentAutomaticallyEnabled : false;
	}
}
