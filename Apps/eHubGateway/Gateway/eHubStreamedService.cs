using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Web;
using System.Xml;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.eHubDataAccess.Integration;
using CargoWise.eHub.Gateway.OpenAPIs.XHGateway;
using CargoWise.Billing.Kafka.API;
using Common.Logging;
using Confluent.Kafka;

namespace CargoWise.eHub.Gateway
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	public class eHubStreamedService : IeHubStreamedService
	{
		readonly int NumberOfCandidates;
		readonly int OrderedDeliveryThresholds;
		readonly long BatchSizeLimitInBytes;
		readonly bool ShouldForwardToProxyGateway;
		static int ExceptionReportTextSizeLimit;
		readonly int CompanyListMaxCount;
		static readonly ILog Logger = LogManager.GetLogger(typeof(eHubStreamedService));
		readonly ILog TransactionLog = LogManager.GetLogger("TransactionLog");
		readonly Stopwatch watch = Stopwatch.StartNew();
		internal static string KafkaMessageTimeoutInSeconds = ConfigurationManager.AppSettings["KafkaMessageTimeoutInSeconds"];
		private const string ConfigurationMessageType = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration";

		public eHubStreamedService()
		{
			OrderedDeliveryThresholds = int.TryParse(ConfigurationManager.AppSettings["OrderedDeliveryThresholdInSeconds"], out OrderedDeliveryThresholds) ? OrderedDeliveryThresholds : 60;
			NumberOfCandidates = int.TryParse(ConfigurationManager.AppSettings["NumberOfCandidates"], out NumberOfCandidates) ? NumberOfCandidates : 1000;
			BatchSizeLimitInBytes = long.TryParse(ConfigurationManager.AppSettings["BatchSizeLimitInBytes"], out BatchSizeLimitInBytes) ? BatchSizeLimitInBytes : 104850000;
			ShouldForwardToProxyGateway = bool.TryParse(ConfigurationManager.AppSettings["ShouldForwardToProxyGateway"], out ShouldForwardToProxyGateway) ? ShouldForwardToProxyGateway : false;
			CompanyListMaxCount = int.TryParse(ConfigurationManager.AppSettings["CompanyListMaxCount"], out CompanyListMaxCount) ? CompanyListMaxCount : 0;
			ExceptionReportTextSizeLimit = int.TryParse(ConfigurationManager.AppSettings["ExceptionReportTextSizeLimit"], out ExceptionReportTextSizeLimit) ? ExceptionReportTextSizeLimit : 307200;
		}

		public virtual bool Ping()
		{
			return true;
		}

		private string GetConfigurationName(eHubGatewayMessage message)
		{
			message.MessageStream.SeekBegin();
			using (var reader = XmlReader.Create(message.MessageStream.DecodeAndDecompress()))
			{
				reader.Read();
				var configurationName = reader.GetAttribute("Name");
				message.MessageStream.SeekBegin();
				return configurationName;
			}
		}

		public void SendStream(SendStreamRequest sendRequest)
		{
			var kafkaClient = CreateBillingKafkaClient();
			try
			{
				string senderId = GetCurrentClientId();

				if (ShouldForwardToProxyGateway)
				{
					var licenceType = EnterpriseExeDetailAccessor.GetLicenceType(senderId);
					if (licenceType != null && licenceType != "PRD")
					{

						var shouldForwardMessages = sendRequest.Messages.ToLookup(m =>
							{
								var configurationName = m.SchemaName == ConfigurationMessageType ? GetConfigurationName(m) : string.Empty;
								return TransformAccessor.GetRecipientCode(
								"eHub",
								"eHub",
								"Forward To Proxy Gateway",
								"Forward To Proxy Gateway",
								"Should Forward",
								senderId,
								m.ClientID,
								configurationName,
								null,
								null) == "Y";
							}
						);

						if (shouldForwardMessages[true].Count() > 0)
						{
							Logger.Info($"Forwarding the messages to the test gateway. Sender: { senderId }. { shouldForwardMessages[true].Count() } Tracking IDs in total: { string.Join(" | ", shouldForwardMessages[true].Select(m => m.MessageTrackingID)) }");
							var securityToken = OperationContext.Current.IncomingMessageProperties.Security.IncomingSupportingTokens[0].SecurityToken as UserNameSecurityToken;
							var shouldForwardRequest = new SendStreamRequest(sendRequest.SendStreamRequestTrackingID, shouldForwardMessages[true].ToArray());
							ForwardToProxyGateway(shouldForwardRequest, senderId, securityToken.Password);
							if (shouldForwardMessages[false].Count() > 0)
							{
								Logger.Info($"Not forwarding the following messages to the test gateway. Sender: { senderId }. { shouldForwardMessages[false].Count() } Tracking IDs in total:  { string.Join(" | ", shouldForwardMessages[false].Select(m => m.MessageTrackingID)) }");
								sendRequest.Messages = shouldForwardMessages[false].ToArray();
							}
							else
							{
								return;
							}
						}
						else
						{
							Logger.Info($"Not forwarding the following messages to the test gateway. Sender: { senderId }. { shouldForwardMessages[false].Count() } Tracking IDs in total:  { string.Join(" | ", shouldForwardMessages[false].Select(m => m.MessageTrackingID)) }");

						}
					}
				}

				Dictionary<Guid, Exception> messageExceptionDictionary = null;
				MessageHandlerCache handlerCache = new MessageHandlerCache(sendRequest.Messages.Count());
				ThrottleChecker throttleChecker = new ThrottleChecker();

				for (var i = 0; i < sendRequest.Messages.Count(); i++)
				{
					var message = sendRequest.Messages[i];
					CheckClientAuthorisation(senderId, message);
					var handler = GetHandler(message);
					if (handler is KafkaMessageHandler)
						(handler as KafkaMessageHandler).KafkaClient = kafkaClient.Value;
					handlerCache.CacheHandler(i, handler);
					throttleChecker.PerformThrottleCheck(handler, senderId, message.ClientID);
				}



				for (int i = 0; i < sendRequest.Messages.Count(); i++)
				{
					var message = sendRequest.Messages[i];
					var handler = handlerCache.GetHandler(i);
					try
					{
						HandleMessage(handler, sendRequest.SendStreamRequestTrackingID, message, senderId);
					}
					catch (SystemUnderMaintananceException systemUnderMaintananceException)
					{
						throw new SystemException(systemUnderMaintananceException.Message, systemUnderMaintananceException);
					}
					catch (Exception ex)
					{
						if (ex is SqlException)
						{
							SqlExceptionHandler.ThrowSystemUnderMaintananceExceptionIfApplicable(ex as SqlException);
						}

						if (ex is XHGatewayException)
						{
							XHGatewayExceptionHandler.ThrowSystemUnderMaintananceExceptionIfApplicable(ex as XHGatewayException);
						}

						if (messageExceptionDictionary == null)
						{
							messageExceptionDictionary = new Dictionary<Guid, Exception>();
							messageExceptionDictionary.Add(message.MessageTrackingID, ex);
						}
						else if (!messageExceptionDictionary.ContainsKey(message.MessageTrackingID))
						{
							messageExceptionDictionary.Add(message.MessageTrackingID, ex);
						}
					}
					finally
					{
						message.Dispose();
					}
				}

				if (kafkaClient.IsValueCreated) kafkaClient.Value.FlushProducers();
				var kafkaHandlerExceptions = handlerCache.Handlers.Where(handler => handler is KafkaMessageHandler messageHandler && !messageHandler.MessageException.Equals(default(KeyValuePair<Guid, Exception>)))
						.Select(handler => ((KafkaMessageHandler)handler).MessageException).ToArray();

				if (kafkaHandlerExceptions.Any())
				{
					messageExceptionDictionary = HandleKafkaHandlerExceptions(messageExceptionDictionary, kafkaHandlerExceptions);
				}


				if (messageExceptionDictionary != null)
				{
					CombineExceptionsAndThrow(messageExceptionDictionary);
				}

				LogTransaction("OK", SendRequestLogMessage);
			}
			catch
			{
				LogTransaction("ER", SendRequestLogMessage);
				throw;
			}
			finally
			{
				if (kafkaClient.IsValueCreated) kafkaClient.Value.Dispose();
			}

			string SendRequestLogMessage() => $"{sendRequest.Messages.Length}|{String.Join("|", sendRequest.Messages.Select(x => x.MessageTrackingID))}";
		}

		void ForwardToProxyGateway(SendStreamRequest sendRequest, string userName, string password)
		{
			var proxyGatewayAddress = ConfigurationManager.AppSettings["ProxyGatewayAddress"];
			var binding = new eHubStreamedServiceConfiguration(new Uri(proxyGatewayAddress)).Binding;
			using (var factory = new ChannelFactory<IeHubStreamedService>(binding, proxyGatewayAddress))
			{
				factory.Credentials.UserName.UserName = userName;
				factory.Credentials.UserName.Password = password;

				foreach (var message in sendRequest.Messages)
				{
					message.MessageStream.Position = 0;
				}

				var channel = factory.CreateChannel();
				using (channel as IDisposable)
				{
					channel.SendStream(sendRequest);
				}
			}
		}

		public void CheckClientAuthorisation(string senderId, eHubGatewayMessage message)
		{
			if (IsIntegrationUserNamePasswordValidator())
			{
				var authorized = SecurityAccessor.CheckClientAuthorisation(senderId, message.ClientID);

				if (!authorized) throw new FaultException($"Client {senderId} is not authorised for sending to {message.ClientID}.");
			}
		}

		public virtual bool IsIntegrationUserNamePasswordValidator() =>
			OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.AuthenticationType ==
			"IntegrationUserNamePasswordValidator";

		protected virtual void HandleMessage(MessageHandler handler, Guid envelopTrackingId, eHubGatewayMessage message, string senderId)
		{
			handler.Handle(senderId, envelopTrackingId, message);
		}

		protected virtual void LogError(string message, Exception exception)
		{
			if (Logger.IsErrorEnabled) Logger.Error(message, exception);
		}

		public virtual string GetCurrentClientId()
		{
			return OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;
		}

		static Dictionary<Guid, Exception> HandleKafkaHandlerExceptions(Dictionary<Guid, Exception> messageExceptionDictionary, KeyValuePair<Guid, Exception>[] kafkaHandlerExceptions)
		{
			foreach (var handlerException in kafkaHandlerExceptions)
			{
				if (messageExceptionDictionary == null)
					messageExceptionDictionary = new Dictionary<Guid, Exception>();

				messageExceptionDictionary.Add(handlerException.Key, handlerException.Value);
			}

			return messageExceptionDictionary;
		}

		static void CombineExceptionsAndThrow(Dictionary<Guid, Exception> messageExceptionDictionary)
		{
			var errorBuilder = new StringBuilder();
			errorBuilder.AppendLine($"{messageExceptionDictionary.Count} errors occured during processing send request:");

			var exceptionsDetails = new StringBuilder();

			var serializableExceptionData = new SerializableDictionary<Guid, string>();

			foreach (var exData in messageExceptionDictionary)
			{
				string exceptionString;
				var exception = exData.Value;

				exceptionsDetails.AppendLine(exception.ToString());
				exceptionString = GetFullExceptionMessage(exception);

				if (serializableExceptionData.ContainsValue(exceptionString))
				{
					var existingExceptionKey = serializableExceptionData.FirstOrDefault(x => x.Value == exceptionString).Key;
					serializableExceptionData.Add(exData.Key, "This Exception is the same as " + existingExceptionKey);
				}
				else
				{
					serializableExceptionData.Add(exData.Key, exceptionString);
				}

				errorBuilder.Append(exceptionString);
			}

			var errorText = errorBuilder.ToString();
			throw new FaultException<ApplicationFault>(new ApplicationFault() { ErrorMessage = errorText, MessageExceptionDictionary = serializableExceptionData }, exceptionsDetails.ToString());
		}

		public RetrieveStreamResponse RetrieveStream()
		{
			RetrieveStreamResponse retrieveResponse = null;
			string recipientID = string.Empty;

			try
			{
				recipientID = GetCurrentClientId();
				string trackingID = Guid.NewGuid().ToString();
				retrieveResponse =
					new RetrieveStreamResponse() { TrackingID = trackingID, Messages = new eHubGatewayMessage[] { } };
				var accessor = OutboxAccessor;
				retrieveResponse.Messages = accessor.GetMessageBatch(recipientID, trackingID, NumberOfCandidates,
					BatchSizeLimitInBytes, OrderedDeliveryThresholds);
				TransformZACustomsClientID(retrieveResponse.Messages);
				LogTransaction("OK", RetrieveStreamLogMessage);
				return retrieveResponse;
			}
			catch (AggregateException ex) when (ex.InnerExceptions.Any(x => x is TimeoutException))
			{
				LogTransaction("ER", RetrieveStreamLogMessage);
				LogError($"{nameof(RetrieveStream)} has timed out for Recipient: {recipientID}", ex);

				return retrieveResponse;
			}
			catch
			{
				LogTransaction("ER", RetrieveStreamLogMessage);
				throw;
			}

			string RetrieveStreamLogMessage() => $"{retrieveResponse.TrackingID}|{retrieveResponse.Messages.Length}|{String.Join("|", retrieveResponse.Messages.Select(x => x.MessageTrackingID))}";
		}

		public RetrieveStreamByCompanyListResponse RetrieveStreamByCompanyList(RetrieveStreamByCompanyListRequest retrieveRequest)
		{
			RetrieveStreamByCompanyListResponse retrieveResponse = null;

			try
			{
				if (CheckForInvalidCompanyIDs(retrieveRequest.CompanyIDs))
					throw new FaultException($"One or more companies were not from the same client or were invalid.");
				if (CompanyListMaxCount != 0 && retrieveRequest.CompanyIDs.Length > CompanyListMaxCount)
					throw new FaultException($"Request contains too many companies to be processed (limit: {CompanyListMaxCount}).");

				var trackingId = Guid.NewGuid().ToString();
				retrieveResponse = new RetrieveStreamByCompanyListResponse() { TrackingID = trackingId, CompanyToMessagesMap = new Dictionary<string, eHubGatewayMessage[]>() };
				var remainingBatchSizeLimit = BatchSizeLimitInBytes;
				var remainingNumberOfCandidates = NumberOfCandidates;

				foreach (var companyId in retrieveRequest.CompanyIDs)
				{
					if (remainingBatchSizeLimit <= 0 || remainingNumberOfCandidates <= 0)
						break;

					var messages = OutboxAccessor.GetMessageBatch(companyId, trackingId, remainingNumberOfCandidates, remainingBatchSizeLimit, OrderedDeliveryThresholds);
					TransformZACustomsClientID(messages);
					retrieveResponse.CompanyToMessagesMap.Add(companyId, messages);

					remainingBatchSizeLimit -= messages.Sum(msg => msg.MessageStream?.Length ?? 0);
					remainingNumberOfCandidates -= messages.Length;
				}

				LogTransaction("OK", RetrieveStreamByCompanyListLogMessage);
				return retrieveResponse;
			}
			catch (AggregateException ex) when (ex.InnerExceptions.Any(x => x is TimeoutException))
			{
				LogTransaction("ER", RetrieveStreamByCompanyListLogMessage);
				LogError($"{nameof(RetrieveStreamByCompanyList)} has timed out for Recipients: [{string.Join(", ", retrieveRequest.CompanyIDs)}]", ex);

				return retrieveResponse;
			}
			catch
			{
				LogTransaction("ER", RetrieveStreamByCompanyListLogMessage);
				throw;
			}

			string RetrieveStreamByCompanyListLogMessage() => $"{retrieveResponse.TrackingID}|{string.Join("|", retrieveResponse.CompanyToMessagesMap.Select(pair => $"{pair.Key}|{pair.Value.Length}|{string.Join("|", pair.Value.Select(m => m.MessageTrackingID))}"))}";
		}

		public void FinaliseBatch(string requestID)
		{
			string clientId = string.Empty;
			try
			{
				clientId = GetCurrentClientId();
				var accessor = OutboxAccessor;
				accessor.UpdateBatchedMessagesDistributionStatuses(requestID);
				LogTransaction("OK", () => requestID);
			}
			catch (Exception exception)
			{
				LogTransaction("ER", () => requestID);
				throw new Exception($"Client: [{clientId}] encountered exception at {nameof(FinaliseBatch)}", exception);
			}
		}

		public Dictionary<string, MessageStatus> GetMessageStatuses(string[] trackingIDs)
		{
			var accessor = DataAccessFactories.NewInboxAccessorInstance();

			var result = new Dictionary<string, MessageStatus>();
			foreach (string trackingID in trackingIDs)
			{
				var inboxStatus = MessageStatus.Failed;

				string statusStr = accessor.GetMessageStatus(trackingID, GetCurrentClientId());
				if (!string.IsNullOrEmpty(statusStr))
					inboxStatus = (MessageStatus)Enum.Parse(typeof(MessageStatus), statusStr);

				result.Add(trackingID, inboxStatus);
			}

			return result;
		}

		bool CheckForInvalidCompanyIDs(string[] companyIds)
		{
			if (companyIds.Any(id => id.Length != 9))
			{
				return true;
			}

			var leadingPrefix = GetCurrentClientId().Substring(0, 3);
			var leadingSuffix = GetCurrentClientId().Substring(6);
			return companyIds.Any(id => !id.StartsWith(leadingPrefix) || !id.EndsWith(leadingSuffix));
		}

		void TransformZACustomsClientID(eHubGatewayMessage[] messages)
		{
			foreach (var message in messages)
			{
				if (message.ClientID == "ZACustomsTest" || message.ClientID == "ZACustomsDocTest" || message.ClientID == "ZACustomsDoc")
					message.ClientID = "ZACustoms";
			} 
		}

		internal static string GetFullExceptionMessage(Exception ex)
		{
			var exceptionMessage = new StringBuilder();
			if (ex is AggregateException aggregateException)
			{
				exceptionMessage.AppendLine(aggregateException.Message);
				foreach (var exception in aggregateException.InnerExceptions)
				{
					exceptionMessage.AppendLine(exception.Message);
					if (exception.InnerException != null)
						exceptionMessage.AppendLine((GetFullExceptionMessage(exception.InnerException)));
				}
			}
			else
			{
				exceptionMessage.AppendLine(ex.Message);
				if (ex.InnerException != null)
					exceptionMessage.AppendLine(GetFullExceptionMessage(ex.InnerException));
			}
			return exceptionMessage.ToString();
		}

		public virtual MessageHandler GetHandler(eHubGatewayMessage message)
		{
			var handler = MessageHandlerFactory.CreateMessageHandler(GetCurrentClientId(), message);
			return handler;
		}

		internal virtual Lazy<BillingKafkaClient> CreateBillingKafkaClient()
		{
			return new Lazy<BillingKafkaClient>(() => new BillingKafkaClient(CreateProducerConfig()));
		}

		internal static Lazy<IssueManager> IssueManger { get; set; } = new Lazy<IssueManager>();

		internal static ProducerConfig CreateProducerConfig()
		{
			var config = ServiceHelper.GetKafkaProducerConfig();
			return config;
		}

		public virtual IOutboxAccessor OutboxAccessor
		{
			get { return outboxAccessor ?? (outboxAccessor = DataAccessFactories.NewOutboxAccessorInstance()); }
		}
		IOutboxAccessor outboxAccessor;

		public virtual ISecurityAccessor SecurityAccessor => securityAccessor ?? (securityAccessor = DataAccessFactories.NewSecurityAccessorInstance());
		ISecurityAccessor securityAccessor;

		public virtual ITransformAccessor TransformAccessor => transformAccessor ?? (transformAccessor = DataAccessFactories.NewTransformAccessorInstance());
		ITransformAccessor transformAccessor;

		public virtual IEnterpriseExeDetailAccessor EnterpriseExeDetailAccessor => enterpriseExeDetailAccessor ?? (enterpriseExeDetailAccessor = DataAccessFactories.NewEnterpriseExeDetailAccessorInstance());
		IEnterpriseExeDetailAccessor enterpriseExeDetailAccessor;

		public virtual void LogTransaction(string status, Func<string> getDataFunc, [CallerMemberName] string caller = "")
		{
			string data = null;
			try
			{
				data = getDataFunc();
			}
			catch { }
			if (string.IsNullOrWhiteSpace(data)) data = "-";

			try
			{
				TransactionLog.InfoFormat("{0} {1} {2} {3} {4} {5} {6}",
										OperationContext.Current.RequestContext.RequestMessage.Headers.To.Host,
										OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name,
										HttpContext.Current.Request.Headers["X-Forwarded-For"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"],
										caller,
										status,
										watch.ElapsedMilliseconds,
										data);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message);
			}
		}
	}

	class MessageHandlerCache
	{
		public MessageHandlerCache(int size)
		{
			Handlers = new MessageHandler[size];
		}

		public void CacheHandler(int index, MessageHandler handler)
		{
			Handlers[index] = handler;
		}

		public MessageHandler GetHandler(int index)
		{
			return Handlers[index];
		}

		public MessageHandler[] Handlers { get; }
	}

	class ThrottleChecker
	{
		public void PerformThrottleCheck(MessageHandler handler, string senderId, string recipientId)
		{
			var tuple = new Tuple<string, string, string>(handler.GetType().FullName, senderId, recipientId);
			if (!checkedSet.Contains(tuple))
			{
				handler.CheckIfTooManyUnprocessedMessages(senderId, recipientId);
				checkedSet.Add(tuple);
			}
		}

		HashSet<Tuple<string, string, string>> checkedSet = new HashSet<Tuple<string, string, string>>();
	}
}
