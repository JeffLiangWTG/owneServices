using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgMatchings
{
	public class OrgMatchingSetting : BaseInterceptorSetting
	{
		public OrgMatchingSetting()
		{
			Enable = true;
		}

		#region Filter

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
		public override IEnumerable<string> EnableList
		{
			get { return new[] { "Order" }; }
		}

		public override IEnumerable<string> DisableList
		{
			get
			{
				return System.Array.Empty<string>();
			}
		}

		#endregion
	}
}
