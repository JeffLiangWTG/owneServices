namespace VSLangProj
{
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Provides access to events that are raised when a project Imports statement is added to or deleted from a Visual Basic project. Use this object for functionality and refer to <see cref="T:VSLangProj.ImportsEventsClass" /> for this object’s documentation.</summary>
	[ComImport, CoClass(typeof(ImportsEventsClass)), Guid("037AD859-7A75-4CF3-8A38-83D6E045FEE3")]
	internal interface ImportsEvents : _ImportsEvents, _dispImportsEvents_Event
	{
	}
}

