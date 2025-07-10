using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(false)]
#if DEBUG
[assembly: InternalsVisibleTo("ZClientMFI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: InternalsVisibleTo("ZClientMFI.ServiceTasks, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientMFI.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
