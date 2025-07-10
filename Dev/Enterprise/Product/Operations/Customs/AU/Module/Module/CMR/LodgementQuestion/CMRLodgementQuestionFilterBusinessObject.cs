using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class CMRLodgementQuestionFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filters.AddTextFilter("Question ID", GetQuestionIDQuery);
			filters.AddTextFilter("Question Text", CMRLodgementQuestionSchema.CQ_LodgementQuestionText);

			filters.AddDateFilter("Start Date", CMRLodgementQuestionSchema.CQ_LodgementQuestionStartDate);
			filters.AddDateFilter("End Date", CMRLodgementQuestionSchema.CQ_LodgementQuestionEndDate);

			return filters;
		}

		ZQuery GetQuestionIDQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery result = new ZQuery();

			ZInt intValue;
			if (ZInt.TryParse(value, out intValue))
			{
				result.AddToFilter(CMRLodgementQuestionSchema.CQ_LodgementQuestionIdentifier, intValue);
			}

			return result;
		}
	}
}
