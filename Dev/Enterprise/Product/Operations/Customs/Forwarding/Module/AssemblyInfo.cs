using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Customs Forwarding Module")]
[assembly: AssemblyDescription("Customs Forwarding Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.Forwarding.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
