using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DocumentEngine.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.DocumentEngine.Module.ServiceTasks, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
