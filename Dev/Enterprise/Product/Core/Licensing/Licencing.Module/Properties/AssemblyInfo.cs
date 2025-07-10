using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("CargoWise Licencing Module")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCulture("")]

#pragma warning disable RS0030
[assembly: Guid("63e5e43a-e4e8-4b46-9238-64b7c7921868")]
#pragma warning restore RS0030
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Licensing.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
