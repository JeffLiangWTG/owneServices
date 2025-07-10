using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("AUCustoms GUI")]
[assembly: AssemblyDescription("AUCustoms GUI")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.AU.Declaration.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
