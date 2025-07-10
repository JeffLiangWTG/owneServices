using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ES.GUI
{
	public class AEODocumentMessageBoxProvider : IAEODocumentMessageBoxProvider
	{
		public AEODocumentMessageBoxProvider()
		{
		}

		public bool AskIfShouldRemoveAEODocument(ZString documentTypeForMessage)
		{
			return Globals.Message.Show(
				Res.GetString("DE64E052-0E36-43CA-970A-DB342072154C", @"Do you want to remove '{0}' documents from Inv.Headers?", documentTypeForMessage),
				Res.GetString("F8D96E05-14EF-4B1B-8014-DE479FCE9386", "AEO Documents"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				DialogResult.No) == DialogResult.Yes;
		}
	}
}
