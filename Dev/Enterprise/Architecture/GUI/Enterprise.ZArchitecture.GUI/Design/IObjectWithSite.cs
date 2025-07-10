namespace Enterprise.ZArchitecture.Design
{
	using System;
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	[
		ComImport,
		Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)
	]
	internal interface IObjectWithSite
	{
		//
		//    HRESULT SetSite(
		//				[in] IUnknown * pUnkSite);
		//
		void SetSite(
			[MarshalAs(UnmanagedType.Interface)] object pUnkSite);

		//
		// HRESULT GetSite(
		//        [in] REFIID riid,
		//        [out, iid_is(riid)] void ** ppvSite);
		//
		void GetSite([In] ref Guid riid, [Out, MarshalAs(UnmanagedType.LPArray)] object[] ppvSite);
	}
}
