using System.Reflection;

[assembly: AssemblyTitle("Error Reporting Business")]
[assembly: AssemblyDescription("Error Reporting Business")]
[assembly: AssemblyConfiguration("")]
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Enterprise.ErrorReporting.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
