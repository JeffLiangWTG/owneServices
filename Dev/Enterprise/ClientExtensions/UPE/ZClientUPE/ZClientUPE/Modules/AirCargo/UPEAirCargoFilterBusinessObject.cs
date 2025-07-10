using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public class UPEAirCargoFilterBusinessObject : UPEAirCargoCalloutBaseFilterBusinessObject
	{
		protected override SchemaStringColumn QueueNameColumn
		{
			get { return ProcessQueueSchema.P4_CustomsQueue; }
		}

		protected override SchemaStringColumn QueueReasonColumn
		{
			get { return ProcessQueueSchema.P4_CustomsStatus; }
		}

		protected override SchemaStringColumn QueueStatusColumn
		{
			get { return ProcessQueueSchema.P4_CustomsSubStatus; }
		}

		protected override SchemaStringColumn QueueRemarksColumn
		{
			get { return ProcessQueueSchema.P4_CustomsReason; }
		}

		protected override SchemaStringColumn QueueTaskAssignedToColumn
		{
			get { return ProcessQueueSchema.P4_GS_NKCustomsTaskAssignedTo; }
		}

		protected override SchemaDecimalColumn ValueSchema
		{
			get { return CusHAWBSchema.CS_GoodsValue; }
		}

		#region Lookups

		public new UPEAirCargoFilterLookups Lookups
		{
			get { return (UPEAirCargoFilterLookups)base.Lookups; }
		}

		protected override UPEAirCargoCalloutFilterLookups NewLookups()
		{
			return new UPEAirCargoFilterLookups(this);
		}

		#endregion
	}
}
