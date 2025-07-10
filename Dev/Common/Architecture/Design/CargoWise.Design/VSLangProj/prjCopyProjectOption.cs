namespace VSLangProj
{
	using System;
	using System.Runtime.InteropServices;

	/// <summary>This enumeration is used by the <see cref="M:VSLangProj.VSProject.CopyProject(System.String,System.String,VSLangProj.prjCopyProjectOption,System.String,System.String)" /> to determine which project files are copied when the project is copied.</summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	[Guid("5DE9F309-E701-44D1-8068-1860738C0084")]
	internal enum prjCopyProjectOption
	{
		prjRunFiles,
		prjProjectFiles,
		prjAllFiles
	}
}

