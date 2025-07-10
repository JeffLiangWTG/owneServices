using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class EDIProjectForm : ProcessManagement.GUI.ProjectForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public EDIProjectForm()
		{
			InitializeComponent();
		}

		public EDIProjectForm(EDIProject project)
			: base(project)
		{
			InitializeComponent();
			PlugIns.Add(ClientControllerRegistration.ProjectClientOrgLicence);
			SetupControls();
			ProjectDetailsControl.SplitContainerMain.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(400);
		}

		void SetupControls()
		{
			ContactInformationUserControlType = typeof(EDIProjectContactControl);
			StateUserControlType = typeof(EDIProjectStatusControl);
		}

		protected override void InitializeRelatedItemsTabPage(ZTabPage relatedItemsTabPage)
		{
			var control = new EDIWorkTaskRelatedItemUserControl(IsViewOrDeleteMode);
			control.Dock = DockStyle.Fill;
			relatedItemsTabPage.Controls.Add(control);
		}

		new EDIProject Project
		{
			get { return (EDIProject)base.Project; }
		}

		protected override void SetupActionsMenuCore()
		{
			SetupActionsMenuItems();
			ActionsMenuItem.Popup += (object sender, EventArgs e) => { SetupActionsMenuItems(); };
		}

		void SetupActionsMenuItems()
		{
			if (Project.IsInstallationProject)
			{
				ActionsMenuItem.MenuItems.AddRange(InstallationProjectMenuItems);
			}
			else
			{
				foreach (MenuItem menuItem in InstallationProjectMenuItems)
				{
					ActionsMenuItem.MenuItems.Remove(menuItem);
				}
			}
			ActionsMenuItem.MenuItems.AddRange(StandardProjectMenuItems.ToArray());
		}

		protected override IEnumerable<MenuItem> ProjectActionsMenuItems
		{
			get
			{
				List<MenuItem> result = new List<MenuItem>();
				result.AddRange(base.ProjectActionsMenuItems);
				result.AddRange(InstallationProjectMenuItems);
				return result;
			}
		}

		MenuItem[] InstallationProjectMenuItems
		{
			get
			{
				if (installationProjectMenuItems == null)
				{
					MenuItem beginPreInstallMenuItem = new ZMenuItem("Begin &Pre-Install", BeginPreInstall_Click);
					beginPreInstallMenuItem.Shortcut = Shortcut.CtrlShiftP;
					beginPreInstallMenuItem.ShowShortcut = true;

					MenuItem completePreInstallMenuItem = new ZMenuItem("&Complete Pre-Install", CompletePreInstall_Click);
					completePreInstallMenuItem.Shortcut = Shortcut.CtrlShiftF;
					completePreInstallMenuItem.ShowShortcut = true;

					MenuItem completeInstallMenuItem = new ZMenuItem("Complete &Install", CompleteInstall_Click);
					completeInstallMenuItem.Shortcut = Shortcut.CtrlShiftI;
					completeInstallMenuItem.ShowShortcut = true;

					MenuItem beginTrainingMenuItem = new ZMenuItem("Begin &Training", BeginTraining_Click);
					beginTrainingMenuItem.Shortcut = Shortcut.CtrlShiftT;
					beginTrainingMenuItem.ShowShortcut = true;

					MenuItem completeTrainingMenuItem = new ZMenuItem("Complete T&raining", CompleteTraining_Click);
					completeTrainingMenuItem.Shortcut = Shortcut.CtrlShiftR;
					completeTrainingMenuItem.ShowShortcut = true;

					installationProjectMenuItems = new MenuItem[]
					{
						new ZMenuItem("-"),
						beginPreInstallMenuItem,
						completePreInstallMenuItem,
						new ZMenuItem("-"),
						completeInstallMenuItem,
						new ZMenuItem("-"),
						beginTrainingMenuItem,
						completeTrainingMenuItem,
						new ZMenuItem("-")
					};
				}
				return installationProjectMenuItems;
			}
		}

		MenuItem[] installationProjectMenuItems;

		void BeginPreInstall_Click(object sender, EventArgs e)
		{
			Project.BeginPreInstall();
		}

		void CompletePreInstall_Click(object sender, EventArgs e)
		{
			Project.CompletePreInstall();
		}

		void CompleteInstall_Click(object sender, EventArgs e)
		{
			Project.CompleteInstall();
		}

		void BeginTraining_Click(object sender, EventArgs e)
		{
			Project.BeginTraining();
		}

		void CompleteTraining_Click(object sender, EventArgs e)
		{
			Project.CompleteTraining();
		}

		#region For Testing
#if DEBUG

		public ZTabControl TopLevelTabControl_Exposed => TopLevelTabControl;

#endif
		#endregion
	}
}
