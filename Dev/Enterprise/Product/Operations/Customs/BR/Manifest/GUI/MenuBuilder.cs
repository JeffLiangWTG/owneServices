using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.BR.Manifest.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("c70d9cdc-b00f-42ca-ba5b-214ac4d10ddb", "BR Manifest");

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (Header.IsMercante && IsValidForMessage())
			{
				var caption = ResString.GetMultilingualString("85C2D3F7-F87C-4FF5-B577-057D11DA86D8", "Save Request Manifest");
				ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header), true);
			}
			else
			{
				var menuItem = GetInvalidMessageMenuItem();
				menuItems.Add(menuItem);
			}

			return menuItems.ToArray();
		}

		void CreateManifestLevelMessage(AsycudaManifestHeader header)
		{
			var fileName = FileNameFormat(header?.MasterBill?.ABL_BillNumber);
			var notificationMessageText = ZString.Empty;

			try
			{
				using (var dialog = CreateSaveFileDialog(fileName))
				{
					if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
					{
						var messageText = new BLMessageBuilder(new ManifestWrapper(header)).GetMessageCore();
						using (var writer = new StreamWriter(dialog.OpenFile()))
						{
							writer.Write(messageText);
						}

						var docManagerInfo = header.DocManagerInfo;
						if (docManagerInfo != null)
						{
							var eDocForXmlMessage = docManagerInfo.AddFileOrDocument(Encoding.Unicode.GetBytes(messageText), fileName + ".txt", Core.Constants.RefDocTypes.MiscellaneousDocument);
							eDocForXmlMessage.Description = Core.Constants.RefDocTypeDescriptions.MiscellaneousDocument;
						}

						header.Notes.AddNew(true, ResString.GetMultilingualString("2174A4C3-F270-4DDF-AC64-462BD9FD78C8", "Request file downloaded"), fileName);

						header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;

						if (mainForm.FireSaveButton() == ContinueWithSave.Yes)
						{
							notificationMessageText = ZString.Format((NoResString)"Request file {0} saved.", fileName);
						}
					}
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			if (!notificationMessageText.IsEmpty)
			{
				Globals.Message.Show(notificationMessageText);
			}
		}

		ZSaveFileDialog CreateSaveFileDialog(ZString fileName) => new ZSaveFileDialog
		{
			Filter = (NoResString)"TXT Files (*.txt)|*.txt|All files (*.*)|*.*",
			CheckPathExists = true,
			AddExtension = true,
			OverwritePrompt = true,
			RestoreDirectory = true,
			Title = (NoResString)"Save Mercante Request",
			FileName = fileName
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		ZString FileNameFormat(string bolNumber)
		{
			var fileName = new ZStringBuilder();
			fileName.Append("MERCANTE_");
			fileName.Append(bolNumber);
			fileName.Append("_");
			fileName.Append(DateTime.Now.ToString("yyyyMMdd"));

			return fileName.ToString();
		}
	}
}
