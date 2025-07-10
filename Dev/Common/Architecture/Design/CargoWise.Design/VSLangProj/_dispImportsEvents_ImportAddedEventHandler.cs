namespace VSLangProj
{
	using System.Runtime.InteropServices;

	/// <summary>Microsoft Internal Use Only.</summary>
	/// <param name="bstrImport"> </param>
	[ComVisible(false)]
	internal delegate void _dispImportsEvents_ImportAddedEventHandler([In, MarshalAs(UnmanagedType.BStr)] string bstrImport);
}

