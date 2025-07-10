using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Interop;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class DashboardForm : ZChildForm
	{
		const int millisecondsInMinute = 60000;

		public DashboardForm()
		{
		}

		public DashboardForm(Dashboard businessObject)
			: base(businessObject)
		{
			zLabel10.AllowOverlap(ZLabel53);
			zLabel9.AllowOverlap(ZLabel53);
			zLabel8.AllowOverlap(ZLabel53);
		}

		public Dashboard Dashboard
		{
			get { return (Dashboard)BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "Dashboard"; }
		}

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Dashboard.Refresh();
			RefreshTimer.Interval = UPEDataRegistry.Instance.DashboardRefreshCycleTimeInMinutes.Value * millisecondsInMinute;
			RefreshTimer.Enabled = true;
		}

		protected virtual Timer NewTimer()
		{
			return new Timer(this.components);
		}

		void RefreshTimer_Tick(object sender, EventArgs e)
		{
#if !WINZOR
			if (Globals.IsTest || Handle == UnsafeNativeMethods.GetForegroundWindow())
			{
				ObjectFactory.Get<IKeepSessionAlive>().KeepAlive();
			}
#endif

			Dashboard.Refresh();
		}

		protected override void OnClosed(EventArgs e)
		{
			RefreshTimer.Enabled = false;
			base.OnClosed(e);
		}

#endregion

		#region Dispose

		IContainer components;
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				RefreshTimer.Dispose();
			}
		}

		#endregion
	}
}
