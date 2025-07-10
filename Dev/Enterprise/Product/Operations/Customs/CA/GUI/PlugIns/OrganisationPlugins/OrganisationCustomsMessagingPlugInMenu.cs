using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public class OrganisationCustomsMessagingPlugInMenu : KMenuItem
	{
		public OrganisationCustomsMessagingPlugInMenu(OrgHeaderTCPMessageWrapper organisation)
		{
			this.organisation = organisation;
			this.Text = Res.GetString("Customs.CA.OrganisationPlugIn", "Customs Messaging");
			fNotification = new MessageInstructionUserNotification();
			InitialiseMenu();
		}

		readonly OrgHeaderTCPMessageWrapper organisation;

		MenuItem exportTCPDataMenu;
		MenuItem sendCSATCPUpdatesMenu;

		void InitialiseMenu()
		{
			exportTCPDataMenu = new ZMenuItem(ResString.GetMultilingualString("Customs.CA.OrganisationPlugIn.ExportTCPDataMenu", "Export TCP Data"), new EventHandler(ExportTCPDataMenu_Click));
			sendCSATCPUpdatesMenu = new ZMenuItem(ResString.GetMultilingualString("Customs.CA.OrganisationPlugIn.CSATCPUpdates", "Send CSA TCP Updates"), new EventHandler(TradeChainPartnerUpdate_Click));
			this.MenuItems.Add(exportTCPDataMenu);
			this.MenuItems.Add(sendCSATCPUpdatesMenu);
		}

		void ExportTCPDataMenu_Click(object sender, EventArgs e)
		{
			if (SaveJob())
			{
				var fileName = GetFileNameForExport();

				var instructions = new ExportInstructions
				{
					BasePath = GetOutptutPathForExport(),
					FileExtension = FileExtensionType.Txt,
					SpecifiedFilename = fileName
				};

				var exporter = new CATCPLoadFlatFileDataExporter(new BusinessObjectFactory(), instructions, "");
				var collectionReader = new ArrayBusinessObjectReader(new OrgHeader[] { organisation.organisation }, typeof(OrgHeader));
				var buffer = new NotificationBuffer();
				exporter.Export(collectionReader, buffer);
				if (!buffer.HasErrors)
				{
					Globals.Message.ShowInformation(Res.GetString("C76CD8A8-2E0E-44E0-86BB-D7112D22516A", "TCP Export saved to file:\n{0}", instructions.BasePath + "\\" + fileName));
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("82570832-A03E-4361-ADE9-A0D143E655C2", "TCP Export failed:\n{0}", buffer.AsString));
				}
			}
		}

		protected ZString GetFileNameForExport()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}-TCPData-{1:yyyyMMddHHmm}.txt", organisation.organisation.OH_Code, ZDateTime.UtcNow);
		}

		protected virtual ZString GetOutptutPathForExport()
		{
			return System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
		}

		void TradeChainPartnerUpdate_Click(object sender, EventArgs e)
		{
			if (SaveJob())
			{
				ZFormModaliser.ShowDialogAndDispose(new TradeChainPartnerSendingMessageForm(new TradeChainPartnerMessageManager(organisation)));
			}
		}

		bool SaveJob()
		{
			return (!organisation.organisation?.HasChanges ?? false) ||
				(Notification.ShowConfirmation(Res.GetString("BFF91D33-1313-48D9-BFED-14ACA6596C8E", "The organization has not yet been saved. Do you want to save and proceed?"), Res.GetString("3D3CDDD3-A6B5-4AAC-877D-5A341B2289B6", "Save Organization")) && ((ZForm)GetMainMenu().GetForm()).FireSaveButton() == ContinueWithSave.Yes);
		}

		Customs.Business.MessageManagers.IUserNotification Notification
		{
			get { return fNotification; }
		}

		readonly Customs.Business.MessageManagers.IUserNotification fNotification;
	}
}
