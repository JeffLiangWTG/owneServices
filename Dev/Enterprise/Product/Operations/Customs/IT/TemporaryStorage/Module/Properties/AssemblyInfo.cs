using System.Reflection;

[assembly: AssemblyTitle("IT Customs TemporaryStorage Module")]
[assembly: AssemblyDescription("IT Customs TemporaryStorage Module")]
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Enterprise.Customs.IT.TemporaryStorage.Module.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.IT.TemporaryStorage.Module.Test")]
