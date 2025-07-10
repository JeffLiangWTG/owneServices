using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("CargoWise.Data.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Startup.Testing, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Dat.Adapter.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
