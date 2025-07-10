using System.Reflection;
using System.Runtime.CompilerServices;
using WTG.StaticAnalysis.Annotation;

[assembly: AssemblyTitle("ediEnterprise Core Module")]
[assembly: AssemblyDescription("Core functionality for ediEnterprise")]
[assembly: AssemblyConfiguration("")]
[assembly: InternalsVisibleTo("Enterprise.Environment, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Licensing.Core, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Security, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.ZArchitecture.GUI, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#if DEBUG
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
[assembly: InternalsVisibleTo("CargoWise.Main.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.ZArchitecture.Core.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.ZArchitecture.GUI.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Licensing.Core.Test, PublicKey=" + CommonAssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Enterprise.Security.Testing, PublicKey=" + CommonAssemblyInfo.PublicKey)]
#endif

[assembly: UsesConstants(typeof(WTG.WiseTechAcademy.WiseTechAcademyApiClient))]
