using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ES Customs NCTS Module")]
[assembly: AssemblyDescription("ES Customs NCTS Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.NCTS.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.NCTS.Module.Test")]
