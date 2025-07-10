using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("IT Customs NCTS GUI")]
[assembly: AssemblyDescription("IT Customs NCTS GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IT.NCTS.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IT.NCTS.GUI.Test",
	"Enterprise.Customs.IT.NCTS.Module",
	"Enterprise.Customs.IT.NCTS.Module.Test")]
