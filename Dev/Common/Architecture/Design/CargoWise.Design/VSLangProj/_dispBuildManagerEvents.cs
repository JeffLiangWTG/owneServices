namespace VSLangProj
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Microsoft Internal Use Only.</summary>
	[ComImport, InterfaceType(2), TypeLibType(0x1000), Guid("828914F7-1D81-4F5C-83CE-37819D7EE759")]
	internal interface _dispBuildManagerEvents
	{
		/// <summary>Microsoft Internal Use Only.</summary>
		/// <param name="bstrOutputMoniker"> </param>
		[PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
		void DesignTimeOutputDirty([In, MarshalAs(UnmanagedType.BStr)] string bstrOutputMoniker);
		/// <summary>Microsoft Internal Use Only.</summary>
		/// <param name="bstrOutputMoniker"> </param>
		[PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
		void DesignTimeOutputDeleted([In, MarshalAs(UnmanagedType.BStr)] string bstrOutputMoniker);
	}
}

