using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCADepotHouseList : BusinessObjectCollection<CusSCADepotHouse>
	{
		public CusSCADepotHouseList(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString MessageType
		{
			get { return fMessageType; }
			set { fMessageType = value; }
		}
		ZString fMessageType;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			if (!MessageType.IsEmpty)
			{
				result.AddToFilter(JoinCondition.And, CusSCADepotHouseSchema.CX_Status, SQLComparisonOperator.Equal, MessageType);
			}
			return result;
		}
	}
}
