using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class CusBRForeignOperatorCollection : ActiveBusinessObjectCollection<CusBRForeignOperator>
	{
		public CusBRForeignOperatorCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CusBRForeignOperatorCollection(BusinessObjectFactory factory, ZGuid ownerPK) : base(factory, GetQueryForeignOperator(ownerPK))
		{
		}

		static ZQuery GetQueryForeignOperator(ZGuid ownerPK)
		{
			var query = new ZQuery(CusBRForeignOperatorSchema.BFR_OH_Owner, ownerPK);
			query.AddToFilter(CusBRForeignOperatorSchema.BFR_OH_ForeignOperator, SQLComparisonOperator.NotEqual, null);
			return query;
		}

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider => new CusBRForeignOperatorFindBoxListProvider(this);

		class CusBRForeignOperatorFindBoxListProvider : FindBoxListProvider
		{
			public CusBRForeignOperatorFindBoxListProvider(CusBRForeignOperatorCollection collection)
				: base(collection)
			{
			}

			protected override void AddCodeEqualsFilter(ZQuery query, string code) => query.AddToFilter(GetCusBRForeignOperatorFromOrgHeaderCode(code));

			ZQuery GetCusBRForeignOperatorFromOrgHeaderCode(string code)
			{
				var query = new ZDBOnlyQuery(typeof(CusBRForeignOperator));

				var orgHeaderForeignOperator = new ZDBOnlySubQuery(typeof(OrgHeader), CusBRForeignOperatorSchema.BFR_OH_ForeignOperator);
				orgHeaderForeignOperator.AddToFilter(OrgHeaderSchema.OH_Code, code);

				query.AddSubQuery(orgHeaderForeignOperator, JoinCondition.And);
				return query;
			}
		}
		#endregion
	}
}

