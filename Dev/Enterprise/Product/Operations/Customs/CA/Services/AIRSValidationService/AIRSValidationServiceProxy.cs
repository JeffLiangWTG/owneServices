using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.Customs.CA.Services.AIRSValidationService;

namespace Enterprise.Customs.CA.Services
{
	public interface IAIRSValidationServiceSettings
	{
		ZString Uri { get; }
		string SchemaVersion { get; }
		string Key { get; }
		bool FrenchPreferred { get; }
		string UserName { get; }
		string Password { get; }
		ZString WebProxyUri { get; }
	}

	public class AIRSValidationServiceProxy
	{
		public AIRSValidationServiceProxy(IAIRSValidationServiceSettings settings)
		{
			this.settings = settings;
			WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
		}
		protected readonly IAIRSValidationServiceSettings settings;

		#region ValidateRequirements

		internal BrokerValidationServiceClient GetNewBVSClient()
		{
			var result = Uri.TryCreate(settings.Uri, UriKind.Absolute, out var uriResult);
			var endPointAddress = new EndpointAddress(settings.Uri);
			var binding = (result && uriResult.Scheme == Uri.UriSchemeHttps)
				? CreateBasicHttpsBinding(settings)
				: CreateBasicHttpBinding(settings);

#if DEBUG
			BindingForTest = binding;
#endif

			return new BrokerValidationServiceClient(binding, endPointAddress);
		}

#if DEBUG
		public System.ServiceModel.Channels.Binding BindingForTest;
#endif

		System.ServiceModel.Channels.Binding CreateBasicHttpsBinding(IAIRSValidationServiceSettings seviceSettings)
		{
			var binding = new BasicHttpsBinding
			{
				UseDefaultWebProxy = true,
				MaxReceivedMessageSize = int.MaxValue,
				MaxBufferSize = int.MaxValue
			};
			if (!string.IsNullOrEmpty(seviceSettings.WebProxyUri))
			{
				Uri uriResult;
				if (Uri.TryCreate(seviceSettings.WebProxyUri, UriKind.Absolute, out uriResult))
				{
					binding.UseDefaultWebProxy = false;
					binding.ProxyAddress = uriResult;
				}
			}
			if (!string.IsNullOrEmpty(seviceSettings.UserName))
			{
				binding.Security.Mode = BasicHttpsSecurityMode.Transport;
				binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
			}
			return binding;
		}

		System.ServiceModel.Channels.Binding CreateBasicHttpBinding(IAIRSValidationServiceSettings seviceSettings)
		{
			var binding = new BasicHttpBinding
			{
				UseDefaultWebProxy = true,
				MaxReceivedMessageSize = int.MaxValue,
				MaxBufferSize = int.MaxValue
			};
			Uri uriResult;
			var result = Uri.TryCreate(seviceSettings.WebProxyUri, UriKind.Absolute, out uriResult);
			if (!string.IsNullOrEmpty(seviceSettings.WebProxyUri) && result)
			{
				binding.UseDefaultWebProxy = false;
				binding.ProxyAddress = uriResult;
			}
			if (!string.IsNullOrEmpty(seviceSettings.UserName))
			{
				binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
				binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
			}
			return binding;
		}

		void SetUpBVSClient(BrokerValidationServiceClient client)
		{
			if (!string.IsNullOrEmpty(settings.UserName))
			{
				client.ClientCredentials.UserName.UserName = settings.UserName;
				client.ClientCredentials.UserName.Password = settings.Password;
			}
		}

		public void ValidateRequirements(IEnumerable<IAIRSValidationQueriedLine> lines, CancellationTokenSource cancellationTokenSource)
		{
			using (var bvsClient = GetNewBVSClient())
			{
				SetUpBVSClient(bvsClient);

				var requestObjects = GenerateValidateTransaction(lines);
				foreach (var requestObject in requestObjects)
				{
					ValidateRequirements(bvsClient, requestObject, lines, cancellationTokenSource);

					if (cancellationTokenSource.IsCancellationRequested)
					{
						PopulateValidateTransactionFault(lines, ValidationFaultMessageType.QueryAborted, ValidationQueryAbortedMessage);
						break;
					}
				}
			}
		}

