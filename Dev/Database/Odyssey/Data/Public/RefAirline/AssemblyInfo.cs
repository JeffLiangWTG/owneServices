using System.Runtime.CompilerServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(RefAirlineSchema))]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.RefAirline.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
