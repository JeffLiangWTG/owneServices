using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("GB Customs Module")]
[assembly: AssemblyDescription("GB Customs Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
