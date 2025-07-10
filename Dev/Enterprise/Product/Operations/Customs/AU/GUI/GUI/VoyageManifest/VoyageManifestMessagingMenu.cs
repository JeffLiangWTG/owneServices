using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class VoyageManifestMessagingMenu : CMRMessageManagementMenu
	{
		public VoyageManifestMessagingMenu(Customs.Business.MultiMessageManager manager) : base(manager)
		{
		}

		CusSeaManTranHead TranHead
		{
			get { return (CusSeaManTranHead)manager.TopLevelBusinessObject; }
		}

		#region Menu

		protected override void InitializeMenu()
		{
			base.InitializeMenu();

			MenuItems.Add("-");
			MenuItem contingencyCRMenuItem = new ZMenuItem("Create Cargo Report Contingency Data", new EventHandler(ContingencyCRMenuItem_Click));
			MenuItem contingencyIAMenuItem = new ZMenuItem("Create Impending Arrival Contingency Data", new EventHandler(ContingencyIAMenuItem_Click));
			MenuItem contingencyAAMenuItem = new ZMenuItem("Create Actual Arrival Contingency Data", new EventHandler(ContingencyAAMenuItem_Click));
			MenuItems.Add(contingencyCRMenuItem);
			MenuItems.Add(contingencyIAMenuItem);
			MenuItems.Add(contingencyAAMenuItem);
		}

		void ContingencyCRMenuItem_Click(object sender, EventArgs args)
		{
			new CMRExportForm(MainForm, new CusSeaManTranHeadExporter(TranHead, "CR")).Export();
		}

		void ContingencyIAMenuItem_Click(object sender, EventArgs args)
		{
			new CMRExportForm(MainForm, new CusSeaManTranHeadExporter(TranHead, "IA")).Export();
		}

		void ContingencyAAMenuItem_Click(object sender, EventArgs args)
		{
			new CMRExportForm(MainForm, new CusSeaManTranHeadExporter(TranHead, "AA")).Export();
		}

		#endregion
	}
}
