using System.Reflection;
using System.Runtime.InteropServices;
using CargoWise.Common;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("BufferManagement.IntegrationTests")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
#pragma warning disable RS0030
[assembly: Guid("9f549575-1451-4e5b-975d-b676cec4e96b")]
#pragma warning restore RS0030

// This is a top-level assembly for testing how PAVE interacts with the rest of the product. Nothing should be referencing this assembly.
[assembly: PreventAssemblyReferences]
