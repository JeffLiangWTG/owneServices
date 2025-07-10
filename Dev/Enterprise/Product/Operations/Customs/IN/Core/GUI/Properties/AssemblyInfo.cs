using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("IN Customs GUI")]
[assembly: AssemblyDescription("IN Customs GUI")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IN.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
