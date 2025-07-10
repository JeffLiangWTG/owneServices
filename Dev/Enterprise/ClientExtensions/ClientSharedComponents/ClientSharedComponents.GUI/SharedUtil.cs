using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ClientSharedComponents
{
	public static class SharedUtil
	{
		public static string GetFinalPath(string path)
		{
			return ZOpenFileDialog.IsRemote ? (new MappedClientPath().GetMappedPath(path) ?? path) : path;
		}

#if WINZOR
		class MappedClientPath
		{
			public MappedClientPath() => throw new System.NotImplementedException();

			public string GetMappedPath(string arg) => throw new System.NotImplementedException();
		}
#endif
	}
}
