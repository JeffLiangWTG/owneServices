using System;

namespace Enterprise.Startup.Tools.Testing
{
	sealed class DownloadFormForTest : DownloadForm
	{
		public DownloadFormForTest(Uri sourceUrl) : base(sourceUrl)
		{
		}

		protected override void OnShown(EventArgs e)
		{
			// We override it so that it doesn't start downloading anything in tests
		}
	}
}