		void ValidateRequirements(BrokerValidationServiceClient bvsClient, IValidateTransaction requestObject, IEnumerable<IAIRSValidationQueriedLine> lines, CancellationTokenSource cancellationTokenSource)
		{
			byte[] xmlContent = requestObject.Transaction.Serialize();

			Task<byte[]> task = null;
			try
			{
				task = ValidateRequirementsCore(bvsClient, xmlContent, requestObject, lines, cancellationTokenSource);
				if (task != null)
				{
					if (task.IsCompleted)
					{
						var response = task.Result.Deserialize<ValidateTransactionResult>();
						PopulateValidateTransactionResult(requestObject, response, lines);
					}
					if (task.IsFaulted)
					{
						string exceptionMessage = task.Exception.Message;
						PopulateValidateTransactionFault(requestObject, lines, ValidationFaultMessageType.HttpError, exceptionMessage);
					}
				}
			}
			catch (Exception e)
			{
				HandleExceptions(e, requestObject, lines);
			}
			finally
			{
				if (task != null)
				{
					task.Dispose();
				}
			}
		}

		protected virtual void HandleExceptions(Exception e, IValidateTransaction requestObject, IEnumerable<IAIRSValidationQueriedLine> lines)
		{
			if (e is CommunicationException)
			{
				var errorMessage = "Cannot connect to the AIRS Validation Service";
				PopulateValidateTransactionFault(requestObject, lines, ValidationFaultMessageType.HttpError, errorMessage);
			}
			else if (e is WebException)
			{
				var errorMessage = "There's network issue to connect to the AIRS Validation Service. Please try later.";
				PopulateValidateTransactionFault(requestObject, lines, ValidationFaultMessageType.HttpError, errorMessage);
			}
			else if (e is TimeoutException timeoutException)
			{
				PopulateValidateTransactionFault(requestObject, lines, ValidationFaultMessageType.HttpError, timeoutException.Message);
			}
			else if (e is AggregateException aggregateException)
			{
				var innerException = aggregateException.InnerException as FaultException<ServiceFaultContract>;
				if (innerException != null)
				{
					PopulateFaultException(requestObject, lines, innerException);
				}
				else
				{
					string errorMessage = string.Join(" ", aggregateException.InnerExceptions.Select(x => x.Message));
					PopulateValidateTransactionFault(requestObject, lines, ValidationFaultMessageType.HttpError, errorMessage);
				}
			}
		}

		void PopulateFaultException(IValidateTransaction requestObject, IEnumerable<IAIRSValidationQueriedLine> lines, FaultException<ServiceFaultContract> ex)
		{
			var soapError = string.Format(CultureInfo.CurrentCulture, "ErrorCode:{0} ErrorMessage:{1}\r\nErrorDetail:{2}\r\nRequestId:{3}",
						   ex.Detail.ErrorCode, ex.Detail.ErrorMessage, ex.Detail.ErrorDetail, ex.Detail.RequestId);
			PopulateValidateTransactionFault(requestObject, lines, ValidationFaultMessageType.SOAPError, soapError);
		}

		const string ValidationQueryAbortedMessage = "AIRS Validation Query Aborted";

		protected virtual Task<byte[]> ValidateRequirementsCore(BrokerValidationServiceClient bvsClient, byte[] xmlContent, IValidateTransaction requestObject, IEnumerable<IAIRSValidationQueriedLine> lines, CancellationTokenSource cancellationTokenSource)
		{
			var task = GetValidationTask(bvsClient, xmlContent);
			task.ContinueWith(t =>
			{
				HandleExceptions(t.Exception, requestObject, lines);
			}, TaskContinuationOptions.OnlyOnFaulted);
			task.Wait(cancellationTokenSource.Token);
			return task;
		}

		protected virtual Task<byte[]> GetValidationTask(BrokerValidationServiceClient bvsClient, byte[] xmlContent)
		{
			return bvsClient.ValidateRequirementsAsync(xmlContent, settings.SchemaVersion, settings.Key, settings.FrenchPreferred ? 2 : 1);
		}

