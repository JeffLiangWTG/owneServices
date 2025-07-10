using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Enterprise.Client.SWT.ServiceTasks")]
[assembly: AssemblyDescription("Enterprise.Client.SWT.ServiceTasks")]

#if DEBUG
[assembly: InternalsVisibleTo("ZClientSWT.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: InternalsVisibleTo("ZClientSWT.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientSWT.ServiceTasks, PublicKey=" + CommonAssemblyInfo.PublicKey)]
