using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public abstract class TemporaryStorageMessagingProvider
	{
		public CodeDescriptionPairList GetMessageTypes(TemporaryStorageHeader header)
		{
			return GetMessageTypesCore(header);
		}

		protected virtual CodeDescriptionPairList GetMessageTypesCore(TemporaryStorageHeader header)
		{
			var messageType = header.AMA_MessageType.ToUpper();
			var messageStatus = header.AMA_MessageStatus.ToUpper();
			var customsStatus = header.CustomsStatus.ToUpper();
			return header.Factory.GetCachedValue($"TemporaryStorageMessagingProvider.{messageType}.{messageStatus}.{customsStatus}", () =>
			{
				var result = new CodeDescriptionPairList();

				if (customsStatus.IsEmpty)
				{
					switch (messageType)
					{
						case PNTSMessageTypeList.Codes.CombinedTemporaryStorage:
							result.AddPair(TemporaryStorageMessageTypeList.Codes.CombinedTSD, TemporaryStorageMessageTypeList.Descriptions.CombinedTSD);
							break;
						case PNTSMessageTypeList.Codes.PreLodgedTempStorage:
							result.AddPair(TemporaryStorageMessageTypeList.Codes.PreLodgedTSD, TemporaryStorageMessageTypeList.Descriptions.PreLodgedTSD);
							break;
						case PNTSMessageTypeList.Codes.PresentationNotification:
							result.AddPair(TemporaryStorageMessageTypeList.Codes.PresentationNotification, TemporaryStorageMessageTypeList.Descriptions.PresentationNotification);
							break;
						default:
							break;
					}
				}
				else if (customsStatus == PNTS.CustomsStatus.TemporaryStoragePreLodged)
				{
					if (messageType != PNTSMessageTypeList.Codes.PreLodgedTempStorage)
					{
						result.AddPair(TemporaryStorageMessageTypeList.Codes.PresentationNotification, TemporaryStorageMessageTypeList.Descriptions.PresentationNotification);
					}

					if (messageType != PNTSMessageTypeList.Codes.PresentationNotification)
					{
						result.AddPair(TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD, TemporaryStorageMessageTypeList.Descriptions.AmendmentRequestTSD);
						result.AddPair(TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD, TemporaryStorageMessageTypeList.Descriptions.InvalidationRequestTSD);
					}
				}
				else if (customsStatus == PNTS.CustomsStatus.TemporaryStorageActivated)
				{
					result.AddPair(TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD, TemporaryStorageMessageTypeList.Descriptions.AmendmentRequestTSD);
				}

				return result;
			});
		}

		protected virtual IEnumerable<TemporaryStorageMessageFunction> GetMessageFunctions(TemporaryStorageMessageSendingObject messageSendingObject)
		{
			yield return new PresentationNotificationMessageFunction();
			yield return new PreLodgedTSDMessageFunction();
			yield return new CombinedTSDMessageFunction();
			yield return new TransferNotificationTSDMessageFunction();
			yield return new DeconsolidationNotificationTSDMessageFunction();
			yield return new AmendmentRequestTSDMessageFunction();
			yield return new InvalidationRequestTSDMessageFunction(messageSendingObject);
		}

		public void SendMessage(TemporaryStorageMessageSendingObject messageSendingObject, ISendsMessagesToCustoms sender, IUserNotification notification)
		{
			var messageType = messageSendingObject.MessageType;
			var header = messageSendingObject.Header;
			var messageFunction = GetMessageFunctions(messageSendingObject).FirstOrDefault(x => x.MessageType == messageType);
			if (messageFunction != null)
			{
				using (var messageBuilder = GetTemporaryStorageMessageBuilder(messageSendingObject, messageFunction))
				{
					var messageSender = new TemporaryStorageMessageManager(header, messageBuilder);
					messageSender.SendTemporaryStorageMessage(sender);
				}
			}
			else
			{
				notification.ShowError(Res.GetString("807618C4-685A-4280-996A-9DBD16AA7195", "Message Type {0} is not supported yet.", messageType));
			}
		}

		protected abstract TemporaryStorageMessageBuilder GetTemporaryStorageMessageBuilder(TemporaryStorageMessageSendingObject messageSendingObject, TemporaryStorageMessageFunction messageFunction);

		public ZString GetDefaultMessageType(TemporaryStorageMessageSendingObject sendingObject) => GetDefaultMessageTypeCore(sendingObject);

		protected virtual ZString GetDefaultMessageTypeCore(TemporaryStorageMessageSendingObject sendingObject)
		{
			var result = ZString.Empty;
			var header = sendingObject.Header;

			if (NoTSDOrPNHasBeenSuccessfullySent())
			{
				switch (header.AMA_MessageType)
				{
					case PNTSMessageTypeList.Codes.CombinedTemporaryStorage:
						result = TemporaryStorageMessageTypeList.Codes.CombinedTSD;
						break;
					case PNTSMessageTypeList.Codes.PreLodgedTempStorage:
						result = TemporaryStorageMessageTypeList.Codes.PreLodgedTSD;
						break;
					case PNTSMessageTypeList.Codes.PresentationNotification:
						result = TemporaryStorageMessageTypeList.Codes.PresentationNotification;
						break;
					default:
						break;
				}
			}

			if (result.IsEmpty)
			{
				result = GetDefaultMessageIfTheThereIsOnlyOne();
			}

			return result;

			bool NoTSDOrPNHasBeenSuccessfullySent()
			{
				return (header.AMA_MessageStatus == PNTSMessageStatusList.Codes.TechnicalFailure
						|| header.AMA_MessageStatus == PNTSMessageStatusList.Codes.FunctionalRejection
						|| header.AMA_MessageStatus.IsEmpty)
					   && header.CustomsStatus.IsEmpty;
			}

			string GetDefaultMessageIfTheThereIsOnlyOne()
			{
				var messageTypes = sendingObject.Lookups.MessageTypes;
				return messageTypes.Count == 1 ? messageTypes.CodesAsString : string.Empty;
			}
		}
	}
}
