using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("IT Customs TemporaryStorage Business")]
[assembly: AssemblyDescription("IT Customs TemporaryStorage Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IT.TemporaryStorage.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IT.TemporaryStorage.GUI",
	"Enterprise.Customs.IT.TemporaryStorage.Module",
	"Enterprise.Customs.IT.TemporaryStorage.Business.Test")]
