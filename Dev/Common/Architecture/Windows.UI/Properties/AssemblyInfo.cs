using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(false)]
#if DEBUG
[assembly: InternalsVisibleTo("CargoWise.Windows.UI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

