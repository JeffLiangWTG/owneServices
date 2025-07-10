using Enterprise.DocumentEngine.ReportErrorManagement;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class UnattendedPrintTaskUIProviderTest : TestCase
	{
		public void TestShowRuntimeOptionsUI()
		{
			IPrintTaskUIProvider provider = new UnattendedPrintTaskUIProvider();
			AssertEquals("ShowRuntimeOptionsUI should return false.", false, provider.ShowRuntimeOptionsUI(null, AllowedDeliveryOptions.All, null, null));
		}

		public void TestShowPrintTaskDeliveryUI()
		{
			IPrintTaskUIProvider provider = new UnattendedPrintTaskUIProvider();
			AssertEquals("ShowPrintTaskDeliveryUI should return true.", true, provider.ShowPrintTaskDeliveryUI(null));
		}

		public void TestGetNewProgressNotificationUI()
		{
			IPrintTaskUIProvider provider = new UnattendedPrintTaskUIProvider();
			using (IProgressNotificationUI progressNotificationUI = provider.GetNewProgressNotificationUI(null))
			{
				AssertEquals("SilentProgressNotificationUI should return a SilentProgressNotificationUI.", typeof(SilentProgressNotificationUI), progressNotificationUI.GetType());
			}

			using (IProgressNotificationUI progressNotificationUI = provider.GetNewProgressNotificationUI(null, 0))
			{
				AssertEquals("SilentProgressNotificationUI should return a SilentProgressNotificationUI.", typeof(SilentProgressNotificationUI), progressNotificationUI.GetType());
			}
		}

		public void TestShowDocDeliveryUI()
		{
			IPrintTaskUIProvider provider = new UnattendedPrintTaskUIProvider();
			AssertEquals("ShowDocDeliveryUI should return true.", true, provider.ShowDocDeliveryUI(null, null, null));
		}

		public void TestShowErrors()
		{
			IPrintTaskUIProvider provider = new UnattendedPrintTaskUIProvider();
			Report report = new Report(new DocumentPack(), null);

			AssertEquals("provider.ShowErrors(report) returns true when error manager doesn't have any errors.", true, provider.ShowErrors(report));

			var mockError = new Mock<IReportProcessingError>();

			mockError.Setup(m => m.Occurrences).Returns(1);
			mockError.Setup(m => m.Severity).Returns(ReportProcessingErrorSeverity.Error);

			report.ErrorManager.Add(mockError.Object);
			AssertEquals("provider.ShowErrors(report) returns false when error manager doesn't have warnings only.", false, provider.ShowErrors(report));

			mockError.Reset();
			mockError.Setup(m => m.Severity).Returns(ReportProcessingErrorSeverity.Warning);
			AssertEquals("provider.ShowErrors(report) returns true when error manager has warnings only.", true, provider.ShowErrors(report));
		}
	}
}
