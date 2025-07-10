using System;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	public class SendDocumentsTriggerActionRunnerFactory : ISendDocumentsTriggerActionRunnerFactory
	{
		public IProcessor GetNewRunner(IProcessTaskNotification notification, IBusiness business, Lazy<IStmALog> logProvider) => new SendDocumentsTriggerActionRunner((ProcessTaskNotification)notification, (BusinessObject)business, logProvider);
	}

	sealed class SendDocumentsTriggerActionRunner : IProcessor
	{
		public SendDocumentsTriggerActionRunner(ProcessTaskNotification action, BusinessObject job, Lazy<IStmALog> logProvider)
		{
			this.action = action;
			this.job = job;
			this.logProvider = logProvider;
		}
		readonly ProcessTaskNotification action;
		readonly BusinessObject job;
		readonly Lazy<IStmALog> logProvider;

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			if (!action.Document.MatchesFilter(action.Lookups.DocumentToSendList.CompleteFilter))
			{
				throw new WorkflowValidationException(Res.GetString("7d0757da-fd2b-4aaf-8d65-3cd942e0cab5", "Cannot send document [{0}] as it is not a valid document for the trigger notification.", action.Document.DocumentId));
			}

			if (job is IDocumentSupportable documentSupportable)
			{
				AutoDocumentDeliveryJob deliveryJob;

				if (action.IsEmailRelatedTriggerParty())
				{
					var emailAddresses = action.GetSubstitutedEmailAddressesWithFallback(job, logProvider?.Value as StmALog);
					var validEmailAddresses = emailAddresses.Where(e => EmailAddressValidation.IsEmailAddressValidAndNotEmpty(e)).ToArray();

					if (!validEmailAddresses.Any())
					{
						throw new WorkflowValidationException(action.EmailAddressInvalidOrEmptyErrorMessageForSendingDoc(job));
					}

					deliveryJob = new EmailDocumentDeliveryJob(documentSupportable, action.PQ_SU_Document, true, validEmailAddresses);
				}
				else
				{
					deliveryJob = new AutoDocumentDeliveryJob(documentSupportable, true, action.PQ_SU_Document, action.PQ_SQ);
				}

				using (documentSupportable.DocumentSupporter.InitialiseFetchStrategy())
				{
					var documentDataState = documentSupportable.DocumentSupporter.GetDataStateBeforeRun(action.Document);
					if (!documentDataState.IsValid)
					{
						throw new WorkflowValidationException(documentDataState.ErrorMessage);
					}
					deliveryJob.Deliver(notifications ?? new NotificationBuffer());
				}
			}
		}
	}
}
