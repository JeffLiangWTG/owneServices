namespace VSLangProj
{
	using System.Runtime.InteropServices;

	[ClassInterface(ClassInterfaceType.None)]
	internal sealed class _dispImportsEvents_SinkHelper : _dispImportsEvents
	{
		public int m_dwCookie;
		public _dispImportsEvents_ImportAddedEventHandler m_ImportAddedDelegate;
		public _dispImportsEvents_ImportRemovedEventHandler m_ImportRemovedDelegate;

		internal _dispImportsEvents_SinkHelper()
		{
		}

		public void ImportAdded(string a_1)
		{
			m_ImportAddedDelegate?.Invoke(a_1);
		}

		public void ImportRemoved(string a_1)
		{
			m_ImportRemovedDelegate?.Invoke(a_1);
		}
	}
}

