namespace VSLangProj
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Provides access to events of the <see cref="T:VSLangProj.References" />, <see cref="T:VSLangProj.Imports" />, and <see cref="T:VSLangProj.BuildManager" /> objects.</summary>
	[ComImport, TypeLibType(0x1040), Guid("F8B92546-F1A2-4066-92F6-FDF2E691A50C")]
	internal interface VSProjectEvents
	{
		/// <summary>Gets a <see cref="T:VSLangProj.ReferencesEvents" /> object that provides access to the events of the <see cref="T:VSLangProj.References" /> collection.</summary>
		/// <returns>Returns a <see cref="T:VSLangProj.ReferencesEvents" /> object.</returns>
		[DispId(1)]
		ReferencesEvents ReferencesEvents { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)] get; }
		/// <summary>Gets a <see cref="T:VSLangProj.BuildManagerEvents" /> object that provides access to the BuildManager events.</summary>
		/// <returns>Returns a <see cref="T:VSLangProj.BuildManagerEvents" /> object.</returns>
		[DispId(2)]
		BuildManagerEvents BuildManagerEvents { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)] get; }
		/// <summary>Gets an <see cref="T:VSLangProj.ImportsEvents" /> object that provides access to the Imports object events.</summary>
		/// <returns>Returns an <see cref="T:VSLangProj.ImportsEvents" /> object.</returns>
		[DispId(3)]
		ImportsEvents ImportsEvents { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)] get; }
	}
}

