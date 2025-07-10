using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.VisualBoards.Business;

namespace Enterprise.VisualBoards.GUI
{
	sealed class SubscribableDispatcher : ControlDispatcher, ISubscribableDispatcher, IDisposable
	{
		internal SubscribableDispatcher(Control control)
			: base(control)
		{
		}

		readonly Dictionary<string, IDisposable> subscriptions = new Dictionary<string, IDisposable>();

		bool ISubscribableDispatcher.TryAdd(string key, Func<IDisposable> makeSubscription)
		{
			if (!subscriptions.ContainsKey(key))
			{
				subscriptions[key] = makeSubscription();
				return true;
			}
			else
			{
				return false;
			}
		}

		#region IDisposable Support

		public void Dispose()
		{
			foreach (var item in subscriptions.Values)
			{
				item.Dispose();
			}
		}

		#endregion
	}
}
