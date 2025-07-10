using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public JobDeclaration Declaration => Parent;

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		public override CodeDescriptionPairList JE_TotalNoOfPacksPackType_List => Factory.GetCachedValue<PackageKindCodeList>();

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<CustomsMessageStatusTypeList>();

		public override CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<CustomsEntryStatusExtendedTypeList>();

		public CodeDescriptionPairList DeclarationProcedureTypeList
		{
			get
			{
				if (Parent.IsExport)
				{
					return Factory.GetCachedValue<DeclarationProcedureTypeList>();
				}
				else
				{
					return Factory.GetCachedValue<ImportKindCodeList>();
				}
			}
		}

		public CodeDescriptionPairList TransactionTypeCodeList
		{
			get
			{
				if (Parent.IsLocalExport)
				{
					return Factory.GetCachedValue<LocalExportGoodsTypeList>();
				}
				else if (Parent.IsD87)
				{
					return Factory.GetCachedValue<CarnetCommodityUsageCodeList>();
				}
				else
				{
					return Factory.GetCachedValue<TransactionTypeCodeList>();
				}
			}
		}

		public override CodeDescriptionPairList MessageSubTypeList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (Parent.IsLocalExport)
				{
					result = Factory.GetCachedValue<LocalExportTransactionNatureCodeList>();
				}
				else if (Parent.IsImport)
				{
					result = Factory.GetCachedValue<ImportDeclarationTypeCodeList>();
				}
				else if (Parent.IsPersonalItemDeclaration)
				{
					result = Factory.GetCachedValue<StayPeriodCodeList>();
				}
				else
				{
					result = Factory.GetCachedValue<ExportTypeCodeList>();
				}
				return result;
			}
		}
		public override CodeDescriptionPairList IncoTermList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (Parent.IsLocalExport)
				{
					result = Factory.GetCachedValue<LocalExportIncotermList>();
				}
				else
				{
					result = Factory.GetCachedValue<IncotermList>();
				}
				return result;
			}
		}

		public new ZZRefCusCodeListCombinedCollection CustomsOfficeList
			=> ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection CustomsDivisonList
			=> ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, ZDateTime.Today);
		public ZZRefCusCodeListCombinedCollection TaxOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.TaxOffice, ZDateTime.Today);
		public ZZRefCusCodeListCombinedCollection BondedAreaCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BondedAreaCode, ZDateTime.Today);
		public ZZRefCusCodeListCombinedCollection IndustrialParkCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.IndustrialParkCode, ZDateTime.Today);
		public override CodeDescriptionPairList TransportTypeList => Business.TransportTypeList.GetTransportTypeList(Factory, Parent.JE_MessageType);
		public CodeDescriptionPairList ExporterTypeList => Factory.GetCachedValue<ExporterTypeCodeList>();
		public override IBusinessObjectCollection CarrierCodeCollection
		{
			get
			{
				string key = string.Format(System.Globalization.CultureInfo.InvariantCulture, "ZZRefCarrierCombinedCollection_{0}_{1}", CountryCodes.KoreaSouth, Declaration.JE_TransportMode);
				return Factory.GetCachedValue(key, GetCachedCollection);
			}
		}
		ZZRefCarrierCombinedCollection GetCachedCollection()
		{
			var filter = new ZQuery(ZZRefCarrierCombinedSchema.ZZ4_CountryOrGrouping, CountryCodes.KoreaSouth);
			switch (Declaration.JE_TransportMode)
			{
				case Core.Constants.TransportModes.Air:
					filter.AddToFilter(ZZRefCarrierCombinedSchema.ZZ4_IsAir, 1);
					break;
				case Core.Constants.TransportModes.Sea:
					filter.AddToFilter(ZZRefCarrierCombinedSchema.ZZ4_IsSea, 1);
					break;
				case Core.Constants.TransportModes.Mail:
					var subFilter = new ZQuery(ZZRefCarrierCombinedSchema.ZZ4_IsAir, 1);
					subFilter.AddToFilter(JoinCondition.Or, ZZRefCarrierCombinedSchema.ZZ4_IsSea, 1);
					filter.AddToFilter(subFilter);
					break;
				default:
					break;
			}
			return new ZZRefCarrierCombinedCollection(Factory, filter);
		}
		public CodeDescriptionPairList IATALoadPortKRList => Factory.GetCachedValue<IATALoadPortKRList>();
		public CodeDescriptionPairList SimpleDRWAppList => Factory.GetCachedValue<ApplicationForSimpleDrawbackCodeList>();
		public CodeDescriptionPairList OutOfHoursDecIndList => Factory.GetCachedValue<OutOfHoursDeclarationIndicatorCodeList>();
		public CodeDescriptionPairList GoodsConditionList => Factory.GetCachedValue<GoodsStatusCodeList>();
		public CodeDescriptionPairList ContainerPackKRList => ContainerPackModeCodeList.GetSupportedContainerPackModeCodeList(Factory, Parent.JE_TransportMode);
		public CodeDescriptionPairList MRNTypeList => Factory.GetCachedValue<MRNTypeList>();
		public CodeDescriptionPairList YesNoCodeList => Factory.GetCachedValue<YesNoList>();
		public ZZRefCusCodeListCombinedCollection CourierCompanyList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.ExpressDeliveryServiceIDs, ZDateTime.Today);
		public override CodeDescriptionPairList PaymentPartyList => Parent.IsImport ? Factory.GetCachedValue<PaymentMethodCodeList>() : base.PaymentPartyList;

		protected override IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
		{
			var list = new List<ZString> { };
			if (Parent.JE_ApplicationCode != Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced)
			{
				if (!KRCustomsRegistry.Instance.EnableToSaveImportDeclaration.GetValueWithoutFallback(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty))
				{
					list.Add(KRJobMessageTypeList.Codes.Import);
				}
			}
			if (!Parent.IsMiscDeclaration)
			{
				list.Add(KRJobMessageTypeList.Codes.PersonalItems);
				list.Add(KRJobMessageTypeList.Codes.Carnet);
				list.Add(KRJobMessageTypeList.Codes.ValuationDeclaration);
			}

			return list;
		}
		public CodeDescriptionPairList CustomsBrokerCommentCode1List => Factory.GetCachedValue<CustomsBrokerDealingRelationshipStatementTypeCodeList>();
		public CodeDescriptionPairList CustomsBrokerCommentCode2List => Factory.GetCachedValue<CustomsBrokerProductNameStandardSizeStatementTypeCodeList>();
		public CodeDescriptionPairList CustomsBrokerCommentCode3List => Factory.GetCachedValue<CustomsBrokerInspectionOpinionStatementTypeCodeList>();

		public RefCountryCollection CountryCollection => new RefCountryCollection(Factory);
		public new CodeDescriptionPairList PaidByList => Factory.GetCachedValue<PaidByCodeList>();
		public CodeDescriptionPairList ImporterTypeList => Factory.GetCachedValue<ImporterTypeCodeList>();
		public CodeDescriptionPairList BankTypeList => Factory.GetCachedValue<BankTypeList>();

		public CodeDescriptionPairList OutOfHoursDeclarationIndicatorList => Factory.GetCachedValue<OutOfHoursDeclarationIndicatorCodeList>();
		public CodeDescriptionPairList ReturnReasonList => Factory.GetCachedValue<ReturnReasonCodeList>();
		public CodeDescriptionPairList ReturnTypeList => Factory.GetCachedValue<ReturnTypeCodeList>();
		public CodeDescriptionPairList GoodsStatusList => Factory.GetCachedValue<GoodsStatusCodeList>();
		public CodeDescriptionPairList ApplicationForSimpleDrawbackList => Factory.GetCachedValue<ApplicationForSimpleDrawbackCodeList>();
		public CodeDescriptionPairList SouthNorthTradeYNList => Factory.GetCachedValue<SouthNorthTradeYNCodeList>();
		public CodeDescriptionPairList SouthNorthTradeIdentificationCodeList => Factory.GetCachedValue<SouthNorthTradeIdentificationCodeList>();
		public CodeDescriptionPairList CustomsDivisionList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, ZDateTime.Today);
		public CodeDescriptionPairList ContainerPackModeKRList => ContainerPackModeCodeList.GetSupportedContainerPackModeCodeList(Factory, Parent.JE_ContainerPackMode);
		public CodeDescriptionPairList GoldTradeTransactionYCodeList =>
			Factory.GetCachedValue("GoldTradeTransactionYCodeList", delegate
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(YesNoList.Codes.Yes, YesNoList.Descriptions.Yes);
				return result;
			});

		public CodeDescriptionPairList ImportDealingTypeCodeList => Factory.GetCachedValue<ImportDealingTypeCodeList>();
		public CodeDescriptionPairList DeclarationPlanList => Factory.GetCachedValue<ImportCustomsClearancePlanCodeList>();
		public OrgContactCollection OrgContactCollectionList
		{
			get
			{
				orgContactCollectionList ??= new CachedProperty<OrgContactCollection>(Factory, () =>
				{
					OrgContactCollection result;
					var importerPK = Declaration.JE_OH_Importer;
					if (importerPK.IsValid)
					{
						var zDBOnlyQuery = new ZDBOnlyQuery(typeof(OrgContact));
						zDBOnlyQuery.AddToFilter(OrgContactSchema.OC_OH, importerPK);
						zDBOnlyQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
						var attributeQuery = new ZDBOnlySubQuery(typeof(OrgContactAttribute), OrgContactAttributeSchema.PC_OC);
						attributeQuery.AddToFilter(OrgContactAttributeSchema.PC_Type, OrgConstants.ContactAllocationType.ValuationAuthorityForKRCustoms);
						zDBOnlyQuery.AddSubQuery(attributeQuery, JoinCondition.And);
						result = new OrgContactCollection(Factory, zDBOnlyQuery);
						result.Load();
					}
					else
					{
						result = new OrgContactCollection(Factory, new ZQuery() { IsNoResultQuery = true });
					}
					return result;
				});
				return orgContactCollectionList.Value;
			}
		}
		CachedProperty<OrgContactCollection> orgContactCollectionList;

		public CodeDescriptionPairList LateDecPenaltyDateCodeList => Factory.GetCachedValue<LateDecPenaltyDateCodeList>();
	}
}
