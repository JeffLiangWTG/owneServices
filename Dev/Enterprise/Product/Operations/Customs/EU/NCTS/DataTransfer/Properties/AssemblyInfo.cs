using System.Reflection;

[assembly: AssemblyTitle("EU NCTS DataTransfer")]
[assembly: AssemblyDescription("EU NCTS DataTransfer")]

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.TR.NCTS.GUI",
	"Enterprise.Customs.IE.NCTS.GUI",
	"Enterprise.Customs.FR.DataTransfer",
	"Enterprise.Customs.FR.GUI",
	"Enterprise.Customs.FR.DataTransfer.Test",
	"Enterprise.Customs.FR.GUI.Test",
	"Enterprise.Customs.ES.NCTS.GUI",
	"Enterprise.Customs.ES.NCTS.GUI.Test",
	"Enterprise.Customs.GB.Business.Test",
	"Enterprise.Customs.GB.DataTransfer",
	"Enterprise.Customs.GB.DataTransfer.Test",
	"Enterprise.Customs.EU.NCTS.GUI",
	"Enterprise.Customs.EU.NCTS.DataTransfer.Test",
	"Enterprise.Customs.EU.NCTS.GUI.Test",
	"Enterprise.Customs.IT.NCTS.GUI",
	"Enterprise.Customs.IT.NCTS.DataTransfer",
	"Enterprise.Customs.IT.NCTS.GUI.Test",
	"Enterprise.Customs.IT.NCTS.DataTransfer.Test",
	"Enterprise.Customs.NO.NCTS.GUI")]
