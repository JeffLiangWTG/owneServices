using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	class OrgContactSetting : BaseInterceptorSetting
	{
		public OrgContactSetting()
		{
			Enable = true;
		}

		#region Filter

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
		public override IEnumerable<string> EnableList
		{
			get { return new[] { "Organization" }; }
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
