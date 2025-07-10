using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgSalesCall
{
	public class OrgSalesCallSetting : BaseInterceptorSetting
	{
		public OrgSalesCallSetting()
		{
			Enable = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
		public override IEnumerable<string> EnableList => new[] { "Organization", "Communication" };

		public override IEnumerable<string> DisableList => System.Array.Empty<string>();
	}
}
