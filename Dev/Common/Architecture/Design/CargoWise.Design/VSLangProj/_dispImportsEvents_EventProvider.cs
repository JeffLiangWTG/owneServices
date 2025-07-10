namespace VSLangProj
{
	using System;
	using System.Collections;
	using System.Runtime.InteropServices.ComTypes;
	using System.Threading;
	using CargoWise.Common;

	internal sealed class _dispImportsEvents_EventProvider : _dispImportsEvents_Event, IDisposable
	{
		ArrayList m_aEventSinkHelpers;
		IConnectionPoint m_ConnectionPoint;
		readonly IConnectionPointContainer m_ConnectionPointContainer;

		public _dispImportsEvents_EventProvider(object a_1)
		{
			m_ConnectionPointContainer = (IConnectionPointContainer)a_1;
		}

		public event _dispImportsEvents_ImportAddedEventHandler ImportAdded
		{
			add
			{
				lock (this)
				{
					if (m_ConnectionPoint == null)
					{
						Init();
					}
					_dispImportsEvents_SinkHelper helper = new _dispImportsEvents_SinkHelper();
					m_ConnectionPoint.Advise(helper, out var pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_ImportAddedDelegate = value;
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
							_dispImportsEvents_SinkHelper helper = (_dispImportsEvents_SinkHelper)m_aEventSinkHelpers[index];
							if ((helper.m_ImportAddedDelegate != null) && (helper.m_ImportAddedDelegate.Equals(value)))
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

		public event _dispImportsEvents_ImportRemovedEventHandler ImportRemoved
		{
			add
			{
				lock (this)
				{
					if (m_ConnectionPoint == null)
					{
						Init();
					}
					_dispImportsEvents_SinkHelper helper = new _dispImportsEvents_SinkHelper();
					m_ConnectionPoint.Advise(helper, out var pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_ImportRemovedDelegate = value;
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
							_dispImportsEvents_SinkHelper helper = (_dispImportsEvents_SinkHelper)m_aEventSinkHelpers[index];
							if ((helper.m_ImportRemovedDelegate != null) && (helper.m_ImportRemovedDelegate.Equals(value)))
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

		~_dispImportsEvents_EventProvider()
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
							_dispImportsEvents_SinkHelper helper = (_dispImportsEvents_SinkHelper)m_aEventSinkHelpers[num2];
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
			byte[] b = new byte[] { 0xea, 0x6c, 0x80, 0x40, 0xab, 0xab, 0x87, 0x48, 0xa3, 0x56, 0xd8, 0x86, 0x9c, 40, 0xa6, 230 };
			Guid riid = new Guid(b);
			m_ConnectionPointContainer.FindConnectionPoint(ref riid, out var ppCP);
			m_ConnectionPoint = ppCP;
			m_aEventSinkHelpers = new ArrayList();
		}
	}
}

