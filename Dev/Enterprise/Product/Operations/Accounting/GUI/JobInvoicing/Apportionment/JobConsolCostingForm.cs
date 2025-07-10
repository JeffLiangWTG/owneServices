using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
#if DEBUG
	[TestExcludeZWinFormHasTypedConstructor()]
#endif
	public partial class JobConsolCostingForm : ZTemplateForm
	{
		readonly IBusiness ParentEntity;
		protected readonly IJobCostingPlugIn Consol;
		public JobConsolCostingForm(IBusiness businessEntity) : base(businessEntity)
		{
			Consol = businessEntity as IJobCostingPlugIn;
			if (Consol == null)
			{
				throw new NotSupportedException("Host entity must implement IJobCostingPlugin");
			}

			ParentEntity = businessEntity;
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.JobConsolCostingForm, null, requestedTabPageIndexDeterminer: () => -1, getBusinessEntityOverride: () => ParentEntity);
			var plugIn = PlugIns.GetPlugIn(ControllerIDs.JobConsolCostingForm) as JobConsolCostingPlugin;

			CaptionRenderingEnabled = true;
			RemoveTabPages();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void RemoveTabPages()
		{
			MainMenu.MenuItems.RemoveByKey(ActionsMenuItem.Name);
			MainTabControl.TabPages.Remove(MainTabPage);
			MainTabControl.TabPages.Remove(LogsTabPage);
		}

		protected override void SaveToRecentItems()
		{ }

		protected override void RemoveFromRecentItems()
		{ }

		#region GUI Setup

		public override string FormVerb => string.Empty;

		protected override bool AllowNew => false;

		public override string FormCaption => Res.GetString("7ac42167-7317-4ff4-84ce-cf8c2e271de8", "{0} Job Consol Costing", ParentEntity.HumanReadableName);

		#endregion

		new void InitializeComponent()
		{
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1065, 632, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(564, 6, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1065, 24, true);
			// 
			// JobConsolCostingForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1065, 688, true);
			this.Name = "JobConsolCostingForm";
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;
	}
}
