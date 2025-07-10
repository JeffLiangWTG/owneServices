using System.Runtime.CompilerServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ProductionRuleSetSchema))]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.ProductionRuleSet.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
