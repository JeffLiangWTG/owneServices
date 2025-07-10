using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.ES.TemporaryStorage.GUI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Customs.EU.NCTS.Business.NctsHeader;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;
using NctsHeaderMessageSendingObject = Enterprise.Customs.ES.NCTS.Business.NctsHeaderMessageSendingObject;
using NctsHeaderMessageSendingObjectParent = Enterprise.Customs.ES.NCTS.Business.NctsHeaderMessageSendingObjectParent;

namespace Enterprise.Customs.ES.NCTS.GUI;

public class Phase5MessagingMenuProvider : EU.NCTS.GUI.Phase5MessagingMenuProvider
{
	public Phase5MessagingMenuProvider(NctsHeader header)
		: base(header)
	{
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	protected override IEnumerable<ZMenuItem> CreateMenuItemsCore()
	{
		yield return SendToCustomsMenuItem;
		yield return SynchronizeWithCustoms;
		yield return DownloadTAD;
		foreach (var menuItem in base.CreateMenuItemsCore())
		{
			if (menuItem != SendToCustomsMenuItem)
			{
				yield return menuItem;
			}
		}
		yield return MakeTNN;
		yield return LoadDataForUnloading;
		yield return Spacer;
		yield return CheckForInboxNotifications;
		yield return CaptureFromCustoms;
		yield return ViewOnCustomsWebsite;
		yield return CreateEXSDeclaration;
		yield return SetAsFailedFromTransmission;
		yield return IntoTemporaryStorageSpacer;
		yield return IntoTemporaryStorage;
		yield return ViewTemporaryStorageRegister;
	}

	ZMenuItem CreateEXSDeclaration => createEXSDeclaration ??= new ZMenuItem(Constants.MenuProviderCaptions.CreateEXSDeclaration, CreateEXSDeclarationClick);
	ZMenuItem createEXSDeclaration;

	ZMenuItem ViewOnCustomsWebsite => viewOnCustomsWebsite ??= new ZMenuItem(Constants.MenuProviderCaptions.ViewOnCustomsWebsite, ViewOnCustomsWebsiteClick);
	ZMenuItem viewOnCustomsWebsite;

	ZMenuItem CaptureFromCustoms => captureFromCustoms ??= new ZMenuItem(ResString.GetMultilingualString("13591DF7-8038-4CF8-A0A5-D8FDC84BDC8B", "Capture from Customs"), CaptureFromCustomsClick);
	ZMenuItem captureFromCustoms;

	ZMenuItem CheckForInboxNotifications => checkForInboxNotifications ??= new ZMenuItem(ResString.GetMultilingualString("58F7267E-35D2-443D-81BF-EF355EE47154", "Check for Inbox Notifications"), CheckForInboxNotificationsClick);
	ZMenuItem checkForInboxNotifications;

	ZMenuItem SynchronizeWithCustoms => synchronizeWithCustoms ??= new ZMenuItem(ResString.GetMultilingualString("0126B5F4-5702-4746-8E46-F78700160205", "Synchronize with Customs"), SynchronizeWithCustomsClick);
	ZMenuItem synchronizeWithCustoms;

	ZMenuItem MakeTNN => makeTNN ??= new ZMenuItem(ResString.GetMultilingualString("28612B98-F425-4FAE-ACBD-F9BDD85585FC", "Make TNN for this Arrival"), MakeTNNClick);
	ZMenuItem makeTNN;

	ZMenuItem LoadDataForUnloading => loadDataForUnloading ??= new ZMenuItem(ResString.GetMultilingualString("F8625583-50D8-4659-9E4E-5821F9AE3723", "Load Data for Unloading"), LoadDataForUnloadingClick);
	ZMenuItem loadDataForUnloading;

	ZMenuItem Spacer => spacer ??= CreateSpacerMenuItem();
	ZMenuItem spacer;

	ZMenuItem DownloadTAD => downloadTAD ??= new ZMenuItem(Constants.MenuProviderCaptions.DownloadTAD, DownloadTADClick);
	ZMenuItem downloadTAD;

	ZMenuItem SetAsFailedFromTransmission => setAsFailedFromTransmission ??= new ZMenuItem(ResString.GetMultilingualString("EC649D8D-2B7F-48D0-8FB5-958BD401C491", "Set Entry as Failed From Transmission"), SetAsFailedFromTransmissionClick);
	ZMenuItem setAsFailedFromTransmission;

	ZMenuItem IntoTemporaryStorageSpacer => intoTemporaryStorageSpacer ??= CreateSpacerMenuItem();
	ZMenuItem intoTemporaryStorageSpacer;

	ZMenuItem IntoTemporaryStorage => intoTemporaryStorage ??= new ZMenuItem(ResString.GetMultilingualString("5FDC4528-01E3-4686-96FD-6EB94B47F6DB", "Into Temporary Storage"), IntoTemporaryStorageClick);
	ZMenuItem intoTemporaryStorage;

	ZMenuItem ViewTemporaryStorageRegister => viewTemporaryStorageRegister ??= new ZMenuItem(ResString.GetMultilingualString("298F999D-442E-4A7B-B70A-5391337440C4", "View TS Register"), ViewTemporaryStorageRegisterClick);
	ZMenuItem viewTemporaryStorageRegister;

	protected override void SendToCustomsClick(object sender, EventArgs e)
	{
		if ((Header.IsDepartureMovement && !sendableDepartureCustomsStatuses.Contains(Header.MovementHeader?.BM_CustomsStatus ?? ZString.Empty)) ||
			(Header.IsArrivalMovement && !sendableArrivalCustomsStatuses.Contains(Header.ArrivalMovementHeader?.BM_CustomsStatus ?? ZString.Empty)))
		{
			Globals.Message.ShowError(NoSendingOptionsAvailableMessage);
			return;
		}

		base.SendToCustomsClick(sender, e);
	}

	string NoSendingOptionsAvailableMessage => Res.GetString("464889AF-64B6-4E24-9495-81FFBFE671F6", "No sending options available.");

	protected override bool IsResending(CusInBondMoveHeader movementHeader) =>
		Header.IsDepartureMovement
		? DepartureStatusCodesForResending.Contains(Header.MovementHeader.BM_CustomsStatus)
		: ArrivalStatusCodesForResending.Contains(Header.ArrivalMovementHeader.BM_CustomsStatus);

	ImmutableHashSet<string> DepartureStatusCodesForResending => departureStatusCodesForResending ??=
		ImmutableHashSet.Create(
			ESNCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid,
			ESNCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit,
			ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit,
			ESNCTS5DepartureCustomsStatusList.Codes.Cancelled,
			ESNCTS5DepartureCustomsStatusList.Codes.Invalidated,
			ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated
		);
	ImmutableHashSet<string> departureStatusCodesForResending;

	ImmutableHashSet<string> ArrivalStatusCodesForResending => arrivalStatusCodesForResending ??=
		ImmutableHashSet.Create(
			ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease,
			ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease,
			NctsMessageStatusList.Codes.ArrivalNotificationSent
		);
		ImmutableHashSet<string> arrivalStatusCodesForResending;

	public override void RefreshMenu()
	{
		base.RefreshMenu();
		SetMenuItemVisibility(CreateEXSDeclaration, () => CanCreateEXSDeclaration());
		SetMenuItemVisibility(ViewOnCustomsWebsite, () => CanViewOnCustomsWebsite());
		SetMenuItemVisibility(CaptureFromCustoms, () => CanCaptureFromCustoms());
		SetMenuItemVisibility(CheckForInboxNotifications, () => CanCheckForInboxNotifications());
		SetMenuItemVisibility(SynchronizeWithCustoms, () => CanSynchronizeWithCustoms());
		SetMenuItemVisibility(MakeTNN, () => CanMakeTNN());
		SetMenuItemVisibility(LoadDataForUnloading, () => CanLoadDataForUnloading());
		SetMenuItemVisibility(DownloadTAD, () => CanDownloadTAD());
		SetMenuItemVisibility(SetAsFailedFromTransmission, () => Header.IsSent);
		SetMenuItemVisibility(IntoTemporaryStorageSpacer, () => CanCreateTemporaryStorage());
		SetMenuItemVisibility(IntoTemporaryStorage, () => CanCreateTemporaryStorage());
		SetMenuItemVisibility(ViewTemporaryStorageRegister, () => CanViewTemporaryStorageRegister);

		var oldText = ZMenuItem.Separator;
		var visibleMenuItem = CreateMenuItems().Where(x => x.Visible);
		foreach (var menuItemRemove in visibleMenuItem)
		{
			var menuText = menuItemRemove.Text;
			if (oldText == menuText && menuText == ZMenuItem.Separator)
			{
				SetMenuItemVisibility(Spacer, () => false);
			}
			oldText = menuText;
		}
	}

	void RefreshDepartureRelatedTabs(ZMenuItem menuItem)
	{
		var parentForm = GetForm(menuItem);
		if (parentForm is Phase5DepartureMovementForm phase5DepartureMovementForm)
		{
			phase5DepartureMovementForm.ToggleDepartureDeclarationRelatedTabsEditableState();
		}
	}

	void RefreshArrivalRelatedTabs(ZMenuItem menuItem)
	{
		var parentForm = GetForm(menuItem);
		if (parentForm is Phase5ArrivalMovementForm phase5ArrivalMovementForm)
		{
			phase5ArrivalMovementForm.ToggleArrivalNotificationTabEditableState();
			phase5ArrivalMovementForm.ToggleIncidentsTabEditableState();
		}
	}

	protected override void SendToCustomsCore(ZMenuItem menuItem)
	{
		var commonMenuProvider = GetESNctsCommonMessagingMenuProvider(parentForm: GetForm(menuItem));
		commonMenuProvider.ESSendMessageToNcts();

		RefreshDepartureRelatedTabs(menuItem);
		RefreshArrivalRelatedTabs(menuItem);
	}

	protected virtual ESNctsCommonMessagingMenuProvider GetESNctsCommonMessagingMenuProvider(SendingType sendingType = SendingType.None, ZForm parentForm = null) => new ESNctsCommonMessagingMenuProvider(Header, sendingType, parentForm);

	void SynchronizeWithCustomsClick(object sender, EventArgs ev)
	{
		var menuItem = (ZMenuItem)sender;
		if (PreSaveHeader(menuItem) &&
			CheckBrokerBeforeSending(Header, out var messageSendingObject))
		{
			var sendTQUConfirmed = Globals.Message.Show(
									Res.GetString("481BD289-74F0-4078-B3DC-8A03DD222B65", "A Query Message (TQU) will be sent to Customs to Synchronize Pre-Declaration data.\r\nDo you want to send that message? Please note that existing data will be overwritten, but Consignee and Consignor will not be updated, you must do it manually if changed."),
									Res.GetString("5B4C3C08-9C4D-48CE-8067-782AA485E49E", "Synchronize with Customs"),
									MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;
			if (sendTQUConfirmed)
			{
				var changesToPreDeclaration = Header.CheckPreDeclarationChanges(messageSendingObject);
				if (!changesToPreDeclaration.IsEmpty)
				{
					SendTransitQueryMessage();
					RefreshDepartureRelatedTabs(menuItem);
				}
				else
				{
					var message = ResString.GetMultilingualString("E23B8D28-20C6-4399-8879-22E302B0A744",
												  "There are no changes in data already sent in Pre-declaration so this action will not be triggered");
					Globals.Message.ShowError(message);
				}
			}
		}
	}

	void MakeTNNClick(object sender, EventArgs ev)
	{
		var menuItem = (ZMenuItem)sender;

		if (PreSaveHeader(menuItem))
		{
			var arrivalMRN = Header.MovementReferenceNumber;
			if (arrivalMRN.IsEmpty)
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("2B642A80-E2A4-438C-BBC7-A56E5C4BBE94", "MRN is mandatory to create a TNN declaration."));
			}
			else
			{
				var (matchingResult, departure) = Header.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
				if (matchingResult == DepartureRecordFindResult.FoundByMatchingMrn)
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("A5EF9AA5-64CE-443D-9878-958C2253BCEA", "The {0} Departure is registered with the MRN {1} and the TNN declaration should not be required.", departure.BH_JobReference, arrivalMRN));
				}
				else
				{
					var factory = new BusinessObjectFactory();
					var newFactoryHeader = factory.Load<NctsHeader>(Header.PK);

					var tnnDataCodeInfo = LoadNewTnnDataCodeInfo(newFactoryHeader);
					using (var tnnforArrivalForm = new TnnForArrivalForm(tnnDataCodeInfo))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(tnnforArrivalForm) == DialogResult.OK)
						{
							try
							{
								newFactoryHeader.ArrivalMovementHeader.GenerateTNNDeparture(tnnDataCodeInfo);
								factory.Save();

								Globals.Message.Show(Res.GetString("B3941B32-1474-4622-8A17-B4F60A03650A", "TNN successfully created."));
							}
							catch (ZSaveException ex)
							{
								ZExceptionReporting.HandleSaveException(ex);
							}
						}
					}

