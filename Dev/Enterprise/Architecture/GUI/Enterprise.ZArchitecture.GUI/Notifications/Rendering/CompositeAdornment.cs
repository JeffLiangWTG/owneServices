using System.Collections.Generic;
using System.Windows.Forms;

using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	public class CompositeAdornment : NotificationAdornment
	{
		readonly List<NotificationAdornment> adornments = new List<NotificationAdornment>();

		public void Add(NotificationAdornment adornment)
		{
			adornments.Add(adornment);
		}

		public NotificationAdornment this[int index]
		{
			get { return adornments[index];  }
		}

		public override void Attach()
		{
			base.Attach();

			foreach (var adornment in adornments)
			{
				adornment.Attach();
			}
		}

		public override void Detach()
		{
			base.Detach();

			foreach (var adornment in adornments)
			{
				adornment.Detach();
			}
		}

		public override IEnumerable<INotification> Notifications
		{
			set
			{
				base.Notifications = value;

				foreach (var adornment in adornments)
				{
					adornment.Notifications = value;
				}
			}
		}

		protected internal override void SetControl(Control control)
		{
			base.SetControl(control);

			foreach (var adornment in adornments)
			{
				if (adornment.Control == null)
				{
					adornment.SetControl(control);
				}
			}
		}

		protected internal override void SetPresenter(INotificationPresenter presenter)
		{
			base.SetPresenter(presenter);

			foreach (var adornment in adornments)
			{
				if (adornment.Presenter == null)
				{
					adornment.SetPresenter(presenter);
				}
			}
		}
	}
}
