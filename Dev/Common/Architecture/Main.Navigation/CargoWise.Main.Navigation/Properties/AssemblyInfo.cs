using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(false)]

#if DEBUG
[assembly: InternalsVisibleTo("CargoWise.Main.Navigation.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
