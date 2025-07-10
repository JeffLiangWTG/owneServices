using System.Runtime.CompilerServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(StmSystemDefinedFieldSchema))]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.StmSystemDefinedField.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
