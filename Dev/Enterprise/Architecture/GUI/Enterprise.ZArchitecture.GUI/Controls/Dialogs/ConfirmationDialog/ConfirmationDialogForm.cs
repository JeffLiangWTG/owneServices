using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public sealed partial class ConfirmationDialogForm : ZChildForm
	{
#if DEBUG
		public ConfirmationDialogForm()
		{
			InitializeComponent();
		}
#endif

		internal ConfirmationDialogForm(ConfirmationDialogDescriptor descriptor)
			: base(descriptor)
		{
			if (descriptor.ConfirmationNotifications.Count == 0)
			{
				Dispose();

				throw new ArgumentException("Dialog was created for no notifications");
			}

			this.descriptor = descriptor;

			InitializeComponent();
			Build();
		}

		readonly ConfirmationDialogDescriptor descriptor;

		#region ZForm Overrides

		public override string FormCaption
		{
			get { return Res.GetString("f8a1fa30-84d8-4963-9277-572eb5e56565", "Confirm action: {0}", descriptor.ActionName); }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		#region Build

		void Build()
		{
			NotificationsPanel.SuspendLayout();

			try
			{
				ControlDpiScalingHelper.SetWidth(this, 624, isOnStandardDpi: true);

				Control previousControl = null;

				foreach (var notification in descriptor.ConfirmationNotifications)
				{
					var control = new ConfirmationNotificationUserControl(notification);
					NotificationsPanel.Controls.Add(control);

					control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
					control.Margin = Margin = Padding.Empty;

					ControlDpiScalingHelper.SetWidth(control, NotificationsPanel.ClientSize.Width, isOnStandardDpi: false);

					if (previousControl != null)
					{
						ControlDpiScalingHelper.SetTop(control, previousControl.Top + previousControl.Height, isOnStandardDpi: false);
					}

					previousControl = control;

					ControlDpiScalingHelper.SetHeight(this, Height + control.Height, isOnStandardDpi: false);
				}
			}
			finally
			{
				NotificationsPanel.ResumeLayout(performLayout: true);
			}

			MinimumSize = Size;
			MaximumSize = ControlDpiScalingHelper.NewScaledSize(int.MaxValue, Size.Height, isInStandardDpi: false);
		}

		#endregion

		#region Event Handlers

		void OKDialogButton_Click(object sender, EventArgs e)
		{
			var nonIgnoredNotifications = descriptor.ConfirmationNotifications.Where(n => !n.IsIgnored).ToArray();

			if (nonIgnoredNotifications.Length > 0)
			{
				if (nonIgnoredNotifications.Any(n => n.NotificationType == NotificationTypes.Error))
				{
					Globals.Message.ShowError(Res.GetString("720b1af6-4ba6-49fd-a34d-a7c9bde64edc", "You must correct the errors before proceeding."));
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("543f97db-40fa-43f0-8c1e-a0ad2eb05757", "You must correct the notifications or choose to ignore them."));
				}

				DialogResult = DialogResult.None;
			}
			else
			{
				descriptor.Result = ZDialogResult.OK;
			}
		}

		void CancelDialogButton_Click(object sender, EventArgs e)
		{
			descriptor.Result = ZDialogResult.Cancel;
		}

		#endregion
	}
}
