using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Integration;

namespace CargoWise.EntityFramework
{
	class ListenerManager
	{
		ConcurrentBag<WeakReference<ITransactionParticipantListener>> listeners;

		public ListenerManager()
		{
			listeners = new ConcurrentBag<WeakReference<ITransactionParticipantListener>>();
		}

		public void Register(ITransactionParticipantListener listener)
		{
			listeners.Add(new WeakReference<ITransactionParticipantListener>(listener));
		}

		public void UnRegister(ITransactionParticipantListener listenerToRemove)
		{
			ConcurrentBag<WeakReference<ITransactionParticipantListener>> originalListeners;
			ConcurrentBag<WeakReference<ITransactionParticipantListener>> newBag;

			do
			{
				originalListeners = listeners;
				newBag = new ConcurrentBag<WeakReference<ITransactionParticipantListener>>();

				foreach (var listenerRef in listeners)
				{
					if (listenerRef.TryGetTarget(out var listener))
					{
						if (listener != listenerToRemove)
						{
							newBag.Add(listenerRef);
						}
					}
				}
			} while (Interlocked.CompareExchange(ref listeners, newBag, listeners) != originalListeners);
		}

		public void NotifyFactorySaveBeginning(ITransactionParticipant[] factories)
		{
			var newBag = new ConcurrentBag<WeakReference<ITransactionParticipantListener>>();
			foreach (var listenerRef in listeners)
			{
				if (listenerRef.TryGetTarget(out var listener))
				{
					newBag.Add(listenerRef);
				}
			}

			// Don't need to loop here because if we lost the race it's to some other kind thread who's done the clean-up
			Interlocked.CompareExchange(ref listeners, newBag, listeners);

			foreach (var listener in GetListeners())
			{
				listener.FactorySaveBeginning(factories);
			}
		}

		public void NotifyFactorySaveCompleted(ITransactionParticipant[] factories, bool isSuccessful)
		{
			foreach (var listener in GetListeners())
			{
				listener.FactorySaveCompleted(factories, isSuccessful);
			}
		}

		IEnumerable<ITransactionParticipantListener> GetListeners()
		{
			foreach (var listenerRef in listeners)
			{
				if (listenerRef.TryGetTarget(out var listener))
				{
					yield return listener;
				}
			}
		}
	}
}
