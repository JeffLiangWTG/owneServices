using System.Reflection;

[assembly: AssemblyTitle("EU Manifest ICS2 Business")]
[assembly: AssemblyDescription("EU Manifest ICS2 Business")]
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Enterprise.Customs.EU.Manifest.ICS2.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.eTail.DataTransfer",
	"Enterprise.eTail.DataTransfer.Testing",
	"Enterprise.eTail.GUI.Testing",
	"Enterprise.Customs.EU.Manifest.ICS2.Business.Test",
	"Enterprise.Customs.EU.Manifest.ICS2.GUI",
	"Enterprise.Customs.EU.Manifest.ICS2.GUI.Test",
	"Enterprise.Customs.EU.Manifest.ICS2.ServiceTasks",
	"Enterprise.Customs.EU.Manifest.ICS2.ServiceTasks.Test")]
