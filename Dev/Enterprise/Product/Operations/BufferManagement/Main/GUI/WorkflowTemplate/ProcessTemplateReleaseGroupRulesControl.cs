using System;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ProcessTemplateReleaseGroupRulesControl : ZUserControl
	{
		public ProcessTemplateReleaseGroupRulesControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ApplicableWorkflowCategoriesGrid.ReadOnly = true;
			SetCategoriesGridReadOnly();

			if (RulesListManager != null)
			{
				RulesListManager.ListChanged += ListManager_ListChanged;
				RulesListManager.CurrentChanged += ListManager_CurrentChanged;
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			SetCategoriesGridReadOnly();
		}

		void ListManager_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
		{
			SetCategoriesGridReadOnly();
		}

		void SetCategoriesGridReadOnly()
		{
			if (RulesListManager != null)
			{
				var rule = RulesListManager.GetCurrent() as ProcessTemplateReleaseGroupRule;
				if (rule != null)
				{
					ApplicableWorkflowCategoriesGrid.ReadOnly = rule.PTR_AreAllWorkflowCategoriesApplicable;
				}
			}
		}

		CurrencyManager RulesListManager => RulesGrid.ListManager;
	}
}
