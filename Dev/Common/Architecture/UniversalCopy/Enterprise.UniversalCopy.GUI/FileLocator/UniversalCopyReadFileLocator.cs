using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UniversalCopy.GUI.FileLocator
{
	public class UniversalCopyReadFileLocator : BaseUniversalCopyFileLocator
	{
		public override IFileDialog CreateDialog()
		{
			var dialog = new ZOpenFileDialog();
			return dialog;
		}

		public override DialogResult ShowDialog(IFileDialog dialog)
		{
			return ((ZOpenFileDialog)dialog).ShowDialog();
		}
	}
}
