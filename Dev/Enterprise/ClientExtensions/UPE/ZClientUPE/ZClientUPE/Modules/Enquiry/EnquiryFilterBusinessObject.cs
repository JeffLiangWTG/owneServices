using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public class EnquiryFilterBusinessObject : UPEAirCargoFilterBusinessObject
	{
		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				QueueFilterHelper.AddSubQueryToProcessQueueToFilter(result, JoinCondition.And, new ZQuery(QueueReasonColumn, SQLComparisonOperator.Equal, (ZString)ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment), true);
				return result;
			}
		}

		protected override SchemaDecimalColumn ValueSchema
		{
			get { return ProcessQueueSchema.P4_CustomDecimal4; }
		}
	}
}

