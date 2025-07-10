namespace VSLangProj
{
	using System.Runtime.InteropServices;

	/// <summary>Microsoft Internal Use Only.</summary>
	[ComVisible(false), ComEventInterface(typeof(_dispImportsEvents), typeof(_dispImportsEvents_EventProvider))]
	internal interface _dispImportsEvents_Event
	{
		/// <summary>Microsoft Internal Use Only.</summary>
		event _dispImportsEvents_ImportAddedEventHandler ImportAdded;

		/// <summary>Microsoft Internal Use Only.</summary>
		event _dispImportsEvents_ImportRemovedEventHandler ImportRemoved;
	}
}

