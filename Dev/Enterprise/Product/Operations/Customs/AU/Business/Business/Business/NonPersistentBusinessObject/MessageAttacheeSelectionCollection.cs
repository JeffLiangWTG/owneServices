using System;
using System.Collections;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IMessageAttacheeParent
	{
		BusinessObjectFactory Factory { get; }
		IMessageAttachee[] MessageAttachees { get; }
	}

	public class MessageAttacheeSelectionCollection : NonPersistentBusinessObjectCollection<MessageAttacheeSelection>
	{
		public MessageAttacheeSelectionCollection(IMessageAttacheeParent holder, MessageAttacheeMessageType messageType)
			: base(holder.Factory)
		{
			this.Holder = holder;
			this.MessageType = messageType;
			CreateItems();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public string MessageTypeString
		{
			get
			{
				string result = "";
				if (MessageType == MessageAttacheeMessageType.Amend)
				{
					result = "amendments";
				}
				else if (MessageType == MessageAttacheeMessageType.Original)
				{
					result = "originals";
				}
				else if (MessageType == MessageAttacheeMessageType.Withdraw)
				{
					result = "withdrawals";
				}
				return result;
			}
		}

		public IMessageAttachee[] GetMessageAttacheesToSendMessagesFor()
		{
			ArrayList result = new ArrayList();

			foreach (MessageAttacheeSelection messageAttacheeSelection in this)
			{
				if (messageAttacheeSelection.ShouldSendNow)
				{
					result.Add(messageAttacheeSelection.MessageAttachee);
				}
			}

			return (IMessageAttachee[])result.ToArray(typeof(IMessageAttachee));
		}

		#region Implementation

		void CreateItems()
		{
			foreach (IMessageAttachee messageAttachee in Holder.MessageAttachees)
			{
				if (messageAttachee.IsValidToSendThisMessageType(MessageType))
				{
					Add(new MessageAttacheeSelection(messageAttachee));
				}
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("A new item cannot be created by users on forms from this collection");
		}

		public readonly MessageAttacheeMessageType MessageType;
		public readonly IMessageAttacheeParent Holder;

		#endregion
	}
}
