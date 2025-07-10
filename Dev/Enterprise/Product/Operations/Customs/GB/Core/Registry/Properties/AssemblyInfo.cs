using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Enterprise.Customs.GB.Registry")]
[assembly: AssemblyDescription("Container for all GB registry items. No longer in Registry.Business.")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.Registry.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.Dover.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)] // Pentant
[assembly: InternalsVisibleTo("Enterprise.Customs.GB.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
