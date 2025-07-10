using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.WorkflowTemplates
{
	public class OrderWorkflowTemplateInterceptor : BaseInterceptor
	{
		public OrderWorkflowTemplateInterceptor(OrderWorkflowTemplateSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{
			factory = setting.Context.ObjectFactory;
		}
		readonly BusinessObjectFactory factory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public override void Invoke(IEntitySet entitySet)
		{
			var order = entitySet.Root;

			var startTime = DateTime.UtcNow;

			Function(entitySet);

			var internalPK = order.InternalPK;
			CreateTaskForOrder(internalPK);
			TriggerTaskForOrder(internalPK, startTime);
		}

		public void CreateTaskForOrder(Guid pk)
		{
			if (pk == Guid.Empty)
			{
				return;
			}

			var order = factory.Load("JD", pk);
			if (order is IWorkflowProvider)
			{
				var workflowProvider = (IWorkflowProvider)order;
				workflowProvider.CreateProcessTaskFromTemplate(factory);
			}
			factory.Save();
		}

		internal void TriggerTaskForOrder(Guid id, DateTime startTime)
		{
			if (id == Guid.Empty)
			{
				return;
			}

			var logs = FindEventLogs(id, startTime);
			foreach (var log in logs)
			{
				((IExternalWorkflowTrigger)log).FireWorkflow();
			}
			factory.Save();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		internal IEnumerable<StmALog> FindEventLogs(Guid id, DateTime startTime)
		{
			var order = factory.Load("JD", id);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystemCode);
			query.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, startTime);
			query.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, DateTime.UtcNow);
			return order.GetLogs().Find(query);
		}
	}
}
