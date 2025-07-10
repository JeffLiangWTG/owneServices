using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	internal class MTDHttpResponse
	{
		internal MTDHttpResponse(HttpStatusCode statusCode, bool isSuccessful, HttpResponseHeaders header, string content)
		{
			StatusCode = statusCode;
			Content = content;
			IsSuccessStatusCode = isSuccessful;

			headerDictionary = header.ToDictionary((kp) => kp.Key.ToUpper(CultureInfo.InvariantCulture), (kp) => kp.Value);
		}
		readonly Dictionary<string, IEnumerable<string>> headerDictionary;

		internal string GetReceiptNumber() => headerDictionary.TryGetValue(ReceiptID, out var receiptNumber) ? receiptNumber.FirstOrDefault() : string.Empty;

		internal HttpStatusCode StatusCode { get; }

		internal bool IsSuccessStatusCode { get; }

		internal bool IsUnauthorizedStatusCode(bool isInvalidAccessTokenErrorCode) => StatusCode == HttpStatusCode.Unauthorized
			|| (StatusCode == HttpStatusCode.Forbidden && isInvalidAccessTokenErrorCode);

		internal string Content { get; }

		const string ReceiptID = "RECEIPT-ID";
	}
}
