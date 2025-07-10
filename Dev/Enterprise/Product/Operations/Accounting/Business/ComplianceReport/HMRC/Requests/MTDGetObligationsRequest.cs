using System;
using System.Net.Http;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	#region SuppressResourceStringsCheckRegion

	public class MTDGetObligationsRequest : MTDRequestBase
	{
		public MTDGetObligationsRequest(MTDClient client)
			: base(client)
		{ }

		public override string Path => FormattableString.Invariant($"{base.Path}/obligations?{QueryString}");

		protected override string QueryString => FormattableString.Invariant($"{base.QueryString}{DateRangeQueryString}");

		public override HttpMethod Method => HttpMethod.Get;

		public string ObligationStatus { get; set; }

		protected override string ScenarioToSimulate => AccountingConfigurationRegistry.Instance.MTDScenarioToSimulateForObligationRequest.GetFallBackValueAtAllLevels(CompanyPK.ToGuid(), Guid.Empty, Guid.Empty);
	}

	#endregion
}