using Enterprise.Environment;

namespace Enterprise.ZArchitecture.Web.Utilities.Environment.Testing
{
	public class WebServicesEnvironmentProviderForTest : WebServicesEnvironmentProvider
	{
		public override BaseEnvironment Instance { get; } = new WebServicesEnvironment();

		protected override void Dispose(bool isDisposing) { if (isDisposing) { Instance.Dispose(); } }
	}
}
