namespace VSLangProj
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using EnvDTE;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Contains the information specific to a Visual Basic or C# project. It is returned by the <see cref="P:EnvDTE.Project.Object" /> object when the project is a Visual Basic, Visual C#, or Visual J# project.</summary>
	[ComImport, Guid("2CFB826F-F6BF-480D-A546-95A0381CC411"), TypeLibType(0x1040)]
	internal interface VSProject
	{
		/// <summary>Gets the <see cref="T:VSLangProj.References" /> collection for the project. Read-only. </summary>
		/// <returns>A <see cref="T:VSLangProj.References" /> collection containing <see cref="T:VSLangProj.Reference" /> objects, each one representing a reference.</returns>
		[DispId(1)]
		References References { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(1)] get; }
		/// <summary>Gets the BuildManager object of the <see cref="T:VSLangProj.VSProject" />. Read-only.</summary>
		/// <returns>Returns a <see cref="T:VSLangProj.BuildManager" /> object.</returns>
		[DispId(2)]
		BuildManager BuildManager { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(2)] get; }
		/// <summary>Gets the top-level extensibility object.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.DTE" /> object.</returns>
		[DispId(3)]
		DTE DTE { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(3)] get; }
		/// <summary>Gets the generic <see cref="T:EnvDTE.Project" /> object associated with the Visual Basic, Visual C#, or Visual J# project. Read-only.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.Project" /> object.</returns>
		[DispId(4)]
		Project Project { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(4)] get; }
		/// <summary>Creates the Web References folder for the project.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.ProjectItem" /> object.</returns>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(5)]
		ProjectItem CreateWebReferencesFolder();
		/// <summary>Gets the <see cref="T:EnvDTE.ProjectItem" /> object representing the Web References folder of the project. If the folder does not exist, this property returns Nothing (a null reference). Read-only.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.ProjectItem" /> object representing the Web References folder.</returns>
		[DispId(6)]
		ProjectItem WebReferencesFolder { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(6)] get; }
		/// <summary>Adds a reference to a Web Service to the project. A new Web Service reference subfolder is added to the Web References folder of the project. This new folder contains several other project items related to the Web Service. The method returns the <see cref="T:EnvDTE.ProjectItem" /> object associated with the new Web Service folder.</summary>
		/// <returns>Returns a <see cref="T:EnvDTE.ProjectItem" /> object that is the new Web Reference folder.</returns>
		/// <param name="bstrUrl">Required. Typically, this is a file name URL with a .disco or .vsdisco extension. </param>
		[return: MarshalAs(UnmanagedType.Interface)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(7)]
		ProjectItem AddWebReference([In, MarshalAs(UnmanagedType.BStr)] string bstrUrl);
		/// <summary>Gets the full path of the directory that contains the project-item templates for Visual Basic or C#. Read-only.</summary>
		/// <returns>Returns a string value representing the template path.</returns>
		[DispId(8)]
		string TemplatePath { [return: MarshalAs(UnmanagedType.BStr)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(8)] get; }
		/// <summary>Refreshes the appearance of the project in Solution Explorer, refreshes the references, and gets the latest compiled versions of the files.</summary>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(9)]
		void Refresh();
		/// <summary>Gets or sets whether a Web project is working online or offline. When it is working offline, development continues on an offline store of project files, so that the project files on the server are not changed.</summary>
		/// <returns>For Web applications, this property returns true if working offline and false if working online. For local projects, this property returns false. </returns>
		[DispId(10)]
		bool WorkOffline { [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] get; [param: In][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(10)] set; }
		/// <summary>Gets the <see cref="T:VSLangProj.Imports" /> object associated with the project. For C# projects, the Imports property is set to Nothing (a null reference). Read-only.</summary>
		/// <returns>Returns an <see cref="T:VSLangProj.Imports" /> object collection.</returns>
		[DispId(11)]
		Imports Imports { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(11)] get; }
		/// <summary>Gets a <see cref="T:VSLangProj.VSProjectEvents" /> object that allows you to respond to events of the <see cref="T:VSLangProj.Imports" />, <see cref="T:VSLangProj.References" />, and <see cref="T:VSLangProj.BuildManager" /> objects.</summary>
		/// <returns>A <see cref="T:VSLangProj.VSProjectEvents" /> object.</returns>
		[DispId(12)]
		VSProjectEvents Events { [return: MarshalAs(UnmanagedType.Interface)][MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(12)] get; }
		/// <summary>Copies some or all of a Web project to a new location. </summary>
		/// <param name="bstrDestFolder">Required. The destination folder for the new project. The folder is created if it does not exist. This string takes the form "http://ServerName/CopyName/".</param>
		/// <param name="bstrDestUNCPath">Required. The UNC path for file-sharing if the project to be copied is using file-share access. Use the empty string if the project is using FrontPage Server Extensions access.</param>
		/// <param name="copyProjectOption">Required. A <see cref="T:VSLangProj.prjCopyProjectOption" /> value that determines which project files are copied — all the files, all the project files, or only the files needed to run the application.</param>
		/// <param name="bstrUsername">Required. The user's network login ID.</param>
		/// <param name="bstrPassword">Required. The user's password. </param>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(13)]
		void CopyProject([In, MarshalAs(UnmanagedType.BStr)] string bstrDestFolder, [In, MarshalAs(UnmanagedType.BStr)] string bstrDestUNCPath, [In] prjCopyProjectOption copyProjectOption, [In, MarshalAs(UnmanagedType.BStr)] string bstrUsername, [In, MarshalAs(UnmanagedType.BStr)] string bstrPassword);
		/// <summary>Microsoft Internal Use Only.</summary>
		/// <param name="command"> </param>
		/// <param name="bSuppressUI"> </param>
		/// <param name="varIn"> </param>
		/// <param name="pVarOut"> </param>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(14), TypeLibFunc(0x40)]
		void Exec([In] prjExecCommand command, int bSuppressUI, [In, MarshalAs(UnmanagedType.Struct)] object varIn, [MarshalAs(UnmanagedType.Struct)] out object pVarOut);
		/// <summary>Generates a public/private key file used to form a strong name for the assembly.</summary>
		/// <param name="strPublicPrivateFile">This name of the file to be generated.</param>
		/// <param name="strPublicOnlyFile">If true, only the public key is generated.</param>
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(15)]
		void GenerateKeyPairFiles([In, MarshalAs(UnmanagedType.BStr)] string strPublicPrivateFile, [In, Optional, DefaultParameterValue("0"), MarshalAs(UnmanagedType.BStr)] string strPublicOnlyFile);
		/// <summary>Generates a unique file name within the project. Used for naming new project items.</summary>
		/// <returns>A file name that is unique in the project or project folder. If the first parameter is a <see cref="T:EnvDTE.Project" /> instance, then the name generated will be unique within the root project folder. The file name will take the form Root?.Extension, where ? is one or more numerals (1, 2, 3, and so on).</returns>
		/// <param name="pDispatch">The name generated will have this value as the extension.</param>
		/// <param name="bstrRoot">A <see cref="T:EnvDTE.Project" /> object or a folder <see cref="T:EnvDTE.ProjectItem" /> object. The file name generated will be unique within this context. </param>
		/// <param name="bstrDesiredExt">The name generated will start with this string.</param>
		[return: MarshalAs(UnmanagedType.BStr)]
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime), DispId(0x10)]
		string GetUniqueFilename([In, MarshalAs(UnmanagedType.IDispatch)] object pDispatch, [In, MarshalAs(UnmanagedType.BStr)] string bstrRoot, [In, MarshalAs(UnmanagedType.BStr)] string bstrDesiredExt);
	}
}

