using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.GUI;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.TemporaryStorage.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.TemporaryStorage.Business.G5MessageSender;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using CsvCodeInfo = Enterprise.Customs.ES.TemporaryStorage.Business.CsvCodeInfo;
using TemporaryStorageHeader = Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader;
using TemporaryStorageHelper = Enterprise.Customs.EU.Business.TemporaryStorageHelper;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public class G5V1TemporaryStorageMessagesMenu : EU.TemporaryStorage.GUI.TemporaryStorageMessagesMenu
{
	readonly ZForm parentForm;

	public G5V1TemporaryStorageMessagesMenu(ZForm parentForm) : base(parentForm)
	{
		this.Name = nameof(G5V1TemporaryStorageMessagesMenu);
		this.parentForm = parentForm;
	}

	public new TemporaryStorageHeader Header => (TemporaryStorageHeader)base.Header;

	protected override IEnumerable<ZMenuItem> CreateMenuItems()
	{
		foreach (var menuItem in base.CreateMenuItems())
		{
			yield return menuItem;
		}
		yield return ViewOnCustomsWebsite;
		yield return UpdateCSVClearance;
		yield return MakeG5V1Reception;
		yield return RollbackInboundTransaction;
		yield return IntoTemporaryStorageLine;
		yield return IntoTemporaryStorage;
		yield return ViewTSRegister;
	}

	ZMenuItem ViewOnCustomsWebsite => viewOnCustomsWebsite ??= new ZMenuItem(ResString.GetMultilingualString("43124016-CA0E-431E-B5E6-23D32DFF8004", "View on Customs Website"), ViewOnCustomsWebsiteClick);
	ZMenuItem viewOnCustomsWebsite;

	ZMenuItem MakeG5V1Reception => makeG5V1Reception ??= new ZMenuItem(ResString.GetMultilingualString("9239D6D4-CFFB-42C0-965F-36E7CF641738", "Make G5 Reception"), MakeG5V1ReceptionClick);
	ZMenuItem makeG5V1Reception;

	ZMenuItem IntoTemporaryStorageLine => intoTemporaryStorageLine ??= new ZMenuItem("-");
	ZMenuItem intoTemporaryStorageLine;

	ZMenuItem IntoTemporaryStorage => intoTemporaryStorage ??= new ZMenuItem(ResString.GetMultilingualString("2DE751A2-12D8-43DB-B231-7CF39C89A8F1", "Into Temporary Storage"), IntoTemporaryStorageClick);
	ZMenuItem intoTemporaryStorage;

	ZMenuItem UpdateCSVClearance => updateCSVClearance ??= new ZMenuItem(ResString.GetMultilingualString("3de868b3-72cd-4f66-befe-cbad658af3ca", "Update CSV Clearance"), UpdateCSVClearanceClick);
	ZMenuItem updateCSVClearance;

	ZMenuItem RollbackInboundTransaction => rollbackInboundTransaction ??= new ZMenuItem(ResString.GetMultilingualString("E997169E-D0A0-4816-BE46-E1530D7A92BD", "Roll-back Inbound Transaction in TS"), RollbackInboundTransactionClick);
	ZMenuItem rollbackInboundTransaction;

	ZMenuItem ViewTSRegister => viewTSRegister ??= new ZMenuItem(ResString.GetMultilingualString("8E53F635-C88D-4523-9165-0BC6F898B0C2", "View TS Register"), ViewTSRegisterClick);
	ZMenuItem viewTSRegister;

	void ViewTSRegisterClick(object sender, EventArgs ev)
	{
		var authNumber = Header.DestinationGoodsLocation.Address.AuthorisationNumber;
		var factory = Header.Factory;

		if (Header.IsMessageTypeG5V1Reception)
		{
			processViewOfTSRegister(Header.DsdtMrnNumberSdFormat);
		}

		if (Header.IsMessageTypeTSM)
		{
			processViewOfTSRegister(Header.DsdtMrnNumber);
		}

		if (Header.IsMessageTypeLAM)
		{
			processViewOfTSRegister(Header.EntryNumber);
		}

		void processViewOfTSRegister(ZString entryNumber)
		{
			var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, entryNumber);
			var regHeader = factory.Load<ES.Business.CusTempStorage.CusTempStorageRegHeader>(query).FirstOrDefault();
			if (regHeader == null)
			{
				ReturnErrorMessage(entryNumber);
			}
			else if (regHeader.Premises.SRP_CustomsLocation != authNumber)
			{
				ReturnErrorMessage(entryNumber);
			}
			else
			{
				ZFormModaliser.ShowDialogAndDispose(new TempStorageRegisterForm(regHeader)
				{
					ControllerID = ControllerIDs.Customs.EU.TempStorageRegister,
				});
			}
		}

		void ReturnErrorMessage(ZString entryNumber)
		{
			Globals.Message.ShowError(TemporaryRegisterNotFoundMessage(entryNumber, authNumber));
		}
	}

	void ViewOnCustomsWebsiteClick(object sender, EventArgs ev)
	{
		var linkWebPage = Header.GetUrlToLaunch();
		if (!string.IsNullOrEmpty(linkWebPage))
		{
			WebUrlLauncher.Launch(linkWebPage);
		}
	}

	void MakeG5V1ReceptionClick(object sender, EventArgs ev)
	{
		if (CheckHasChanges())
		{
			var queryTempStorageReception = new ZQuery(AsycudaManifestHeaderSchema.AMA_MessageType, G5MessageTypeCodeList.Codes.G5v1Reception);
			var tempStorageReception = Header.Factory.Load<TemporaryStorageHeader>(queryTempStorageReception).Where(t => t.MRN == Header.MRN);
			if (tempStorageReception.Any())
			{
				var reception = tempStorageReception.First();
				Globals.Message.ShowError(GetMessageWhenG5V1ReceptionAlreadyExists(reception.MRN, reception.AMA_JobReference));
			}
			else
			{
				var factory = new BusinessObjectFactory();
				var newFactoryHeader = factory.Load<TemporaryStorageHeader>(Header.PK);
				newFactoryHeader.Reload();

				try
				{
					var newReception = newFactoryHeader.CreateG5V1Reception();
					factory.Save();

					if (newReception != null)
					{
						ZFormModaliser.ShowDialogAndDispose(new G5V1TemporaryStorageForm(newReception)
						{
							ControllerID = ControllerIDs.Customs.EU.UCC6TemporaryStorage
						});
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}
	}

	public ZGlobalMutex Mutex => fMutex ??= new ZGlobalMutex(MutexIDs.CusTempStorageRegHeaderMutex, Header.PK.ToString());
	ZGlobalMutex fMutex;

	void IntoTemporaryStorageClick(object sender, EventArgs ev)
	{
		void ShowMutexWarning() => Globals.Message.ShowWarning(IntoTemporaryStorageHelper.GetMutexLockText(Mutex.GetLockInfo().UserWithLock.FullName));
		if (SaveAndContinue())
		{
			if (Header.IsMessageTypeTSM && !Header.UnionGoods && Header.DsdtMrnNumber.IsEmpty)
			{
				Globals.Message.ShowError(IntoTempStorageNoDSDTNumberForTSMError);
			}
			else if (Header.IsMessageTypeLAM && Header.EntryDate.IsEmpty)
			{
				Globals.Message.ShowError(IntoTempStorageNoEntryDateForLAMError);
			}
			else
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
	}

	void IntoTemporaryStorageProcess()
	{
		var header = Header;
		var destinationGoodsLocation = header.DestinationGoodsLocation.Address.AuthorisationNumber;
		var guarantee = header.Guarantee;

		var messageTypeLAM = header.IsMessageTypeLAM;
		var isMessageTypeTSMAndIsUnionGoods = header.IsMessageTypeTSMAndIsUnionGoods;

		var entryNumber = messageTypeLAM
							? header.EntryNumber
							: header.IsMessageTypeTSM
									? header.DsdtMrnNumber
									: header.DsdtMrnNumberSdFormat;

		if (IntoTemporaryStorageHelper.GoodsLocationExistsInPremises(header.Factory, destinationGoodsLocation, messageTypeLAM) &&
			(messageTypeLAM || isMessageTypeTSMAndIsUnionGoods
				|| IntoTemporaryStorageHelper.GuaranteeAndLiabilityAmountAreDeclared(guarantee) &&
					IntoTemporaryStorageHelper.GuaranteNumberExistsAndIsValid(guarantee, guarantee.CusGuaranteeWithoutPermitHolder)) &&
			IntoTemporaryStorageHelper.AtLeastOneLineNotMissing(header) &&
			(isMessageTypeTSMAndIsUnionGoods || IntoTemporaryStorageHelper.GoodsItemsAreNotInTemporaryStorage(header.Factory, entryNumber, messageTypeLAM)) &&
			(!messageTypeLAM || IntoTemporaryStorageHelper.OnlyOneLineForLAM(header)) &&
			(messageTypeLAM || isMessageTypeTSMAndIsUnionGoods || IntoTemporaryStorageHelper.ShowWarningIfGuaranteeLiabilityAmountIsZero(guarantee)))
		{
			IntoTemporaryStorageHelper.ShowWarningIfMoreThanOnePackageLinkedToALine(header);
			CreateIntoTemporaryStorage();
		}
	}

	void UpdateCSVClearanceClick(object sender, EventArgs ev)
	{
		if (CheckHasChanges())
		{
			var header = Header;
			var csvCodeInfo = CsvCodeInfo.LoadNew(header);
			using (var updateCSVForm = GetUpdateCSVClearanceForm(csvCodeInfo))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(updateCSVForm) == DialogResult.OK)
				{
					csvCodeInfo.Validation.ValidateAll();

					if (!csvCodeInfo.CsvCodeFromUserInfo.HasErrors()
						&& !csvCodeInfo.ClearanceDateFromUserInfo.HasErrors())
					{
						header.SetClearanceNumber(csvCodeInfo.CsvCodeFromUser);
						header.SetClearanceDate(csvCodeInfo.ClearanceDateFromUser);
						header.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;
					}
					else
					{
						Globals.Message.Show(Res.GetString("BBC3D390-E958-491C-BB7A-F577EA9023B6", "Nothing was updated because there were errors"));
					}
				}
			}
		}
	}

	protected virtual UpdateCSVClearanceForm GetUpdateCSVClearanceForm(CsvCodeInfo csvCodeInfo) => new UpdateCSVClearanceForm(csvCodeInfo);

	bool NoTransactionExistsWithSameReferenceInGuarantee(TemporaryStorageHeader header) => !header.Guarantee.CusGuarantee.GetTransactions().Any(x => x.CPL_TransactionStatus == "CON" && x.CPL_Reference == header.DsdtMrnNumber);

	void CreateIntoTemporaryStorage()
	{
		var factory = new BusinessObjectFactory();
		var newFactoryHeader = factory.Load<TemporaryStorageHeader>(Header.PK);
		if (newFactoryHeader != null)
		{
			newFactoryHeader.Reload();

			try
			{
				var (result, regHeader) = newFactoryHeader.CreateTemporaryStorageData();
				factory.Save();

				if (result)
				{
					if (newFactoryHeader.IsMessageTypeLAM && newFactoryHeader.EntryNumber.IsEmpty)
					{
						newFactoryHeader.EntryNumber = regHeader.SRH_InternalReference;
						regHeader.SRH_Reference = regHeader.SRH_InternalReference;
						factory.Save();
					}

					Globals.Message.Show(ResString.GetMultilingualString("A4AF781D-295C-4A1D-AC29-6B49BA5D49BD", "Temporary Storage data created successfully."));
					Header.Reload();
					((G5V1TemporaryStorageForm)parentForm).SetReadOnlyFieldsForSentDeclaration();

					if ((newFactoryHeader.IsMessageTypeG5V1Reception && NoTransactionExistsWithSameReferenceInGuarantee(newFactoryHeader)) || (newFactoryHeader.IsMessageTypeTSM && !newFactoryHeader.UnionGoods))
					{
						var warningMessage = newFactoryHeader.Guarantee.GetWarningErrorIfRemainingBalanceIsNotEnough();
						if (!warningMessage.IsEmpty)
						{
							Globals.Message.ShowWarning(warningMessage);
						}

						if (newFactoryHeader.IsMessageTypeTSM)
						{
							newFactoryHeader.AddNewGuaranteeTransactionForTSM();
						}
						else
						{
							newFactoryHeader.AddNewGuaranteeTransactionForG5V1Reception();
						}

						factory.Save();
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}
	}

	void RollbackInboundTransactionClick(object sender, EventArgs ev)
	{
		if (SaveAndContinue())
		{
			var factory = new BusinessObjectFactory();
			var newFactoryHeader = factory.Load<TemporaryStorageHeader>(Header.PK);
			if (newFactoryHeader != null)
			{
				newFactoryHeader.Reload();

				try
				{
					var isMessageTypeTSM = newFactoryHeader.IsMessageTypeTSM;
					var entryNumber = isMessageTypeTSM ? newFactoryHeader.DsdtMrnNumber : newFactoryHeader.EntryNumber;
					var query = new ZQuery(CusTempStorageRegHeaderSchema.SRH_Reference, entryNumber);
					var regHeaderAssociated = newFactoryHeader.Factory.Load<ES.Business.CusTempStorage.CusTempStorageRegHeader>(query).FirstOrDefault();

					if (RollbackInboundTransactionRegHeaderNotExists(regHeaderAssociated, entryNumber, newFactoryHeader, factory) || RollbackInboundTransactionRegHeaderHasPendingOrConfirmedTransactions(regHeaderAssociated))
					{
						return;
					}

					RollbackInboundTransactionProcess(regHeaderAssociated, isMessageTypeTSM, newFactoryHeader, factory);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}
	}

	bool RollbackInboundTransactionRegHeaderNotExists(ES.Business.CusTempStorage.CusTempStorageRegHeader regHeaderAssociated, ZString entryNumber, TemporaryStorageHeader newFactoryHeader, BusinessObjectFactory factory)
	{
		if (entryNumber.IsEmpty || regHeaderAssociated == null)
		{
			Globals.Message.ShowError(GetRegHeaderNotFoundError(entryNumber));
			SetCustomsStatusToEmptyAndSave(newFactoryHeader, factory);
			return true;
		}
		else
		{
			return false;
		}
	}

	bool RollbackInboundTransactionRegHeaderHasPendingOrConfirmedTransactions(ES.Business.CusTempStorage.CusTempStorageRegHeader regHeaderAssociated)
	{
		if (regHeaderAssociated.CusTempStorageRegLines.Any(regLine =>
							regLine.CusTempStorageRegLineTransactions.Any(transaction =>
								transaction.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance
									&& (transaction.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Pending || transaction.SRT_TransactionStatus == CusTempStorageRegLineTransactionStatusList.Codes.Confirmed))))
		{
			Globals.Message.ShowError(RegHeaderHasTransactionsPNDOrCONError);
			return true;
		}
		else
		{
			return false;
		}
	}

	void RollbackInboundTransactionProcess(ES.Business.CusTempStorage.CusTempStorageRegHeader regHeaderAssociated, bool isMessageTypeTSM, TemporaryStorageHeader newFactoryHeader, BusinessObjectFactory factory)
	{
		if (isMessageTypeTSM)
		{
			var guarantee = regHeaderAssociated.Guarantee;
			guarantee?.CusGuarantee?.AddTransaction(regHeaderAssociated.SRH_Reference, RollbackGuaranteeTransactionComment, ZString.Empty, ZString.Empty, Math.Abs(guarantee.PW_BondAmount), ZDecimal.Zero, status: PermitTransactionStatusList.Codes.Confirmed, checkBursting: false);
		}

		regHeaderAssociated.Delete();
		SetCustomsStatusToEmptyAndSave(newFactoryHeader, factory);
		Globals.Message.Show(RollbackSuccessfullyMessage);
	}

	void SetCustomsStatusToEmptyAndSave(TemporaryStorageHeader newFactoryHeader, BusinessObjectFactory factory)
	{
		newFactoryHeader.CustomsStatus = ZString.Empty;
		factory.Save();
		Header.Reload();
	}

	protected override void RefreshMenuItems()
	{
		base.RefreshMenuItems();
		SetMenuItemVisibility(ViewOnCustomsWebsite, () => CanViewOnCustomsWebsite());
		SetMenuItemVisibility(UpdateCSVClearance, () => CanUpdateCSVClearance());
		SetMenuItemVisibility(MakeG5V1Reception, () => CanMakeG5V1Reception());
		SetMenuItemVisibility(IntoTemporaryStorageLine, () => CanCreateTemporaryStorage());
		SetMenuItemVisibility(IntoTemporaryStorage, () => CanCreateTemporaryStorage());
		SetMenuItemVisibility(SendToCustomsMenuItem, () => CanSendToCustoms());
		SetMenuItemVisibility(RollbackInboundTransaction, () => CanRollbackInboundTransaction());
		SetMenuItemVisibility(ViewTSRegister, () => CanViewTSRegister());
	}

	ZBool CanViewOnCustomsWebsite() => (Header.AMA_MessageType == G5MessageTypeCodeList.Codes.G5v1Expedition || Header.AMA_MessageType == G5MessageTypeCodeList.Codes.G5v1Reception) && !Header.MRN.IsEmpty;

	ZBool CanMakeG5V1Reception() => Header.IsMessageTypeG5V1Expedition && Header.CustomsStatus == EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance;

	ZBool CanCreateTemporaryStorage() => Header.AMA_MessageStatus != LogicalStatusList.Codes.Sent
									&& ((Header.AMA_MessageType == G5MessageTypeCodeList.Codes.G5v1Reception
											&& (Header.CustomsStatus == EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance
												|| Header.CustomsStatus == EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated))
										|| Header.IsMessageTypeTSM
										|| Header.IsMessageTypeLAM);

	ZBool CanSendToCustoms() => !Header.IsMessageTypeManual;

	ZBool CanUpdateCSVClearance() => Header.CustomsStatus == EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl;

	ZBool CanRollbackInboundTransaction() => Header.CustomsStatus == EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated
		&& (Header.IsMessageTypeTSM || Header.IsMessageTypeLAM);

	ZBool CanViewTSRegister() => Header.CustomsStatus == EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated && (Header.IsMessageTypeManual || Header.IsMessageTypeG5V1Reception);

	protected override void SendToCustomsCore(EDIMessageCollection messages, EU.Business.CusTempStorage.TemporaryStorageHeader header)
	{
		if (CheckHasChanges())
		{
			var brokerStaff = Header.CustomsAgent;
			if (brokerStaff == null || CertificateHasMessageErrors(Header))
			{
				Globals.Message.ShowError(MissingOrWrongBrokerOrCertificate);
			}
			else
			{
				var factory = new BusinessObjectFactory();
				var newFactoryHeader = factory.Load<TemporaryStorageHeader>(Header.PK);
				newFactoryHeader.Reload();

				var messageSendingObject = new G5MessageSendingObject(newFactoryHeader, brokerStaff);
				messageSendingObject.ShouldEditMessage = MessageEditHelper.GetShouldEditMessagePopUpResponse(DialogResult.No);

				var decWrapper = new G5TemporaryStorageMessageSendingObjectParent(messageSendingObject);

				var continueWithSend = ShowMessageSendingForm(decWrapper);

				if (continueWithSend)
				{
					var messageSender = new G5MessageSender(decWrapper);
					var messageBuildersData = messageSender.GetMessageBuildersData();

					if (messageSendingObject.ShouldEditMessage)
					{
						foreach (var messageBuilderData in messageBuildersData)
						{
							var messageBuilder = messageBuilderData.MessageBuilder;
							var messageText = messageBuilder.UnsignedMessageText;
							using (var editForm = GetMessageEditForm())
							{
								(messageText, continueWithSend) = editForm.EditMessage(messageText);
							}
							messageBuilder.UnsignedMessageText = messageText;
						}
					}

					if (continueWithSend)
					{
						messageBuildersData = InventoryManagementAction(factory, messageBuildersData);

						var result = SendAndSaveHeader(factory, messageBuildersData);
						header.Reload();
						((G5V1TemporaryStorageForm)parentForm).SetReadOnlyFieldsForSentDeclaration();

						if (!result.IsEmpty)
						{
							Globals.Message.Show(result);
						}
					}
				}
			}
		}
	}

	List<MessageBuilderData> InventoryManagementAction(BusinessObjectFactory factory, List<MessageBuilderData> messageBuildersData)
	{
		var messageBuildersDataToContinue = new List<MessageBuilderData>();
		messageBuildersDataToContinue.AddRange(messageBuildersData);

		foreach (var builderData in messageBuildersData)
		{
			var messageBuilder = builderData.MessageBuilder;
			var header = builderData.Header;
			var goodsLocation = header.GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty;

			if (messageBuilder.MessageSubType == DeclarationMessageSubTypeList.Codes.OriginalDeclaration
				&& InventoryManagementActionMessageTypesForG5.Contains(messageBuilder.MessageType)
				&& !goodsLocation.IsEmpty
				&& TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(header.CountryCode)
				&& TemporaryStorageHelper.IsLocationManagedInPremises(factory, goodsLocation, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse))
			{
				var (errorMessage, errorMessageForVINs, dataToReserve) = TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(factory, header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, header.LRN, ES.Business.PreviousDocumentHelper.PreviousDocumentCode337, goodsLocation, header.GetEntryLineDataDeclaredToReserveTSGoods, jobNumber: header.AMA_JobReference, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

				if (!errorMessage.IsEmpty)
				{
					messageBuildersDataToContinue = ReserveTemporaryStorageGoodsWhenErrors(header, errorMessage, dataToReserve, builderData, messageBuildersDataToContinue);
				}
				else if (!errorMessageForVINs.IsEmpty)
				{
					messageBuildersDataToContinue = ReserveTemporaryStorageGoodsWhenErrors(header, errorMessageForVINs, dataToReserve, builderData, messageBuildersDataToContinue);
				}
				else
				{
					TemporaryStorageHelper.ReserveTemporaryStorageGoods(header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, dataToReserve);
				}
			}
		}

		return messageBuildersDataToContinue;
	}

	List<MessageBuilderData> ReserveTemporaryStorageGoodsWhenErrors(TemporaryStorageHeader header, ZString errorMessage, IEnumerable<DataToReserveTSGoods> dataToReserve, MessageBuilderData builderData, List<MessageBuilderData> messageBuildersDataToContinue)
	{
		var erroMessageTextToAsk = Res.GetString("D6FBBBF5-B8A1-493F-BE1C-F726C9E7EB98", "Do you want to cancel this declaration to check?");
		var erroMessageTextToAskForVINs = Res.GetString("0616EA03-24D1-4262-BF1C-591499BEDCEF", "Would you like to cancel this action and check the gross weight declared for the vehicles?");

		if (errorMessage.Contains(erroMessageTextToAsk) || errorMessage.Contains(erroMessageTextToAskForVINs))
		{
			var result = Globals.Message.Show(
							errorMessage,
							Res.GetString("4DFCA909-CEDC-48BE-97B8-A6C287BA04BC", "Temporary Storage Management"),
							MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.No;
			if (result)
			{
				TemporaryStorageHelper.ReserveTemporaryStorageGoods(header.TemporaryStorageTransactionInternalReferenceNumber, header.TemporaryStorageTransactionInternalReferenceType, dataToReserve);
			}
			else
			{
				messageBuildersDataToContinue.Remove(builderData);
			}
		}
		else
		{
			messageBuildersDataToContinue.Remove(builderData);
			Globals.Message.Show(errorMessage);
		}

		return messageBuildersDataToContinue;
	}

	ZString[] InventoryManagementActionMessageTypesForG5 => new ZString[] { DeclarationMessageTypeList.Codes.G5v1Expedition };

	protected virtual MessageEditForm GetMessageEditForm() => new MessageEditForm();

	protected bool CheckHasChanges()
	{
		var topLevelBusinessObject = (BusinessObject)Header;
		return CustomsPlugIn.FormPreSaved(topLevelBusinessObject, (ZForm)GetMainMenu()?.GetForm());
	}

	bool CertificateHasMessageErrors(TemporaryStorageHeader esHeader)
	{
		esHeader.Validation.ValidateAMA_CustomsProfile();
		return esHeader.AMA_CustomsProfileInfo.HasMessageErrors();
	}

	bool ShowMessageSendingForm(G5TemporaryStorageMessageSendingObjectParent decWrapper)
	{
		using (var form = GetNewMessageSendingForm(decWrapper))
		{
			return ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
		}
	}

	ZString SendAndSaveHeader(BusinessObjectFactory factory, List<MessageBuilderData> messageBuildersData)
	{
		MessagesInfo messagesInfo = null;
		try
		{
			messagesInfo = G5MessageSender.Send(messageBuildersData);
			factory.Save();
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
			messagesInfo.MessagesWithCreateFailure++;
		}

		return SetResultMessage(messagesInfo);
	}

	ZString SetResultMessage(MessagesInfo messagesInfo)
	{
		var result = new ZString();

		if (messagesInfo.MessagesSent > 0)
		{
			result = GetMessageSendSuccessful(messagesInfo.MessagesSent);
		}
		if (messagesInfo.MessagesWithCreateFailure > 0)
		{
			result += System.Environment.NewLine + GetMessageCreateFailure(messagesInfo.MessagesWithCreateFailure);
		}
		if (messagesInfo.MessagesWithSendFailure > 0)
		{
			result += System.Environment.NewLine + GetGetMessageSendFailure(messagesInfo.MessagesWithSendFailure);
		}
		return result;
	}

	static ZString GetMessageSendSuccessful(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("49C2F5AB-FE90-4A16-81D5-820CE25FDE6D", "{0} Messages sent successfully.", numberOfMessages) : Res.GetString("7FED57DF-138F-4EB9-81CB-3BEFE25EEC79", "{0} Message sent successfully.", numberOfMessages);

	static ZString GetGetMessageSendFailure(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("83A65781-B575-44B3-8EE7-B0185428B953", "Failed to send {0} messages.", numberOfMessages) : Res.GetString("69AA4E7C-56E6-499D-B49C-CFC168C25210", "Failed to send {0} message.", numberOfMessages);

	static ZString GetMessageCreateFailure(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("93062C87-CEBC-452F-9794-3726FE2E8C24", "Failed to create {0} messages.", numberOfMessages) : Res.GetString("4097E796-E228-47EE-B482-29CD6C8E8142", "Failed to create {0} message.", numberOfMessages);

	static ZString GetMessageWhenG5V1ReceptionAlreadyExists(ZString mrn, ZString jobNumber) => ResString.GetMultilingualString("458B8566-410C-4FE2-8F7A-17E723977731", "The {0} MRN is already registered at {1} G5 Reception.", mrn, jobNumber);

	static ZString MissingOrWrongBrokerOrCertificate => ResString.GetMultilingualString("B12BAD8B-C645-44A4-8892-D20F4189A4FA", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.");

	static ZString IntoTempStorageNoDSDTNumberForTSMError => ResString.GetMultilingualString("51A7C2E4-E34C-4AF0-978C-3ADBBC81432E", "To enter goods into the Temporary Storage, DSDT number must be supplied.");

	static ZString IntoTempStorageNoEntryDateForLAMError => ResString.GetMultilingualString("1555B651-7C94-4EE3-842A-47009EC596E6", "To enter goods into the LAME, Entry Date must be supplied.");

	static ZString GetRegHeaderNotFoundError(ZString entryNumber) => Res.GetString("0547050C-FB95-461A-9D87-602F9CE0B19E", "No Temporary Storage Register Entry has been found with Job Reference ({0}). The Status of this record will be cleared.", entryNumber);

	static ZString RegHeaderHasTransactionsPNDOrCONError => ResString.GetMultilingualString("2FD70A5E-6D9F-4B53-9BB7-819EA48779BF", "Inbound transactions cannot be rolled back when there are already transactions that represent an outbound or adjustment for those goods.");

	static ZString RollbackGuaranteeTransactionComment => ResString.GetMultilingualString("D86394A0-3BA7-4C04-A339-EB2D55FE90BF", "Roll-back inbound transaction in TS Register without any outbound");

	static ZString RollbackSuccessfullyMessage => ResString.GetMultilingualString("436AB3B7-C82C-4234-B5C3-C5536832B8CA", "Temporary Storage data deleted successfully.");

	static ZString TemporaryRegisterNotFoundMessage(ZString entryNumber, ZString authNumber) => ResString.GetMultilingualString("0BD705F2-0E4C-4BD3-A7F9-6087A3401D1B", "No entry in the Temporary Storage Register found for {0} (TSD Number) in {1} (Premise Location)", entryNumber, authNumber);
}
