using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgOpportunity
{
	public class OrgOpportunitySetting : BaseInterceptorSetting
	{
		public OrgOpportunitySetting()
		{
			Enable = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
		public override IEnumerable<string> EnableList => new[] { "Organization", "Opportunity" };

		public override IEnumerable<string> DisableList => System.Array.Empty<string>();
	}
}
