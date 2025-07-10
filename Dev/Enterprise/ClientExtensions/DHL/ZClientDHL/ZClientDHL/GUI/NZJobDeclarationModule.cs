using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Client.DHL.Business;
using Enterprise.Customs.NZ.Module.Declaration.FormalEntry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.DHL.GUI
{
	public class NZJobDeclarationModule : JobDeclarationModule
	{
		public NZJobDeclarationModule()
		{
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItemCollection = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItemCollection.Add(new ZMenuItem("-"));
			MenuItem flightBulkUpdate = new ZMenuItem(FlightBulkUpdateMenuItem, new EventHandler(FlightBulkUpdate_Click));
			menuItemCollection.Add(flightBulkUpdate);
			return menuItemCollection.ToArray();
		}

		void FlightBulkUpdate_Click(object sender, EventArgs e)
		{
			ZFormModaliser.ShowDialogAndDispose(new FlightBulkUpdateForm(new FlightBulkUpdateBusinessObject()));
		}

		internal const string FlightBulkUpdateMenuItem = "Flight Bulk Update";
	}
}
