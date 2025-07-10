using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ES Customs NCTS Business")]
[assembly: AssemblyDescription("ES Customs NCTS Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.NCTS.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.Business.Test",
	"Enterprise.Customs.ES.DocumentWrappers",
	"Enterprise.Customs.ES.DocumentWrappers.Test",
	"Enterprise.Customs.ES.NCTS.Business.Test",
	"Enterprise.Customs.ES.NCTS.GUI",
	"Enterprise.Customs.ES.NCTS.GUI.Test",
	"Enterprise.Customs.ES.NCTS.Module",
	"Enterprise.Customs.ES.NCTS.Module.Test")]
