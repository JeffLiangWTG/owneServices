using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("C5387604-9F6F-47F1-A360-034AAAEF881B", "AR Manifest");

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
						var caption = ResString.GetMultilingualString("5E3FC9DF-4F52-4DBE-9082-A463EF21CFD0", "Send Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Original), true);
					}
					if (messageStatusProvider.AllowCancellationMessage(Header))
					{
						var caption = ResString.GetMultilingualString("84E33A9C-5B23-4474-B38E-8E81CD457FF6", "Cancel Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Cancellation), true);
					}
					if (messageStatusProvider.AllowModificationMessage(Header))
					{
						var caption = ResString.GetMultilingualString("AE71C3DB-1FA2-4D35-8DD8-AFCD914AC427", "Amend Manifest");
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

		void CreateManifestLevelMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType)
		{
			IEnumerable<ISelectionItem> items = null;
			var messageType = ZString.Empty;

			var isSeaManifest = header.AMA_TransportMode == Core.Constants.TransportModes.Sea;
			if (isSeaManifest)
			{
				messageType = MessageTypes.Codes.ARA;
			}
			else
			{
				messageType = MessageTypes.Codes.ARD;
			}
			if (messageSubType == MessageSubTypeCodes.Codes.Original)
			{
				items = header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendOriginalMessage());
			}
			else if (messageSubType == MessageSubTypeCodes.Codes.Cancellation)
			{
				items = header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendCancellationMessage());
			}
			else if (messageSubType == MessageSubTypeCodes.Codes.Change)
			{
				items = header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendModificationMessage());
			}

			ISelectionItem[] selectedItems = null;
			var messageChooser = header.GetNewMessageChooser(items, messageSubType, true);

			using (var dlg = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetNewAsycudaItemSelectionDialog(messageChooser, ManifestCaption, messageType))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(dlg) == DialogResult.OK)
				{
					selectedItems = dlg.GetSelectedItems();
				}
			}

			var message = new ZStringBuilder();

			if (selectedItems != null)
			{
				var selected = selectedItems.Cast<Business.AsycudaBill>().ToArray();
				foreach (var item in selected)
				{
					if (isSeaManifest)
					{
						if (messageSubType == MessageSubTypeCodes.Codes.Original)
						{
							message.Append(new BLOriginalMessageSender(item, new BLArgentinaWrapperManifest(item)).SendMessage());
						}
						else if (messageSubType == MessageSubTypeCodes.Codes.Cancellation)
						{
							message.Append(new BLCancelMessageSender(item, new BLCancellationArgentinaWrapper(item)).SendMessage());
						}
						else if (messageSubType == MessageSubTypeCodes.Codes.Change)
						{
							message.Append(new BLAmendMessageSender(item, new BLArgentinaWrapperManifest(item)).SendMessage());
						}
					}
					else
					{
						if (messageSubType == MessageSubTypeCodes.Codes.Original)
						{
							message.Append(new AWBOriginalMessageSender(item, new HouseWaybillWrapper(item, messageSubType)).SendMessage());
						}
						else if (messageSubType == MessageSubTypeCodes.Codes.Cancellation)
						{
							message.Append(new AWBCancelMessageSender(item, new HouseWaybillWrapper(item, messageSubType)).SendMessage());
						}
						else if (messageSubType == MessageSubTypeCodes.Codes.Change)
						{
							message.Append(new AWBAmendMessageSender(item, new HouseWaybillWrapper(item, messageSubType)).SendMessage());
						}
					}
					message.Append(Spaces + item.ABL_BillNumber + NewLine);
				}
				header.Messages.Reload(false);
			}

			if (message.Length > 0)
			{
				Globals.Message.Show(message.ToString());
			}
		}

		const string NewLine = "\r\n";
		const string Spaces = "  ";
		static ZString ManifestCaption => Res.GetString("07C65099-4DDE-4A10-8440-FD230851B631", "Manifest");
	}
}

