using System;
using System.Net.Http;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	#region SuppressResourceStringsCheckRegion

	[CodeAlive("Will be used for calling Payments API of MTD")]
	public class MTDGetPaymentsRequest : MTDRequestBase
	{
		public MTDGetPaymentsRequest(MTDClient client)
			: base(client)
		{
		}

		public override string Path => FormattableString.Invariant($"{base.Path}/payments?{QueryString}");

		protected override string QueryString => FormattableString.Invariant($"{base.QueryString}{DateRangeQueryString}");

		public override HttpMethod Method => HttpMethod.Get;
	}

	#endregion
}