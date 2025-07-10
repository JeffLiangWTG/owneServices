namespace VSLangProj
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Microsoft Internal Use Only.</summary>
	[ComImport, InterfaceType(2), Guid("40806CEA-ABAB-4887-A356-D8869C28A6E6"), TypeLibType(0x1000)]
	internal interface _dispImportsEvents
	{
		/// <summary>Microsoft Internal Use Only.</summary>
		/// <param name="bstrImport"> </param>
		[PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
		void ImportAdded([In, MarshalAs(UnmanagedType.BStr)] string bstrImport);
		/// <summary>Microsoft Internal Use Only.</summary>
		/// <param name="bstrImport"> </param>
		[PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
		void ImportRemoved([In, MarshalAs(UnmanagedType.BStr)] string bstrImport);
	}
}

