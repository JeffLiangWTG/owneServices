using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("IL Customs GUI")]
[assembly: AssemblyDescription("IL Customs GUI")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IL.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
