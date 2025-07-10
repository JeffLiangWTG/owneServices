using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	[SuppressFormsLocalizedTest]
	public class CMRMessageManagementMenu : MessageManagementMenu
	{
		public CMRMessageManagementMenu(Business.MultiMessageManager manager)
			: base(manager)
		{
		}

		protected override CargoWise.Types.ZString MessagingHelpText
		{
			get { return CMRMessage.MessagingHelpMenuCaption; }
		}

		protected override CargoWise.Types.ZString MessagingHelpURL
		{
			get { return CMRMessage.MessagingHelpUpdateNoteURL; }
		}

		protected ZForm MainForm
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}

		protected virtual ZGlobalMutex GetMutex()
		{
			return null;
		}

		protected override bool SendMessagesClickCore(object sender)
		{
			var result = false;
			var mutex = GetMutex();
			if (mutex != null)
			{
				try
				{
					if (mutex.Lock())
					{
						result = base.SendMessagesClickCore(sender);
					}
					else
					{
						DisplayLockedMessage(mutex.GetLockInfo());
					}
				}
				finally
				{
					if (mutex.HasLock)
					{ mutex.Unlock(); }
				}
			}
			else
			{
				result = base.SendMessagesClickCore(sender);
			}
			return result;
		}

		protected void DisplayLockedMessage(LockInfo info)
		{
			var userWithLock = info != null && info.UserWithLock != null ? info.UserWithLock.GS_FullName.ToString() : "someone else";
			Globals.Message.Show(string.Format("Messages are currently being generated and sent by {0} for this master, please try again later.", userWithLock), "Try later", MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}
	}
}
