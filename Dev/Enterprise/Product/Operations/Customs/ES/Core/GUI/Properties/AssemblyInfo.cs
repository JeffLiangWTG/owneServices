using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ES Customs GUI")]
[assembly: AssemblyDescription("ES Customs GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.ExitControl.GUI",
	"Enterprise.Customs.ES.ExitControl.GUI.Test",
	"Enterprise.Customs.ES.GUI.Test",
	"Enterprise.Customs.ES.Module",
	"Enterprise.Customs.ES.Module.Test",
	"Enterprise.Customs.ES.NCTS.GUI",
	"Enterprise.Customs.ES.NCTS.GUI.Test",
	"Enterprise.Customs.ES.NCTS.Module",
	"Enterprise.Customs.ES.TemporaryStorage.GUI",
	"Enterprise.Customs.ES.TemporaryStorage.GUI.Test")]
