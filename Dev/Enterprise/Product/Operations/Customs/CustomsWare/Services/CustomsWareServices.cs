#if NET
using Enterprise.Customs.DataRegistry.Business;
#endif
using System;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CustomsWare.Services
{
	public interface ICustomsForceWebServiceSettings
	{
		string Uri { get; }
		string UserName { get; }
		string Password { get; }
		string Company { get; }
		string ApplicationID { get; }
	}

	public interface ICustomsForceWebService
	{
		XElement ExecAPI(BasicHttpBinding binding, EndpointAddress endpointAddress, XElement request);
	}

	public class CustomsWareServices : CusIntegrationService, ICustomsForceWebService
	{
		public XElement ExecApiSafe(ICustomsForceWebServiceSettings settings, string inputXML)
		{
			try
			{
				return ExecAPI(settings, inputXML);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				return GetExceptionSubmissionResult(e);
			}
		}

		public static XElement GetExceptionSubmissionResult(Exception e)
		{
			var sb = new ZStringBuilder();
			var otherException = e;
			while (otherException != null)
			{
				sb.Append(otherException.Message);
				otherException = otherException.InnerException;
			}

			return new XElement("ErrorItem",
				new XElement("ErrorDescription", sb.ToStringWithNewLineBetweenAppends()));
		}

		public XElement ExecAPI(ICustomsForceWebServiceSettings settings, string inputXML)
		{
			var binding = CreateHttpBinding(settings);
			var endPointAddress = new EndpointAddress(settings.Uri);
			var request = BuildRequest(settings.UserName, settings.Password, settings.Company, settings.ApplicationID, inputXML);
			return ObjectFactory.Get<ICustomsForceWebService>().ExecAPI(binding, endPointAddress, request);
		}

		public BasicHttpBinding CreateHttpBinding(ICustomsForceWebServiceSettings settings)
		{
			var binding = HttpBinding;
			var uri = new Uri(settings.Uri);
			binding.Security.Mode = uri.Scheme == Uri.UriSchemeHttps ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None;
			return binding;
		}

		//to be removed once the implementation for HttpBinding is standardized for NETCORE
		#if NET
		new BasicHttpBinding HttpBinding
		{
			get
			{
				return _httpBinding ??= new BasicHttpBinding
				{
					SendTimeout = new TimeSpan(0, 0, CustomsDataRegistry.Instance.WebServiceTimeoutInSeconds.Value)
				};
			}
		}
		BasicHttpBinding _httpBinding;
		#endif

		XElement ICustomsForceWebService.ExecAPI(BasicHttpBinding binding, EndpointAddress endpointAddress, XElement request)
		{
			using (var webService = new CustomsForceWebService.CustomsForceWebServiceSoapClient(binding, endpointAddress))
			{
				var xmlRequest = ToXmlElement(request);
				return ToXElement(webService.ExecAPI(xmlRequest));
			}
		}

		XElement BuildRequest(string userName, string password, string company, string applicationID, string inputXML)
		{
			XElement result = new XElement("CustomsForceServiceRequest");
			result.Add(
				new XElement("MessageHeader",
					new XElement("SecurityToken",
						new XElement("UserName", userName),
						new XElement("Password", password),
						new XElement("Company", company),
						new XElement("ApplicationId", applicationID)
					)
				),

				new XElement("MessageBody",
					new XElement("RequestList",
						new XElement("RequestItem",
							new XElement("DataList",
								new XElement("DataItem", XElement.Parse(inputXML)))))));

			return result;
		}

		static XmlElement ToXmlElement(XElement xElement)
		{
			var doc = new XmlDocument();
			using (var reader = xElement.CreateReader())
			{
				doc.Load(reader);
			}
			return doc.DocumentElement;
		}
		static XElement ToXElement(XmlElement xmlElement)
		{
			using (var reader = new XmlNodeReader(xmlElement))
			{
				return XElement.Load(reader);
			}
		}
	}
}
