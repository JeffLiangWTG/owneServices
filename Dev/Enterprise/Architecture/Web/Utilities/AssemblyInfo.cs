using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Web")]
[assembly: AssemblyDescription("Base functionality for Web projects")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.ZArchitecture.Web.Utilities.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
