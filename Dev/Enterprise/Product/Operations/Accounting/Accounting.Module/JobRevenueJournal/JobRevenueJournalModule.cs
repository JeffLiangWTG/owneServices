using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class JobRevenueJournalModule : FilterGridModuleWithMultipleReversing
	{
		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.JobRevenueJournal; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.JobRevenueJournal; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.JobRevenueJournal);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobRevenueJournalFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new JobRevenueJournalCollection(Factory);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			if (DeleteMenuItem != null)
			{
				menuItems.Remove(DeleteMenuItem);
			}
			if (CopyMenuItem != null)
			{
				menuItems.Remove(CopyMenuItem);
			}

			if (DeleteMenuItem != null)
			{
				menuItems.Add(DeleteMenuItem);
			}

			menuItems.Add(new ZMenuItem(ResString.GetMultilingualString("C52CC548-A984-46AF-878A-43A8E42D246A", "&Print"), HandlePrint));
			menuItems.Last().MenuItems.Add(new ZMenuItem(AccountingJournalPrintHelper.PrintAccountingJournalText, new EventHandler(HandlePrintAccountingJournal)));

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
			{
				menuItems.Last().MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("2E980903-D798-4e67-B0A2-8F88E1800803", "Print Accounting Voucher"), new EventHandler(HandlePrintAccountingVoucher)));
			}

			return menuItems.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewActionMenuItems());

			RegenerateJournalEntriesHelper.AddRegenerateJournalEntriesMenuItemIfAllowed(menuItems, HandleRegenerateJournalEntries);

			menuItems.Add(new ZMenuItem("-"));
			menuItems.Add(new ZMenuItem(AccountingConstants.AuditAndCashActionText.AuditTransactionText, (sender, e) => AccountingAuditHelper.HandleAuditTransaction(this, new AuditAndCashEventArgs(AuditSecurityCheckpoint, SelectedTransactions))));
			menuItems.Add(new ZMenuItem(AccountingConstants.AuditAndCashActionText.UndoAuditTransactionText, (sender, e) => AccountingAuditHelper.HandleUndoAuditTransaction(this, new AuditAndCashEventArgs(UndoAuditSecurityCheckpoint, SelectedTransactions))));
			return menuItems.ToArray();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobRevenueJournalFilterBusinessObject();
		}

		SecurityCheckpoint printSecurityCheckpoint => Env.Security.PrintJobRevenueJournal;

		void HandlePrint(object sender, EventArgs e)
		{
			if (CurrentTransaction == null)
			{
				Globals.Message.Show(Res.GetString("d7510303-ac63-4a05-8091-03d219a34e97", "Please select a transaction to print"));
			}
			else if (!printSecurityCheckpoint.IsAllowed)
			{
				printSecurityCheckpoint.ShowError();
			}
			else
			{
				AccPrintingUtility printUtil = new AccPrintingUtility(Factory, Constants.DataContext.JobRevenueJournal);
				printUtil.PrintDocument(CurrentTransaction, (NoResString)"Job Revenue Journal", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			}
		}

		void HandlePrintAccountingJournal(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("4d853794-d2b2-4cc6-b1b5-7a4d6005ecbe", "Please select Journal(s) to print."));
			}
			else
			{
				AccountingJournalPrintHelper.PrintAccountingJournal(SelectedBusinessObjects.Cast<JobRevenueJournal>());
			}
		}

		void HandlePrintAccountingVoucher(object sender, EventArgs e)
		{
			if (SelectedBusinessObjects == null || SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(Res.GetString("7F65D9B4-0D5D-4c77-95BB-C64F1ED4CB45", "Please select Journal(s) to print"));
			}
			else
			{
				AccountingVoucherPrintHelper accountingVoucherPrintHelper = new AccountingVoucherPrintHelper();
				TransactionHeader[] transactions = Grid.GetSelectedElements<TransactionHeader>();
				PrintTask task = accountingVoucherPrintHelper.GetAccountingVoucherPrintTask(transactions);
				if (task != null)
				{
					task.Run(Env.Security.None);
				}
			}
		}

		#region Audit & Undo Aduit

		AccTransactionHeader[] SelectedTransactions => Grid.GetSelectedElements<AccTransactionHeader>();

		TransactionHeader CurrentTransaction => CurrentBusinessObjectInGrid as TransactionHeader;

		SecurityCheckpoint AuditSecurityCheckpoint => Env.Security.JobRevenueJournalAuditTransaction;

		SecurityCheckpoint UndoAuditSecurityCheckpoint => Env.Security.JobRevenueJournalUndoAuditTransaction;

		#endregion
	}
}
