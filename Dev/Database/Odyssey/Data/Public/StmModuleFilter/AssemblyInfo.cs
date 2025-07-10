using System.Runtime.CompilerServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(StmModuleFilterSchema))]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.StmModuleFilter.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
