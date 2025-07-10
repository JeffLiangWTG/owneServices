using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class CusGoodsCatalogForm : Customs.GUI.CusGoodsCatalogForm
	{
		public CusGoodsCatalogForm(CusGoodsCatalog goodsCatalog)
			: base(goodsCatalog)
		{
			this.goodsCatalog = goodsCatalog;
		}

		readonly CusGoodsCatalog goodsCatalog;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			ReorderTabPages();
		}

		protected override void AddAdornments()
		{
			base.AddAdornments();

			var actionList = new ActionList();
			ZMenuItem CreateSendMessageMenu(string action)
			{
				return new ZMenuItem(actionList.GetMultilingualDescriptionFromCode(action), (s, e) => SendGoodsCatalogMessage(action));
			}

			var sendMessageMenuItem = new ZMenuItem(ResString.GetMultilingualString("08da205d-54ce-4a10-b8b6-191ce4bc5d73", "Send Message"),
				new[]
				{
					updateDraftMessageMenuItem = CreateSendMessageMenu(ActionList.Codes.UpdateDraft),
					createDraftMessageMenuItem = CreateSendMessageMenu(ActionList.Codes.CreateDraft),
					activationMessageMenuItem = CreateSendMessageMenu(ActionList.Codes.Activate),
					createNewVersionMessageMenuItem = CreateSendMessageMenu(ActionList.Codes.CreateNewVersion),
					deactivationMessageMenuItem = CreateSendMessageMenu(ActionList.Codes.Deactivate),
					linkUnlinkForeignOperatorMenuItem = CreateSendMessageMenu(ActionList.Codes.LinkUnlinkForeignOperator),
				});

			var revertChangesMenuItem = new ZMenuItem(ResString.GetMultilingualString("2CAD8C64-7117-4349-9E3D-6DCE1F292BE7", "Revert Changes"), (s, e) => RevertChanges());

			var customsMenuItem = new ZMenuItem(ResString.GetMultilingualString("8D1E6DAD-ADBF-422A-BCA3-49E8091AC37B", "Customs"), new MenuItem[] { sendMessageMenuItem, revertChangesMenuItem });

			customsMenuItem.Popup -= SendMessageContextMenu_Popup;
			customsMenuItem.Popup += SendMessageContextMenu_Popup;

			Menu.MenuItems.Add(Menu.MenuItems.IndexOf(HelpMenuItem), customsMenuItem);
		}

		MenuItem createDraftMessageMenuItem;
		MenuItem updateDraftMessageMenuItem;
		MenuItem activationMessageMenuItem;
		MenuItem createNewVersionMessageMenuItem;
		MenuItem deactivationMessageMenuItem;
		MenuItem linkUnlinkForeignOperatorMenuItem;

		void SendMessageContextMenu_Popup(object sender, EventArgs e)
		{
			var authorityStatus = goodsCatalog?.CGC_AuthorityStatus ?? ZString.Empty;
			var customsStatus = goodsCatalog?.CGC_CustomsStatus ?? ZString.Empty;
			var canSendCatalogMessage = customsStatus.NeedsToSendMessage();

			createDraftMessageMenuItem.Visible = authorityStatus.IsEmpty && canSendCatalogMessage;
			updateDraftMessageMenuItem.Visible = authorityStatus == GoodsCatalogStatusTypeList.Codes.Draft && canSendCatalogMessage;
			activationMessageMenuItem.Visible = authorityStatus == GoodsCatalogStatusTypeList.Codes.Draft || authorityStatus.IsEmpty;
			createNewVersionMessageMenuItem.Visible = (authorityStatus == GoodsCatalogStatusTypeList.Codes.Inactive || authorityStatus == GoodsCatalogStatusTypeList.Codes.Active) && canSendCatalogMessage;
			deactivationMessageMenuItem.Visible = authorityStatus == GoodsCatalogStatusTypeList.Codes.Active;

			var canSendLinkMessage = goodsCatalog.ForeignOperators.Any(c => c.CGI_CustomsStatus.NeedsToSendMessage());
			linkUnlinkForeignOperatorMenuItem.Visible = !authorityStatus.IsEmpty && customsStatus.IsAccepted() && canSendLinkMessage;
		}

		void SendGoodsCatalogMessage(string action)
		{
			if (PreSaveGoodsCatalog(goodsCatalog))
			{
				if (goodsCatalog.IsMessageAwaitingResponse)
				{
					Globals.Message.ShowError(Res.GetString("5EBD55AB-E85C-4D81-B0D9-0E6F6E149A53", "There is message waiting for response. Please wait until the messages are responded."));
				}
				else if (action == ActionList.Codes.Deactivate && (goodsCatalog.CGC_CustomsStatus.IsUpdatePending() || goodsCatalog.ForeignOperators.Any(c => c.CGI_CustomsStatus.IsAccepted() || c.CGI_CustomsStatus.IsDeletePending())))
				{
					Globals.Message.ShowError(Res.GetString("5580D1B0-B35B-41FC-A790-ABB515DCDEF1", "This Catalog cannot be deactivated because there are pending changes to be sent to Customs."));
				}
				else
				{
					var messageSendingObject = new GoodsCatalogMessageSendingObject(goodsCatalog);
					messageSendingObject.Action = action;
					using (var form = new SingleMessageSendingForm(messageSendingObject))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
						{
							var countOfMessages = messageSendingObject.SendMessagesAndSave();
							Globals.Message.Show(ResString.GetMultilingualString("2422CCC4-C531-48E9-AAB4-0F938E06D57A", "{0} message(s) have been sent.", countOfMessages));
						}
					}
				}
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && goodsCatalog.HasChanges && goodsCatalog is IMessageManageableBizObj messageManageable && messageManageable.IsInAStatusAmendmentSendable)
			{
				var manager = messageManageable.GetMessageManagerForAmendmentDetection();
				if (manager != null)
				{
					result = new SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(manager);
				}
			}
			return result;
		}

		bool PreSaveGoodsCatalog(CusGoodsCatalog goodsCatalog) => CustomsPlugIn.FormPreSaved(goodsCatalog, this);

		protected override Customs.GUI.CusGoodsCatalogUserControl GetUserControl()
		{
			return new CusGoodsCatalogUserControl();
		}

		void ReorderTabPages()
		{
			MainTabControl.TabPages.Remove(WorkflowTabPage);
			MainTabControl.TabPages.Add(WorkflowTabPage);
		}

		void RevertChanges()
		{
			if (PreSaveGoodsCatalog(goodsCatalog))
			{
				if (goodsCatalog.CGC_AuthorityIdentifier.IsEmpty && goodsCatalog.IsMessageSent)
				{
					Globals.Message.ShowError(Res.GetString("A28B8819-D676-4FC4-B405-BEE33E21A72C", "This catalog does not meet the criteria to be reverted (it does not have an Authority Identifier, and its Message Status is different from NOT - Not Sent)."));
				}
			}
		}
	}
}
