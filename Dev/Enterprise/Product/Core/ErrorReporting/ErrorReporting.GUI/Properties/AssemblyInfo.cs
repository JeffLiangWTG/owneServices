using System.Reflection;
[assembly: AssemblyTitle("Error Reporting GUI")]
[assembly: AssemblyDescription("Error Reporting GUI")]
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Enterprise.ErrorReporting.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
