using System;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class QueryStringAttribute : Attribute
	{
		public QueryStringAttribute(string queryString)
		{
			fQueryString = queryString;
		}

		public string QueryString
		{
			get { return fQueryString; }
		}
		readonly string fQueryString;
	}
}
