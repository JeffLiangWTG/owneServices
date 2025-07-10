using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public abstract class ComplianceDocumentFilterStripBusinessObject : AccountingFilterStripBusinessObject
	{
		public abstract ZString LedgerType { get; }

		public override ZQuery Filter
		{
			get
			{
				ZQuery resultFilter = base.Filter;
				AddLedgerFilter(resultFilter);

				return resultFilter;
			}
		}

		protected virtual void AddLedgerFilter(ZQuery filter)
		{
			filter.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, LedgerType);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var moduleFilters = new ModuleFilterCollection();

			AddNumberFilters(moduleFilters);
			AddDateFilters(moduleFilters);
			AddStatusesFilter(moduleFilters);
			AddModesAndTypesFilters(moduleFilters);
			AddTextFilters(moduleFilters);

			return moduleFilters;
		}

		#region Date Filters

		protected virtual void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(AccountingConstants.DateFilterTypes.DocumentDate, GetDocumentDateQuery).MultilingualDescription = ResString.GetMultilingualString("8B212E37-4D95-4ACA-856D-ED6EFA8C6B70", "Document Date");
		}

		ZQuery GetDocumentDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, AccComplianceDocumentHeaderSchema.ADH_DocumentDate, fromDate.Date, toDate.Date);
			return query;
		}

		#endregion

		#region Text Filters
		protected virtual void AddTextFilters(ModuleFilterCollection filters)
		{
			AddPeriodFilter(filters);
		}
		#endregion

		void AddPeriodFilter(ModuleFilterCollection filters)
		{
			ModuleTextRangeFilter periodFilter = filters.AddTextRangeFilter("Accounting Period", GetPeriodQuery);
			periodFilter.Property1Validation = ValidateAccountingPeriod;
			periodFilter.Property2Validation = ValidateAccountingPeriod;
			periodFilter.MultilingualDescription = ResString.GetMultilingualString("EFC1AA69-8AC2-4F49-A45E-A82C356E635D", "Accounting Period");
		}

		protected void ValidateAccountingPeriod(ZPropertyInfo info)
		{
			var valueAsString = (ZString)info.Value;
			if (!valueAsString.IsEmpty)
			{
				if (ZInt.CanParse(valueAsString))
				{
					var period = ZInt.Parse(valueAsString);
					if (!new AccountingPeriodCalculator(Factory).IsPeriodValid(period))
					{
						info.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(period));
					}
				}
				else
				{
					info.AddError(AccountingPeriodCalculator.GetInvalidPeriodValidationError(valueAsString));
				}
			}
		}

		ZQuery GetPeriodQuery(ZString from, ZString to)
		{
			ZInt startPeriod = 0;
			ZInt endPeriod = 0;

			var query = new ZDBOnlyQuery(typeof(AccComplianceDocumentHeader));

			if (!from.IsEmpty)
			{
				if (ZInt.CanParse(from))
				{
					startPeriod = ZInt.Parse(from);
					query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_ReportingPeriod, SQLComparisonOperator.GreaterThanOrEqualTo, startPeriod);
				}
			}

			if (!to.IsEmpty)
			{
				if (ZInt.CanParse(to))
				{
					endPeriod = ZInt.Parse(to);
					query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_ReportingPeriod, SQLComparisonOperator.LessThanOrEqualTo, endPeriod);
				}
			}

			return query;
		}

		#region Number Filters
		protected virtual void AddNumberFilters(ModuleFilterCollection filters)
		{
			AddJobNumberFilter(filters);
			AddTransactionNumberFilter(filters);
			AddComplianceDocumentNumberFilter(filters);
			AddInternalReferenceFilter(filters);
		}

		protected virtual void AddTransactionNumberFilter(ModuleFilterCollection filters)
		{
			var transactionNumFilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.TransactionNumber, GetTransactionNumber);
			transactionNumFilter.MaxLength = AccTransactionHeaderSchema.AH_TransactionNum.MaxLength;
			transactionNumFilter.MultilingualDescription = ResString.GetMultilingualString("B3F14095-FADA-44C5-A344-0D8C2531414F", "Transaction #");
		}

		ZQuery GetTransactionNumber(SQLComparisonOperator comparisionOperator, ZString transactionNumber)
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceDocumentHeader));
			var subQuery1 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentLine), AccComplianceDocumentLineSchema.ADL_ADH);
			var subQuery2 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_ADL);
			var subquery3 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			var subquery4 = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			subquery4.AddToFilter(new ZQuery().AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderSchema.AH_TransactionNum, comparisionOperator, transactionNumber));

			subquery3.AddSubQuery(AccTransactionLinesSchema.AL_AH, subquery4, JoinCondition.And);
			subQuery2.AddSubQuery(AccComplianceDocumentPivotSchema.ADP_AL, subquery3, JoinCondition.And);
			subQuery1.AddSubQuery(AccComplianceDocumentLineSchema.PK, subQuery2, JoinCondition.And);
			query.AddSubQuery(AccComplianceDocumentHeaderSchema.PK, subQuery1, JoinCondition.And);

			return query;
		}

		protected virtual void AddComplianceDocumentNumberFilter(ModuleFilterCollection filters)
		{
			var documentNumFilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.DocumentNumber, AccComplianceDocumentHeaderSchema.ADH_DocumentNumber);
			documentNumFilter.MaxLength = AccComplianceDocumentHeaderSchema.ADH_DocumentNumber.MaxLength;
			documentNumFilter.MultilingualDescription = ResString.GetMultilingualString("50DE1C81-5533-4D58-9928-DF6CB51B9E41", "Document #");
		}

		protected virtual void AddInternalReferenceFilter(ModuleFilterCollection filters)
		{
			var internalReferenceFilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.InternalReferenceNumber, AccComplianceDocumentHeaderSchema.ADH_InternalReference);
			internalReferenceFilter.MaxLength = AccComplianceDocumentHeaderSchema.ADH_InternalReference.MaxLength;
			internalReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("3B22BF97-5484-4A0B-951C-F86440E8B522", "Internal Reference #");
		}

		protected virtual void AddJobNumberFilter(ModuleFilterCollection filters)
		{
			var jobNumFilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.JobNumber, GetJobNumberQuery);
			jobNumFilter.MaxLength = JobHeaderSchema.JH_JobNum.MaxLength;
			jobNumFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ComplianceDocumentFilter|JobNumber", "Job #");
		}

		ZQuery GetJobNumberQuery(SQLComparisonOperator comparisionOperator, ZString jobNumber)
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceDocumentHeader));
			var subQuery1 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentLine), AccComplianceDocumentLineSchema.ADL_ADH);
			var subQuery2 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_ADL);
			var subquery3 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			var subquery4 = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			subquery4.AddToFilter(new ZQuery().AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobNum, comparisionOperator, jobNumber));

			subquery3.AddSubQuery(AccTransactionLinesSchema.AL_JH, subquery4, JoinCondition.And);
			subQuery2.AddSubQuery(AccComplianceDocumentPivotSchema.ADP_AL, subquery3, JoinCondition.And);
			subQuery1.AddSubQuery(AccComplianceDocumentLineSchema.PK, subQuery2, JoinCondition.And);
			query.AddSubQuery(AccComplianceDocumentHeaderSchema.PK, subQuery1, JoinCondition.And);

			return query;
		}
		#endregion

		#region Status Filters

		protected virtual void AddStatusesFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter documentStatusFilter = filters.AddTextFilter("Document Status", DocumentStatusQuery, ComplianceStatusList);
			documentStatusFilter.Category = FilterCategories.StatusAndFlags;
			documentStatusFilter.MaxLength = AccComplianceDocumentHeaderSchema.ADH_DocumentStatus.MaxLength;
			documentStatusFilter.MultilingualDescription = ResString.GetMultilingualString("C13DEA40-0FD0-4DE5-90FA-0E3107067896", "Document Status");
		}

		ZQuery DocumentStatusQuery(SQLComparisonOperator comparisonOperator, ZString status)
		{
			var query = new ZQuery();

			if (!status.IsEmpty)
			{
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_DocumentStatus, comparisonOperator, status);
			}

			return query;
		}

		protected CodeDescriptionPairList fComplianceStatusList;
		public virtual CodeDescriptionPairList ComplianceStatusList
		{
			get
			{
				if (fComplianceStatusList == null)
				{
					fComplianceStatusList = new CodeDescriptionPairList(OLookUpEditType.ComplianceDocumentStatus);
				}
				return fComplianceStatusList;
			}
		}

		#endregion

		#region Modes And Types Filters

		protected virtual void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			AddComplianceDocumentSubType(filters);
			AddTransactionTypeFilter(filters);
		}

		void AddComplianceDocumentSubType(ModuleFilterCollection filters)
		{
			ModuleTextFilter complianceSubTypeFilter = filters.AddTextFilter("Compliance Sub Type", ComplianceSubTypeQuery, ComplianceSubTypeList);
			complianceSubTypeFilter.Category = FilterCategories.ModesAndTypes;
			complianceSubTypeFilter.MaxLength = AccComplianceDocumentHeaderSchema.ADH_ComplianceSubType.MaxLength;
			complianceSubTypeFilter.MultilingualDescription = ResString.GetMultilingualString("220AED7E-B576-4E70-8F46-CDE68BA3E79F", "Compliance Sub Type");
		}

		ZQuery ComplianceSubTypeQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_ComplianceSubType, SQLComparisonOperator.Equal, value);
			}
			return query;
		}

		protected CodeDescriptionPairList fComplianceSubTypeList;
		public CodeDescriptionPairList ComplianceSubTypeList
		{
			get
			{
				if (fComplianceSubTypeList == null)
				{
					fComplianceSubTypeList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}
				return fComplianceSubTypeList;
			}
		}

		void AddTransactionTypeFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter transactionTypeFilter = filters.AddTextFilter("Transaction Type", GetTransactionTypeQuery, GetTransactionTypeList);
			transactionTypeFilter.Category = FilterCategories.ModesAndTypes;
			transactionTypeFilter.MaxLength = AccTransactionHeaderSchema.AH_TransactionType.MaxLength;
			transactionTypeFilter.MultilingualDescription = ResString.GetMultilingualString("60B302AC-017B-4681-8F64-1C537208F99D", "Transaction Type");
		}

		protected virtual ZQuery GetTransactionTypeQuery(ZString transactionType)
		{
			var query = new ZDBOnlyQuery(typeof(AccComplianceDocumentHeader));
			if (transactionType != "ALL")
			{
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_TransactionType, transactionType);
			}
			return query;
		}

		public CodeDescriptionPairList GetTransactionTypeList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("ALL", Res.GetString("BAC52839-11D1-486A-A23B-BE13E01D0BD0", "All Transaction Types"));
			list.AddPair(TransactionTypes.Invoice, Res.GetString("6E6439A7-8655-4C34-BE63-B0F6A999B204", "Invoice"));
			list.AddPair(TransactionTypes.CreditNote, Res.GetString("6C85C971-3C3C-45DD-95D2-DADA87FEBBBE", "Credit Note"));
			return list;
		}

		#endregion

		AccComplianceDocumentHeaderCollection fComplianceDocumentHeaders;
		public AccComplianceDocumentHeaderCollection ComplianceDocumentHeaders
		{
			get
			{
				if (fComplianceDocumentHeaders == null)
				{
					fComplianceDocumentHeaders = new AccComplianceDocumentHeaderCollection(Factory);
					fComplianceDocumentHeaders.SetReadOnlyIncludingChildren(true);
				}
				return fComplianceDocumentHeaders;
			}
		}

		AccComplianceDocumentLineCollection fComplianceDocumentLines;
		public AccComplianceDocumentLineCollection ComplianceDocumentLines
		{
			get
			{
				if (fComplianceDocumentLines == null)
				{
					fComplianceDocumentLines = new AccComplianceDocumentLineCollection(Factory);
					fComplianceDocumentLines.SetReadOnlyIncludingChildren(true);
				}
				return fComplianceDocumentLines;
			}
		}

		public void SetComplianceDocumentLinesCurrentLinesForHeader(AccComplianceDocumentHeader selectedHeader)
		{
			ComplianceDocumentLines.RemoveAll();
			ComplianceDocumentLines.AddRange(selectedHeader.ComplianceDocumentLines);
		}
	}
}
