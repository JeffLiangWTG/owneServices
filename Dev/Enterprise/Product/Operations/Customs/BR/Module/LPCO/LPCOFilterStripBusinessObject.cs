using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Module
{
	public class LPCOFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string LPCOHolder = CusLPCOHeaderCollection.FilterConstants.LPCOHolder;
			public const string LPCONumber = CusLPCOHeaderCollection.FilterConstants.LPCONumber;
			public const string LPCOMessageStatus = CusLPCOHeaderCollection.FilterConstants.LPCOMessageStatus;
			public const string LPCOJobNumber = CusLPCOHeaderCollection.FilterConstants.LPCOJobNumber;
			public const string ReferenceDate = CusLPCOHeaderCollection.FilterConstants.LPCORetroactiveDate;
			public const string LPCOCustomsStatus  = CusLPCOHeaderCollection.FilterConstants.LPCOCustomsStatus;
			public const string StartDate = CusLPCOHeaderCollection.FilterConstants.StartDate;
			public const string EndDate = CusLPCOHeaderCollection.FilterConstants.EndDate;
			public const string UnitOfMeasure = CusLPCOHeaderCollection.FilterConstants.UnitOfMeasure;
		}

		public LPCOFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new LPCOFilterLookups(this);
				}
				return lookups;
			}
		}
		LPCOFilterLookups lookups;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var permitHolderFilter = result.AddGuidFilter(Schema.LPCOHolder, ModuleIDs.Organisation, CusPermitHeaderSchema.CPH_OH_PermitHolder, new OrgHeaderCollection(Factory));
			permitHolderFilter.Category = FilterCategories.Organisations;
			permitHolderFilter.MultilingualDescription = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|LPCOHolder", Schema.LPCOHolder);

			var permitNumberFilter = result.AddTextFilter(Schema.LPCONumber, CusPermitHeaderSchema.CPH_Number);
			permitNumberFilter.Category = FilterCategories.NumbersAndReferences;
			permitNumberFilter.MaxLength = CusPermitHeaderSchema.CPH_Number.MaxLength;
			permitNumberFilter.MultilingualDescription = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|LPCONumber", Schema.LPCONumber);

			var startDateFilter = result.AddDateFilter(Schema.StartDate, GetStartDateQuery);
			startDateFilter.Category = FilterCategories.Dates;
			startDateFilter.MultilingualDescription = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|StartDate", Schema.StartDate);

			var endDateFilter = result.AddDateFilter(Schema.EndDate, GetEndDateQuery);
			endDateFilter.Category = FilterCategories.Dates;
			endDateFilter.MultilingualDescription = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|EndDate", Schema.EndDate);

			var jobNumber = result.AddTextFilter(Schema.LPCOJobNumber, CusPermitHeaderSchema.CPH_JobNumber);
			jobNumber.Category = FilterCategories.NumbersAndReferences;
			jobNumber.MultilingualDescription = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|LPCOJobNumber", Schema.LPCOJobNumber);

			var messageStatus = result.AddTextFilter(Schema.LPCOMessageStatus, CusPermitHeaderSchema.CPH_MessageStatus, Lookups.MessageStatusList);
			messageStatus.Category = FilterCategories.StatusAndFlags;
			messageStatus.MultilingualDescription = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|LPCOMessageStatus", Schema.LPCOMessageStatus);

			var customsStatus = result.AddTextFilter(Schema.LPCOCustomsStatus, CusPermitHeaderSchema.CPH_CustomsStatus);
			customsStatus.Category = FilterCategories.StatusAndFlags;
			customsStatus.MultilingualDescription = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|LPCOCustomsStatus", Schema.LPCOCustomsStatus);

			var referenceDateFilter = result.AddDateFilter(Schema.ReferenceDate, GetReferenceDateQuery);
			referenceDateFilter.Category = FilterCategories.Dates;
			referenceDateFilter.MultilingualDescription = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|ReferenceDate", Schema.ReferenceDate);

			var unitOfMeasureFilter = result.AddTextFilter(Schema.UnitOfMeasure, CusPermitHeaderSchema.CPH_UnitOfMeasure);
			unitOfMeasureFilter.Category = FilterCategories.Other;
			unitOfMeasureFilter.MultilingualDescription = ResString.GetMultilingualString("LPCOFilterStripBusinessObject|UnitOfMeasure", Schema.UnitOfMeasure);

			return result;
		}

		ZQuery GetStartDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();
			AddDateTimeRange(result, comparisonOperator, JoinCondition.And, CusPermitHeaderSchema.CPH_StartDate, value1, value2);
			return result;
		}

		ZQuery GetEndDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();
			AddDateTimeRange(result, comparisonOperator, JoinCondition.And, CusPermitHeaderSchema.CPH_EndDate, value1, value2);
			if (comparisonOperator == DateComparisonOperator.HasDateInRange && !value1.IsEmpty && value2.IsEmpty)
			{
				result.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
			}
			return result;
		}

		ZQuery GetReferenceDateQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2)
		{
			var result = new ZQuery();
			AddDateTimeOffsetRange(result, comparisonOperator, JoinCondition.And, CusPermitHeaderSchema.CPH_RetroactiveDate, value1, value2, false, false);
			if (comparisonOperator == DateComparisonOperator.HasDateInRange && !value1.IsEmpty && value2.IsEmpty)
			{
				result.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_RetroactiveDate, null);
			}
			return result;
		}
	}
}
