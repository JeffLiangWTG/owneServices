using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GUI;
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
	/// <summary>
	/// Module Controller for WIPAccruals.
	/// </summary>
	public partial class WIPAccrualsModule : FilterGridModuleWithMultipleReversing
	{
		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.WipsAndAccruals; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WIPAccruals; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		#region Implementation

		protected static MultilingualString PrintMenuItemText
		{
			get { return ResString.GetMultilingualString("e0b06171-b049-4384-a73d-875001ba8a6a", "&Print"); }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			if (selectedBusinessObject is TransactionLine)
			{
				if (((TransactionLine)selectedBusinessObject).AL_LineType == TransactionLineTypes.WIP)
				{
					return new WIPController();
				}
				else
				{
					return new AccrualController();
				}
			}

			return null;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WIPAccrualsFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WIPAccrualCollection(Factory, new ZQuery());
		}

		#region Grid Context Menu

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("2c9d432a-9cdb-4845-bbbc-69a7d884b304", "Reverse", "Creates a new reversed item(s) to offset the currently selected item(s) (shortcut Del)");
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			menuItems.Remove(NewMenuItem);
			menuItems.Remove(EditMenuItem);
			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			ZMenuItem printMenuItem = null;
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			menuItems.Insert(1, printMenuItem = new ZMenuItem(PrintMenuItemText));
			printMenuItem.MenuItems.Add(new ZMenuItem(AccountingJournalPrintHelper.PrintAccountingJournalText, new EventHandler(HandlePrintAccountingJournal)));
			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			RegenerateJournalEntriesHelper.AddRegenerateJournalEntriesMenuItemIfAllowed(menuItems, HandleRegenerateJournalEntries);
			return menuItems.ToArray();
		}

		#endregion

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WIPAccrualsFilterBusinessObject();
		}

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			return ShowViewForm(selectedBusinessObject);
		}

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			IZForm result = null;
			BaseWIPAccrual bizObj = (BaseWIPAccrual)selectedBusinessObject;

			if (bizObj.IsReversed)
			{
				Globals.Message.ShowError(bizObj.AlreadyReversedErrorMessage, Res.GetString("5610ee71-529c-4e51-bc12-538e543d32f6", "Reverse WIP/Accrual"));
			}
			else if (bizObj.IsApportioned)
			{
				ZString message = Res.GetString("7e202680-5433-4d09-8f5b-2048925e2083", "This accrual is apportioned at Consol level.") + "\r\n";
				message += Res.GetString("02edf87c-10e8-4ba1-b230-764db2f7fe09", "Please go to the Costing Tab of Consol {0} to reverse this accrual.", bizObj.JK_UniqueConsignRef);
				Globals.Message.ShowError(message, Res.GetString("5610ee71-529c-4e51-bc12-538e543d32f6", "Reverse WIP/Accrual"));
			}
			else if (!bizObj.CanReverseWhenRelatedJobStatusIsJFC)
			{
				Globals.Message.ShowError(Res.GetString("68C64066-7300-49AA-99FC-DF022D8DFDC1", "This {0} cannot be reversed as the related job has Ready For Financial Closure status.", bizObj.AL_LineType), Res.GetString("E1067A64-A8ED-4B5D-A2A3-D28570799D46", "Reverse WIP/Accrual"));
			}
			else
			{
				result = base.ShowDeleteForm(selectedBusinessObject);
			}

			return result;
		}

		protected override bool IsUserAllowedForMultipleReversing()
		{
			var isAllowed = Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed;
			if (!isAllowed)
			{
				Env.Security.ReverseMultipleWipsAndAccruals.ShowError();
			}
			return isAllowed;
		}

		#region EventHandlers		

		protected virtual void HandlePrintAccountingJournal(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("4275de68-9cf8-4aa4-b0aa-273b840e6afe", "Please select transaction(s) to print."));
			}
			else
			{
				AccountingJournalPrintHelper.PrintAccountingJournal(SelectedBusinessObjects.Cast<BaseWIPAccrual>());
			}
		}

		#endregion

		#endregion
	}
}
