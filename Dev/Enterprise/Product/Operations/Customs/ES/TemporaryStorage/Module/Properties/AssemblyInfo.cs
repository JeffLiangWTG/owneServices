using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ES Customs TemporaryStorage Module")]
[assembly: AssemblyDescription("ES Customs TemporaryStorage Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.TemporaryStorage.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.TemporaryStorage.Module.Test")]
