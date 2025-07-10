using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCADepotHouseCollection : DependentBusinessObjectCollection<CusSCADepotHouse, BusinessObject>
	{
		#region Constructors

		public CusSCADepotHouseCollection(CusSCADepotContainer container, BusinessObjectFactory factory)
			: base(container, factory)
		{
		}

		public CusSCADepotHouseCollection(CommonShipment shipment, BusinessObjectFactory factory)
			: base(shipment, factory)
		{
		}

		#endregion
		#region MessageType

		public ZString MessageType
		{
			get { return fMessageType; }
			set { fMessageType = value; }
		}
		ZString fMessageType;

		#endregion

		#region Find

		public CusSCADepotHouse Find(ZString houseBillNumber)
		{
			CusSCADepotHouse result = null;
			foreach (CusSCADepotHouse house in this)
			{
				if (house.CX_HouseBill == houseBillNumber)
				{
					result = house;
					break;
				}
			}
			return result;
		}

		#endregion

		#region Relationship Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			if (!MessageType.IsEmpty)
			{
				result.AddToFilter(JoinCondition.And, CusSCADepotHouseSchema.CX_Status, SQLComparisonOperator.Equal, MessageType);
			}
			return result;
		}

		#endregion
	}
}
