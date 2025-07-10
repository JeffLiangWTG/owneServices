using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("AsycudaCustoms Customs Business")]
[assembly: AssemblyDescription("AsycudaCustoms Customs Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.AsycudaCustoms.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Customs.AsycudaCustoms.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
