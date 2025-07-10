namespace VSLangProj
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using EnvDTE;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Used by third-party developers to manage the portable executable (PE) files produced by running custom tools.</summary>
	[ComImport, Guid("C711E2B7-3C58-4C37-9359-705208A890AE"), TypeLibType(0x1040)]
	internal interface BuildManager
	{
		/// <summary>Gets the top-level extensibility object.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.DTE" /> object.</returns>
		[DispId(1)]
		DTE DTE { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)] get; }
		/// <summary>Gets the immediate parent object of a given object.</summary>
		/// <returns>Parent object</returns>
		[DispId(2)]
		object Parent { [return: MarshalAs(UnmanagedType.IDispatch)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)] get; }
		/// <summary>Gets the project of which the selected item is a part. Read-only.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.Project" /> object.</returns>
		[DispId(3)]
		Project ContainingProject { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)] get; }
		/// <summary>Gets the temporary portable executable (PE) monikers for a project.</summary>
		/// <returns>The returned object is an array of strings, where each string in the array is a temporary PE moniker.</returns>
		[DispId(4)]
		object DesignTimeOutputMonikers { [return: MarshalAs(UnmanagedType.Struct)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(4)] get; }
		/// <summary>Builds a temporary portable executable (PE) and returns its description in an XML string.</summary>
		/// <returns>The XML description of the portable executable follows this format, shown for a custom tool output file named DataSchema.cs:&lt;root&gt;
		/// &lt;Application private_binpath = "AbsoluteTemporaryOutputPath"/&gt;
		/// &lt;Assembly
		/// codebase = "DataSchema.cs.dll"
		/// name = "DataSchema.cs"
		/// version = "0.0.0.0"
		/// snapshot_id = "0"
		/// replaceable = "True"
		/// /&gt;
		/// &lt;/root&gt;The attributes are described below.AttributeDescriptionprivate_binpathThe full path of the directory that contains the temporary PE. This path is combined with the <paramref name="codebase" /> attribute to create the full path to the temporary PE assembly.codebaseThe path and file name of the temporary PE relative to the <paramref name="private_binpath" />.nameThe .NET Framework simple name of the assembly being generated. versionThe major, minor, build, and revision version numbers. The value is always "0.0.0.0".snapshot_idAn integer that is incremented each time the PE is recompiled.replaceableIndicates whether the project system can replace the assembly, and whether a designer should not hold a write lock on the file. Is always set to true.</returns>
		/// <param name="bstrOutputMoniker">Required. Moniker string for the temporary PE to build. The moniker string must be one of the strings returned by the <see cref="P:VSLangProj.BuildManager.DesignTimeOutputMonikers" /> property.</param>
		[return: MarshalAs(UnmanagedType.BStr)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(5)]
		string BuildDesignTimeOutput([In, MarshalAs(UnmanagedType.BStr)] string bstrOutputMoniker);
	}
}

