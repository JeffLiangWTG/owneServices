using System;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public interface INotificationHandler
	{
		void ReportInformation(string message, string caption);
		void ReportError(string message, string caption, string errorContext = null, Exception exception = null);
	}

	public interface INotificationHandlerWithMessageOverride : INotificationHandler
	{
		string MergeWarningMessage { get; }
		string CriticalWarningMessage { get; }
		string CannotDeleteMessage { get; }
		string DeletedObjectsHeader { get; }
		string MergedObjectsHeader { get; }
		string CriticalObjectsHeader { get; }
		string CannotDeleteObjectsHeader { get; }
	}

	public static class NotificationHandler
	{
		public static INotificationHandler Instance
		{
			get { return instance ?? (instance = new DefaultNotificationHandler()); }
			set { instance = value; }
		}

		[ThreadStatic]
		static INotificationHandler instance;

		public static IDisposable SetHandler(INotificationHandler handler)
		{
			var oldHandler = instance;
			instance = handler;

			return new DisposableAction(() => instance = oldHandler);
		}
	}

	sealed class DefaultNotificationHandler : INotificationHandler
	{
		#region INotificationHandler Members

		public void ReportInformation(string message, string caption)
		{
			ReportError(caption, message);
		}

		public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
		{
			ErrorReporter.ReportOnce(string.Format("{0}: {1}", caption, message), exception);
		}

		#endregion
	}

	sealed class ErrorsNotificationHandler : INotificationHandler
	{
		public ErrorsNotificationHandler(bool reportErrorsOnly)
		{
			this.reportErrorsOnly = reportErrorsOnly;
		}

		readonly bool reportErrorsOnly;

		#region INotificationHandler Members

		public void ReportInformation(string message, string caption)
		{
			if (!reportErrorsOnly && !(NotificationHandler.Instance is ErrorsNotificationHandler))
			{
				NotificationHandler.Instance.ReportInformation(message, caption);
			}
		}

		public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
		{
			if (!(NotificationHandler.Instance is ErrorsNotificationHandler))
			{
				NotificationHandler.Instance.ReportError(message, caption, null, exception);
			}
			else
			{
				ErrorReporter.ReportOnce(caption, message);
			}
			LastErrorMessage = message;
			HasErrorsReported = true;
		}

		#endregion

		public bool HasErrorsReported { get; private set; }
		public string LastErrorMessage { get; private set; }
	}
}
