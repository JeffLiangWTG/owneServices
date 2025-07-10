using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.WebServiceDelivery.LocalService
{
	public partial class EnterpriseNativeDataServiceClient : INativeServiceClient
	{
		public IConverter<IRequestMessage, XElement> RequestConverter { get; set; }
		public IConverter<XElement, IResponseMessage> ResponseConverter { get; set; }

		public EnterpriseNativeDataServiceClient(Uri uri)
			: this(GetBinding(), GetAddress(uri))
		{
			RequestConverter = new RequestConverter();
			ResponseConverter = new ResponseConverter();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Web Service Identity")]
		static EndpointAddress GetAddress(Uri uri)
		{
#if NETFRAMEWORK
			var identity = EndpointIdentity.CreateDnsIdentity("localhost");
			return new EndpointAddress(uri, identity, new AddressHeaderCollection());
#else
			var identity = new DnsEndpointIdentity("localhost");
			return new EndpointAddress(uri, identity, Array.Empty<AddressHeader>());
#endif
		}

		public static Binding GetBinding()
		{
			var binding = new WSHttpBinding();
			binding.Security.Mode = SecurityMode.None;
			binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
			binding.SendTimeout = new TimeSpan(0, 5, 0);
			return binding;
		}

		public IResponseMessage Update(IRequestMessage input)
		{
			var sessionServices = new AncillaryImportServices();
			var updateRequest = RequestConverter.Convert(input);
			var response = Update(updateRequest);
			var output = ResponseConverter.Convert(response);
			return output;
		}
	}
}
