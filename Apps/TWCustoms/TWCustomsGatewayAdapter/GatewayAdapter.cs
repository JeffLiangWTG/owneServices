using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter.Data;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace CargoWise.eHub.Products.TWCustoms.TWCustomsGatewayAdapter
{
	public class GatewayAdapter : IGatewayAdapter
	{
		private readonly string pluginDirectory;
		private readonly ILogger logger;
		private readonly IConfiguration configuration;
		private readonly IFileManager fileManager;
		private readonly IHttpClient httpClient;
		private readonly IPluginManager pluginManager;

		public GatewayAdapter(IConfiguration configuration, IPluginManager pluginManager, IFileManager fileManager, IHttpClient httpClient, ILogger logger)
		{
			this.logger = logger;
			this.configuration = configuration;
			this.fileManager = fileManager;
			this.httpClient = httpClient;
			this.pluginManager = pluginManager;
			pluginDirectory = configuration.GetSection("PluginDirectory").Value;
		}

		public AdapterResult SendMessage(ITWCustomsRequest request)
		{
			if (!fileManager.DirectoryExists(pluginDirectory))
				throw new ApplicationException($"Plugin directory \"{pluginDirectory}\" does not exist.");

			var sendRequest = request as TWCustomsGatewaySendRequest;

			var pluginConfig = GetPluginConfiguration(pluginDirectory, request, "S");
			pluginConfig.IsCAA = sendRequest.MessageType == "CAA";

			PreparePluginConfigurations(pluginConfig);

			var messageFileName = $"{sendRequest.MessageFormat}.{sendRequest.InterchangeNum}.xml";
			var messageFilePath = Path.Combine(pluginConfig.SendSrcPath, messageFileName);
			logger.Information($"Create message file: {messageFileName}");
			if (!fileManager.FileExists(messageFilePath))
			{
				fileManager.CreateFile(messageFilePath, sendRequest.MessageBodyBase64, true);
			}
			else
			{
				logger.Information($"Message file already exists: {messageFileName}");
			}

			if (sendRequest.Attachments != null)
			{
				foreach (var attachment in sendRequest.Attachments)
				{
					var attachmentFileName = $"{sendRequest.MessageFormat}.{sendRequest.InterchangeNum}.{attachment.AttachmentName}.{attachment.AttachmentFileType}";
					var attachmentFilePath = Path.Combine(pluginConfig.SendSrcAttachmentPath, attachmentFileName);
					logger.Information($"Create attachment file: {attachmentFileName}");

					if (!fileManager.FileExists(attachmentFilePath))
					{
						fileManager.CreateFile(attachmentFilePath, attachment.AttachmentDataBase64, true);
					}
					else
					{
						logger.Information($"Attachment file already exists: {attachmentFileName}");
					}
				}
			}

			return pluginManager.ExecutePluginHandler(pluginConfig, configuration);
		}

		public AdapterResult ReceiveMessage(ITWCustomsRequest request)
		{
			if (!fileManager.DirectoryExists(pluginDirectory))
				throw new ApplicationException($"Plugin directory \"{pluginDirectory}\" does not exist.");

			var pluginConfig = GetPluginConfiguration(pluginDirectory, request, "R");
			var endpoint = configuration.GetSection("TWCustomsReceiveEndpoint").Value;

			PreparePluginConfigurations(pluginConfig);

			var result = pluginManager.ExecutePluginHandler(pluginConfig, configuration);

			if (!result.HasError)
			{
				var sender = configuration.GetSection("SenderClientID").Value;
				foreach (FileInfo responseFile in fileManager.GetTWCustomsResponseFiles(pluginConfig.ReceiveTargetPath))
				{
					var responseContent = fileManager.GetText(responseFile.FullName);
					var responseXml = XDocument.Parse(responseContent);
					responseXml.Declaration = null;
					var recipient = GetCW1ClientId(request);
					var twCustomsResponseMessage = $@"<TWCustomsResponse xmlns=""http://cargowise.com/ehub/products/TWCustoms"">
	<Header>
		<Sender>{sender}</Sender>
		<Recipient>{recipient}</Recipient>
	</Header>
	<Body>
		<InterchangeNum>{responseFile.Name}</InterchangeNum>
		{responseXml}
	</Body>
</TWCustomsResponse>";
					logger.Information($"Http client post to {endpoint}.");
					logger.Debug($"TWCustoms response content: {twCustomsResponseMessage}");
					var response = httpClient.PostAsync(endpoint, twCustomsResponseMessage);

					logger.Information($"Response status: {response.StatusCode.GetHashCode()} {response.ReasonPhrase.ToUpper()}");
					if (response.IsSuccessStatusCode)
					{
						fileManager.Delete(responseFile.FullName);
					}
				}
			}

			return result;
		}

		private void PreparePluginConfigurations(PluginInfo pluginConfig)
		{
			var builtConfigContent = pluginManager.BuildRequestConfigurationAndUpdatePluginInfo(pluginConfig, configuration);

			if (!fileManager.FileExists(pluginConfig.CertificateFilePath) || pluginManager.IsCertificateUpdated(pluginConfig))
			{
				logger.Information($"Create certificate file: {pluginConfig.CertificateFilePath}");
				fileManager.CreateFile(pluginConfig.CertificateFilePath, pluginConfig.CertificateContent, true);
			}

			if (!fileManager.FileExists(pluginConfig.ConfigFilePath) ||
				pluginManager.IsConfigurationUpdated(builtConfigContent, pluginConfig))
			{
				logger.Information($"Create config file: {pluginConfig.ConfigFilePath}");
				fileManager.CreateFile(pluginConfig.ConfigFilePath, builtConfigContent, false);
			}
		}

		public PluginInfo GetPluginConfiguration(string pluginDirectory, ITWCustomsRequest request, string requestType)
		{
			XDocument configurationDocument = XDocument.Load(new MemoryStream(Convert.FromBase64String(request.RegistrationConfiguration)));

			var mailboxGroupElement = configurationDocument.Root.XPathSelectElement("//*[local-name()='Group' and @Type='MailBoxID']");

			if (mailboxGroupElement == null)
			{
				throw new ApplicationException("Cannot find configuration for processing request.");
			}

			var passwordType = mailboxGroupElement.XPathSelectElement("//*[local-name()='Item' and @Name='Platform']")?.Value;
			var userName = mailboxGroupElement.XPathSelectElement("//*[local-name()='Credential']/*[local-name()='UserName']")?.Value;
			var userPassword = EhubServerDecryptor.Decrypt(mailboxGroupElement.XPathSelectElement("//*[local-name()='Credential']/*[local-name()='Password']")?.Value);
			var certificateName = mailboxGroupElement.XPathSelectElement("//*[local-name()='Certificate']")?.Attribute("Name")?.Value;
			var certificateContent = mailboxGroupElement.XPathSelectElement("//*[local-name()='Certificate']/*[local-name()='File']")?.Value;
			var certificatePassword = EhubServerDecryptor.Decrypt(mailboxGroupElement.XPathSelectElement("//*[local-name()='Certificate']/*[local-name()='Passphrase']")?.Value);
			var system = configurationDocument.Root.XPathSelectElement("//*[local-name()='Group' and @Type='System']").Attribute("Reference")?.Value;
			var company = configurationDocument.Root.XPathSelectElement("//*[local-name()='Group' and @Type='Company']")?.Attribute("Reference")?.Value;
			var staff = configurationDocument.Root.XPathSelectElement("//*[local-name()='Group' and @Type='Staff']")?.Attribute("Reference")?.Value;
			var mailbox = mailboxGroupElement.Attribute("Reference")?.Value;

			var pluginConfiguration = new PluginInfo
			{
				WorkingDirectory = pluginDirectory,
				CertificateName = certificateName,
				CertificateContent = certificateContent,
				CertificatePassword = certificatePassword,
				Mailbox = mailbox,
				SystemId = system,
				CompanyId = company,
				StaffCode = staff,
				PasswordType = passwordType,
				RequestType = requestType,
				UserName = userName,
				UserPassword = userPassword,
				IsForwarderManifest = string.IsNullOrEmpty(staff)
			};

			return pluginConfiguration;
		}

		string GetCW1ClientId(ITWCustomsRequest request)
		{
			XDocument configuration = XDocument.Load(new MemoryStream(Convert.FromBase64String(request.RegistrationConfiguration)));

			var systemId = configuration.Root.XPathSelectElement("./*[local-name()='Group']")?.Attribute("Reference")?.Value;
			var company = configuration.Root.XPathSelectElement("./*[local-name()='Group']/*[local-name()='Group']")?.Attribute("Reference")?.Value;

			if (systemId?.Length != 6 || company?.Length != 3)
			{
				throw new InvalidOperationException($"Invalid CW1 client ID. SystemId: {systemId}, Company: {company}");
			}

			return systemId.Substring(0, 3) + company + systemId.Substring(3);
		}
	}
}
