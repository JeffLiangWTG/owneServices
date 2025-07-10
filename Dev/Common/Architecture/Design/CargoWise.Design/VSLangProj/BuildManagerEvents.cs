namespace VSLangProj
{
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Provides access to the events of the <see cref="T:VSLangProj.BuildManager" />. Use this object for functionality and refer to <see cref="T:VSLangProj.BuildManagerEventsClass" /> for this object’s documentation.</summary>
	[ComImport, CoClass(typeof(BuildManagerEventsClass)), Guid("5F4AAE42-BC94-401D-9213-B8A8B9E553DE")]
	internal interface BuildManagerEvents : _BuildManagerEvents, _dispBuildManagerEvents_Event
	{
	}
}

