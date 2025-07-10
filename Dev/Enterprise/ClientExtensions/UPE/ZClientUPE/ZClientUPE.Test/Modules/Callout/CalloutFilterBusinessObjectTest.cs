using System;
using CargoWise.Schema;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(CalloutFilterBusinessObject))]
	public class CalloutFilterBusinessObjectTest : UPEAirCargoCalloutBaseFilterBusinessObjectTest
	{
		protected override SchemaStringColumn QueueNameColumn
		{
			get
			{
				return ProcessQueueSchema.P4_QueueName;
			}
		}

		protected override SchemaStringColumn QueueReasonColumn
		{
			get
			{
				return ProcessQueueSchema.P4_Status;
			}
		}

		protected override SchemaStringColumn QueueStatusColumn
		{
			get
			{
				return ProcessQueueSchema.P4_SubStatus;
			}
		}

		protected override SchemaStringColumn QueueRemarksColumn
		{
			get
			{
				return ProcessQueueSchema.P4_Reason;
			}
		}

		protected override SchemaStringColumn QueueTaskAssignedToColumn
		{
			get
			{
				return ProcessQueueSchema.P4_GS_NKTaskAssignedTo;
			}
		}

		public void TestLookups()
		{
			CalloutFilterBusinessObject filterBizObj = (CalloutFilterBusinessObject)GetNewBusinessObject();
			AssertEquals("Lookups of correct type", typeof(CalloutFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestFilterDoesNotIncludeSubsequentSplitShipments()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			Callout callout = (Callout)mAWB.ChildBills.AddNew(typeof(Callout));
			callout.CurrentQueue[QueueReasonColumn.Name] = ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment;
			Factory.Save();
			AssertFilterMatches("Should not find any matches", Array.Empty<CusHAWB>());
		}

		#region Only records in a queue match
		public void TestOnlyRecordsWithQueueNamePopulatedMatches()
		{
			CusMAWB mAWB = Factory.New<CusMAWB>();
			CusHAWB hAWB = mAWB.ChildBills.AddNew(typeof(QueuedCusHAWB));
			hAWB.CurrentQueue[QueueNameColumn.Name] = "";
			Factory.Save();
			AssertFilterMatches("Shouldnt match when no queue name specified", Array.Empty<UPECusHAWB>());
			hAWB.CurrentQueue[QueueNameColumn.Name] = "XXX";
			Factory.Save();
			AssertFilterMatches("Should match when queue name is specified", hAWB);
		}

		#endregion
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CalloutFilterBusinessObject();
		}

		protected override SchemaDecimalColumn ValueColumn
		{
			get
			{
				return Callout.TotalAmountDueColumn;
			}
		}
	}
}
