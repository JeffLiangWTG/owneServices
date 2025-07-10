namespace VSLangProj
{
	using System;
	using System.Collections;
	using System.Runtime.InteropServices.ComTypes;
	using System.Threading;
	using CargoWise.Common;

	internal sealed class _dispBuildManagerEvents_EventProvider : _dispBuildManagerEvents_Event, IDisposable
	{
		ArrayList m_aEventSinkHelpers;
		IConnectionPoint m_ConnectionPoint;
		readonly IConnectionPointContainer m_ConnectionPointContainer;

		public _dispBuildManagerEvents_EventProvider(object a_1)
		{
			m_ConnectionPointContainer = (IConnectionPointContainer)a_1;
		}

		public event _dispBuildManagerEvents_DesignTimeOutputDeletedEventHandler DesignTimeOutputDeleted
		{
			add
			{
				lock (this)
				{
					if (m_ConnectionPoint == null)
					{
						Init();
					}
					_dispBuildManagerEvents_SinkHelper helper = new _dispBuildManagerEvents_SinkHelper();
					m_ConnectionPoint.Advise(helper, out var pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_DesignTimeOutputDeletedDelegate = value;
					m_aEventSinkHelpers.Add(helper);
				}
			}
			remove
			{
				lock (this)
				{
					int count = m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count)
					{
						do
						{
							_dispBuildManagerEvents_SinkHelper helper = (_dispBuildManagerEvents_SinkHelper)m_aEventSinkHelpers[index];
							if ((helper.m_DesignTimeOutputDeletedDelegate != null) && (helper.m_DesignTimeOutputDeletedDelegate.Equals(value)))
							{
								m_aEventSinkHelpers.RemoveAt(index);
								m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1)
								{
									m_ConnectionPoint = null;
									m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}

		public event _dispBuildManagerEvents_DesignTimeOutputDirtyEventHandler DesignTimeOutputDirty
		{
			add
			{
				lock (this)
				{
					if (m_ConnectionPoint == null)
					{
						Init();
					}
					_dispBuildManagerEvents_SinkHelper helper = new _dispBuildManagerEvents_SinkHelper();
					m_ConnectionPoint.Advise(helper, out var pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_DesignTimeOutputDirtyDelegate = value;
					m_aEventSinkHelpers.Add(helper);
				}
			}
			remove
			{
				lock (this)
				{
					int count = m_aEventSinkHelpers.Count;
					int index = 0;
					if (0 < count)
					{
						do
						{
							_dispBuildManagerEvents_SinkHelper helper = (_dispBuildManagerEvents_SinkHelper)m_aEventSinkHelpers[index];
							if ((helper.m_DesignTimeOutputDirtyDelegate != null) && (helper.m_DesignTimeOutputDirtyDelegate.Equals(value)))
							{
								m_aEventSinkHelpers.RemoveAt(index);
								m_ConnectionPoint.Unadvise(helper.m_dwCookie);
								if (count <= 1)
								{
									m_ConnectionPoint = null;
									m_aEventSinkHelpers = null;
								}
								break;
							}
							index++;
						}
						while (index < count);
					}
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~_dispBuildManagerEvents_EventProvider()
		{
			Dispose(false);
		}

		void Dispose(bool disposing)
		{
			Monitor.Enter(this);
			try
			{
				if (m_ConnectionPoint != null)
				{
					int count = m_aEventSinkHelpers.Count;
					int num2 = 0;
					if (0 < count)
					{
						do
						{
							_dispBuildManagerEvents_SinkHelper helper = (_dispBuildManagerEvents_SinkHelper)m_aEventSinkHelpers[num2];
							m_ConnectionPoint.Unadvise(helper.m_dwCookie);
							num2++;
						}
						while (num2 < count);
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
			}
			finally
			{
				Monitor.Exit(this);
			}
		}

		void Init()
		{
			byte[] b = new byte[] { 0xf7, 20, 0x89, 130, 0x81, 0x1d, 0x5c, 0x4f, 0x83, 0xce, 0x37, 0x81, 0x9d, 0x7e, 0xe7, 0x59 };
			Guid riid = new Guid(b);
			m_ConnectionPointContainer.FindConnectionPoint(ref riid, out var ppCP);
			m_ConnectionPoint = ppCP;
			m_aEventSinkHelpers = new ArrayList();
		}
	}
}

