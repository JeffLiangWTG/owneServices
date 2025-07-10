#if DEBUG
using CargoWise.Types;
using Enterprise.Licensing;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public class ZTestGlobalConfig : ZGlobalConfig
	{
		protected override LicenceCheckpoint GetLicenceCheckPoint(Licences licences)
		{
			return !LicenceCheckPointCodeForTesting.IsEmpty ? licences.GetCheckpointFromCode(LicenceCheckPointCodeForTesting) : null;
		}

		public ZString LicenceCheckPointCodeForTesting;
	}
}
#endif
