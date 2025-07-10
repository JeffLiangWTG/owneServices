using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Enterprise.BufferManagement.Business")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

// The following GUID is for the ID of the typelib if this project is exposed to COM
#pragma warning disable RS0030
[assembly: Guid("3d2e1130-fcc2-4217-9b19-3072d5394a64")]
#pragma warning restore RS0030
[assembly: CargoWise.Common.PreventAssemblyReferences(
	"BufferManagement",
	"PAVE",
	"Enterprise.MasterFiles.Business.Test",
	"Enterprise.VisualBoards.Business.Test",
	"Enterprise.VisualBoards.GUI.Test",
	"Enterprise.BehaviourManagement.Business.Test",
	"Enterprise.TimeEngineScheduler.ServiceTask.Test",
	"Enterprise.DataTransfer.Native.ConcreteUnitTesting",
	"Enterprise.LogWalker.Test",
	"ZClientEDI",
	"ZClientEDI.Test",
	"ZClientEDI.Business.Test",
	"ZClientEDI.GUI.Test")]
#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.BufferManagement.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.BufferManagement.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.BufferManagement.NetworkVisualisation.Business.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
