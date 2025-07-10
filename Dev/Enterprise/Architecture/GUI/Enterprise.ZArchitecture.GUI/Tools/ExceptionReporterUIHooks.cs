using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

#if !WINZOR
using System.Windows.Threading;
#else
using System;
#endif

namespace Enterprise.ZArchitecture.GUI
{
	public class ExceptionReporterUIHooks : IExceptionReporterUIHooks
	{
		[SuppressMessage("CargoWiseOne", "CW1067:DoNotApplicationThreadException", Justification = "This is the single place where this is set")]
		public void HookThreadSpecificUnhandledExceptions()
		{
			if (!threadHooked.Value)
			{
				threadHooked.Value = true;
				Application.ThreadException += Application_ThreadException; // This is the single place where this is set
#if !WINZOR
				if (System.Environment.UserInteractive)
				{
					Dispatcher.CurrentDispatcher.UnhandledException += Dispatcher_UnhandledException;
				}
#endif
			}

#if WINZOR
			// added once per application, will not be unhooked
			if (!developerExceptionHooked)
			{
				Application.DeveloperException += Application_DeveloperException;
				Application.UnhandledException += Application_UnhandledException;
				developerExceptionHooked = true;
			}
#endif
		}

		[SuppressMessage("CargoWiseOne", "CW1067:DoNotApplicationThreadException", Justification = "This is the single place where this is set")]
		public void UnHookUnhandledExceptions()
		{
			if (threadHooked.Value)
			{
				Application.ThreadException -= Application_ThreadException; // This is the single place where this is set
#if !WINZOR
				if (System.Environment.UserInteractive)
				{
					Dispatcher.CurrentDispatcher.UnhandledException -= Dispatcher_UnhandledException;
				}
#endif
			}
		}

		void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			using (ApplicationDispatcher.RecordThreadException(e.Exception))
			{
				ExceptionReporter.Instance.HandleUnhandledException(e.Exception);
			}
		}

#if WINZOR

		void Application_DeveloperException(object sender, string message, Exception exception)
		{
			ErrorReporter.ReportOnce(message, exception);
		}

		void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Application_ThreadException(sender, new ThreadExceptionEventArgs((Exception)e.ExceptionObject));
		}

		[SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule")]
		static bool developerExceptionHooked;

#endif

#if !WINZOR

		void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			ExceptionReporter.Instance.HandleUnhandledException(e.Exception);
		}

#endif

		readonly ThreadLocal<bool> threadHooked = new ThreadLocal<bool>();
	}
}

