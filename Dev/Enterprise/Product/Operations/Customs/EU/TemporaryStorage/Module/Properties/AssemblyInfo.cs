using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("EU.TemporaryStorage Customs Module")]
[assembly: AssemblyDescription("EU.TemporaryStorage Customs Module")]
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IE.Module",
	"Enterprise.Customs.FR.Module",
	"Enterprise.Customs.ES.TemporaryStorage.Module",
	"Enterprise.Customs.ES.TemporaryStorage.Module.Test",
	"Enterprise.Customs.DE.Module",
	"Enterprise.Customs.EU.TemporaryStorage.Module.Test",
	"Enterprise.Customs.IT.Module",
	"Enterprise.Customs.IT.TemporaryStorage.Module",
	"Enterprise.Customs.PL.Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.EU.TemporaryStorage.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
