using System;
using System.Net.Http;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	#region SuppressResourceStringsCheckRegion

	class MTDGetLiabilitiesRequest : MTDRequestBase
	{
		public MTDGetLiabilitiesRequest(MTDClient client)
			: base(client)
		{
		}

		public override string Path => FormattableString.Invariant($"{base.Path}/liabilities?{QueryString}");

		public override HttpMethod Method => HttpMethod.Get;

		protected override string QueryString => FormattableString.Invariant($"{base.QueryString}{DateRangeQueryString}");
	}

	#endregion
}