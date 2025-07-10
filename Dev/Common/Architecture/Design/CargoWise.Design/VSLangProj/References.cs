namespace VSLangProj
{
	using System;
	using System.Collections;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using EnvDTE;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Contains all the references to external components in a Visual Basic or C# project.</summary>
	[ComImport, DefaultMember("Item"), TypeLibType(0x1040), Guid("B8758EE4-0553-4BC9-8432-440449D35C14")]
	internal interface References : IEnumerable
	{
		/// <summary>Gets the top-level extensibility object.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.DTE" /> object.</returns>
		[DispId(1)]
		DTE DTE { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)] get; }
		/// <summary>Gets the immediate parent object of a given object.</summary>
		/// <returns />
		[DispId(2)]
		object Parent { [return: MarshalAs(UnmanagedType.IDispatch)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)] get; }
		/// <summary>Gets the project of which the selected item is a part. Read-only.</summary>
		/// <returns>Returns a Project object.</returns>
		[DispId(3)]
		Project ContainingProject { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)] get; }
		/// <summary>Gets a value indicating the number of objects in the collection. Read-only.</summary>
		/// <returns>The number of objects in the collection.</returns>
		[DispId(4)]
		int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(4)] get; }
		/// <summary>Returns an indexed <see cref="T:VSLangProj.Reference" /> object.</summary>
		/// <returns>Returns a <see cref="T:VSLangProj.Reference" /> object.</returns>
		/// <param name="index">Required. Either the <see cref="P:VSLangProj.Reference.Identity" /> string of the <see cref="T:VSLangProj.Reference" /> object or a one-based index into the <see cref="T:VSLangProj.References" /> collection.</param>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)]
		Reference Item([In, MarshalAs(UnmanagedType.Struct)] object index);
		/// <summary>Searches the references in a project for an object that has the specified <see cref="P:VSLangProj.Reference.Identity" />.</summary>
		/// <returns>Returns a <see cref="T:VSLangProj.Reference" /> object.</returns>
		/// <param name="bstrIdentity">Required. The <see cref="P:VSLangProj.Reference.Identity" /> of the <see cref="T:VSLangProj.Reference" /> object.</param>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(5)]
		Reference Find([In, MarshalAs(UnmanagedType.BStr)] string bstrIdentity);
		/// <summary>Adds a new reference to the project.</summary>
		/// <returns>Returns a <see cref="T:VSLangProj.Reference" /> object.</returns>
		/// <param name="bstrPath">Required. The path of a COM or .NET Framework reference. This parameter may take three forms:A simple .NET Framework object name, such as "System". A .NET Framework file name (.tlb, .dll, or .ocx file).A COM library file (.tlb, .dll, .ocx, or .exe file). For COM objects, the path needs to include a full path and file name, such as, "D:\SomeApplication\SomeApp.dll". See restrictions in the "COM References" section below on adding COM .exe file references.</param>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(6)]
		Reference Add([In, MarshalAs(UnmanagedType.BStr)] string bstrPath);
		/// <summary>Adds a reference to an ActiveX type library.</summary>
		/// <returns>Returns a <see cref="T:VSLangProj.Reference" /> object for the requested ActiveX library object.</returns>
		/// <param name="bstrTypeLibGuid">Required. The type library's globally unique identifier (GUID), expressed as a string.</param>
		/// <param name="lMajorVer">Optional. The major version number. If omitted, 0 is used.</param>
		/// <param name="lMinorVer">Optional. The minor version number. If omitted, 0 is used.</param>
		/// <param name="lLocaleId">Optional locale id. For more information, see <see cref="P:EnvDTE.DTEClass.LocaleID" />.</param>
		/// <param name="bstrWrapperTool">The wrapper tool to be used when generating an assembly wrapper for the type library. The supported values are described in the following table. ValueResult"tlbimp"The project system generates a generic wrapper suitable for accessing the COM object's methods and properties."aximp"The project system generates a wrapper that Windows forms uses for ActiveX hosting. "" (empty string)The project system checks if a primary interoperability assembly for the specified type library exists. If found, then that assembly is used as the wrapper for the COM object's methods and properties. If not found, the behavior is the same as if "tlbimp" were specified.</param>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)]
		Reference AddActiveX([In, MarshalAs(UnmanagedType.BStr)] string bstrTypeLibGuid, [In, Optional, DefaultParameterValue(0)] int lMajorVer, [In, Optional, DefaultParameterValue(0)] int lMinorVer, [In, Optional, DefaultParameterValue(0)] int lLocaleId, [In, Optional, DefaultParameterValue(""), MarshalAs(UnmanagedType.BStr)] string bstrWrapperTool);
		/// <summary>Adds a new project reference to the project and returns the newly created <see cref="T:VSLangProj.Reference" /> object.</summary>
		/// <returns />
		/// <param name="pProject">Required. The Project object to be added.</param>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)]
		Reference AddProject([In, MarshalAs(UnmanagedType.Interface)] Project pProject);
	}
}

