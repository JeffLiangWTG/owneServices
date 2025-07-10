using System.IO;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZSaveFileDialog : ZFileDialog<SaveFileDialog>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:DoNotUseSystemWindowsFormsDialogs", Justification = "Baseline")]
		public ZSaveFileDialog()
			: base(new SaveFileDialog())
		{
		}

		public bool CreatePrompt
		{
			get => FileDialog.CreatePrompt;
			set => FileDialog.CreatePrompt = value;
		}

		public bool OverwritePrompt
		{
			get => FileDialog.OverwritePrompt;
			set => FileDialog.OverwritePrompt = value;
		}

		public static Stream OpenFile(string unmappedFileName) => new SaveFileStream(unmappedFileName);

		public override Stream OpenFile() => FileDialog.OpenFile();
	}
}
