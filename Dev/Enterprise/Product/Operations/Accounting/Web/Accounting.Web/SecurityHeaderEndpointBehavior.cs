#if NET
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Dispatcher;

namespace Enterprise.Accounting.Web;
public class SecurityHeaderEndpointBehaviour : IEndpointBehavior
{
	public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
	{
	}

	public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
	{
	}

	public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
	{
		// This method is implemented to intercept messages sent from the client for security header.
		var inspector = new SecurityHeaderMessageInspector();
		endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
	}

	public void Validate(ServiceEndpoint endpoint)
	{
	}
}
#endif
