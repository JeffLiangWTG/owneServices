using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public class CMRCusUnderbondPlugin : CusUnderbondPlugin
	{
		#region Constructors

		public CMRCusUnderbondPlugin(IAUCusUnderbondUnionCollectionParent hostEntity, ZString bindPrepend, string pluginName)
			: base(hostEntity, bindPrepend, pluginName)
		{
		}

		public CMRCusUnderbondPlugin(IAUCusUnderbondUnionCollectionParent hostEntity, ZString bindPrepend) : this(hostEntity, bindPrepend, "Customs Underbond Movement")
		{
		}

		public CMRCusUnderbondPlugin(IAUCusUnderbondUnionCollectionParent hostEntity, ZString bindPrepend, string pluginName, Type typeOfCurrent)
			: base(hostEntity, bindPrepend, pluginName, typeOfCurrent)
		{
		}

		#endregion

		protected override Business.CusUnderbondUnionCollectionParentCollection GetNewCurrentDependentCusUnderbondUnionCollectionParentCollection(BusinessObjectFactory factory, Type typeOfCurrent)
		{
			return new CusUnderbondUnionCollectionParentCollection(factory, typeOfCurrent);
		}

		protected override Business.CusUnderbondUnionCollectionParentCollection GetNewCusUnderbondUnionCollectionParentCollection(BusinessObjectFactory factory)
		{
			return new CusUnderbondUnionCollectionParentCollection(factory);
		}

		protected override Control GetNewUserControl()
		{
			fCusUnderbondUserControl = (CusUnderbondUserControl)base.GetNewUserControl();
			fCusUnderbondUserControl.UnderbondsGrid.ContextMenu.MenuItems.Add(new ZMenuItem("Create Underbond Contingency Data", ExportUnderbondToCSV));

			sendUniversalXMLMenuItem = new ZMenuItem("Send Universal XML");
			sendUniversalXMLMenuItem.MenuItems.Add(new ZMenuItem("Universal Shipment", ExportUnderbondUniversalShipment));

			fCusUnderbondUserControl.UnderbondsGrid.ContextMenu.MenuItems.Add(sendUniversalXMLMenuItem);
			fCusUnderbondUserControl.UnderbondsGrid.ContextMenu.Popup += ContextMenu_Popup;
			return fCusUnderbondUserControl;
		}

		protected override CusUnderbondUserControl CreateNewUserControl()
		{
			return new AUCusUnderbondUserControl();
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			sendUniversalXMLMenuItem.Visible = SelectedUnderBond?.IsAirCargo ?? false;
		}

		ZMenuItem sendUniversalXMLMenuItem;
		CusUnderbondUserControl fCusUnderbondUserControl;

		ZForm MainForm => HasUserControl ? (ZForm)UserControl.ParentForm : null;

		CusUnderbond SelectedUnderBond => fCusUnderbondUserControl.UnderbondsGrid.ListManager.GetCurrent() as CusUnderbond;

		void ExportUnderbondUniversalShipment(object sender, EventArgs args)
		{
			if (SelectedUnderBond != null)
			{
				if (SelectedUnderBond.HasChanges)
				{
					Globals.Message.ShowWarning(Declaration.GUI.Res.GetString("2495EC8F-2A5A-4280-9B7C-439A2875DDCE", "Please save your changes before sending XML Universal Data."),
						Declaration.GUI.Res.GetString("FDCA0EF9-3383-4D8F-A7B5-5C163FE5E4D3", "Send Universal XML"));
				}
				else
				{
					var businessObjectFactory = new BusinessObjectFactory { NameForDebugging = "AU Customs Export" }; // This code smells like copy pasta.
					using (businessObjectFactory.AddDisposableService())
					using (var exportForm = new ManualDataExportForm(businessObjectFactory, SelectedUnderBond, UniversalDataType.UniversalShipment))
					{
						ZFormModaliser.ShowDialogWithoutDispose(exportForm);
					}
				}
			}
		}

		void ExportUnderbondToCSV(object sender, EventArgs args)
		{
			if (fCusUnderbondUserControl != null && fCusUnderbondUserControl.UnderbondsGrid.CurrentRowIndex >= 0)
			{
				if (SelectedUnderBond != null)
				{
					using (var exportForm = new CMRExportForm(MainForm, new CMRUnderbondExporter(SelectedUnderBond)))
					{
						exportForm.Export();
					}
				}
			}
			else
			{
				Globals.Message.ShowWarning("Please select a row before attempting to create Contingency Data.");
			}
		}
	}
}
