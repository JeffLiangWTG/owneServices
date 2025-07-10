using System.Reflection;

[assembly: AssemblyTitle("EU Manifest GUI")]
[assembly: AssemblyDescription("EU Manifest GUI")]
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.GB.ICS.GUI",
	"Enterprise.Customs.GB.ICS.GUI.Test",
	"Enterprise.Customs.EU.Manifest.GUI.Test")]
