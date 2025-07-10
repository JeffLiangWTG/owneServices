using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CargoWise.Types;

namespace Enterprise.Accounting.Business
{
	public static class HttpExtensions
	{
		public static string GetAsString(this HttpHeaders headers)
		{
			#region SuppressResourceStringsCheckRegion

			var result = new ZStringBuilder();

			if (headers != null)
			{
				foreach (var headerItem in headers)
				{
					result.Append(FormattableString.Invariant($"{headerItem.Key} : {string.Join(", ", headerItem.Value.ToArray())}"));
				}
			}

			return result.ToStringWithNewLineBetweenAppends();

			#endregion
		}

		public static string GetAsString(this HttpResponseMessage response)
		{
			#region SuppressResourceStringsCheckRegion

			var result = string.Empty;

			if (response != null)
			{
				result = FormattableString.Invariant($@"Received response details:
Request Uri: {response.RequestMessage?.RequestUri.ToString() ?? "<Empty>"} 
Header: {response.Headers.GetAsString()}
Body: {response.Content.ReadAsStringAsync().Result}
StatusCode: {response.StatusCode}
Reason: {response.ReasonPhrase}
IsSuccessful: {response.IsSuccessStatusCode}");
			}

			return result;

			#endregion
		}

		public static string GetAsString(this HttpRequestMessage request)
		{
			#region SuppressResourceStringsCheckRegion

			if (request != null)
			{
				return FormattableString.Invariant($@"Request Details:
Method: {request.Method.ToString().ToUpper(CultureInfo.InvariantCulture)}
Uri: {request.RequestUri}
Header: {request.Headers?.GetAsString() ?? "<Empty>"}
Body: {request.Content?.ReadAsStringAsync().Result ?? "<Empty>"}");
			}
			return string.Empty;

			#endregion
		}
	}
}