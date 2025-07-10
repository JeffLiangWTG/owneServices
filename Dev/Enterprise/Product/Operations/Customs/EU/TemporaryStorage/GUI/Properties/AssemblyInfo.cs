using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("EU.TemporaryStorage Customs GUI")]
[assembly: AssemblyDescription("EU.TemporaryStorage Customs GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.EU.TemporaryStorage.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IE.GUI",
	"Enterprise.Customs.IE.GUI.Test",
	"Enterprise.Customs.FR.GUI",
	"Enterprise.Customs.ES.TemporaryStorage.GUI",
	"Enterprise.Customs.ES.TemporaryStorage.Module",
	"Enterprise.Customs.ES.TemporaryStorage.GUI.Test",
	"Enterprise.Customs.EU.TemporaryStorage.Module",
	"Enterprise.Customs.EU.TemporaryStorage.GUI.Test",
	"Enterprise.Customs.IT.TemporaryStorage.GUI",
	"Enterprise.Customs.IT.TemporaryStorage.GUI.Test")]
