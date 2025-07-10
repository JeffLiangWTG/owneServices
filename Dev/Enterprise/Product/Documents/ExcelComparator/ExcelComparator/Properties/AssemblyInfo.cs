using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ExcelComparator")]
[assembly: AssemblyDescription("Allows you to compare the text contents of 2 Excel xls files using a 3rd party tool like KDiff or Araxis Merge.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

#pragma warning disable RS0030
[assembly: Guid("dad62fd1-3f94-4445-b41d-e9c3eccb79fc")]
#pragma warning restore RS0030
#if DEBUG
[assembly: InternalsVisibleTo("ExcelComparator.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

