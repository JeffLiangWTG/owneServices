using System;
using System.Xml.Serialization;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	[XmlType(TypeName = "MessageNumber")]
	public class MessageNumberWrapper
	{
		MessageNumberWrapper() { }

		[XmlAttribute]
		public string Type { get; set; }

		[XmlText]
		public string Value { get; set; }

		public static MessageNumberWrapper New(MessageNumber messageNumber)
		{
			return messageNumber == null ? null : new MessageNumberWrapper()
			{
				Type = Enum.GetName(typeof(MessageNumberType), messageNumber.Type),
				Value = messageNumber.Value.ToString()
			};
		}
	}
}
