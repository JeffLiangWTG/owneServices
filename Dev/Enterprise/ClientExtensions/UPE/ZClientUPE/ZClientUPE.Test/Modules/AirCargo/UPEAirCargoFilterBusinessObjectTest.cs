using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEAirCargoFilterBusinessObject))]
	public class UPEAirCargoFilterBusinessObjectTest : UPEAirCargoCalloutBaseFilterBusinessObjectTest
	{
		protected override SchemaStringColumn QueueNameColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsQueue;
			}
		}

		protected override SchemaStringColumn QueueReasonColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsStatus;
			}
		}

		protected override SchemaStringColumn QueueStatusColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsSubStatus;
			}
		}

		protected override SchemaStringColumn QueueRemarksColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsReason;
			}
		}

		protected override SchemaStringColumn QueueTaskAssignedToColumn
		{
			get
			{
				return ProcessQueueSchema.P4_GS_NKCustomsTaskAssignedTo;
			}
		}

		protected override SchemaDecimalColumn ValueColumn
		{
			get
			{
				return CusHAWBSchema.CS_GoodsValue;
			}
		}

		public void TestLookups()
		{
			UPEAirCargoFilterBusinessObject filterBizObj = (UPEAirCargoFilterBusinessObject)GetNewBusinessObject();
			AssertEquals("Lookups of correct type", typeof(UPEAirCargoFilterLookups), filterBizObj.Lookups.GetType());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UPEAirCargoFilterBusinessObject();
		}
	}
}
