using System;
using System.Collections;

namespace Enterprise.ZArchitecture.Environment
{
	/// <summary>
	/// Base class for user notifications.
	/// </summary>
	public abstract class UserNotificationBase
	{
		public void ShowErrorOnce(string message, string caption)
		{
			var key = caption + message;

			if (!ShownErrorKeysList.Contains(key))
			{
				ShowError(message, caption);
				ShownErrorKeysList.Add(key);
			}
		}

		/// <summary>
		/// Shows the Error every time
		/// </summary>
		public void ShowDeveloperErrorAlways(string key, string message, string caption)
		{
			ShowDeveloperException(key, caption, new DeveloperNotificationException(message));
		}

		public void ShowDeveloperErrorAlways(string message, string caption)
		{
			ShowDeveloperErrorAlways("", message, caption);
		}

		/// <summary>
		/// Shows the Error once, per session.
		/// </summary>
		/// <param name="key">Identifier to prevent the Developer Error being shown more than once.</param>
		public void ShowDeveloperErrorOnce(string key, string message, string caption)
		{
			if (!ShownErrorKeysList.Contains(key))
			{
				ShowDeveloperException(key, caption, new DeveloperNotificationException(message));
				ShownErrorKeysList.Add(key);
			}
		}

		public void ShowDeveloperExceptionOnce(string key, string message, Exception ex)
		{
			if (!ShownErrorKeysList.Contains(key))
			{
				ShowDeveloperException(key, message, ex);
				ShownErrorKeysList.Add(key);
			}
		}

		public void ShowDeveloperException(string message, Exception e)
		{
			ShowDeveloperException("", message, e);
		}

		public abstract void ShowError(string message, string caption);

		public abstract void ShowDeveloperException(string key, string message, Exception e);

		readonly ArrayList ShownErrorKeysList = new ArrayList();

		public string[] ShownErrorKeys
		{
			get { return (string[])ShownErrorKeysList.ToArray(typeof(string)); }
		}

		public void ClearShownErrorKeys()
		{
			ShownErrorKeysList.Clear();
		}
	}
}
