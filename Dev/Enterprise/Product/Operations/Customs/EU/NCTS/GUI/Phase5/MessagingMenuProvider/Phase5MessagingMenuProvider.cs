using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Customs.GUI.WarehouseExtensions;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class Phase5MessagingMenuProvider : NctsMessagingMenuProvider
	{
		public static Phase5MessagingMenuProvider GetProvider(NctsHeader header)
		{
			var messagingMenuProviders = ObjectFactory.Get<Hashtable>("NCTSMessagingMenuProviders");
			var messagingMenuProviderHandle = (ObjectHandle)messagingMenuProviders[header.CountryCode.ToString()];
			return (Phase5MessagingMenuProvider)messagingMenuProviderHandle?.GetObject(header) ?? new Phase5MessagingMenuProvider(header);
		}

		public Phase5MessagingMenuProvider(NctsHeader header) : base(header)
		{
		}

		public Phase5MessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper) : base(header, ntcsHeaderUniversalMessagingHelper)
		{
		}

		public override IEnumerable<ZMenuItem> CreateMenuItems()
		{
			foreach (var menuItem in CreateMenuItemsCore())
			{
				yield return menuItem;
			}
			yield return SpacerMenuItem;
			yield return InventoryManagementMenuItem;
			yield return TSRegisterManagementMenuItem;
			yield return InventoryManagementSpacerMenuItem;
			yield return ImportEntryLinesMenuItem;
			yield return ImportInvoiceLinesMenuItem;
			yield return CopyPreviousGoodsItemMenuItem;
			yield return LockCustomsDeclarationMenuItem;
			yield return UnlockCustomsDeclarationMenuItem;
		}

		protected virtual IEnumerable<ZMenuItem> CreateMenuItemsCore()
		{
			yield return SendToCustomsMenuItem;
			yield return MakeArrivalNotificationMenuItem;
		}

		protected virtual SendsMessagesToCustomsGUI GetNewMessageInitiator()
		{
			return new SendsMessagesToCustomsGUI();
		}

		protected ZMenuItem SendToCustomsMenuItem => sendToCustomsMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("7E69F43B-4DB7-473A-B1F2-D856338B54DB", "Send to Customs"), SendToCustomsClick);
		ZMenuItem sendToCustomsMenuItem;

		ZMenuItem ImportEntryLinesMenuItem => importEntryLinesMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("8C57AF44-3CAF-4CA8-BA68-A8CD2F1A0D66", "Import Entry Lines"), ImportEntryLinesClick);
		ZMenuItem importEntryLinesMenuItem;

		ZMenuItem ImportInvoiceLinesMenuItem => importInvoiceLinesMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("4B9C1DCA-4A8A-4920-B269-7611D90F6626", "Import Invoice Lines"), ImportInvoiceLinesClick);
		ZMenuItem importInvoiceLinesMenuItem;

		ZMenuItem InventoryManagementMenuItem => inventoryManagementMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("fee55d27-a13e-410f-aefd-79632af849f2", "Inventory Management"));
		ZMenuItem inventoryManagementMenuItem;

		ZMenuItem InventoryManagementSpacerMenuItem => inventoryManagementSpacerMenuItem ??= CreateSpacerMenuItem();
		ZMenuItem inventoryManagementSpacerMenuItem;

		ZMenuItem MakeArrivalNotificationMenuItem => makeArrivalNotificationMenuItem ??= CreateNewMakeArrivalNotificationMenuItem();
		ZMenuItem makeArrivalNotificationMenuItem;

		ZMenuItem TSRegisterManagementMenuItem => tsRegisterManagementMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("a6d08853-b654-4253-8fd1-98b4b84bdf9b", "TS Register Management"));
		ZMenuItem tsRegisterManagementMenuItem;

		protected virtual ZMenuItem CreateNewMakeArrivalNotificationMenuItem()
			=> new ZMenuItem(ResString.GetMultilingualString("B4389825-B5E8-4A37-A134-817D19780309", "Make Arrival Notification for this Departure"), MakeArrivalNotificationClick);

		ZMenuItem SpacerMenuItem => spacerMenuItem ??= CreateSpacerMenuItem();
		ZMenuItem spacerMenuItem;

		protected ZMenuItem CreateSpacerMenuItem() => new ZMenuItem(ZMenuItem.Separator);

		ZMenuItem LockCustomsDeclarationMenuItem => lockCustomsDeclarationMenuItem ??= CreateNewLockCustomsDeclarationMenuItem();
		ZMenuItem lockCustomsDeclarationMenuItem;

		protected virtual ZMenuItem CreateNewLockCustomsDeclarationMenuItem()
		{
			var menuItem = new ZMenuItem(LockCustomsFileMenuItemCaption, CustomsDeclarationClick);
			menuItem.Tag = Events.LockForEditCode;
			return menuItem;
		}

		ZMenuItem UnlockCustomsDeclarationMenuItem => unlockCustomsDeclarationMenuItem ?? (unlockCustomsDeclarationMenuItem = CreateNewUnlockCustomsDeclarationMenuItem());
		ZMenuItem unlockCustomsDeclarationMenuItem;

		protected virtual ZMenuItem CreateNewUnlockCustomsDeclarationMenuItem()
		{
			var menuItem = new ZMenuItem(UnlockCustomsFileMenuItemCaption, CustomsDeclarationClick);
			menuItem.Tag = Events.UnlockForEditCode;
			return menuItem;
		}

		ZMenuItem CopyPreviousGoodsItemMenuItem => copyPreviousGoodsItemMenuItem ??= new ZMenuItem(ResString.GetMultilingualString("BD422E73-748E-4620-85FA-0887A12CF792", "&Copy Previous Goods Item"), CopyPreviousGoodsItem_Click);
		ZMenuItem copyPreviousGoodsItemMenuItem;

		void CopyPreviousGoodsItem_Click(object sender, EventArgs e)
		{
			copyPreviousGoodsItemMenuItem.Checked = !copyPreviousGoodsItemMenuItem.Checked;
			Header.Bills.ForEach(x => x.GoodsItems.CopyLastGoodsItemToNewLines = copyPreviousGoodsItemMenuItem.Checked);
		}

		protected override NctsHeader Header
		{
			get => base.Header;
			set
			{
				if (Header != value)
				{
					var oldValue = Header;
					base.Header = value;
					NctsHeaderChanged(oldValue, value);
				}
			}
		}

		void NctsHeaderChanged(NctsHeader oldValue, NctsHeader newValue)
		{
			CopyPreviousGoodsItemMenuItem.Checked = newValue.Bills.Count > 0 ? newValue.Bills.First().B0_CopyLastGoodsLineToNewLines : CustomsDataRegistry.Instance.AlwaysCopyFromPreviousLine.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		void CustomsDeclarationClick<T>(T sender, EventArgs e)
		{
			var menuItem = sender as ZMenuItem;

			using (var frm = new CustomsWriteToLogForm(Header, new BusinessObject[] { Header }, menuItem.Caption, (BusinessObject[] businessObjects, ZString reference) =>
			{ WriteLockLog(businessObjects, reference, menuItem.Tag.ToString() == Events.LockForEditCode); }))
			{
				ZFormModaliser.ShowDialogWithoutDispose(frm);
			}
		}

		protected void WriteLockLog(BusinessObject[] businessObjects, ZString reference, bool isLock)
		{
			foreach (var businessObject in businessObjects)
			{
				var fileParent = businessObject as ICustomsFileParent;
				if (isLock)
				{
					fileParent?.LockFile(reference);
				}
				else
				{
					fileParent?.UnlockFile(reference);
				}
			}

			RefreshMenu();
		}

		static MultilingualString LockCustomsFileMenuItemCaption => ResString.GetMultilingualString("21F994BE-EB35-4000-BBE9-5C72718CC796", "Lock Customs Declaration");

		static MultilingualString UnlockCustomsFileMenuItemCaption => ResString.GetMultilingualString("8152CBAF-15C4-4456-A2BB-4C9FFE208D5E", "Unlock Customs Declaration");

		public override void RefreshMenu()
		{
			SetMenuItemVisibility(SendToCustomsMenuItem, () => CanSendToCustoms);
			SetMenuItemVisibility(MakeArrivalNotificationMenuItem, () => CanMakeArrivalNotification);
			SetMenuItemVisibility(LockCustomsDeclarationMenuItem, () => IsLockUnlockCustomsDeclarationAvailable && !Header.IsLocked);
			SetMenuItemVisibility(UnlockCustomsDeclarationMenuItem, () => IsLockUnlockCustomsDeclarationAvailable && Header.IsLocked);
			SetMenuItemVisibility(InventoryManagementMenuItem, () => Header.Configuration.IsImportBondedWarehouseOrderAvailable && BillCount > 0);
			SetMenuItemVisibility(TSRegisterManagementMenuItem, () => CanTSRegisterManagement);
			SetMenuItemVisibility(InventoryManagementSpacerMenuItem, () => InventoryManagementMenuItem.Visible || TSRegisterManagementMenuItem.Visible);
			SetMenuItemVisibility(ImportInvoiceLinesMenuItem, () => CanImportInvoiceLines);
			SetMenuItemVisibility(ImportEntryLinesMenuItem, () => CanImportEntryLines);
			SetMenuItemVisibility(CopyPreviousGoodsItemMenuItem, () => Header.IsDepartureMovement);

			UpdateBillsCountDependantMenuItems(ImportEntryLinesMenuItem.MenuItems, ImportEntryLinesClick);
			UpdateBillsCountDependantMenuItems(ImportInvoiceLinesMenuItem.MenuItems, ImportInvoiceLinesClick);
			UpdateBondedWarehouseMenuItem();
			UpdateTSRegisterManagementMenuItem();
		}

		protected virtual bool CanSendToCustoms => true;

		protected virtual bool CanImportInvoiceLines => true;

		protected virtual bool CanImportEntryLines => Header.IsDepartureMovement;

		protected virtual bool CanMakeArrivalNotification => Header.IsPhase5Departure
			&& Header.MovementHeader is NctsDepartureMovementHeader departureMovementHeader
			&& (departureMovementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture || departureMovementHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit);

		bool CanTSRegisterManagement => Header.IsDepartureMovement
			&& Header.MovementHeader is NctsDepartureMovementHeader departureMovementHeader
			&& IsTemporaryStorageRegisterEnabled
			&& departureMovementHeader.IsLocationManagedInPremises;

		protected bool IsLockUnlockCustomsDeclarationAvailable => Env.Security.EuNctsLockOrUnlockFileForEdit.IsAllowed && (Header.IsPhase5Arrival ? (Header.CanLockUnlockDeclaration(EUJobMessageTypeList.Codes.NctsArrivalNotification) || Header.CanLockUnlockDeclaration(EUJobMessageTypeList.Codes.NctsArrivalUnloadingRemarks)) : Header.CanLockUnlockDeclaration(EUJobMessageTypeList.Codes.NctsDeparture));

		int BillCount => Header.IsDepartureMovement ? Header.Bills.Count : 0;

		bool IsTemporaryStorageRegisterEnabled => EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(Header.CountryCode);

		void UpdateBillsCountDependantMenuItems(Menu.MenuItemCollection menuItems, EventHandler onClickEventHandler)
		{
			var billCount = BillCount;
			if (billCount <= 1)
			{
				menuItems.Clear();
			}
			else
			{
				var currentCount = menuItems.Count;
				if (currentCount > billCount)
				{
					for (var i = currentCount - 1; i >= billCount; --i)
					{
						menuItems.RemoveAt(i);
					}
				}
				else
				{
					for (var i = currentCount; i < billCount; ++i)
					{
						menuItems.Add(new ZMenuItem(ImportToHouseConsignmentCaption(i + 1), onClickEventHandler) { Tag = i });
					}
				}
			}
		}

		void ImportEntryLinesClick(object sender, EventArgs e)
		{
			if (sender is MenuItem menuItem)
			{
				if (BillCount < 1)
				{
					Globals.Message.Show(Res.GetString("88AADDFD-0D79-4623-98D1-3D6F800C994B", "In order to import Entry Lines, please first add at least one House Consignment."));
				}
				else
				{
					var billIndex = menuItem.Tag as int? ?? 0;
					var nctsHeader = Header;
					var houseConsignment = nctsHeader.Bills[billIndex];
					if (houseConsignment != null)
					{
						var attacher = new CusEntryHeaderEntryLinesToNctsAttacher(houseConsignment, nctsHeader.Lookups.CusEntryHeadersToAttach);
						attacher.Show((ZForm)importEntryLinesMenuItem.GetMainMenu().GetForm());
					}
				}
			}
		}

		void UpdateBondedWarehouseMenuItem()
		{
			if (!Header.IsDepartureMovement)
			{
				return;
			}

			var menuItems = InventoryManagementMenuItem.MenuItems;

			menuItems.Clear();

			var existsAnyNonImportedBillWithWhsQty = Header.Bills.SelectMany(b => b.GoodsItems).Any(gi => !gi.Bill.IsOutwardOrderImported && !gi.BY_BondedWhsQuantity.IsEmpty);
			var existsAnyOutwardOrderImportedBill = Header.Bills.Any(b => b.IsOutwardOrderImported);

			foreach (var bill in Header.Bills)
			{
				var houseConsignmentMenuItem = CreateHouseConsignmentMenuItem(bill.SequenceNumber);

				if (!bill.IsOutwardOrderImported && !existsAnyNonImportedBillWithWhsQty)
				{
					var importWarehouseOrderMenuItem = new ZMenuItem(Res.GetString("7e5a43ec-978c-4ff4-9f1f-3c8f7d762509", "Import Warehouse Order"), ImportWarehouseOrderClick) { Tag = bill.PK };
					houseConsignmentMenuItem.MenuItems.Add(importWarehouseOrderMenuItem);
				}

				if (NctsCustomsDataRegistry.Instance.EnableInventoryManagement.Value && !existsAnyOutwardOrderImportedBill && !bill.Header.MovementHeader.BM_OA_WarehouseAddress.IsEmpty)
				{
					var selectInventoryMenuItem = new ZMenuItem(Res.GetString("AF1DA9B9-4929-4220-A3F1-C225D99A4EB7", "Select Inventory"), SelectInventoryClickWHS) { Tag = bill.PK };
					houseConsignmentMenuItem.MenuItems.Add(selectInventoryMenuItem);
				}

				if (WarehouseTransactionStatusList.HasWHSTransaction(((IWarehouseIntegrationSupporter)Header).WarehouseTransactionStatus))
				{
					var cancelInventoryMenuItem = new ZMenuItem(Res.GetString("82A7F1E1-29E5-4134-A501-EA4A61F45786", "Cancel Inventory"), CancelInventoryClick);
					houseConsignmentMenuItem.MenuItems.Add(cancelInventoryMenuItem);
				}

				menuItems.Add(houseConsignmentMenuItem);
			}
		}

		void UpdateTSRegisterManagementMenuItem()
		{
			if (!Header.IsDepartureMovement || !IsTemporaryStorageRegisterEnabled)
			{
				return;
			}

			var menuItems = TSRegisterManagementMenuItem.MenuItems;
			menuItems.Clear();

			if (Header.MovementHeader.IsLocationManagedInPremises)
			{
				if (Header.Bills.Count == 0)
				{
					var houseConsignmentMenuItem = CreateHouseConsignmentMenuItem(1);
					AddSelectInventoryMenuItem(houseConsignmentMenuItem, Guid.Empty);
					menuItems.Add(houseConsignmentMenuItem);
				}
				else
				{
					foreach (var bill in Header.Bills)
					{
						var houseConsignmentMenuItem = CreateHouseConsignmentMenuItem(bill.SequenceNumber);
						AddSelectInventoryMenuItem(houseConsignmentMenuItem, bill.PK);
						menuItems.Add(houseConsignmentMenuItem);
					}
				}
			}

			void AddSelectInventoryMenuItem(ZMenuItem houseConsignmentMenuItem, ZGuid billPK)
			{
				var selectInventoryMenuItem = new ZMenuItem(Res.GetString("5F938FE0-E6C8-4799-9605-3A3DDD1E076B", "Select Inventory"), SelectInventoryClickTS) { Tag = billPK };
				houseConsignmentMenuItem.MenuItems.Add(selectInventoryMenuItem);
				menuItems.Add(houseConsignmentMenuItem);
			}
		}

		ZMenuItem CreateHouseConsignmentMenuItem(ZShort sequenceNumber)
		{
			return new ZMenuItem(Res.GetString("881F0901-57A5-4AD6-A38F-DC8F2BFBBBD6", "House Consignment {0}", sequenceNumber));
		}

		void ImportInvoiceLinesClick(object sender, EventArgs e)
		{
			if (sender is MenuItem menuItem)
			{
				if (BillCount < 1)
				{
					Globals.Message.Show(Res.GetString("5E52F6F2-9F63-468E-9797-71F3EECD6C14", "In order to import Invoice Lines, please first add at least one House Consignment."));
				}
				else
				{
					var billIndex = menuItem.Tag as int? ?? 0;
					var nctsHeader = Header;
					var attacher = new CusEntryHeaderInvoiceLinesToNctsAttacher(nctsHeader, nctsHeader.Lookups.CusEntryHeadersToAttach) { TargetConsignmentIndex = billIndex };

					attacher.Show((ZForm)ImportInvoiceLinesMenuItem.GetMainMenu().GetForm());
				}
			}
		}

		void ImportWarehouseOrderClick(object sender, EventArgs e)
		{
			if (sender is MenuItem menuItem)
			{
				var billPK = menuItem.Tag as ZGuid?;
				if (billPK.HasValue)
				{
					var nctsHeader = Header;
					var bill = nctsHeader.Bills.FirstOrDefault(b => b.PK == billPK);
					if (bill != null)
					{
						new OrderSelectionPrompter(nctsHeader.Configuration.GetNewOrderInventorySelectionHeader(bill)).Prompt();
					}
				}
			}
		}
		void SelectInventoryClickWHS(object sender, EventArgs e)
		{
			if (sender is MenuItem menuItem)
			{
				var billPK = menuItem.Tag as ZGuid?;
				if (billPK.HasValue)
				{
					var nctsHeader = Header;
					var bill = nctsHeader.Bills.FirstOrDefault(b => b.PK == billPK);
					if (bill != null)
					{
						var inventoryHeader = GetInventorySelectionHeader(bill);
						using var form = new InventorySelectionForm(inventoryHeader);
						ZFormModaliser.ShowDialogWithoutDispose(form);
					}
				}
			}
		}

		void SelectInventoryClickTS(object sender, EventArgs e)
		{
			Globals.Message.ShowError((NoResString)"This functionality is not yet available");
		}

		protected virtual NctsInventorySelectionHeader GetInventorySelectionHeader(NctsBill bill) => Header.Configuration.GetNewInventorySelectionHeader(bill);

		void CancelInventoryClick(object sender, EventArgs e)
		{
			if (Globals.Message.ShowConfirmation(
					ResString.GetMultilingualString("4DF63199-52FE-4F8B-89E2-9C39FA79F2DD", "Are you sure you want to cancel the Inventory Management stock release for this job? If you click 'Yes', all stock for this job will be uncommitted in the Inventory Management."),
					ResString.GetMultilingualString("FBC9B770-CDF2-4F30-964C-4088F438A33B", "Cancel Inventory"),
					ResString.GetMultilingualString("8A6F7E3B-4966-4D2C-8BEA-03FE5A885BB0", "yes"),
					MessageBoxIcon.Question) == DialogResult.OK)
			{
				Header.CancelBondedWarehouseOutward();
			}
		}

		string ImportToHouseConsignmentCaption(int index) =>
			string.Format(ResString.GetMultilingualString("834304C8-E4DB-4860-8684-88690EC18EF7", "Import to House Consignment {0}").ToString(), index);

		protected virtual void SendToCustomsClick(object sender, EventArgs e)
		{
			if (IsResending(Header.MovementHeader) || IsResending(Header.ArrivalMovementHeader))
			{
				if (Globals.Message.ShowConfirmation(
					ResString.GetMultilingualString("BDF5FB15-77F7-42D8-8DC3-37A557F6574E", "Are you sure you want to resend this message?"),
					ResString.GetMultilingualString("8EEA095B-87A3-4E6E-92FA-BD6F74E024CC", "Resend Message"),
					ResString.GetMultilingualString("DC6A857A-216A-47E7-B052-6873DA633F8E", "yes"),
					MessageBoxIcon.Question) == DialogResult.OK)
				{
					ReSendToCustoms(GetLatestEDIInterchange());
				}
			}
			else
			{
				SendToCustoms((ZMenuItem)sender);
			}
		}

		void MakeArrivalNotificationClick(object sender, EventArgs e)
		{
			var generator = GetNctsHeaderGenerator();

			if (generator.IsGenerationAllowed(Header))
			{
				var nctsArrival = generator.GenerateArrivalFromDeparture(Header);
				ZFormModaliser.ShowDialogAndDispose(new Phase5ArrivalMovementForm(nctsArrival)
				{
					ControllerID = ControllerIDs.Customs.EU.NctsMovementController
				});
			}
			else
			{
				Globals.Message.Show(Res.GetString("5C233D0F-D4AA-4AB8-81DA-B32DEB1CB781", "NCTSP5 arrival for MRN {0} already generated.", Header.MovementReferenceNumber));
			}
		}

		protected virtual NctsHeaderGenerator GetNctsHeaderGenerator() => new NctsHeaderGenerator();

		EDIInterchange GetLatestEDIInterchange()
		{
			return Header.Messages
				.Cast<EDIMessage>()
				.Select(m => m.Interchange)
				.Where(i => i != null && i.EI_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit)
				.OrderByDescending(x => x.EI_SystemCreateTimeUtc)
				.FirstOrDefault();
		}

		void ReSendToCustoms(EDIInterchange latestInterchange)
		{
		}

		void SendToCustoms(ZMenuItem menuItem)
		{
			var movementHeader = Header.IsDepartureMovement ? (NctsCommonMovementHeader)Header.MovementHeader : Header.ArrivalMovementHeader;
			if (movementHeader.ShouldGenerateLocalReferenceNumberOnSaving)
			{
				movementHeader.BM_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}

			if (!Header.HasMessageInitiator)
			{
				Header.MessageInitiator = GetNewMessageInitiator();
			}

			if (PreSaveHeader(menuItem))
			{
				SendToCustomsCore(menuItem);
			}
		}

		protected virtual void SendToCustomsCore(ZMenuItem menuItem)
		{
			var sendingObjectParent = Header.Configuration.MessageSendingConfiguration.GetNewNctsHeaderMessageSendingObjectParent(Header);
			using (var form = GetMessageSendingForm(sendingObjectParent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					if (sendingObjectParent.SendAndSaveMessages())
					{
						Globals.Message.Show(Res.GetString("80ff0b55-6578-4580-9925-5b758b66ad28", "The message has been sent."));
					}
					else
					{
						Globals.Message.ShowWarning(Res.GetString("5037599B-A73F-4F7C-BA65-6C844A9815A2", "The message has not been sent. This may be caused by a failure or because sending has not been implemented"));
					}
				}
			}
		}

		MessageSendingForm GetMessageSendingForm(NctsHeaderMessageSendingObjectParent messageSendingobjectParent) => GetMessageSendingFormCore(messageSendingobjectParent);

		protected virtual MessageSendingForm GetMessageSendingFormCore(NctsHeaderMessageSendingObjectParent messageSendingobjectParent) => new MessageSendingForm(messageSendingobjectParent);

		protected virtual bool IsResending(CusInBondMoveHeader movementHeader) =>
			(string)(movementHeader?.BM_Phase ?? ZString.Empty) switch
			{
				NctsMovementHeaderTransactionStatusList.Codes.AmendmentSent or
				NctsMovementHeaderTransactionStatusList.Codes.InvalidationSent or
				NctsMovementHeaderTransactionStatusList.Codes.DeclarationSent or
				NctsMovementHeaderTransactionStatusList.Codes.RequestForReleaseSent or
				NctsMovementHeaderTransactionStatusList.Codes.InformationNonArrivedMovementSent or
				NctsMovementHeaderTransactionStatusList.Codes.PresentationNotificationSent or
				NctsMovementHeaderTransactionStatusList.Codes.ArrivalNotificationSent or
				NctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarksSent => true,
				_ => false
			};

		protected bool PreSaveHeader(ZMenuItem menuItem)
		{
			var topLevelBizObj = (BusinessObject)Header.Consol ?? (BusinessObject)Header.Shipment ?? Header;
			return CustomsPlugIn.FormPreSaved(topLevelBizObj, GetForm(menuItem));
		}

		protected ZForm GetForm(ZMenuItem menuItem) => (ZForm)menuItem.GetMainMenu().GetForm();
	}
}
