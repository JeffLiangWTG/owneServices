using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCADepotContainerList : BusinessObjectCollection<CusSCADepotContainer>
	{
		public CusSCADepotContainerList(BusinessObjectFactory factory, ZString messageType)
			: base(factory)
		{
			this.MessageType = messageType;
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
				result.AddToFilter(JoinCondition.And, CusSCADepotContainerSchema.CJ_Status, SQLComparisonOperator.Equal, MessageType);
			}
			return result;
		}
	}
}
