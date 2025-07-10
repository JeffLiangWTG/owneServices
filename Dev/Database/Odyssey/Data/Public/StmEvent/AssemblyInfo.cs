using System.Runtime.CompilerServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(StmEventSchema))]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.StmEvent.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
