using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public interface IMessageSendingObjectParent
{
	BusinessObjectFactory Factory { get; }

	ZString CanSendMessage();

	void UpdateSendingObjectsBeforeSending();

	IEnumerable<IMessageSendingObject> SelectedSendingObjects { get; }
}

public static class MessageSendingObjectParentExtensions
{
	public static int SendMessagesAndSave(this IMessageSendingObjectParent messageSendingObjectParent, Func<IMessageSendingObject, IMessageManager> messageManagerCreator)
	{
		var messages = new List<EDIMessage>();
		var messageManagers = new List<IMessageManager>();
		messageSendingObjectParent.UpdateSendingObjectsBeforeSending();

		foreach (var objectToSend in messageSendingObjectParent.SelectedSendingObjects)
		{
			var messageManager = messageManagerCreator?.Invoke(objectToSend);
			if (messageManager != null)
			{
				messages.AddRange(messageManager.GenerateMessages());
				messageManagers.Add(messageManager);
			}
		}

		if (messages.Count > 0)
		{
			try
			{
				messageSendingObjectParent.Factory.Save();
				return messages.Count;
			}
			catch (ZSaveException ex)
			{
				messages.ForEach(m => m.Delete());
				messageManagers.ForEach(m => m.RollbackOnSaveFailed());
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		return 0;
	}
}
