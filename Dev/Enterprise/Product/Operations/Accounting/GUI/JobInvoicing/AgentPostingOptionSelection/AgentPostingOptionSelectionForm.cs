using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing.Posting.AgentPostingOptionSelection;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.JobInvoicing.AgentPostingOptionSelection
{
	public partial class AgentPostingOptionSelectionForm : ZArchitecture.GUI.ZChildForm
	{
		MasterFiles.GUI.ZOrganisationFindBox AgentBeingPostedFindBox;
		ZArchitecture.ZLabel PostingInstructionLabel;
		ZArchitecture.GUI.ZButton ContinueButton;
		ZArchitecture.ZCalcEdit zCalcEdit1;
		ZArchitecture.GUI.ZDropEdit CurrencySelectedDropEdit;
		ZArchitecture.GUI.ZButton CancelPostingButton;
		ZArchitecture.ZLabel zLabel2;
		ZArchitecture.ZLabel zLabel3;
		ZArchitecture.ZLabel zLabel4;
		ZArchitecture.GUI.ZGroupBox PostingOptionsGroupBox;
		ZArchitecture.GUI.ZPanel InstructionsPanel;
		ZArchitecture.ZLabel PostingConsolAgentsInvoiceLabel;
		ZArchitecture.GUI.ZDropEdit zDropEdit1;
		protected ZArchitecture.GUI.ZGuidDropEdit ContactDropEdit;
		protected ZArchitecture.GUI.ZGuidDropEdit AddressGuidDropEdit;
		readonly System.ComponentModel.IContainer components;

		public AgentPostingOptionSelectionForm(AgentPostingOptionSelector bizO) : base(bizO)
		{
			PostingConsolAgentsInvoiceLabel.Font = OFont.GetFontBold();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Dispose

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

		#endregion

		void ContinueButton_Click(object sender, System.EventArgs e)
		{
			var selector = BusinessEntity as AgentPostingOptionSelector;
			selector.RunPreSaveValidation();
			if (selector.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
			}
		}

		void CancelPostingButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}

