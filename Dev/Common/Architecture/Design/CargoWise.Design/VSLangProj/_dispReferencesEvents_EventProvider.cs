namespace VSLangProj
{
	using System;
	using System.Collections;
	using System.Runtime.InteropServices.ComTypes;
	using System.Threading;
	using CargoWise.Common;

	internal sealed class _dispReferencesEvents_EventProvider : _dispReferencesEvents_Event, IDisposable
	{
		ArrayList m_aEventSinkHelpers;
		IConnectionPoint m_ConnectionPoint;
		readonly IConnectionPointContainer m_ConnectionPointContainer;

		public _dispReferencesEvents_EventProvider(object a_1)
		{
			m_ConnectionPointContainer = (IConnectionPointContainer)a_1;
		}

		public event _dispReferencesEvents_ReferenceAddedEventHandler ReferenceAdded
		{
			add
			{
				lock (this)
				{
					if (m_ConnectionPoint == null)
					{
						Init();
					}
					_dispReferencesEvents_SinkHelper helper = new _dispReferencesEvents_SinkHelper();
					m_ConnectionPoint.Advise(helper, out var pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_ReferenceAddedDelegate = value;
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
							_dispReferencesEvents_SinkHelper helper = (_dispReferencesEvents_SinkHelper)m_aEventSinkHelpers[index];
							if ((helper.m_ReferenceAddedDelegate != null) && (helper.m_ReferenceAddedDelegate.Equals(value)))
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

		public event _dispReferencesEvents_ReferenceChangedEventHandler ReferenceChanged
		{
			add
			{
				lock (this)
				{
					if (m_ConnectionPoint == null)
					{
						Init();
					}
					_dispReferencesEvents_SinkHelper helper = new _dispReferencesEvents_SinkHelper();
					m_ConnectionPoint.Advise(helper, out var pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_ReferenceChangedDelegate = value;
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
							_dispReferencesEvents_SinkHelper helper = (_dispReferencesEvents_SinkHelper)m_aEventSinkHelpers[index];
							if ((helper.m_ReferenceChangedDelegate != null) && (helper.m_ReferenceChangedDelegate.Equals(value)))
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

		public event _dispReferencesEvents_ReferenceRemovedEventHandler ReferenceRemoved
		{
			add
			{
				lock (this)
				{
					if (m_ConnectionPoint == null)
					{
						Init();
					}
					_dispReferencesEvents_SinkHelper helper = new _dispReferencesEvents_SinkHelper();
					m_ConnectionPoint.Advise(helper, out var pdwCookie);
					helper.m_dwCookie = pdwCookie;
					helper.m_ReferenceRemovedDelegate = value;
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
							_dispReferencesEvents_SinkHelper helper = (_dispReferencesEvents_SinkHelper)m_aEventSinkHelpers[index];
							if ((helper.m_ReferenceRemovedDelegate != null) && (helper.m_ReferenceRemovedDelegate.Equals(value)))
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

		~_dispReferencesEvents_EventProvider()
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
							_dispReferencesEvents_SinkHelper helper = (_dispReferencesEvents_SinkHelper)m_aEventSinkHelpers[num2];
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
			byte[] b = new byte[] { 0x7c, 0xb2, 0x7e, 40, 0x8b, 15, 0x2d, 0x4d, 0x8e, 130, 0xa9, 0xca, 80, 0xb6, 0x76, 110 };
			Guid riid = new Guid(b);
			m_ConnectionPointContainer.FindConnectionPoint(ref riid, out var ppCP);
			m_ConnectionPoint = ppCP;
			m_aEventSinkHelpers = new ArrayList();
		}
	}
}

