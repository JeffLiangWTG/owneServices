namespace VSLangProj
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Microsoft Internal Use Only.</summary>
	[ComImport, InterfaceType(2), TypeLibType(0x1000), Guid("287EB27C-0F8B-4D2D-8E82-A9CA50B6766E")]
	internal interface _dispReferencesEvents
	{
		/// <summary>Microsoft Internal Use Only.</summary>
		/// <param name="pReference"> </param>
		[PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)]
		void ReferenceAdded([In, MarshalAs(UnmanagedType.Interface)] Reference pReference);
		/// <summary>Microsoft Internal Use Only.</summary>
		/// <param name="pReference"> </param>
		[PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)]
		void ReferenceRemoved([In, MarshalAs(UnmanagedType.Interface)] Reference pReference);
		/// <summary>Microsoft Internal Use Only.</summary>
		/// <param name="pReference"> </param>
		[PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)]
		void ReferenceChanged([In, MarshalAs(UnmanagedType.Interface)] Reference pReference);
	}
}

