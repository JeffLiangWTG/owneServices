using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public EDIMenu() : base()
		{
		}

		protected override void AddAdditionalMenuItems()
		{
			sendUniversalCustomsMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("989E677A-C501-41D2-959B-E2093472F392", "Send to Customs"));
			sendUniversalCustomsMessagingMenuItem.Click += SendUniversalCustomsMessagingMenuItem_Click;
			sendUniversalCustomsMessagingMenuItem.Visible = Declaration != null;
			MenuItems.Add(sendUniversalCustomsMessagingMenuItem);
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			sendUniversalCustomsMessagingMenuItem.Visible = Declaration != null && !Declaration.IsDeclarationIntegrated;
		}

		protected override JobDeclarationUniversalMessagingHelper GetJobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent wrapper)
		{
			return new CNJobDeclarationUniversalMessagingHelper(wrapper);
		}

		public new JobDeclaration Declaration => base.Declaration as JobDeclaration;

		protected override bool DisplayGenerateEntriesMenuOption => true;

		protected void SendUniversalCustomsMessagingMenuItem_Click(object sender, EventArgs e)
		{
			var needMerge = !Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge;
			if ((!needMerge || PerformMerge()) && RunPreSaveValidationIfHasNoChanges(Declaration) && PreSaveDeclaration(Declaration))
			{
				var formWrapper = new CNJobDeclarationMessageSendingObjectParent(Declaration);
				var messagingHelper = GetJobDeclarationUniversalMessagingHelper(formWrapper);
				var validateResult = formWrapper.ValidateCanSubmit();

				if (validateResult.IsEmpty)
				{
					using (var form = GetMessageSendingForm(formWrapper))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
						{
							var shouldSendUniversalMessage = !ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.CNBuildMessageInCW1, Core.Constants.CountryCodes.China, ZDateTime.Now);

							int messagesCount = shouldSendUniversalMessage ? messagingHelper.SendUniversalMessage() : formWrapper.SendMessages();
							if (messagesCount > 0)
							{
								foreach (Business.CusEntryHeader entryHeader in Declaration.ActiveEntryHeaders)
								{
									entryHeader.Messages.Load();
								}
								Globals.Message.ShowInformation(Res.GetString("f5750eba-7399-4222-a91a-e3a25406c0e8", "{0} message(s) queued for sending", messagesCount));
							}
						}
					}
				}
				else
				{
					Globals.Message.ShowError(validateResult);
				}
			}
		}

		protected ZForm GetMessageSendingForm(IJobDeclarationMessageSendingObjectParent wrapper)
		{
			var sendingParent = wrapper as BaseMessageSendingObjectParent ?? throw new DeveloperNotificationException("Parameter wrapper must be a instance of BaseMessageSendingObjectParent");
			return new MessageSendingFormWithValidationDetails(sendingParent);
		}

		protected ZMenuItem sendUniversalCustomsMessagingMenuItem;
	}
}
