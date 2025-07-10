using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.Customs.ASYCUDA.Gui.Res;
using ResString = Enterprise.Customs.ASYCUDA.Gui.ResString;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public static class ResetMessageStatusHelper
	{
		public static ZMenuItem CreateResetMessageStatusMenuItem(ZGrid grid, System.Action resetMessageStatus_Click, System.Action resetMessageStatus_Popup)
		{
			var resetMessageStatus = new ZMenuItem(ResString.GetMultilingualString("ed47f199-f840-49f6-ab8e-39fe1cc93dd3", "Reset Message Status"));
			resetMessageStatus.Click += (s, e) => resetMessageStatus_Click();
			var index = grid.DeleteMenuItem.Index + 1;
			grid.ContextMenu.MenuItems.InsertRange(index, new[] { resetMessageStatus });
			grid.ContextMenu.Popup += (s, e) => resetMessageStatus_Popup();
			return resetMessageStatus;
		}

		public static ZMenuItem CreateResetMessageStatusMenuItem(Control control, System.Action resetMessageStatus_Click, System.Action resetMessageStatus_Popup)
		{
			var resetMessageStatus = new ZMenuItem(ResString.GetMultilingualString("9ad01b75-9bc7-4766-8509-2ad2e4dd5ef6", "Reset Message Status"));
			resetMessageStatus.Click += (s, e) => resetMessageStatus_Click();
			if (control.ContextMenu == null)
			{
				control.ContextMenu = new ContextMenu();
			}
			control.ContextMenu.MenuItems.InsertRange(0, new[] { resetMessageStatus });
			control.ContextMenu.Popup += (s, e) => resetMessageStatus_Popup();
			return resetMessageStatus;
		}

		public static void ShowResetMessageStatusDialog(ZString message, MessageParentProvider messageParentsToReset)
		{
			if (Env.Security.GlobalManifestResetMessageStatus.IsAllowed)
			{
				if (Globals.Message.Show(message, ResetMessageStatusText, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					foreach (var messageParent in messageParentsToReset())
					{
						messageParent.ResetMessageStatus();
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Env.Security.GlobalManifestResetMessageStatus.ErrorMessageForNotAllowed);
			}
		}

		public static string ResetMessageStatusText => Res.GetString("88C3CE29-861C-4759-AC31-DC46121C8203", "Reset Message Status");

		public delegate IEnumerable<IMessageParent> MessageParentProvider();

		public static string ResetMessageStatusMessage(AsycudaManifestHeader header, MessageLevel currentLevel, ZString referenceNumber)
		{
			var manifestMessageLevel = header.IsBillLevelManifestType
											? MessageLevel.Bill
											: header.IsPackedItemLevelManifestType
												? MessageLevel.Pack
												: MessageLevel.Manifest;
			ZString statusPart;

			if (currentLevel != manifestMessageLevel)
			{
				statusPart = Res.GetString("01C2A150-C863-4489-AD3C-366A9679B99C", "all {0} level message statuses", manifestMessageLevel);
				if (currentLevel == MessageLevel.Bill)
				{
					statusPart += Res.GetString("3B3B34BF-1C82-4773-8083-10C13B53A1EB", " against Bill {0}", referenceNumber);
				}
			}
			else
			{
				statusPart = Res.GetString("44549742-F373-48A5-8D6F-EB8A0B1C360F", "the message status for {0} {1}", manifestMessageLevel, referenceNumber);
			}

			return Res.GetString("820F0272-D0A9-44BA-9BC6-39F15E8C69FD", "Are you sure you want to reset {0} for {1} manifest type {2}?", statusPart, header.AMA_RN_NKCountry, header.AMA_ManifestType);
		}

		#region Manifest Level

		public static void ResetMessageStatus_PopupManifestLevel(AsycudaManifestHeader header, ZMenuItem resetMessageStatus, bool hideCompletely)
		{
			if (header != null)
			{
				var messageStatusProvider = header.MessageStatusProvider;
				bool visible = messageStatusProvider != null && AsycudaManifestUniversalMessagingHelper.ForMessageLevel(header,
									() => messageStatusProvider.MessageStatusCanBeReset(header),
									() => header.Bills.OfType<AsycudaBill>().Any(bill => messageStatusProvider.MessageStatusCanBeReset(bill)),
									() => header.IsOnePackedItemRelationship && header.Bills.OfType<AsycudaBill>().Any(bill => bill.Packs.OfType<AsycudaPack>().Any(pack => messageStatusProvider.MessageStatusCanBeReset(pack.PackedItem)))
								);

				if (hideCompletely)
				{
					resetMessageStatus.Visible = visible;
				}
				else
				{
					resetMessageStatus.Enabled = visible;
				}
			}
		}

		public static void ResetMessageStatus_ClickManifestLevel(AsycudaManifestHeader header)
		{
			if (header != null)
			{
				var messageStatusProvider = header.MessageStatusProvider;
				var message = ResetMessageStatusMessage(header, MessageLevel.Manifest, header.ManifestNumber);
				IEnumerable<IMessageParent> MessageParentsToReset()
				{
					if (messageStatusProvider == null)
					{
						return System.Array.Empty<IMessageParent>();
					}

					return AsycudaManifestUniversalMessagingHelper.ForMessageLevel(header,
						() => new IMessageParent[] { header }.Where(messageStatusProvider.MessageStatusCanBeReset),
						() => header.Bills.OfType<AsycudaBill>().Where(messageStatusProvider.MessageStatusCanBeReset),
						() => header.IsOnePackedItemRelationship ? header.Bills.OfType<AsycudaBill>().SelectMany(bill => bill.Packs.OfType<AsycudaPack>()).Select(pack => pack.PackedItem).Where(messageStatusProvider.MessageStatusCanBeReset) : Enumerable.Empty<AsycudaPackedItem>()
					);
				}
				ShowResetMessageStatusDialog(message, MessageParentsToReset);
			}
		}

		#endregion

		#region Bill Level

		public static void ResetMessageStatus_PopupBillLevel(AsycudaBill bill, ZMenuItem resetMessageStatus, bool hideCompletely)
		{
			if (bill != null)
			{
				var header = bill.Header;
				bool visible = false;

				if (header != null)
				{
					var messageStatusProvider = header.MessageStatusProvider;
					if (messageStatusProvider != null)
					{
						if (header.IsPackedItemLevelManifestType)
						{
							visible = bill.IsOnePackedItemRelationship && bill.Packs.OfType<AsycudaPack>().Any(pack => messageStatusProvider.MessageStatusCanBeReset(pack.PackedItem));
						}
						else if (header.IsBillLevelManifestType)
						{
							visible = messageStatusProvider.MessageStatusCanBeReset(bill);
						}
					}
				}

				if (hideCompletely)
				{
					resetMessageStatus.Visible = visible;
				}
				else
				{
					resetMessageStatus.Enabled = visible;
				}
			}
		}

		public static void ResetMessageStatus_ClickBillLevel(AsycudaBill bill)
		{
			if (bill != null)
			{
				var header = bill.Header;
				if (header != null)
				{
					var messageStatusProvider = header.MessageStatusProvider;
					var message = ResetMessageStatusMessage(header, MessageLevel.Bill, bill.ABL_BillNumber);

					IEnumerable<IMessageParent> MessageParentsToReset()
					{
						if (header.IsBillLevelManifestType)
						{
							return new IMessageParent[] { bill };
						}

						if (header.IsOnePackedItemRelationship && header.IsPackedItemLevelManifestType && messageStatusProvider != null)
						{
							return bill.Packs.OfType<AsycudaPack>()
								.Select(pack => pack.PackedItem)
								.Where(messageStatusProvider.MessageStatusCanBeReset);
						}

						return System.Array.Empty<IMessageParent>();
					}

					ShowResetMessageStatusDialog(message, MessageParentsToReset);
				}
			}
		}

		#endregion

		#region Pack Level

		public static void ResetMessageStatus_PopupPackLevel(AsycudaPackedItem packedItem, ZMenuItem resetMessageStatus, bool hideCompletely)
		{
			var visible = false;
			if (packedItem != null)
			{
				var header = packedItem.Header;
				visible = header != null
						&& header.IsPackedItemLevelManifestType
						&& (header.MessageStatusProvider?.MessageStatusCanBeReset(packedItem) ?? false);
			}

			if (hideCompletely)
			{
				resetMessageStatus.Visible = visible;
			}
			else
			{
				resetMessageStatus.Enabled = visible;
			}
		}

		public static void ResetMessageStatus_ClickPackLevel(AsycudaPackedItem packedItem)
		{
			if (packedItem != null)
			{
				var pack = packedItem.Pack;
				var header = packedItem.Header;

				if (header != null && pack != null && header.IsPackedItemLevelManifestType)
				{
					var message = ResetMessageStatusMessage(header, ResetMessageStatusHelper.MessageLevel.Pack, pack.CodeProperty);
					IEnumerable<IMessageParent> MessageParentsToReset() => new IMessageParent[] { packedItem };
					ShowResetMessageStatusDialog(message, MessageParentsToReset);
				}
			}
		}

		#endregion

		public enum MessageLevel
		{
			Manifest,
			Bill,
			Pack
		}
	}
}
