using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("EU Customs Module")]
[assembly: AssemblyDescription("EU Customs Module")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.EU.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.TR.Module",
	"Enterprise.Customs.DK.Module",
	"Enterprise.Customs.IE.Module",
	"Enterprise.Customs.IE.Module.Test",
	"Enterprise.Customs.FR.Module",
	"Enterprise.Customs.ES.Module",
	"Enterprise.Customs.ES.Module.Test",
	"Enterprise.Customs._EUCustomsTemplate_.Module",
	"Enterprise.Customs.BE.Module",
	"Enterprise.Customs.GB.Module",
	"Enterprise.Customs.GB.Module.Test",
	"Enterprise.Customs.DE.Module",
	"Enterprise.Customs.DE.Module.Test",
	"Enterprise.Customs.EU.Module.Test",
	"Enterprise.Customs.IT.Module",
	"Enterprise.Customs.IT.Module.Test",
	"Enterprise.Customs.PL.Module",
	"Enterprise.Customs.PL.Module.Test",
	"Enterprise.Customs.SE.Module",
	"Enterprise.Customs.NL.Module",
	"Enterprise.Customs.FI.Module")]
