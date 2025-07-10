using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("ZClientTIP.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: InternalsVisibleTo("ZClientTIP.ServiceTask, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientTIP.ServiceTask.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
