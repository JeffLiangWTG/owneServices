namespace VSLangProj
{
	using System.Runtime.InteropServices;

	/// <summary>Microsoft Internal Use Only.</summary>
	/// <param name="bstrOutputMoniker"> </param>
	[ComVisible(false)]
	internal delegate void _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler([In, MarshalAs(UnmanagedType.BStr)] string bstrOutputMoniker);
}

