using System.Runtime.CompilerServices;

#if DEBUG
[assembly:InternalsVisibleTo("Enterprise.RemoteDesktopServices.Testing, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly:InternalsVisibleTo("CargoWise.Main, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
