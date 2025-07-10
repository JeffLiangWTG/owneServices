using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	partial class TopLevelDataObject
	{
		[DataProperty]
		public List<MessageNumber> MessageNumberCollection { get; set; }

		IEnumerable<IMessageNumber> ITopLevelDataObject.MessageNumberCollection => MessageNumberCollection.Cast<IMessageNumber>();

		public void SetMessageNumber(MessageNumberType type, ZString value)
		{
			EnsureMessageNumberCollection();

			if (type == MessageNumberType.MessageNumber)
			{
				MessageNumberCollection.Add(new MessageNumber { Type = type, Value = value });
			}
			else
			{
				var messageNumber = MessageNumberCollection.FirstOrDefault(mn => mn.Type.HasValue && mn.Type.Value == type);
				if (messageNumber == null)
				{
					MessageNumberCollection.Add(messageNumber = new MessageNumber { Type = type });
				}

				messageNumber.Value = value;
			}
		}

		void EnsureMessageNumberCollection()
		{
			if (MessageNumberCollection == null)
			{
				MessageNumberCollection = new List<MessageNumber>();
			}
		}
	}
}
