using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.WCB.DaimlerChrysler.GUI
{
	public partial class DCImportDataEditorForm : ZChildForm, IButtonPostTextOverride
	{
		protected DCImportDataEditorForm()
		{
			InitializeComponent();
		}

		public DCImportDataEditorForm(JobDeclarationCollection jobDecs)
			: base(jobDecs)
		{
			InitializeComponent();
			MinimumSize = Size;
			ZFormPostingButtonsStrategy.SetupPosting(this, ImportButton, CancelButton);
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public override string FormCaption
		{
			get { return "Imported declarations"; }
		}

		string IButtonPostTextOverride.PostButtonText
		{
			get { return "&Import"; }
		}

		void ImportButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		protected override void ZForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//Do nothing
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			return ContinueWithSave.Yes;
		}
	}
}
