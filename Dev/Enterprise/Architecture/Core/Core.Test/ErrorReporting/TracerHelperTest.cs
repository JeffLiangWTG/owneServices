using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class TracerHelperTest : TestCase
	{
		public void TestRemoveReportElements()
		{
			string inputWithHandleThreadException =
				"   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.DoReportException(Exception ex, String key, String message)" +
				"\r\n   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.ReportException(String key, Exception ex)" +
				"\r\n   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.HandleThreadException(Object sender, ThreadExceptionEventArgs e)" +
				"\r\n   at System.Windows.Forms.Application.ThreadContext.OnThreadException(Exception t)" +
				"\r\n   at System.Windows.Forms.Control.WndProcException(Exception e)";

			string outputWithHandleThreadException =
				"   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.HandleThreadException(Object sender, ThreadExceptionEventArgs e)" +
				"\r\n   at System.Windows.Forms.Application.ThreadContext.OnThreadException(Exception t)" +
				"\r\n   at System.Windows.Forms.Control.WndProcException(Exception e)";

			AssertEquals("HandleThreadException is left in the trace", outputWithHandleThreadException,
				TracerHelper.RemoveReportElements(inputWithHandleThreadException));

			string inputWithReport =
				"   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.ReportSilently(String key, String message, Exception ex)" +
				"\r\n   at Enterprise.ZArchitecture.Environment.UserNotificationBase.ShowDeveloperErrorAlways(String Key, String Message, String Caption)" +
				"\r\n   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.CargoWise.Common.IErrorReporter.Report(String key, String message, Exception exception)" +
				"\r\n   at Enterprise.eHubMessaging.ServiceTasks.eHubServiceManager.Execute(IeHubMessagingCompanySettingsManager companySettingsManager, INotifications notify)" +
				"\r\n   at Enterprise.eHubMessaging.ServiceTasks.eHubServiceTask.RunTask()";

			string outputWithReport =
				"   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.CargoWise.Common.IErrorReporter.Report(String key, String message, Exception exception)" +
				"\r\n   at Enterprise.eHubMessaging.ServiceTasks.eHubServiceManager.Execute(IeHubMessagingCompanySettingsManager companySettingsManager, INotifications notify)" +
				"\r\n   at Enterprise.eHubMessaging.ServiceTasks.eHubServiceTask.RunTask()";

			AssertEquals("IErrorReporter.Report is left in the trace", outputWithReport, TracerHelper.RemoveReportElements(inputWithReport));

			string inputWithReportOnce =
				"   at Enterprise.ZArchitecture.Environment.UnattendedUserNotification.ShowDeveloperException(String Key, String Message, Exception E)" +
				"\r\n   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.CargoWise.Common.IErrorReporter.Report(String key, String message, Exception exception)" +
				"\r\n   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)" +
				"\r\n   at Enterprise.eHubMessaging.Business.EHubNotifier.SendEmailNotification()" +
				"\r\n   at Enterprise.eHubMessaging.ServiceTasks.eHubServiceTask.RunTask()";

			string outputWithReportOnce =
				"   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)" +
				"\r\n   at Enterprise.eHubMessaging.Business.EHubNotifier.SendEmailNotification()" +
				"\r\n   at Enterprise.eHubMessaging.ServiceTasks.eHubServiceTask.RunTask()";

			AssertEquals("ErrorReporter.ReportOnce is left in the trace. IErrorReporter.Report is stripped", outputWithReportOnce,
				TracerHelper.RemoveReportElements(inputWithReportOnce));

			string inputWithShowDeveloperException =
				"   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.SendAllUnsentDeveloperExceptions()" +
				"\r\n   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.ReportSilently(String Key, String Message, Exception E)" +
				"\r\n   at Enterprise.ZArchitecture.Environment.UnattendedUserNotification.ShowDeveloperException(String Key, String Message, Exception E)" +
				"\r\n   at Enterprise.ZArchitecture.Web.GUI.FilterStrips.ZFilterStripControl.GetFilterStripDataSource(ZGuid pk)" +
				"\r\n   at Enterprise.ZArchitecture.Web.GUI.FilterStrips.ZFilterStripControl.AddFilterStripsFromViewState()";

			string outputWithShowDeveloperException =
				"   at Enterprise.ZArchitecture.Environment.UnattendedUserNotification.ShowDeveloperException(String Key, String Message, Exception E)" +
				"\r\n   at Enterprise.ZArchitecture.Web.GUI.FilterStrips.ZFilterStripControl.GetFilterStripDataSource(ZGuid pk)" +
				"\r\n   at Enterprise.ZArchitecture.Web.GUI.FilterStrips.ZFilterStripControl.AddFilterStripsFromViewState()";

			AssertEquals("ShowDeveloperException is left in the trace", outputWithShowDeveloperException,
				TracerHelper.RemoveReportElements(inputWithShowDeveloperException));
		}

		public void TestRemoveReportElementsWithNotConsecutiveReportOnce()
		{
			string inputWithMultipleReportOnce =
				"   at Enterprise.Core.Environment.UserNotification.ShowDeveloperException(string Key, string Message, Exception E)" +
				"\r\n   at Enterprise.ZArchitecture.Environment.UserNotificationBase.ShowDeveloperErrorAlways(string key, string message, string caption)" +
				"\r\n   at Enterprise.ZArchitecture.Core.BaseExceptionReporter.CargoWise.Common.IErrorReporter.Report(String key, String message, Exception exception)" +
				"\r\n   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)" +
				"\r\n   at System.Windows.Forms.Control.WndProc(Message & m)" +
				"\r\n   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)" +
				"\r\n   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)" +
				"\r\n   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)" +
				"\r\n   at CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()";

			string outputWithMultipleReportOnce =
				"   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)" +
				"\r\n   at System.Windows.Forms.Control.WndProc(Message & m)" +
				"\r\n   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)" +
				"\r\n   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)" +
				"\r\n   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)" +
				"\r\n   at CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()";

			AssertEquals("Both ErrorReporter.ReportOnce are left in the trace. IErrorReporter.Report is stripped", outputWithMultipleReportOnce,
				TracerHelper.RemoveReportElements(inputWithMultipleReportOnce));
		}
	}
}
