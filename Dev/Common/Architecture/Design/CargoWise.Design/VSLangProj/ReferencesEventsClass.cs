namespace VSLangProj
{
	using System;
	using System.Runtime.InteropServices;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	/// <summary>Refer to <see cref="T:VSLangProj.ReferencesEvents" /> for this functionality. Do not instantiate from this class.</summary>
	[ComImport, ComSourceInterfaces("VSLangProj._dispReferencesEvents\0"), ClassInterface((short)0), TypeLibType(2), Guid("1CDB29FE-33B7-4392-9742-D9415D3408FE")]
	internal abstract class ReferencesEventsClass : _ReferencesEvents, ReferencesEvents, _dispReferencesEvents_Event
	{
		/// <summary>Occurs immediately after a reference is added to a project.</summary>
		public abstract event _dispReferencesEvents_ReferenceAddedEventHandler ReferenceAdded;

		/// <summary>Occurs immediately after a reference is modified in a project.</summary>
		public abstract event _dispReferencesEvents_ReferenceChangedEventHandler ReferenceChanged;

		/// <summary>Occurs immediately after a reference is removed from a project.</summary>
		public abstract event _dispReferencesEvents_ReferenceRemovedEventHandler ReferenceRemoved;
	}
}

