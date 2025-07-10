using System;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	public partial class CustomMapListsForm : ZChildForm
	{
		public CustomMapListsForm()
		{
			InitializeComponent();
		}

		internal CustomMapListsForm(ImportExportWizard wizard)
			: base(wizard)
		{
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
