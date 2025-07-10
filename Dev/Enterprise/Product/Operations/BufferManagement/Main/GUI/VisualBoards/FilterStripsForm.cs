using System;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class FilterStripsForm : ZChildForm
	{
		public FilterStripsForm(StmModuleFilterViewModel viewModel)
			: base(viewModel)
		{
			InitializeComponent();
			name = viewModel.Name;
		}

		readonly string name;

		public override string FormVerb => Res.GetString("D48AF566-7B64-4D35-B173-1C368F700A7E", "Edit \"{0}\"", name);

		void fApplyButton_Click(object sender, EventArgs e)
		{
			var shouldResumeSuspendValidation = BusinessEntity.Factory.IsValidationSuspended;
			if (shouldResumeSuspendValidation)
			{
				BusinessEntity.Factory.ResumeValidation();
			}

			BusinessEntity.RunPreSaveValidation();
			if (shouldResumeSuspendValidation)
			{
				BusinessEntity.Factory.SuspendValidation();
			}

			if (BusinessEntity.HasNotifications())
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			workflowFilterStrips.FilterStripAdding += (sender, e1) => workflowFilterStrips.RefreshStripControl();
			taskFilterStrips.FilterStripAdding += (sender, e1) => taskFilterStrips.RefreshStripControl();
		}
	}
}
