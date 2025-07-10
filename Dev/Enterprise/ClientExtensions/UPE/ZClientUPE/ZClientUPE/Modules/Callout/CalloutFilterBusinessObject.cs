using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public class CalloutFilterBusinessObject : UPEAirCargoCalloutBaseFilterBusinessObject
	{
		protected override SchemaStringColumn QueueNameColumn
		{
			get { return ProcessQueueSchema.P4_QueueName; }
		}

		protected override SchemaStringColumn QueueReasonColumn
		{
			get { return ProcessQueueSchema.P4_Status; }
		}

		protected override SchemaStringColumn QueueStatusColumn
		{
			get { return ProcessQueueSchema.P4_SubStatus; }
		}

		protected override SchemaStringColumn QueueRemarksColumn
		{
			get { return ProcessQueueSchema.P4_Reason; }
		}

		protected override SchemaStringColumn QueueTaskAssignedToColumn
		{
			get { return ProcessQueueSchema.P4_GS_NKTaskAssignedTo; }
		}

		protected override SchemaDecimalColumn ValueSchema
		{
			get { return ProcessQueueSchema.P4_CustomDecimal4; }
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				QueueFilterHelper.AddIsInProcessQueueToFilter(result);
				QueueFilterHelper.AddSubQueryToProcessQueueToFilter(result, JoinCondition.And, new ZQuery(QueueReasonColumn, (ZString)ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment), true);
				return result;
			}
		}

		#region Lookups

		public new CalloutFilterLookups Lookups
		{
			get { return (CalloutFilterLookups)base.Lookups; }
		}

		protected override UPEAirCargoCalloutFilterLookups NewLookups()
		{
			return new CalloutFilterLookups(this);
		}

		#endregion
	}
}
