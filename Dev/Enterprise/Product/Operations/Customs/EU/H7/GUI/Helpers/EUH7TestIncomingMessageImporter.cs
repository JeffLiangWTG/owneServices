using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7TestIncomingMessageImporter
	{
		public EUH7TestIncomingMessageImporter(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		public void ImportEUICS2IncomingMessageFromXmlFile()
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Title = Res.GetString("7a5da4a4-26c9-432c-9319-df6f4bf37ec9", "Import EU ICS2 Incoming Message From XML File [DEV Only]");
				dialog.CheckFileExists = true;
				dialog.Filter = (NoResString)"XML File|*.xml";

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					try
					{
						var stream = dialog.OpenFile();

						EUH7IncomingMessageImporter.Import(stream, header);
						Globals.Message.ShowInformation($"Import succeed.");
					}
					catch (Exception ex)
					{
						Globals.Message.ShowError($"Import failed.\r\n{ex}");
					}
				}
			}
		}
	}
}
