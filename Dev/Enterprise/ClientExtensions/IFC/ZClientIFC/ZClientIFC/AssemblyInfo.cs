using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("ZClientIFC.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: InternalsVisibleTo("ZClientIFC.ServiceTasks, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientIFC.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]

