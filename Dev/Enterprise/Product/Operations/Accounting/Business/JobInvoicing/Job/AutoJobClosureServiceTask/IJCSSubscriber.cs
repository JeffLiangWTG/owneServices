namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IJCSSubscriber
	{
		string Code { get; }

		bool CanProcess(Job job);

		bool Process(Job job, bool changesWillBeSavedToDB);

		bool CanChainToNextSubscriber();
	}

	public abstract class JCSSubscriber : IJCSSubscriber
	{
		protected JCSSubscriber(IJCSLogger logger)
		{
			Logger = logger;
		}

		string IJCSSubscriber.Code => Code;

		bool IJCSSubscriber.CanProcess(Job job) => CanProcess(job);

		bool IJCSSubscriber.Process(Job job, bool changesWillBeSavedToDB) => Process(job, changesWillBeSavedToDB);

		bool IJCSSubscriber.CanChainToNextSubscriber() => CanChainToNextSubscriber();

		protected abstract string Code { get; }

		protected virtual bool CanProcess(Job job) =>
			job != null && job.JobType != null && JobClosureConfigurationLookups.GetAllJobTypesThatCanBeProcessedByJCS().ContainsCode(job.JobType.Code);

		protected abstract bool Process(Job job, bool changesWillBeSavedToDB);

		protected virtual bool CanChainToNextSubscriber() => true;

		protected IJCSLogger Logger { get; }
	}
}
