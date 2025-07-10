using System.Reflection;

[assembly: AssemblyTitle("EU.ExitControl Customs Module")]
[assembly: AssemblyDescription("EU.ExitControl Customs Module")]

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IE.ExitControl.Module",
	"Enterprise.Customs.IE.ExitControl.Module.Test",
	"Enterprise.Customs.ES.ExitControl.Module",
	"Enterprise.Customs.ES.ExitControl.Module.Test",
	"Enterprise.Customs.DE.ExitControl.Module",
	"Enterprise.Customs.EU.ExitControl.Module.Test")]
