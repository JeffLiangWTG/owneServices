using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Controls.Extensions
{
	#region Interface

	public interface IStatusbarExtension : IControlExtension
	{
		void Display();
		void Clear();
	}

	#endregion

	public class StatusbarExtension : ControlExtension, IStatusbarExtension
	{
		#region Fields

		readonly Statusbar statusbar;

		#endregion

		#region Constructors

		public StatusbarExtension()
			: this(new Statusbar())
		{ }

		/// <summary>
		/// For testing purposes
		/// </summary>
		internal StatusbarExtension(Statusbar statusbar)
		{
			this.statusbar = statusbar;
		}

		#endregion

		#region Mounting

		public override void Initialize(IExtendedControl owner)
		{
			base.Initialize(owner);

			HookEvents(Owner.Host);

			statusbar.Initialize(Owner.Host);
		}

		public override void Dispose()
		{
			if (Owner != null)
			{
				UnhookEvents(Owner.Host);
			}

			base.Dispose();
		}

		#region Control Event Handlers

		protected void HookEvents(Control control)
		{
			control.Enter += OnEnter;
			control.Validated += OnValidated;
		}

		protected void UnhookEvents(Control control)
		{
			control.Enter -= OnEnter;
			control.Validated -= OnValidated;
		}

		void OnEnter(object sender, EventArgs args)
		{
			Display();
		}

		void OnValidated(object sender, EventArgs args)
		{
			Clear();
		}

		#endregion

		#endregion

		#region Implementation

		public void Display()
		{
			if (!statusbar.CanUpdate())
			{
				return;
			}

			var notification = GetNotificationExtension();
			var hint = GetHintExtension();

			var highestSeverityNotification = notification == null ? null : notification.Notifications.GetHighestSeverityNotification();
			if (highestSeverityNotification != null)
			{
				statusbar.Update(highestSeverityNotification.Message, highestSeverityNotification.Type);
			}
			else if (hint != null)
			{
				var desc = hint.Description;

				if (!string.IsNullOrWhiteSpace(desc))
				{
					statusbar.Update(desc, null);
				}
				else
				{
					statusbar.Update(hint.Caption, null);
				}
			}
		}

		public void Clear()
		{
			if (statusbar.CanUpdate())
			{
				statusbar.Update("", null);
			}
		}

		#endregion

		#region Support

		protected virtual IHintExtension GetHintExtension()
		{
			return Owner.Extensions.Get<IHintExtension>();
		}

		protected virtual INotificationExtension GetNotificationExtension()
		{
			return Owner.Extensions.Get<NotificationExtension>();
		}

		#endregion
	}
}
