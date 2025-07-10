using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customise this process see: https://aka.ms/assembly-info-properties

#if DEBUG
[assembly: InternalsVisibleTo("ZClientWebEDI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
// The following GUID is for the ID of the typelib if this project is exposed to COM.

#pragma warning disable RS0030
[assembly: Guid("3f44ceeb-ba50-4d1e-ac44-0755c143f9c3")]
#pragma warning restore RS0030
