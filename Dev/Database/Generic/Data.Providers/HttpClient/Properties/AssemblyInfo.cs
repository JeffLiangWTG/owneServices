using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("CargoWise.Data.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("CargoWise.Data.HttpClient.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("CargoWise.Data.SqlProxy.Server.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
