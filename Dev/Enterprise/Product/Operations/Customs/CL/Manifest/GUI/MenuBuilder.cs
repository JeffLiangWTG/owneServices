using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CL.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("612C148B-4308-4483-A5DF-4A6582C546A1", "CL Manifest");

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
						var caption = ResString.GetMultilingualString("762F1738-09ED-40E5-AA2F-7F5675DCF296", "Send Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Original), true);
					}
					if (messageStatusProvider.AllowCancellationMessage(Header))
					{
						var caption = ResString.GetMultilingualString("A0FDF40C-865E-4D7E-B507-71817513B4E3", "Cancel Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Cancellation), false);
					}
					if (messageStatusProvider.AllowModificationMessage(Header))
					{
						var caption = ResString.GetMultilingualString("41D8E633-0B78-431C-BD97-44FE8BD37703", "Amend Manifest");
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
			var messageType = ZString.Empty;
			var isSeaManifest = header.AMA_TransportMode == Core.Constants.TransportModes.Sea;

			if (isSeaManifest)
			{
				if (messageSubType == MessageSubTypeCodes.Codes.Original)
				{
					items = header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendOriginalMessage);
					messageType = MessageTypes.Codes.CHB;
				}
				else if (messageSubType == MessageSubTypeCodes.Codes.Cancellation)
				{
					items = header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendCancellationMessage());
					messageType = MessageTypes.Codes.CHC;
				}
				else if (messageSubType == MessageSubTypeCodes.Codes.Change)
				{
					items = header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendModificationMessage());
					messageType = MessageTypes.Codes.CHA;
				}
			}
			else
			{
				if (messageSubType == MessageSubTypeCodes.Codes.Original)
				{
					items = header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendOriginalMessage);
					messageType = MessageTypes.Codes.CHE;
				}
				else if (messageSubType == MessageSubTypeCodes.Codes.Cancellation)
				{
					items = header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendCancellationMessage());
					messageType = MessageTypes.Codes.CHF;
				}
				else if (messageSubType == MessageSubTypeCodes.Codes.Change)
				{
					items = header.Bills.Cast<Business.AsycudaBill>().Where(x => x.CanSendModificationMessage());
					messageType = MessageTypes.Codes.CHD;
				}
			}

			ISelectionItem[] selectedItems = null;
			var observation = ZString.Empty;
			var amendReason = ZString.Empty;
			var amendType = ZString.Empty;

			var messageChooser = header.GetNewMessageChooser(items, messageSubType, true);

			using (var dlg = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetNewAsycudaItemSelectionDialog(messageChooser, ManifestCaption, messageType))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(dlg) == DialogResult.OK)
				{
					selectedItems = dlg.GetSelectedItems();

					if (dlg.BusinessEntity is CLMessageChooser chooser)
					{
						observation = chooser.Reason;

						if (messageSubType == MessageSubTypeCodes.Codes.Change)
						{
							amendReason = chooser.AmendReason;
							amendType = chooser.AmendType;
						}
					}
				}
			}

			var message = new ZStringBuilder();

			if (selectedItems != null)
			{
				var actionType = string.Empty;
				var selected = selectedItems.Cast<Business.AsycudaBill>().ToArray();
				foreach (var item in selected)
				{
					if (messageSubType == MessageSubTypeCodes.Codes.Original)
					{
						actionType = WrappersConstants.ActionType.I;

						if (isSeaManifest)
						{
							message.Append(new BLOriginalMessageSender(item, new BLSendChileWrapper(item, actionType)).SendMessage());
						}
						else
						{
							message.Append(new AWBOriginalMessageSender(item, new AWBSendChileWrapper(item, WrappersConstants.ActionType.I, WrappersConstants.ObservationName.Gral)).SendMessage());
						}
					}
					else if (messageSubType == MessageSubTypeCodes.Codes.Cancellation)
					{
						if (isSeaManifest)
						{
							message.Append(new BLCancelMessageSender(item, new BLCancelChileWrapper(item, observation)).SendMessage());
						}
						else
						{
							message.Append(new AWBCancelMessageSender(item, new AWBCancelChileWrapper(item, observation)).SendMessage());
						}
					}
					else if (messageSubType == MessageSubTypeCodes.Codes.Change)
					{
						if (isSeaManifest)
						{
							message.Append(new BLAmendMessageSender(item, new BLSendChileWrapper(item, amendType, amendReason, observation)).SendMessage());
						}
						else
						{
							message.Append(new AWBAmendMessageSender(item, new AWBSendChileWrapper(item, amendType, amendReason, observation)).SendMessage());
						}
					}
					message.Append(Spaces + item.ABL_BillNumber + System.Environment.NewLine);
				}
				header.Messages.Reload(false);
			}

			if (message.Length > 0)
			{
				Globals.Message.Show(message.ToString());
			}
		}

		const string Spaces = "  ";
		static ZString ManifestCaption => Res.GetString("E92548ED-B880-4B5F-A0C2-6D3FFD9F97E5", "Manifest");
	}
}
