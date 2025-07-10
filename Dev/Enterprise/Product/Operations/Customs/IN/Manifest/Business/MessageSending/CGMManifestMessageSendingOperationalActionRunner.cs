using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.IN.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using EDIMessage = Enterprise.Customs.IN.Business.EDIMessage;
using EDIMessageSubTypeList = Enterprise.Customs.IN.Business.EDIMessageSubTypeList;

namespace Enterprise.Customs.IN.Manifest.Business;

public class CGMManifestMessageSendingOperationalActionRunner
{
	public void SendMessage(CGMAsycudaManifestHeader[] cgmHeaders, IOperationalActionSectionLog log, bool allowSendWithMessageError)
	{
		var totalTransactionsSelected = cgmHeaders.Length;
		var totalTransactionsSuccessful = 0;
		var totalTransactionsSkipped = 0;
		var subTypes = new[] { EDIMessageSubTypeList.Codes.SeaCgm, EDIMessageSubTypeList.Codes.AirCgm };
		log.SetSectionProgressMax(totalTransactionsSelected);

		foreach (var headerPK in cgmHeaders.Select(x => x.PK))
		{
			var factory = new BusinessObjectFactory();
			if (factory.Load<CGMAsycudaManifestHeader>(headerPK) is CGMAsycudaManifestHeader header)
			{
				var messageLoader = new EDIMessage.Loader(factory);
				var link = new LogControllerLink(header.AMA_JobReference, ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest, header.PK);
				if (messageLoader.MessageHasBeenSentOn(header, INManifestTypes.Codes.CGM, subTypes))
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0} Skipped. Message already exists.", link);
					totalTransactionsSkipped++;
				}
				else
				{
					var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
					if (sendingObjectParent.ValidateBeforeSend().IsEmpty && (allowSendWithMessageError || sendingObjectParent.BizObjValidationMessageErrors.IsEmpty))
					{
						foreach (ManifestMessageSendingObject sendingObject in sendingObjectParent.SendingObjectsCollection)
						{
							sendingObject.MessageType = ManifestMessageTypeList.Codes.Fresh;
						}
						var messages = sendingObjectParent.SendAndSaveMessages(MessageSendingContext.EMAIL);
						if (messages?.Length > 0)
						{
							foreach (var message in messages)
							{
								log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0} Successful. Message Number: {1}", link, new LogControllerLink(message.EM_MessageNum, ControllerIDs.Messaging.EDIMessage, message.PK));
							}
							totalTransactionsSuccessful++;
						}
						else
						{
							log.NotifyFormat(OperationalActionLogErrorLevel.Error, "{0} Skipped. Sending failed.", link);
							totalTransactionsSkipped++;
						}
					}
					else
					{
						log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "{0} Skipped. Errors & Warning observed.", link);
						totalTransactionsSkipped++;
					}
				}
			}
			log.BumpSectionProgress();
		}

		log.Notify(OperationalActionLogErrorLevel.Informational, $"Total Transactions Selected\t: {totalTransactionsSelected}\r\nTransactions Successful\t: {totalTransactionsSuccessful}\r\nTransactions Skipped\t: {totalTransactionsSkipped}");
	}
}
