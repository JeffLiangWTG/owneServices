using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("EU.ExitControl Customs GUI")]
[assembly: AssemblyDescription("EU.ExitControl Customs GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.EU.ExitControl.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IE.ExitControl.GUI",
	"Enterprise.Customs.IE.ExitControl.GUI.Test",
	"Enterprise.Customs.ES.ExitControl.GUI",
	"Enterprise.Customs.ES.ExitControl.GUI.Test",
	"Enterprise.Customs.DE.ExitControl.GUI",
	"Enterprise.Customs.DE.ExitControl.GUI.Test",
	"Enterprise.Customs.EU.ExitControl.Module",
	"Enterprise.Customs.EU.ExitControl.GUI.Test",
	"Enterprise.Customs.EU.ExitControl.Module.Test",
	"Enterprise.Customs.NL.ExitControl.GUI",
	"Enterprise.Customs.NL.ExitControl.GUI.Test",
	"Enterprise.Customs.PL.ExitControl.GUI",
	"Enterprise.Customs.PL.ExitControl.GUI.Test")]
