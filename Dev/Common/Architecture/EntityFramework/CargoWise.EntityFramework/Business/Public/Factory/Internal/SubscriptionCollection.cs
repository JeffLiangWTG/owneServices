using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Accord.Collections;
using CargoWise.Common.Collections;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// A collection of Subscription objects.
	/// The lifecycle of the Subscription objects are tied to the lifecycle of the factory.
	/// When the factory goes out of scope for a particular Subscription object, the subscription also
	/// goes out of scope (and thus is garbage collected at the same time as the factory,
	/// not each individual subscription participant which has WeakReference dereference performance penalty).
	/// </summary>
	partial class SubscriptionCollection : IEnumerable<DataRefreshBus.Subscription>
	{
		//a white lie, but the only thing that happens if it's multithread-hammered is GetHashCode aliases, which it's already allowed to be
		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		public static long _NextInstance = 1;
		public long _Instance;

		public SubscriptionCollection() : base()
		{
			_Instance = _NextInstance++;
		}

		public IEnumerable<DataRefreshBus.Subscription> GetSubscriptions(BusinessObjectFactory currentFactory, ISubscriptionSubject subscriptionSubject, IEnumerable<Object> publishers)
		{
			foreach (BusinessObjectFactory factory in Factories)
			{
				if (factory != currentFactory)
				{
					var set = GetSubscriptionsMatchingKey(factory, subscriptionSubject.Key);
					if (set != null)
					{
						foreach (var subscription in set.GetSubscriptions(subscriptionSubject.SubKey))
						{
							yield return subscription;
						}
					}

					foreach (var publisherSubscription in GetSubscriptionsFromPublishers(factory, publishers))
					{
						yield return publisherSubscription;
					}
				}
			}
		}

		public IEnumerable<DataRefreshBus.Subscription> GetSubscriptionsByParticipants(ISubscriptionSubject subscriptionSubject, params object[] participants)
		{
			foreach (BusinessObjectFactory factory in Factories)
			{
				var set = GetSubscriptionsMatchingKey(factory, subscriptionSubject.Key);
				if (set != null)
				{
					foreach (var subscription in set.GetSubscriptionsByParticipants(subscriptionSubject.SubKey, participants))
					{
						yield return subscription;
					}
				}
			}
		}

		SubscriptionSet GetSubscriptionsMatchingKey(BusinessObjectFactory factory, string key)
		{
			var subscriptions = GetSubscriptions(factory, false);
			return subscriptions != null && subscriptions.TryGetValue(key, out var result) ? result : null;
		}

		IEnumerable<DataRefreshBus.Subscription> GetSubscriptionsFromPublishers(BusinessObjectFactory factory, IEnumerable<Object> publishers)
		{
			if (publishers != null)
			{
				//data rows that match and have no business object for them (e.g. prefetched) are considered subscribers
				foreach (var bizoPublisher in publishers.OfType<BusinessObject>())
				{
					var rowFactory = factory.GetRowFactory(suppressThreadSentryCheck_neverSetThisToTrueUnlessYouKnowWhatYouAreDoing: true); // This is unsafe cross thread access.
					var row = (ZDataRow)rowFactory.GetRow(bizoPublisher.TableName, bizoPublisher.PK);

					if (row != null && row.RowState == System.Data.DataRowState.Unchanged)
					{
						var businessObjectsForRow = factory.GetBizOsForDataRow(row);
						if (businessObjectsForRow == null || businessObjectsForRow.Length < 1)
						{
							yield return new DataRefreshBus.ZDataRowSubscription(row, factory, bizoPublisher.Factory);
						}
					}
				}
			}
		}

		public void Add(DataRefreshBus.SubscriptionSubject subscriptionSubject, DataRefreshBus.Subscription subscription)
		{
#if DEBUG
			if (NUnit.Framework.TestingState.IsRunningTests && Contains(subscriptionSubject, subscription))
			{
				throw new InvalidOperationException("Subscription already exists in this collection");
			}
#endif

			if (!ContainsFactory(subscription.Factory))
			{
				lock (allFactoriesOperationMutex)
				{
					if (!ContainsFactory(subscription.Factory))
					{
						factories.Add(new WeakReference(subscription.Factory));
					}
				}
			}
			Add(subscriptionSubject.Key, subscriptionSubject.SubKey, subscription);
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		void Add(string key, string subKey, DataRefreshBus.Subscription subscription)
		{
			var subscriptions = GetSubscriptions(subscription.Factory, true);
			if (!subscriptions.TryGetValue(key, out var set))
			{
				set = new SubscriptionSet();
				subscriptions.TryAdd(key, set);
				subscriptions.TryGetValue(key, out set);
			}
			set?.Add(subKey, subscription);
		}

		public bool Contains(DataRefreshBus.SubscriptionSubject subscriptionSubject, DataRefreshBus.Subscription subscription)
		{
			var key = subscriptionSubject.Key;
			var subKey = subscriptionSubject.SubKey;
			var factory = subscription.Factory;
			var set = GetSubscriptionsMatchingKey(factory, key);
			if (set != null && set.ContainsSubscription(subKey, subscription))
			{
				return true;
			}
			return false;
		}

		public bool ContainsParticipant(DataRefreshBus.SubscriptionSubject subscriptionSubject, object participant, BusinessObjectFactory factory)
		{
			var key = subscriptionSubject.Key;
			var subKey = subscriptionSubject.SubKey;
			var set = GetSubscriptionsMatchingKey(factory, key);
			if (set != null && set.ContainsParticipant(subKey, participant))
			{
				return true;
			}
			return false;
		}

		public void Remove(DataRefreshBus.SubscriptionSubject subscriptionSubject, IEnumerable<DataRefreshBus.Subscription> subscriptions)
		{
			var key = subscriptionSubject.Key;
			var subKey = subscriptionSubject.SubKey;

			foreach (BusinessObjectFactory factory in Factories)
			{
				var subscriptionsInOneFactory = GetSubscriptions(factory, false);
				Remove(key, subKey, factory, subscriptionsInOneFactory, subscriptions);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		bool Remove(string key, string subKey, BusinessObjectFactory factory, ConcurrentDictionary<string, SubscriptionSet> subscriptionsInOneFactory, IEnumerable<DataRefreshBus.Subscription> subscriptions)
		{
			var result = false;

			if (subscriptionsInOneFactory != null && subscriptions != null
				&& subscriptionsInOneFactory.TryGetValue(key, out var set) && set.Remove(subKey, subscriptions))
			{
				if (set.IsEmpty)
				{
					subscriptionsInOneFactory.TryRemove(key, out _);
					if (subscriptionsInOneFactory.Count == 0)
					{
						RemoveFactory(factory);
					}
				}
				result = true;
			}
			return result;
		}

		public int Count
		{
			get
			{
				int result = 0;
				foreach (BusinessObjectFactory factory in Factories)
				{
					var subscriptions = GetSubscriptions(factory, false);
					if (subscriptions != null)
					{
						result += subscriptions.Count;
					}
				}
				return result;
			}
		}

		internal IEnumerable<BusinessObjectFactory> Factories
		{
			get
			{
				lock (allFactoriesOperationMutex)
				{
					for (int i = factories.Count - 1; i >= 0; i--)
					{
						if (i < factories.Count)
						{
							WeakReference factoryRef = factories[i];
							BusinessObjectFactory factory = null;
							try
							{
								factory = (BusinessObjectFactory)factoryRef.Target;
							}
							catch (InvalidOperationException) { }
							if (factory == null)
							{
								factories.RemoveAt(i);
							}
							else
							{
								yield return factory;
							}
						}
					}
				}
			}
		}

		public override int GetHashCode()
		{
			return _Instance.GetHashCode();
		}

		#region SubscriptionSetService class

		internal class SubscriptionSet
		{
			internal void Add(string subKey, DataRefreshBus.Subscription subscription)
			{
				RedBlackTree<DataRefreshBus.Subscription> subscriptionsList;
				RedBlackTree<object> participantsList;
				if (string.IsNullOrEmpty(subKey))
				{
					subscriptionsList = internalSubscriptionsList ?? (internalSubscriptionsList = GetRedBlackTree<DataRefreshBus.Subscription>());
					participantsList = internalParticipantsList ?? (internalParticipantsList = GetRedBlackTree<object>());
				}
				else
				{
					var subscriptionsDictionary = internalSubscriptionsDictionary ?? (internalSubscriptionsDictionary = new Dictionary<string, RedBlackTree<DataRefreshBus.Subscription>>());
					if (!subscriptionsDictionary.TryGetValue(subKey, out subscriptionsList))
					{
						subscriptionsList = GetRedBlackTree<DataRefreshBus.Subscription>();
						subscriptionsDictionary.Add(subKey, subscriptionsList);
					}

					var participantsDictionary = internalParticipantsDictionary ?? (internalParticipantsDictionary = new Dictionary<string, RedBlackTree<object>>());
					if (!participantsDictionary.TryGetValue(subKey, out participantsList))
					{
						participantsList = GetRedBlackTree<object>();
						participantsDictionary.Add(subKey, participantsList);
					}
				}
				subscriptionsList.Add(subscription);
				participantsList.Add(subscription.Participant);
			}

			RedBlackTree<T> GetRedBlackTree<T>()
			{
				return new RedBlackTree<T>(new ZComparer<T>((x, y) => RuntimeHelpers.GetHashCode(x).CompareTo(RuntimeHelpers.GetHashCode(y))), true);
			}

			internal bool Remove(string subKey, IEnumerable<DataRefreshBus.Subscription> subscriptionsArg)
			{
				var subscriptions = subscriptionsArg.ToArray();

				bool result = false;
				foreach (var subscription in subscriptions)
				{
					result |= internalSubscriptionsList != null && internalSubscriptionsList.Remove(subscription) != null;
					result |= internalParticipantsList != null && internalParticipantsList.Remove(subscription.Participant) != null;
				}

				if (internalSubscriptionsDictionary != null)
				{
					if (string.IsNullOrEmpty(subKey))
					{
						foreach (var tree in internalSubscriptionsDictionary.Values)
						{
							foreach (var subscription in subscriptions)
							{
								result |= tree.Remove(subscription) != null;
							}
						}
					}
					else
					{
						if (internalSubscriptionsDictionary.TryGetValue(subKey, out var tree))
						{
							foreach (var subscription in subscriptions)
							{
								result |= tree.Remove(subscription) != null;
							}
						}
					}
				}

				if (internalParticipantsDictionary != null)
				{
					if (string.IsNullOrEmpty(subKey))
					{
						foreach (var tree in internalParticipantsDictionary.Values)
						{
							foreach (var subscription in subscriptions)
							{
								result |= tree.Remove(subscription.Participant) != null;
							}
						}
					}
					else
					{
						if (internalParticipantsDictionary.TryGetValue(subKey, out var tree))
						{
							foreach (var subscription in subscriptions)
							{
								result |= tree.Remove(subscription.Participant) != null;
							}
						}
					}
				}

				return result;
			}

			internal bool ContainsSubscription(string subKey, DataRefreshBus.Subscription subscription)
				=> ContainsItem(internalSubscriptionsList, internalSubscriptionsDictionary, subKey, subscription);

			internal bool ContainsParticipant(string subKey, object participant)
				=> ContainsItem(internalParticipantsList, internalParticipantsDictionary, subKey, participant);

			bool ContainsItem<T>(RedBlackTree<T> itemsList, Dictionary<string, RedBlackTree<T>> itemsDictionary, string subKey, T item)
			{
				var result = itemsList != null && Contains(itemsList, item);
				if (!result && itemsDictionary != null)
				{
					if (string.IsNullOrEmpty(subKey))
					{
						result = itemsDictionary.Values.Any(x => Contains(x, item));
					}
					else
					{
						result = itemsDictionary.TryGetValue(subKey, out var tree) && Contains(tree, item);
					}
				}
				return result;
			}

			internal IEnumerable<DataRefreshBus.Subscription> GetSubscriptionsByParticipants(string subKey, object[] participants)
			{
				if (participants != null && participants.Length > 0)
				{
					if (!string.IsNullOrEmpty(subKey))
					{
						if (internalSubscriptionsDictionary != null && internalSubscriptionsDictionary.TryGetValue(subKey, out var list))
						{
							foreach (var subscription in list.Where<DataRefreshBus.Subscription>(x => participants.Contains(x.Participant)))
							{
								yield return subscription;
							}
						}
					}
					else
					{
						if (internalSubscriptionsList != null)
						{
							foreach (var subscription in internalSubscriptionsList.Where<DataRefreshBus.Subscription>(x => participants.Contains(x.Participant)))
							{
								yield return subscription;
							}
						}
						if (internalSubscriptionsDictionary != null)
						{
							foreach (var list in internalSubscriptionsDictionary.Values)
							{
								foreach (var subscription in list.Where<DataRefreshBus.Subscription>(x => participants.Contains(x.Participant)))
								{
									yield return subscription;
								}
							}
						}
					}
				}
			}

			internal IEnumerable<DataRefreshBus.Subscription> GetSubscriptions(string subKey)
			{
				if (!string.IsNullOrEmpty(subKey))
				{
					if (internalSubscriptionsDictionary != null && internalSubscriptionsDictionary.TryGetValue(subKey, out var list))
					{
						foreach (var node in list)
						{
							yield return node.Value;
						}
					}
				}
				else
				{
					if (internalSubscriptionsList != null)
					{
						foreach (var node in internalSubscriptionsList)
						{
							yield return node.Value;
						}
					}
					if (internalSubscriptionsDictionary != null)
					{
						foreach (var list in internalSubscriptionsDictionary.Values)
						{
							foreach (var node in list)
							{
								yield return node.Value;
							}
						}
					}
				}
			}

			bool Contains<T>(RedBlackTree<T> tree, T item)
			{
				var node = tree.Find(item); // RedBlackTree.Find doesn't consider object reference equality.
				if (node != null)
				{
					var queue = new Queue<RedBlackTreeNode<T>>();
					queue.Enqueue(node);
					while ((node = queue.Dequeue()) != null)
					{
						if (object.ReferenceEquals(node.Value, item))
						{
							return true;
						}
						queue.Enqueue(node.Left);
						queue.Enqueue(node.Right);
					}
				}
				return false;
			}

			internal bool IsEmpty
			{
				get { return (internalSubscriptionsList == null || internalSubscriptionsList.Count == 0) && (internalSubscriptionsDictionary == null || internalSubscriptionsDictionary.Count == 0); }
			}
			RedBlackTree<DataRefreshBus.Subscription> internalSubscriptionsList;
			Dictionary<string, RedBlackTree<DataRefreshBus.Subscription>> internalSubscriptionsDictionary;

			RedBlackTree<object> internalParticipantsList;
			Dictionary<string, RedBlackTree<object>> internalParticipantsDictionary;
		}

		internal class SubscriptionSetService : IService
		{
			[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
			public ConcurrentDictionary<string, SubscriptionSet> GetSubscriptions(SubscriptionCollection list)
			{
				lists.TryGetValue(list, out var result);
				if (result == null)
				{
#if DEBUG
					if (Thread.CurrentThread.Name == CurrentThreadName)
					{
						AnotherThreadGo = true;
						Thread.Sleep(2000);
					}
#endif
					result = new ConcurrentDictionary<string, SubscriptionSet>();
					lists.TryAdd(list, result);
					lists.TryGetValue(list, out result);
				}
				return result;
			}

#if DEBUG
			internal bool AnotherThreadGo { get; set; }
			internal string CurrentThreadName { get; set; } = "WhatEverAName";
#endif
			readonly ConcurrentDictionary<SubscriptionCollection, ConcurrentDictionary<string, SubscriptionSet>> lists = new ConcurrentDictionary<SubscriptionCollection, ConcurrentDictionary<string, SubscriptionSet>>();
		}

		#endregion

		#region IEnumerable<DataRefreshBus.Subscription> Members

		IEnumerator<DataRefreshBus.Subscription> IEnumerable<DataRefreshBus.Subscription>.GetEnumerator()
		{
			return
				Factories
				.Select(factory => GetSubscriptions(factory, false))
				.Where(subscription => subscription != null)
				.SelectMany(dic => dic.Values)
				.SelectMany(set => set.GetSubscriptions(null)).GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<DataRefreshBus.Subscription>)this).GetEnumerator();
		}

		#endregion

		#region Implementation

		static readonly object allFactoriesOperationMutex = new object();

		readonly List<WeakReference> factories = new List<WeakReference>(2);

		bool ContainsFactory(BusinessObjectFactory factory)
		{
			return Factories.Contains(factory);
		}

		internal void RemoveFactory(BusinessObjectFactory factory)
		{
			lock (allFactoriesOperationMutex)
			{
				for (int i = 0; i < factories.Count; i++)
				{
					try
					{
						if (factories[i].Target == factory)
						{
							factories.RemoveAt(i);
							break;
						}
					}
					catch (InvalidOperationException)
					{
					}
				}
			}
		}

		ConcurrentDictionary<string, SubscriptionSet> GetSubscriptions(BusinessObjectFactory factory, bool create)
		{
			var subscriptions = factory.ServiceContainer.GetService<SubscriptionSetService>();
			if (subscriptions == null && create)
			{
				subscriptions = new SubscriptionSetService();
				factory.ServiceContainer.AddService(subscriptions);
			}
			return subscriptions?.GetSubscriptions(this);
		}

		#endregion
	}

	#region Test
#if DEBUG

	partial class SubscriptionCollection
	{
		public void StartPublish()
		{
			foreach (var subscription in this)
			{
				subscription.StartPublish();
			}
		}

		public bool SubscriptionFired
		{
			get
			{
				foreach (var subscription in this)
				{
					if (subscription.SubscriptionFired)
					{
						return true;
					}
				}

				return false;
			}
		}
	}
#endif
	#endregion
}
