namespace Enterprise.ZArchitecture.GUI
{
	partial class TextTemplatePreviewForm : ZChildForm
	{
		public TextTemplatePreviewForm()
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return Res.GetString("213f815f-ce0b-49af-85fa-c55993cff574", "Template Preview"); }
		}

		public string PreviewText
		{
			set { textBox.Text = value; }
		}

		void closeButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
