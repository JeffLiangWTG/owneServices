using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.Staff
{
	public class StaffSetting : BaseInterceptorSetting
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
		public override IEnumerable<string> EnableList
		{
			get { yield return "Staff"; }
		}

		public override IEnumerable<string> DisableList
		{
			get { yield break; }
		}
	}
}
