using System;
using System.Collections.Generic;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.PrintProcessing.Test.Processors;

public class PrintJobManagerForTesting : PrintJobManager
{
	public Exception ExceptionToThrowBeforeSwitchingBlobType { get; set; }

	public Exception ExceptionToThrowWhenProcessingPrintJobs { get; set; }

	protected override void RunBeforeSwitchingBlobTypeForTesting()
	{
		if (ExceptionToThrowBeforeSwitchingBlobType != null)
		{
			throw ExceptionToThrowBeforeSwitchingBlobType;
		}
	}

	internal override void ProcessPrintJobsCore(IEnumerable<StmPrintJob> jobs)
	{
		if (ExceptionToThrowWhenProcessingPrintJobs != null)
		{
			throw ExceptionToThrowWhenProcessingPrintJobs;
		}

		base.ProcessPrintJobsCore(jobs);
	}
}
