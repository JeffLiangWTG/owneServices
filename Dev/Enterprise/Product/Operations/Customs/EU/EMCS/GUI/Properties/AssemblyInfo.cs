using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("EU Customs EMCS GUI")]
[assembly: AssemblyDescription("EU Customs EMCS GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.EU.EMCS.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IE.EMCS.GUI",
	"Enterprise.Customs.IE.EMCS.GUI.Test",
	"Enterprise.Customs.GB.EMCS.GUI",
	"Enterprise.Customs.GB.EMCS.GUI.Test",
	"Enterprise.Customs.DE.EMCS.GUI",
	"Enterprise.Customs.DE.EMCS.GUI.Test",
	"Enterprise.Customs.EU.EMCS.Module",
	"Enterprise.Customs.EU.EMCS.GUI.Test")]
