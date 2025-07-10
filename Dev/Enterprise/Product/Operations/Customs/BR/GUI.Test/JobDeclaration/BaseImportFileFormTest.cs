using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(BaseImportFileForm))]
	public class BaseImportFileFormTest : ZFormBasherTest
	{
		public void TestBaseImportFileFormFields()
		{
			using (var control = new BaseImportFileForm())
			{
				AssertType<ZOpenFileDialog>("OpenFileDialog must be ZOpenFileDialog", control.OpenFileDialog);
				AssertType<ZTextBox>("FileNameTextBox must be ZTextBox", control.FileNameTextBox);
				AssertType<ZButton>("BrowseButton must be ZButton", control.BrowseButton);
				AssertType<ZButton>("ImportButton must be ZButton", control.ImportButton);
				AssertType<ZButton>("CloseButton must be ZButton", control.CloseButton);
				AssertType<ZGroupBox>("FileContentGroupBox must be ZGroupBox", control.FileContentGroupBox);
				AssertType<ZGroupBox>("LogDetailsGroupBox must be ZGroupBox", control.LogDetailsGroupBox);
				AssertType<ZListBox>("LogDetailsListBox must be ZListBox", control.LogDetailsListBox);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new BaseImportFileForm();
		}

		#endregion
	}
}
