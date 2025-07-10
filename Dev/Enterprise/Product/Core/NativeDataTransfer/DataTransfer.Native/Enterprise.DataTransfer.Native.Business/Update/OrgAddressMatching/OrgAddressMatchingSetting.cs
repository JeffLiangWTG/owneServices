using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgAddressMatching
{
	public class OrgAddressMatchingSetting : BaseInterceptorSetting
	{
		public OrgAddressMatchingSetting()
		{
			Enable = true;
		}

		public override IEnumerable<string> EnableList
		{
			get { return System.Array.Empty<string>(); }
		}

		public override IEnumerable<string> DisableList
		{
			get { return System.Array.Empty<string>(); }
		}
	}
}
