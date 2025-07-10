using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ES Customs NCTS Messaging")]
[assembly: AssemblyDescription("ES Customs NCTS Messaging")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.NCTS.Messaging.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.NCTS.Business",
	"Enterprise.Customs.ES.NCTS.GUI.Test",
	"Enterprise.Customs.ES.NCTS.Messaging.Test",
	"Enterprise.Customs.ES.NCTS.Module.Test")]
