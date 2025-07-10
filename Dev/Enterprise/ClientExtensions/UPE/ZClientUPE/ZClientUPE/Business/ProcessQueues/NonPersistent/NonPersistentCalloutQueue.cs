using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class NonPersistentCalloutQueue : NonPersistentProcessQueue
	{
		public NonPersistentCalloutQueue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override SchemaStringColumn QueueNameSchemaColumn
		{
			get { return ProcessQueueSchema.P4_QueueName; }
		}

		protected override SchemaStringColumn StatusSchemaColumn
		{
			get { return ProcessQueueSchema.P4_Status; }
		}

		protected override SchemaStringColumn SubStatusSchemaColumn
		{
			get { return ProcessQueueSchema.P4_SubStatus; }
		}

		protected override SchemaStringColumn ReasonSchemaColumn
		{
			get { return ProcessQueueSchema.P4_Reason; }
		}

		protected override SchemaStringColumn AssignedToSchemaColumn
		{
			get { return ProcessQueueSchema.P4_GS_NKTaskAssignedTo; }
		}

		protected override NonPersistentProcessQueueLookups GetNewNonPersistentProcessQueueLookups()
		{
			return new NonPersistentCalloutQueueLookups(this);
		}

		protected override UPEProcessQueueValidationHelper GetNewUPEProcessQueueValidationHelper()
		{
			return new UPECommercialQueueValidationHelper(this);
		}
	}
}
