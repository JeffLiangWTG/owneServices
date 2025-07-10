using System;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	public class AddDocumentToEDocsTriggerActionRunnerFactory : IAddDocumentToEDocsTriggerActionRunnerFactory
	{
		public IProcessor GetNewRunner(IProcessTaskNotification notification, IBusiness job) => new AddDocumentToEDocsTriggerActionRunner((ProcessTaskNotification)notification, (BusinessObject)job);
	}

	sealed class AddDocumentToEDocsTriggerActionRunner : IProcessor
	{
		public AddDocumentToEDocsTriggerActionRunner(ProcessTaskNotification action, BusinessObject job)
		{
			this.action = action;
			this.job = job;
		}
		readonly ProcessTaskNotification action;
		readonly BusinessObject job;

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var documentSupportable = job as IDocumentSupportable;
			if (documentSupportable != null)
			{
				if (action.Document != null)
				{
					var deliveryJob = new AutoDocumentDeliveryJob(businessObject: documentSupportable, documentCommandPK: action.PQ_SU_Document, onlySendToDocManager: true, isFactoryPopulateButDoNotSave: true);
					var documentDataState = documentSupportable.DocumentSupporter.GetDataStateBeforeRun(action.Document);
					if (!documentDataState.IsValid)
					{
						throw new WorkflowValidationException(documentDataState.ErrorMessage);
					}
					deliveryJob.Deliver(notifications ?? new NotificationBuffer());
				}
				else
				{
					notifications?.AddWarning(FormattableString.Invariant($"{WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs} action does not contain a document"));
				}
			}
			else
			{
				notifications?.AddWarning(FormattableString.Invariant($"{job} does not support document"));
			}
		}
	}
}
