namespace VSLangProj
{
	using System.Runtime.InteropServices;

	/// <summary>Microsoft Internal Use Only.</summary>
	[ComEventInterface(typeof(_dispReferencesEvents), typeof(_dispReferencesEvents_EventProvider)), ComVisible(false)]
	internal interface _dispReferencesEvents_Event
	{
		/// <summary>Microsoft Internal Use Only.</summary>
		event _dispReferencesEvents_ReferenceAddedEventHandler ReferenceAdded;

		/// <summary>Microsoft Internal Use Only.</summary>
		event _dispReferencesEvents_ReferenceChangedEventHandler ReferenceChanged;

		/// <summary>Microsoft Internal Use Only.</summary>
		event _dispReferencesEvents_ReferenceRemovedEventHandler ReferenceRemoved;
	}
}

