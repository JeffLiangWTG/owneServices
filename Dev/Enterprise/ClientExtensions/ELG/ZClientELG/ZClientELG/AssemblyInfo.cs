using System.Reflection;
using System.Runtime.CompilerServices;
[assembly: AssemblyTitle("Enterprise.Client.ELG")]
[assembly: AssemblyDescription("Enterprise.Client.ELG")]

#if DEBUG
[assembly: InternalsVisibleTo("ZClientELG.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: InternalsVisibleTo("ZClientELG.ServiceTasks, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientELG.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
