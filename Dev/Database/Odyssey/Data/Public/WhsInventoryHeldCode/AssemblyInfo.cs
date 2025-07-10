using System.Runtime.CompilerServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(WhsInventoryHeldCodeSchema))]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.WhsInventoryHeldCode.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
