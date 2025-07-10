using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.ServiceManager.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.ServiceManager.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("CargoWise.ServiceManager.Host.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ServiceManager.Host.CW.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
