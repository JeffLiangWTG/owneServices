using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Error Reporting Module")]
[assembly: AssemblyDescription("Error Reporting Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.ErrorReporting.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif