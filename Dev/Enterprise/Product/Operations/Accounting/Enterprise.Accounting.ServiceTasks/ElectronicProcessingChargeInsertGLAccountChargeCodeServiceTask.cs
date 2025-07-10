using System;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.ServiceTasks;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask.Code,
	"Electronic Processing Charge Insert GLAccount Charge Code",
	"ACC",
	typeof(ElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

namespace Enterprise.Accounting.ServiceTasks
{
	public class ElectronicProcessingChargeInsertGLAccountChargeCodeServiceTask : ServiceProviderImpl
	{
		public const string Code = "EPC";

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Debug($"Electronic Processing Charge Insert GLAccount Charge Code service task started.");
			try
			{
				if (!token.IsCancellationRequested)
				{
					ObjectFactory.Get<IElectronicProcessingChargeProvider>().InsertAndSetElectronicProcessingChargeRegistry();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var exceptionErrorMessage = string.Format(CultureInfo.InvariantCulture, $"Electronic Processing Charge Insert GLAccount Charge Code service task ended abruptly.\r\nException: {ex.GetType()}\r\nException Message: {ex.Message} StackTrace: {ex.StackTrace}.");

				ServiceLogger.Error(exceptionErrorMessage);

				ErrorReporter.ReportOnce("F4C1CCF3-B360-41C2-825B-E5781DAD4B61", exceptionErrorMessage);
			}
			ServiceLogger.Information($"Electronic Processing Charge Insert GLAccount Charge Code service task completed.");
		}
	}
}
