using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(false)]
#if DEBUG
[assembly: InternalsVisibleTo("CargoWise.Design.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
