using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Information bus for in-memory data synchronisation in the business layer
	/// </summary>
	internal partial class DataRefreshBus
	{
		public void Subscribe(BusinessObject participant)
		{
			SubscriptionSubject subject = new ZGuidSubscriptionSubject(participant.PK);
			Subscription newSubscriber = new BusinessObjectSubscription(participant);
			Subscribe(subject, newSubscriber);
		}

		public void Subscribe(string tableName, IDataRefreshBusSubscriber participant)
		{
			SubscriptionSubject subject = new SubscriptionSubject(tableName, string.Empty);
			Subscription subscription = new DataRefreshBusSubscription(participant.Factory, participant);
			Subscribe(subject, subscription);
		}

		public void Subscribe(BusinessObjectCollection participant)
		{
			var tableName = ((INeedTable)participant).Table.TableName;
			SubscriptionSubject subject = new SubscriptionSubject(tableName, participant.GetType().FullName);
			Subscription subscription = new BusinessObjectCollectionSubscription(participant);
			Subscribe(subject, subscription);
		}

		public void UnSubscribe(BusinessObject participant)
		{
			UnSubscribe(new[] { participant });
		}

		public void UnSubscribe(IEnumerable<BusinessObject> participants)
		{
			foreach (BusinessObject participant in participants)
			{
				SubscriptionSubject subject = new ZGuidSubscriptionSubject(participant.PK);
				UnSubscribe(subject, participant);
			}
		}

		public void UnSubscribe(BusinessObjectCollection participant)
		{
			SubscriptionSubject subject = new SubscriptionSubject(((INeedTable)participant).Table.TableName, participant.GetType().FullName);
			UnSubscribe(subject, participant);
		}

		public void UnSubscribe(string tableName, IDataRefreshBusSubscriber participant)
		{
			SubscriptionSubject subject = new SubscriptionSubject(tableName, string.Empty);
			UnSubscribe(subject, participant);
		}

		public void FetchForRefresh(IEnumerable<BusinessObject> participants)
		{
			foreach (var group in participants.GroupBy(participant => participant.Table))
			{
				var first = group.First();
				var factory = first.Factory;
				SubscriptionSubject subject = new SubscriptionSubject(group.Key.TableName, string.Empty);
				foreach (Subscription subscription in Subscriptions.GetSubscriptions(factory, subject, null).ToList())
				{
					subscription.FetchForRefresh(group);
				}
			}
		}

		public void Publish(BusinessObject participant)
		{
			SubscriptionSubject subject = new ZGuidSubscriptionSubject(participant.PK);
			Publish(subject, new[] { participant }, participant.Factory, potentialMatch => participant != potentialMatch.Participant);
		}

#if DEBUG
		// do not call this in production - will create array..
		public void PublishByTableName(params BusinessObject[] participants)
		{
			PublishByTableName((IEnumerable<BusinessObject>)participants);
		}
#endif

		List<INotifiableRefreshSubscriber> GetSubscriptionPartipantsRequiringNotification()
		{
			lock (this)
			{
				return Subscriptions.ToList().Select(s => s.Participant).OfType<INotifiableRefreshSubscriber>().Distinct().ToList();
			}
		}

		public void PublishByTableName(IEnumerable<BusinessObject> participants)
		{
			var tableGroups = participants.GroupBy(participant => participant.Table).ToArray();
			var subscriptionPartipantsRequiringNotification = tableGroups.Length > 0 ? GetSubscriptionPartipantsRequiringNotification() : null;

			if (subscriptionPartipantsRequiringNotification != null && subscriptionPartipantsRequiringNotification.Any())
			{
				foreach (var notifiable in subscriptionPartipantsRequiringNotification)
				{
					notifiable.NotifyRefreshByTableStarting();
				}
			}

			foreach (var tableGroup in tableGroups)
			{
				SubscriptionSubject collectionSubject = new SubscriptionSubject(tableGroup.Key.TableName, string.Empty);

				foreach (var typeGroup in tableGroup.GroupBy(participant => participant.GetType()))
				{
					Publish(collectionSubject, typeGroup, typeGroup.First().Factory,
						potentialMatch =>
						{
							var collectionParticipant = potentialMatch.Participant as BusinessObjectCollection;
							return collectionParticipant == null || collectionParticipant.TypeOfElements == null ||
								collectionParticipant.TypeOfElements.IsAssignableFrom(typeGroup.Key) ||
								typeGroup.Key.IsAssignableFrom(collectionParticipant.TypeOfElements);
						});
				}
			}

			if (subscriptionPartipantsRequiringNotification != null && subscriptionPartipantsRequiringNotification.Any())
			{
				foreach (var notifiable in subscriptionPartipantsRequiringNotification)
				{
					notifiable.NotifyRefreshByTableCompleted();
				}
			}
		}

		public int SubscriptionCount
		{
			get
			{
				lock (this)
				{
					return Subscriptions.Count;
				}
			}
		}

		public int CollectionSubscriptionCount
		{
			get
			{
				lock (this)
				{
					return Subscriptions.OfType<BusinessObjectCollectionSubscription>().Count();
				}
			}
		}

		public bool IsRefreshing(object participantToCheck)
		{
			return ItemsBeingRefreshed.ContainsKey(participantToCheck);
		}

		public void SetIsRefreshing(object participantToCheck, bool isBeingRefreshed)
		{
			if (isBeingRefreshed && !IsRefreshing(participantToCheck))
			{
				ItemsBeingRefreshed.Add(participantToCheck, true);
			}
			else if (!isBeingRefreshed && IsRefreshing(participantToCheck))
			{
				ItemsBeingRefreshed.Remove(participantToCheck);
			}
		}

		internal IEnumerable<BusinessObjectFactory> Factories
		{
			get { return Subscriptions.Factories; }
		}

		#region Implementation

		internal readonly SubscriptionCollection Subscriptions = new SubscriptionCollection();
		readonly Dictionary<object, bool> ItemsBeingRefreshed = new Dictionary<object, bool>();

		void Subscribe(SubscriptionSubject subscriptionSubject, Subscription subscription)
		{
			lock (this) //henry: locking this critical section is required as BusinessObjectFactory's finalizer may be called on another thread that unsubscribes things while we are subscribing.
			{
				if (!Subscriptions.ContainsParticipant(subscriptionSubject, subscription.Participant, subscription.Factory))
				{
					Subscriptions.Add(subscriptionSubject, subscription);
				}
			}
		}

		void UnSubscribe(SubscriptionSubject subscriptionSubject, params object[] participants)
		{
			lock (this)
			{
				var subscribersToRemove = Subscriptions.GetSubscriptionsByParticipants(subscriptionSubject, participants).ToArray();
				Subscriptions.Remove(subscriptionSubject, subscribersToRemove);
			}
		}

		void Publish(SubscriptionSubject subscriptionSubject, IEnumerable<BusinessObject> publisher, BusinessObjectFactory factoryToExclude, Func<ISubscription, bool> uberDelegate)
		{
			PerformImmediateOrDeferredUpdate(() =>
			{
				try
				{
					List<Subscription> subscriptionsToRemove = new List<Subscription>();
					var subscriptions = SubscriptionProvider.GetSubscriptions(factoryToExclude, subscriptionSubject, publisher, uberDelegate).ToList();
					subscriptions.Sort(SortByType);
					foreach (Subscription subscription in subscriptions)
					{
						ItemsBeingRefreshed.Clear();
						object participant = subscription.Participant;
						if (participant != null)
						{
							SetIsRefreshing(participant, true);
						}

						try
						{
							subscription.TakeAction(publisher);
						}
						finally
						{
							ItemsBeingRefreshed.Clear();
						}
						if (subscription.MarkedForDelete)
						{
							subscriptionsToRemove.Add(subscription);
						}
					}

					Subscriptions.Remove(subscriptionSubject, subscriptionsToRemove);
				}
				catch (CannotAddToCollectionException)
				{
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ErrorReporter.ReportOnce("Error During DataRefreshBus publish (publisher type = " + publisher.GetType().ToString() + ")", e);
				}
			});
		}

		internal int SortByType(ISubscription x, ISubscription y)
		{
			int result = 0;

			if (y.Participant.GetType().IsSubclassOf(x.Participant.GetType()))
			{
				result = 1;
			}
			else if (x.Participant.GetType().IsSubclassOf(y.Participant.GetType()))
			{
				result = -1;
			}
#if DEBUG
			else if (EnableOverrideGetHashCode_ForTest)
			{
				result = x.Participant.GetHashCode().CompareTo(y.Participant.GetHashCode());
			}
#endif

			return result;
		}

#if DEBUG

		internal bool EnableOverrideGetHashCode_ForTest { set; get; }

#endif

		public T PerformImmediateOrDeferredUpdate<T>(Func<T> updateDelegate)
		{
			lock (this)
			{
				return updateDelegate();
			}
		}

		public void PerformImmediateOrDeferredUpdate(Action updateDelegate)
		{
			lock (this)
			{
				updateDelegate();
			}
		}

		internal ISubscriptionsBySubjectProvider SubscriptionProvider
		{
			get { return subscriptionProvider ?? (subscriptionProvider = new SubscriptionsBySubjectProvider(this)); }
			set { subscriptionProvider = value; }
		}
		ISubscriptionsBySubjectProvider subscriptionProvider;

		#region Inner classes

		class SubscriptionsBySubjectProvider : ISubscriptionsBySubjectProvider
		{
			internal SubscriptionsBySubjectProvider(DataRefreshBus bus)
			{
				this.bus = bus;
			}

			readonly DataRefreshBus bus;

			IEnumerable<ISubscription> ISubscriptionsBySubjectProvider.GetSubscriptions(BusinessObjectFactory factoryToExclude, ISubscriptionSubject subscriptionSubject, IEnumerable<object> publisher, Func<ISubscription, bool> uberDelegate)
			{
				return bus.Subscriptions.GetSubscriptions(factoryToExclude, subscriptionSubject, publisher).Where(x => uberDelegate(x));
			}
		}

		internal class SubscriptionSubject : ISubscriptionSubject
		{
			public SubscriptionSubject(string key, string subKey)
			{
				this.key = key;
				this.subKey = subKey;
			}
			readonly string key;
			readonly string subKey;

			public string Key
			{
				get { return key; }
			}

			public string SubKey
			{
				get { return subKey; }
			}

			public override bool Equals(object obj)
			{
				return obj != null && obj is SubscriptionSubject s2 && s2.Key == Key && s2.SubKey == SubKey;
			}

			public override int GetHashCode()
			{
				return Key.GetHashCode() + SubKey.GetHashCode();
			}
		}

		internal class ZGuidSubscriptionSubject : SubscriptionSubject
		{
			public ZGuidSubscriptionSubject(ZGuid pK)
				: base(pK.ToStringKey(), string.Empty)
			{
			}
		}

		public abstract partial class Subscription : ISubscription
		{
			public Subscription(BusinessObjectFactory factory, object participant)
			{
				this.participant = participant;
				this.Factory = factory;
			}

			public readonly BusinessObjectFactory Factory;
			protected readonly object participant;

			public object Participant
			{
				get { return participant; }
			}

			public bool MarkedForDelete
			{
				get { return fMarkedForDelete; }
			}

			public void FetchForRefresh(IEnumerable<object> publishedObjects)
			{
				FetchForRefreshCore(publishedObjects);
			}

			protected virtual void FetchForRefreshCore(IEnumerable<object> publishedObjects)
			{
			}

			public void TakeAction(IEnumerable<BusinessObject> publishedObjects)
			{
				if (Factory.ThreadSentry.IsOwner)
				{
					if (Participant == null)
					{
						MarkForDelete();
					}
					else
					{
						DoAction(publishedObjects);
					}
				}
			}

			protected void MarkForDelete()
			{
				fMarkedForDelete = true;
			}

			protected void OnSubscriptionUpdate(bool result)
			{
				if (result)
				{
					ValueCacheDomainService.GetIfCreated(Factory)?.ClearAfterFactorySave();
				}

				UpdateSubscriptionStats(result);
			}

			partial void UpdateSubscriptionStats(bool result);

			#region Implementation

			/// <summary>
			/// </summary>
			/// <param name="publishedObject"></param>
			/// <returns>Indication whether a successful action occurred</returns>
			internal abstract void DoAction(IEnumerable<BusinessObject> publishedObject);

			bool fMarkedForDelete;

			public override bool Equals(object obj)
			{
				return obj != null && obj is Subscription s2 && s2.Factory == Factory && s2.participant == participant;
			}

			public override int GetHashCode()
			{
				return Factory.GetHashCode() + participant.GetHashCode();
			}

			#endregion
		}

		internal class ZDataRowSubscription : Subscription
		{
			public ZDataRowSubscription(ZDataRow participant, BusinessObjectFactory sunscriberFactory, BusinessObjectFactory publisherFactory)
				: base(sunscriberFactory, participant)
			{
				this.publisherFactory = publisherFactory;
			}
			readonly BusinessObjectFactory publisherFactory;

			internal override void DoAction(IEnumerable<BusinessObject> publishedObjects)
			{
				var result = false;
				var subscriber = (ZDataRow)Participant;
				foreach (var publisher in publishedObjects)
				{
					if (subscriber != publisher.Row)
					{
						if (subscriber.RowState == DataRowState.Unchanged)
						{
							if (publisher.IsDeleted)
							{
								if (subscriber.RowState != DataRowState.Deleted)
								{
									Delete(subscriber, publisher);
								}
								result = true;
							}
							else
							{
								UpdateForDataRefresh(publisher);
								result = true;
							}
						}
						else if (subscriber.RowState == DataRowState.Deleted && !publisher.IsDeleted)
						{
							subscriber.RejectChanges();
							UpdateForDataRefresh(publisher);
							subscriber.Delete();
							result = true;
						}
					}

					if (subscriber == null || subscriber.RowState == DataRowState.Deleted || publisher.IsDeleted)
					{
						MarkForDelete();
					}
				}
				OnSubscriptionUpdate(result);
			}

			void Delete(object sender, BusinessObject publisher)
			{
				if (sender is ZDataRow subscriber && subscriber.RowState != DataRowState.Detached)
				{
					var subscriberBOs = Factory.GetBizOsForDataRow(subscriber);
					if (subscriberBOs != null && subscriberBOs.Length > 0)
					{
						subscriberBOs[0].DeleteForDataRefreshThreadSafe(publisher);
					}
					else
					{
						subscriber.Delete();
						subscriber.AcceptChanges();
					}
				}
			}

			internal void UpdateForDataRefresh(BusinessObject source)
			{
				UpdateForDataRefresh(source.Row, (ZDataRow)Participant);
			}

			internal static void UpdateForDataRefresh(DataRow source, DataRow target)
			{
				var pkName = ZDataUtils.GetPKNameFromTable(target.Table);
				if (source.Table.TableName == target.Table.TableName && source[pkName].Equals(target[pkName]))
				{
					foreach (DataColumn column in target.Table.Columns)
					{
						if (column.ColumnName != pkName &&
							source.Table.Columns.Contains(column.ColumnName) &&
							!source[column.ColumnName].Equals(target[column.ColumnName]))
						{
							target[column] = source[column.ColumnName];
						}
					}
					target.AcceptChanges();
				}
			}

			public override bool Equals(object obj)
			{
				return obj != null && obj is ZDataRowSubscription s2 && s2.Factory == Factory && s2.participant == participant && s2.publisherFactory == publisherFactory;
			}

			public override int GetHashCode()
			{
				return Factory.GetHashCode() + participant.GetHashCode() + publisherFactory.GetHashCode();
			}
		}

		internal class BusinessObjectSubscription : Subscription
		{
			public BusinessObjectSubscription(BusinessObject participant)
				: base(participant.Factory, participant)
			{
			}

			internal override void DoAction(IEnumerable<BusinessObject> publishedObjects)
			{
				bool result = false;
				BusinessObject subscriber = (BusinessObject)Participant;
				foreach (BusinessObject publisher in publishedObjects)
				{
					if (subscriber.Row != publisher.Row)
					{
						if (subscriber.Row.RowState == DataRowState.Unchanged || subscriber.Row.RowState == DataRowState.Modified)
						{
							if (publisher.IsDeleted)
							{
								if (CanApplyDataRefresh(subscriber, DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherDeleted, publisher))
								{
									try
									{
										if (!subscriber.IsDeleted)
										{
											subscriber.DeleteForDataRefreshThreadSafe(publisher);
										}
										((IBusinessObjectInternals)subscriber).AcceptChangesDelayedUntilJustBeforeSavingToDatabase = true;
										result = true;
									}
									catch (NotSupportedException)
									{
										// Delete may throw NotSupported
									}
								}
							}
							else if (CanApplyDataRefresh(subscriber, DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, publisher))
							{
								subscriber.UpdateForDataRefresh(publisher);
								result = true;
							}
						}
						else if (subscriber.IsDeleted && !publisher.IsDeleted)
						{
							if (subscriber.Row.RowState == DataRowState.Deleted && CanApplyDataRefresh(subscriber, DataRefreshAction.UpdateDeletedSubscriberWhenPublisherUpdated, publisher)) // Skip detached rows
							{
								using (ActiveBusinessObjectCollection.DelayListChangedEvents(subscriber.Factory))
								{
									subscriber.Row.RejectChanges();
									ZDataRowSubscription.UpdateForDataRefresh(publisher.Row, subscriber.Row);
									subscriber.Row.Delete();
									result = true;
								}
							}
						}
						else if (publisher.IsDeleted && subscriber.IsDeleted)
						{
							if (subscriber.Row.RowState == DataRowState.Deleted && CanApplyDataRefresh(subscriber, DataRefreshAction.UpdateDeletedSubscriberWhenPublisherDeleted, publisher))
							{
								((IBusinessObjectInternals)subscriber).AcceptChangesDelayedUntilJustBeforeSavingToDatabase = true;
							}
						}
					}
					if (subscriber == null || subscriber.IsDeleted || (publisher.IsDeleted && subscriber.CanBeHandledByDataFresh(publisher)))
					{
						MarkForDelete();
					}
				}
				OnSubscriptionUpdate(result);
			}

			// To avoid CA1502 cyclomatic complexity warning.
			bool CanApplyDataRefresh(BusinessObject subscriber, DataRefreshAction category, BusinessObject publisher)
			{
				var canApply = subscriber as ICanApplyDataRefresh;
				return canApply == null || canApply.CanApplyDataRefresh(category, publisher);
			}
		}

		internal partial class BusinessObjectCollectionSubscription : Subscription
		{
			public BusinessObjectCollectionSubscription(BusinessObjectCollection subscriberCollection)
				: base(subscriberCollection.Factory, subscriberCollection)
			{
			}

			internal override void DoAction(IEnumerable<BusinessObject> publishedObjects)
			{
				((BusinessObjectCollection)Participant).AddFromDataRefreshIfMatching(publishedObjects);
			}
		}

		#endregion

		#endregion
	}

	public interface ISubscription
	{
		object Participant { get; }
	}

	public interface ISubscriptionSubject
	{
		string Key { get; }
		string SubKey { get; }
	}

	public interface ISubscriptionsBySubjectProvider
	{
		IEnumerable<ISubscription> GetSubscriptions(BusinessObjectFactory factoryToExclude, ISubscriptionSubject subscriptionSubject, IEnumerable<object> publisher, Func<ISubscription, bool> uberDelegate);
	}

	#region Test
#if DEBUG

	internal partial class DataRefreshBus
	{
		public void StartPublish()
		{
			Subscriptions.StartPublish();
		}

		public bool SubscriptionFired
		{
			get { return Subscriptions.SubscriptionFired; }
		}

		public abstract partial class Subscription
		{
			bool subscriptionFired;

			public virtual void StartPublish()
			{
				subscriptionFired = false;
			}

			partial void UpdateSubscriptionStats(bool result)
			{
				if (result)
				{
					subscriptionFired = true;
				}
			}

			public virtual bool SubscriptionFired
			{
				get { return subscriptionFired; }
			}
		}

		internal partial class BusinessObjectCollectionSubscription
		{
			public override void StartPublish()
			{
				((BusinessObjectCollection)Participant).StartPublish();
			}

			public override bool SubscriptionFired
			{
				get { return ((BusinessObjectCollection)Participant).SubscriptionFired; }
			}
		}
	}

#endif
	#endregion
}
