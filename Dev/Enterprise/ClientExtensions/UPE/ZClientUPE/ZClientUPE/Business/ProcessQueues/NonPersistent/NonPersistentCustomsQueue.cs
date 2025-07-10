
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public abstract class NonPersistentCustomsQueue : NonPersistentProcessQueue
	{
		public NonPersistentCustomsQueue(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override SchemaStringColumn QueueNameSchemaColumn
		{
			get { return ProcessQueueSchema.P4_CustomsQueue; }
		}

		protected override SchemaStringColumn StatusSchemaColumn
		{
			get { return ProcessQueueSchema.P4_CustomsStatus; }
		}

		protected override SchemaStringColumn SubStatusSchemaColumn
		{
			get { return ProcessQueueSchema.P4_CustomsSubStatus; }
		}

		protected override SchemaStringColumn ReasonSchemaColumn
		{
			get { return ProcessQueueSchema.P4_CustomsReason; }
		}

		protected override SchemaStringColumn AssignedToSchemaColumn
		{
			get { return ProcessQueueSchema.P4_GS_NKCustomsTaskAssignedTo; }
		}
	}
}
