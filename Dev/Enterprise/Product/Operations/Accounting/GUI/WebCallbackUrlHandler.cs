using System;
using CargoWise.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.GUI
{
	public class WebCallbackUrlHandler(Action<QueryString> handleCallback) : UrlHandler
	{
		readonly Action<QueryString> Callback = handleCallback;

		protected override string ExpectedCommandText => "Acc.WebCallback";

		public QueryString LastQueryString { get; private set; }

		protected override bool CanHandleCore(QueryString queryString)
		{
			return queryString["Command"]?.StartsWith(ExpectedCommandText) ?? false;
		}

		protected override bool HandleCore(QueryString queryString)
		{
			LastQueryString = queryString;

			Callback(queryString);

			return true;
		}
	}
}
