using System.Windows.Forms;
using CargoWise.UniversalCopy;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UniversalCopy.GUI.FileLocator
{
	public class UniversalCopyWriteFileLocator : BaseUniversalCopyFileLocator
	{
		public CopyTemplateTree CopyTemplateTree { get; set; }

		public override IFileDialog CreateDialog()
		{
			return new ZSaveFileDialog();
		}

		public override DialogResult ShowDialog(IFileDialog dialog)
		{
			return ZFormModaliser.ShowCommonDialogWithoutDispose(dialog);
		}

		public override string GetDefaultFileName()
		{
			return CopyTemplateTree.ConfigurationName.Replace("&", "") + "_" + CopyTemplateTree.Name;
		}
	}
}
