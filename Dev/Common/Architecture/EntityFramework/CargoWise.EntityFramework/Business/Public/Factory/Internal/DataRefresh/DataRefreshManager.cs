using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public partial class DataRefreshManager
	{
		public bool Enabled = !ObjectFactory.Get<IEntityFrameworkSettings>().IsWeb && !ObjectFactory.Get<IEntityFrameworkSettings>().IsWebService;

		public static int ServiceTaskRunningCount
		{
			get => serviceTaskRunningCount;
			private set
			{
				Argument.GreaterThanOrEqualToZero(value, (NoResString)"How the heck did that happen?");
				serviceTaskRunningCount = value;
			}
		}

		public sealed class DisableRefreshForServiceTask : IDisposable
		{
			public DisableRefreshForServiceTask()
			{
				ServiceTaskRunningCount++;
			}

			public void Dispose()
			{
				if (!isDisposed)
				{
					isDisposed = true;
					ServiceTaskRunningCount--;
				}
			}

			bool isDisposed;
		}

		public static DisposableAction BeginRefreshOverrideForServiceTask()
		{
			enableRefreshForServiceTask = true;
			DisposableAction disposableAction = null;
			disposableAction = new DisposableAction(() =>
			{
				enableRefreshForServiceTask = false;
				DisposableLeakListener.Instance.UnRegisterDisposable(disposableAction);
			});
			DisposableLeakListener.Instance.RegisterDisposable(disposableAction);
			return disposableAction;
		}

		public static DisposableAction BeginDisableRefresh()
		{
			disableRefresh = true;
			DisposableAction disposableAction = null;
			disposableAction = new DisposableAction(() =>
			{
				disableRefresh = false;
				DisposableLeakListener.Instance.UnRegisterDisposable(disposableAction);
			});
			DisposableLeakListener.Instance.RegisterDisposable(disposableAction);
			return disposableAction;
		}

		[ThreadStatic]
		static int serviceTaskRunningCount;

		[ThreadStatic] static bool enableRefreshForServiceTask;
		[ThreadStatic] static bool disableRefresh;

		public void StartManaging(BusinessObject bizObject)
		{
			if (Enabled)
			{
				Bus.Subscribe(bizObject);
			}
		}

		public void StartManaging(BusinessObjectCollection collectionObject)
		{
			if (Enabled)
			{
				Bus.Subscribe(collectionObject);
			}
		}

		public void StartManaging(string tableName, IDataRefreshBusSubscriber subscriber)
		{
			if (Enabled)
			{
				Bus.Subscribe(tableName, subscriber);
			}
		}

		public void StopManaging(BusinessObjectCollection collectionObject)
		{
			if (Enabled)
			{
				Bus.UnSubscribe(collectionObject);
			}
		}

		public void StopManaging(string tableName, IDataRefreshBusSubscriber collectionObject)
		{
			if (Enabled)
			{
				Bus.UnSubscribe(tableName, collectionObject);
			}
		}

		public void AcceptChangesOnRowsWithDelayedAcceptChanges(IEnumerable<BusinessObject> bizObjects)
		{
			if (Enabled)
			{
				foreach (var bizObject in bizObjects)
				{
					if (bizObject.Row != null)
					{
						if (((IBusinessObjectInternals)bizObject).AcceptChangesDelayedUntilJustBeforeSavingToDatabase)
						{
							if (bizObject.Row.RowState != DataRowState.Detached)
							{
								bizObject.Row.AcceptChanges();
							}
							((IBusinessObjectInternals)bizObject).AcceptChangesDelayedUntilJustBeforeSavingToDatabase = false;
						}
					}
				}
			}
		}

		internal void ForcePublish(BusinessObject[] bizObjectsToPublishOut)
		{
			Publish(true, bizObjectsToPublishOut);
		}

		internal void Publish(BusinessObject[] bizObjectsToPublishOut)
		{
			Publish(false, bizObjectsToPublishOut);
		}

		void Publish(bool forcePublish, BusinessObject[] bizObjectsToPublishOut)
		{
			StartPublish();
			if ((ServiceTaskRunningCount == 0 || enableRefreshForServiceTask) && (forcePublish || Enabled) && !disableRefresh)
			{
				Bus.FetchForRefresh(bizObjectsToPublishOut);
				foreach (var bizO in bizObjectsToPublishOut)
				{
					Bus.Publish(bizO);
					OnPublish();
				}

				if (bizObjectsToPublishOut.Length > 0)
				{
					Bus.PublishByTableName(bizObjectsToPublishOut);
					OnPublish();
				}
			}
		}
		partial void OnPublish();

		public static int CollectionSubscriptionCount
		{
			get { return Bus.CollectionSubscriptionCount; }
		}

		public static int SubscriptionCount
		{
			get { return Bus.SubscriptionCount; }
		}

		internal bool IsRefreshing(IBusiness businessObjectToCheck)
		{
			return Bus.IsRefreshing(businessObjectToCheck);
		}

#if DEBUG
		internal void SetIsRefreshing(IBusiness businessObjectToSet, bool isBeingRefreshed)
		{
			Bus.SetIsRefreshing(businessObjectToSet, isBeingRefreshed);
		}
#endif

		internal static DataRefreshBus Bus
		{
			get { return fBus; }
		}

		partial void StartPublish();

		public T PerformImmediateOrDeferredUpdate<T>(Func<T> updateDelegate)
		{
			return Bus.PerformImmediateOrDeferredUpdate(updateDelegate);
		}

		public void PerformImmediateOrDeferredUpdate(Action updateDelegate)
		{
			Bus.PerformImmediateOrDeferredUpdate(updateDelegate);
		}

		public static IEnumerable<BusinessObjectFactory> Factories
		{
			get { return fBus.Factories; }
		}

		#region Implementation

		[SuppressThreadStaticFieldMessage]
		static readonly DataRefreshBus fBus = new DataRefreshBus();

		#endregion

	}

	#region Test
#if DEBUG
	public partial class DataRefreshManager
	{
		int publishCount;

		public bool SubscriptionFired
		{
			get { return Bus.SubscriptionFired; }
		}

		public int PublishCount
		{
			get { return publishCount; }
		}

		partial void StartPublish()
		{
			publishCount = 0;
			Bus.StartPublish();
		}

		partial void OnPublish()
		{
			publishCount++;
		}
	}
#endif
	#endregion
}
