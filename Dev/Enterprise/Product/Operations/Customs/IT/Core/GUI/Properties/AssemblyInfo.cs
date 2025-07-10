using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("IT Customs GUI")]
[assembly: AssemblyDescription("IT Customs GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IT.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IT.Module",
	"Enterprise.Customs.IT.GUI.Test",
	"Enterprise.Customs.IT.NCTS.GUI",
	"Enterprise.Customs.IT.NCTS.Module",
	"Enterprise.Customs.IT.NCTS.GUI.Test",
	"Enterprise.Customs.IT.TemporaryStorage.GUI",
	"Enterprise.Customs.IT.TemporaryStorage.GUI.Test")]
