using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public class BackgroundAdornment : NotificationAdornment
	{
		Color oldColor = Color.Empty;

		public override IEnumerable<INotification> Notifications
		{
			get { return base.Notifications; }
			set
			{
				base.Notifications = value;

				if (ShouldStoreDefaultColor())
				{
					StoreDefaultColor();
				}

				if (ShouldRestoreDefaultColor())
				{
					RestoreDefaultColor();
				}

				if (ShouldSetColor())
				{
					SetColor();
				}
			}
		}

		bool ShouldStoreDefaultColor()
		{
			return oldColor == Color.Empty && State != null && !Control.GetReadOnly();
		}

		void StoreDefaultColor()
		{
			oldColor = Control.BackColor;
		}

		bool ShouldRestoreDefaultColor()
		{
			return State == null && oldColor != Color.Empty;
		}

		void RestoreDefaultColor()
		{
			var colorMutator = Control as IBackColorMutable;

			if (colorMutator != null)
			{
				colorMutator.ColorChanger.ClearNotificationBackColor();
			}
			else
			{
				Control.BackColor = oldColor;
			}

			oldColor = Color.Empty;
		}

		bool ShouldSetColor()
		{
			return State != null && !Control.GetReadOnly();
		}

		void SetColor()
		{
			var color = NotificationColorScheme.GetColor(State);
			var colorMutator = Control as IBackColorMutable;

			if (colorMutator != null)
			{
				colorMutator.ColorChanger.SetNotificationBackColor(color);
			}
			else
			{
				Control.BackColor = color;
			}
		}
	}
}
