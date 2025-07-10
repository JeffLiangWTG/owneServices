using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(false)]
#if DEBUG
[assembly: InternalsVisibleTo("ZClientNZP.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
