using System;
using System.Collections.Generic;
using System.Web.UI;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	public abstract class WebServiceMethod<ParametersType> : IWebServiceMethod
		where ParametersType : WebServiceParameters
	{
		#region Properties

		#region MethodName

		public string MethodName
		{
			get { return GetMethodName(); }
		}

		protected abstract string GetMethodName();

		#endregion

		#region ServiceScripts

		public Dictionary<string, ZWebResource> ServiceScripts(ZPage page)
		{
			Dictionary<string, ZWebResource> result = new Dictionary<string, ZWebResource>();
			if (page != null)
			{
				ZWebResource commonScript = GetCommonScript(page);
				if (commonScript != null)
				{
					result.Add(CommonScriptKey, commonScript);
				}
				AddServiceScripts(page, result);
			}
			return result;
		}

		protected virtual void AddServiceScripts(ZPage page, Dictionary<string, ZWebResource> serviceScripts)
		{
			ZWebResource script = GetScript(page);
			if (script != null)
			{
				serviceScripts.Add(ScriptKey, script);
			}
		}

		#endregion

		#region WebServiceReference

		public ServiceReference WebServiceReference
		{
			get { return GetWebServiceReference(); }
		}

		protected virtual ServiceReference GetWebServiceReference()
		{
			return WebServices.Instance.SharedWebServiceReference;
		}

		#endregion

		#endregion

		#region Methods

		public WebServiceResponse Execute(string parameters)
		{
			var response = new WebServiceResponse();
			try
			{
				var methodParameters = WebServiceParameters.Parse<ParametersType>(parameters)
					?? throw new Exception("Unable to parse method parameters");
				ExecuteCore(methodParameters, response);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (AddErrorMessageToResponse())
				{
					response.Add(new ShowErrorResponseToken(e));
				}
			}
			return response;
		}

		protected abstract void ExecuteCore(ParametersType parameters, WebServiceResponse response);

		#endregion

		#region Implementation

		protected virtual bool AddErrorMessageToResponse()
		{
			return true;
		}

		protected virtual ZWebResource GetCommonScript(ZPage page)
		{
			return new ZWebResource(typeof(IWebServiceMethod), "WebServiceMethod.js", page, "Enterprise.ZArchitecture.Web.GUI.WebService.JavaScripts");
		}

		protected string CommonScriptKey
		{
			get { return GetCommonScriptKey(); }
		}

		protected virtual string GetCommonScriptKey()
		{
			return "WebServiceMethodScriptKey";
		}

		protected virtual string GetScriptKey()
		{
			return MethodName + "WebServiceMethodScriptKey";
		}

		protected virtual ZWebResource GetScript(ZPage page)
		{
			return new ZWebResource(GetScriptType(), GetScriptFileName(), page, GetScriptLocation());
		}

		protected virtual Type GetScriptType()
		{
			return typeof(IWebServiceMethod);
		}

		protected virtual string GetScriptLocation()
		{
			return "Enterprise.ZArchitecture.Web.GUI.WebService.JavaScripts";
		}

		protected virtual string GetScriptFileName()
		{
			return MethodName + "WebServiceMethod.js";
		}

		protected string ScriptKey
		{
			get { return GetScriptKey(); }
		}

		#endregion
	}
}
