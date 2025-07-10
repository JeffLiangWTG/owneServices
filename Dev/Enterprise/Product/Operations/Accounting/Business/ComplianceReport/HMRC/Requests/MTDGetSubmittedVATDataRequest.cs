using System;
using System.Net.Http;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	#region SuppressResourceStringsCheckRegion

	public class MTDGetSubmittedVATDataRequest : MTDRequestBase
	{
		public MTDGetSubmittedVATDataRequest(MTDClient client, ZString periodKey)
			: base(client)
		{
			PeriodKey = periodKey;
		}

		public override string Path => FormattableString.Invariant($"{base.Path}/returns/{PeriodKey}");

		string PeriodKey { get; set; }

		public override HttpMethod Method => HttpMethod.Get;
	}

	#endregion
}
