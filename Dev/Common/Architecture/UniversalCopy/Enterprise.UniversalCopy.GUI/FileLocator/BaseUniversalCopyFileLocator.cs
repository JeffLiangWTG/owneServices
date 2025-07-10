using System;
using System.IO;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UniversalCopy.GUI.FileLocator
{
	public abstract class BaseUniversalCopyFileLocator
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public virtual Stream GetFileStream(out string displayFileName)
		{
			displayFileName = null;
			using (var dialog = CreateDialog())
			{
				SetupDialog(dialog);

				if (ShowDialog(dialog) == DialogResult.OK)
				{
					displayFileName = dialog.UnmappedFileName;
					return dialog.OpenFile();
				}
			}
			return null;
		}

		public virtual void SetupDialog(IFileDialog dialog)
		{
			dialog.Filter = Res.GetString("7EC39C46-F7BF-40B2-8F96-2911C92F7C74", "UC Template Files") + (NoResString)" *.uctemplate|*.uctemplate";
			dialog.FileName = GetDefaultFileName();
		}

		public virtual string GetDefaultFileName()
		{
			return String.Empty;
		}

		public abstract IFileDialog CreateDialog();

		public abstract DialogResult ShowDialog(IFileDialog dialog);
	}
}
