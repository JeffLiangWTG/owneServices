using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.NewsAnnouncement
{
	public class NewsAnnouncementSetting : BaseInterceptorSetting
	{
		public override IEnumerable<string> EnableList => new[] { "NewsAnnouncement" };

		public override IEnumerable<string> DisableList => System.Array.Empty<string>();
	}
}
