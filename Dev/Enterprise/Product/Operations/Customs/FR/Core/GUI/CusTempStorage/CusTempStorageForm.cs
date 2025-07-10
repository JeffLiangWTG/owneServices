using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	public partial class CusTempStorageForm : ZTemplateForm
	{
		public CusTempStorageForm(CusTempStorageJobHeader header)
			: base(header)
		{
			FetchOriginalValues(header);
			InitializeComponent();
			InitializeComponentExtend(header);
			SetDataBinding(header, string.Empty);
			WorkflowTabPage.Initialize(header);
			AddMessagingMenu();
		}

		#region Form Caption
		public override string FormCaption
		{
			get
			{
				string caption = FormCaptionCore;
				var header = Header;
				if (!header.SJH_JobReference.IsEmpty && header.Customer != null && !header.Customer.OH_Code.IsEmpty && header.Branch != null && !header.Branch.GB_Code.IsEmpty)
				{
					var sb = new CargoWise.Types.ZStringBuilder();
					caption = sb.Append(FormCaptionCore).Append(header.SJH_JobReference).Append(header.Customer.OH_Code).Append(header.Branch.GB_Code).ToStringWithDelimiterBetweenAppends(" - ");
				}
				return Res.GetString("4F00F9F2-B08C-4616-A7DB-C1828F1E2514", "{0}", caption);
			}
		}
		protected string FormCaptionCore => (NoResString)"Declaration";
		#endregion

		protected override bool SupportsEDocs
		{
			get { return true; }
		}

		protected override bool ShowNotesTab
		{
			get { return true; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ForceUpdateReadOnly();
		}

		public void ForceUpdateReadOnly()
		{
			if (Header.HasInStoreEvent)
			{
				MainTabPage.UpdateEditableIncludingChildren(false);
			}
		}

		void AddMessagingMenu()
		{
			var messagingMenu = new CusTempStorageFormMenu() { Header = Header };
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(ActionsMenuItem), messagingMenu);
		}

		void FetchOriginalValues(CusTempStorageJobHeader header)
		{
			var localFactory = header.Factory.CreateNewFactory();

			originalHeader = localFactory.Load<CusTempStorageJobHeader>(header.PK);
		}

		CusTempStorageJobHeader Header => (CusTempStorageJobHeader)BusinessEntity;
		CusTempStorageJobHeader originalHeader;

		void InitializeComponentExtend(CusTempStorageJobHeader header)
		{
			userControlForPLugin = GetTemporyStorageUserControlForPlugin(header);
			this.userControlForPLugin.SuspendLayout();
			this.MainTabPage.Controls.Add(this.userControlForPLugin);
			// 
			// userControlForPLugin
			// 
			this.userControlForPLugin.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.userControlForPLugin, ".");
			this.userControlForPLugin.Dock = DockStyle.Fill;
			this.userControlForPLugin.Name = "userControlForPLugin";
			this.userControlForPLugin.TabIndex = 0;

			this.userControlForPLugin.ResumeLayout(true);
			this.userControlForPLugin.PerformLayout();
		}

		CINTemporyStorageUserControlForPlugin GetTemporyStorageUserControlForPlugin(CusTempStorageJobHeader jobHeader)
		{
			CINTemporyStorageUserControlForPlugin result;
			if (jobHeader.IsFRC)
			{
				result = new CINTemporyStorageUserControlForPlugin();
			}
			else if (jobHeader.IsIST)
			{
				result = new ISTTemporyStorageUserControlForPlugin();
			}
			else if (jobHeader.IsLADT)
			{
				result = new LADTTemporyStorageUserControlForPlugin();
			}
			else
			{
				result = new CINTemporyStorageUserControlForPlugin();
			}
			return result;
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			var messageSender = new FRCINImportMessageSender(Header, originalHeader, Messaging.MessageBuilders.CIN.CINImportMessageBuilder.MessageBuilderType.Correction);

			var (canSend, reason) = messageSender.CanSend();

			if (canSend)
			{
				messageSender.PrepareMessage();
			}

			base.Save(factories);

			if (canSend)
			{
				var (success, resultMessage) = messageSender.Send();

				Globals.Message.Show(resultMessage);

				if (!success)
				{
					messageSender.CancelMessageOnFailure();
				}
			}
			else if (!string.IsNullOrWhiteSpace(reason))
			{
				Globals.Message.Show(reason);
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && Header.ShowPreSaveDialogIfTempStorageEndDateUtcIsNotEmpty)
			{
				var message = Res.GetString("F5567587-A2D5-4501-8DAA-2E35F593157A", "End date cannot be amended once saved. Do you want to continue saving the temporary storage?");
				var caption = Res.GetString("76E72906-B54E-49A6-965F-0DDF0F8CAD0B", "Warning: End date cannot be amended");
				var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				result = dialogResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			return result;
		}
	}
}
