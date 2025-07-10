using System;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowProvidersLinkageUserControl : ZUserControl, IPostingButtonsProvider, IDialogDefaultControlMembers
	{
		public WorkflowProvidersLinkageUserControl()
		{
			InitializeComponent();
		}

		public WorkflowProvidersLinkageUserControl(WorkflowProvidersLinkViewModel viewModel)
		{
			InitializeComponent();
			SetDataBinding(viewModel, string.Empty);

			ZFormPostingButtonsStrategy.SetupPosting(this, CreateLinkButton, CancelLinkButton);

			CreateLinkButton.ToolTipCaption = ResString.GetMultilingualString("5952e4ec-8947-45cb-9379-40523b1c5219", "Commit the Workflow links shown above.");
			CancelLinkButton.ToolTipCaption = ResString.GetMultilingualString("ae779ce4-4882-47fc-b243-f1898b895e6b", "Discard the links shown above. The jobs will still be attached but workflows between jobs will not be linked.");
		}

		public WorkflowProvidersLinkViewModel ViewModel
		{
			get { return (WorkflowProvidersLinkViewModel)base.DataSource; }
		}

		#region Event handlers

		void CreateLinkButton_Click(object sender, EventArgs e)
		{
			ValidateAndShowErrorsIfAny();

			if (!ViewModel.HasErrors)
			{
				DialogResult = DialogResult.OK;
			}
		}

		void CancelLinkButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		public DialogResult DialogResult
		{
			get
			{
				return dialogResult;
			}
			set
			{
				dialogResult = value;

				var form = FindForm();

				if (form != null)
				{
					form.DialogResult = value;
				}
			}
		}

		DialogResult dialogResult;

		#endregion

		#region IPostingButtonsProvider Members

		bool IPostingButtonsProvider.AllowNew
		{
			get { return false; }
		}

		void IPostingButtonsProvider.AssignButtonsInternal(IButton saveAndCloseButtonControl, IButton cancelButtonControl, IButton saveButtonControl)
		{
			// Not sure what this method is for...
		}

		IButton IPostingButtonsProvider.CommandButtonApply
		{
			get { return CreateLinkButton; }
		}

		IButton IPostingButtonsProvider.CommandButtonCancel
		{
			get { return CancelLinkButton; }
		}

		IButton IPostingButtonsProvider.CommandButtonPost
		{
			get { return CreateLinkButton; }
		}

		bool IPostingButtonsProvider.IsPostOnly { get; set; }
		bool IPostingButtonsProvider.SetupPostingCalled { get; set; }

		#endregion

		#region IDialogDefaultControlMembers Members

		public void SetReadOnly(bool readOnly, ZDialogResult allowedResult)
		{
			LinksGrid.ReadOnly = readOnly;
			var isCreateLinksButton = allowedResult == (ZDialogResult)CreateLinkButton.DialogResult;
			CreateLinkButton.Enabled = !readOnly || isCreateLinksButton;
			CancelLinkButton.Enabled = !readOnly || !isCreateLinksButton;
		}

		#endregion
	}
}
