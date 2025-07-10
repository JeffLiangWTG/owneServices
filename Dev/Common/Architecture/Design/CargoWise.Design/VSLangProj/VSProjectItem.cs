namespace VSLangProj
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using EnvDTE;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Contains the information specific to a Visual Basic or C# project item. It is returned by the <see cref="P:EnvDTE.ProjectItem.Object" /> of the <see cref="T:EnvDTE.ProjectItem" /> object when the project is a Visual Basic or C# project.</summary>
	[ComImport, Guid("89FF44C6-979D-49B6-AF56-EC9509001DE4"), TypeLibType(0x1040)]
	internal interface VSProjectItem
	{
		/// <summary>Returns the top-level extensibility object.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.DTE" /> object.</returns>
		[DispId(1)]
		DTE DTE { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)] get; }
		/// <summary>Gets the <see cref="T:EnvDTE.ProjectItem" /> object associated with the given object.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.ProjectItem" /> object.</returns>
		[DispId(2)]
		ProjectItem ProjectItem { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)] get; }
		/// <summary>Gets the project that the selected item is a part of. Read-only.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.Project" /> object.</returns>
		[DispId(3)]
		Project ContainingProject { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)] get; }
		/// <summary>Runs the custom tool associated with a ProjectItem object. A custom tool is a registered component that implements the IVsSingleFileGenerator interface. Custom tools are similar to designers and editors. </summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(4)]
		void RunCustomTool();
	}
}

