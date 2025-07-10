#if DEBUG

using System;
using System.Windows.Forms;
using Enterprise.Security;

namespace Enterprise.Accounting.Module
{
	public partial class JobRevenueJournalModule
	{
		public void HandlePrint_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrint(sender, e);
		}

		public SecurityCheckpoint AuditSecurityCheckpoint_ForTestOnly => AuditSecurityCheckpoint;

		public SecurityCheckpoint UndoAuditSecurityCheckpoint_ForTestOnly => UndoAuditSecurityCheckpoint;

		public void HandlePrintAccountingJournal_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrintAccountingJournal(sender, e);
		}

		public MenuItem[] ContextMenu_ForTestOnly => ContextMenu;
	}
}

#endif
