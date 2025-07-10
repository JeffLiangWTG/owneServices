#if !WINZOR
using System;
using CargoWise.BrandManager;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZNotifyIconExTest : TestCase
	{
		public void TestNoMemoryLeak()
		{
			var notifyIconRef = GetWeakReferenceToZNotifyIconEx();
			GC.Collect();
			AssertEquals("ZNotifyIconEx reference should be collected", false, notifyIconRef.IsAlive);
		}

		WeakReference GetWeakReferenceToZNotifyIconEx()
		{
			var notifyIcon = new ZNotifyIconEx();
			notifyIcon.Click += new EventHandler(OnNotifyIcon_Click);
			notifyIcon.Text = "Text";
			notifyIcon.Icon = BrandingFactory.Instance.ProductIcon;
			notifyIcon.Visible = true;
			notifyIcon.ShowBalloon("MEH", "I have a Mullet", ZNotifyIconEx.NotifyInfoFlags.Error, 1000);

			var notifyIconRef = new WeakReference(notifyIcon);
			notifyIcon.Dispose();
			notifyIcon = null;

			return notifyIconRef;
		}

		void OnNotifyIcon_Click(object sender, EventArgs e)
		{
		}

		void OnNotifyIcon_BalloonClick(object sender, EventArgs e)
		{
		}
	}
}
#endif
