using System.Reflection;

[assembly: AssemblyTitle("EU Customs DataTransfer")]
[assembly: AssemblyDescription("EU Customs DataTransfer")]

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IE.DataTransfer",
	"Enterprise.Customs.IE.DataTransfer.Test",
	"Enterprise.Customs.FR.DataTransfer",
	"Enterprise.Customs.FR.DataTransfer.Test",
	"Enterprise.Customs.ES.DataTransfer",
	"Enterprise.Customs.ES.DataTransfer.Test",
	"Enterprise.Customs.GB.DataTransfer",
	"Enterprise.Customs.GB.DataTransfer.Test",
	"Enterprise.Customs.DE.DataTransfer",
	"Enterprise.Customs.DE.DataTransfer.Test",
	"Enterprise.Customs.EU.DataTransfer.Test",
	"Enterprise.Customs.EU.NCTS.DataTransfer",
	"Enterprise.Customs.IT.DataTransfer",
	"Enterprise.Customs.IT.DataTransfer.Test",
	"Enterprise.Customs.IT.NCTS.DataTransfer")]
