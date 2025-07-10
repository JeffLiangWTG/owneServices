namespace Enterprise.ZArchitecture.Design
{
	using System;
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	[
		ComImport,
		Guid("6D5140C1-7436-11CE-8034-00AA006009FA"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
	]
	public interface IOleServiceProvider {
		[PreserveSig]
		int QueryService([In]ref Guid guidService,
						 [In]ref Guid riid,
							 out IntPtr ppvObject);
	}
}
