using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.BR.Business.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.BR.Business.CusEntryInstruction;

namespace Enterprise.Customs.BR.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
	{
		#region Lookups

		public new JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		#endregion

		#region Filters

		public override ZQuery Filter => base.Filter.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.NotEqual, new ZString[] { BRJobMessageTypeList.Codes.ImportLicense, BRJobMessageTypeList.Codes.LPCO });

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddAdministrativeStatusFilter(filters);
			AddCargoStatusFilter(filters);
			AddUCRNumberFilter(filters);
			AddLPCOFilter(filters);
			AddClearanceDateFilter(filters);
			AddNaladiNccaFilter(filters);
			AddNaladiHsFilter(filters);
			AddImportLicenseFilter(filters);
			AddLinkedDocumentFilter(filters);
			AddForeignDeclarationFilter(filters);
			AddIssueDateFilter(filters);
			AddRiskChannelFilter(filters);
			AddPermitFilter(filters);
			AddGoodsCatalogFilter(filters);
			AddAuthorityIdentifierFilter(filters);
			AddAuthorityVersionFilter(filters);

			return filters;
		}

		void AddUCRNumberFilter(ModuleFilterCollection filters)
		{
			var ucrNumberFilter = filters.AddTextFilter(DeclarationFilterConstants.UCRNumber, GetUCRNumberFilterQuery);
			ucrNumberFilter.Category = FilterCategories.NumbersAndReferences;
			ucrNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			ucrNumberFilter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|UCRNumber", DeclarationFilterConstants.UCRNumber);
			ucrNumberFilter.MaxLength = CusEntryNumber.Schema.CE_EntryNumMaxLength;
		}

		ZQuery GetUCRNumberFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.UniqueConsignementReference);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);

			var cusEntryHeader = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.PK);
			cusEntryHeader.AddSubQuery(cusEntryNumQuery, JoinCondition.And);

			var cusEntryInstruction = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryInstructionSchema.PK);
			cusEntryInstruction.AddSubQuery(cusEntryNumQuery, JoinCondition.And);

			declarationQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, CusEntryHeaderSchema.CH_ClusterKey, cusEntryHeader, JoinCondition.And);
			declarationQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, CusEntryInstructionSchema.CEI_ClusterKey, cusEntryInstruction, JoinCondition.Or);

			return declarationQuery;
		}

		void AddLPCOFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.Lpco, GetLPCOQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|Lpco", DeclarationFilterConstants.Lpco);
			filter.SubGroup = JobComInvLineRefsSubGroup;
		}

		ZQuery GetLPCOQuery(SQLComparisonOperator @operator, ZString value)
		{
			var lpcoQuery = new ZQuery(JobComInvLineRefsSchema.JG_ReferenceType, JobComInvLineRefsType.Codes.Lpco);
			lpcoQuery.AddToFilter_PossiblyCommaSeparated(JobComInvLineRefsSchema.JG_ReferenceNumber, @operator, value);

			return lpcoQuery;
		}

		void AddClearanceDateFilter(ModuleFilterCollection filters)
		{
			var clearanceFilter = filters.AddDateFilter(DeclarationFilterConstants.ClearanceDate, GetClearanceDateFilterQuery);
			clearanceFilter.Category = FilterCategories.Dates;
			clearanceFilter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|ClearanceDate", DeclarationFilterConstants.ClearanceDate);
			clearanceFilter.SubGroup = EntryHeaderSubGroup;
		}

		ZQuery GetClearanceDateFilterQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var dateQuery = new ZQuery();
			AddDateRange(dateQuery, comparisonOperator, JoinCondition.And, CusEntryHeaderSchema.CH_EntryReleaseDate, startDate.Date, endDate.Date);
			return dateQuery;
		}

		void AddNaladiNccaFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.NaladiNcca, GetNaladiNccaQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|NaladiNcca", DeclarationFilterConstants.NaladiNcca);
			filter.MaxLength = JobComInvoiceLine.Schema.NaladiNccaMaxLength;
			filter.SubGroup = TariffDetailSubGroup;
		}

		ZQuery GetNaladiNccaQuery(SQLComparisonOperator @operator, ZString value)
		{
			var naladiQuery = new ZQuery(CusLineTariffDetailSchema.BZ_Type, Constants.TariffTypes.NCCA);
			naladiQuery.AddToFilter(CusLineTariffDetailSchema.BZ_Tariff, @operator, value);

			return naladiQuery;
		}

		void AddNaladiHsFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.NaladiHs, GetNaladiHsQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|NaladiHs", DeclarationFilterConstants.NaladiHs);
			filter.MaxLength = JobComInvoiceLine.Schema.NaladiHsMaxLength;
			filter.SubGroup = TariffDetailSubGroup;
		}

		ZQuery GetNaladiHsQuery(SQLComparisonOperator @operator, ZString value)
		{
			var naladiQuery = new ZQuery(CusLineTariffDetailSchema.BZ_Type, Constants.TariffTypes.NALADIHS);
			naladiQuery.AddToFilter(CusLineTariffDetailSchema.BZ_Tariff, @operator, value);

			return naladiQuery;
		}

		void AddImportLicenseFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.ImportLicense, GetImportLicenseNumber);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|ImportLicense", DeclarationFilterConstants.ImportLicense);
			filter.MaxLength = ImportLicenseInfo.Schema.CSI_ReferenceNumberMaxLength;
			filter.SubGroup = InvoiceLineCusSupportingInfoSubGroup;
		}

		void AddLinkedDocumentFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.LinkedDocument, GetLinkedDocumentQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|LinkedDocument", DeclarationFilterConstants.LinkedDocument);
			filter.MaxLength = PreviousDocument.Schema.ReferenceMaxLength;
			filter.SubGroup = InvoiceLineCusSupportingInfoSubGroup;
		}

		ZQuery GetLinkedDocumentQuery(SQLComparisonOperator @operator, ZString value)
		{
			var linkedDocQuery = new ZQuery(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.PreviousDocument);
			linkedDocQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, @operator, value);

			return linkedDocQuery;
		}

		void AddForeignDeclarationFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.ForeignDeclaration, GetForeignDeclarationQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|ForeignDeclaration", DeclarationFilterConstants.ForeignDeclaration);
			filter.MaxLength = MercosulForeignDeclaration.Schema.DescriptionMaxLength;
			filter.SubGroup = InvoiceLineCusSupportingInfoSubGroup;
		}

		ZQuery GetForeignDeclarationQuery(SQLComparisonOperator @operator, ZString value)
		{
			var linkedDocQuery = new ZQuery(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.MercosulForeignDeclaration);
			linkedDocQuery.AddToFilter(CusSupportingInfoSchema.CSI_Description, @operator, value);

			return linkedDocQuery;
		}

		void AddIssueDateFilter(ModuleFilterCollection filters)
		{
			var issueDateFilter = filters.AddDateFilter(DeclarationFilterConstants.IssueDate, GetIssueDateFilterQuery);
			issueDateFilter.Category = FilterCategories.Dates;
			issueDateFilter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|IssueDate", DeclarationFilterConstants.IssueDate);
			issueDateFilter.SubGroup = EntryNumSubGroup;
		}

		ZQuery GetIssueDateFilterQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var cusEntryNumQuery = new ZQuery(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Brazil);
			cusEntryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			AddDateRange(cusEntryNumQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, startDate.Date, endDate.Date);

			return cusEntryNumQuery;
		}

		ZQuery GetImportLicenseNumber(SQLComparisonOperator @operator, ZString value)
		{
			var linkedDocQuery = new ZQuery(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.ImportLicense);
			linkedDocQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, @operator, value);

			return linkedDocQuery;
		}

		protected override void AddSubmittedDate(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.Submitted, GetSubmittedDateFilterQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|SubmittedDate", DeclarationFilterConstants.DateFilterTypes.Submitted);
			filter.SubGroup = EntryHeaderSubGroup;
		}

		ZQuery GetSubmittedDateFilterQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, CusEntryHeaderSchema.CH_EntrySubmittedDate, startDate.Date, endDate.Date);

			return query;
		}

		void AddPermitFilter(ModuleFilterCollection filters)
		{
			var permitFilter = filters.AddTextFilter(DeclarationFilterConstants.Permit, GetPermitQuery);
			permitFilter.Category = FilterCategories.NumbersAndReferences;
			permitFilter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|Permit", DeclarationFilterConstants.Permit);
			permitFilter.MaxLength = Permit.Schema.CSI_ReferenceNumberMaxLength;
			permitFilter.SubGroup = InvoiceLineCusSupportingInfoSubGroup;
		}

		ZQuery GetPermitQuery(SQLComparisonOperator @operator, ZString value)
		{
			var permitQuery = new ZQuery(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.Permit);
			permitQuery.AddToFilter(CusSupportingInfoSchema.CSI_ReferenceNumber, @operator, value);

			return permitQuery;
		}

		void AddGoodsCatalogFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.GoodsCatalog, GetGoodsCatalogFilterQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|GoodsCatalog", DeclarationFilterConstants.GoodsCatalog);
			filter.SubGroup = InvoiceLineCusGoodsCatalogSubGroup;
		}

		ZQuery GetGoodsCatalogFilterQuery(SQLComparisonOperator @operator, ZString value)
		{
			return new ZQuery(CusGoodsCatalogSchema.CGC_CatalogCode, @operator, value);
		}

		void AddAuthorityIdentifierFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.AuthorityIdentifier, GetAuthorityIdentifierFilterQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|AuthorityIdentifier", DeclarationFilterConstants.AuthorityIdentifier);
			filter.SubGroup = InvoiceLineCusGoodsCatalogSubGroup;
		}

		ZQuery GetAuthorityIdentifierFilterQuery(SQLComparisonOperator @operator, ZString value)
		{
			return new ZQuery(CusGoodsCatalogSchema.CGC_AuthorityIdentifier, @operator, value);
		}

		void AddAuthorityVersionFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.AuthorityVersion, GetAuthorityVersionFilterQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|AuthorityVersion", DeclarationFilterConstants.AuthorityVersion);
			filter.SubGroup = InvoiceLineCusGoodsCatalogSubGroup;
		}

		ZQuery GetAuthorityVersionFilterQuery(SQLComparisonOperator @operator, ZString value)
		{
			return new ZQuery(CusGoodsCatalogSchema.CGC_AuthorityVersion, @operator, value);
		}

		#region Model View Filters

		void AddAdministrativeStatusFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.AdministrativeStatus, GetAdministrativeStatusFilterQuery);
			filter.BRCustomizedStatusFilters();
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|AdministrativeStatus", DeclarationFilterConstants.AdministrativeStatus);
			filter.MaxLength = BRCusEntryHeaderSchema.CH_AdministrativeStatus.MaxLength;
		}

		void AddCargoStatusFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.CargoStatus, GetCargoStatusFilterQuery);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.BRCustomizedStatusFilters();
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|CargoStatus", DeclarationFilterConstants.CargoStatus);
			filter.MaxLength = BRCusEntryHeaderSchema.CH_CargoStatus.MaxLength;
		}

		void AddRiskChannelFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(DeclarationFilterConstants.RiskChannel, GetRiskChannelFilterQuery, Lookups.RiskChannelList);
			filter.MultilingualDescription = ResString.GetMultilingualString("DeclarationFilterConstants|RiskChannel", DeclarationFilterConstants.RiskChannel);
			filter.BRCustomizedStatusFilters();
			filter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetAdministrativeStatusFilterQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetBRCusEntryHeaderFilterQuery(ModelViewConstants.BRCusEntryHeader.AdministrativeStatus, comparisonOperator, value);

		ZQuery GetCargoStatusFilterQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetBRCusEntryHeaderFilterQuery(ModelViewConstants.BRCusEntryHeader.CargoStatus, comparisonOperator, value);

		ZQuery GetRiskChannelFilterQuery(SQLComparisonOperator filterOperator, ZString value) => GetBRCusEntryHeaderFilterQuery(ModelViewConstants.BRCusEntryHeader.RiskChannel, filterOperator, value);

		ZQuery GetBRCusEntryHeaderFilterQuery(ZString columnName, SQLComparisonOperator filterOperator, ZString value)
		{
			return EntryHeaderModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.JE_ClusterKey, ModelViewConstants.BRCusEntryHeader.ClusterKey, ModelViewConstants.BRCusEntryHeader.Name, columnName, filterOperator, value);
		}

		ModelViewColumnQueryHelper<CusEntryHeader> EntryHeaderModelViewColumnHelper => entryHeaderModelViewColumnHelper ?? (entryHeaderModelViewColumnHelper = new ModelViewColumnQueryHelper<CusEntryHeader>());
		ModelViewColumnQueryHelper<CusEntryHeader> entryHeaderModelViewColumnHelper;

		#endregion

		#region Sub Groups

		public class InvoiceHeaderCusSupportingInfoFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				var invoiceHeaderQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				var supportingInfoQuery = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID);
				supportingInfoQuery.AddToFilter(filter);
				invoiceHeaderQuery.AddSubQuery(supportingInfoQuery, JoinCondition.And);
				declarationQuery.AddSubQuery(invoiceHeaderQuery, JoinCondition.And);
				return declarationQuery;
			}
		}

		InvoiceLineCusSupportingInfoFilterSubGroup InvoiceLineCusSupportingInfoSubGroup => invoiceLineCusSupportingInfoSubGroup ?? (invoiceLineCusSupportingInfoSubGroup = new InvoiceLineCusSupportingInfoFilterSubGroup());
		InvoiceLineCusSupportingInfoFilterSubGroup invoiceLineCusSupportingInfoSubGroup;

		public class InvoiceLineCusSupportingInfoFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				var invoiceLineQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				var supportingInfoQuery = new ZDBOnlySubQuery(typeof(CusSupportingInfo), CusSupportingInfoSchema.CSI_ParentID);
				supportingInfoQuery.AddToFilter(filter);
				invoiceLineQuery.AddSubQuery(supportingInfoQuery, JoinCondition.And);
				declarationQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
				return declarationQuery;
			}
		}

		JobComInvLineRefsFilterSubGroup JobComInvLineRefsSubGroup => invLineRefsFilterSubGroup ?? (invLineRefsFilterSubGroup = new JobComInvLineRefsFilterSubGroup());
		JobComInvLineRefsFilterSubGroup invLineRefsFilterSubGroup;

		class JobComInvLineRefsFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				var invoiceLineQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				var supportingInfoQuery = new ZDBOnlySubQuery(typeof(JobComInvLineRefs), JobComInvLineRefsSchema.JG_JI);
				supportingInfoQuery.AddToFilter(filter);
				invoiceLineQuery.AddSubQuery(supportingInfoQuery, JoinCondition.And);
				declarationQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
				return declarationQuery;
			}
		}

		EntryHeaderFilterSubGroup EntryHeaderSubGroup => entryHeaderSubgroup ?? (entryHeaderSubgroup = new EntryHeaderFilterSubGroup());
		EntryHeaderFilterSubGroup entryHeaderSubgroup;

		class EntryHeaderFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				var entryHeaderSubgroup = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				entryHeaderSubgroup.AddToFilter(filter);
				declarationQuery.AddSubQuery(entryHeaderSubgroup, JoinCondition.And);
				return declarationQuery;
			}
		}

		CusLineTariffDetailFilterSubGroup TariffDetailSubGroup => tariffDetailSubGroup ?? (tariffDetailSubGroup = new CusLineTariffDetailFilterSubGroup());
		CusLineTariffDetailFilterSubGroup tariffDetailSubGroup;

		class CusLineTariffDetailFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				var invoiceLineQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				var tariffDetailQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusLineTariffDetail), CusLineTariffDetailSchema.BZ_ParentID);
				tariffDetailQuery.AddToFilter(filter);
				invoiceLineQuery.AddSubQuery(tariffDetailQuery, JoinCondition.And);
				declarationQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
				return declarationQuery;
			}
		}

		CusEntryNumFilterSubGroup EntryNumSubGroup => entryNumSubGroup ?? (entryNumSubGroup = new CusEntryNumFilterSubGroup());
		CusEntryNumFilterSubGroup entryNumSubGroup;

		public class CusEntryNumFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberQuery.AddToFilter(filter);
				entryHeaderQuery.AddSubQuery(entryNumberQuery, JoinCondition.And);
				declarationQuery.AddSubQuery(entryHeaderQuery, JoinCondition.And);
				return declarationQuery;
			}
		}

		InvoiceLineCusGoodsCatalogFilterSubGroup InvoiceLineCusGoodsCatalogSubGroup => invoiceLineCusGoodsCatalogSubGroup ?? (invoiceLineCusGoodsCatalogSubGroup = new InvoiceLineCusGoodsCatalogFilterSubGroup());
		InvoiceLineCusGoodsCatalogFilterSubGroup invoiceLineCusGoodsCatalogSubGroup;

		public class InvoiceLineCusGoodsCatalogFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				var invoiceLineQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				var cusGoodsCatalogQuery = new ZDBOnlySubQuery(typeof(CusGoodsCatalog), CusGoodsCatalogSchema.PK);
				cusGoodsCatalogQuery.AddToFilter(filter);
				invoiceLineQuery.AddSubQuery(JobComInvoiceLineSchema.JI_CGC_Catalog, cusGoodsCatalogQuery, JoinCondition.And);
				declarationQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);
				return declarationQuery;
			}
		}

		#endregion

		#endregion
	}
}
