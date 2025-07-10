#if DEBUG

using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class GLJournalModule
	{
		public void ExportJournalCSVEventHandler_ForTestOnly(object sender, EventArgs args)
		{
			ExportJournalCSVEventHandler(sender, args);
		}

		public void ImportJournalXMLEventHandler_ForTestOnly(object sender, EventArgs args)
		{
			ImportJournalXMLEventHandler(sender, args);
		}

		public ResourceStringData GetDeleteMenuItemText_ForTestOnly()
		{
			return GetDeleteMenuItemText();
		}

		public void ImportJournalCSVEventHandler_ForTestOnly(object sender, EventArgs args)
		{
			ImportJournalCSVEventHandler(sender, args);
		}

		public MultilingualString ForeignCurrencyBalanceAdjustmentMenuItemText_ForTestOnly
		{
			get { return ForeignCurrencyBalanceAdjustmentMenuItemText; }
			set { ForeignCurrencyBalanceAdjustmentMenuItemText = value; }
		}

		public MenuItem[] GetNewActionMenuItems_ForTestOnly()
		{
			return GetNewActionMenuItems();
		}

		public BusinessObject[] SelectedBusinessObjects_ForTestOnly() => SelectedBusinessObjects;

		public void HandleRegenerateJournalEntries_ForTestOnly(object sender, EventArgs e) => HandleRegenerateJournalEntries(sender, e);

		public MultilingualString ReverseAndRedoCurrencyAdjustmentMenuItemText_ForTestOnly
		{
			get { return ReverseAndRedoCurrencyAdjustmentMenuItemText; }
			set { ReverseAndRedoCurrencyAdjustmentMenuItemText = value; }
		}

		public MultilingualString UploadGLJournalsMenuItemText_ForTestOnly
		{
			get { return UploadGLJournalsMenuItemText; }
			set { UploadGLJournalsMenuItemText = value; }
		}

		public IBusinessObjectCollection GetNewGridCollection_ForTestOnly()
		{
			return GetNewGridCollection();
		}

		public void HandlePrint_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrint(sender, e);
		}

		public Business.GeneralLedger.GLJournals.GLJournal CurrentlySelectedJournal_ForTestOnly => CurrentlySelectedJournal;

		public void HandleDeleteClickCore_ForTestOnly(object sender, EventArgs e)
		{
			HandleDeleteClickCore(sender, e);
		}

		public void HandlePrintAccountingJournal_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrintAccountingJournal(sender, e);
		}

		public SecurityCheckpoint AuditSecurityCheckpoint_ForTestOnly => AuditSecurityCheckpoint;

		public SecurityCheckpoint UndoAuditSecurityCheckpoint_ForTestOnly => UndoAuditSecurityCheckpoint;

		public bool CheckCanReverseAndRedoCurrencyAdjustment_ForTestOnly()
		{
			return CheckCanReverseAndRedoCurrencyAdjustment();
		}

		public bool CheckCanReverse_ForTestOnly(Business.GeneralLedger.GLJournals.GLJournal glJournal)
		{
			return CheckCanReverse(glJournal);
		}

		public MenuItem[] ContextMenu_ForTestOnly => ContextMenu;

		public void HandleEditClick_ForTestOnly(object sender, EventArgs e)
		{
			HandleEditClick(sender, e);
		}

		public void HandleUploadGLJournals_ForTestOnly(object sender, EventArgs args)
		{
			HandleUploadGLJournals(sender, args);
		}

		public MenuItem FindMenuItemByText_ForTestOnly(string text)
		{
			var menuItem = GetNewActionMenuItems();
			return menuItem.FindByText(text);
		}

		public bool AllowAdvancedDataAutomationWizard_ForTestOnly => AllowAdvancedDataAutomationWizard;
	}
}

#endif
