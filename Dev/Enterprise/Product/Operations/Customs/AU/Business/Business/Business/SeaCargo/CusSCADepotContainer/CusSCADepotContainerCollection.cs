using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCADepotContainerCollection : DependentBusinessObjectCollection<CusSCADepotContainer, CommonContainer>
	{
		public CusSCADepotContainerCollection(CommonContainer container, BusinessObjectFactory factory)
			: base(container, factory)
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
				result.AddToFilter(JoinCondition.And, CusSCADepotContainerSchema.CJ_Status, SQLComparisonOperator.Equal, MessageType);
			}
			return result;
		}
	}
}
