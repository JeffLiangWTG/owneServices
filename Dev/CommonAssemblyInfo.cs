using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("WiseTech Global")]
[assembly: AssemblyProduct("CargoWise")]
[assembly: AssemblyCopyright("Copyright WiseTech Global")]
[assembly: AssemblyTrademark("CargoWise")]
#if !NOASSEMBLYINFOVERSION
[assembly: AssemblyInformationalVersion("2019-05-23 05:00:14")]
#endif

#if NET5_0_OR_GREATER
[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

#if !SGEN // SGen already generates this for us
#if WINZOR
[assembly: AssemblyVersion("3.0.0.0")]
#else
[assembly: AssemblyVersion("2.0.0.0")]
#endif
#endif
[assembly: AssemblyFileVersion("20.8.20.0")]
#if !PocketPC && !WindowsCE
[assembly: StringFreezing] // when combined with NGen, this saves time and memory creating strings at runtime.
#endif

[assembly: ComVisible(false)]
[assembly: NeutralResourcesLanguage("en-US")]

internal static class CommonAssemblyInfo
{
	public const string PublicKey =
		"0024000004800000940000000602000000240000525341310004000001000100dd81107145159ef74a16159de9575e9d49a7038ce8abf4d12445c50b6ac89c71034e02e783d04ee4820779121ec441843d12f4103385a118911b98b33f3e32c2393f761299b87b38cf2825d8cd295c4d567e6dc46e4295c3850de6c39b21e8f9db896639a0a0b360e5f858112a5f15aad857633842e7bc692c6e5fa45d0e13ba";

	public const string PublicKeyToken = "4f570df270576350";
	public const string AssemblyVersion = "2.0.0.0";

	// #NetCoreDirectories
	public const string CWNetCoreSubfolder = "net8.0";
}
