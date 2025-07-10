using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Enterprise.Client.WFN")]
[assembly: AssemblyDescription("Enterprise.Client.WFN")]

#if DEBUG
[assembly: InternalsVisibleTo("ZClientWFN.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: InternalsVisibleTo("ZClientWFN.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientWFN.ServiceTasks, PublicKey=" + CommonAssemblyInfo.PublicKey)]
