using System.ComponentModel;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class OverwriteOrCreateNewForm : ZChildForm
	{
		public OverwriteOrCreateNewForm(string displayText)
		{
			InitializeComponent();

			DisposableLeakListener.Instance.RegisterDisposable(this);
			NameLabel.Text = displayText;
#if DEBUG
			TypeDescriptor.AddAttributes(NameLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public override string FormVerb
		{
			get { return ZString.Empty; }
		}

		public FileAction FileActionResult
		{
			get { return fFileActionResult; }
		}

		FileAction fFileActionResult;

		void OverwriteButton_Click(object sender, System.EventArgs e)
		{
			fFileActionResult = FileAction.Overwrite;
			Close();
		}

		void CreateNewButton_Click(object sender, System.EventArgs e)
		{
			fFileActionResult = FileAction.CreateNew;
			Close();
		}

		void CancelNewButton_Click(object sender, System.EventArgs e)
		{
			fFileActionResult = FileAction.None;
			Close();
		}
	}
}
