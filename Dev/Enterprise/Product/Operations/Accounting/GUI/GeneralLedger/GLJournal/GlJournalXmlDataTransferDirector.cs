using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GLJournals
{
	public class GlJournalXmlDataTransferDirector : DataTransfer.GLJournals.GlJournalXmlDataTransferDirector
	{
		public GlJournalXmlDataTransferDirector(GLJournalDataAdapter adapter, bool checkLicence)
			: base(adapter, checkLicence)
		{
		}

		protected override void PromptUserAndImportCore(BillingInterfaceName interfaceName)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Title = (NoResString)"Import Journal Xml"; // May be used as constant

				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					try
					{
						using (Stream xmlFileStream = dialog.OpenFile())
						{
							BusinessObjectFactory factory = new BusinessObjectFactory();
							ValueObjectImportContext context = new ValueObjectImportContext(factory, new NotificationBuffer());
							ImportJournalFromXml(xmlFileStream, context);
						}
					}
					catch (UnauthorizedAccessException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
					catch (IOException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
					catch (XmlException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
		}
	}
}
