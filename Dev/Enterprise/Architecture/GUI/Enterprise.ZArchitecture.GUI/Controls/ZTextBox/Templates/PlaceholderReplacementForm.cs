using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class PlaceholderReplacementForm : ZChildForm
	{
		public PlaceholderReplacementForm(ExpressionNoteTemplate template)
			: base(template)
		{
			InitializeComponent();
		}

		void DoneButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
