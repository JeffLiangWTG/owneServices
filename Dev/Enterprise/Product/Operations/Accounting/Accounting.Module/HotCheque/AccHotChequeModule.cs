using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.GUI.ARAP.HotCheque;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class AccHotChequeModule : ZFilterGridModule
	{
		public AccHotChequeModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccHotCheque; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccHotCheque);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccHotChequeFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccHotChequeCollection(Factory);
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("f746a336-2d4b-4436-9411-e91ae3ef1764", "Cancel", "Cancels the selected item after viewing its details read-only (shortcut Del)");
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("968350c7-da21-461b-8721-19d3949b28f7", "Print"), new EventHandler(HandlePrint)));
			return menuItems.ToArray();
		}

		protected void HandlePrint(object sender, EventArgs e)
		{
			if (Env.Security.PrintHotCheque.IsAllowed)
			{
				BusinessObject[] chequesToPrint = this.Grid.SelectedElements;
				if (chequesToPrint.Length > 0)
				{
					AccHotCheque cheque = chequesToPrint[0] as AccHotCheque;
					if (cheque.AQ_Cancelled)
					{
						Globals.Message.ShowError(Res.GetString("8f4f067f-ec29-41a2-b891-bbb6ba64c863", "This cheque is canceled and cannot be printed."));
					}
					else
					{
						HotChequePrintManager printManager = new HotChequePrintManager(cheque, Factory);
						printManager.Print();
					}
				}
			}
			else
			{
				Env.Security.PrintHotCheque.ShowError();
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccHotChequeFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.HotCheques; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}
	}
}
