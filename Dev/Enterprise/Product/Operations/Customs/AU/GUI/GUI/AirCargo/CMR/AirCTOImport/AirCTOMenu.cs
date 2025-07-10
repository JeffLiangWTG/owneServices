using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirCTOMenu : CMRMessageManagementMenu
	{
		public AirCTOMenu(CTOCusMAWBMessageManager manager) : base(manager)
		{
		}

		CTOCusMAWBMessageManager Manager
		{
			get { return (CTOCusMAWBMessageManager)manager; }
		}

		CTOCusMAWB MAWB
		{
			get { return Manager.MAWB; }
		}

		#region InitializeMenu

		protected override void InitializeMenu()
		{
			base.InitializeMenu();

			MenuItems.Add("-");
			MenuItem contingencyMenuItem = new ZMenuItem("Create Contingency Data", new EventHandler(ContingencyMenuItem_Click));
			MenuItems.Add(contingencyMenuItem);
		}

		void ContingencyMenuItem_Click(object sender, EventArgs args)
		{
			new CMRExportForm(MainForm, new AirCTOMAWBExporter(MAWB)).Export();
		}

		#endregion
	}
}
