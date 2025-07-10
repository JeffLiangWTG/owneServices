namespace VSLangProj
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using EnvDTE;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Represents one reference in the project. Including a reference in a project allows you to use any of the public members contained in the reference. Projects may include references to other .NET projects, .NET assemblies, and COM objects.</summary>
	[ComImport, TypeLibType(0x1040), Guid("35D6FB50-35B6-4C81-B91C-3930B0D95386")]
	internal interface Reference
	{
		/// <summary>Gets the top-level extensibility object.</summary>
		/// <returns>Returns a DTE object.</returns>
		[DispId(1)]
		DTE DTE { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)] get; }
		/// <summary>Gets the collection containing the object supporting this property or contained within this code construct.</summary>
		/// <returns>Returns a collection relating to the object. See Remarks for more information.</returns>
		[DispId(2)]
		References Collection { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)] get; }
		/// <summary>Gets the project that the selected item is a part of. Read-only.</summary>
		/// <returns>Returns a Project object.</returns>
		[DispId(3)]
		Project ContainingProject { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)] get; }
		/// <summary>Gets the reference from the <see cref="T:VSLangProj.References" /> object that contains it.</summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(4)]
		void Remove();
		/// <summary>Gets the name of the object. Read-only.</summary>
		/// <returns>String.The string returned depends on the reference type.Reference TypeValue ReturnedAssemblyThe assembly name, which is the file name of the reference without the extension.COMThe name of the wrapper for the referenced type library, which is the file name without the extension.</returns>
		[DispId(5)]
		string Name { [return: MarshalAs(UnmanagedType.BStr)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(5)] get; }
		/// <summary>Gets a <see cref="T:VSLangProj.prjReferenceType" /> value indicating whether the reference is an assembly or a COM component. Read-only.</summary>
		/// <returns>Returns a <see cref="T:VSLangProj.prjReferenceType" /> enumeration value.</returns>
		[DispId(6)]
		prjReferenceType Type { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(6)] get; }
		/// <summary>Gets the unique identifier of the reference. Read-only. </summary>
		/// <returns>The string returned depends on the reference type.Reference TypeValue ReturnedAssemblyAssembly name (assembly name without path or extension)COMThe format is:"GUID\major.minor\localeid\wrappertool"For example,"{00000205-0000-0010-8000-00AA006D2EA4}\2.5\409\tlbimp"is returned for ActiveX Data Objects 2.5.</returns>
		[DispId(7)]
		string Identity { [return: MarshalAs(UnmanagedType.BStr)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)] get; }
		/// <summary>Gets the path to the reference file. Read-only. </summary>
		/// <returns>String. This property returns the path and file name of the reference, if it can be resolved. If the path cannot be resolved, then a blank string is returned. For information on resolving references, see <see cref="M:VSLangProj.References.Add(System.String)" />.</returns>
		[DispId(8)]
		string Path { [return: MarshalAs(UnmanagedType.BStr)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] get; }
		/// <summary>Gets a text description of the reference. Read-only.</summary>
		/// <returns>For a <see cref="T:VSLangProj.Reference" /> object, the return value depends on the reference type.Reference TypeValue ReturnedAssemblyAssembly description.COMType library description.</returns>
		[DispId(9)]
		string Description { [return: MarshalAs(UnmanagedType.BStr)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)] get; }
		/// <summary>Gets the culture string of a reference. Read-only.</summary>
		/// <returns>The return value depends on the reference type.Reference TypeValue ReturnedAssemblyCulture string. For example, "EN-US" for English - United States.COMLocale ID. The string is the hex locale ID of the type library being reference. For example, "0" for multilanguage or "409" for English - United States.</returns>
		[DispId(10)]
		string Culture { [return: MarshalAs(UnmanagedType.BStr)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] get; }
		/// <summary>Gets the major version number of the reference. Read-only. </summary>
		/// <returns>Long. The number returned depends on the reference type.Reference TypeValue ReturnedAssemblyMajor release number of the reference; 0–9999.COMA long value.</returns>
		[DispId(11)]
		int MajorVersion { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(11)] get; }
		/// <summary>Gets the minor version number of the reference. Read-only. </summary>
		/// <returns>Long. The number returned depends on the reference type.Reference TypeValue ReturnedAssemblyMinor release number of the reference; 0–9999. COMA long value.</returns>
		[DispId(12)]
		int MinorVersion { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(12)] get; }
		/// <summary>Gets the revision number of the reference. Read-only. </summary>
		/// <returns>Long. The number returned depends on the reference type.Reference TypeValue ReturnedAssemblyRevision version number of the reference; 0–9999.COM0</returns>
		[DispId(13)]
		int RevisionNumber { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(13)] get; }
		/// <summary>Gets the build number of the reference. Read-only.</summary>
		/// <returns>Long. The value returned depends on the reference type.Reference TypeValue ReturnedAssemblyBuild number of the reference; 0-9999.COM0</returns>
		[DispId(14)]
		int BuildNumber { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(14)] get; }
		/// <summary>Gets whether the reference is signed with a public/private key pair. Read-only. </summary>
		/// <returns>true if the reference is signed with a public/private key pair.</returns>
		[DispId(15)]
		bool StrongName { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(15)] get; }
		/// <summary>Gets a Project object if the reference is a project. Otherwise, it returns Nothing (a null reference). Read-only. </summary>
		/// <returns>Returns a Project object.</returns>
		[DispId(0x10)]
		Project SourceProject { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x10)] get; }
		/// <summary>Determines whether the reference is copied to the local bin path. </summary>
		/// <returns>Boolean.</returns>
		[DispId(0x11)]
		bool CopyLocal { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x11)] get; [param: In][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x11)] set; }
		/// <summary>Gets the requested Extender object if it is available for this object.</summary>
		/// <returns>This property returns Nothing or null is no Extender object is available.</returns>
		/// <param name="extenderName">Required. The name of the Extender to return.</param>
		[DispId(0x12)]
		object this[string extenderName] { [return: MarshalAs(UnmanagedType.IDispatch)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x12), TypeLibFunc(0x440)] get; }
		/// <summary>Gets a list of available Extenders for the object.</summary>
		/// <returns>An object containing an automation-compliant array of strings.</returns>
		[DispId(0x13)]
		object ExtenderNames { [return: MarshalAs(UnmanagedType.Struct)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc(0x440), DispId(0x13)] get; }
		/// <summary>Gets the Extender category ID (CATID) for the object.</summary>
		/// <returns>Returns the GUID representing the CATID of the object as a string.</returns>
		[DispId(20)]
		string ExtenderCATID { [return: MarshalAs(UnmanagedType.BStr)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), TypeLibFunc(0x440), DispId(20)] get; }
		/// <summary>Gets the public key token for the referenced assembly.</summary>
		/// <returns>A string representing the public key token for the referenced assembly.</returns>
		[DispId(0x15)]
		string PublicKeyToken { [return: MarshalAs(UnmanagedType.BStr)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x15), TypeLibFunc(0x440)] get; }
		/// <summary>Gets the version of the selected reference.</summary>
		/// <returns>A string representing the version of the selected reference.</returns>
		[DispId(0x16)]
		string Version { [return: MarshalAs(UnmanagedType.BStr)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x16)] get; }
	}
}

