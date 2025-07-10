using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("A0468492-E2A4-4F8D-BC57-D0DCC55BBA67", "MX Manifest");

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
						var caption = ResString.GetMultilingualString("E6C6F1F2-DE92-4CD4-B723-8CFBE979354F", "Send Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Original), true);
					}
					if (messageStatusProvider.AllowCancellationMessage(Header))
					{
						var caption = ResString.GetMultilingualString("69E7B4D6-31FF-4FAF-B3E6-44873DA9EC7C", "Cancel Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Cancellation), false);
					}
					if (messageStatusProvider.AllowModificationMessage(Header))
					{
						var caption = ResString.GetMultilingualString("8299C580-16CA-4294-814D-9AE645481481", "Amend Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Change), false);
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
			if (messageSubType == MessageSubTypeCodes.Codes.Original)
			{
				items = header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendOriginalMessage());
			}
			else if (messageSubType == MessageSubTypeCodes.Codes.Cancellation || messageSubType == MessageSubTypeCodes.Codes.Change)
			{
				items = Header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendCancellationMessage() || x.CanSendModificationMessage());
			}

			var messageChooser = header.GetNewMessageChooser(items, messageSubType, true);
			var isSeaManifest = header.AMA_TransportMode == Core.Constants.TransportModes.Sea;
			var reason = ZString.Empty;
			var messageType = ZString.Empty;
			if (isSeaManifest)
			{
				messageType = MessageTypes.Codes.MXA;
			}
			else
			{
				messageType = MessageTypes.Codes.MXE;
			}

			ISelectionItem[] selectedItems = null;
			using (var dlg = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetNewAsycudaItemSelectionDialog(messageChooser, ManifestCaption, messageType))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(dlg) == DialogResult.OK)
				{
					selectedItems = dlg.GetSelectedItems();

					if (dlg.BusinessEntity is MXMessageChooser chooser)
					{
						reason = chooser.Reason;
					}
				}
			}

			var message = ZString.Empty;
			if (selectedItems != null)
			{
				var selected = selectedItems.Cast<Business.AsycudaBill>().ToArray();
				foreach (var item in selected)
				{
					if (isSeaManifest)
					{
						message += new SEA309MessageSender(item, new SEA309Wrapper(item, messageSubType, reason), messageSubType).SendMessage();
					}
					else
					{
						message += new AWBMessageSender(item, new HouseWaybillWrapper(item, messageSubType), messageSubType).SendMessage();
					}
					message += Spaces + item.ABL_BillNumber + System.Environment.NewLine;
				}
				header.Messages.Reload(false);
			}

			if (message.Length > 0)
			{
				Globals.Message.Show(message);
			}
		}

		const string Spaces = "  ";
		static ZString ManifestCaption => Res.GetString("F079CD6E-DCCB-4131-B370-82725E828F58", "Manifest");
	}
}
