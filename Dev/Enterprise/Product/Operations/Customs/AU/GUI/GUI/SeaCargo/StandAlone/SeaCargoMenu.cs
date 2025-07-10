using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public abstract class SeaCargoMenu : AU.GUI.CMRMessageManagementMenu
	{
		public SeaCargoMenu(SeaCargoMessageManager messageManager) : base(messageManager)
		{
		}

		protected override void InitializeMenu()
		{
			MenuItems.Add(new ZMenuItem(Declaration.GUI.Res.GetString("91618401-EA60-4127-B5EC-5B121E445EF3", "Send &Underbond Requests"), new EventHandler(MenuItemSendCMRUnderbondRequests_Click)));
			base.InitializeMenu();
		}

		void MenuItemSendCMRUnderbondRequests_Click(object sender, EventArgs e)
		{
			if (manager != null)
			{
				var oldCursor = Cursor.Current;
				var action = new SendOrignalUnderbondMessageAction();
				try
				{
					Cursor.Current = Cursors.WaitCursor;

					action.SendWithMessageErrors = true;
					action.ShowQuestion += Action_ShowQuestion;
					action.ShowWarning += Action_ShowWarning;

					if (manager.SendOriginalMessages(action).Any())
					{
						Globals.Message.Show(action.LastMessage);
						try
						{
							manager.Factory.Save();
						}
						catch (ZSaveException e1)
						{
							ZExceptionReporting.HandleSaveException(e1);
						}
					}
					else
					{
						if (!action.InvalidOperationText.IsEmpty)
						{
							Globals.Message.ShowError(action.InvalidOperationText, Declaration.GUI.Res.GetString("1F6D0FD9-9ACB-4CBE-B7D5-293F6E17BED0", "Underbond Request Error"));
						}
						else
						{
							Globals.Message.Show(Declaration.GUI.Res.GetString("76E3BE62-B632-4FD1-B545-F742E562310B", "No new messages generated."));
						}
					}
				}
				finally
				{
					Cursor.Current = oldCursor;
					action.ShowQuestion -= Action_ShowQuestion;
					action.ShowWarning -= Action_ShowWarning;
				}
			}
		}

		void Action_ShowQuestion(object sender, ContinueEventArgs e)
		{
			e.Cancel = Globals.Message.Show(e.Message, e.Caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel;
		}

		void Action_ShowWarning(object sender, WarningEventArgs e)
		{
			Globals.Message.Show(e.Message, e.Caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
