namespace VSLangProj
{
	using System.Runtime.InteropServices;

	[ClassInterface(ClassInterfaceType.None)]
	internal sealed class _dispBuildManagerEvents_SinkHelper : _dispBuildManagerEvents
	{
		public _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler m_DesignTimeOutputDeletedDelegate;
		public _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler m_DesignTimeOutputDirtyDelegate;
		public int m_dwCookie;

		internal _dispBuildManagerEvents_SinkHelper()
		{
		}

		public void DesignTimeOutputDeleted(string a_1)
		{
			m_DesignTimeOutputDeletedDelegate?.Invoke(a_1);
		}

		public void DesignTimeOutputDirty(string a_1)
		{
			m_DesignTimeOutputDirtyDelegate?.Invoke(a_1);
		}
	}
}

