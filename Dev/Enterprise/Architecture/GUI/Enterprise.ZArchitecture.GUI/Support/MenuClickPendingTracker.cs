using System;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Tracks Menu Click events while they are being processed.
	/// Construct an instance at the start of the event and Dispose it at the end.
	/// 
	/// Used when showing a modeless form from a popup menu.
	/// A delay is inserted when executing as a Remote App to workaround a Microsoft bug.
	/// </summary>
	public class MenuClickPendingTracker : IDisposable
	{
		public MenuClickPendingTracker()
		{
			++count;
#if DEBUG
			++instanceCount;
#endif
		}

		public void Dispose()
		{
			--count;
		}

		/// <summary>
		/// Count of Click events being processed on the current thread.
		/// </summary>
		static public int Count
		{
			get { return count; }
		}

		[ThreadStatic]
		static int count;

#if DEBUG
		static public int InstanceCount
		{
			get { return instanceCount; }
			set { instanceCount = value; }
		}

		[ThreadStatic]
		static int instanceCount;
#endif
	}
}
