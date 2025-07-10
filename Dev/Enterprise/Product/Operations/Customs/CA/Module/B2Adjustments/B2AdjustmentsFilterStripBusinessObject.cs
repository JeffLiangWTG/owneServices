using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	public class B2AdjustmentsFilterStripBusinessObject : CommonJobDeclarationFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var entryNumberFilter = result.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.TransactionNumber, GetEntryNumberQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(CusEntryNumSchema.CE_EntryNum);
			entryNumberFilter.IsCommon = true;
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CA|B2AdjustmentsFilterStripBusinessObject|TransactionNumber", DeclarationFilterConstants.NumberFilterTypes.TransactionNumber);

			AddBranchAndBrokerFilters(result);

			var portOfClearance = result.AddNkFilter(DeclarationFilterConstants.PortFilterTypes.PortOfClearance, GetPortOfClearanceQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CBSAOffices).WithMaxLengthOf<ModuleNkFilter>(JobDeclarationSchema.JE_CustomsOffice);
			portOfClearance.Category = FilterCategories.Locations;
			portOfClearance.MultilingualDescription = ResString.GetMultilingualString("CA|B2AdjustmentsFilterStripBusinessObject|PortOfClearance", DeclarationFilterConstants.PortFilterTypes.PortOfClearance);

			result.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ReleaseDate, JobDeclarationSchema.JE_EntryAuthorisationDate)
				.MultilingualDescription = ResString.GetMultilingualString("CA|B2AdjustmentsFilterStripBusinessObject|ReleaseDate", DeclarationFilterConstants.DateFilterTypes.ReleaseDate);
			result.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.K84AccountingDate, GetK84AccountingDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("CA|B2AdjustmentsFilterStripBusinessObject|K84AccountingDate", DeclarationFilterConstants.DateFilterTypes.K84AccountingDate);
			result.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.SubmittedDate, GetSubmittedDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("CA|B2AdjustmentsFilterStripBusinessObject|SubmittedDate", DeclarationFilterConstants.DateFilterTypes.SubmittedDate);
			result.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ConfirmedDate, GetConfirmedDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("CA|B2AdjustmentsFilterStripBusinessObject|ConfirmedDate", DeclarationFilterConstants.DateFilterTypes.ConfirmedDate);
			result.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.DecisionDate, GetDecisionDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("CA|B2AdjustmentsFilterStripBusinessObject|DecisionDate", DeclarationFilterConstants.DateFilterTypes.DecisionDate);
			result.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.Consignee, ModuleIDs.Organisation, GetConsigneeQuery, Lookups.Consignees)
				.MultilingualDescription = ResString.GetMultilingualString("CA|B2AdjustmentsFilterStripBusinessObject|Consignee", DeclarationFilterConstants.OrgFilterTypes.Consignee);

			var b2TypeFilter = result.AddTextFilter(DeclarationFilterConstants.B2Type, GetB2TypeQuery, Lookups.B2Types)
				.WithMaxLengthOf<ModuleTextFilter>(CAAddInfoSchema.CA_B2Type);
			b2TypeFilter.Category = FilterCategories.ModesAndTypes;
			b2TypeFilter.MultilingualDescription = ResString.GetMultilingualString("CA|B2AdjustmentsFilterStripBusinessObject|B2Type", DeclarationFilterConstants.B2Type);

			var originalTransactionNoFilter = result.AddTextFilter(DeclarationFilterConstants.NumberFilterTypes.OriginalTransactionNo, GetOriginalTransactionNo)
				.WithMaxLengthOf<ModuleTextFilter>(CAAddInfoSchema.CA_OriginalTransactionNo);
			originalTransactionNoFilter.Category = FilterCategories.NumbersAndReferences;
			originalTransactionNoFilter.MultilingualDescription = ResString.GetMultilingualString("CA|B2AdjustmentsFilterStripBusinessObject|OriginalTransactionNo", DeclarationFilterConstants.NumberFilterTypes.OriginalTransactionNo);

			return result;
		}

		ZQuery GetB2TypeQuery(ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(CAAddInfoSchema.Constants.CA_B2Type, SQLComparisonOperator.Equal, value);
		}

		ZQuery GetOriginalTransactionNo(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(CAAddInfoSchema.Constants.CA_OriginalTransactionNo, comparisonOperator, value);
		}

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_MessageType, new[] { JobMessageTypeList.Codes.B2Adjustments, JobMessageTypeList.Codes.ImportCopyforB2, JobMessageTypeList.Codes.XTypeEntry });
				return result;
			}
		}

		protected ZQuery GetSubmittedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryOnGenAddOnColumn(CAAddInfoSchema.Constants.CA_B2SubmissionDate, comparisonOperator, startDate, endDate);
		}

		protected ZQuery GetConfirmedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryOnGenAddOnColumn(CAAddInfoSchema.Constants.CA_ConfirmedDate, comparisonOperator, startDate, endDate);
		}

		protected ZQuery GetDecisionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryOnGenAddOnColumn(CAAddInfoSchema.Constants.CA_B2AcceptedDate, comparisonOperator, startDate, endDate);
		}
	}
}
