using System;
using System.Threading;
using CargoWise.ComponentModel;
using Enterprise.Client.SWT.GUI;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.SWT
{
	class SWTOrderModuleOverride : OrdersModule, INotifications
	{
		public SWTOrderModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddExportDataMenuItem(": Printing Out 'Not Yet Arrived' Reports", new EventHandler(OnAutoPrinting));
		}

		void OnAutoPrinting(object sender, EventArgs e)
		{
			BatchReportPrintingRunner reportRunner = new BatchReportPrintingRunner();
			reportRunner.RunBatchReports(this, CancellationToken.None);
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Globals.Message.ShowInformation(notification.Message);
		}

		#endregion
	}
}
