using System;
using System.Globalization;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public partial class ISTTemporyStorageUserControlForPlugin : ZUserControl
	{
		public ISTTemporyStorageUserControlForPlugin()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (MainTabControl.SelectedTab == EntrySummaryDeclarationTabPage && EntrySummaryDeclarationTabControl.SelectedTab == DeclarationTabPage)
			{
				LoadUserControl();
			}
		}

		void DeclarationTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (EntrySummaryDeclarationTabControl.SelectedTab == DeclarationTabPage)
			{
				LoadUserControl();
			}
		}

		#region Display Controls & Messages
		public void LoadUserControl()
		{
			if (CurrentDataItem != null)
			{
				if (!CusTempStorageDecExists())
				{
					ErrorReporter.ReportOnce("The temporary storage declaration doesn't exist.",
						string.Format(CultureInfo.InvariantCulture, "The temporary storage declaration linked to header {0} doesn't exist.", CurrentDataItem.SJH_JobReference));
				}
				else
				{
					ShowCusTempStorageDecControl();
				}
			}
			else
			{
				ShowCoveringLabel(NotToCreateJobText);
			}
		}

		void ShowCoveringLabel(string text)
		{
			CoveringLabel.Text = text;
			CoveringLabel.Visible = true;
			DeclarationPanel.Visible = false;
		}

		void ShowCusTempStorageDecControl()
		{
			DeclarationPanel.Visible = true;
			CoveringLabel.Visible = false;
		}

		#region Messages
		string NotToCreateJobText => Res.GetString("ES|CusTempStorageDec|NotToCreateJobText", "You have chosen not to create a Declaration now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a Declaration.");
		#endregion

		#endregion

		#region CusTempStorageDec
		bool CusTempStorageDecExists()
		{
			return CusTempStorageDec.Load<CusTempStorageDec>(CurrentDataItem) != null;
		}
		#endregion

		public new CusTempStorageJobHeader CurrentDataItem => (CusTempStorageJobHeader)base.CurrentDataItem;

		protected MessagesUserControl GetTemporaryMessageUserControl() => new MessagesUserControl();

		protected ISTCusTempStorageDecUserControl GetTemporaryDeclarationUserControl() => new ISTCusTempStorageDecUserControl();

		protected TemporaryStorageHeaderUserControl GetTemporaryStorageEntrySummaryUserControl() => new TemporaryStorageHeaderUserControl();
	}
}
