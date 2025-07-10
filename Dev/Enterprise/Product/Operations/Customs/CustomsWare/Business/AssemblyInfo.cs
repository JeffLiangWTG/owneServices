using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Enterprise.Customs.CustomsWare.Business")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

// The following GUID is for the ID of the typelib if this project is exposed to COM
#pragma warning disable RS0030
[assembly: Guid("311e8aaf-d800-4c74-8ff4-70e2ecc3067c")]
#pragma warning restore RS0030

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.GUI",
	"Enterprise.Customs.GUI.Test",
	"Enterprise.Customs.Module",
	"Enterprise.Customs.Module.Test",
	"Enterprise.Customs.CustomsWare.Business.Test",
	"Enterprise.Customs.CustomsWare.Business.XmlSerializers",
	"Enterprise.Customs.CustomsWare.ServiceTasks")]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.Customs.CustomsWare.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
