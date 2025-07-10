using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.GUI;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Customs.ES.NCTS.Business.ESNctsMessageSender;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;

namespace Enterprise.Customs.ES.NCTS.GUI;

public class ESNctsCommonMessagingMenuProvider
{
	public ESNctsCommonMessagingMenuProvider(NctsHeader header)
		: this (null, header, SendingType.None, null)
	{
	}

	public ESNctsCommonMessagingMenuProvider(NctsMessageFunctionSet messageFunction, NctsHeader header, ZForm parentForm = null)
		: this (messageFunction, header, SendingType.None, parentForm)
	{
	}

	public ESNctsCommonMessagingMenuProvider(NctsHeader header, SendingType sendingType = SendingType.None, ZForm parentForm = null)
		: this (null, header, sendingType, parentForm)
	{
	}

	ESNctsCommonMessagingMenuProvider(NctsMessageFunctionSet messageFunction, NctsHeader header, SendingType sendingType, ZForm parentForm)
	{
		this.messageFunction = messageFunction;
		this.header = header;
		this.sendingType = sendingType;
		this.parentForm = parentForm;
		isArrival = header.IsArrivalMovement;
	}

	readonly NctsMessageFunctionSet messageFunction;
	readonly NctsHeader header;
	readonly SendingType sendingType;
	readonly ZForm parentForm;
	readonly ZBool isArrival;

