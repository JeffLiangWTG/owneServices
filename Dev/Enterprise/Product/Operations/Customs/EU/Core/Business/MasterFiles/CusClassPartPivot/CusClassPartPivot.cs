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
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public class CusClassPartPivot : BaseCusClassPartPivot
		, ICanBeImportOrExport
		, IAddInfoManager
		, Integration.Customs.EU.ICusClassPartPivot
		, ITaxAndDocsProvider
		, ICusSupportingInfoTypeSupporter
		, ISupplementaryCodeSupporter
		, ICusCodeDataTypeSupporter
		, ICusAddInfoTypeSupporter
		, IAdditionalProcedureParent
		, IImportExport
	{
		public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public static new readonly CusClassPartPivotTypeDecider TypeDecider = new CusClassPartPivotTypeDecider();

		public CusClassPartPivotConfiguration Configuration => configuration ?? (configuration = CusClassPartPivotConfiguration.GetConfiguration(Factory, CI_RN_NKCountry));
		CusClassPartPivotConfiguration configuration;

		#region Base overrides

		protected override ZString DefaultChildType => ClassificationType.Both;

		protected override Customs.Business.CusClassPartPivotLookups GetNewLookups()
		{
			return new CusClassPartPivotLookups(this);
		}

		public new CusClassPartPivotLookups Lookups
		{
			get { return (CusClassPartPivotLookups)base.Lookups; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsTariffNumReadOnly")]
		[ReadOnlyMember("IsTariffNumReadOnly")]
		[ResourceStringData("7b0ee739-cc78-4ec5-880c-3b49bd55f33e|CI_FormattedTariffNum", Caption = "[33] Tariff")]
		public override ZString CI_FormattedTariffNum
		{
			get { return base.CI_FormattedTariffNum; }
			set { base.CI_FormattedTariffNum = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsTariffNumReadOnly")]
		[ReadOnlyMember("IsTariffNumReadOnly")]
		public override ZString CI_TariffNum
		{
			get { return base.CI_TariffNum; }
			set { base.CI_TariffNum = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member CI_CC_ReadOnly")]
		[ReadOnlyMember("CI_CC_ReadOnly")]
		public override ZGuid CI_CC
		{
			get { return base.CI_CC; }
			set { base.CI_CC = value; }
		}

		protected override ZString UniversalTariffType => TariffFormatter.GetTariffType(CI_ChildType == ClassificationType.EXP);

		public override ZString CI_ChildType
		{
			get => base.CI_ChildType;
			set
			{
				if (CI_ChildType != value)
				{
					if (value == ClassificationType.Both && HasAnyDetailUnrelatedToBoth)
					{
						var args = new CancelEventArgs(false);
						OnChildTypeToBeBothAndClearUnrelatedDetails?.Invoke(value, args);
						if (!args.Cancel)
						{
							base.CI_ChildType = value;
							ClearAllDetailsUnrelatedToBoth();
						}
					}
					else
					{
						base.CI_ChildType = value;
					}
				}
			}
		}

		public event CancelEventHandler OnChildTypeToBeBothAndClearUnrelatedDetails;

		bool HasAnyDetailUnrelatedToBoth => !CI_Supplement1.IsEmpty
													|| !CI_Supplement2.IsEmpty
													|| AdditionalSupplementaryCodes.Count > 0
													|| !CI_CPC.IsEmpty
													|| AdditionalProcedureCodes.Count > 0
													|| !CI_ConcessionOrder.IsEmpty
													|| !CI_ThirdQty.IsEmpty
													|| !PreferenceCode.IsEmpty
													|| SupportingDocuments.Count > 0
													|| AdditionalInfos.Count > 0
													|| PreviousDocuments.Count > 0
													|| Taxes.Any();

		void ClearAllDetailsUnrelatedToBoth()
		{
			CI_Supplement1 = ZString.Empty;
			CI_Supplement2 = ZString.Empty;
			AdditionalSupplementaryCodes.RemoveAndDeleteAll();

			CI_CPC = ZString.Empty;
			AdditionalProcedureCodes.RemoveAndDeleteAll();
			CI_ConcessionOrder = ZString.Empty;
			CI_ThirdQty = ZDecimal.Zero;
			PreferenceCode = ZString.Empty;
			SupportingDocuments.RemoveAndDeleteAll();
			AdditionalInfos.RemoveAndDeleteAll();
			PreviousDocuments.RemoveAndDeleteAll();
			Taxes.RemoveAndDeleteAll();
		}

		public bool IsClassificationBoth => CI_ChildType == ClassificationType.Both;

		#endregion

		#region EU CI_AddInfo properties and helpers

		[ReadOnlyMember(nameof(IsClassificationBoth))]
		public virtual ZDecimal CI_SecondQty
		{
			get { return AddInfo.ZG_SecondQty; }
			set { AddInfo.ZG_SecondQty = value; }
		}

		public ZPropertyInfo CI_SecondQtyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CI_SecondQty), x => AddInfo.ZG_SecondQtyInfo); }
		}

		public virtual ZString CI_SecondUnitQty
		{
			get { return AddInfo.ZG_SecondUnitQty; }
			set { AddInfo.ZG_SecondUnitQty = value; }
		}

		public ZPropertyInfo CI_SecondUnitQtyInfo => GetWrappedZPropertyInfo(nameof(CI_SecondUnitQty), x => AddInfo.ZG_SecondUnitQtyInfo);

		[ReadOnlyMember(nameof(IsClassificationBoth))]
		public virtual ZDecimal CI_ThirdQty
		{
			get { return AddInfo.ZG_ThirdQty; }
			set { AddInfo.ZG_ThirdQty = value; }
		}

		public ZPropertyInfo CI_ThirdQtyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CI_ThirdQty), x => AddInfo.ZG_ThirdQtyInfo); }
		}

		[ReadOnlyMember(nameof(IsClassificationBoth))]
		public virtual ZDecimal CI_FourthQty
		{
			get { return AddInfo.ZG_FourthQty; }
			set { AddInfo.ZG_FourthQty = value; }
		}

		public ZPropertyInfo CI_FourthQtyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CI_FourthQty), x => AddInfo.ZG_FourthQtyInfo); }
		}

		[ReadOnlyMember(nameof(IsClassificationBoth))]
		public virtual ZDecimal CI_FifthQty
		{
			get { return AddInfo.ZG_FifthQty; }
			set { AddInfo.ZG_FifthQty = value; }
		}

		public ZPropertyInfo CI_FifthQtyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CI_FifthQty), x => AddInfo.ZG_FifthQtyInfo); }
		}

		[ReadOnlyMember(nameof(IsClassificationBoth))]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.RefCusProcedureCollection))]
		public virtual ZString CI_CPC
		{
			get { return AddInfo.ZG_ProcedureCode; }
			set { AddInfo.ZG_ProcedureCode = value; }
		}

		public ZPropertyInfo CI_CPCInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(CI_CPC), x => AddInfo.ZG_ProcedureCodeInfo); }
		}

		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.TaxTypeList))]
		[ResourceStringData("Enterprise.Customs.GB.Business.MasterFiles.CusClassPartPivot.CI_ZZF_NKTaxType", Caption = "VAT")]
		public override ZString CI_ZZF_NKTaxType
		{
			get => base.CI_ZZF_NKTaxType;
			set => base.CI_ZZF_NKTaxType = value;
		}

		[ReadOnlyMember(nameof(IsClassificationBoth))]
		public override ZString CI_ConcessionOrder
		{
			get => base.CI_ConcessionOrder;
			set => base.CI_ConcessionOrder = value;
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(CusClassPartPivotLookups.GoodsCategoryList))]
		public virtual ZString CI_GoodsCategory
		{
			get => AddInfo.ZG_GoodsCategory;
			set => AddInfo.ZG_GoodsCategory = value;
		}

		public ZPropertyInfo CI_GoodsCategoryInfo => GetWrappedZPropertyInfo(nameof(CI_GoodsCategory), x => AddInfo.ZG_GoodsCategoryInfo);

		#region SupplementaryCodes

		#region CI_Supplement1

		[ReadOnlyMember(nameof(IsClassificationBoth))]
		[MaxLength(Customs.EU.Business.SupplementaryCode.Schema.CY_CodeMaxLength)]
		public virtual ZString CI_Supplement1
		{
			get { return (Supplement1 != null) ? Supplement1.CY_Code : ZString.Empty; }
			set => Business.SupplementaryCodeHelper.SupplementaryCodeSetter(this, CI_Supplement1Info, value, Supplement1, 1);
		}

		public ZPropertyInfo CI_Supplement1Info
		{
			get { return Supplement1 != null ? GetWrappedZPropertyInfo(nameof(CI_Supplement1), x => Supplement1.CY_CodeInfo) : GetZPropertyInfo(nameof(CI_Supplement1)); }
		}

		SupplementaryCode Supplement1
		{
			get
			{
				if (supplement1 == null || supplement1.IsDeleted)
				{
					var loader = new BaseSupplementaryCode.Loader(Factory);
					supplement1 = loader.Load<SupplementaryCode, CusClassPartPivot>(this, 1);
					if (supplement1 != null)
					{
						RegisterEditableChildObject(supplement1);
					}
				}
				return supplement1;
			}
		}
		SupplementaryCode supplement1;

		#endregion

		#region CI_Supplement2

		[ReadOnlyMember(nameof(IsClassificationBoth))]
		[MaxLength(Customs.EU.Business.SupplementaryCode.Schema.CY_CodeMaxLength)]
		public virtual ZString CI_Supplement2
		{
			get { return (Supplement2 != null) ? Supplement2.CY_Code : ZString.Empty; }
			set => Business.SupplementaryCodeHelper.SupplementaryCodeSetter(this, CI_Supplement2Info, value, Supplement2, 2);
		}

		public ZPropertyInfo CI_Supplement2Info
		{
			get { return Supplement2 != null ? GetWrappedZPropertyInfo(nameof(CI_Supplement2), x => Supplement2.CY_CodeInfo) : GetZPropertyInfo(nameof(CI_Supplement2)); }
		}

		SupplementaryCode Supplement2
		{
			get
			{
				if (supplement2 == null || supplement2.IsDeleted)
				{
					var loader = new BaseSupplementaryCode.Loader(Factory);
					supplement2 = loader.Load<SupplementaryCode, CusClassPartPivot>(this, 2);
					if (supplement2 != null)
					{
						RegisterEditableChildObject(supplement2);
					}
				}
				return supplement2;
			}
		}
		SupplementaryCode supplement2;

		#endregion

		#region CI_AdditionalSupplements

		[ReadOnlyMember(nameof(CI_AdditionalSupplements_ReadOnly))]
		public ZString CI_AdditionalSupplements => AdditionalSupplementaryCodes.AsString;

		public ZPropertyInfo CI_AdditionalSupplementsInfo => GetZPropertyInfo(nameof(CI_AdditionalSupplements));

		public bool CI_AdditionalSupplements_ReadOnly => false;

		#endregion

		[ChildEditable(true)]
		public SupplementaryCodeCollection AdditionalSupplementaryCodes
		{
			get
			{
				if (additionalSupplements == null)
				{
					additionalSupplements = SupplementaryCodeCollection.New(CI_AdditionalSupplementsInfo);
					RegisterEditableChildObject(additionalSupplements);
				}
				return additionalSupplements;
			}
		}
		SupplementaryCodeCollection additionalSupplements;

		ICusCodeDataCollection<BaseSupplementaryCode> ISupplementaryCodeSupporter.AdditionalSupplementaryCodes => AdditionalSupplementaryCodes;

		public IEnumerable<SupplementaryCode> SupplementaryCodes => AdditionalSupplementaryCodes.OfType<SupplementaryCode>().Union(new SupplementaryCode[] { Supplement1, Supplement2 }).Where(x => x != null);

		IEnumerable<BaseSupplementaryCode> ISupplementaryCodeSupporter.SupplementaryCodes => SupplementaryCodes;

		ZString ISupplementaryCodeSupporter.SupplementaryCodesFieldType => nameof(FieldType.Text);

		ResourceStringData ISupplementaryCodeSupporter.SupplementaryCodeCaption => null;

		public static string GetCountryCodeForSupplementaryCodeHelper(OrgSupplierPart part)
		{
			return part?.PivotsForBinding.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		public ZString GetCountryCodeForCodeProvider() => GetCountryCodeForSupplementaryCodeHelper(Part);

		TariffView ICusCodeDataWithOrderSupporter.Tariff => null;
		IZZRateSelectionCriteria ICusCodeDataWithOrderSupporter.RateSelectionCriteria => null;
		CodeDescriptionPairList ICusCodeDataWithOrderSupporter.CachedListOfAdditionalCodeDescriptions => null;

		void ICusCodeDataWithOrderSupporter.OnCodesChanged()
		{
		}

		#endregion

		public AddInfoCusClassPartPivotLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public AddInfoCusClassPartPivotValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		AddInfoCusClassPartPivot AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AddInfoCusClassPartPivot(CI_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AddInfoCusClassPartPivot fAddInfo;

		IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region Multi-line cus add infos (EU documents)

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = GetSupportingDocuments();
				}

				supportingDocuments.SetReadOnlyIncludingChildren(ReadOnly);
				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		SupportingDocumentCollection GetSupportingDocuments()
		{
			var result = CreateNewSupportingDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual SupportingDocumentCollection CreateNewSupportingDocumentCollection()
		{
			return new SupportingDocumentCollection(this);
		}

		[ChildEditable(true)]
		public AdditionalInfoCollection AdditionalInfos
		{
			get
			{
				if (fAdditionalInfos == null)
				{
					fAdditionalInfos = GetAdditionalInfos();
				}

				fAdditionalInfos.SetReadOnlyIncludingChildren(ReadOnly);
				return fAdditionalInfos;
			}
		}
		AdditionalInfoCollection fAdditionalInfos;

		AdditionalInfoCollection GetAdditionalInfos()
		{
			var result = CreateNewAdditionalInfoCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual AdditionalInfoCollection CreateNewAdditionalInfoCollection()
		{
			return new AdditionalInfoCollection(this);
		}

		[ChildEditable(true)]
		public PreviousDocumentCollection PreviousDocuments
		{
			get
			{
				if (fPreviousDocuments == null)
				{
					fPreviousDocuments = GetPreviousDocuments();
				}

				fPreviousDocuments.SetReadOnlyIncludingChildren(ReadOnly);
				return fPreviousDocuments;
			}
		}
		PreviousDocumentCollection fPreviousDocuments;

		PreviousDocumentCollection GetPreviousDocuments()
		{
			var result = CreateNewPreviousDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual PreviousDocumentCollection CreateNewPreviousDocumentCollection()
		{
			return new PreviousDocumentCollection(this);
		}
		ZDateTime ITaxAndDocsProvider.DateOfValuation => ZDateTime.Now;
		BusinessObjectCollection ITaxAndDocsProvider.Taxes => Taxes;

		[ChildEditable(true)]
		public CusAddInfoCollection<Tax_CusAddInfoOnlyForPIVOT> Taxes
		{
			get
			{
				if (fTaxes == null)
				{
					fTaxes = GetTaxes();
				}

				fTaxes.SetReadOnlyIncludingChildren(ReadOnly);
				return fTaxes;
			}
		}
		CusAddInfoCollection<Tax_CusAddInfoOnlyForPIVOT> fTaxes;

		CusAddInfoCollection<Tax_CusAddInfoOnlyForPIVOT> GetTaxes()
		{
			var result = CreateTaxCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual CusAddInfoCollection<Tax_CusAddInfoOnlyForPIVOT> CreateTaxCollection()
		{
			return new CusAddInfoCollection<Tax_CusAddInfoOnlyForPIVOT>(this);
		}

		ZString ITaxAndDocsProvider.CountryCode
		{
			get { return CI_RN_NKCountry; }
		}

		[List(nameof(Lookups) + "+" + nameof(CusClassPartPivotLookups.PrimaryPreferenceList))]
		[MaxLength(3)]
		[ReadOnlyMember(nameof(IsClassificationBoth))]
		public virtual ZString PreferenceCode
		{
			get { return CI_PrimaryPreference; }
			set { CI_PrimaryPreference = value; }
		}

		public ZPropertyInfo PreferenceCodeInfo => GetWrappedZPropertyInfo(nameof(PreferenceCode), x => CI_PrimaryPreferenceInfo);

		public ZString SupplementaryCode
		{
			get { return CI_Supplement1; }
			set { CI_Supplement1 = value; }
		}

		public ZString Tariff
		{
			get { return CI_TariffNum; }
		}

		public ZString CountryOfOriginCode
		{
			get { return CI_RN_NKCountryOfOrigin; }
		}

		BusinessObjectFactory ITaxAndDocsProvider.Factory
		{
			get { return this.Factory; }
		}

		#endregion

		#region ICanBeImportOrExport - needed when we're the parent of an EU previous/supporting/additional document
		ZBool ICanBeImportOrExport.IsImport
		{
			get
			{
				return Classification != null ? (Classification.IsImport || Classification.IsBoth) :
				(IsClassificationBoth || CI_ChildType == ClassificationType.IMP);
			}
		}

		ZBool ICanBeImportOrExport.IsExport
		{
			get
			{
				return Classification != null ? (Classification.IsExport || Classification.IsBoth) :
				(IsClassificationBoth || CI_ChildType == ClassificationType.EXP);
			}
		}

		string ICanBeImportOrExport.Level => UniversalReferenceConstants.RefCusCodeListLevelType.Item;

		void ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}

		string ICanBeImportOrExport.TrueCountryCode => CI_RN_NKCountry;
		string ICanBeImportOrExport.DataGroupingCode => ((ICanBeImportOrExport)this).TrueCountryCode;

		public Directions JobDirection => ((ICanBeImportOrExport)this).IsImport ? Directions.Import : (((ICanBeImportOrExport)this).IsExport ? Directions.Export : Directions.Unknown);

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		protected override Customs.Business.CusClassPartPivotValidation GetNewValidation()
		{
			return new CusClassPartPivotValidation(this);
		}

		public new CusClassPartPivotValidation Validation
		{
			get { return (CusClassPartPivotValidation)GetNewValidation(); }
		}

		public new OrgSupplierPart Part
		{
			get { return (OrgSupplierPart)base.Part; }
		}

		protected override Customs.Business.TariffFormatter GetTariffFormatter()
		{
			return TariffFormatter.New(Country?.Code);
		}

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return GetCusSupportingInfoTypes();
		}

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo));
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument));
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument));
			return result;
		}

		public virtual ZString DataGroupingCodeForAdditionalProcedures => ZString.Empty;

		ISupportingDocumentCollection<SupportingDocument> ISupportingDocumentsProvider.SupportingDocuments => SupportingDocuments;

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			foreach (var fetchStrategy in GetAdditionalBusinessObjectFetchStrategies())
			{
				yield return fetchStrategy;
			}
		}

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			return GetCusAddInfoTypes();
		}

		protected virtual IDictionary<ZString, Type> GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.GBTax, typeof(CusAddInfo<Tax_CusAddInfoOnlyForPIVOT>));
			return result;
		}

		#endregion

		#region IAdditionalProcedureParent Members

		BusinessObject IAdditionalProcedureParent.BusinessObject => this;

		CodeDescriptionPairList IAdditionalProcedureParent.AdditionalProcedureCodeList => Lookups.AdditionalCPCs;

		ZString IAdditionalProcedureParent.MainProcedure => CI_CPC;

		ZString IAdditionalProcedureParent.MainProcedurePrefix => CI_CPC.Left(4);

		AdditionalProcedureCodeCollection IAdditionalProcedureParent.AdditionalProcedureCodes => AdditionalProcedureCodes;

		int IAdditionalProcedureParent.MaxNumberOfAdditionalProcedureCode => 98;

		ZPropertyInfo IAdditionalProcedureParent.AdditionalProcedureCodesAsStringInfo => AdditionalProcedureCodesAsStringInfo;

		AdditionalProcedureCodeCollection additionalProcedureCodes;
		[ChildEditable(true)]
		public AdditionalProcedureCodeCollection AdditionalProcedureCodes
		{
			get
			{
				if (additionalProcedureCodes == null)
				{
					additionalProcedureCodes = new AdditionalProcedureCodeCollection(this);
					additionalProcedureCodes.CountChanged += AdditionalProcedureCodes_CountChanged;
					RegisterEditableChildObject(additionalProcedureCodes);
					additionalProcedureCodes.Load();
				}

				return additionalProcedureCodes;
			}
		}

		void AdditionalProcedureCodes_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			AdditionalProcedureCodesAsStringInfo.RefreshBinding();
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusClassPartPivot|AdditionalProcedureCodesAsString", Caption = "Additional Procedure Codes", MediumCaption = "Add. Procedure Codes", ShortCaption = "Add. CPCs")]
		public ZString AdditionalProcedureCodesAsString => AdditionalProcedureCodes.AsString;

		public ZPropertyInfo AdditionalProcedureCodesAsStringInfo => GetZPropertyInfo(nameof(AdditionalProcedureCodesAsString));

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.AdditionalProcedureCode, typeof(AdditionalProcedureCode));
			result.Add(CusCodeDataTypeList.Codes.SupplementaryCode, typeof(SupplementaryCode));
			return result;
		}

		public ZString GetCountryCodeFromAdditionalCode(ZString additionalCode) => ZString.Empty;

		#endregion
	}
}
