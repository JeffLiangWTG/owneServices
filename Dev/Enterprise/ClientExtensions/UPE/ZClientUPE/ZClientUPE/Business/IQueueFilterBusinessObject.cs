using System;

using CargoWise.EntityFramework;

using CargoWise.Schema;

namespace Enterprise.Client.UPE.Business
{
	public interface IQueueFilterBusinessObject : IBusiness
	{
		Type QueueParentType
		{
			get;
		}
		DefaultQueueCodeDescriptionPairList QueueNames_List
		{
			get;
		}

		QueueCodeSet QueueStatus
		{
			get;
		}

		SchemaStringColumn QueueNameColumn
		{
			get;
		}
		SchemaStringColumn QueueReasonColumn
		{
			get;
		}
		SchemaStringColumn QueueStatusColumn
		{
			get;
		}
		SchemaStringColumn QueueRemarksColumn
		{
			get;
		}
		SchemaStringColumn QueueTaskAssignedToColumn
		{
			get;
		}
	}
}
