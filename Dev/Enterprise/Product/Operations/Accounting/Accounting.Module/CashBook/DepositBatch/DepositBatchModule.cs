using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class DepositBatchModule : FilterGridModuleWithMultipleReversing, IDocumentBusinessContext
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DepositBatch; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DepositBatch; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("c74a2125-52b9-4241-9d92-18df93cf422c", "Cancel", "Cancels the selected item after viewing its details read-only (shortcut Del)");
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			menuItems.Remove(EditMenuItem);
			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("36691BEE-7E68-4C52-AF47-982659CBE375", "&Print"), new EventHandler(HandlePrint)));
			return menuItems.ToArray();
		}

		protected override bool CanBeCopied()
		{
			return false;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new DepositBatchController();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DepositBatchModuleCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DepositBatchFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DepositBatchFilterBusinessObject();
		}

		void HandlePrint(object sender, EventArgs e)
		{
			if (Env.Security.PrintDepositBatch.IsAllowed)
			{
				DepositBatch batchToPrint = CurrentBusinessObjectInGrid as DepositBatch;

				if (batchToPrint != null)
				{
					DepositSlipPrintHelper print = new DepositSlipPrintHelper();
					print.PrintDepositSlip(batchToPrint.AH_ReceiptBatchNo);
				}
			}
			else
			{
				Globals.Message.Show(Env.Security.PrintDepositBatch.ErrorMessageForNotAllowed, Res.GetString("57e457d6-2b14-440c-aa8d-ea5798dfb957", "Access Denied"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		#region IDocumentBusinessContext members

		BusinessContext IDocumentBusinessContext.BusinessContext
		{
			get { return BusinessContext.DepositBatch; }
		}

		#endregion
	}
}