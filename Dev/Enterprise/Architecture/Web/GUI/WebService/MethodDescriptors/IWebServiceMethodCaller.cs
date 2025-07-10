using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public interface IWebServiceMethodsCaller
	{
		List<IWebServiceMethod> WebServiceMethods { get; }
	}
}
