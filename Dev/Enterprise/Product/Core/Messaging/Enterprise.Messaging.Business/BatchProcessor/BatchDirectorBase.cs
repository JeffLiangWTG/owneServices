using System.Threading;
using CargoWise.Types;
using Enterprise.Integration.BatchProcessor;

namespace Enterprise.BatchProcessor
{
	public abstract class BatchDirectorBase : IBatchDirector
	{
		#region IBatchDirector Members

		/// <summary>
		/// Execute main loop of the Process every time the BatchProcessor timer ticks
		/// </summary>
		public abstract void DoMainProcessingLoop(CancellationToken token);

		public virtual bool ValidateEnvironment()
		{
			return true;
		}

		public virtual string BatchProcessorName
		{
			get { return fBatchProcessorName; }
			set { fBatchProcessorName = value; }
		}
		ZString fBatchProcessorName;

		public virtual void SetInformationLogger(ILoggingInformation logger)
		{
			this.Logger = (LoggingInformation)logger;
		}

		public virtual void InitialiseBatchProcesses()
		{
		}

		public virtual int TimerIntervalInMilliseconds
		{
			get { return 8000; }
		}

		protected virtual void SetBatchProcessorName(BatchProcess currentProcess)
		{
			BatchProcessorName = currentProcess.HumanReadableName;
		}

		protected LoggingInformation Logger;

		#endregion
	}
}
