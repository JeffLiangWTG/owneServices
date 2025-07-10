using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	class WorkflowCapabilityAssignerBatchProcessor : IWorkflowCapabilityAssignerBatchProcessor
	{
		public WorkflowCapabilityAssignerBatchProcessor(Func<ZQuery> workflowsQueryGetter, BatchLogger batchLogger)
		{
			this.workflowsQueryGetter = workflowsQueryGetter;
			this.batchLogger = batchLogger;
		}

		readonly Func<ZQuery> workflowsQueryGetter;
		readonly BatchLogger batchLogger;

		BusinessObjectFactoryProvider WorkflowFactoryProvider => workflowFactoryProvider
			?? (workflowFactoryProvider = new BusinessObjectFactoryProvider(new BusinessObjectFactory { NameForDebugging = "WorkflowCapabilityAssignerBatchProcessor.Workflows" }));
		BusinessObjectFactoryProvider workflowFactoryProvider;

		FilteredBusinessObjectReaderWithLogger WorkflowReader => workflowReader
			?? (workflowReader = new FilteredBusinessObjectReaderWithLogger(WorkflowFactoryProvider, workflowsQueryGetter(), typeof(ProcessHeader), batchLogger) { BatchSize = BMSRegistry.Instance.CapabilityAutoAssignmentBatchSize.Value });
		FilteredBusinessObjectReaderWithLogger workflowReader;

		BusinessObject lastBizoRead;

		#region IWorkflowCapabilityAssignerBatchProcessor Members

		public IEnumerable<ProcessHeader> LoadNextBatch()
		{
			var workflowBatch = WorkflowReader.LoadNextBatchInANewFactory(lastBizoRead).Cast<ProcessHeader>();
			lastBizoRead = workflowBatch.LastOrDefault();
			return workflowBatch;
		}

		public void SaveBatchChanges()
		{
			WorkflowFactoryProvider.SaveCurrentAndUpdateRecordCounts();
		}

		public void SaveBatchChangesAndReclaimMemory()
		{
			WorkflowFactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
		}

		#endregion

		#region Disposable

		bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					batchLogger.Flush();

					//We call garbage collector here for the reason - 'high memory usage' error happened on the service task run when 
					//memory consumption exceeds the value set in the registry.
					//it can happens on the large databases with the big amount of data.
					GCWrapper.ReclaimMemory(ref workflowFactoryProvider);
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			System.GC.SuppressFinalize(this);
		}

		#endregion
	}
}
