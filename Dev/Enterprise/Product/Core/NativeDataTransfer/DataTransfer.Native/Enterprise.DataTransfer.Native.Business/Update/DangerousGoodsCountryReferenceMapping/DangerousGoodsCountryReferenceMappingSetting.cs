using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class DangerousGoodsCountryReferenceMappingSetting : BaseInterceptorSetting
	{
		public override IEnumerable<string> EnableList => new[] { "DangerousGoodsCountryReferenceMapping" };

		public override IEnumerable<string> DisableList => System.Array.Empty<string>();
	}
}
