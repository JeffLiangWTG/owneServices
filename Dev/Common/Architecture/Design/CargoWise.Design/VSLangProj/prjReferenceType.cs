namespace VSLangProj
{
	using System;
	using System.Runtime.InteropServices;

	/// <summary>Indicates whether a Reference is an assembly or a COM component.</summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	[Guid("B21668EF-29F1-425D-85D7-118CB838C362")]
	internal enum prjReferenceType
	{
		prjReferenceTypeAssembly,
		prjReferenceTypeActiveX
	}
}

