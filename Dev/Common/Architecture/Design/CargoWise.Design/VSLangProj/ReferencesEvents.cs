namespace VSLangProj
{
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Provides access to events that are raised by adding, deleting, or changing project references. Use this object for functionality and refer to <see cref="T:VSLangProj.ReferencesEventsClass" /> for this object’s documentation.</summary>
	[ComImport, CoClass(typeof(ReferencesEventsClass)), Guid("1CF40C9E-D548-4B45-AD0F-3D7843F62BBB")]
	internal interface ReferencesEvents : _ReferencesEvents, _dispReferencesEvents_Event
	{
	}
}

