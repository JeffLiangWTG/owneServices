using System.Threading.Tasks;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Licensing.GUI
{
	public abstract class LicenseAcceptanceGuarantorStrategy
	{
		#region API

		public bool NeedsLicense() => NeedsLicenseCore();
		public async Task GetAcceptanceAsync() => await GetAcceptanceCore();

		#endregion

		#region Abstract

		protected abstract bool NeedsLicenseCore();
		protected abstract Task GetAcceptanceCore();

		#endregion

		public ZPanel HostPanel { get; set; }
	}
}
