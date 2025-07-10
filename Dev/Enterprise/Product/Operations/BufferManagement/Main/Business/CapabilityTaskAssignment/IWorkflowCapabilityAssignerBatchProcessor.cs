using System;
using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business
{
	public interface IWorkflowCapabilityAssignerBatchProcessor : IDisposable
	{
		IEnumerable<ProcessHeader> LoadNextBatch();

		void SaveBatchChanges();

		void SaveBatchChangesAndReclaimMemory();
	}
}
