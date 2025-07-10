using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business.Public.Interfaces;
using Enterprise.ServiceManager.Shared.Interfaces;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Shared.CW
{
	// singleton utility class, created using service locator... factory in it used to load up all the service hosts/tasks
	public class ServiceTaskHostsControllerNudgeScheduleFactoryExtension : IFactoryProcessingExtension
	{
		readonly INudgingController controller;
		readonly IBusinessObjectToServiceTaskMapper serviceTaskMapper;

		public ServiceTaskHostsControllerNudgeScheduleFactoryExtension() : this(ObjectFactory.Get<INudgingController>(), BusinessObjectToServiceTaskMapper.Instance)
		{
		}

		internal ServiceTaskHostsControllerNudgeScheduleFactoryExtension(INudgingController controller, IBusinessObjectToServiceTaskMapper serviceTaskMapper)
		{
			this.controller = controller;
			this.serviceTaskMapper = serviceTaskMapper;
		}

		public void OnFactoryBusinessObjectsSaved(BusinessObjectFactory factory, List<BusinessObject> businessObjects)
		{
			if (Db.IsUpgradeWorkingInProgress ||
				!SharedRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled)
			{
				return;
			}

			try
			{
				controller.ReportNudgeStarted(taskCodes: null, stackTrace: null);

				var taskToBizOMapping = serviceTaskMapper.MapBusinessObjectsToServiceTasks(controller, businessObjects);
				if (taskToBizOMapping.Any())
				{
					var connection = ((IDbConnected)factory).Connection;
					if (connection.AppTransactionCount == 0)
					{
						controller.ScheduleTasks(taskToBizOMapping.Keys);
					}
					else
					{
						new DeferredNudge(controller, connection, taskToBizOMapping.Keys);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				controller.ReportNudgeFailed(null, ex, 0);
			}
		}

		class DeferredNudge
		{
			readonly INudgingController controller;
			readonly IEnumerable<string> taskCodes;

			public DeferredNudge(INudgingController controller, DbConnection connection, IEnumerable<string> codes)
			{
				this.controller = controller;
				this.taskCodes = codes;

				connection.AppTransactionCountReset += Connection_AppTransactionCountReset;
			}

			void Connection_AppTransactionCountReset(object sender, DbConnection.AppTransactionCountResetEventArgs e)
			{
				var connection = (DbConnection)sender;
				connection.AppTransactionCountReset -= Connection_AppTransactionCountReset;

				if (e.Committed)
				{
					try
					{
						controller.ReportNudgeDeferred(taskCodes, "The database transaction was committed");
						controller.ScheduleTasks(taskCodes);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						controller.ReportNudgeFailed(null, ex, 0);
					}
				}
				else
				{
					controller.ReportNudgeAbandoned(taskCodes, "The database transaction was rolled back");
				}
			}
		}
	}
}
