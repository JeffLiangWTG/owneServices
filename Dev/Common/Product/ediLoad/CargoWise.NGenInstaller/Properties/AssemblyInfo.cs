#if DEBUG

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CargoWise.NGenInstaller.Test, PublicKey = " + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Client.Common.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
