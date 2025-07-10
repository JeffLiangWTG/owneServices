using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public class UPEAUBrokerageUserControl : AUBrokerageUserControl
	{
		public UPEAUBrokerageUserControl()
		{
			this.MainTabControl.Controls.Add(UPSTabPage);
		}

		public BaseDeclarationTabPage UPSTabPage
		{
			get
			{
				if (fUPSTabPage == null)
				{
					fUPSTabPage = new BaseDeclarationTabPage();
					fUPSTabPage.CheckForNotifications = true;
					fUPSTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23);
					fUPSTabPage.Name = "UPS";
					fUPSTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 629);
					fUPSTabPage.TabIndex = 1;
					fUPSTabPage.Text = "UPS";
				}
				return fUPSTabPage;
			}
		}
		BaseDeclarationTabPage fUPSTabPage;

		protected override void InitializeLazyCreate()
		{
			this.UPSTabPage.LazyCreateControls += new EventHandler(this.LazyCreateControlsFired);
			base.InitializeLazyCreate();
		}

		protected override void LazyCreateControlsFired(object sender, EventArgs e)
		{
			if (sender == UPSTabPage)
			{
				if (UPSTabPage.Controls.Count == 0 && fUPSUserControl == null)
				{
					fUPSUserControl = new UPSUserControl();
					fUPSUserControl.JobDeclaration = JobDeclaration;
					fUPSUserControl.Dock = DockStyle.Fill;
					UPSTabPage.Controls.Add(fUPSUserControl);
				}
			}
			else
			{
				base.LazyCreateControlsFired(sender, e);
			}
		}
		UPSUserControl fUPSUserControl;

		#region Test
		[Browsable(false)]
		internal UPSUserControl UPSUserControlForTest
		{
			get { return fUPSUserControl; }
		}
		#endregion
	}
}
