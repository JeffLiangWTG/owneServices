using System.Reflection;

[assembly: AssemblyTitle("Error Reporting Service Tasks")]
[assembly: AssemblyDescription("Error Reporting Service Tasks")]
[assembly: AssemblyConfiguration("")]
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Enterprise.ErrorReporting.ServiceTasks.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
