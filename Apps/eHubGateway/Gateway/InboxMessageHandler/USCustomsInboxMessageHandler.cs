using System;
using System.Configuration;
using CargoWise.eHub.Common;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Integration;
using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.Services;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace CargoWise.eHub.Gateway
{
	public class USCustomsInboxMessageHandler : InboxMessageHandler
	{
		public override void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
		{
			CheckServiceSupported(senderID, message);
			var inboxPk = Guid.NewGuid();
			if (message.ApplicationCode == ApplicationCode.USeManifest)
				message.ClientID = "USC";

            var testUsc = new HashSet<string>(ConfigurationManager.AppSettings["USCustomsLicenceOverrideToProd"].Split(','));

			var isProd = testUsc.Contains(senderID) || IsEnterpriseLicenceProduction(senderID);

		    EnqueueMessage(new OutboundMessageQueuer(isProd), senderID, inboxPk, message);
		}

		internal virtual void CheckServiceSupported(string clientId, eHubGatewayMessage message)
		{
			var accessor = NewRegistrationAccessor;
			string result = accessor.SelectRegistryValue(clientId, message.ApplicationCode, Constants.RegistryName.EntryFilerCode);
			if (string.IsNullOrEmpty(result) && message.ApplicationCode == ApplicationCode.USImport && Regex.IsMatch(message.SchemaName, "S[NFA]"))
			{
				result = accessor.SelectRegistryValue(clientId, message.ApplicationCode, Constants.RegistryName.USI.ISFUserData);
			}
			else if (string.IsNullOrEmpty(result) && message.ApplicationCode == ApplicationCode.USeManifest)
			{
				result = accessor.SelectRegistryValue(clientId, message.ApplicationCode, Constants.RegistryName.MAN.ClientNetworkID);
			}
			else if (string.IsNullOrEmpty(result) && message.ApplicationCode == ApplicationCode.AMA)
			{
				result = accessor.SelectRegistryValue(clientId, message.ApplicationCode, Constants.RegistryName.AMA.ParticipantOriginatorCode);
			}
			else if (string.IsNullOrEmpty(result) && message.ApplicationCode == ApplicationCode.USExportManifest)
			{
				result = accessor.SelectRegistryValue(clientId, message.ApplicationCode, Constants.RegistryName.UEM.CarrierCode);
			}
			if (string.IsNullOrEmpty(result))
			{
				throw new ApplicationException("Interchange rejected by eHub because you are not registered with WTG for US Customs messaging. Please register.");
			}
		}

		public virtual IRegistryAccessor NewRegistrationAccessor
		{
			get { return DataAccessFactories.NewRegistryAccessorInstance(); }
		}
	}
}
