using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Enterprise.Customs.GB.MCP.CusDec.destin8WebService;

public class UserAgentEndpointBehavior : IEndpointBehavior
{
	readonly string userAgent;

	public UserAgentEndpointBehavior(string userAgent)
	{
		this.userAgent = userAgent;
	}

	public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
	public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
	public void Validate(ServiceEndpoint endpoint) { }

	public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
	{
		clientRuntime.MessageInspectors.Add(new UserAgentMessageInspector(userAgent));
	}

	class UserAgentMessageInspector : IClientMessageInspector
	{
		readonly string userAgent;

		public UserAgentMessageInspector(string userAgent)
		{
			this.userAgent = userAgent;
		}

		public void AfterReceiveReply(ref Message reply, object correlationState) { }

		public object BeforeSendRequest(ref Message request, IClientChannel channel)
		{
			var httpRequest = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty
				?? new HttpRequestMessageProperty();

			httpRequest.Headers["User-Agent"] = userAgent;
			request.Properties[HttpRequestMessageProperty.Name] = httpRequest;
			return null;
		}
	}
}
