using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("EU Intrastat GUI")]
[assembly: AssemblyDescription("EU Intrastat GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.EU.Intrastat.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
