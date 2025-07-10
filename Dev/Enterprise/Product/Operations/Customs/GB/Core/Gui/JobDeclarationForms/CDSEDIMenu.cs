using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.CDS.Messaging.DocumentSending;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.MessageManagers;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders.Query;
using Enterprise.Customs.GB.GUI.Wizards;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public class CDSEDIMenu : EU.GUI.EDIMenu
	{
		public override void RefreshMenu()
		{
			base.RefreshMenu();

			queryCDSMenuItem = CreateQueryMenu();
		}

		public new JobDeclaration Declaration
		{
			get => (JobDeclaration)base.Declaration;
			set => base.Declaration = value;
		}

		public CDSEDIMenu()
		{
			SendsMessagesToCustoms = new CDSSendsMessagesToCustomsGUI();
			Text = CDSMenuCaption;
		}

		protected override void AddAuditMenuItems()
		{
		}

		protected override void SetupTopLevelMenu()
		{
			MenuItems.Clear();

			sendToCDSMenuItem = new ZMenuItem(SendToCDS, SendToCDS_Click);
			uploadSupportingDocumentsMenuItem = new ZMenuItem(UploadSupportingDocuments, UploadSupportingDocsMenu_Click);
			jobDeclarationWizardMenuItem = new ZMenuItem(JobDeclarationWizardCaption, JobDecWizard_Click);
			queryCDSMenuItem = CreateQueryMenu();

			MenuItems.AddRange(new MenuItem[]
			{
				sendToCDSMenuItem,
				uploadSupportingDocumentsMenuItem,
				jobDeclarationWizardMenuItem,
				queryCDSMenuItem
			});
		}

		ZMenuItem CreateQueryMenu()
		{
			if (queryCDSMenuItem == null)
			{
				queryCDSMenuItem = new ZMenuItem(CDSQueryMenuCaption);
			}
			else
			{
				queryCDSMenuItem.MenuItems.Clear();
			}

			if ((Declaration?.ActiveEntryHeaders?.Count ?? 0) > 0)
			{
				queryCDSMenuItem.Enabled = true;
				var mrnMenu = new ZMenuItem(CDSQueryMRNMenuCaption);
				mrnMenu.MenuItems.AddRange(new MenuItem[]
				{
					CreateQueryMenuChild(CDSQueryMRNMenuSummaryCaption, new CDSQueryMRNSendingObject(Declaration, QueryNotificationType.Status)),
					CreateQueryMenuChild(CDSQueryMRNMenuSnapshotCaption, new CDSQueryMRNSendingObject(Declaration, QueryNotificationType.Full)),
				});
				queryCDSMenuItem.MenuItems.AddRange(new MenuItem[]
				{
					mrnMenu,
					CreateQueryMenuChild(CDSQueryDUCRMenuCaption, new CDSQueryDUCRSendingObject(Declaration)),
					CreateQueryMenuChild(CDSQueryInventoryMenuCaption, new CDSQueryInventorySendingObject(Declaration)),
					CreateQueryMenuChild(CDSQueryUCRMenuCaption, new CDSQueryUCRSendingObject(Declaration))
				});
			}
			else
			{
				queryCDSMenuItem.Enabled = false;
			}

			return queryCDSMenuItem;
		}

		MenuItem CreateQueryMenuChild(ZString caption, CDSQuerySendingObject sendingObject)
		{
			var menuItem = new ZMenuItem(caption, QueryMenu_Click);

			menuItem.Tag = sendingObject;

			foreach (var menuOption in sendingObject.GetMenuOptions())
			{
				var child = new ZMenuItem(menuOption.Value, QueryMenu_Click);
				child.Tag = menuOption.Key;

				menuItem.MenuItems.Add(child);
			}

			return menuItem;
		}

		void QueryMenu_Click(object sender, EventArgs e)
		{
			var menu = sender as MenuItem;

			if (menu != null)
			{
				var menuParam = ZGuid.Empty;
				var sendingObject = menu.Tag as CDSQuerySendingObject;
				if (sendingObject == null)
				{
					sendingObject = menu.Parent.Tag as CDSQuerySendingObject;
					menuParam = (ZGuid)menu.Tag;
				}

				new CDSQuerySendingManager(sendingObject, menuParam).Send();
			}
		}

		void SendToCDS_Click(object sender, EventArgs e)
		{
			SendToCds();
			if (sender is ZMenuItem menuItem && menuItem.Parent is EU.GUI.EDIMenu menu && MessageResponseSemaphoreHelper.SemaphoreExistsForDeclaration((JobDeclaration)menu.Declaration))
			{
				var parentForm = menuItem.ParentControl;
				var declarationUserControl = parentForm as JobDeclarationUserControl ?? parentForm.FindSingleOrDefault<JobDeclarationUserControl>();
				declarationUserControl?.StartWaitForMessageResponseSemaphore(parentForm);
			}
		}

		void SendToCds()
		{
			if (PreSaveDeclaration(Declaration))
			{
				bool continueWithSend = true;
				if (Declaration.CustomsEntryHeaders.Count == 0)
				{
					if (Globals.Message.Show(Res.GetString("30f6c426-45dd-426e-9a29-dd552f3c741d", "There are no entries. Would you like to merge (generate entries) now, save and proceed?"), ZString.Empty,
						MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						if (Declaration.DoMerge(SendsMessagesToCustoms))
						{
							continueWithSend = Form.FireSaveButton() == ContinueWithSave.Yes;
						}
						else
						{
							Globals.Message.Show(Res.GetString("9D36C43B-E634-4431-8E75-C6A957EE607D", "Error when merging {0}", Declaration.JobNumber));
							continueWithSend = false;
						}
					}
					else
					{
						continueWithSend = false;
					}
				}

				if (continueWithSend)
				{
					var decWrapper = new JobDeclarationMessageSendingObjectParent(Declaration);

					using (var form = GetMessageSendingForm(decWrapper))
					{
						continueWithSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK && Customs.Business.MessageManagerCreditCheckWithSecurityHelper.CheckDeniedParty(Declaration);
					}

					if (continueWithSend)
					{
						using (Form != null ? new CellNotificationSuspender(Form) : null)
						using (Declaration.Factory != null ? ActiveBusinessObjectCollection.DelayListChangedEvents(Declaration.Factory) : null)
						{
							var sender = new CDSMessageSender(decWrapper);
							sender.Send(SendsMessagesToCustoms);
						}
					}
				}
			}
		}

		protected virtual MessageSendingForm GetMessageSendingForm(JobDeclarationMessageSendingObjectParent decWrapper) => new MessageSendingForm(decWrapper);

		void UploadSupportingDocsMenu_Click(object sender, EventArgs e)
		{
			if (PreSaveDeclaration(Declaration))
			{
				var decWrapper = new JobDeclarationSupportingDocSendingObjectParent(Declaration);
				using (var form = new SupportingDocSendingForm(decWrapper))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						new CDSSupportingDocSendingManager(decWrapper, new MessageNotificationCollector()).SendMessages();
					}
				}
			}
		}

		void JobDecWizard_Click(object sender, EventArgs e)
		{
			DeclarationWizardGuiHelper.WizardClicked(Declaration);
		}

		protected readonly ISendsMessagesToCustoms SendsMessagesToCustoms;

		ZMenuItem sendToCDSMenuItem;
		protected ZMenuItem uploadSupportingDocumentsMenuItem;
		ZMenuItem jobDeclarationWizardMenuItem;
		ZMenuItem queryCDSMenuItem;

		public static ZString CDSMenuCaption => ResString.GetMultilingualString("AD75A065-B728-4B30-A220-051461BC282D", "CDS");
		public static ZString SendToCDS => ResString.GetMultilingualString("6F78F165-09ED-4A6D-BC53-5EDA0B89D79F", "Send to CDS");
		static ZString UploadSupportingDocuments => ResString.GetMultilingualString("AAAB637E-BC40-44E2-B2F2-524BB6702A24", "Upload supporting documents");
		public static ZString JobDeclarationWizardCaption => ResString.GetMultilingualString("AD75A065-B728-4B30-A220-051461BC263A", "Help me make a CDS declaration");
		public static ZString SendToCDSNotEnabled => ResString.GetMultilingualString("3023ED1D-972D-4F33-83BA-2EB77E9D2D19", "CDS is not yet enabled for your company.");

		public static ZString CDSQueryMenuCaption => ResString.GetMultilingualString("778DD729-2519-4CE5-8CED-75BB272FF011", "Query CDS");
		public static ZString CDSQueryMRNMenuCaption => ResString.GetMultilingualString("6E894850-373D-4D39-9A25-71C80B78D7EA", "By movement reference (MRN)");
		public static ZString CDSQueryMRNMenuSummaryCaption => ResString.GetMultilingualString("2DD9D42C-B7A2-4117-8E9F-9CA8CAE838FF", "Summary with Status Update");
		public static ZString CDSQueryMRNMenuSnapshotCaption => ResString.GetMultilingualString("F5AEDDF7-CEE9-4650-8C22-8C8BA74858A5", "Snapshot");
		public static ZString CDSQueryDUCRMenuCaption => ResString.GetMultilingualString("478891CB-7A19-436F-821B-EBB7BEEB4024", "By previous document type DCR (DUCR)");
		public static ZString CDSQueryInventoryMenuCaption => ResString.GetMultilingualString("EC2EA1AA-8C2A-43FD-9C95-9B839A4D1088", "By inventory reference (MUCR)");
		public static ZString CDSQueryUCRMenuCaption => ResString.GetMultilingualString("CA9241DA-3DD9-48E3-AB21-8AB58EB34CD6", "By entry reference (data element 2/4 - UCR)");
	}
}
