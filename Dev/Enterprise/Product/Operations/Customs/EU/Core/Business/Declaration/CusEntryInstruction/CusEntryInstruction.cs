using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusEntryInstruction : AutoCusEntryInstruction
		, IAddInfoManager
		, Integration.Customs.ICusCodeDataTypeSupporter
		, Integration.Customs.ICusSupportingInfoTypeSupporter
		, ICusReferenceTypeSupporter
		, ICanBeImportOrExport
		, ICusFiscalReferenceProviderWithValidationDecider
		, ICusAuthorizationUsageMaster
		, ISupportMultipleResourceStringData
		, ICusGoodsLocationProvider
		, IFirstPlaceOfUseOrProcessingProvider
		, IUcc6ValueProvider
		, IValidationModesSupporter
		, IAdditionalInfosProviderWithValidationDecider
		, ISupportingDocumentsProviderWithValidationDecider
		, IPreviousDocumentsProviderWithValidationDecider
		, IDocAddresses
		, ISequenceNumberHeader
		, ISynchroniserReadOnlyMembersProvider
		, ICusAuthorizationUsageProviderWithValidationDecider
		, IRequestedDocumentsProvider
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusEntryInstruction.Schema
		{
			public const string WarehouseIDFor27 = nameof(CusEntryInstruction.WarehouseIDFor27);
			public const string CustomsDocStatus = nameof(CusEntryInstruction.CustomsDocStatus);
			public const string CustomsDocStatusDesc = nameof(CusEntryInstruction.CustomsDocStatusDesc);
			public const string FromWarehouseCode = nameof(CusEntryInstruction.FromWarehouseCode);
			public const string ToWarehouseCode = nameof(CusEntryInstruction.ToWarehouseCode);
			public const string FromWarehouseType = nameof(CusEntryInstruction.FromWarehouseType);
			public const string ToWarehouseType = nameof(CusEntryInstruction.ToWarehouseType);
			public const string ProcedureDescription = nameof(CusEntryInstruction.ProcedureDescription);
		}

		public new static readonly CusEntryInstructionTypeDecider TypeDecider = new CusEntryInstructionTypeDecider();

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

		public IEnumerable<JobComInvoiceHeader> Invoices => InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.InvoiceHeader != null).Select(x => x.InvoiceHeader).Distinct();

		public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

		public FiscalRepresentativeDefaulter FiscalRepresentativeDefaulter => GetFiscalRepresentativeDefaulterCore();
		protected virtual FiscalRepresentativeDefaulter GetFiscalRepresentativeDefaulterCore() => new FiscalRepresentativeDefaulter();

		public ZPropertyInfo FromWarehouseCodeInfo => GetZPropertyInfo(Schema.FromWarehouseCode);

		public virtual ZString FromWarehouseType => Factory.GetValue(ref fromWarehouseTypeCached, GetFromWarehouseType);
		CachedProperty<ZString> fromWarehouseTypeCached;

		protected virtual ZString GetFromWarehouseType() => FromWarehouseAuthorizationUsage?.AGC_Code ?? ZString.Empty;

		public ZPropertyInfo FromWarehouseTypeInfo => GetZPropertyInfo(Schema.FromWarehouseType);

		protected CusAuthorizationUsage FromWarehouseAuthorizationUsage
		{
			get
			{
				var authorisations = FromWarehouseAuthorizationUsages;
				return authorisations.Count == 1 ? authorisations[0] : null;
			}
		}

		public IReadOnlyList<CusAuthorizationUsage> FromWarehouseAuthorizationUsages => Factory.GetValue(ref fromWarehouseAuthorizationUsagesCached, () => GetAuthorizationUsages(CEI_OA_Warehouse, WarehouseAuthorisationTypes));
		CachedProperty<IReadOnlyList<CusAuthorizationUsage>> fromWarehouseAuthorizationUsagesCached;

		public ZPropertyInfo ToWarehouseCodeInfo => GetZPropertyInfo(Schema.ToWarehouseCode);

		public virtual ZString ToWarehouseType => Factory.GetValue(ref toWarehouseTypeCached, GetToWarehouseType);
		CachedProperty<ZString> toWarehouseTypeCached;

		protected virtual ZString GetToWarehouseType() => ToWarehouseAuthorizationUsage?.AGC_Code ?? ZString.Empty;

		public ZPropertyInfo ToWarehouseTypeInfo => GetZPropertyInfo(Schema.ToWarehouseType);

		protected CusAuthorizationUsage ToWarehouseAuthorizationUsage
		{
			get
			{
				var authorisations = ToWarehouseAuthorizationUsages;
				return authorisations.Count == 1 ? authorisations[0] : null;
			}
		}

		public IReadOnlyList<CusAuthorizationUsage> ToWarehouseAuthorizationUsages => Factory.GetValue(ref toWarehouseAuthorizationUsagesCached, () => GetAuthorizationUsages(CEI_OA_Warehouse2, WarehouseAuthorisationTypes));
		CachedProperty<IReadOnlyList<CusAuthorizationUsage>> toWarehouseAuthorizationUsagesCached;

		protected virtual IReadOnlyList<ZString> WarehouseAuthorisationTypes => new ZString[] { CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP };

		protected CusAuthorisationHeader[] GetAuthorisationHeaders(ZGuid appliesTo, IReadOnlyList<ZString> authorisationTypes)
		{
			var customsCountry = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(CountryCode);
			var date = ZDateTime.Today;
			var key = $"GetAuthorisationHeaders|{customsCountry}|{date.ToISO8601ShortDateString()}|{appliesTo.ToStringKey()}|{string.Join("_", authorisationTypes.OrderBy(x => x))}";
			return Factory.GetCachedValue(key, () => CusAuthorisationHeader.Loader.GetAuthorisationsForAddresses(Factory, customsCountry, authorisationTypes.ToArray(), date, new[] { appliesTo }));
		}

		protected CusAuthorizationUsage[] GetAuthorizationUsages(ZGuid appliesTo, IReadOnlyList<ZString> authorisationTypes)
		{
			CusAuthorizationUsage[] result = null;
			if (appliesTo.IsValid)
			{
				var usages = CusAuthorizationUsages.Where(x => authorisationTypes.Contains(x.AGC_Code)).ToArray();
				if (usages.Length > 0)
				{
					var permitHolders = GetAuthorisationHeaders(appliesTo, authorisationTypes).Select(x => x.CPH_OH_PermitHolder).ToHashSet();
					if (permitHolders.Count > 0)
					{
						result = usages.Where(x => permitHolders.Contains(x.AGC_OH_Owner)).ToArray();
					}

					var hasHeader = result?.Length > 0;
					if (!hasHeader && Factory.Load<OrgAddress>(appliesTo) is OrgAddress warehouseAddress)
					{
						var warehouseOrgPK = warehouseAddress.OA_OH;
						result = usages.Where(x => x.AGC_OH_Owner == warehouseOrgPK).ToArray();
					}
				}
			}
			return result ?? Array.Empty<CusAuthorizationUsage>();
		}

		public override ZGuid CEI_JE
		{
			get => base.CEI_JE;
			set
			{
				var oldValue = CEI_JE;
				base.CEI_JE = value;
				if (!IsCopying && oldValue != CEI_JE)
				{
					EmptyGuaranteesIfNecessary();
					JobDeclaration?.MarkAsNeedingValidation();
					JobDeclaration?.MarkInvoicesAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("0795C05C-E05D-46BC-AA2F-915CCDB835CA", ShortCaption = "Inner Pack.", Caption = "Total Inner Packages", FullDescription = "This field is used to communicate the number of inner packages to the Transit Warehouse only.")]
		public override ZInt CEI_TotalInnerPackages
		{
			get => base.CEI_TotalInnerPackages;
			set => base.CEI_TotalInnerPackages = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.StyleList))]
		[ResourceStringData("EU.CEI_Style", Caption = "Declaration Type")]
		public override ZString CEI_Style
		{
			get => base.CEI_Style;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_Style))
				{
					if (CEI_Style != value)
					{
						base.CEI_Style = value;
						JobDeclaration?.MarkAsNeedingValidation();
						JobDeclaration?.MarkInvoicesAsNeedingValidation();
						JobDeclaration?.MarkInvoiceLinesAsNeedingValidation();
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.EntrySubStyleList))]
		[ResourceStringData("bf48208d-ff09-4b21-9e7b-d13f7a11decd", Caption = "Sub Style")]
		[ResourceStringData("3F6FBF18-130D-409C-BAE1-66EF5AAEF9F2", Caption = "Sub Style", FullDescription = "[11 02 001 000] Additional Declaration Type", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString CEI_SubStyle
		{
			get => base.CEI_SubStyle;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_SubStyle))
				{
					if (CEI_SubStyle != value)
					{
						base.CEI_SubStyle = value;
						JobDeclaration?.MarkAsNeedingValidation();
						JobDeclaration?.MarkInvoicesAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("AB7B4DB5-5172-40EF-ACAD-2CDA53545ADA", ShortCaption = "Procedure", MediumCaption = "Req. Procedure", Caption = "Requested Procedure", FullDescription = "[11 09 001 000] Requested Procedure", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ProcedureCodeList))]
		public override ZString CEI_Procedure
		{
			get => base.CEI_Procedure;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_Procedure))
				{
					var oldValue = CEI_Procedure;
					base.CEI_Procedure = value;
					if (!IsCopying && oldValue != CEI_Procedure)
					{
						ApplyProcedureToInvoiceLines();
						JobDeclaration?.MarkInvoicesAsNeedingValidation();
					}
				}
			}
		}

		[ResourceStringData("C73575CC-C239-4B8B-82E4-F7C764C978AB", ShortCaption = "Procedure Desc.", MediumCaption = "Req. Procedure Description", Caption = "Requested Procedure Description", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		[ReadOnly(true)]
		public ZString ProcedureDescription => Lookups.ProcedureCodeList.GetDescriptionFromCode(CEI_Procedure);

		public ZPropertyInfo ProcedureDescriptionInfo => GetZPropertyInfo(Schema.ProcedureDescription);

		void ApplyProcedureToInvoiceLines()
		{
			if (IsRequestedProcedureValid)
			{
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					line.SetFirst2CharactersOfJI_Procedure(CEI_Procedure);
				}
			}
		}

		public ZString CEI_Calc_RequestedProcedure => InvoiceLines.Cast<JobComInvoiceLine>().FirstOrDefault()?.JI_Calc_RequestedProcedure ?? ZString.Empty;

		#region ICusGoodsLocationProvider

		[ReadOnlyMember(nameof(IsGoodsLocationReadOnly))]
		public CusGoodsLocation GoodsLocation
		{
			get
			{
				if (goodsLocation == null)
				{
					goodsLocation = GetGoodsLocation();
					RegisterEditableChildObject(goodsLocation);
					SetGoodsLocationReadOnly();
				}
				return goodsLocation;
			}
		}
		CusGoodsLocation goodsLocation;

		protected virtual bool IsGoodsLocationReadOnly => false;

		public void SetGoodsLocationReadOnly()
		{
			goodsLocation?.SetReadOnlyIncludingChildren(IsGoodsLocationReadOnly);
		}

		protected virtual CusGoodsLocation GetGoodsLocation() => Customs.Business.CusGoodsLocation.LoadOrCreate<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.EntryInstruction);

		public bool IsInnerGoodsLocationActive => IsInnerGoodsLocationActiveCore;

		protected virtual bool IsInnerGoodsLocationActiveCore => true;

		public bool HasLoadedGoodsLocation => goodsLocation != null;

		[ResourceStringData("FEFFFC45-96A1-465F-8C93-FDBA80B5CE5A", Caption = "Location of Goods")]
		public virtual ZString GoodsLocationDescription
		{
			get
			{
				if (goodsLocation == null)
				{
					goodsLocation = GetCusGoodsLocation();
					if (goodsLocation != null)
					{
						RegisterEditableChildObject(goodsLocation);
					}
				}
				return goodsLocation?.DisplayText ?? ZString.Empty;
			}
		}

		protected virtual CusGoodsLocation GetCusGoodsLocation() => Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(this, CusGoodsLocationUseList.Codes.EntryInstruction);

		public ZPropertyInfo GoodsLocationDescriptionInfo => GetZPropertyInfo(nameof(GoodsLocationDescription));

		void ICusGoodsLocationProvider.ValidateGoodsLocationDescription()
		{
			Validation.ValidateGoodsLocationDescription();
		}

		ZString ICusGoodsLocationProvider.ProviderKey => CountryCode + GoodsLocationProviderApplications.Codes.JobDeclaration;

		CusGoodsLocation ICusGoodsLocationProvider.GoodsLocation => GoodsLocation;

		ZString ICusGoodsLocationProvider.GoodsLocationDescription => GoodsLocationDescription;

		ZPropertyInfo ICusGoodsLocationProvider.GoodsLocationDescriptionInfo => GoodsLocationDescriptionInfo;

		#endregion

		public virtual OrgAddress WarehouseFor27 => HasIntoWarehouseProcedure ? Warehouse2 : (HasOutOfWarehouseProcedure ? Warehouse : null);

		// DE 2/7
		public virtual ZString WarehouseIDFor27
		{
			get
			{
				var warehouse = WarehouseFor27;
				return warehouse?.Header?.CustomsCodes?.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, warehouse.OA_RN_NKCountryCode, warehouse.PK) ?? ZString.Empty;
			}
		}

		[ResourceStringData("28FBE202-F691-41A7-BA3B-5DBB50E5FB3B", Caption = "Seals Quantity")]
		public override ZInt ZG_SealsCount { get => AddInfo.ZG_SealsCount; set => AddInfo.ZG_SealsCount = value; }

		public bool HasHeaderLevelPreviousDocuments() => Factory.GetValue(ref hasHeaderLevelPreviousDocumentsCached, () =>
			{
				return PreviousDocuments.Count > 0
					|| Invoices.Any(x => x.PreviousDocuments.Count > 0);
			});
		CachedProperty<bool> hasHeaderLevelPreviousDocumentsCached;

		[ChildEditable(true)]
		public PreviousDocumentCollection PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					previousDocuments = CreateNewPreviousDocumentCollection();
					previousDocuments.Load();
					RegisterEditableChildObject(previousDocuments);
				}
				return previousDocuments;
			}
		}
		PreviousDocumentCollection previousDocuments;

		protected virtual PreviousDocumentCollection CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection(this);

		PreviousDocumentCollection IPreviousDocumentsProvider.PreviousDocuments => PreviousDocuments;

		IPreviousDocumentValidationDecider IPreviousDocumentsProviderWithValidationDecider.ValidationDecider => JobDeclaration?.Configuration?.InstructionConfiguration?.GetPreviousDocumentValidationDecider(this);

		#region SupportingDocuments

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = CreateNewSupportingDocumentCollection();
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}
				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		protected virtual SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		ISupportingDocumentCollection<SupportingDocument> ISupportingDocumentsProvider.SupportingDocuments => SupportingDocuments;

		ISupportingDocumentValidationDecider ISupportingDocumentsProviderWithValidationDecider.ValidationDecider => JobDeclaration?.Configuration?.InstructionConfiguration?.GetSupportingDocumentValidationDecider(this);

		#endregion

		#region AdditionalInfos

		IAdditionalInfoCollection<AdditionalInfo> IAdditionalInfosProvider.AdditionalInfos => AdditionalInfos;

		IAdditionalInfoValidationDecider IAdditionalInfosProviderWithValidationDecider.ValidationDecider => JobDeclaration?.Configuration?.InstructionConfiguration?.GetAdditionalInfoValidationDecider(this);

		#endregion

		SchemaGuidColumn ICusAuthorizationUsageMaster.FKSchemaColumnInDependent => CusAuthorizationUsageSchema.AGC_ParentID;

		[ChildEditable]
		public ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction> CusAuthorizationUsages
		{
			get
			{
				if (cusAuthorizationUsages == null)
				{
					cusAuthorizationUsages = GetCusAuthorizationUsages();
					cusAuthorizationUsages.Load();
					cusAuthorizationUsages.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(cusAuthorizationUsages);
				}
				return cusAuthorizationUsages;
			}
		}
		ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction> cusAuthorizationUsages;

		public OrgHeader OldOwner => JobDeclaration?.Importer;

		protected virtual ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction> GetCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>(this, Factory);

		internal void CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsage(CusAuthorizationUsage authUsage) => CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCore(authUsage);
		protected virtual void CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsageCore(CusAuthorizationUsage authUsage)
		{
			if (InvoiceLines.Any() && !authUsage.AGC_Code.IsEmpty)
			{
				var date = InvoiceLines[0].EffectiveAssessmentDate;
				if (GetCusAuthorizationHeader(authUsage, date) is CusAuthorisationHeader header)
				{
					var supportingDocCodes = GetSupportingDocTypesForAuthorisation(authUsage.AGC_Code);
					foreach (var docCode in supportingDocCodes)
					{
						foreach (JobComInvoiceLine line in InvoiceLines)
						{
							if (!line.SupportingDocuments.Where(x => x.CSI_Code == docCode).Any())
							{
								var newDoc = line.SupportingDocuments.AddNew();
								newDoc.SetPropertiesFromCusAuthorisationHeader(header, docCode);
							}
						}
					}
				}
			}
		}

		internal List<ZString> GetSupportingDocTypesForAuthorisation(string authCode)
		{
			var results = new List<ZString>();

			if (InvoiceLines.Any())
			{
				var parent = InvoiceLines[0] as ICanBeImportOrExport;
				var authCodeFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.AuthCode, JoinCondition.And, authCode);
				var collection = Factory.GetSupportingDocumentList(parent.DataGroupingCode, parent.Direction(), parent.Level, new[] { authCodeFilter });
				collection.Load();
				results = collection.Select(x => x.ZZD_Code).ToList();
			}

			return results;
		}

		protected internal virtual CusAuthorisationHeader GetCusAuthorizationHeader(CusAuthorizationUsage authUsage, ZDateTime effectiveDate)
		{
			var auths = CusAuthorisationHeader.Loader.GetAuthorisations(Factory, CountryCode, new[] { authUsage.AGC_Code }, effectiveDate, authUsage.AGC_OH_Owner);

			return auths.Length == 1 ? auths[0] : null;
		}

		[ChildEditable]
		public INonPersistentCusDV1DetailPivotCollection<NonPersistentCusDV1DetailPivot> DV1DetailsPivots
		{
			get
			{
				if (cusDV1DetailsPivots == null)
				{
					cusDV1DetailsPivots = GetNewDV1DetailsPivotCollection();
					RegisterEditableChildObject(cusDV1DetailsPivots);
				}
				return cusDV1DetailsPivots;
			}
		}

		protected virtual INonPersistentCusDV1DetailPivotCollection<NonPersistentCusDV1DetailPivot> GetNewDV1DetailsPivotCollection()
		{
			return new NonPersistentCusDV1DetailPivotCollection<CusEntryInstruction, NonPersistentCusDV1DetailPivot>(this, e => new (e));
		}

		INonPersistentCusDV1DetailPivotCollection<NonPersistentCusDV1DetailPivot> cusDV1DetailsPivots;

		[ChildEditable(true)]
		public ICusFiscalReferenceCollection<CusFiscalReference> FiscalReferences
		{
			get
			{
				if (cusFiscalReferences == null)
				{
					cusFiscalReferences = GetNewFiscalReferenceCollection();
					cusFiscalReferences.Load();
					RegisterEditableChildObject(cusFiscalReferences);
				}
				return cusFiscalReferences;
			}
		}
		ICusFiscalReferenceCollection<CusFiscalReference> cusFiscalReferences;

		protected virtual ICusFiscalReferenceCollection<CusFiscalReference> GetNewFiscalReferenceCollection() => new CusFiscalReferenceCollection<CusFiscalReference>(this);

		ICusFiscalReferenceValidationDecider ICusFiscalReferenceProviderWithValidationDecider.ValidationDecider => Factory.GetValue(ref cusFiscalReferenceValidationDeciderCached,
			() => JobDeclaration?.Configuration?.InstructionConfiguration?.GetCusFiscalReferenceValidationDecider(this));
		CachedProperty<ICusFiscalReferenceValidationDecider> cusFiscalReferenceValidationDeciderCached;

		ICusAuthorizationUsageValidationDecider ICusAuthorizationUsageProviderWithValidationDecider.ValidationDecider => Factory.GetValue(ref cusAuthorizationUsageValidationDeciderCached,
			() => JobDeclaration?.Configuration?.InstructionConfiguration?.GetCusAuthorizationUsageValidationDecider(this));
		CachedProperty<ICusAuthorizationUsageValidationDecider> cusAuthorizationUsageValidationDeciderCached;

		[ChildEditable]
		public ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences
		{
			get
			{
				if (cusSupplyChainActorReferences == null)
				{
					cusSupplyChainActorReferences = GetNewCusSupplyChainActorReferenceCollection();
					cusSupplyChainActorReferences.Load();
					RegisterEditableChildObject(cusSupplyChainActorReferences);
				}

				return cusSupplyChainActorReferences;
			}
		}
		ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> cusSupplyChainActorReferences;

		protected virtual ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> GetNewCusSupplyChainActorReferenceCollection() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);

		[ChildEditable(true)]
		public SealNumberCollection Seals
		{
			get
			{
				if (seals == null)
				{
					seals = new SealNumberCollection(this);
					seals.Load();
					RegisterEditableChildObject(seals);
				}
				return seals;
			}
		}

		SealNumberCollection seals;

		[ChildEditable(true)]
		public RequestedDocumentCollection RequestedDocuments
		{
			get
			{
				if (fRequiredDocuments == null)
				{
					fRequiredDocuments = new RequestedDocumentCollection(this);
					fRequiredDocuments.Load();
					if (RequestedDocumentsReadOnly)
					{
						fRequiredDocuments.SetReadOnlyIncludingChildren(true);
					}
					RegisterEditableChildObject(fRequiredDocuments);
				}
				return fRequiredDocuments;
			}
		}
		RequestedDocumentCollection fRequiredDocuments;
		protected virtual ZBool RequestedDocumentsReadOnly => false;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				CusAuthorizationUsages.RemoveAndDeleteAll();
				DV1DetailsPivots.RemoveAndDeleteAll();
				FiscalReferences.RemoveAndDeleteAll();
				CusSupplyChainActorReferences.RemoveAndDeleteAll();
				Seals.RemoveAndDeleteAll();
				OwnerOfGoodsCollection.RemoveAndDeleteAll();
				PlaceOfUseOrProcessingCollection.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new FetchStrategies.CusEntryInstructionFetchStrategy(this);

		protected override bool SupportsCloneCore() => true;

		protected override ZString OrgCusCodeTypeForWarehouse => OrgCusCode.CodeTypes.ControlledPremisesID;

		protected override Type GoodsLocationTypeCore => CusGoodsLocation.TypeDecider.GetTypeForCountryCode(CountryCode);

		public bool ShouldKeepNotAllowDeleteEntryLinesErrors { get; set; }

		public CurrencyConverter CurrencyConverter => currencyConverter ?? (currencyConverter = new CurrencyConverterWithDataProvider(Factory, new JobDeclarationCurrencyConverterDataProvider(JobDeclaration)));
		CurrencyConverter currencyConverter;

		protected override bool IsIntoRegimeCore => base.IsIntoRegimeCore || HasIntoVATWarehouseProcedure;

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes() => GetCusCodeDataTypesCore();

		protected virtual IDictionary<ZString, Type> GetCusCodeDataTypesCore()
		{
			return new Dictionary<ZString, Type>()
			{
				{ CusCodeDataTypeList.Codes.Seal, typeof(SealNumber) }
			};
		}

		#endregion

		#region ICusSupportingInfoTypeSupporter Members

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypesCore();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			return new Dictionary<ZString, Type>
			{
				{ Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument, typeof(RequestedDocument) },
				{ Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
				{ Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo) },
				{ Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) }
			};
		}

		[ResourceStringData("D85DF12F-0410-47ED-9D03-7D17A52DABBE", Caption = "Customs Doc. Status", FullDescription = "Customs Document Status", ShortCaption = "Doc. Status")]
		public ZString CustomsDocStatus => Factory.GetValue(ref customsDocStatusCached, () =>
		{
			var result = ZString.Empty;
			if (JobDeclaration?.IsExport ?? false)
			{
				var statuses = RequestedDocuments.Cast<RequestedDocument>().Where(x => x.CSI_Type.EqualsIgnoringCase(Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument)).Select(x => x.CSI_Status).GroupBy(x => x).Select(x => x.Key).ToHashSet();
				switch (statuses.Count)
				{
					case 0:
						//do nothing
						break;
					case 1:
						result = statuses.First();
						break;
					default:
						if (statuses.Contains(RequestedDocumentStatusList.Codes.RequestOpened))
						{
							result = RequestedDocumentStatusList.Codes.RequestOpened;
						}
						else if (statuses.Count == 2 && statuses.Contains(RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived) && statuses.Contains(RequestedDocumentStatusList.Codes.RequestCancelled))
						{
							result = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
						}
						else if (statuses.Contains(RequestedDocumentStatusList.Codes.PhysicallyPresentDocument))
						{
							result = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
						}
						break;
				}
			}
			return result;
		});
		CachedProperty<ZString> customsDocStatusCached;

		public ZPropertyInfo CustomsDocStatusInfo => GetZPropertyInfo(Schema.CustomsDocStatus);

		[ResourceStringData("5FF1AE4D-6E72-4DB2-85D0-FDD9BFAB8335", Caption = "Customs Doc. Status Description", FullDescription = "Customs Document Status Description", MediumCaption = "Customs Doc. Status Desc.", ShortCaption = "Doc. Status Desc.")]
		public ZString CustomsDocStatusDesc => Factory.GetValue(ref customsDocStatusDescCached, () => Factory.GetCachedValue<RequestedDocumentStatusList>().GetDescriptionFromCode(CustomsDocStatus));
		CachedProperty<ZString> customsDocStatusDescCached;
		public ZPropertyInfo CustomsDocStatusDescInfo => GetZPropertyInfo(Schema.CustomsDocStatusDesc);

		#endregion

		#region ICusReferenceTypeSupporter Members

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
		{
			{ CusReferenceTypeList.Codes.SupplyChainActor, SupplyChainActorType },
			{ CusReferenceTypeList.Codes.FiscalReference, FiscalReferenceType },
		};

		protected virtual Type SupplyChainActorType => typeof(CusSupplyChainActorReference);
		protected virtual Type FiscalReferenceType => typeof(CusFiscalReference);

		#endregion

		#region ICanBeImportOrExport Members

		ZBool ICanBeImportOrExport.IsImport => JobDeclaration?.IsImport ?? ZBool.False;

		ZBool ICanBeImportOrExport.IsExport => JobDeclaration?.IsExport ?? ZBool.False;

		string ICanBeImportOrExport.Level => UniversalReferenceConstants.RefCusCodeListLevelType.Both;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		string ICanBeImportOrExport.TrueCountryCode => JobDeclaration?.CountryCode;

		string ICanBeImportOrExport.DataGroupingCode => JobDeclaration?.GetDefaultDataGroupingCode();

		#endregion

		public bool IsSimplifiedEntryInstruction => IsSimplifiedEntryInstructionCore;
		protected virtual bool IsSimplifiedEntryInstructionCore => CEI_Style == EUCommonConstants.ImportDeclarationTypeList.I1;

		public bool IsSimplifiedOrPreliminaryUnderCodeC => CEI_SubStyle == EntrySubStyleList.Codes.SimplifiedDeclaration || CEI_SubStyle == EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC;

		public bool IsCountryOfSupplySameForAllInvoiceLines
		{
			get
			{
				return InvoiceLines.Cast<JobComInvoiceLine>().Select(x => x.ZG_CountryOfSupply).Distinct().Count() <= 1;
			}
		}

		public ZString GetOwnerReference(OrgHeader owner) => GetOwnerReferenceCore(owner);

		protected virtual ZString GetOwnerReferenceCore(OrgHeader owner) => owner.GetEuIdentificationNumber();

		#region Guarantees

		[ChildEditable(true)]
		public GuaranteeForEntryInstructionCollection Guarantees
		{
			get
			{
				if (guarantees == null)
				{
					guarantees = GetNewGuaranteeForEntryInstructionCollection();
					guarantees.Load();
					RegisterEditableChildObject(guarantees);
				}
				return guarantees;
			}
		}
		GuaranteeForEntryInstructionCollection guarantees;

		protected virtual GuaranteeForEntryInstructionCollection GetNewGuaranteeForEntryInstructionCollection() => new GuaranteeForEntryInstructionCollection(this);

		public void EmptyGuaranteesIfNecessary()
		{
			if (!JobDeclaration?.Configuration?.InstructionConfiguration?.GuaranteesSupport(JobDeclaration, this) ?? false)
			{
				Guarantees.RemoveAndDeleteAll();
			}
		}

		#endregion

		#region Seal Numbers

		public HashSet<ZString> GetEffectiveSealNumbers()
		{
			var sealNumbers = GetEntryInstructionLevelSealNumbers();
			sealNumbers.UnionWith(GetContainerLevelSealNumbers());
			sealNumbers.RemoveWhere(sealNumber => sealNumber.IsEmpty);
			return sealNumbers;
		}

		HashSet<ZString> GetContainerLevelSealNumbers()
		{
			return InvoiceLines.Cast<JobComInvoiceLine>()
				.SelectMany(invoiceLine => invoiceLine.ContainersPivot)
				.Cast<CusContainerInvoiceLinePivot>()
				.Select(x => x.Container)
				.WhereNotNull()
				.SelectMany(container => new[] { container.CO_Seal, container.CO_SecondSeal })
				.ToHashSet();
		}

		HashSet<ZString> GetEntryInstructionLevelSealNumbers()
		{
			return Seals
				.Cast<SealNumber>()
				.Select(x => x.CY_Data)
				.ToHashSet();
		}

		#endregion

		public IReadOnlyList<string> MultipleKeysToUse => JobDeclaration?.MultipleKeysToUse ?? Array.Empty<string>();

		#region IUcc6ValueProvider

		bool IUcc6ValueProvider.IsUCC6 => JobDeclaration?.IsUCC6 ?? false;

		bool IUcc6ValueProvider.IsExport => JobDeclaration?.IsExport ?? false;

		bool IUcc6ValueProvider.IsImport => JobDeclaration?.IsImport ?? false;

		#endregion

		#region IValidationModesSupporter
		public ValidationModes ValidationModes
		{
			get
			{
				if (!fValidationModes.HasValue)
				{
					ValidationModesCalculator.RecalculateValidationModes();
				}
				return fValidationModes.Value;
			}
			set
			{
				fValidationModes = value;
			}
		}
		ValidationModes? fValidationModes;

		public EntryInstructionValidationModesCalculator ValidationModesCalculator => validationModesCalculator ?? (validationModesCalculator = CreateNewValidationModesCalculator());
		EntryInstructionValidationModesCalculator validationModesCalculator;

		protected virtual EntryInstructionValidationModesCalculator CreateNewValidationModesCalculator() => new EntryInstructionValidationModesCalculator(this);

		public bool IsOriginalValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.Original); }
		}

		public bool IsAmendmentValidationMode
		{
			get { return ValidationModesCalculator.IsThisValidationOn(ValidationModes, ValidationModes.Amendment); }
		}

		#endregion

		#region public AdditionalInfoCollection AdditionalInfos

		[ChildEditable(true)]
		public AdditionalInfoCollection AdditionalInfos => additionalInfos ?? (additionalInfos = GetAdditionalInfos());

		AdditionalInfoCollection additionalInfos;

		AdditionalInfoCollection GetAdditionalInfos()
		{
			var result = CreateNewAdditionalInfoCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		#endregion

		#region CEI_OH_Owner

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction|CEI_OH_Owner", Caption = "Owner")]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction|IMPUCC6|CEI_OH_Owner", Caption = "Owner", FullDescription = "[3/8] Owner of Goods", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZGuid CEI_OH_Owner
		{
			get => base.CEI_OH_Owner;
			set
			{
				if (base.CEI_OH_Owner != value)
				{
					base.CEI_OH_Owner = value;
					OwnerOfGoodsCollection.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}
		#endregion

		#region OwnerOfGoods

		[ChildEditable(true)]
		public OwnerOfGoodsCollection OwnerOfGoodsCollection
		{
			get
			{
				if (cusOwnerOfGoodsCollection == null)
				{
					cusOwnerOfGoodsCollection = new OwnerOfGoodsCollection(this);
					cusOwnerOfGoodsCollection.Load();
					RegisterEditableChildObject(cusOwnerOfGoodsCollection);
				}
				return cusOwnerOfGoodsCollection;
			}
		}
		OwnerOfGoodsCollection cusOwnerOfGoodsCollection;

		#endregion

		#region ISequenceNumberHeader
		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(OwnerOfGoodsCollection);

		internal ShortSequenceNumberGenerator SequenceGenerator => sequenceGenerator ?? (sequenceGenerator = new ShortSequenceNumberGenerator(this, () => 0, () => byte.MaxValue));
		ShortSequenceNumberGenerator sequenceGenerator;
		#endregion

		#region FirstPlaceOfUseOrProcessing

		public PlaceOfUseOrProcessing FirstPlaceOfUseOrProcessing
		{
			get
			{
				if (firstPlaceOfUseOrProcessing == null)
				{
					firstPlaceOfUseOrProcessing = GetFirstPlaceOfUseOrProcessing();
					RegisterEditableChildObject(firstPlaceOfUseOrProcessing);
				}
				return firstPlaceOfUseOrProcessing;
			}
		}
		PlaceOfUseOrProcessing firstPlaceOfUseOrProcessing;

		protected virtual PlaceOfUseOrProcessing GetFirstPlaceOfUseOrProcessing() => Customs.Business.CusGoodsLocation.LoadOrCreate<PlaceOfUseOrProcessing>(this, PlaceOfUseOrProcessingLocationUseList.Codes.FirstPlaceOfUseOrProcessing);

		public ZString FirstPlaceOfUseOrProcessingDescription
		{
			get
			{
				if (firstPlaceOfUseOrProcessing == null)
				{
					firstPlaceOfUseOrProcessing = Customs.Business.CusGoodsLocation.Load<PlaceOfUseOrProcessing>(this, PlaceOfUseOrProcessingLocationUseList.Codes.FirstPlaceOfUseOrProcessing);
					if (firstPlaceOfUseOrProcessing != null)
					{
						RegisterEditableChildObject(firstPlaceOfUseOrProcessing);
					}
				}
				return firstPlaceOfUseOrProcessing?.DisplayText ?? ZString.Empty;
			}
		}

		public ZPropertyInfo FirstPlaceOfUseOrProcessingDescriptionInfo => GetZPropertyInfo(nameof(FirstPlaceOfUseOrProcessingDescription));

		#endregion

		#region PlacesOfUseOrProcessing
		[ChildEditable(true)]
		public PlaceOfUseOrProcessingCollection PlaceOfUseOrProcessingCollection
		{
			get
			{
				if (placeOfUseOrProcessingCollection == null)
				{
					placeOfUseOrProcessingCollection = new PlaceOfUseOrProcessingCollection(this);
					placeOfUseOrProcessingCollection.Load();
					placeOfUseOrProcessingCollection.CountChanged += PlaceOfUseOrProcessingCollection_CountChanged;
					RegisterEditableChildObject(placeOfUseOrProcessingCollection);
				}
				return placeOfUseOrProcessingCollection;
			}
		}

		PlaceOfUseOrProcessingCollection placeOfUseOrProcessingCollection;

		void PlaceOfUseOrProcessingCollection_CountChanged(object sender, EventArgs e)
		{
			FirstPlaceOfUseOrProcessing.Validation.ValidateAll();
		}
		#endregion

		[ResourceStringData("7E6D7B05-9623-48DA-BF00-D47C92A49F08", Caption = "Period (Month)")]
		[ResourceStringData("IMP87-0CE9-4152-BF8F-D677F915171F", Caption = "Period (Month)", FullDescription = "[Annex A 4/17] Dates, Times, Periods and Places > Period for Discharge > Period (Month)", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZInt ZG_PeriodForDischarge { get => base.ZG_PeriodForDischarge; set => base.ZG_PeriodForDischarge = value; }

		[ResourceStringData("548D6BB5-4425-47BF-80AC-0DCA885F8D36", Caption = "Automatic Extension?")]
		[ResourceStringData("BF3B758F-B4E1-41CF-A298-23D3B3CC1901", Caption = "Automatic Extension?", FullDescription = "[Annex A 4/17] Dates, Times, Periods and Places > Period for Discharge > Automatic Extension?", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZBool ZG_PeriodForDischargeAutoExtension { get => base.ZG_PeriodForDischargeAutoExtension; set => base.ZG_PeriodForDischargeAutoExtension = value; }

		[ResourceStringData("01056DA1-90C6-4623-A1B9-4841CF9A8689", Caption = "Deadline")]
		[ResourceStringData("4A84DDAE-1400-492C-818B-9E5FEBC8EA85", Caption = "Deadline", FullDescription = "[Annex A 4/18] Dates, Times, Periods and Places > Bill of Discharge > Deadline", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZInt ZG_BillOfDischargeDeadline { get => base.ZG_BillOfDischargeDeadline; set => base.ZG_BillOfDischargeDeadline = value; }

		[ResourceStringData("BBC4D9CA-C367-4AD8-BDDF-B4A144368406", Caption = "Necessary?")]
		[ResourceStringData("58656FB5-27A6-44BB-B214-2E4A0E0E034A", Caption = "Necessary?", FullDescription = "[Annex A 4/18] Dates, Times, Periods and Places > Bill of Discharge > Necessary?", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZBool ZG_BillOfDischargeIsNecessary { get => base.ZG_BillOfDischargeIsNecessary; set => base.ZG_BillOfDischargeIsNecessary = value; }

		#region BillOfDischargeDetails

		[MaxLength(512)]
		[BusinessObjectTestExclude]
		[ResourceStringData("511ACC79-82F8-4CBC-A0CC-385099155D7F", Caption = "Details")]
		[ResourceStringData("2C55C465-3C38-4ED4-8A10-43C2ACAAA15E", Caption = "Details", FullDescription = "[Annex A 4/18] Dates, Times, Periods and Places > Bill of Discharge > Details", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public virtual ZString BillOfDischargeDetails
		{
			get => BillOfDischargeDetailsNoteWriter.Value;
			set
			{
				var oldValue = BillOfDischargeDetails;
				CheckMaximumLength(BillOfDischargeDetailsInfo, value);
				BillOfDischargeDetailsNoteWriter.UpdateValue(value);
				BillOfDischargeDetailsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo BillOfDischargeDetailsInfo => GetZPropertyInfo(nameof(BillOfDischargeDetails));

		PredefinedNoteWriter BillOfDischargeDetailsNoteWriter
		{
			get { return billOfDischargeDetailsNoteWriter ?? (billOfDischargeDetailsNoteWriter = new PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.BillOfDischargeDetails)); }
		}
		PredefinedNoteWriter billOfDischargeDetailsNoteWriter;

		#endregion

		#region PeriodForDischargeDetails

		[MaxLength(512)]
		[BusinessObjectTestExclude]
		[ResourceStringData("33144FCB-4346-45F7-82BE-E5B04AE8B7E3", Caption = "Details")]
		[ResourceStringData("4540DBB5-C733-4764-9B01-81992DA27A1F", Caption = "Details", FullDescription = "[Annex A 4/17] Dates, Times, Periods and Places > Period for Discharge > Details", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public virtual ZString PeriodForDischargeDetails
		{
			get => PeriodForDischargeDetailsNoteWriter.Value;
			set
			{
				var oldValue = PeriodForDischargeDetails;
				CheckMaximumLength(PeriodForDischargeDetailsInfo, value);
				PeriodForDischargeDetailsNoteWriter.UpdateValue(value);
				PeriodForDischargeDetailsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo PeriodForDischargeDetailsInfo => GetZPropertyInfo(nameof(PeriodForDischargeDetails));

		PredefinedNoteWriter PeriodForDischargeDetailsNoteWriter
		{
			get { return periodForDischargeDetailsNoteWriter ?? (periodForDischargeDetailsNoteWriter = new PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.PeriodForDischargeDetails)); }
		}
		PredefinedNoteWriter periodForDischargeDetailsNoteWriter;

		#endregion

		#region Article 163 Others

		[ResourceStringData("8C515348-8AE8-44BB-A651-EDFB64063718", Caption = "Calculate the Amount of Import Duty in Accordance with Article 86(3) of the Code", FullDescription = "[Annex A 8/13] Others > Calculate the Amount of Import Duty in Accordance with Article 86(3) of the Code")]
		public override ZString ZG_Article86_3_UCC { get => base.ZG_Article86_3_UCC; set => base.ZG_Article86_3_UCC = value; }

		[MaxLength(512)]
		[BusinessObjectTestExclude]
		[ResourceStringData("61C50184-1A00-4E3B-85D9-E99258C862C1", Caption = "Additional Information", MediumCaption = "Additional Info.", ShortCaption = "Add. Info.", FullDescription = "[Annex A 8/5] Others > Additional Information")]
		public virtual ZString AdditionalInformation
		{
			get => AdditionalInformationNoteWriter.Value;
			set
			{
				var oldValue = AdditionalInformation;
				CheckMaximumLength(AdditionalInformationInfo, value);
				AdditionalInformationNoteWriter.UpdateValue(value);
				AdditionalInformationInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo AdditionalInformationInfo => GetZPropertyInfo(nameof(AdditionalInformation));

		PredefinedNoteWriter AdditionalInformationNoteWriter
		{
			get { return additionalInformationNoteWriter ?? (additionalInformationNoteWriter = new PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.AdditionalInformation)); }
		}
		PredefinedNoteWriter additionalInformationNoteWriter;

		#endregion

		#region IdentificationOfGoods

		[ResourceStringData("CE5352D1-B5B2-4772-9E67-E07FBE8945E6", Caption = "Rate of Yield")]
		[ResourceStringData("2788FC30-E19B-4CAD-8E8D-C0F01E139CE0", Caption = "Rate of Yield", FullDescription = "[Annex 5/5] Identification of goods > Rate of Yield", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString ZG_RateOfYield { get => base.ZG_RateOfYield; set => base.ZG_RateOfYield = value; }

		[ResourceStringData("2204695D-8F0C-4990-94FC-834CD49EED82", Caption = "Commodity Code")]
		[ResourceStringData("5ECA4B8D-F36C-42FB-82F1-210FE48A70AE", Caption = "Commodity Code", FullDescription = "[Annex 5/5] Identification of goods > Processed Products > Commodity Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString ZG_ProcessedProductsCommodityCode { get => base.ZG_ProcessedProductsCommodityCode; set => base.ZG_ProcessedProductsCommodityCode = value; }

		[ResourceStringData("173DCB8F-D80C-4A8F-AACF-6EF082463D7E", Caption = "Code")]
		[ResourceStringData("829F6570-FBEB-4240-A9D3-77818AF2A180", Caption = "Code", FullDescription = "[Annex 5/8] Identification of goods > Identification of Goods > Code", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
		public override ZString ZG_IdOfGoodCode { get => base.ZG_IdOfGoodCode; set => base.ZG_IdOfGoodCode = value; }

		[MaxLength(512)]
		[BusinessObjectTestExclude]
		[ResourceStringData("3B3B72F3-3273-44C2-A4D8-3ED747F3A7D6", Caption = "Goods Description", FullDescription = "[Annex 5/5] Identification of goods > Processed Products > Goods Description")]
		public virtual ZString ProcessedProductDescription
		{
			get => ProcessedProductDescriptionNoteWriter.Value;
			set
			{
				var oldValue = ProcessedProductDescription;
				CheckMaximumLength(ProcessedProductDescriptionInfo, value);
				ProcessedProductDescriptionNoteWriter.UpdateValue(value);
				ProcessedProductDescriptionInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ProcessedProductDescriptionInfo => GetZPropertyInfo(nameof(ProcessedProductDescription));

		PredefinedNoteWriter ProcessedProductDescriptionNoteWriter
		{
			get { return processedProductDescriptionNoteWriter ?? (processedProductDescriptionNoteWriter = new PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.ProcessedProductDescription)); }
		}
		PredefinedNoteWriter processedProductDescriptionNoteWriter;

		[MaxLength(512)]
		[BusinessObjectTestExclude]
		[ResourceStringData("14D2F1B4-CA99-4744-93B4-8BE45E9ACBBE", Caption = "Details", FullDescription = "[Annex 5/8] Identification of goods > Identification of Goods > Details")]
		public virtual ZString IdentificationofGoodsDetails
		{
			get => IdentificationofGoodsDetailsNoteWriter.Value;
			set
			{
				var oldValue = IdentificationofGoodsDetails;
				CheckMaximumLength(IdentificationofGoodsDetailsInfo, value);
				IdentificationofGoodsDetailsNoteWriter.UpdateValue(value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateIdentificationofGoodsDetails();
				}
				IdentificationofGoodsDetailsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IdentificationofGoodsDetailsInfo => GetZPropertyInfo(nameof(IdentificationofGoodsDetails));

		PredefinedNoteWriter IdentificationofGoodsDetailsNoteWriter
		{
			get { return identificationofGoodsDetailsNoteWriter ?? (identificationofGoodsDetailsNoteWriter = new PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.IdentificationofGoodsDetails)); }
		}
		PredefinedNoteWriter identificationofGoodsDetailsNoteWriter;

		#endregion

		#region DetailsOfPlannedActivities

		[MaxLength(512)]
		[BusinessObjectTestExclude]
		[ResourceStringData("AA2945C9-93CA-49F2-99AA-F0DA7E9CED77", Caption = "Planned Activities", FullDescription = "[Annex A 7/5] Activities and Procedures > Details of Planned Activities")]
		public virtual ZString DetailsOfPlannedActivities
		{
			get => DetailsOfPlannedActivitiesNoteWriter.Value;
			set
			{
				var oldValue = DetailsOfPlannedActivities;
				CheckMaximumLength(DetailsOfPlannedActivitiesInfo, value);
				DetailsOfPlannedActivitiesNoteWriter.UpdateValue(value);
				DetailsOfPlannedActivitiesInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo DetailsOfPlannedActivitiesInfo => GetZPropertyInfo(nameof(DetailsOfPlannedActivities));

		PredefinedNoteWriter DetailsOfPlannedActivitiesNoteWriter
		{
			get { return detailsOfPlannedActivitiesNoteWriter ?? (detailsOfPlannedActivitiesNoteWriter = new PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.DetailsOfPlannedActivities)); }
		}
		PredefinedNoteWriter detailsOfPlannedActivitiesNoteWriter;

		#endregion

		#region Conditions and Terms

		[ResourceStringData("DD714F6D-C6D2-42AA-84E3-85DD3A10131D",
			ShortCaption = "Processing Procedure",
			Caption = "Processing Procedure Code",
			FullDescription = "[Annex A 6/2] Conditions and Terms > Economic Conditions > Processing Procedure Code",
			MultipleKey = JobDeclaration.CaptionKeyImportUCC6
		)]
		public override ZString ZG_ProcessingProcedureCode { get => base.ZG_ProcessingProcedureCode; set => base.ZG_ProcessingProcedureCode = value; }

		[MaxLength(512)]
		[ResourceStringData("AF80C287-0287-4244-B0EE-C6F341602567",
			ShortCaption = "Details",
			Caption = "Processing Procedure Details",
			FullDescription = "[Annex A 6/2] Conditions and Terms > Economic Conditions > Details",
			MultipleKey = JobDeclaration.CaptionKeyImportUCC6
		)]
		public virtual ZString ProcessingProcedureDetails
		{
			get => ProcessingProcedureDetailsNoteWriter.Value;
			set
			{
				var oldValue = ProcessingProcedureDetails;
				CheckMaximumLength(ProcessingProcedureDetailsInfo, value);
				ProcessingProcedureDetailsNoteWriter.UpdateValue(value);
				ProcessingProcedureDetailsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ProcessingProcedureDetailsInfo => GetZPropertyInfo(nameof(ProcessingProcedureDetails));

		PredefinedNoteWriter ProcessingProcedureDetailsNoteWriter =>
			processingProcedureDetailsNoteWriter ?? (processingProcedureDetailsNoteWriter = new PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.ProcessingProcedureDetails));
		PredefinedNoteWriter processingProcedureDetailsNoteWriter;

		#endregion

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}

		public List<string> SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>());
		List<string> synchroniserReadOnlyMembers;

		public ZBool IsCentralisedClearance => IsCentralisedClearanceCore;
		protected virtual ZBool IsCentralisedClearanceCore => CusAuthorizationUsages.Any(auth => auth.AGC_Code == CusAuthorizationHeaderTypeList.Codes.CentralizedClearance);

		public bool IsRequestedProcedureValid => Factory.GetValue(ref isRequestedProcedureValidCached, GetIsRequestedProcedureValid);
		CachedProperty<bool> isRequestedProcedureValidCached;

		bool GetIsRequestedProcedureValid() => CEI_Procedure.Length == 2 && (JobDeclaration?.IsRequestedProcedureEnable ?? false) && (Lookups.ProcedureCodeList?.ContainsCode(CEI_Procedure) ?? false);
	}
}
