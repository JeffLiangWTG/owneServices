using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.Abstractions;
using Constants = CargoWise.EventReference.Constants;
using static Enterprise.Customs.ES.Business.MessageSending.ESMessageSender;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;

namespace Enterprise.Customs.ES.Business.Declaration;

public class AutoSendCustomsMessageProcessor : Customs.Business.AutoSendCustomsMessageProcessor
{
	public AutoSendCustomsMessageProcessor(Customs.Business.BaseJobDeclaration declaration) : base(declaration)
	{
	}
	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override ZBool CanSendEntryHeader(Customs.Business.CusEntryHeader entryHeader)
	{
		return base.CanSendEntryHeader(entryHeader) && entryHeader.CH_EntryStatus.IsEmpty;
	}

	protected override ZString MessageDescription => (NoResString)"Spain Customs Declaration";

	protected override IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersToSendCore()
	{
		var notifier = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
		return Declaration.DoMerge(notifier) ? Declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>() : Enumerable.Empty<Customs.Business.CusEntryHeader>();
	}

	protected override ZBool SendCustomsMessageCore(INotifications notifications, Customs.Business.CusEntryHeader entryHeader)
	{
		var sent = false;
		var messageObject = new MessageSendingObject(Declaration, GlbStaff.CurrentUser);
		var sendingObjectParent = new JobDeclarationMessageSendingObjectParent(messageObject, isAutoSend: true);
		if (sendingObjectParent.ObjectsToSend.Any(x => x.Header.PK == entryHeader.PK))
		{
			var sender = new ESMessageSender(sendingObjectParent);
			var messageBuildersData = sender.GetMessageBuildersData().Where(x => x.EntryHeader.PK == entryHeader.PK).ToList();

			messageBuildersData = ES.Business.CusTempStorage.InventoryManagementHelper.InventoryManagementAction(Declaration.Factory, messageBuildersData, ReserveTemporaryStorageGoodsWhenErrors);

			var result = ESMessageSender.Send(messageBuildersData, messageDecorator: m => { m.EM_ApplicationReference = Declaration.JE_CustomsProfile; });

			ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask("ESS", TimeSpan.FromMinutes(10));

			if (result.MessagesWithCreateFailure > 0)
			{
				notifications.Add(NotificationType.Error, ZString.Format(messageCreateFailureHint, Declaration.JE_DeclarationReference));
			}
			if (result.MessagesWithSendFailure > 0)
			{
				notifications.Add(NotificationType.Error, ZString.Format(messageSendFailureHint, Declaration.JE_DeclarationReference));
			}
			if (result.MessagesSent > 0)
			{
				sent = true;
			}
		}
		return sent;
	}

	List<MessageBuilderData> ReserveTemporaryStorageGoodsWhenErrors(CusEntryHeader entryHeader, ZString errorMessage, ZString errorMessageVINs, IEnumerable<DataToReserveTSGoods> dataToReserve, MessageBuilderData builderData, List<MessageBuilderData> messageBuildersDataToContinue)
	{
		var strMessage = ZString.Empty;
		if (!errorMessage.IsEmpty)
		{
			strMessage = errorMessage;
		}
		if (!errorMessageVINs.IsEmpty)
		{
			if (!strMessage.IsEmpty)
			{
				strMessage += ", ";
			}
			strMessage += errorMessageVINs;
		}
		strMessage = strMessage.Replace(SuffixForErrorMessageWhenGrossWeightForVins, "");
		strMessage = strMessage.Replace(SuffixForErrorMessageDoYowWantToCancelDeclaration, "");
		strMessage = strMessage.Replace("\n\n", " ");
		strMessage = strMessage.Replace("\n", " ");

		messageBuildersDataToContinue.Remove(builderData);

		var logTypeCode = (NoResString)"Sending Error";
		var logReason = (NoResString)"Entry was not sent automatically because of a Temporary Storage Inventory Management error: ";
		var parameters = new KeyValuePair<string, string>[]
		{
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, logTypeCode),
						new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, logReason + strMessage)
		};
		entryHeader.Logs.AddNew(ZArchitecture.Business.AutoEvents.DeclarationHasErrors, "Declaration has not been sent", parameters);

		return messageBuildersDataToContinue;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string SuffixForErrorMessageWhenGrossWeightForVins = "Would you like to cancel this action and check the gross weight declared for the vehicles?";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string SuffixForErrorMessageDoYowWantToCancelDeclaration = "Do you want to cancel this declaration to check?";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string messageCreateFailureHint = "There are messages failing to be created for JobDeclaration: {0}";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string messageSendFailureHint = "There are messages failing to be sent for JobDeclaration: {0}";
}
