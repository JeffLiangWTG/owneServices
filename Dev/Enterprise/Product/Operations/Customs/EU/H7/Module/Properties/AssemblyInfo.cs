using System.Reflection;

[assembly: AssemblyTitle("EU.H7 Customs Module")]
[assembly: AssemblyDescription("EU.H7 Customs Module")]

[assembly: CargoWise.Common.PreventAssemblyReferences([
	"Enterprise.Customs.EU.H7.Module.Test",
	"Enterprise.Customs.GB.H7.Module",
	"Enterprise.Customs.GB.H7.Module.Test",
	"Enterprise.Customs.ES.Manifest.H7.Module",
	"Enterprise.Customs.ES.Manifest.H7.Module.Test",
])]
