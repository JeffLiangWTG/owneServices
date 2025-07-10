using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Customs.AU.Declaration.GUI.Res;

namespace Enterprise.Customs.AU.Sailing.GUI
{
	public class ArrivalPluginMenu : CMRMessageManagementMenu
	{
		public ArrivalPluginMenu(Business.MultiMessageManager manager) : base(manager)
		{
		}

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			MenuItem impendingArrival = new ZMenuItem(Res.GetString("6db37d90-7706-48aa-a705-458745e122b2", "Create Impending Arrival Contingency Data"), new EventHandler(ExportImpendingArrivalToCSV));
			MenuItem actualArrival = new ZMenuItem(Res.GetString("ebc59cbc-7197-4a7e-80a9-8f686517ce4b", "Create Actual Arrival Contingency Data"), new EventHandler(ExportActualArrivalToCSV));
			MenuItems.Add(new ZMenuItem("-"));
			MenuItems.Add(impendingArrival);
			MenuItems.Add(actualArrival);
		}

		void ExportImpendingArrivalToCSV(object sender, EventArgs args)
		{
			if (manager != null)
			{
				new CMRExportForm(MainForm, new CMRAirArrivalExporter(manager.TopLevelBusinessObject, "IA")).Export();
			}
		}

		void ExportActualArrivalToCSV(object sender, EventArgs args)
		{
			if (manager != null)
			{
				new CMRExportForm(MainForm, new CMRAirArrivalExporter(manager.TopLevelBusinessObject, "AA")).Export();
			}
		}
	}
}
