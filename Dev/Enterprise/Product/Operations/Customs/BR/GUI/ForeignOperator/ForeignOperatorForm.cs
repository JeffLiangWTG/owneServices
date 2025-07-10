using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ForeignOperatorForm : ZTemplateForm
	{
		public ForeignOperatorForm() : base()
		{
			if (!this.IsDesignMode())
			{
				throw new InvalidOperationException("This constructor is only for the designer. Please use the one that takes a business object.");
			}
		}

		public ForeignOperatorForm(CusBRForeignOperator businessEntity)
			: base(businessEntity)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			WorkflowTabPage.Initialize(BRForeignOperator);
		}

		CusBRForeignOperator BRForeignOperator => (CusBRForeignOperator)base.BusinessEntity;

		public override string FormCaption => ResString.GetMultilingualString("C76F2AA9-7F9E-4AB1-A868-C5C1AA9721C5", "Foreign Operator - {0}", BRForeignOperator?.ForeignOperator?.OH_Code);

		protected override void AddAdornments()
		{
			base.AddAdornments();
			var actionList = new ActionList();
			ZMenuItem CreateSendMessageMenu(string action)
			{
				return new ZMenuItem(actionList.GetMultilingualDescriptionFromCode(action), (s, e) => SendForeignOperatorMessage(action));
			}

			var sendMessageMenuItem = new ZMenuItem(ResString.GetMultilingualString("EBF9B218-5C6C-4479-B715-6B01D883EE2E", "Send Message"),
				new[]
				{
					activationMessageMenuItem = CreateSendMessageMenu(ActionList.Codes.Activate),
					createNewVersionMessageMenuItem = CreateSendMessageMenu(ActionList.Codes.CreateNewVersion),
					deactivationMessageMenuItem = CreateSendMessageMenu(ActionList.Codes.Deactivate)
				});

			var customsMenuItem = new ZMenuItem(ResString.GetMultilingualString("88281765-DE59-41DA-BB53-60C96506F382", "Customs"), new MenuItem[] { sendMessageMenuItem });

			customsMenuItem.Popup -= SendMessageContextMenu_Popup;
			customsMenuItem.Popup += SendMessageContextMenu_Popup;

			Menu.MenuItems.Add(Menu.MenuItems.IndexOf(HelpMenuItem), customsMenuItem);
		}

		MenuItem activationMessageMenuItem;
		MenuItem createNewVersionMessageMenuItem;
		MenuItem deactivationMessageMenuItem;

		void SendMessageContextMenu_Popup(object sender, EventArgs e)
		{
			var isActivateVisible = BRForeignOperator != null && (BRForeignOperator.BFR_AuthorityIdentifier.IsEmpty || BRForeignOperator.BFR_CustomsStatus == ForeignOperatorCustomsStatusTypeList.Codes.Inactive);

			activationMessageMenuItem.Visible = isActivateVisible;
			createNewVersionMessageMenuItem.Visible = !isActivateVisible;
			deactivationMessageMenuItem.Visible = !isActivateVisible;
		}

		void SendForeignOperatorMessage(string action)
		{
			if (PreSaveForeignOperator(BRForeignOperator))
			{
				if (BRForeignOperator.IsMessageAwaitingResponse)
				{
					Globals.Message.ShowError(Res.GetString("B802ECDD-6E0F-47AC-BE3E-2AB48245A896", "There is message waiting for response. Please wait until the messages are responded."));
				}
				else
				{
					var messageSendingObjectParent = new ForeignOperatorMessageSendingObject(BRForeignOperator);
					messageSendingObjectParent.Action = action;
					using (var form = new SingleMessageSendingForm(messageSendingObjectParent))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
						{
							var countOfMessages = messageSendingObjectParent.SendMessagesAndSave();
							Globals.Message.Show(ResString.GetMultilingualString("1AE3A053-6EE2-4BD3-A5F8-FC0BBA85C638", "{0} message(s) have been sent.", countOfMessages));
						}
					}
				}
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && BRForeignOperator.HasChanges && BRForeignOperator is IMessageManageableBizObj messageManageable && messageManageable.IsInAStatusAmendmentSendable)
			{
				var manager = messageManageable.GetMessageManagerForAmendmentDetection();
				if (manager != null)
				{
					result = new SendsMessagesToCustomsGUI().DetermineRequiredMessagesAndSendThem(manager);
				}
			}
			return result;
		}

		bool PreSaveForeignOperator(CusBRForeignOperator foreignOperator) => CustomsPlugIn.FormPreSaved(foreignOperator, this);
	}
}
