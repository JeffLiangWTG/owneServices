using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Implemented on a component that renders notifications (such as errors and warnings)
	/// to controls.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
	public interface INotificationRenderer
	{
		/// <summary>
		/// Set the notifications for a particular control.
		/// </summary>
		void SetNotifications(Control control, NotificationCollection notification);
	}

	/// <summary>
	/// Renders notifications (such as errors and warnings) to controls using
	/// System.Windows.Forms.ErrorProvider.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renderer")]
	public class NotificationRenderer : Component, INotificationRenderer, IDisposable
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#")]
		public virtual void SetNotifications(Control control, NotificationCollection notifications)
		{
			INotificationType type = notifications.GetHighestSeverityNotificationType();
			ErrorProvider existingProvider = null;
			if (type != null)
			{
				existingProvider = GetErrorProvider(type);
				existingProvider.SetError(control, notifications.ToMessageListString());
			}

			foreach (ErrorProvider next in errorProviders.Values)
			{
				if (next != existingProvider)
				{
					// NOTE: this is expensive as it invalidates the control and it's parent
					next.SetError(control, "");
				}
			}
		}

		#region GetActiveErrorProviderForControl for testing

		protected ErrorProvider GetActiveErrorProviderForControl(Control control)
		{
			ErrorProvider result = null;
			foreach (ErrorProvider provider in errorProviders.Values)
			{
				string error = provider.GetError(control);
				if (!string.IsNullOrEmpty(error))
				{
					result = provider;
				}
			}
			return result;
		}

		#endregion

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			foreach (ErrorProvider errorProvider in errorProviders.Values)
			{
				errorProvider.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		readonly Dictionary<INotificationType, ErrorProvider> errorProviders = new Dictionary<INotificationType, ErrorProvider>();

		ErrorProvider GetErrorProvider(INotificationType notificationType)
		{
			ErrorProvider result = null;
			errorProviders.TryGetValue(notificationType, out result);
			if (result == null)
			{
				result = new ErrorProvider();
				result.BlinkStyle = ErrorBlinkStyle.NeverBlink;
				result.Icon = NotificationIconScheme.Instance.GetIcon(notificationType);
				errorProviders[notificationType] = result;
			}
			return result;
		}

		#endregion
	}
}
