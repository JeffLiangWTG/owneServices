#if NET
using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Dispatcher;

namespace Enterprise.Accounting.Web;

public class SecurityHeaderMessageInspector : IDispatchMessageInspector
{
	const string SecurityHeaderNamespace = "http://cargowise.com/Accounting/";

	public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
	{
		var securityHeader = new SecuritySOAPHeader();
		var headerIndex = request.Headers.FindHeader(nameof(SecuritySOAPHeader), SecurityHeaderNamespace);
		if (headerIndex != -1)
		{
			var serviceInstance = (BaseService)instanceContext.GetServiceInstance();
			securityHeader = request.Headers.GetHeader<SecuritySOAPHeader>(headerIndex);
			serviceInstance.SecurityHeader = securityHeader;
		}

		return securityHeader;
	}

	public void BeforeSendReply(ref Message reply, object correlationState)
	{
		// Do nothing -- we do not send any special headers back to our client
	}
}
#endif
