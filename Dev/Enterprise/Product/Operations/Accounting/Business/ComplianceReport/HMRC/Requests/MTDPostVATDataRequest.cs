using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CargoWise.Common.JSON.Extensions;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	#region SuppressResourceStringsCheckRegion

	public class MTDPostVATDataRequest : MTDRequestBase
	{
		public MTDPostVATDataRequest(MTDClient client, MTDVATData vatData)
			: base(client)
		{
			VATData = vatData;
		}

		MTDVATData VATData { get; }

		public override string Path => FormattableString.Invariant($"{base.Path}/returns/");

		public override HttpMethod Method => HttpMethod.Post;

		protected override void BuildRequestHeader(HttpRequestHeaders requestHeaders)
		{
			requestHeaders.Accept.Clear();

			base.BuildRequestHeader(requestHeaders);
		}

		protected override ByteArrayContent GetRequestBody()
		{
			var jsonVATData = VATData.ToJSON();
			var content = new ByteArrayContent(Encoding.ASCII.GetBytes(jsonVATData));
			content.Headers.Add("Content-Type", "application/json");
			return content;
		}
	}

	#endregion
}
