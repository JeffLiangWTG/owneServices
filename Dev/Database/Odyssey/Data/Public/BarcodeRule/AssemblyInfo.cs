using System.Runtime.CompilerServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(BarcodeRuleSchema))]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.BarcodeRule.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
