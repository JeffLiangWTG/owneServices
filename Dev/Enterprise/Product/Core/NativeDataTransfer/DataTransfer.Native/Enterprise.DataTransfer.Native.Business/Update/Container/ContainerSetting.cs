using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.Container
{
	public class ContainerSetting : BaseInterceptorSetting
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
		public override IEnumerable<string> EnableList => new[] { "Container" };

		public override IEnumerable<string> DisableList => System.Array.Empty<string>();
	}
}
