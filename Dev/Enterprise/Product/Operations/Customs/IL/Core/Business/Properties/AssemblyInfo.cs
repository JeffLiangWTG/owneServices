using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("IL Customs Business")]
[assembly: AssemblyDescription("IL Customs Business")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IL.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
