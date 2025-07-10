using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(false)]
#if DEBUG
[assembly: InternalsVisibleTo("ZClientKNA.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
