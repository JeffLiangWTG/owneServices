#if DEBUG
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Client.YAS.Business.ProofOfDeliveryInterface;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.YAS.Module
{
	class YASShipmentModuleOverride : JobShipmentModule
	{
		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("Import YAS POD Data <Debug Menu Only>", new EventHandler(BypassPODImportServiceTaskForDebugOnly)));
			return result.ToArray();
		}

		#region POD Import

		void BypassPODImportServiceTaskForDebugOnly(object sender, EventArgs args)
		{
			NotificationBuffer notifications = new NotificationBuffer();
			ImportProcessor.ExecuteForTest(notifications);
			if (notifications.Events.Length > 0)
			{
				Globals.Message.Show(notifications.AsString, "POD Import Exception Messages", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		PODImportProcessor ImportProcessor
		{
			get { return importProcessor ?? (importProcessor = new PODImportProcessor()); }
		}
		PODImportProcessor importProcessor;

		#endregion
	}
}
#endif
