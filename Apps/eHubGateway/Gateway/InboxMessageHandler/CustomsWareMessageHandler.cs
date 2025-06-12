using System;
using System.IO;

using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway
{
	class CustomsWareMessageHandler : DefaultInboxMessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			if (!PreCheck(message)) return;

			base.Handle(senderID, envelopeTrackingID, message);

		}

		#region Implementation
		bool PreCheck(eHubGatewayMessage message)
		{
			if (message.SchemaName == "http://www.customsware.com/schema/api#InputDocument" && message.MessageStream.Length == 0)
			{
				FailMessage("Message is empty");
				return false;
			}

			return true;
		}

		internal void FailMessage(string errorDescription)
		{
			throw new InvalidDataException(errorDescription);
		}
		#endregion
	}
}