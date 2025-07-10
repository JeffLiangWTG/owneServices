using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("CH Customs Module")]
[assembly: AssemblyDescription("CH Customs Module Project")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.CH.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
