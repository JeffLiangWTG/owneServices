using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("ES.ExitControl Customs GUI")]
[assembly: AssemblyDescription("ES.ExitControl Customs GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.ES.ExitControl.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.ES.ExitControl.GUI.Test")]
