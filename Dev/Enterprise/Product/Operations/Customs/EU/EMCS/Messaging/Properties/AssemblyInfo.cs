using System.Reflection;

[assembly: AssemblyTitle("EU Customs EMCS Messaging")]
[assembly: AssemblyDescription("EU Customs EMCS Messaging")]

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IE.EMCS.Messaging",
	"Enterprise.Customs.GB.EMCS.Messaging",
	"Enterprise.Customs.EU.EMCS.Messaging.Test")]
