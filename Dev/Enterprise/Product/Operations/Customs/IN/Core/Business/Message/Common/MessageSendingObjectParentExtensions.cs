using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public static class MessageSendingObjectParentExtensions
{
	public static EDIMessage[] SendAndSaveMessages<T>(this IMessageSendingObjectParent sendingObjectParent, Func<T, IMessageSender> createMessageSender, MessageSendingContext context)
		where T : IMessageSendingObject
	{
		var messages = new List<EDIMessage>();
		foreach (var messageSendingObject in sendingObjectParent.SelectedSendingObjects.Cast<T>())
		{
			var messageSender = createMessageSender?.Invoke(messageSendingObject);
			if (messageSender != null)
			{
				messages.Add(messageSender.Send(context));
			}
		}

		try
		{
			sendingObjectParent.Factory.Save();
		}
		catch (ZSaveException ex)
		{
			messages.Clear();
			ZExceptionReporting.HandleSaveException(ex);
		}

		return messages.ToArray();
	}
}
