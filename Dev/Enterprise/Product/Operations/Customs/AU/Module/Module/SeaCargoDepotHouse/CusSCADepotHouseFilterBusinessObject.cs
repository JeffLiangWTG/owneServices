using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module.SeaCargo
{
	public class CusSCADepotHouseFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddTextFilter("Container Number", GetContainerNumberQuery).MaxLength = CusSCADepotContainerSchema.CJ_ContainerNumber.MaxLength;
			filters.AddTextFilter("House Bill", CusSCADepotHouseSchema.CX_HouseBill);

			return filters;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				result.AddToFilter(CusSCADepotHouseSchema.CX_JS, ZGuid.Empty);
				return result;
			}
		}

		ZQuery GetContainerNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CusSCADepotHouse));
			ZDBOnlySubQuery containerQuery = new ZDBOnlySubQuery(typeof(CusSCADepotContainer), CusSCADepotHouseSchema.CX_CJ);
			containerQuery.AddToFilter(CusSCADepotContainerSchema.CJ_ContainerNumber, @operator, value);
			result.AddSubQuery(containerQuery, JoinCondition.And);
			return result;
		}
	}
}
