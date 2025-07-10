using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public interface IMessageSendingObjectParent
	{
		BusinessObjectFactory Factory { get; }

		IEnumerable<BaseMessageSendingObject> SelectedSendingObjects { get; }
	}

	public static class IMessageSendingObjectParentExtensions
	{
		public static int SendMessagesAndSave(this IMessageSendingObjectParent sendingObjectParent)
		{
			var messages = new List<EDIMessage>();

			var messageManagers = sendingObjectParent.SelectedSendingObjects.Select(x => MessageManagerCreator.CreateNew(x)).Where(x => x != null);
			foreach (var messageManager in messageManagers)
			{
				messages.AddRange(messageManager.GenerateMessages());
			}

			if (messages.Count > 0)
			{
				var interchanges = GenerateInterchangesIfNeeded(sendingObjectParent, messages);

				try
				{
					sendingObjectParent.Factory.Save();
				}
				catch (ZSaveException ex)
				{
					messages.ForEach(m => m.Delete());
					messages.Clear();

					interchanges?.ForEach(i => i.Delete());

					messageManagers.ForEach(x => x.RollbackOnSavingFailed());

					ZExceptionReporting.HandleSaveException(ex);
				}
			}
			return messages.Count;
		}

		static EDIInterchange[] GenerateInterchangesIfNeeded(IMessageSendingObjectParent sendingObjectParent, List<EDIMessage> messages)
		{
			var messageCollection = new NonDependentEDIMessageCollection(sendingObjectParent.Factory);
			messageCollection.AddRange(messages);

			var interchangeProvider = GetInterchangeProvider(sendingObjectParent, messageCollection);
			if (interchangeProvider != null)
			{
				interchangeProvider.PackCollatedMessagesIntoInterchanges();
			}

			return interchangeProvider?.Interchanges;
		}

		static BRInterchangeProvider GetInterchangeProvider(IMessageSendingObjectParent sendingObjectParent, NonDependentEDIMessageCollection messageCollection)
		{
			switch (sendingObjectParent)
			{
				case ImportLicenseMessageSendingObjectParent:
					return new BRImportLicenseInterchangeProvider(messageCollection);
				case ImportSiscomexMessageSendingObjectParent:
					return new BRImportSiscomexInterchangeProvider(messageCollection);
				case ForeignOperatorMessageSendingObject:
				case GoodsCatalogMessageSendingObject:
					return new BRBatchInterchangeProvider(messageCollection);
				default:
					return null;
			}
		}
	}
}
