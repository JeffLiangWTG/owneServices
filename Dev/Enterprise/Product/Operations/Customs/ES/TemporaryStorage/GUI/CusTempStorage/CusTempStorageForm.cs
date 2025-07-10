using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class CusTempStorageForm : ZTemplateForm
	{
		public CusTempStorageForm(CusTempStorageJobHeader header)
			: base(header)
		{
			InitializeComponent();
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
				return Res.GetString("2297C48A-76E1-4F7C-9140-AB29E5D4E8BF", "{0}", caption);
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

		void AddMessagingMenu()
		{
			var messagingMenu = new CusTempStorageFormMenu() { Header = Header };
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(ActionsMenuItem), messagingMenu);
		}

		CusTempStorageJobHeader Header => (CusTempStorageJobHeader)BusinessEntity;

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && Header.ShowPreSaveDialogIfTempStorageEndDateUtcIsNotEmpty)
			{
				var message = Res.GetString("ECB5133F-2435-4EE7-96FC-6FF6E0AFEC3A", "End date cannot be amended once saved. Do you want to continue saving the temporary storage?");
				var caption = Res.GetString("5ACFA5F4-D5DD-42B3-9C7A-89E8EC9D01A6", "Warning: End date cannot be amended");
				var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				result = dialogResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			return result;
		}
	}
}
