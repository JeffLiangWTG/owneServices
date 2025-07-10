#if !NETFRAMEWORK
using System;
using System.ServiceModel;
using System.Xml;
#endif
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.TestClient.LocalService;

namespace Enterprise.DataTransfer.Native.TestClient
{
	class ServiceClientWrapper
	{
#if NETFRAMEWORK
		string endpointConfigurationName;
#endif
		readonly string remoteAddress;

		public ServiceClientWrapper(string remoteAddress)
		{
			this.remoteAddress = remoteAddress;
		}

		EnterpriseNativeDataServiceClient CreateServiceClient()
		{
#if NETFRAMEWORK
			endpointConfigurationName = "WSHttpBinding_EnterpriseNativeDataService";
#endif
			EnterpriseNativeDataServiceClient service;
			if (string.IsNullOrEmpty(remoteAddress))
			{
				service = new EnterpriseNativeDataServiceClient();
			}
			else
			{
#if NETFRAMEWORK
				service = new EnterpriseNativeDataServiceClient(endpointConfigurationName, remoteAddress);
#else
				service = new EnterpriseNativeDataServiceClient(GetWSHttpBinding(), new EndpointAddress(remoteAddress));
#endif
			}
			return service;
		}

#if !NETFRAMEWORK
		WSHttpBinding GetWSHttpBinding()
		{
			return new WSHttpBinding
			{
				CloseTimeout = TimeSpan.FromMinutes(1),
				OpenTimeout = TimeSpan.FromMinutes(1),
				ReceiveTimeout = TimeSpan.FromMinutes(10),
				SendTimeout = TimeSpan.FromMinutes(1),
				BypassProxyOnLocal = false,
				TransactionFlow = false,
				MaxBufferPoolSize = 524288,
				MaxReceivedMessageSize = 65536,
				MessageEncoding = WSMessageEncoding.Text,
				TextEncoding = System.Text.Encoding.UTF8,
				UseDefaultWebProxy = true,
				AllowCookies = false,
				Security = new WSHttpSecurity
				{
					Mode = SecurityMode.None,
					Transport = new HttpTransportSecurity
					{
						ClientCredentialType = HttpClientCredentialType.Windows,
						ProxyCredentialType = HttpProxyCredentialType.None
					},
					Message = new NonDualMessageSecurityOverHttp
					{
						ClientCredentialType = MessageCredentialType.Windows,
						NegotiateServiceCredential = true
					}
				},
				ReaderQuotas = new XmlDictionaryReaderQuotas
				{
					MaxDepth = 32,
					MaxStringContentLength = 8192,
					MaxArrayLength = 16384,
					MaxBytesPerRead = 4096,
					MaxNameTableCharCount = 16384
				},
				ReliableSession = new OptionalReliableSession
				{
					Ordered = true,
					InactivityTimeout = TimeSpan.FromMinutes(10),
					Enabled = false
				}
			};
		}
#endif

		internal string Retrieve(string requestXmlString)
		{
			var requestElement = XElement.Parse(requestXmlString);

			var dataServiceClient = CreateServiceClient();

			XElement responseElement;
			using (dataServiceClient)
			{
				responseElement = dataServiceClient.Retrieve(requestElement);
				dataServiceClient.Close();
			}

			return responseElement.ToString();
		}

		internal string Update(string requestXmlString)
		{
			var requestElement = XElement.Parse(requestXmlString);

			var dataServiceClient = CreateServiceClient();

			XElement responseElement;
			using (dataServiceClient)
			{
				responseElement = dataServiceClient.Update(requestElement);
				dataServiceClient.Close();
			}

			return responseElement.ToString();
		}
	}
}
