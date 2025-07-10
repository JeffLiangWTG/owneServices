using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ediEnterprise CargoWise Client Overrides")]
[assembly: AssemblyDescription("")]
#if DEBUG
[assembly: InternalsVisibleTo("ZClientEDI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("ZClientEDI.Winzor.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
