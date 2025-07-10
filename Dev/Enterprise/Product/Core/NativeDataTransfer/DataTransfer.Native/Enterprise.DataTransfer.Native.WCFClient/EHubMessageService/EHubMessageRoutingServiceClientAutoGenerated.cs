using System.CodeDom.Compiler;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Enterprise.DataTransfer.Native.WCFClient.EHubMessageService
{
	[GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
	[ServiceContractAttribute(Namespace = "http://www.cargowise.com/", ConfigurationName = "EHubMessageRoutingService")]
	public interface IEHubMessageRoutingService
	{
		[OperationContractAttribute(Action = "Send", ReplyAction = "*")]
		[XmlSerializerFormatAttribute()]
		SendResponse Send(SendRequest request);
	}

	[GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
	public interface WcfService_CargoWise_eHub_MessageRoutingChannel : IEHubMessageRoutingService, IClientChannel
	{
	}

	[DebuggerStepThrough()]
	[GeneratedCode("System.ServiceModel", "3.0.0.0")]
	public partial class EHubMessageRoutingServiceClient : ClientBase<IEHubMessageRoutingService>, IEHubMessageRoutingService
	{
		public EHubMessageRoutingServiceClient()
		{
		}

#if NETFRAMEWORK
		public EHubMessageRoutingServiceClient(string endpointConfigurationName)
			: base(endpointConfigurationName)
		{
		}

		public EHubMessageRoutingServiceClient(string endpointConfigurationName, string remoteAddress)
			: base(endpointConfigurationName, remoteAddress)
		{
		}

		public EHubMessageRoutingServiceClient(string endpointConfigurationName, EndpointAddress remoteAddress)
			: base(endpointConfigurationName, remoteAddress)
		{
		}
#else
		public EHubMessageRoutingServiceClient(System.ServiceModel.Description.ServiceEndpoint endpoint)
			: base(endpoint)
		{
		}

		public EHubMessageRoutingServiceClient(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
			: base(binding, remoteAddress)
		{
		}
#endif
		public EHubMessageRoutingServiceClient(Binding binding, EndpointAddress remoteAddress)
			: base(binding, remoteAddress)
		{
		}

		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
		SendResponse IEHubMessageRoutingService.Send(SendRequest request)
		{
			return base.Channel.Send(request);
		}

		public MessageSendResponse Send(MessageSendRequest messageSendRequest)
		{
			var inValue = new SendRequest();
			inValue.MessageSendRequest = messageSendRequest;
			var retValue = ((IEHubMessageRoutingService)(this)).Send(inValue);

			return retValue.MessageSendResponse;
		}

		public SendResponse Send(SendRequest request)
		{
			return base.Channel.Send(request);
		}
	}
}
