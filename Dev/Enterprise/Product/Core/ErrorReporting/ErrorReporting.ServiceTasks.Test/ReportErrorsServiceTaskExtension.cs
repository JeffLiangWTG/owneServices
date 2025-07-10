using System;
using CargoWise.Common;

namespace Enterprise.ErrorReporting.ServiceTasks.Test
{
	internal static class ReportErrorsServiceTaskExtension
	{
		internal static IDisposable TemporarySetDeleteBatchSize_ForTest(this ReportErrorsServiceTask reportErrorsServiceTask, int newValue)
		{
			var valueBeforeChange = reportErrorsServiceTask.deleteBatchSize;
			reportErrorsServiceTask.deleteBatchSize = newValue;

			return new DisposableAction(() => reportErrorsServiceTask.deleteBatchSize = valueBeforeChange);
		}
	}
}
