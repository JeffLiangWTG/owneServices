using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	internal sealed class CollectionListChangedSuspender : IService
	{
		CollectionListChangedSuspender(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public static CollectionListChangedSuspender GetInstance(BusinessObjectFactory factory)
		{
			CollectionListChangedSuspender result = factory.ServiceContainer.GetService<CollectionListChangedSuspender>()
				?? factory.ServiceContainer.AddService(new CollectionListChangedSuspender(factory));
			return result;
		}

		public void RunDelayableListChanged(ListChangedEventHandler handler, object sender, ListChangedEventArgs e)
		{
			if (currentListChangedDelayer != null)
			{
				currentListChangedDelayer.AddDelayedListChangedEvent(new ListChangedEvent(handler, (IList)sender, e));
			}
			else
			{
				handler(sender, e);
			}
		}

		#region DelayListChangedEvents

		internal int DelayListChangedEventsIndex
		{
			get
			{
				return delayListChangedEventsIndex;
			}
		}

		int delayListChangedEventsIndex;

#if DEBUG
		public bool MakeListNullForTest;
#endif

		public IDisposable DelayListChangedEvents()
		{
			if (delayListChangedEventsIndex++ == 0
#if DEBUG
				&& !MakeListNullForTest
#endif
				)
			{
				currentListChangedDelayer = new ListChangedEventsDelayer(this);
			}
			return new DisposableAction(delegate
			{
				if (delayListChangedEventsIndex - 1 == 0 && currentListChangedDelayer != null)
				{
					currentListChangedDelayer.Dispose();
					currentListChangedDelayer = null;
				}
				delayListChangedEventsIndex--;
			});
		}

		internal sealed class ListChangedEventsDelayer : IDisposable
		{
			internal ListChangedEventsDelayer(CollectionListChangedSuspender owner)
			{
				this.owner = owner;
			}

			internal void AddDelayedListChangedEvent(ListChangedEvent e)
			{
				var changeEvent = GetLastChangeEvent(e.List, e.Handler);
				if (changeEvent == null || changeEvent.Args.ListChangedType != ListChangedType.Reset)
				{
					if (changeEvent == null ||
						(changeEvent.Args.ListChangedType == ListChangedType.ItemDeleted && e.Args.ListChangedType == ListChangedType.ItemDeleted && e.Args.NewIndex <= changeEvent.Args.NewIndex) ||
						(e.Args.ListChangedType == ListChangedType.ItemAdded && e.Args.NewIndex == e.List.Count - 1))
					{
						AddListChangedEvent(new ListChangedEvent(e.Handler, e.List, e.Args));
					}
					else
					{
						RemoveAllEvents(e.List, e.Handler);
						AddListChangedEvent(new ListChangedEvent(e.Handler, e.List, new ListChangedEventArgs(ListChangedType.Reset, -1)));
					}
				}
			}

			public void Dispose()
			{
				FireChangeEvents();
			}

			void FireChangeEvents()
			{
				var processingListChangedEventNode = listChangedEvents.First;

				using (owner.factory.CacheBusinessObjectHasChanges())
				{
					while (processingListChangedEventNode != null)
					{
						var e = processingListChangedEventNode.Value;

						try
						{
							ListChangedDelegateInvoker.Invoke(e.Handler, e.List, e.Args);
						}
						catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is IndexOutOfRangeException)
						{
							// Processed item was probably removed earlier by some previous event
							RemoveAllEvents(e.List, e.Handler);
							ListChangedDelegateInvoker.Invoke(e.Handler, e.List, new ListChangedEventArgs(ListChangedType.Reset, -1));
						}

						if (processingListChangedEventNode.List != null)
						{
							var previousNode = processingListChangedEventNode;
							processingListChangedEventNode = processingListChangedEventNode.Next;

							RemoveEvent(previousNode);
						}
						else
						{
							// Current node was invalidated, and zero or more subsequent nodes removed
							processingListChangedEventNode = listChangedEvents.First;
						}
					}
				}
			}

			#region Implementation

			readonly CollectionListChangedSuspender owner;

			readonly LinkedList<ListChangedEvent> listChangedEvents = new LinkedList<ListChangedEvent>();
			readonly Dictionary<(IList Sender, ListChangedEventHandler Handler), List<LinkedListNode<ListChangedEvent>>> LastChangedEventLookup = new Dictionary<(IList, ListChangedEventHandler), List<LinkedListNode<ListChangedEvent>>>();

			void AddListChangedEvent(ListChangedEvent listChangedEvent)
			{
				var key = (listChangedEvent.List, listChangedEvent.Handler);
				if (!LastChangedEventLookup.TryGetValue(key, out var listChangedEventsForThisKey))
				{
					LastChangedEventLookup[key] = listChangedEventsForThisKey = new List<LinkedListNode<ListChangedEvent>>();
				}

				var node = listChangedEvents.AddLast(listChangedEvent);
				listChangedEventsForThisKey.Add(node);
			}

			ListChangedEvent GetLastChangeEvent(IList sender, ListChangedEventHandler handler)
			{
				return LastChangedEventLookup.TryGetValue((sender, handler), out var lastChangeEvents) ? lastChangeEvents[0].Value : null;
			}

			void RemoveEvent(LinkedListNode<ListChangedEvent> eventNode)
			{
				var value = eventNode.Value;
				var key = (value.List, value.Handler);

				if (LastChangedEventLookup.TryGetValue(key, out var lastChangeEvents))
				{
					lastChangeEvents.Remove(eventNode);

					if (lastChangeEvents.Count == 0)
					{
						LastChangedEventLookup.Remove(key);
					}
				}

				listChangedEvents.Remove(eventNode);
			}

			void RemoveAllEvents(IList sender, ListChangedEventHandler handler)
			{
				var key = (sender, handler);
				if (LastChangedEventLookup.TryGetValue(key, out var lastChangeEvents))
				{
					foreach (var node in lastChangeEvents)
					{
						listChangedEvents.Remove(node);
					}

					LastChangedEventLookup.Remove(key);
				}
			}

			#endregion
		}

		internal class ListChangedEvent
		{
			public ListChangedEvent(ListChangedEventHandler handler, IList sender, ListChangedEventArgs e)
			{
				Handler = handler;
				List = sender;
				Args = e;
			}

			public ListChangedEventHandler Handler { get; private set; }
			public IList List { get; private set; }
			public ListChangedEventArgs Args { get; private set; }
		}

		#endregion

		ListChangedEventsDelayer currentListChangedDelayer;

		internal bool IsDelayerEnabled { get { return currentListChangedDelayer != null; } }
	}
}
