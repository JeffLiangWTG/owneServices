namespace VSLangProj
{
	using System;
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Refer to <see cref="T:VSLangProj.ImportsEvents" /> for this functionality. Do not instantiate from this class.</summary>
	[ComImport, ComSourceInterfaces("VSLangProj._dispImportsEvents\0"), ClassInterface((short)0), TypeLibType(2), Guid("AC779606-837C-444F-B8FA-A69805B59976")]
	internal abstract class ImportsEventsClass : _ImportsEvents, ImportsEvents, _dispImportsEvents_Event
	{
		/// <summary>Occurs immediately after an Imports statement has been added to a Visual Basic project.</summary>
		public abstract event _dispImportsEvents_ImportAddedEventHandler ImportAdded;

		/// <summary>Occurs immediately after an Imports statement is removed from a Visual Basic project.</summary>
		public abstract event _dispImportsEvents_ImportRemovedEventHandler ImportRemoved;
	}
}

