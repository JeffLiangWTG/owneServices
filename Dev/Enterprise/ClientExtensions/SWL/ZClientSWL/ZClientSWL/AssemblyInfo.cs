using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("ZClientSWL.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientSWL.ServiceTasks, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientSWL.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
