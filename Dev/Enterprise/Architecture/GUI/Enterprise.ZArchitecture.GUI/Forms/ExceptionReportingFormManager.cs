using System;
using System.Windows.Forms;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

#if WINZOR
using WinzorFramework;
#endif

namespace Enterprise.ZArchitecture.GUI
{
	class ExceptionReportingFormManager : IExceptionReportingFormManager
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception report message should not be translated")]
		public bool ShowReportForm(Exception ex, string errorReportId, string key, string message, bool isFullMode, Action<ExceptionReportArgs> sendErrorReport)
		{
#if WINZOR

			if (!WinzorDispatcher.IsCurrent)
			{
				sendErrorReport(new ExceptionReportArgs(ex, errorReportId, key, "Silently reported") { IsSlient = true });
				return false;
			}

#endif

			var result = false;
			using (var exceptionReportForm = new ExceptionReportingForm(errorReportId, isFullMode))
			{
#if DEBUG
				// this is done for the test only, so the test can run non-interactively
				if (Globals.GetIsUnitTestingProductionFunctionality())
				{
					exceptionReportForm.Shown += delegate
					{ exceptionReportForm.Close(); };
				}
#endif

				exceptionReportForm.SetException(ex, key, message);

				if (isFullMode)
				{
					exceptionReportForm.ErrorReportSend += new ExceptionReportingForm.ErrorReportSendHandler(sendErrorReport);
				}

				result = exceptionReportForm.ShowDialog() == DialogResult.OK;
				if (!result && !Globals.IsDebugMode)
				{
					sendErrorReport(new ExceptionReportArgs(ex, errorReportId, key, "None supplied by client"));
				}

				if (exceptionReportForm.IsShutDownRequested)
				{
					ProgramRestarter.Instance.ShutdownEnterpriseWithMessage(Res.GetString("9fc7ad0a-f03d-4cb0-908b-399bd7a6f485", "{0} will now close.", Constants.ProductName));
				}
			}

			return result;
		}
	}
}
