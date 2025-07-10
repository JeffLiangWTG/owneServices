using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.CA.Business
{
	public class MessageBlockGenerator
	{
		public MessageBlockGenerator()
		{
			messageBlocks = new List<MessageBlock>();
		}
		readonly List<MessageBlock> messageBlocks;

		internal List<MessageBlock> MessageBlocks
		{
			get
			{
				hasReturnedMessageBlocks = true;
				return messageBlocks;
			}
		}
		bool hasReturnedMessageBlocks;

		public void AddMessageBlocks(IEnumerable<MessageBlock> passed)
		{
			if (hasReturnedMessageBlocks)
			{
				throw new InvalidOperationException("Cannot add more blocks - have already read them");
			}
			messageBlocks.AddRange(passed);
		}

		public void AddMessageBlock(MessageBlock messageBlock)
		{
			if (hasReturnedMessageBlocks)
			{
				throw new InvalidOperationException("Cannot add more blocks - have already read them");
			}
			messageBlocks.Add(messageBlock);
		}

		public void Deserialise(string messageRaw)
		{
			string[] messages = messageRaw.Split('\r', '\n');
			messageBlocks.Capacity = messages.Length;
			foreach (string message in messages)
			{
				if (!string.IsNullOrEmpty(message))
				{
					MessageBlock messageBlock = GetMessageBlock(message);
					AddMessageBlock(messageBlock);
				}
			}
		}

		public EDIMessage CreateMessage(BusinessObjectFactory factory)
		{
			var message = factory.New<DLMMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = Serialise();
			return message;
		}

		public string Serialise()
		{
			return Serialise(false);
		}

		public string Serialise(bool humanFriendly)
		{
			ZStringBuilder stringBuilder = new ZStringBuilder();
			foreach (MessageBlock messageBlockBase in messageBlocks)
			{
				string message = messageBlockBase.Serialise(humanFriendly);
				stringBuilder.Append(message);
			}
			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		protected MessageBlock GetMessageBlock(string characterBlock)
		{
			MessageBlock result = CreateMessageBlock(characterBlock[0]);
			result.Deserialise(characterBlock);
			return result;
		}

		MessageBlock CreateMessageBlock(char recordType)
		{
			switch (recordType)
			{
				case 'H':
					return new DLMHeader();
				case 'D':
					return new DLMDetail();
				case 'P':
					return new DLMPermit();
				case 'C':
					return new DLMContainer();
				case 'R':
					return new DLMReference();
				default:
					throw new InvalidMessageFormatException("Record Type '" + recordType.ToString() + "' is Unknown");
			}
		}
	}
}
