using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ES Customs TemporaryStorage Business")]
[assembly: AssemblyDescription("ES Customs TemporaryStorage Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.TemporaryStorage.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.TemporaryStorage.Business.Test",
	"Enterprise.Customs.ES.TemporaryStorage.GUI",
	"Enterprise.Customs.ES.TemporaryStorage.GUI.Test",
	"Enterprise.Customs.ES.TemporaryStorage.Module",
	"Enterprise.Customs.ES.TemporaryStorage.Module.Test")]
