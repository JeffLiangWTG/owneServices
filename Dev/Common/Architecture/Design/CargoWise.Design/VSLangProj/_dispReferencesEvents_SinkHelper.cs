namespace VSLangProj
{
	using System.Runtime.InteropServices;

	[ClassInterface(ClassInterfaceType.None)]
	internal sealed class _dispReferencesEvents_SinkHelper : _dispReferencesEvents
	{
		public int m_dwCookie;
		public _dispReferencesEvents_ReferenceAddedEventHandler m_ReferenceAddedDelegate;
		public _dispReferencesEvents_ReferenceChangedEventHandler m_ReferenceChangedDelegate;
		public _dispReferencesEvents_ReferenceRemovedEventHandler m_ReferenceRemovedDelegate;

		internal _dispReferencesEvents_SinkHelper()
		{
		}

		public void ReferenceAdded(Reference a_1)
		{
			m_ReferenceAddedDelegate?.Invoke(a_1);
		}

		public void ReferenceChanged(Reference a_1)
		{
			m_ReferenceChangedDelegate?.Invoke(a_1);
		}

		public void ReferenceRemoved(Reference a_1)
		{
			m_ReferenceRemovedDelegate?.Invoke(a_1);
		}
	}
}

