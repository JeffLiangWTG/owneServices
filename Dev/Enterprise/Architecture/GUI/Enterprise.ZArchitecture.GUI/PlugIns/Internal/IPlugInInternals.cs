using CargoWise.EntityFramework;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.PlugIn.Internal
{
	public interface IPlugInInternals
	{
		void InitializePlugin(ZTabControl topLevelTabControl, ZForm form);
		LicenceCheckpoint LicenceCheckPoint { get; }
#if DEBUG
		ZLabel CoveringLabel { get; }
#endif

		IBusiness HostBusinessEntity { get; }
	}
}
