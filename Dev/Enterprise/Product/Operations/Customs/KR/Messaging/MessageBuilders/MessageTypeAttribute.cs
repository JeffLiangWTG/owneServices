using System;

namespace Enterprise.Customs.KR.Messaging
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class MessageTypeAttribute : Attribute
	{
		public MessageTypeAttribute(string messageType)
		{
			if (string.IsNullOrEmpty(messageType))
			{
				throw new ArgumentException("messageType should have a non empty value");
			}
			MessageType = messageType;
		}

		public readonly string MessageType;
	}
}
