using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.GUI
{
	public class MiscEDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			SetMenuItemVisibility();
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();

			MenuItems.Add(menu008 = new ZMenuItem(SendMenuText.Send008, Send008_Click));
			MenuItems.Add(menuD87 = new ZMenuItem(SendMenuText.SendD87, SendD87_Click));
			MenuItems.Add(menu5SM = new ZMenuItem(SendMenuText.Send5SM, Send5SM_Click));
		}

		void SetMenuItemVisibility()
		{
			menu008.Visible = Declaration?.IsPersonalItemDeclaration ?? false;
			menuD87.Visible = Declaration?.IsD87 ?? false;
			menu5SM.Visible = Declaration?.Is5SM ?? false;
		}

		void Send008_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._008);
		void SendD87_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._D87);
		void Send5SM_Click(object sender, EventArgs e) => SendMessage(ElectronicDocumentTypeList.Codes._5SM);

		void SendMessage(ZString messageType)
		{
			if (Declaration != null && EDIMenuMethods.DeclarationHasEntry(Declaration, Form))
			{
				if (PreSaveDeclaration(Declaration))
				{
					bool shouldContinue = false;
					var wrapper = GetJobDeclarationMessageSendingObjectParent(Declaration, messageType, MessageFunctionCode.Original);
					var parentObject = wrapper as JobDeclarationMessageSendingObjectParent;
					var sendingObjectsCollection = parentObject.SendingObjectsCollection;
					if (sendingObjectsCollection.Count == 1)
					{
						sendingObjectsCollection[0].ShouldSend = true;
						if (sendingObjectsCollection[0].HasErrors)
						{
							Globals.Message.Show(
								Res.GetString("F97AD2D0-1F75-4242-910E-766A6DCEE8F8", "There are following errors. Please check and rectify the problems before attempting to print this document again.") + "\r\n" +
								sendingObjectsCollection[0].Notifications.GetErrors().ToUniqueMessageListString("\r\n"),
								Res.GetString("E847F310-D1BC-468F-BC32-82D9B596AE73", "Errors"),
								ZMessageBoxButtons.OK,
								ZMessageBoxIcon.Error,
								ZDialogResult.OK
							);
						}
						else
						{
							var bizObjValidationMessageErrors = sendingObjectsCollection[0].BizObjValidationMessageErrors;
							if (!bizObjValidationMessageErrors.IsEmpty)
							{
								if (Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
								{
									if (Globals.Message.Show(bizObjValidationMessageErrors, Res.GetString("5724F6B2-0AF0-40A6-90FA-291C00F31F26", "Validation Errors"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
									{
										shouldContinue = true;
									}
								}
								else
								{
									Globals.Message.Show(bizObjValidationMessageErrors, Res.GetString("5724F6B2-0AF0-40A6-90FA-291C00F31F26", "Validation Errors"), ZMessageBoxButtons.OK, ZMessageBoxIcon.Error);
								}
							}
							else
							{
								shouldContinue = true;
							}
						}
					}
					if (shouldContinue)
					{
						wrapper.SendMessage(MessageFunctionCode.Original, Declaration);
					}
				}
			}
		}

		protected override bool DisplayGenerateEntriesMenuOption => true;

		protected virtual IJobDeclarationMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent(JobDeclaration declaration, ZString messageType, MessageFunctionCode messageFunctionCode)
		{
			return JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, messageType, messageFunctionCode);
		}
		ZMenuItem menu008;
		ZMenuItem menuD87;
		ZMenuItem menu5SM;

		class SendMenuText
		{
			static public string Send008 => ResString.GetMultilingualString("CD7FB9CF-D28C-4683-918B-D6EA2356A4AD", "Send 008 - Personal Items Declaration");
			static public string SendD87 => ResString.GetMultilingualString("3A16D117-D9E6-4CDC-B504-7B3ADEC02368", "Send D87 - Carnet Temporary Import Certificate");
			static public string Send5SM => ResString.GetMultilingualString("A1613A58-D4B1-442D-B002-16B54FD78064", "Send 5SM - Valuation Declaration Template");
		}
	}
}
