using System;
using System.IO;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataAccess.Integration;

namespace CargoWise.eHub.Gateway
{
	public abstract class MessageHandler
	{
		public abstract void Handle(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message);

		public virtual IEnterpriseExeDetailAccessor NewEnterpriseExeDetailAccessor
		{
			get { return DataAccessFactories.NewEnterpriseExeDetailAccessorInstance(); }
		}

		protected static String MessageToString(eHubGatewayMessage message)
		{
			using (var stream = message.MessageStream.DecodeAndDecompress())
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		protected internal virtual bool IsEnterpriseLicenceProduction(string clientID)
		{
            string licenceType;
			if (TryLoadEnterpriseLicenceType(clientID, out licenceType))
    			return IsLicenceProduction(licenceType);
            return false;
		}

        protected bool TryLoadEnterpriseLicenceType(string clientID, out string licenceType)
        {
            licenceType = NewEnterpriseExeDetailAccessor.GetLicenceType(clientID);
            return !string.IsNullOrEmpty(licenceType);
        }

        private bool IsLicenceProduction(string licenceType)
		{
			return licenceType == "PRD";
		}

		public virtual void CheckIfTooManyUnprocessedMessages(string senderId, string recipientId)
		{
		}

		protected static String GetClientSystemID(string gatewayMessageSenderID)
		{
			if (gatewayMessageSenderID == null || gatewayMessageSenderID.Length != 9)
			{
				throw new ArgumentException("Invalid Sender ID: {0}", gatewayMessageSenderID);
			}

			return gatewayMessageSenderID.Substring(0, 3) + gatewayMessageSenderID.Substring(6, 3);
		}

        protected void CheckLicenceAndUpdateClientID(string senderID, eHubGatewayMessage message, string clientIdSuffix = "")
        {
            string licenceType = null;
            var isCheckingLicence = !senderID.StartsWith("T_____");

			if (isCheckingLicence && !TryLoadEnterpriseLicenceType(senderID, out licenceType))
            {
                throw new ArgumentException("Error retrieving licence details for sender.");
            }

            if (!string.IsNullOrWhiteSpace(clientIdSuffix))
            {
                message.ClientID += clientIdSuffix;
            }

            if (isCheckingLicence && !IsLicenceProduction(licenceType))
            {
                message.ClientID += "Test";
            }
        }
	}
}