		#endregion

		#region Request

		internal IEnumerable<IValidateTransaction> GenerateValidateTransaction(IEnumerable<IAIRSValidationQueriedLine> lines)
		{
			var valueObject = CreateNewValidateTransaction();
			if (valueObject != null)
			{
				foreach (var line in lines)
				{
					if (IsCommoditiesNumberLimitExceed(valueObject))
					{
						yield return valueObject;
						valueObject = CreateNewValidateTransaction();
					}

					if (valueObject != null)
					{
						valueObject.AddNewCommodity(line);

						if (IsMessageSizeLimitExceed(valueObject))
						{
							valueObject.RemoveLastCommodity(line.CommodityGroup);
							yield return valueObject;

							valueObject = CreateNewValidateTransaction();
							if (valueObject != null)
							{
								valueObject.AddNewCommodity(line);
							}
						}
					}
				}
			}

			yield return valueObject;
		}

		IValidateTransaction CreateNewValidateTransaction()
		{
			IValidateTransaction result = null;

			switch (settings.SchemaVersion)
			{
				case AIRSValidationServiceSchemaVersion.OGD:
					result = new ValidateOGDTransactionProto(new ValidateTransaction());
					break;
				case AIRSValidationServiceSchemaVersion.IID:
					result = new ValidateIIDTransactionProto(new ValidateIIDTransaction());
					break;
			}

			if (result != null)
			{
				result.Transaction.ShouldCreateElementForEmptyValue = false;
			}

			return result;
		}

#if DEBUG
		internal virtual
#endif
		int MessageSizeLimit
		{ get { return 500 * 1024; } }

		int CommoditiesNumberLimit { get { return 400; } }

		bool IsCommoditiesNumberLimitExceed(IValidateTransaction valueObject)
		{
			return valueObject.CommoditiesNumber >= CommoditiesNumberLimit;
		}

		bool IsMessageSizeLimitExceed(IValidateTransaction valueObject)
		{
			return valueObject.Transaction.Serialize().Length >= MessageSizeLimit;
		}

		#endregion

		#region Response

		void PopulateValidateTransactionResult(IValidateTransaction request, ValidateTransactionResult result, IEnumerable<IAIRSValidationQueriedLine> lines)
		{
			foreach (ValidateTransactionResultCommodityGroup commodityGroup in result.CommodityGroup)
			{
				foreach (ValidateTransactionResultCommodityGroupCommodity commodity in commodityGroup.Commodity)
				{
					var line = lines.FirstOrDefault(l => l.Commodity == commodity.commodityId);
					if (line != null)
					{
						line.ValidationFaultMessageType = ValidationFaultMessageType.None;
						line.ValidationResponse = commodity.Error;
						line.ValidationCompleted = true;
					}
				}
			}

			foreach (var commodityId in request.GetAllCommodityGroupIds())
			{
				var line = lines.FirstOrDefault(l => l.Commodity == commodityId);
				if (line != null && !line.ValidationCompleted)
				{
					line.ValidationFaultMessageType = ValidationFaultMessageType.None;
					line.ValidationCompleted = true;
				}
			}
		}

		void PopulateValidateTransactionFault(IValidateTransaction request, IEnumerable<IAIRSValidationQueriedLine> lines, ValidationFaultMessageType messageType, string faultMessage)
		{
			foreach (var commodityId in request.GetAllCommodityGroupIds())
			{
				var line = lines.FirstOrDefault(l => l.Commodity == commodityId);
				if (line != null)
				{
					line.ValidationFaultMessageType = messageType;
					line.ValidationFaultMessage = faultMessage;
					line.ValidationCompleted = true;
				}
			}
		}

		void PopulateValidateTransactionFault(IEnumerable<IAIRSValidationQueriedLine> lines, ValidationFaultMessageType messageType, string faultMessage)
		{
			foreach (var line in lines)
			{
				if (!line.ValidationCompleted)
				{
					line.ValidationFaultMessageType = messageType;
					line.ValidationFaultMessage = faultMessage;
				}
			}
		}

		#endregion
	}
}
