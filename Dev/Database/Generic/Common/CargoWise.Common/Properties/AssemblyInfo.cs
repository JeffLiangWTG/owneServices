using System;
using System.Reflection;

[assembly: AssemblyTitle("CargoWise.Common")]
[assembly: AssemblyDescription("Common utilities")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(true)]
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("CargoWise.Common.Testing, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
#if NET6_0_OR_GREATER
[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
