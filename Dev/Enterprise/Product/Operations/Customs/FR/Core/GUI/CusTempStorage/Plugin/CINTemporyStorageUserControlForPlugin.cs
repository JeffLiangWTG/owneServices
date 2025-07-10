using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	public partial class CINTemporyStorageUserControlForPlugin : ZUserControl
	{
		public CINTemporyStorageUserControlForPlugin()
		{
			InitializeComponent();
			InitializeComponentExtend();
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

		void InitializeComponentExtend()
		{
			//
			//MessagesUserControl
			//
			MessagesUserControl = GetTemporaryMessageUserControl();
			this.BindingSource.SetBindingMember(this.MessagesUserControl, "CusTempStorageDecs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((CusTempStorageJobHeader)(null)).CusTempStorageDecs)));
			MessagesUserControl.AllowDrop = true;
			MessagesUserControl.Dock = DockStyle.Fill;
			MessagesUserControl.Name = "MessagesUserControl";
			DeclarationMessagesTabPage.Controls.Add(this.MessagesUserControl);
			//
			//TemporaryStorageHeaderUserControl
			//
			TemporaryStorageHeaderUserControl = GetTemporaryStorageEntrySummaryUserControl();
			TemporaryStorageHeaderUserControl.AllowDrop = true;
			BindingSource.SetBindingMember(this.TemporaryStorageHeaderUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			TemporaryStorageHeaderUserControl.Dock = DockStyle.Fill;
			TemporaryStorageHeaderUserControl.Name = "TemporaryStorageHeaderUserControl";
			MainTabPage.Controls.Add(TemporaryStorageHeaderUserControl);
			// 
			// CusDecTabPageUserControl
			//
			this.CusDecTabPageUserControl = tempStorageDecTabPageUserControl;
			BindingSource.SetBindingMember(this.CusDecTabPageUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			this.CusDecTabPageUserControl.SuspendLayout();
			this.DeclarationPanel.Controls.Add(this.CusDecTabPageUserControl);
			this.CusDecTabPageUserControl.AllowDrop = true;
			this.CusDecTabPageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusDecTabPageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CusDecTabPageUserControl.Name = "CusDecTabPageUserControl";
			this.CusDecTabPageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1079, 542, true);
			this.CusDecTabPageUserControl.TabIndex = 0;
			this.CusDecTabPageUserControl.ResumeLayout(true);
			this.CusDecTabPageUserControl.PerformLayout();
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
		string NotToCreateJobText => Res.GetString("FR|CusTempStorageDec|NotToCreateJobText", "You have chosen not to create a Declaration now.\r\nPlease change to another tab. You can click back to this tab when/if you need to create a Declaration.");
		#endregion

		#endregion

		#region CusTempStorageDec
		bool CusTempStorageDecExists()
		{
			return EU.Business.CusTempStorage.CusTempStorageDec.Load<CusTempStorageDec>(CurrentDataItem) != null;
		}
		#endregion

		public new CusTempStorageJobHeader CurrentDataItem => (CusTempStorageJobHeader)base.CurrentDataItem;

		protected virtual MessagesUserControl GetTemporaryMessageUserControl() => new MessagesUserControl();

		#region CusTempStorageDecTabPageUserControl
		CusTempStorageDecUserControl fTempStorageDecTabPageUserControl;

		protected CusTempStorageDecUserControl tempStorageDecTabPageUserControl => fTempStorageDecTabPageUserControl ?? (fTempStorageDecTabPageUserControl = GetTemporaryDeclarationUserControl());

		protected virtual CusTempStorageDecUserControl GetTemporaryDeclarationUserControl() => new CusTempStorageDecUserControl();
		#endregion

		protected virtual TemporaryStorageHeaderUserControl GetTemporaryStorageEntrySummaryUserControl() => new TemporaryStorageHeaderUserControl();
	}
}