	public void ESSendMessageToNcts(Func<NctsHeaderMessageSendingObjectParent, bool> identifyMessagesToSend = null)
	{
		var brokerStaff = isArrival ? header.ArrivalMovementHeader.CusAgent : header.MovementHeader.CusAgent;
		if (brokerStaff == null || HasInvalidCertificate())
		{
			Globals.Message.ShowError(MissingOrWrongBrokerOrCertificate);
		}
		else
		{
			var factory = new BusinessObjectFactory();
			var newFactoryHeader = factory.Load<NctsHeader>(header.PK);
			newFactoryHeader.Reload();

			var newFactoryHeaderForSequence = isArrival ? (newFactoryHeader.ArrivalMovementHeader.HeaderTNN ?? newFactoryHeader) : newFactoryHeader;

			EU.NCTS.Business.NctsHeaderDeclarationGoodsItemNumbersHelper.AssignUnassignedDeclarationGoodsItemNumbers(newFactoryHeaderForSequence);
			NctsHeaderDocumentsSequenceNumberHelper.AssignSupportingDocumentsSequenceNumbers(newFactoryHeaderForSequence);
			NctsHeaderDocumentsSequenceNumberHelper.AssignAdditionalDocumentsSequenceNumbers(newFactoryHeaderForSequence);

			var messageSendingObject = new NctsMessageSendingObject(newFactoryHeader, brokerStaff);
			messageSendingObject.ShouldEditMessage = GetShouldEditMessagePopUpResponse(DialogResult.No);

			var decWrapper = new NctsHeaderMessageSendingObjectParent(messageSendingObject, sendingType, messageFunction);

			var continueWithSend = identifyMessagesToSend is not null
				? identifyMessagesToSend(decWrapper)
				: ShowMessageSendingForm(decWrapper);

			if (continueWithSend)
			{
				foreach (var sendingObject in decWrapper.SendingObjectsCollection)
				{
					if (sendingObject is NctsHeaderMessageSendingObject headerSendingObject)
					{
						var (canSend, message) = CanSendNctsHeaderMessage(headerSendingObject, messageSendingObject);
						if (!canSend)
						{
							Globals.Message.ShowError(message);
							return;
						}
					}
				}

				var messageSender = new ESNctsMessageSender(decWrapper);
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

					var result = SendAndSaveNctsHeader(factory, messageBuildersData);
					header.Reload();

					if (!result.IsEmpty)
					{
						Globals.Message.Show(result);
					}

					if (parentForm is NctsMovementForm nctsMovementForm)
					{
						nctsMovementForm.ShowMovementTabs();
					}
					else if (parentForm is ShipmentForm shipmentForm)
					{
						((NctsUserControlForPlugin)shipmentForm.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.NctsMovementController).UserControl).ShowMovementTabs();
					}
					else if (parentForm is Phase5ArrivalMovementForm phase5ArrivalMovementForm)
					{
						phase5ArrivalMovementForm.ToggleTNNTabEditableState();
					}
				}
			}
		}
	}

	(bool CanSend, string Message) CanSendNctsHeaderMessage(NctsHeaderMessageSendingObject headerSendingObject, NctsMessageSendingObject messageSendingObject)
	{
		if (headerSendingObject.NctsHeader.IsPhase5Departure)
		{
			return (string)headerSendingObject.MessageType switch
			{
				DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration => CanSendPreDeclaration(headerSendingObject.NctsHeader),
				DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment => CanSendAmendment(headerSendingObject.NctsHeader),
				DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation => CanSendCancellation(headerSendingObject.NctsHeader),
				DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes => CanSendAnnexes(headerSendingObject.NctsHeader),
				DeclarationMessageTypeList.Codes.Ncts5DepartureNotification => CanSendNotificationOfGoods(headerSendingObject.NctsHeader, messageSendingObject),
				_ => (true, null)
			};
		}
		else if (headerSendingObject.NctsHeader.IsPhase5Arrival)
		{
			return (string)headerSendingObject.MessageType switch
			{
				DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods => CanSendUnloadingRemarks(headerSendingObject.NctsHeader),
				DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration => CanSendTNN(headerSendingObject.NctsHeader),
				_ => (true, null)
			};
		}
		else
		{
			return (true, null);
		}

		bool IsDepartureAndNotSNT(NctsHeader nctsHeader) => header.BH_HeaderType == EU.NCTS.Business.NctsMovementType.Codes.Departure && !nctsHeader.IsSent;

		bool IsDepartureNotSNTAndCustomsStatusPRE(NctsHeader nctsHeader) => IsDepartureAndNotSNT(nctsHeader) && nctsHeader.MovementHeader.IsCustomsStatusPRE;

		(bool CanSend, string Message) CanSendPreDeclaration(NctsHeader nctsHeader) =>
			IsDepartureAndNotSNT(nctsHeader) && nctsHeader.MovementHeader.BM_CustomsStatus.IsEmpty && !nctsHeader.MovementHeader.IsPhaseStatusTNN
			? (true, null)
			: (false, CannotSendPreDeclarationMessage);

		(bool CanSend, string Message) CanSendAmendment(NctsHeader nctsHeader) =>
			IsDepartureNotSNTAndCustomsStatusPRE(nctsHeader)
			? (true, null)
			: (false, CannotSendAmendmentMessage);

		(bool CanSend, string Message) CanSendCancellation(NctsHeader nctsHeader) =>
			IsDepartureAndNotSNT(nctsHeader)
			&& (nctsHeader.MovementHeader.IsCustomsStatusPRE
				|| nctsHeader.MovementHeader.BM_CustomsStatus == ESNCTS5DepartureCustomsStatusList.Codes.DeclarationPendingOnEuGuaranteeAcceptance)
			? (true, null)
			: (false, CannotSendCancellationMessage);

		(bool CanSend, string Message) CanSendAnnexes(NctsHeader nctsHeader) =>
			IsDepartureAndNotSNT(nctsHeader) && nctsHeader.MovementHeader.BM_CustomsStatus == ESNCTS5DepartureCustomsStatusList.Codes.DecisionToControl
			? (true, null)
			: (false, CannotSendAnnexesMessage);

		(bool CanSend, string Message) CanSendNotificationOfGoods(NctsHeader nctsHeader, NctsMessageSendingObject messageSendingObject)
		{
			if (!IsDepartureNotSNTAndCustomsStatusPRE(nctsHeader))
			{
				return (false, CannotSendNotificationOfGoodsMessage);
			}

			var changesToPreDeclaration = nctsHeader.CheckPreDeclarationChanges(messageSendingObject);
			if (!changesToPreDeclaration.IsEmpty)
			{
				return (false, ChangesInPreDeclarationMessage(changesToPreDeclaration));
			}

			return (true, null);
		}

		bool IsOnlyArrivalHeader(NctsHeader nctsHeader) => nctsHeader.BH_HeaderType == EU.NCTS.Business.NctsMovementType.Codes.Arrival;
		bool IsMRNEmpty(NctsHeader nctsHeader) => nctsHeader.MovementReferenceNumber.IsEmpty;

		(bool CanSend, string Message) CanSendUnloadingRemarks(NctsHeader nctsHeader) =>
			IsOnlyArrivalHeader(nctsHeader)
			&& !IsMRNEmpty(nctsHeader)
			&& !nctsHeader.IsSent
			&& nctsHeader.ArrivalMovementHeader.BM_CustomsStatus == ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted
			? (true, null)
			: (false, CannotSendUnloadingRemarksMessage);

		(bool CanSend, string Message) CanSendTNN(NctsHeader nctsHeader) =>
			IsOnlyArrivalHeader(nctsHeader)
			&& nctsHeader.ESNctsHeader.CEN_TNNArrival
			&& nctsHeader.ArrivalMovementHeader.HeaderTNN != null
			&& nctsHeader.ArrivalMovementHeader.HeaderTNN.MovementHeader.BM_CustomsStatus != ESNCTS5DepartureCustomsStatusList.Codes.MrnAllocated
			? (true, null)
			: (false, CannotSendTNNMessage);
	}

	public static string ChangesInPreDeclarationMessage(ZString changesToPreDeclaration) =>
		Res.GetString(
			"CF4231E9-D8D2-4585-A6CC-00C2800A2718",
			@"There are changes in the Pre-declaration which have not been submitted to Customs. Please, send an Amendment if the changes are correct or use the menu option ‘Synchronize with Customs‘ to get declared data from Customs.
There are changes in:
{0}", changesToPreDeclaration);

	public static string CannotSendPreDeclarationMessage =>
		Res.GetString(
			"DAFBC9D9-8154-4789-BBDB-5CFBAC207577",
			"Cannot send Pre-Declaration. Departure declaration must not be sent already, must have empty Customs Status, and not be in TNN phase.");

	public static string CannotSendAmendmentMessage =>
		Res.GetString(
			"92C47516-1166-4B55-B6EF-DD4B8D975AB9",
			"Cannot send Amendment. Departure declaration must not be sent already, and must have Pre-Lodged Customs Status.");

	public static string CannotSendCancellationMessage =>
		Res.GetString(
			"2BD14864-3CC7-40B4-A2EE-C1EA06031545",
			"Cannot send Cancellation. Departure declaration must not be sent already, and must have Pre-Lodged or Pending Acceptance Customs Status.");

	public static string CannotSendAnnexesMessage =>
		Res.GetString(
			"B2856B4E-25D7-4765-BF1B-8F60303BDCCF",
			"Cannot send Annexes. Departure declaration must not be sent already, and must have Decision to Control Customs Status.");

	public static string CannotSendNotificationOfGoodsMessage =>
		Res.GetString(
			"9B12C877-7E50-435D-815C-01D0A3F802A3",
			"Cannot send Notification of Goods. Departure declaration must not be sent already, and must have Pre-Lodged Customs Status.");

	public static string CannotSendUnloadingRemarksMessage =>
		Res.GetString(
			"29935248-0D6F-47B3-ABEC-4AD49228A363",
			"Cannot send Unloading Remarks. Arrival declaration must not be sent already, must have Unload Permission Granted Customs Status, and must have MRN.");

	public static string CannotSendTNNMessage =>
		Res.GetString(
			"9CF1F1B1-2B7D-46A2-B6BA-1417002CCA21",
			"Cannot send TNN. Arrival declaration must have linked TNN Departure that is not in MRN Allocated Customs Status.");

	List<MessageBuilderData> InventoryManagementAction(BusinessObjectFactory factory, List<MessageBuilderData> messageBuildersData)
	{
		var messageBuildersDataToContinue = new List<MessageBuilderData>();
		messageBuildersDataToContinue.AddRange(messageBuildersData);

		foreach (var builderData in messageBuildersData)
		{
			var messageBuilder = builderData.MessageBuilder;
			var nctsHeader = builderData.NctsHeader;
			var departureMovementHeader = nctsHeader.MovementHeader;
			var goodsLocation = departureMovementHeader?.GoodsLocation?.CGL_AdditionalIdentifier ?? ZString.Empty;

			if (messageBuilder.MessageSubType == DeclarationMessageSubTypeList.Codes.OriginalDeclaration
				&& InventoryManagementActionMessageTypesForDeparture.Contains(messageBuilder.MessageType)
				&& !goodsLocation.IsEmpty
				&& EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(nctsHeader.CountryCode)
				&& EU.Business.TemporaryStorageHelper.IsLocationManagedInPremises(factory, goodsLocation, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse))
			{
				var (errorMessage, errorMessageForVINs, dataToReserve) = EU.Business.TemporaryStorageHelper.GetDataToReserveTemporaryStorageGoods(factory, departureMovementHeader.TemporaryStorageTransactionInternalReferenceNumber, departureMovementHeader.TemporaryStorageTransactionInternalReferenceType, nctsHeader.MovementHeader.BM_PaperlessInbondNum, ES.Business.PreviousDocumentHelper.PreviousDocumentCodeN337, goodsLocation, departureMovementHeader.GetGoodsItemDataDeclaredForDepartureToReserveTSGoods, useCSI_ItemNumber: true, formatDocRef: DocumentHelper.GetDsdtMRNNumberFormat);

				if (!errorMessage.IsEmpty)
				{
					messageBuildersDataToContinue = ReserveTemporaryStorageGoodsWhenErrors(departureMovementHeader, errorMessage, dataToReserve, builderData, messageBuildersDataToContinue);
				}
				else if (!errorMessageForVINs.IsEmpty)
				{
					messageBuildersDataToContinue = ReserveTemporaryStorageGoodsWhenErrors(departureMovementHeader, errorMessageForVINs, dataToReserve, builderData, messageBuildersDataToContinue);
				}
				else
				{
					EU.Business.TemporaryStorageHelper.ReserveTemporaryStorageGoods(departureMovementHeader.TemporaryStorageTransactionInternalReferenceNumber, departureMovementHeader.TemporaryStorageTransactionInternalReferenceType, dataToReserve);
				}
			}
		}

		return messageBuildersDataToContinue;
	}

	List<MessageBuilderData> ReserveTemporaryStorageGoodsWhenErrors(NctsDepartureMovementHeader departureMovementHeader, ZString errorMessage, IEnumerable<DataToReserveTSGoods> dataToReserve, MessageBuilderData builderData, List<MessageBuilderData> messageBuildersDataToContinue)
	{
		var erroMessageTextToAsk = Res.GetString("6A53D971-92B5-4C92-84F3-FFD57554E9A7", "Do you want to cancel this declaration to check?");
		var erroMessageTextToAskForVINs = Res.GetString("7447BB97-D3E0-4724-A3AE-68552B6E8651", "Would you like to cancel this action and check the gross weight declared for the vehicles?");

		if (errorMessage.Contains(erroMessageTextToAsk) || errorMessage.Contains(erroMessageTextToAskForVINs))
		{
			var result = Globals.Message.Show(
							errorMessage,
							Res.GetString("FC8A63DE-8C97-453A-9698-E28CE2D39624", "Temporary Storage Management"),
							MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.No;
			if (result)
			{
				EU.Business.TemporaryStorageHelper.ReserveTemporaryStorageGoods(departureMovementHeader.TemporaryStorageTransactionInternalReferenceNumber, departureMovementHeader.TemporaryStorageTransactionInternalReferenceType, dataToReserve);
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

	ZString[] InventoryManagementActionMessageTypesForDeparture => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5Departure, DeclarationMessageTypeList.Codes.Ncts5DepartureNotification };

	ZString SendAndSaveNctsHeader(BusinessObjectFactory factory, List<MessageBuilderData> messageBuilders)
	{
		MessagesInfo messagesInfo = null;
		try
		{
			messagesInfo = ESNctsMessageSender.Send(messageBuilders);
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

	static ZString GetMessageSendSuccessful(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{6B409AEF-83B9-4C22-B922-922B2B8CEA6A}", "{0} Messages sent successfully.", numberOfMessages) : ResString.GetMultilingualString("C484AD22-65E8-4461-9DA0-AAD668869286", "Message sent successfully.");

	static ZString GetGetMessageSendFailure(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{417143E3-2D38-489D-A18A-A5EA3FA87B5F}", "Failed to send {0} messages.", numberOfMessages) : ResString.GetMultilingualString("7EDD83A4-39E9-42C9-BF85-692F13000E8C", "Failed to send message.");

	static ZString GetMessageCreateFailure(int numberOfMessages) => numberOfMessages > 1 ? Res.GetString("{91491845-D6C0-43A7-9233-BF8E5EC235E7}", "Failed to create {0} messages.", numberOfMessages) : ResString.GetMultilingualString("F8AB3010-1E33-417D-ACD1-D56CC4AE445B", "Failed to create message.");

	protected virtual MessageEditForm GetMessageEditForm() => new MessageEditForm();

	protected virtual bool GetShouldEditMessagePopUpResponse(DialogResult defaultValue) => MessageEditHelper.GetShouldEditMessagePopUpResponse(defaultValue);

	bool ShowMessageSendingForm(NctsHeaderMessageSendingObjectParent decWrapper)
	{
		using (var form = GetMessageSendingForm(decWrapper))
		{
			return ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
		}
	}

	protected virtual MessageSendingForm GetMessageSendingForm(NctsHeaderMessageSendingObjectParent decWrapper) => new MessageSendingForm(decWrapper);

	protected virtual bool HasInvalidCertificate()
	{
		header.Validation.ValidateBH_CustomsProfile();
		return header.BH_CustomsProfileInfo.HasMessageErrors();
	}

	public void LaunchCustomsWebsite()
	{
		var linkWebPage = header.GetUrlToLaunch();
		if (!string.IsNullOrEmpty(linkWebPage))
		{
			WebUrlLauncher.Launch(linkWebPage);
		}
	}

	public void DownloadTAD()
	{
		if (CheckBeforeSendingMissingDocuments(out var messageSendingObject))
		{
			SendAndSaveRequestMissingDocuments(((ICertificateProvider)messageSendingObject).CertificateName);
		}
	}

	void SendAndSaveRequestMissingDocuments(ZString certificateName)
	{
		var factory = new BusinessObjectFactory();
		var newFactoryHeader = factory.Load<NctsHeader>(header.PK);
		if (newFactoryHeader != null)
		{
			newFactoryHeader.Reload();

			if (!newFactoryHeader.MovementReferenceNumber.IsEmpty && !certificateName.IsEmpty)
			{
				var nctsDepartureDocRequest = new NCTSDepartureDocumentRequest(newFactoryHeader, certificateName);
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

				Globals.Message.Show(GetMessageDocumentCaptureSuccessful(messagesSentCount));
			}
		}
	}

	public bool CheckBeforeSendingMissingDocuments(out NctsMessageSendingObject messageSendingObject)
	{
		messageSendingObject = null;
		var continueWithSend = true;

		var broker = isArrival ? header.ArrivalMovementHeader.CusAgent : header.MovementHeader.CusAgent;
		if (broker == null || HasInvalidCertificate())
		{
			Globals.Message.ShowError(MissingOrWrongBrokerOrCertificate);
			continueWithSend = false;
		}
		else
		{
			messageSendingObject = new NctsMessageSendingObject(header, broker);
		}
		return continueWithSend;
	}

	public static ZString GetMessageDocumentCaptureSuccessful(int messagesSentCount) => ResString.GetMultilingualString("23E728FA-C93E-47E0-8E19-A9BD9A93D8E3", "{0} Document Capture request(s) created", messagesSentCount.ToString(Culture.Current));

	public static ZString MissingOrWrongBrokerOrCertificate => ResString.GetMultilingualString("EE1484A3-289D-4EAF-91AB-B3581B0AE958", "Cannot send message without a representative and valid certificate; please enter the representative and a valid certificate.");

	public void CreateEXSDeclaration()
	{
		var factory = new BusinessObjectFactory();
		var newFactoryHeader = factory.Load<NctsHeader>(header.PK);
		newFactoryHeader.Reload();

		var declarationPK = ZGuid.Empty;
		var referenceCreated = ZString.Empty;
		if (newFactoryHeader.CanCreateExsDeclaration())
		{
			declarationPK = newFactoryHeader.CreateEXSFromArrival();
		}

		try
		{
			if (!declarationPK.IsEmpty)
			{
				factory.Save();
				referenceCreated = factory.Load<JobDeclaration>(declarationPK).JE_DeclarationReference;
			}
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ZExceptionReporting.HandleSaveException(ex);
		}
		var notCreateDeclarationMessage = Res.GetString("BE329A14-1F5E-4739-B1FF-C398081CD66B", "EXS custom declaration could not be created. Please check good items data entered in NCTS Arrival declaration.");
		var successfulMessage = Res.GetString("5BCBED77-B727-4132-A835-BC2E14437DA0", "EXS custom declaration with number {0} has been successfully created.", referenceCreated);

		Globals.Message.Show(!declarationPK.IsEmpty && !referenceCreated.IsEmpty ? successfulMessage : notCreateDeclarationMessage);
	}
}
