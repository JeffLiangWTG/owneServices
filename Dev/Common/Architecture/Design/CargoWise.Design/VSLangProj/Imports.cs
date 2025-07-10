namespace VSLangProj
{
	using System;
	using System.Collections;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using EnvDTE;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Contains the collection of all project imports for a Visual Basic project. The specified imports statements are passed directly to the compiler and apply to all the files in the project.</summary>
	[ComImport, Guid("642789F9-210D-4574-96FD-5A653451E216"), DefaultMember("Item"), TypeLibType(0x1040)]
	internal interface Imports : IEnumerable
	{
		/// <summary>Gets the top-level extensibility object.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.DTE" /> object.</returns>
		[DispId(1)]
		DTE DTE { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)] get; }
		/// <summary>Gets the immediate parent object of a given object.</summary>
		/// <returns>Parent object.</returns>
		[DispId(2)]
		object Parent { [return: MarshalAs(UnmanagedType.IDispatch)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)] get; }
		/// <summary>Gets the project that contains the selected item. Read-only.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.Project" /> object.</returns>
		[DispId(3)]
		Project ContainingProject { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)] get; }
		/// <summary>Gets a value indicating the number of objects in the collection. Read-only.</summary>
		/// <returns>The number of objects in the collection.</returns>
		[DispId(4)]
		int Count { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(4)] get; }
		/// <summary>Returns an indexed member of the <see cref="T:VSLangProj.Imports" /> object. The Item method is the default method for the <see cref="T:VSLangProj.Imports" /> object.</summary>
		/// <returns>Returns an imports statement string by its position in the collection.</returns>
		/// <param name="lIndex">Required. A one-based index into <see cref="T:VSLangProj.Imports" /> object.</param>
		[return: MarshalAs(UnmanagedType.BStr)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0)]
		string Item([In] int lIndex);
		/// <summary>Adds a new import statement to the <see cref="T:VSLangProj.Imports" /> collection.</summary>
		/// <param name="bstrImport">Required. The import statement string. The format of an import string is:[alias = ]namespace[.*]Items enclosed in square brackets are optional. </param>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(5)]
		void Add([In, MarshalAs(UnmanagedType.BStr)] string bstrImport);
		/// <summary>Removes an imports statement from the <see cref="T:VSLangProj.Imports" /> collection. The imports statement to be removed may be indexed by either the string specifying the import or by a one-based index.</summary>
		/// <param name="index">Required. Either a one-based index into the collection, or the imports statement string to remove.</param>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(6)]
		void Remove([In, MarshalAs(UnmanagedType.Struct)] object index);
		/// <summary>Gets an enumeration for items in a collection.</summary>
		/// <returns>An enumerator.</returns>
	}
}

