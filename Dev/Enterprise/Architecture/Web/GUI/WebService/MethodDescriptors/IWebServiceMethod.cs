using System.Collections.Generic;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public interface IWebServiceMethod
	{
		string MethodName { get; }
		Dictionary<string, ZWebResource> ServiceScripts(ZPage page);
		ServiceReference WebServiceReference { get; }
		WebServiceResponse Execute(string parameters);
	}
}
