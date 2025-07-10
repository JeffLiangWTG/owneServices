using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using Enterprise.DocumentVisualizer.Business.RoutingRuleValidationWebService;
using Enterprise.Registry.Business;
using Polly;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class EHubRoutingRuleValidatorService : IRoutingRuleValidatorService
	{
		public EHubRoutingRuleValidatorService()
			: this(new RoutingRuleValidationCheck())
		{
		}

		public EHubRoutingRuleValidatorService(IRoutingRuleValidationCheck service)
		{
			_ = service ?? throw new ArgumentNullException(nameof(service));
			this.service = service;
		}

		readonly IRoutingRuleValidationCheck service;

		public RoutingRuleValidatorResponse PerformValidationCheck(RoutingRuleValidatorRequest request)
		{
			_ = request ?? throw new ArgumentNullException(nameof(request));

			if (string.IsNullOrWhiteSpace(request.ClientId)
				|| string.IsNullOrWhiteSpace(request.Password)
				|| string.IsNullOrWhiteSpace(request.Interchange))
			{
				return new RoutingRuleValidatorResponse();
			}

			var endpoint = new EndpointAddress(request.ServiceUrl);

			const int minTimeoutInSeconds = 5;

			var timeoutInSeconds = Math.Max(FreightDataRegistry.Instance.CarrierMessagingConnectionValidationTimeout.Value, minTimeoutInSeconds);

			var binding = new CustomBinding
			{
				Elements =
				{
					new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8),
					SecurityBindingElement.CreateUserNameOverTransportBindingElement(),
					new HttpsTransportBindingElement(),
				},
				OpenTimeout = TimeSpan.FromSeconds(timeoutInSeconds),
				SendTimeout = TimeSpan.FromSeconds(timeoutInSeconds * 4)
			};

			var channelFactory = new ChannelFactory<IRoutingRuleValidationWebService>(binding, endpoint);

			if (channelFactory.Credentials?.UserName != null)
			{
				channelFactory.Credentials.UserName.UserName = request.ClientId;
				channelFactory.Credentials.UserName.Password = request.Password;
			}

			var messageContent = Encoding.UTF8.GetBytes(request.Interchange);
			var routingEvaluationOCMInput = new RoutingEvaluationOCMInput
			{
				Message = messageContent
			};

			var retryPolicy = Policy
				.Handle<Exception>()
				.OrTransientException()
				.WaitAndRetry(new[]
				{
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(1),
					TimeSpan.FromSeconds(3),
				});

			return retryPolicy.Execute(() =>
			{
				return service.SendValidationCheck(routingEvaluationOCMInput, channelFactory);
			});
		}
	}
}
