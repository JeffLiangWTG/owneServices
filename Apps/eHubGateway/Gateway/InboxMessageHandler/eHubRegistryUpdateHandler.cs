using System;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataAccess.Integration;

namespace CargoWise.eHub.Gateway
{
	class eHubRegistryUpdateHandler : DatabaseMessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			Execute(() => 
				{
					string systemID = GetClientSystemID(senderID);
					string xmlstring = message.MessageStream.DecodeAndDecompress().ReadToEnd();
					string url = ParseEHINudgeURL(xmlstring);
					NewEHubClientSystemAccessor.InsertOrUpdateClientSystemAndGenerateSuccessStatusMessage(systemID, url, message.MessageTrackingID, senderID);
				});
		}

		string ParseEHINudgeURL(string xmlstring)
		{
			XElement xml = XElement.Parse(xmlstring);
			var ediNudgeURLElement = xml.XPathSelectElement("/*[local-name()='EHINudgeURL']");
			if (ediNudgeURLElement == null)
				throw new ArgumentException("Message does not contains EHI nudge url.");
			return ediNudgeURLElement.Value.Trim();
		}

		public virtual IEHubClientSystemAccessor NewEHubClientSystemAccessor
		{
			get { return DataAccessFactories.NewEHubClientSystemAccessorInstance(); }
		}
	}
}