					var parentForm = GetForm(menuItem);
					if (parentForm is Phase5ArrivalMovementForm phase5ArrivalMovementForm)
					{
						phase5ArrivalMovementForm.ChangeTNNTabVisibility();
					}
				}
			}
		}
	}

	protected virtual TnnDataCodeInfo LoadNewTnnDataCodeInfo(NctsHeader header) => TnnDataCodeInfo.LoadNew(header);

	void LoadDataForUnloadingClick(object sender, EventArgs ev)
	{
		if (PreSaveHeader((ZMenuItem)sender))
		{
			var (matchingResult, departure) = Header.FindRelevantDepartureRecordForCombinedDepartureAndArrival();
			if (matchingResult == DepartureRecordFindResult.FoundByMatchingMrn)
			{
				var loadDataFromDeparture = false;
				var selectorAnswerOK = false;
				using (var loadDataForUnloadingSelectorForm = GetLoadDataForUnloadingSelectorForm())
				{
					(selectorAnswerOK, loadDataFromDeparture) = loadDataForUnloadingSelectorForm.GetResultFromSelector();
				}

				if (selectorAnswerOK)
				{
					if (loadDataFromDeparture)
					{
						Header.ArrivalMovementHeader.LoadDataForUnloadingFromDeparture((NctsHeader)departure);
						Globals.Message.Show(Res.GetString("88C7AC1E-E5D5-489C-A3B6-4034EB08BEB0", "Data loaded correctly. Please, save the job before changing any value."));
					}
					else
					{
						SendTransitQueryMessage();
					}
				}
			}
			else
			{
				var sendTQUConfirmed = Globals.Message.Show(
									Res.GetString("88374DF3-8A15-4680-B990-D3251314F56D", "A Query Message (TQU) will be sent to Customs to obtain the Departure Data for the Unloading.\r\nDo you want to send that message? Please note existing data in Unloading Remarks could be overwritten. "),
									Res.GetString("3AFD3FD4-8BB4-4071-976C-FD2353EBE692", "Load from Customs Query"),
									MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;
				if (sendTQUConfirmed)
				{
					SendTransitQueryMessage();
				}
			}
		}
	}

	protected virtual LoadDataForUnloadingSelectorForm GetLoadDataForUnloadingSelectorForm() => new LoadDataForUnloadingSelectorForm();

	void CreateEXSDeclarationClick(object sender, EventArgs ev)
	{
		if (PreSaveHeader((ZMenuItem)sender))
		{
			var commonMenuProvider = GetESNctsCommonMessagingMenuProvider();
			commonMenuProvider.CreateEXSDeclaration();
		}
	}

	void ViewOnCustomsWebsiteClick(object sender, EventArgs ev)
	{
		var commonMenuProvider = GetESNctsCommonMessagingMenuProvider();
		commonMenuProvider.LaunchCustomsWebsite();
	}

	void CaptureFromCustomsClick(object sender, EventArgs ev)
	{
		if (PreSaveHeader((ZMenuItem)sender))
		{
			SendTransitQueryMessage();
		}
	}

	void SendTransitQueryMessage()
	{
		var commonMenuProvider = GetESNctsCommonMessagingMenuProvider(SendingType.TransitQuery);
		commonMenuProvider.ESSendMessageToNcts(SelectEntriesForTransitQuery);
	}

	bool SelectEntriesForTransitQuery(NctsHeaderMessageSendingObjectParent decWrapper)
	{
		foreach (NctsHeaderMessageSendingObject sendingObject in decWrapper.SendingObjectsCollection)
		{
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = DeclarationMessageTypeList.Codes.TransitNcts5Query;
		}

		return true;
	}

	void SetAsFailedFromTransmissionClick(object sender, EventArgs ev)
	{
		var confirmationMessage = ResString.GetMultilingualString("64E7090A-5436-4FB2-AE74-2D8B5CE7148C", "Are you sure you want to set this Entry as Failed from Transmission?");
		var caption = ResString.GetMultilingualString("B815D74E-54FD-4ED3-AE72-BC921EE55162", "Failed from Transmission");
		var confirmationPrompt = ResString.GetMultilingualString("9C402288-15F4-4EFF-8C79-1CD17FE2DCA7", "If you are absolutely sure you want to set this Entry as Failed From Transmission, please type:");
		var confirmationString = ResString.GetMultilingualString("354738BD-EDE5-4D55-BD5F-9926CC3643FE", "yes");

		if (Header.IsSent
					&& Globals.Message.ShowConfirmation(confirmationMessage, caption, confirmationPrompt, confirmationString, MessageBoxIcon.Warning) == DialogResult.OK)
		{
			Header.CommonMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Failed;
			Globals.Message.Show(ResString.GetMultilingualString("BDDDA1FE-66E0-4F8E-BF63-48A3F003E12C", "Entry was set to Failed from Transmission"));
		}
	}

	void CheckForInboxNotificationsClick(object sender, EventArgs ev)
	{
		if (PreSaveHeader((ZMenuItem)sender))
		{
			if (CheckBrokerBeforeSending(Header, out var messageSendingObject))
			{
				if (CheckDepartureStatusBeforeSendingForInboxNotification(Header))
				{
					int messagesCreated = 0;

					var messagesTypesToSend = new List<ZString>();
					var messageTypesList = GetInboxRequestMessageTypes(Header);

					foreach (var messageType in messageTypesList)
					{
						CreateAndSaveInboxRequestMessage(Header, messageType, ((ICertificateProvider)messageSendingObject).CertificateName);
						messagesCreated++;
						if (!messagesTypesToSend.Contains(messageType))
						{
							messagesTypesToSend.Add(messageType);
						}
					}

					CreateAndSaveInboxListMessages(messagesTypesToSend.ToArray());

					if (messagesCreated == 1)
					{
						Globals.Message.Show(ResString.GetMultilingualString("C2F4980B-9ADE-438C-8A60-EC174E59F2AE", "1 In-box Notification request created"));
					}
					else
					{
						Globals.Message.Show(ResString.GetMultilingualString("AD577A46-516B-4092-855B-EEF9EFCCB516", "{0} In-box Notification requests created", messagesCreated.ToString(Culture.Current)));
					}
				}
			}
		}
	}

	void DownloadTADClick(object sender, EventArgs ev)
	{
		if (PreSaveHeader((ZMenuItem)sender))
		{
			if (CheckBrokerBeforeSending(Header, out var messageSendingObject))
			{
				var factory = new BusinessObjectFactory();
				var newFactoryHeader = factory.Load<NctsHeader>(Header.PK);
				if (newFactoryHeader != null)
				{
					newFactoryHeader.Reload();

					var certName = ((ICertificateProvider)messageSendingObject).CertificateName;
					if (newFactoryHeader.ClearanceReferenceNumber.IsEmpty)
					{
						Globals.Message.ShowError(ResString.GetMultilingualString("22C3A9D9-538A-49A6-A7A0-4A1983A47CD8", "Cannot download TAD when Clearance Number is empty."));
					}
					else
					{
						var nctsDepartureDocRequest = new NCTS5DepartureDocumentRequest(newFactoryHeader, certName);
						int messagesSentCount = ZInt.Zero;
						try
						{
							messagesSentCount = nctsDepartureDocRequest.RequestMissingDocuments();
							factory.Save();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ZExceptionReporting.HandleSaveException(ex);
						}

						Globals.Message.Show(ESNctsCommonMessagingMenuProvider.GetMessageDocumentCaptureSuccessful(messagesSentCount));
					}
				}
			}
		}
	}

	public ZGlobalMutex Mutex => fMutex ??= new ZGlobalMutex(MutexIDs.CusTempStorageRegHeaderMutex, Header.PK.ToString());
	ZGlobalMutex fMutex;

	void IntoTemporaryStorageClick(object sender, EventArgs ev)
	{
		void ShowMutexWarning() => Globals.Message.ShowWarning(IntoTemporaryStorageHelper.GetMutexLockText(Mutex.GetLockInfo().UserWithLock.FullName));
		if (PreSaveHeader((ZMenuItem)sender))
		{
			if (!Mutex.IsLocked)
			{
				var lockedSuccessfully = Mutex.Lock();
				if (lockedSuccessfully)
				{
					IntoTemporaryStorageProcess();
				}
				else
				{
					ShowMutexWarning();
				}
				Mutex.Unlock();
			}
			else
			{
				ShowMutexWarning();
			}
		}
	}

	void IntoTemporaryStorageProcess()
	{
		var header = Header;
		var arrivalGoodsLocation = header.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier;
		var guarantee = header.ArrivalMovementHeader.GuaranteesForArrival.FirstOrDefault();

		if (IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(header.Factory, arrivalGoodsLocation, false) &&
			IntoTemporaryStorageHelper.GuaranteeAndLiabilityAmountAreDeclared(guarantee) &&
			IntoTemporaryStorageHelper.GoodsItemsAreNotInTemporaryStorage(header.Factory, header.SummaryEntryNumber.CE_EntryNum, false) &&
			IntoTemporaryStorageHelper.GuaranteNumberExistsAndIsValid(guarantee, guarantee.CusGuaranteeWithoutPermitHolder) &&
			AtLeastOnePackageWithStatusNotMIS(header) &&
			GrossMassCheckForPackages(header) &&
			IntoTemporaryStorageHelper.ShowWarningIfGuaranteeLiabilityAmountIsZero(guarantee) &&
			GoodsItemsDoesNotContainPackageWithGrossMassEmpty(header))
		{
			var factory = new BusinessObjectFactory();
			var newFactoryHeader = factory.Load<NctsHeader>(Header.PK);
			if (newFactoryHeader != null)
			{
				newFactoryHeader.Reload();

				var result = false;

				try
				{
					result = newFactoryHeader.ArrivalMovementHeader.CreateTemporaryStorageData();
					factory.Save();

					if (result)
					{
						Globals.Message.Show(ResString.GetMultilingualString("36588669-55C8-4334-8727-4166C5FE929F", "Temporary Storage data created successfully."));
						newFactoryHeader.ArrivalMovementHeader.BM_Phase = ESNcts5ArrivalPhaseList.Codes.TemporaryStorageActivated;
						factory.Save();

						var warningMessage = newFactoryHeader.ArrivalMovementHeader.GuaranteesForArrival.FirstOrDefault().GetWarningErrorIfRemainingBalanceIsNotEnough();
						if (!warningMessage.IsEmpty)
						{
							Globals.Message.ShowWarning(warningMessage);
						}

						newFactoryHeader.ArrivalMovementHeader.AddNewGuaranteeTransactionForTemporaryStorage();
						factory.Save();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}
	}

	void ViewTemporaryStorageRegisterClick(object sender, EventArgs ev)
	{ 
		var header = Header;
		IntoTemporaryStorageHelper.ShowTemporaryStorageRegisterForm(header.Factory, header.ArrivalSummaryDeclaration, header.ArrivalMovementHeader.GoodsLocation.CGL_AdditionalIdentifier);
	}

	bool AtLeastOnePackageWithStatusNotMIS(NctsHeader header)
	{
		var anyPackageNotMIS = header.Bills.Any(b =>
		{
			return b.UnloadedStatus != NctsUnloadedStateList.Codes.MIS &&
				   b.ArrivalGoodsItems.Any(g =>
				   {
					   return g.UnloadedStatus != NctsUnloadedStateList.Codes.MIS &&
							  g.Packages.Any<Business.NctsPackage>(p => p.UnloadedStatus != NctsUnloadedStateList.Codes.MIS);
				   });
		});

		if (!anyPackageNotMIS)
		{
			var error = ResString.GetMultilingualString("228920E7-0721-4036-BFF0-A64157683F1A",
														"There are no good items available to enter the Temporary Storage. Please, check if action 'NCTS/Load Data for Unloading' has been previously triggered.");
			Globals.Message.ShowError(error);
		}

		return anyPackageNotMIS;
	}

	bool GrossMassCheckForPackages(NctsHeader header)
	{
		var anyPackageWithEmptyGrossWeight = header.Bills.Any(b =>
		{
			var anyArrivalGoodItemNotMISWithGrossWeightZero = b.ArrivalGoodsItems.Any(g => g.UnloadedStatus != NctsUnloadedStateList.Codes.MIS && g.BY_GrossWeight.IsEmpty);
			return	b.UnloadedStatus != NctsUnloadedStateList.Codes.MIS && anyArrivalGoodItemNotMISWithGrossWeightZero &&
					b.ArrivalGoodsItems.Any(g =>
					{
						bool AnyPackageInDifferentArrivalGoodsItemWithSameTypeAndMark(Business.NctsPackage package)
						{
							return b.ArrivalGoodsItems.Any(g2 =>
							{
								if (g2.UnloadedStatus != NctsUnloadedStateList.Codes.MIS)
								{
									var isDifferentGoodsItem = g2.PK != g.PK;
									var otherPackageFound = isDifferentGoodsItem &&
															g2.Packages.Any<Business.NctsPackage>(p2 =>
															{
																return  p2.B5_UnitCount.IsEmpty &&
																		p2.UnloadedStatus != NctsUnloadedStateList.Codes.MIS &&
																		p2.EffectiveUnitType == package.EffectiveUnitType &&
																		p2.EffectiveMarksAndNumbers == package.EffectiveMarksAndNumbers;
															});

									return otherPackageFound;
								}
								return false;
							});
						}

						if (g.UnloadedStatus != NctsUnloadedStateList.Codes.MIS)
						{
							var packageWithGrossWeightEmpty = g.Packages.FirstOrDefault<Business.NctsPackage>(p => p.UnloadedStatus != NctsUnloadedStateList.Codes.MIS && p.B5_GrossWeight.IsEmpty);
							return packageWithGrossWeightEmpty != null && (g.BY_GrossWeight.IsEmpty || !PackageHelper.PackTypeIsBulk(packageWithGrossWeightEmpty.EffectiveUnitType, header.Factory) && AnyPackageInDifferentArrivalGoodsItemWithSameTypeAndMark(packageWithGrossWeightEmpty));
						}
						return false;
					});
		});

		if (anyPackageWithEmptyGrossWeight)
		{
			var error = ResString.GetMultilingualString("216E0941-F555-4E7B-8CBA-DA4664B011C5",
														"When the same packages are used for different goods, real gross mass should be entered in all related package lines.");
			Globals.Message.ShowError(error);
		}

		return !anyPackageWithEmptyGrossWeight;
	}

	bool GoodsItemsDoesNotContainPackageWithGrossMassEmpty(NctsHeader header)
	{
		var anyPackageWithEmptyGrossWeight = header.Bills.Any(b =>
		{
			return	b.UnloadedStatus != NctsUnloadedStateList.Codes.MIS &&
					b.ArrivalGoodsItems.Any(g =>
					{
						if (g.UnloadedStatus != NctsUnloadedStateList.Codes.MIS)
						{
							var packagesNotMIS = g.Packages.Where<Business.NctsPackage>(p => p.UnloadedStatus != NctsUnloadedStateList.Codes.MIS);
							return  packagesNotMIS.Count() > 1 &&
									packagesNotMIS.Any(p => p.B5_GrossWeight.IsEmpty);
						}
						return false;
					});
		});

		if (anyPackageWithEmptyGrossWeight)
		{
			var error = ResString.GetMultilingualString("A6C5E082-3D08-4DC8-9972-24838A1AF997",
@"There are some package lines with gross mass empty. For these packages, the Goods Item’s gross weight will be automatically apportioned on the Temporary Storage Register.
This action cannot be undone after introducing goods into the Temporary Storage.
Would you like to cancel this action and enter the gross weight for each package line?");

			var result = Globals.Message.Show(
				error,
				Res.GetString("82814348-A1C9-497C-8F5E-E1EF886509F7", "Into Temporary Storage"),
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.No;

			return result;
		}

		return !anyPackageWithEmptyGrossWeight;
	}

	ZString[] GetInboxRequestMessageTypes(NctsHeader nctsHeader)
	{
		var customsStatus = nctsHeader.MovementHeader.BM_CustomsStatus;
		if (customsStatus == ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl)
		{
			return new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance,
								   DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture };
		}
		else if (customsStatus == ESNCTS5DepartureCustomsStatusList.Codes.PreLodged)
		{
			return new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation };
		}
		else if (customsStatus == ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance)
		{
			return new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance,
								   DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation,
								   DeclarationMessageTypeList.Codes.InboxNotificationNctsControls };
		}
		else if (customsStatus == ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit)
		{
			return new ZString[] { DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation };
		}

		return Array.Empty<ZString>();
	}

	void CreateAndSaveInboxRequestMessage(NctsHeader nctsHeader, ZString messageType, ZString certName)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			MessageRequest.CreateEDIMessageForInboxRequest(factory, nctsHeader, nctsHeader.PK, nctsHeader.TablePrefix, nctsHeader.MovementReferenceNumber, nctsHeader.BH_JobReference, messageType, certName);
			factory.Save();
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	void CreateAndSaveInboxListMessages(ZString[] messagesTypesToSend)
	{
		try
		{
			var factory = new BusinessObjectFactory();
			var inboxListRequestSender = new InboxListRequestSender(null);
			inboxListRequestSender.CreateListPollingEDIMessages(factory, specificTypes: messagesTypesToSend);
			factory.Save();
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
	}

	ZBool CanCreateEXSDeclaration() => Header.IsArrivalMovement
									   && Header.ArrivalMovementHeader.BM_CustomsStatus == ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease
									   && Header.EffectiveMessageStatus.IsEmpty;

	ZBool CanViewOnCustomsWebsite() => !IsMRNEmpty;

	ZBool CanCaptureFromCustoms() => !IsMRNEmpty && !Header.IsSent && !CanSynchronizeWithCustoms();

	protected override bool CanSendToCustoms => GetCanSendToCustoms();

	bool GetCanSendToCustoms()
	{
		var header = Header;
		return !header.IsSent && (CanSendDepartureToCustoms(header) || CanSendArrivalToCustoms(header));
	}

	bool CanSendDepartureToCustoms(NctsHeader header) =>
		header.IsDepartureMovement &&
		!header.MovementHeader.IsPhaseStatusTNN;

	bool CanSendArrivalToCustoms(NctsHeader header) =>
		header.IsArrivalMovement &&
		!nonSendableArrivalCustomsStatuses.Contains(header.ArrivalMovementHeader.BM_CustomsStatus);

	protected override bool CanMakeArrivalNotification =>
		Header.IsDepartureMovement &&
		(Header.MovementHeader.BM_CustomsStatus == ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit
			|| Header.MovementHeader.BM_CustomsStatus == ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl
			|| Header.MovementHeader.BM_CustomsStatus == ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance);

	ZBool CanSynchronizeWithCustoms() => Header.BH_HeaderType == NctsMovementType.Codes.Departure && Header.MovementHeader.IsCustomsStatusPRE;

	protected override bool CanImportEntryLines => CanImportLines();

	protected override bool CanImportInvoiceLines => CanImportLines();

	bool CanImportLines() => IsDepartureAndNotSNT && (Header.MovementHeader.IsCustomsStatusPRE || Header.MovementHeader.BM_CustomsStatus.IsEmpty);

	ZBool CanMakeTNN() => IsOnlyArrivalHeader
						&& Header.ArrivalMovementHeader.BM_CustomsStatus.IsEmpty
						&& !Header.ESNctsHeader.CEN_TNNArrival;

	ZBool CanLoadDataForUnloading() => IsOnlyArrivalHeader
									&& !IsMRNEmpty
									&& !Header.IsSent
									&& (Header.ArrivalMovementHeader.BM_Phase == ESNctsMovementHeaderTransactionStatusList.Codes.Arrival
									|| Header.ArrivalMovementHeader.BM_CustomsStatus == ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted
									|| Header.ArrivalMovementHeader.BM_CustomsStatus == ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease
									|| Header.ArrivalMovementHeader.BM_CustomsStatus == ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease);

	ZBool CanCheckForInboxNotifications() => Header.IsDepartureMovement
											&& !IsMRNEmpty
											&& IsCustomsStatusDifferentFromCanMrnInv;

	ZBool CanDownloadTAD() => Header.BH_HeaderType == NctsMovementType.Codes.Departure
							&& !IsMRNEmpty
							&& IsCustomsStatusDifferentFromCanMrnInv
							&& !Header.MovementHeader.IsCustomsStatusPRE;

	ZBool CanCreateTemporaryStorage() => IsOnlyArrivalHeader
										&& !Header.IsSent
										&& (Header.ArrivalMovementHeader.BM_Phase == ESNctsMovementHeaderTransactionStatusList.Codes.UnloadingRemarks
										 || Header.ArrivalMovementHeader.BM_Phase == ESNcts5ArrivalPhaseList.Codes.TemporaryStorageActivated);

	ZBool CanViewTemporaryStorageRegister => IsOnlyArrivalHeader && Header.ArrivalMovementHeader.BM_Phase == ESNcts5ArrivalPhaseList.Codes.TemporaryStorageActivated;

	ZBool IsDepartureAndNotSNT => Header.BH_HeaderType == NctsMovementType.Codes.Departure && !Header.IsSent;

	ZBool IsCustomsStatusDifferentFromCanMrnInv => Header.MovementHeader.BM_CustomsStatus != ESNCTS5DepartureCustomsStatusList.Codes.Cancelled
											&& Header.MovementHeader.BM_CustomsStatus != ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated
											&& Header.MovementHeader.BM_CustomsStatus != ESNCTS5DepartureCustomsStatusList.Codes.Invalidated;

	ZBool IsMRNEmpty => Header.MovementReferenceNumber.IsEmpty;

	ZBool IsOnlyArrivalHeader => Header.BH_HeaderType == NctsMovementType.Codes.Arrival;

	bool CheckBrokerBeforeSending(NctsHeader nctsHeader, out NctsMessageSendingObject messageSendingObject)
	{
		messageSendingObject = null;
		bool continueWithSend = true;
		var brokerStaff = nctsHeader.MovementHeader?.CusAgent;
		if (brokerStaff == null || CertificateHasMessageErrors())
		{
			Globals.Message.ShowError(ESNctsCommonMessagingMenuProvider.MissingOrWrongBrokerOrCertificate);
			continueWithSend = false;
		}
		else
		{
			messageSendingObject = new NctsMessageSendingObject(nctsHeader, brokerStaff);
		}
		return continueWithSend;

		ZBool CertificateHasMessageErrors()
		{
			nctsHeader.Validation.ValidateBH_CustomsProfile();
			return nctsHeader.BH_CustomsProfileInfo.HasMessageErrors();
		}
	}

	bool CheckDepartureStatusBeforeSendingForInboxNotification(NctsHeader nctsHeader)
	{
		bool continueWithSend = true;
		if (nctsHeader.MovementHeader == null || IsDepartureStatusNotExpectedInboxNotificationMessage(nctsHeader.MovementHeader.BM_CustomsStatus))
		{
			Globals.Message.ShowError(ResString.GetMultilingualString("C43129EF-0A05-42FE-9BC7-814AAE34A872", "Current NCTS status does not expect any Inbox Notification message."));
			continueWithSend = false;
		}
		return continueWithSend;

		ZBool IsDepartureStatusNotExpectedInboxNotificationMessage(ZString code) =>
			!departureInboxNotificationCustomsStatuses.Contains(code);
	}

	readonly ImmutableHashSet<ZString> departureInboxNotificationCustomsStatuses =
		new ZString[]
		{
			ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl,
			ESNCTS5DepartureCustomsStatusList.Codes.PreLodged,
			ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance,
			ESNCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit
		}.ToImmutableHashSet();

	readonly ImmutableHashSet<ZString> nonSendableArrivalCustomsStatuses =
		new ZString[]
		{
			ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl,
			ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease,
			ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease
		}.ToImmutableHashSet();

	readonly ImmutableHashSet<ZString> sendableDepartureCustomsStatuses =
		new ZString[]
		{
			string.Empty,
			ESNCTS5DepartureCustomsStatusList.Codes.PreLodged,
			ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl,
			ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance
		}.ToImmutableHashSet();

	readonly ImmutableHashSet<ZString> sendableArrivalCustomsStatuses =
		new ZString[]
		{
			string.Empty,
			ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted,
		}.ToImmutableHashSet();
}
