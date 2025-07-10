using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class DecCusTempStorageLineCollectionFrom<T, MasterT> : CusTempStorageLineCollection<T, MasterT>
		where T : CusTempStorageLine
		where MasterT : CusTempStorageDec
	{
		public DecCusTempStorageLineCollectionFrom(MasterT parentStorageDec) : base(parentStorageDec)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			var subQuery = GetRelationshipFilterBasedOnParent();
			result.AddToFilter(subQuery);
			return result;
		}

		protected virtual ZDBOnlyQuery GetRelationshipFilterBasedOnParent()
		{
			var subQuery = new ZDBOnlyQuery(typeof(T));
			var divotSubQuery = new ZDBOnlySubQuery(typeof(CusTempStorageLinePivot), CusTempStorageLinePivotSchema.SLR_TSL_ToLine, true);
			subQuery.AddSubQuery(divotSubQuery, JoinCondition.And);
			return subQuery;
		}
	}
}
