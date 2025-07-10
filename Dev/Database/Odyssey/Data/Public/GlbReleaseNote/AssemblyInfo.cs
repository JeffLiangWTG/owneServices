using System.Runtime.CompilerServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(GlbReleaseNoteSchema))]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.GlbReleaseNote.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
