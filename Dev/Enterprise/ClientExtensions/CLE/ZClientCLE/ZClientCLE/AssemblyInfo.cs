using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ZClientCLE.ServiceTasks, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#if DEBUG
[assembly: InternalsVisibleTo("ZClientCLE.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientCLE.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
