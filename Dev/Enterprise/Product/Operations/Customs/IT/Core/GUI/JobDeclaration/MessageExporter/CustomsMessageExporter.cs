using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public abstract class CustomsMessageExporter : ICustomsMessageExporter
{
	void ICustomsMessageExporter.SaveToFile(ITEDIMessage message)
	{
		Argument.NotNull(message, nameof(message));

		using (var fileDialog = new ZSaveFileDialog())
		{
			fileDialog.CheckPathExists = true;
			fileDialog.FileName = GetFileName(message);

			if (ZFormModaliser.ShowCommonDialogWithoutDispose(fileDialog) == DialogResult.OK)
			{
				var fileContent = GetFileContent(message);
				using (var outputFile = fileDialog.OpenFile())
				using (var writer = new StreamWriter(outputFile))
				{
					writer.Write(fileContent);
					writer.Flush();
				}
			}
		}
	}

	protected abstract ZString GetFileName(ITEDIMessage message);

	protected abstract ZString GetFileContent(ITEDIMessage message);
}
