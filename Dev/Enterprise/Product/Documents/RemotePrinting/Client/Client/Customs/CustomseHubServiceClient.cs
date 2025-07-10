using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.ServiceModel;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Common.Service.Reference;

namespace Enterprise.RemotePrinting.Client
{
	class eHubStreamedServiceConfiguration : CargoWise.eHub.Common.ServiceConfiguration
	{
		public string ServiceAddress { get; }

		readonly EndpointAddress endpointAddress;

		public eHubStreamedServiceConfiguration(string serverAddress)
		{
			endpointAddress = new EndpointAddress($"https://{serverAddress}/eHubGateway/eHubStreamedService.svc");
		}

		public override EndpointAddress EndpointAddress => endpointAddress;
	}

	public class CustomseHubServiceClientProxy : IDisposable
	{
		protected string ServerAddress { get; }
		protected string ClientId { get; }
		protected string Password { get; }

		protected virtual eHubStreamedService Client => fClient ?? (fClient = CreateClient());
		eHubStreamedServiceClient fClient;

		public CustomseHubServiceClientProxy(string serverAddress, string clientID, string password)
		{
			ServerAddress = serverAddress;
			ClientId = clientID;
			Password = password;
		}

		eHubStreamedServiceClient CreateClient()
		{
			eHubStreamedServiceClient result = null;
			if (!string.IsNullOrWhiteSpace(ServerAddress))
			{
				if (ServicePointManager.ServerCertificateValidationCallback == null)
				{
					ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
					ServicePointManager.DefaultConnectionLimit = 100;
				}

				var config = new eHubStreamedServiceConfiguration(ServerAddress);
				result = new eHubStreamedServiceClient(config.Binding, config.EndpointAddress);
				result.ClientCredentials.UserName.UserName = ClientId;
				result.ClientCredentials.UserName.Password = Password;
			}
			return result;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && fClient != null)
			{
				if (fClient.State == CommunicationState.Faulted)
				{
					fClient.Abort();
				}
				else
				{
					fClient.Close();
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public string Ping()
		{
			bool success;
			var exceptionMessage = string.Empty;
			try
			{
				success = Client.Ping();
			}
			catch (Exception ex)
			{
				success = false;
				exceptionMessage = ex.Message;
			}

			return success ? string.Empty : $"Connecting to server: {ServerAddress} failed {exceptionMessage}";
		}

		public RetrieveStreamResponse ReceiveStream()
		{
			return Client.RetrieveStream(new RetrieveStreamRequest());
		}

		public void Finalise(string trackingID)
		{
			Client.FinaliseBatch(trackingID);
		}

		public SendStreamResponse SendStream(Guid trackingID, string recipient, string fileName, string schemaName, Stream messageStream)
		{
			var message = new CargoWise.eHub.Common.eHubGatewayMessage();
			message.MessageTrackingID = trackingID;
			message.ClientID = recipient;
			message.SchemaName = schemaName;
			message.SchemaType = CargoWise.eHub.Common.MessageSchemaType.Xml;
			message.ApplicationCode = "UDM";
			message.FileName = fileName;
			message.MessageStream = messageStream.CompressAndEncode();

			return Client.SendStream(new SendStreamRequest(Guid.NewGuid(), new[] { message }));
		}
	}
}
