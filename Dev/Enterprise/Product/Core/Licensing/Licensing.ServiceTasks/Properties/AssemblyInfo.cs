using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Licensing Tasks")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

#pragma warning disable RS0030
[assembly: Guid("81601afa-9387-420b-a098-58133e6dce78")]
#pragma warning restore RS0030
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Enterprise.Licensing.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
