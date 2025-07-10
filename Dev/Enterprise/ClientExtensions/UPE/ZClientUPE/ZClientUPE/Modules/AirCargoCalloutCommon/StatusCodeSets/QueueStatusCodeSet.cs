
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class QueueStatusCodeSet : CodeSet
	{
		public QueueStatusCodeSet(string reasonCode)
		{
			this.ReasonCode = reasonCode;
		}

		public readonly string ReasonCode;

		public ZQuery GetMultipleCodeFilter(SchemaStringColumn queueReasonColumn, SchemaStringColumn queueStatusColumn)
		{
			ZQuery result = base.GetMultipleCodeFilter(queueStatusColumn);
			result.AddToFilter(JoinCondition.And, queueReasonColumn, SQLComparisonOperator.Equal, ReasonCode);
			return result;
		}
	}
}
