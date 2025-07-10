using System.Runtime.CompilerServices;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(RefLatLongPostcodeSchema))]

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Data.RefLatLongPostcode.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
