using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.AU.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject, Integration.Customs.AU.IJobDeclarationFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			if (QuarantineColsHeader.IsCOLSFunctionEnabled)
			{
				var colsNumberSubGroup = new COLSNumberFilterSubGroup();
				var colsLodgementReferenceNumber = result.AddNumberFilter(DeclarationFilterConstants.COLSLodgementReferenceNumber, GetCOLSEntryNumberQuery);
				colsLodgementReferenceNumber.MultilingualDescription = ResString.GetMultilingualString("AUJobDeclarationFilter|COLSLodgementReferenceNumber", "COLS Lodgement Reference Number");
				colsLodgementReferenceNumber.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
				colsLodgementReferenceNumber.Category = FilterCategories.NumbersAndReferences;

				var colsLodgementReferenceNumberStatus = result.AddTextFilter(DeclarationFilterConstants.COLSLodgementReferenceNumberStatus, CusEntryNumSchema.CE_EntryStatus, Lookups.COLSEntryNumberStatusList);
				colsLodgementReferenceNumberStatus.MultilingualDescription = ResString.GetMultilingualString("AUJobDeclarationFilter|COLSLodgementReferenceNumberStatus", "COLS Lodgement Reference Number Status");
				colsLodgementReferenceNumberStatus.MaxLength = CusEntryNumSchema.CE_EntryStatus.MaxLength;
				colsLodgementReferenceNumberStatus.Category = FilterCategories.StatusAndFlags;
				colsLodgementReferenceNumberStatus.SubGroup = colsNumberSubGroup;
				colsLodgementReferenceNumberStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
				colsLodgementReferenceNumberStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
				colsLodgementReferenceNumberStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				colsLodgementReferenceNumberStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

				var colsEntryStatus = result.AddTextFilter(DeclarationFilterConstants.COLSEntryStatus, GetCOLSEntryStatusQuery, Lookups.COLSLodgementStatusList);
				colsEntryStatus.MultilingualDescription = ResString.GetMultilingualString("AUJobDeclarationFilter|COLSEntryStatus", "COLS Entry Status");
				colsEntryStatus.MaxLength = QuarantineColsHeaderSchema.QCH_LodgementStatus.MaxLength;
				colsEntryStatus.Category = FilterCategories.StatusAndFlags;
				colsEntryStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
				colsEntryStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
				colsEntryStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				colsEntryStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

				var colsMessageStatus = result.AddTextFilter(DeclarationFilterConstants.COLSMessageStatus, GetCOLSMessageStatusQuery, Lookups.COLSMessageStatusList);
				colsMessageStatus.MultilingualDescription = ResString.GetMultilingualString("AUJobDeclarationFilter|COLSMessageStatus", "COLS Message Status");
				colsMessageStatus.MaxLength = QuarantineColsHeaderSchema.QCH_MessageStatus.MaxLength;
				colsMessageStatus.Category = FilterCategories.StatusAndFlags;
				colsMessageStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
				colsMessageStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
				colsMessageStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				colsMessageStatus.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			}

			var consignRefNumberFilter = result.AddNumberFilter(DeclarationFilterConstants.ConsignmentRefNumber, GetConsignmentRefNumberQuery);
			consignRefNumberFilter.MaxLength = Bill.Schema.CU_fPartShipConsignmentReferenceMaxLength;
			consignRefNumberFilter.Category = FilterCategories.NumbersAndReferences;
			consignRefNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			consignRefNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			consignRefNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			consignRefNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			consignRefNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);

			var rfpNumberFilter = result.AddNumberFilter(DeclarationFilterConstants.RFPNumber, GetRFPNumberQuery);
			rfpNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			rfpNumberFilter.Category = FilterCategories.NumbersAndReferences;

			var rfpStatusFilter = result.AddTextFilter(DeclarationFilterConstants.RFPStatus, GetRFPStatusQuery, Lookups.RFPStatusList);
			rfpStatusFilter.MaxLength = 5;
			rfpStatusFilter.Category = FilterCategories.StatusAndFlags;

			var exportPermitNumberFilter = result.AddTextFilter(DeclarationFilterConstants.ExportPermitNumber, GetExportPermitNumberQuery);
			exportPermitNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			exportPermitNumberFilter.Category = FilterCategories.NumbersAndReferences;

			var produceTypeFilter = result.AddTextFilter(DeclarationFilterConstants.QuarantineProduceType, GetProduceTypeQuery, Lookups.ProduceTypeList);
			produceTypeFilter.MaxLength = QuarantineExDocHeaderSchema.QH_ProduceType.MaxLength;
			produceTypeFilter.Category = FilterCategories.NumbersAndReferences;

			var customsMessageStatusFilter = result.AddTextFilter(DeclarationFilterConstants.CustomsMessageStatus, GetCustomsMessageStatusQuery, Lookups.CMRMessageStatusList);
			customsMessageStatusFilter.Category = FilterCategories.StatusAndFlags;

			var customsEntryStatusFilter = result.AddTextFilter(DeclarationFilterConstants.CustomsEntryStatus, JobDeclarationSchema.JE_EntryStatus, Lookups.CMREntryStatusList);
			customsEntryStatusFilter.Category = FilterCategories.StatusAndFlags;
			customsEntryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			customsEntryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			customsEntryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			customsEntryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			customsEntryStatusFilter.ComparisonOperatorChanged += CustomsEntryStatusFilter_ComparisonOperatorChanged;

			var paymentStatusFilter = result.AddTextFilter(DeclarationFilterConstants.PaymentStatusText, GetPaymentStatusQuery, Lookups.PaymentStatusList);
			paymentStatusFilter.Category = FilterCategories.StatusAndFlags;

			var natureTypeFilter = result.AddTextFilter(DeclarationFilterConstants.NatureType, GetNatureTypeQuery, Lookups.NatureTypeList);
			natureTypeFilter.Category = FilterCategories.NumbersAndReferences;

			var consolidatedCargoStatusFilter = result.AddTextFilter(DeclarationFilterConstants.CustomsConsolidatedCargoStatus, GetConsolidatedCargoStatusStatusQuery, Lookups.CMRConsolidatedCargoStatusList);
			consolidatedCargoStatusFilter.Category = FilterCategories.StatusAndFlags;

			result.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.EntryPaymentDate, GetEntryPaymentDateQuery);

			return result;
		}

		protected override void AddMessageStatusFilter(ModuleFilterCollection filters)
		{
			if (IsForConsolidation)
			{
				base.AddMessageStatusFilter(filters);
			}
		}

		protected ZQuery GetConsignmentRefNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var billQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
			var billAddOnColumnQuery = new GenAddOnColumnQueryHelper(typeof(Bill)).GetQueryOnGenAddOnColumn(Bill.Schema.CU_fPartShipConsignmentReference, comparisonOperator, value);
			billQuery.AddToFilter(billAddOnColumnQuery);
			result.AddSubQuery(billQuery, JoinCondition.And);
			return result;
		}

		#region RFP Filter

		protected ZQuery GetRFPNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZDBOnlySubQuery invoiceHeaderQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			ZDBOnlySubQuery quarantineExDocHeader = new ZDBOnlySubQuery(typeof(QuarantineExDocHeader), QuarantineExDocHeaderSchema.QH_JZ);
			ZDBOnlySubQuery cusEntryNumber = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumber.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Australia);
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.RequestForPermitStatus);

			quarantineExDocHeader.AddSubQuery(cusEntryNumber, JoinCondition.And);
			invoiceHeaderQuery.AddSubQuery(quarantineExDocHeader, JoinCondition.And);
			result.AddSubQuery(invoiceHeaderQuery, JoinCondition.And);
			return result;
		}

		protected ZQuery GetRFPStatusQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZDBOnlySubQuery invoiceHeaderQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			ZDBOnlySubQuery quarantineExDocHeader = new ZDBOnlySubQuery(typeof(QuarantineExDocHeader), QuarantineExDocHeaderSchema.QH_JZ);
			ZDBOnlySubQuery cusEntryNumber = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumber.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryStatus, value.Left(3));
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Australia);
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.RequestForPermitStatus);

			quarantineExDocHeader.AddSubQuery(cusEntryNumber, JoinCondition.And);
			invoiceHeaderQuery.AddSubQuery(quarantineExDocHeader, JoinCondition.And);
			result.AddSubQuery(invoiceHeaderQuery, JoinCondition.And);
			return result;
		}

		protected ZQuery GetExportPermitNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZDBOnlySubQuery invoiceHeaderQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			ZDBOnlySubQuery quarantineExDocHeader = new ZDBOnlySubQuery(typeof(QuarantineExDocHeader), QuarantineExDocHeaderSchema.QH_JZ);
			ZDBOnlySubQuery cusEntryNumber = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumber.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Australia);
			cusEntryNumber.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.ExdocPermitNumber);

			quarantineExDocHeader.AddSubQuery(cusEntryNumber, JoinCondition.And);
			invoiceHeaderQuery.AddSubQuery(quarantineExDocHeader, JoinCondition.And);
			result.AddSubQuery(invoiceHeaderQuery, JoinCondition.And);
			return result;
		}

		protected ZQuery GetProduceTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZDBOnlySubQuery invoiceHeaderQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			ZDBOnlySubQuery quarantineExDocHeader = new ZDBOnlySubQuery(typeof(QuarantineExDocHeader), QuarantineExDocHeaderSchema.QH_JZ);
			quarantineExDocHeader.AddToFilter(JoinCondition.And, QuarantineExDocHeaderSchema.QH_ProduceType, comparisonOperator, value);

			invoiceHeaderQuery.AddSubQuery(quarantineExDocHeader, JoinCondition.And);
			result.AddSubQuery(invoiceHeaderQuery, JoinCondition.And);
			return result;
		}

		#endregion

		public new JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		protected override bool SupportWHSStatus
		{
			get { return true; }
		}

		#region COLS Filter

		ZQuery GetCOLSEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var colsHeaderQuery = new ZDBOnlySubQuery(typeof(QuarantineColsHeader), QuarantineColsHeaderSchema.QCH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);

			bool notIn = comparisonOperator == SpecialComparisonOperator.IsBlank;

			var entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, notIn);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Australia);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, QuarantineColsHeader.Schema.TableName);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			if (comparisonOperator == SpecialComparisonOperator.IsNotBlank || !value.IsEmpty)
			{
				entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value);
			}

			colsHeaderQuery.AddSubQuery(entryNumQuery, JoinCondition.And);
			declarationQuery.AddSubQuery(colsHeaderQuery, JoinCondition.And);
			return declarationQuery;
		}

		ZQuery GetCOLSEntryStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var colsHeaderQuery = new ZDBOnlySubQuery(typeof(QuarantineColsHeader), QuarantineColsHeaderSchema.QCH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			colsHeaderQuery.AddToFilter(QuarantineColsHeaderSchema.QCH_LodgementStatus, comparisonOperator, value);
			declarationQuery.AddSubQuery(colsHeaderQuery, JoinCondition.And);
			return declarationQuery;
		}

		ZQuery GetCOLSMessageStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var colsHeaderQuery = new ZDBOnlySubQuery(typeof(QuarantineColsHeader), QuarantineColsHeaderSchema.QCH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
			switch (value)
			{
				case DeclarationFilterConstants.COLSExtraMessageStatus.Failed:
				case DeclarationFilterConstants.COLSExtraMessageStatus.Success:
					var compareOperator = comparisonOperator == SQLComparisonOperator.Equal ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.DoesNotStartWith;
					var compareValue = value == DeclarationFilterConstants.COLSExtraMessageStatus.Failed ? "F" : "S";
					colsHeaderQuery.AddToFilter(QuarantineColsHeaderSchema.QCH_MessageStatus, compareOperator, compareValue);
					break;
				default:
					colsHeaderQuery.AddToFilter(QuarantineColsHeaderSchema.QCH_MessageStatus, comparisonOperator, value);
					break;
			}
			declarationQuery.AddSubQuery(colsHeaderQuery, JoinCondition.And);
			return declarationQuery;
		}

		sealed class COLSNumberFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.LocalReferenceNumber);
				entryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Australia);
				entryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, QuarantineColsHeader.Schema.TableName);
				entryNumQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				entryNumQuery.AddToFilter(filter);
				var colsHeaderQuery = new ZDBOnlySubQuery(typeof(QuarantineColsHeader), QuarantineColsHeaderSchema.QCH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				colsHeaderQuery.AddSubQuery(entryNumQuery, JoinCondition.And);
				var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				declarationQuery.AddSubQuery(colsHeaderQuery, JoinCondition.And);
				return declarationQuery;
			}
		}

		#endregion

		#region PaymentStatusFilter

		protected ZQuery GetPaymentStatusQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (value == DeclarationFilterConstants.PaymentStatus.Paid)
			{
				ZDBOnlySubQuery headerQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				headerQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_AddInfo, SQLComparisonOperator.Contains, AUAddInfoSchema.ZA_PaymentStatus_Hidden.Name.Substring(3) + "=" + CMREntryPaymentStatusList.Codes.Paid);
				headerQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_AddInfo, SQLComparisonOperator.Contains, AUAddInfoSchema.ZA_PaymentStatus_Hidden.Name.Substring(3) + "=" + CMREntryPaymentStatusList.Codes.Refunded);
				ZDBOnlyQuery jobDecQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				jobDecQuery.AddSubQuery(headerQuery, JoinCondition.And);
				result.AddToFilter(jobDecQuery);
			}
			else if (value == DeclarationFilterConstants.PaymentStatus.NotPaid)
			{
				ZDBOnlySubQuery headerQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				ZDBOnlySubQuery allHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, true);
				headerQuery.AddToFilter(JoinCondition.And, CusEntryHeaderSchema.CH_AddInfo, SQLComparisonOperator.NotContains, AUAddInfoSchema.ZA_PaymentStatus_Hidden.Name.Substring(3) + "=" + CMREntryPaymentStatusList.Codes.Paid);
				headerQuery.AddToFilter(JoinCondition.And, CusEntryHeaderSchema.CH_AddInfo, SQLComparisonOperator.NotContains, AUAddInfoSchema.ZA_PaymentStatus_Hidden.Name.Substring(3) + "=" + CMREntryPaymentStatusList.Codes.Refunded);
				ZDBOnlyQuery jobDecQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				jobDecQuery.AddSubQuery(headerQuery, JoinCondition.And);
				jobDecQuery.AddSubQuery(allHeaderQuery, JoinCondition.Or);
				result.AddToFilter(jobDecQuery);
			}
			return result;
		}

		#endregion

		#region NatureFilter

		protected ZQuery GetNatureTypeQuery(ZString value)
		{
			ZQuery result = new ZQuery();

			if (value == DeclarationFilterConstants.NatureTypes.Nature30)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.Equal, JobMessageTypeList.Codes.ExWarehouse);
			}
			else if (value == DeclarationFilterConstants.NatureTypes.Nature10 || value == DeclarationFilterConstants.NatureTypes.Nature20)
			{
				ZDBOnlyQuery decQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				ZDBOnlySubQuery invHeaderQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				ZDBOnlySubQuery invLineQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ);
				SQLComparisonOperator comparisonOperator =
					(value == DeclarationFilterConstants.NatureTypes.Nature20)
					? SQLComparisonOperator.Contains : SQLComparisonOperator.NotContains;

				invLineQuery.AddToFilter(JoinCondition.And, JobComInvoiceLineSchema.JI_AddInfo, comparisonOperator, "IsPackToBondForLine_Hidden=Y");

				invHeaderQuery.AddSubQuery(invLineQuery, JoinCondition.And);
				decQuery.AddSubQuery(invHeaderQuery, JoinCondition.And);
				result.AddToFilter(decQuery);
			}

			return result;
		}

		#endregion

		#region MessageStatusFilter

		protected ZQuery GetCustomsMessageStatusQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (value == DeclarationFilterConstants.EntryStatus.NotSentForFilter)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageStatus, ZString.Empty);
				result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_ApplicationCode, SQLComparisonOperator.Equal, Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages);
			}
			else
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageStatus, value);
			}
			return result;
		}

		#endregion

		#region ConsolidatedCargoStatusFilter

		protected ZQuery GetConsolidatedCargoStatusStatusQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (value == DeclarationFilterConstants.ConsolidatedCargoStatus.NotClearForFilter)
			{
				result.AddToFilter(JobDeclarationSchema.JE_ConsolidatedCargoStatus, SQLComparisonOperator.NotEqual, ZString.Empty);
				result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_ConsolidatedCargoStatus, SQLComparisonOperator.NotEqual, CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased);
				result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_ConsolidatedCargoStatus, SQLComparisonOperator.NotEqual, CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement);
				result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_ConsolidatedCargoStatus, SQLComparisonOperator.NotEqual, CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation);
			}
			else
			{
				result.AddToFilter(JobDeclarationSchema.JE_ConsolidatedCargoStatus, value);
			}
			return result;
		}

		#endregion

		#region JE_DateTimeFilterTypeFilterControlFilter

		ZQuery GetEntryPaymentDateQuery(DateComparisonOperator comaprisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

			ZDBOnlySubQuery cusEntryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);

			ZDBOnlySubQuery eDIMessageQuery = new ZDBOnlySubQuery(typeof(CMRMessage), EDIMessageSchema.EM_LinkUniqueID);
			eDIMessageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, "PAR");

			AddDateTimeRange(eDIMessageQuery, comaprisonOperator, JoinCondition.And, EDIMessageSchema.EM_SystemCreateTimeUtc, value1, value2);

			cusEntryHeaderQuery.AddSubQuery(eDIMessageQuery, JoinCondition.And);
			jobDeclarationQuery.AddSubQuery(cusEntryHeaderQuery, JoinCondition.And);
			return jobDeclarationQuery;
		}

		#endregion
	}
}
