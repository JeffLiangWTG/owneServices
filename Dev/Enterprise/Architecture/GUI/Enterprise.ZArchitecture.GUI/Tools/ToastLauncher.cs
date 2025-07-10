using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.BrandManager;

namespace Enterprise.ZArchitecture.GUI
{
	class ToastLauncher : IToastLauncher
	{
		public void Launch(string title, string text)
		{
			Launch(title, text, TimeSpan.FromSeconds(5));
		}

		public void Launch(string title, string text, TimeSpan delay)
		{
			var secondsDelay = Convert.ToInt32(delay.TotalSeconds);
			if (countdown <= 0 && icon == null)
			{
				countdown = secondsDelay;
				Task.Factory.StartNew(async () =>
				{
					while (countdown > 0)
					{
						countdown--;
						await Task.Delay(TimeSpan.FromSeconds(1));
					}

					lock (iconState)
					{
						if (icon != null)
						{
							icon.Visible = false;
							icon.Dispose();
							icon = null;
						}
					}
				});
			}
			else
			{
				countdown = secondsDelay;
			}

			Icon.Visible = true;
			Icon.ShowBalloonTip(secondsDelay * 1000, title, text, ToolTipIcon.Info);
		}

		NotifyIcon Icon
		{
			get
			{
				lock (iconState)
				{
					if (icon == null)
					{
						icon = new NotifyIcon { Icon = BrandingFactory.Instance.ProductIcon };
					}
				}

				return icon;
			}
		}
		NotifyIcon icon;
		readonly Object iconState = new Object();
		int countdown;
	}
}
