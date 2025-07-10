using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ES Customs TemporaryStorage GUI")]
[assembly: AssemblyDescription("ES Customs TemporaryStorage GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.TemporaryStorage.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.NCTS.GUI",
	"Enterprise.Customs.ES.NCTS.GUI.Test",
	"Enterprise.Customs.ES.TemporaryStorage.GUI.Test",
	"Enterprise.Customs.ES.TemporaryStorage.Module",
	"Enterprise.Customs.ES.TemporaryStorage.Module.Test")]
