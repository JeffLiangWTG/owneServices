using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("EU Customs NCTS GUI")]
[assembly: AssemblyDescription("EU Customs NCTS GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.EU.NCTS.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.TR.NCTS.GUI",
	"Enterprise.Customs.TR.NCTS.GUI.Test",
	"Enterprise.Customs.TR.NCTS.Module.Test",
	"Enterprise.Customs.IE.NCTS.GUI",
	"Enterprise.Customs.IE.NCTS.GUI.Test",
	"Enterprise.Customs.FR.GUI",
	"Enterprise.Customs.FR.GUI.Test",
	"Enterprise.Customs.FR.Module.Test",
	"Enterprise.Customs.ES.NCTS.GUI",
	"Enterprise.Customs.ES.NCTS.GUI.Test",
	"Enterprise.Customs.BE.NCTS.GUI",
	"Enterprise.Customs.BE.NCTS.GUI.Test",
	"Enterprise.Customs.GB.GUI",
	"Enterprise.Customs.GB.GUI.Test",
	"Enterprise.Customs.DE.NCTS.GUI",
	"Enterprise.Customs.DE.NCTS.GUI.Test",
	"Enterprise.Customs.DE.NCTS.Module.Test",
	"Enterprise.Customs.EU.NCTS.Module",
	"Enterprise.Customs.EU.NCTS.Module.Test",
	"Enterprise.Customs.EU.NCTS.GUI.Test",
	"Enterprise.Customs.IT.NCTS.GUI",
	"Enterprise.Customs.IT.NCTS.GUI.Test",
	"Enterprise.Customs.IT.NCTS.Module.Test",
	"Enterprise.Customs.PL.NCTS.GUI",
	"Enterprise.Customs.PL.NCTS.GUI.Test",
	"Enterprise.Customs.PL.NCTS.Module.Test",
	"Enterprise.Customs.CH.NCTS.GUI",
	"Enterprise.Customs.CH.NCTS.GUI.Test",
	"Enterprise.Customs.NO.NCTS.GUI",
	"Enterprise.Customs.NO.NCTS.GUI.Test",
	"Enterprise.Customs.NL.NCTS.GUI",
	"Enterprise.Customs.NL.NCTS.GUI.Test",
	"Enterprise.Customs.SE.NCTS.GUI",
	"Enterprise.Customs.SE.NCTS.GUI.Test")]
