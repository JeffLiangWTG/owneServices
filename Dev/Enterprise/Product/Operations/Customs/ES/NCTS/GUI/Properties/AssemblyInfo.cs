using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ES Customs NCTS GUI")]
[assembly: AssemblyDescription("ES Customs NCTS GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.NCTS.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.NCTS.GUI.Test",
	"Enterprise.Customs.ES.NCTS.Module",
	"Enterprise.Customs.ES.NCTS.Module.Test")]
