using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Business.Responses;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.EntityInfos
{
	public class EntityInfoSetting : BaseInterceptorSetting
	{
		public override IEnumerable<string> EnableList
		{
			get { return System.Array.Empty<string>(); }
		}

		public override IEnumerable<string> DisableList
		{
			get { return System.Array.Empty<string>(); }
		}

		public EntityInfo EntityInfo { get; set; }
	}
}