using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Enterprise.Customs.CustomsWare.Services")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

// The following GUID is for the ID of the typelib if this project is exposed to COM
#pragma warning disable RS0030
[assembly: Guid("d51f0ee6-135d-418a-950e-d650d2b07594")]
#pragma warning restore RS0030

[assembly: CargoWise.Common.PreventAssemblyReferences(
	"Enterprise.Customs.CustomsWare.Business",
	"Enterprise.Customs.CustomsWare.Services.Test")]
