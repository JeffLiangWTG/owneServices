using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ES.ExitControl Customs Business")]
[assembly: AssemblyDescription("ES.ExitControl Customs Business")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.ExitControl.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.ExitControl.Business.Test",
	"Enterprise.Customs.ES.ExitControl.GUI",
	"Enterprise.Customs.ES.ExitControl.GUI.Test",
	"Enterprise.Customs.ES.ExitControl.Module",
	"Enterprise.Customs.ES.ExitControl.Module.Test")]
