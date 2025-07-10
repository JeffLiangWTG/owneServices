using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic
{
	internal class JCSJobProviderForDiagnostics : IJCSJobProvider
	{
		internal JCSJobProviderForDiagnostics(IEnumerable<ZGuid> jobPKs)
		{
			if (jobPKs == null || !jobPKs.Any())
			{
				throw new InvalidOperationException(Res.GetString("f558e51a-5e7e-45e4-9b01-788dbd9865e3", "No job is selected for diagnosis."));
			}
			jobQueue = new Queue<ZGuid>(jobPKs);
			jobCount = jobQueue.Count;
		}
		readonly Queue<ZGuid> jobQueue;

		readonly int jobCount;

		PickedJob? IJCSJobProvider.LoadJobPKFromQueue() => new PickedJob(rowNumber: jobCount - jobQueue.Count, jobQueue.Dequeue());

		bool IJCSJobProvider.CanContinue(int numberOfJobsDiagnosed) => numberOfJobsDiagnosed < jobCount;

		bool IJCSJobProvider.HasCounterReachedEndOfQueue() => jobQueue.Count == 0;

		void IJCSJobProvider.UpdateRowNumberInRegistry()
		{
			//does nothing
		}

		void IJCSJobProvider.Reset()
		{
			//dose nothing
		}

		bool IJCSJobProvider.CanTriggerQueuePopulation => false;
	}
}
