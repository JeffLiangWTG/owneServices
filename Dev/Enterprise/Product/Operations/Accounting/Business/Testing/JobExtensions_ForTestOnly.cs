#if DEBUG

using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class JobExtensions_ForTestOnly
	{
		public static Job TryLoadOrCreateWithoutMutexForTestOnly(this Job.Loader loader)
		{
			var job = loader.TryLoadOrCreate();
			job.CreateMockJobSavingWithoutMutexErrorReporter();

			return job;
		}

		public static Job TryLoadOrCreateWithoutMutexForTestOnly(this Job.Loader loader, GlbBranch branch)
		{
			var job = loader.TryLoadOrCreate(branch);
			job.CreateMockJobSavingWithoutMutexErrorReporter();

			return job;
		}

		public static Job TryCreateWithoutMutexForTestOnly(this Job.Loader loader)
		{
			var job = loader.TryCreate();
			job.CreateMockJobSavingWithoutMutexErrorReporter();

			return job;
		}

		public static Job TryCreateWithoutMutexForTestOnly(this Job.Loader loader, GlbBranch branch)
		{
			var job = loader.TryCreate(branch);
			job.CreateMockJobSavingWithoutMutexErrorReporter();

			return job;
		}
	}
}
#endif
