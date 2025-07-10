using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.OCR
{
	public partial class OCRResultForm : ZChildForm
	{
		public OCRResultForm()
		{
			InitializeComponent();
		}

		public string DisplayText
		{
			set { ResultTextBox.Text = value; }
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
