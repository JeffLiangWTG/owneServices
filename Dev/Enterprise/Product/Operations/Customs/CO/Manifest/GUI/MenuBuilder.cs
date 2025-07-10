using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.CO.Manifest.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CO.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("EB901B51-E805-4261-97E6-828B4580E834", "CO Manifest");

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (IsValidForMessage())
			{
				var messageStatusProvider = Header?.MessageStatusProvider;
				if (messageStatusProvider != null)
				{
					if (messageStatusProvider.AllowOriginalMessage(Header))
					{
						var caption = ResString.GetMultilingualString("EAF52CC6-A22F-4718-B5B1-702972569B81", "Save Request Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Original), true);
					}
					if (messageStatusProvider.AllowModificationMessage(Header))
					{
						var caption = ResString.GetMultilingualString("4CF8A539-FAB8-4C00-AD7A-AF6055E17A49", "Save Amend Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Change), true);
					}
				}
			}
			else
			{
				var menuItem = GetInvalidMessageMenuItem();
				menuItems.Add(menuItem);
			}

			return menuItems.ToArray();
		}

		void CreateManifestLevelMessage(AsycudaManifestHeader header, string messageSubType)
		{
			var shippingNumber = uint.TryParse(header.AMA_ManifestNumber, out var number) ? number : 0;
			var fileName = FileNameFormat(messageSubType, shippingNumber.ToString("00000000"));
			var notificationMessageText = ZString.Empty;

			try
			{
				using (var dialog = CreateSaveFileDialog(fileName))
				{
					if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
					{
						if (header.LockConsumeDocumentIDMutex())
						{
							var countOfIDNeeded = header.Bills.Count + 1; //Bill level + Header Level
							var validDocumentIDs = header.GetValidDocumentIDs(countOfIDNeeded);
							if (validDocumentIDs.Count < countOfIDNeeded)
							{
								notificationMessageText = Res.GetString("924943C2-DD3B-4EB1-A87A-8BB5168FD264", "There is not enough documents IDs to be used in the current file.");
								notificationMessageText += System.Environment.NewLine;
								notificationMessageText += Res.GetString("5FAB892C-AAA1-42EC-A1F9-9DD046BE94A5", "You should ask for a new set of IDs and load it on Maintain->Customs(CO)->Document IDs to the system before saving again.");
							}
							else
							{
								var messageSender = new SeaMessageSender(header, messageSubType, validDocumentIDs);
								var messageText = messageSender.CreateMessage();
								if (!messageText.IsEmpty)
								{
									using (var writer = new StreamWriter(dialog.OpenFile()))
									{
										writer.Write(messageText);
									}

									var docManagerInfo = header.Consol?.DocManagerInfo ?? header.DocManagerInfo;
									if (docManagerInfo != null)
									{
										var eDocForXmlMessage = docManagerInfo.AddFileOrDocument(Encoding.Unicode.GetBytes(messageText), fileName + ".xml", Core.Constants.RefDocTypes.MiscellaneousDocument);
										eDocForXmlMessage.Description = Core.Constants.RefDocTypeDescriptions.MiscellaneousDocument;
									}

									header.AMA_MessageStatus = ASYCUDA.Business.MessageStatusCodeList.Codes.Sent;
									validDocumentIDs.ForEach(id => { id.TN_IsUsed = true; });

									if (mainForm.FireSaveButton() == ContinueWithSave.Yes)
									{
										notificationMessageText = messageSubType == MessageSubTypeCodes.Codes.Original ? ZString.Format((NoResString)"Request file {0} saved.", fileName) : ZString.Format((NoResString)"Amend file {0} saved.", fileName);
									}
								}
							}
						}
						else
						{
							notificationMessageText = Res.GetString("5438B270-5EE7-42ED-AA50-EDEE2C0BB435", "{0} is in the process of sending messages, please try again later.", header.GetConsumeDocumentIDMutexInfo());
						}
					}
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			finally
			{
				header.UnlockConsumeDocumentIDMutex();
			}

			if (!notificationMessageText.IsEmpty)
			{
				Globals.Message.Show(notificationMessageText);
			}
		}

		ZSaveFileDialog CreateSaveFileDialog(ZString fileName) => new ZSaveFileDialog
		{
			Filter = (NoResString)"XML Files (*.xml)|*.xml|All files (*.*)|*.*",
			CheckPathExists = true,
			AddExtension = true,
			OverwritePrompt = true,
			RestoreDirectory = true,
			Title = (NoResString)"Save Manifest",
			FileName = fileName
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		ZString FileNameFormat(string messageSubType, string shippingNumber)
		{
			var action = messageSubType == MessageSubTypeCodes.Codes.Original ? "01" : "02";

			var fileName = new ZStringBuilder();
			fileName.Append("Dmuisca_");
			fileName.Append(action);
			fileName.Append("0116607");
			fileName.Append(DateTime.Now.Year.ToString());
			fileName.Append(shippingNumber);

			return fileName.ToString();
		}
	}
}
