using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal abstract class NonPersistentCustomsQueueTestCase : NonPersistentProcessQueueTestCase
	{
		protected override SchemaStringColumn ExpectedQueueNameSchemaColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsQueue;
			}
		}

		protected override SchemaStringColumn ExpectedStatusSchemaColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsStatus;
			}
		}

		protected override SchemaStringColumn ExpectedSubStatusSchemaColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsSubStatus;
			}
		}

		protected override SchemaStringColumn ExpectedReasonSchemaColumn
		{
			get
			{
				return ProcessQueueSchema.P4_CustomsReason;
			}
		}

		protected override SchemaStringColumn ExpectedAssignedToSchemaColumn
		{
			get
			{
				return ProcessQueueSchema.P4_GS_NKCustomsTaskAssignedTo;
			}
		}
	}
}
