using System;

namespace CargoWise.Loader.Common
{
	interface ISplash : IDisposable
	{
		int ProgressValue { get; }
		string Status { get; }
		public void Start();
		public void UpdateStatus(string status, int progressValue);
		public void UpdateBranding();
	}
}
