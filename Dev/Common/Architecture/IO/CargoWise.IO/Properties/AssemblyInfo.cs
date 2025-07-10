using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("CargoWise IO Library")]
[assembly: AssemblyDescription("Provides IO Functionality")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

#pragma warning disable RS0030
[assembly: Guid("5b22f19e-df0f-4969-852f-e5e0ed6ca7f3")]
#pragma warning restore RS0030
#if DEBUG
[assembly: InternalsVisibleTo("CargoWise.IO.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
