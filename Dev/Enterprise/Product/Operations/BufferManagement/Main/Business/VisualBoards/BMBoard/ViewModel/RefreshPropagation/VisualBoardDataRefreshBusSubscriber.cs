using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Pipes;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class VisualBoardDataRefreshBusSubscriber : Disposable, IDataRefreshBusSubscriber, INotifiableRefreshSubscriber
	{
		public VisualBoardDataRefreshBusSubscriber(IBoardRefreshable refreshable, BoardFactoryProvider factoryProvider, IDispatcher dispatcher)
		{
			this.refreshable = refreshable;
			this.factoryProvider = factoryProvider;
			this.dispatcher = dispatcher;

			Factory = factoryProvider.GetNewBackgroundThreadLoaderFactory(nameof(VisualBoardDataRefreshBusSubscriber) + (NoResString)" Constructor"); // Factory name for debugging

			refreshManager = new DataRefreshManager();

			DisposableLeakListenerWrapper.RegisterDisposable(this);
			StartManaging();
		}

		readonly BoardFactoryProvider factoryProvider;
		readonly DataRefreshManager refreshManager;
		readonly IBoardRefreshable refreshable;
		readonly IDispatcher dispatcher;

		volatile bool refreshEnqueued;
		HashSet<ZGuid> WorkflowPks { get; } = new HashSet<ZGuid>();
		List<ZGuid> TaskPks { get; } = new List<ZGuid>();

		#region Suspend

		public IDisposable Suspend()
		{
			return new DisposableAction(StopManaging, StartManaging);
		}

		void StartManaging()
		{
			refreshManager.StartManaging(ProcessHeaderSchema.Constants.TableName, this);
			refreshManager.StartManaging(ProcessTasksSchema.Constants.TableName, this);
		}

		void StopManaging()
		{
			refreshManager.StopManaging(ProcessHeaderSchema.Constants.TableName, this);
			refreshManager.StopManaging(ProcessTasksSchema.Constants.TableName, this);
		}

		#endregion

		#region IDataRefreshBusSubscriber Members

		public BusinessObjectFactory Factory { get; }

		public void AddExtraWorkflows(IEnumerable<ZGuid> workflowPKs)
		{
			WorkflowPks.UnionWith(workflowPKs);
		}

		void IDataRefreshBusSubscriber.UpdatedByDataRefresh(IEnumerable<object> publishedObjects)
		{
			lock (this)
			{
				TaskPks.AddRange(publishedObjects.OfType<ProcessTask>().Select(t => t.PK));
				WorkflowPks.UnionWith(publishedObjects.OfType<ProcessHeader>().Select(t => t.PK));

				if (!isInRefreshByTable)
				{
					DispatchFireRefresh();
				}
			}
		}

		bool IDataRefreshBusSubscriber.IncludeDeletedObjectsInRefresh => true;

		#endregion

		#region Refresh Implementation

		void DispatchFireRefresh()
		{
			if (!refreshEnqueued && (TaskPks.Count > 0 || WorkflowPks.Count > 0))
			{
				refreshEnqueued = true;
				refreshable.BeforeDataRefresh();
				dispatcher.Dispatch(new Action(FireRefresh));
			}
		}

		void FireRefresh()
		{
			using (PerformanceStatisticsCollector.StartMonitoring(GetType().Name + ".BoardDataRefreshBusMonitor", refreshable.Name))
			{
				ZGuid[] taskPks;
				ZGuid[] workflowPks;

				lock (this)
				{
					taskPks = TaskPks.ToArray();
					TaskPks.Clear();
					workflowPks = WorkflowPks.ToArray();
					WorkflowPks.Clear();
					refreshEnqueued = false;
				}

				var factory = factoryProvider.GetNewBackgroundThreadLoaderFactory(nameof(VisualBoardDataRefreshBusSubscriber));
				var refreshContext = new WorkflowUpdatedOperation(taskPks, workflowPks, factory) { IsSavingDetailedTicket = isSavingDetailedTicket };

				try
				{
					refreshable.RefreshAll(refreshContext);
				}
				catch (BoardMustReloadException)
				{
					refreshable.PerformRefreshAction(new ForcedReloadOperation());
				}
				finally
				{
					isSavingDetailedTicket = false;
				}
			}
		}

		internal static void NotifyIsSavingDetailedTicket()
		{
			isSavingDetailedTicket = true;
		}

		[ThreadStatic]
		static bool isSavingDetailedTicket;

		#endregion

		#region INotifiableRefreshSubscriber Members

		void INotifiableRefreshSubscriber.NotifyRefreshByTableStarting()
		{
			isInRefreshByTable = true;
		}

		void INotifiableRefreshSubscriber.NotifyRefreshByTableCompleted()
		{
			isInRefreshByTable = false;
			DispatchFireRefresh();
		}

		bool isInRefreshByTable;

		#endregion

		#region Disposable Overrides

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopManaging();
				DisposableLeakListenerWrapper.UnRegisterDisposable(this);
			}
		}

		#endregion
	}
}
