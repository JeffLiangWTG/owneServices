using System;
using System.ServiceModel;
using System.Xml.Linq;
using CargoWise.Application;

namespace Enterprise.Customs.CustomsWare.Services.Testing
{
	public class CustomsForceWebServiceForTesting : ICustomsForceWebService
	{
		public static IDisposable Setup(Func<BasicHttpBinding, EndpointAddress, XElement, XElement> execAPIForTesting)
		{
			return ObjectFactory.Substitute<ICustomsForceWebService>(new CustomsForceWebServiceForTesting { ExecAPIForTesting = execAPIForTesting });
		}

		public Func<BasicHttpBinding, EndpointAddress, XElement, XElement> ExecAPIForTesting;
		public XElement ExecAPI(BasicHttpBinding binding, EndpointAddress endpointAddress, XElement request) => ExecAPIForTesting(binding, endpointAddress, request);
	}
}
