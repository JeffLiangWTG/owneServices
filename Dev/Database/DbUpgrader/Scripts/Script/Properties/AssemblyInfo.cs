using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("Enterprise.DbUpgrader.Transformation.DataModification, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Build.Database.Script.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif
