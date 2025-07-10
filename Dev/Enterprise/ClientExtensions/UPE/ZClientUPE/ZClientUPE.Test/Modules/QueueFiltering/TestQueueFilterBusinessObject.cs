using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	class TestQueueFilterBusinessObject : NonPersistentBusinessObject, IQueueFilterBusinessObject
	{
		public TestQueueFilterBusinessObject()
			: base(new BusinessObjectFactory())
		{
		}

		Type IQueueFilterBusinessObject.QueueParentType
		{
			get { return typeof(UPECusHAWB); }
		}

		public class TestQueueCodeDescriptionPairList : CargoReportQueueCodeDescriptionPairList
		{
		}

		public DefaultQueueCodeDescriptionPairList QueueNames_List
		{
			get { return new TestQueueCodeDescriptionPairList(); }
		}

		public QueueCodeSet QueueStatus
		{
			get { return fQueueStatus; }
		}
		readonly QueueCodeSet fQueueStatus = new QueueCodeSet();

		#region IsUnworked / IsUnworkedToday

		public ZBool IsUnworked
		{
			get { return fIsUnworked; }
			set { fIsUnworked = value; }
		}
		ZBool fIsUnworked;

		public ZPropertyInfo IsUnworkedInfo
		{
			get { return GetZPropertyInfo(nameof(IsUnworked)); }
		}

		public ZBool IsUnworkedToday
		{
			get { return fIsUnworkedToday; }
			set { fIsUnworkedToday = value; }
		}
		ZBool fIsUnworkedToday;

		public ZPropertyInfo IsUnworkedTodayInfo
		{
			get { return GetZPropertyInfo(nameof(IsUnworkedToday)); }
		}

		#endregion

		#region Schema Columns

		public SchemaStringColumn QueueNameColumn
		{
			get { return ProcessQueueSchema.P4_QueueName; }
		}

		public SchemaStringColumn QueueReasonColumn
		{
			get { return ProcessQueueSchema.P4_Status; }
		}

		public SchemaStringColumn QueueStatusColumn
		{
			get { return ProcessQueueSchema.P4_SubStatus; }
		}

		public SchemaStringColumn QueueRemarksColumn
		{
			get { return ProcessQueueSchema.P4_Reason; }
		}

		public SchemaStringColumn QueueTaskAssignedToColumn
		{
			get { return ProcessQueueSchema.P4_GS_NKTaskAssignedTo; }
		}

		#endregion
	}
}
