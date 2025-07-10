using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLHeaderCollection : Customs.Business.CusSeaManOBLHeaderCollection
	{
		public CusSeaManOBLHeaderCollection(CusSeaManTranHead parent)
			: base(parent)
		{
		}

		public new CusSeaManOBLHeader AddNew()
		{
			return (CusSeaManOBLHeader)base.AddNew();
		}

		public new CusSeaManOBLHeader this[int index]
		{
			get { return (CusSeaManOBLHeader)base[index]; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery filter = base.CreateRelationshipFilter();
			filter.AddToFilter(JoinCondition.And, CusSeaManOBLHeaderSchema.BO_BA, SQLComparisonOperator.Equal, null);
			return filter;
		}
	}
}
