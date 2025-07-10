using System;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequestStringAttribute : Attribute
	{
		public RequestStringAttribute(string requestString)
		{
			fRequestString = requestString;
		}

		public string RequestString
		{
			get { return fRequestString; }
		}

		readonly string fRequestString;
	}
}
