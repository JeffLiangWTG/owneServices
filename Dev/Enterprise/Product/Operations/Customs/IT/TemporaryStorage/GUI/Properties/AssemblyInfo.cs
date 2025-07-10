using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("IT Customs TemporaryStorage GUI")]
[assembly: AssemblyDescription("IT Customs TemporaryStorage GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.IT.TemporaryStorage.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IT.TemporaryStorage.Module",
	"Enterprise.Customs.IT.TemporaryStorage.GUI.Test")]
