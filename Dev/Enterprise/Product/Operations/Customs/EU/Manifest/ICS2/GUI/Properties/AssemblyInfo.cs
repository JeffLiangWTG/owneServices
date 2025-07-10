using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("EU ICS2 Manifest GUI")]
[assembly: AssemblyDescription("EU ICS2 Manifest GUI")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.EU.Manifest.ICS2.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.EU.Manifest.ICS2.GUI.Test")]
