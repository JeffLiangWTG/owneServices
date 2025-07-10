using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ZClientDHL")]
[assembly: AssemblyDescription("DHL Express specific overrides")]
#if DEBUG
[assembly: InternalsVisibleTo("ZClientDHL.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
