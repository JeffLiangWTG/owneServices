using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Enterprise.DataTransfer.Native.WCFClient
{
	public static class BindingFactory
	{
		public static Binding EHubBinding()
		{
			var binding = new WSHttpBinding();
			binding.Security.Mode = SecurityMode.Transport;
			binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
			binding.Security.Message.NegotiateServiceCredential = true;
			binding.Security.Message.EstablishSecurityContext = true;
			return binding;
		}
	}
}
