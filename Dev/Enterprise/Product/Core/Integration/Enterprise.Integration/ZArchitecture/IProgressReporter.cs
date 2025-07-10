using System;
using System.ComponentModel;
using CargoWise.Common;

namespace Enterprise.Integration
{
	public interface IProgressReporter : IProgress<int>, IDisposable
	{
		int ItemsProcessed { get; }
		bool IsCancelled { get; }

		void ShowForm(IComponent parentFormToShowModallyTo, string initialMessage);
	}

	public static class IProgressReporterExtensions
	{
		public static void ReportOneItemProcessed(this IProgressReporter reporter)
		{
			Argument.NotNull(reporter, nameof(reporter));

			reporter.Report(reporter.ItemsProcessed + 1);
		}
	}
}
