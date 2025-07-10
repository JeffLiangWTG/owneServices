using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	/// <summary>
	/// Summary description for CMRDepotUnderbondUserControl.
	/// </summary>
	public partial class CMRDepotUnderbondUserControl : ZUserControl
	{
		public CMRDepotUnderbondUserControl()
		{
			InitializeComponent();
			MenuItem exportItem = new ZMenuItem("Create Underbond Contingency Data", new EventHandler(ExportUnderbondToCSV));
			cusUnderbondUserControl1.UnderbondsGrid.ContextMenu.MenuItems.Add(exportItem);
			cusUnderbondUserControl1.SetBindPrepend(ZString.Empty);
			cusUnderbondUserControl1.RemoveColumnsForSea();
		}

		#region Export Contingency Data

		ZForm MainForm
		{
			get { return (ZForm)ParentForm; }
		}

		void ExportUnderbondToCSV(object sender, EventArgs args)
		{
			if (cusUnderbondUserControl1.UnderbondsGrid.CurrentRowIndex >= 0)
			{
				CusUnderbond selectedUnderBond = cusUnderbondUserControl1.UnderbondsGrid.ListManager.GetCurrent() as CusUnderbond;
				if (selectedUnderBond != null)
				{
					new AU.GUI.CMRExportForm(MainForm, new CMRUnderbondExporter(selectedUnderBond)).Export();
				}
			}
			else
			{
				ZArchitecture.Environment.Globals.Message.ShowWarning("Please select a row before attempting to create Contingency Data.");
			}
		}

		#endregion
	}
}
