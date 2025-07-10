using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("CA Customs Module")]
[assembly: AssemblyDescription("CA Customs Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.CA.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
