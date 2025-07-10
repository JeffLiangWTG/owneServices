using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class InvoiceBatchModule : FilterGridModuleWithMultipleReversing
	{
		#region Overrides

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new InvoiceBatchController();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new InvoiceBatchHeaderCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new InvoiceBatchFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new InvoiceBatchFilterBusinessObject();
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("09f22aa6-4542-4f52-9501-e30a996be9ba", "Cancel Batch", "Cancels the selected item after viewing its details read-only (shortcut Del)");
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (EditMenuItem != null)
			{
				menuItems.Remove(EditMenuItem);
			}
			if (NewMenuItem != null)
			{
				var newIndex = menuItems.IndexOf(NewMenuItem);
				NewMenuItem = new ZMenuItem(ResString.GetMultilingualString("MenuItem.New", "New"));
				NewMenuItem.MenuItems.Add(new ZMenuItem(NewBatchText, new EventHandler(HandleNewClick)));
				NewMenuItem.MenuItems.Add(new ZMenuItem(NewBulkBatchText, new EventHandler(HandleNewBulkBatch)));
				menuItems[newIndex] = NewMenuItem;
			}

			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			result.Add(new ZMenuItem(PrintMenuText, HandlePrint));
			return result.ToArray();
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.InvoiceBatch; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.InvoiceBatch; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override bool CanBeCopied()
		{
			return false;
		}

		#endregion

		#region EventHandlers

		protected virtual void HandlePrint(object sender, EventArgs e)
		{
			InvoiceBatchHeader batchHeader = CurrentBusinessObjectInGrid as InvoiceBatchHeader;
			Print(batchHeader);
		}

		protected virtual void HandleNewBulkBatch(object sender, EventArgs e)
		{
			ZController controller = (InvoiceBulkBatchController)ZControllerFactory.Create(ControllerIDs.InvoiceBulkBatch);
			controller.SetFormsModalTo(ParentModalForm);
			controller.ShowNewForm();
		}

		#endregion

		#region Implementation

		void Print(InvoiceBatchHeader batchHeader)
		{
			if (batchHeader != null)
			{
				if (batchHeader.AH_IsCancelled)
				{
					Globals.Message.ShowError(Res.GetString("ba246152-9c89-427c-95e3-a5eb6afbd92e", "You cannot print canceled Invoice Batch Statement."), Res.GetString("fec981a7-7176-4ace-af1c-059ce79214da", "Invoice Batch Statement"));
				}
				else
				{
					new InvoiceBatchHeaderPrintTask(batchHeader.PK).Run();
				}
			}
		}

		protected MultilingualString PrintMenuText
		{
			get { return ResString.GetMultilingualString("1763e9de-375c-4400-bd44-193aadb8256e", "&Print"); }
		}
		protected MultilingualString NewBatchText
		{
			get { return ResString.GetMultilingualString("387ec8b5-d57b-4d29-af98-2cd96704834c", "&New Batch"); }
		}
		protected MultilingualString NewBulkBatchText
		{
			get { return ResString.GetMultilingualString("5a2509ca-cbc0-4f31-ad78-00d808169503", "New &Bulk Batch"); }
		}

		#endregion
	}
}
