using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Polly;

namespace Enterprise.BufferManagement.Business
{
	/// <summary>
	/// Go through links and call processHeaderDelegate for each workflow that matches the filter,
	/// deactivates links that throws TransferRulesAreInvalidException or has no filter if deactivateLinkIfNoFilter is true
	/// </summary>
	public class LinksProcessorWithDeactivation
	{
		#region LinksProcessorWithDeactivationResult
		public struct LinksProcessorWithDeactivationResult
		{
			public LinksProcessorWithDeactivationResult(bool hasComponentLinksToDeactivate, bool hadConcurrencyException)
			{
				HasComponentLinksToDeactivate = hasComponentLinksToDeactivate;
				HadConcurrencyException = hadConcurrencyException;
			}

			public bool HasComponentLinksToDeactivate { get; }
			public bool HadConcurrencyException { get; }
		}
		#endregion

		public LinksProcessorWithDeactivation(
			IReadOnlyCollection<IComponentLink> linksToProcess,
			ITransferRuleRunnerDataAccessor dataAccessor,
			bool shouldUseSecondaryServerIfAllowed,
			ILogger logger,
			Action<ITransferrableProcessHeader, IComponentLink> processHeaderDelegate,
			Action onBeforeProcessDelegate = null,
			Func<IComponentLink, bool> shouldProcessLinkDelegate = null,
			Action<IComponentLink, IReadOnlyCollection<ITransferrableProcessHeader>> onBatchProcessedDelegate = null,
			Func<CancellationToken, bool> onAllLinksProcessedDelegateReturningIfShouldSave = null)
		{
			this.linksToProcess = linksToProcess;
			this.dataAccessor = dataAccessor;
			this.shouldUseSecondaryServerIfAllowed = shouldUseSecondaryServerIfAllowed;
			this.logger = logger;
			this.processHeaderDelegate = processHeaderDelegate;
			this.onBeforeProcessDelegate = onBeforeProcessDelegate;
			this.shouldProcessLinkDelegate = shouldProcessLinkDelegate;
			this.onBatchProcessedDelegate = onBatchProcessedDelegate;
			this.onAllLinksProcessedDelegateReturningIfShouldSave = onAllLinksProcessedDelegateReturningIfShouldSave;
		}

		#region Fields

		readonly IReadOnlyCollection<IComponentLink> linksToProcess;
		readonly ITransferRuleRunnerDataAccessor dataAccessor;
		readonly bool shouldUseSecondaryServerIfAllowed;
		readonly ILogger logger;
		readonly Action<ITransferrableProcessHeader, IComponentLink> processHeaderDelegate;
		readonly Action onBeforeProcessDelegate;
		readonly Func<IComponentLink, bool> shouldProcessLinkDelegate;
		readonly Action<IComponentLink, IReadOnlyCollection<ITransferrableProcessHeader>> onBatchProcessedDelegate;
		readonly Func<CancellationToken, bool> onAllLinksProcessedDelegateReturningIfShouldSave;

		const int NumberOfReasonableAttempts = 3;
		List<Guid> componentLinksToDeactivate;

		#endregion

		#region Process

		public LinksProcessorWithDeactivationResult Process(CancellationToken token)
		{
			using (dataAccessor.GetTemporaryEnvironmentForServiceTaskBranch())
			{
				return ProcessCore(token);
			}
		}

		LinksProcessorWithDeactivationResult ProcessCore(CancellationToken token)
		{
			if (!linksToProcess.Any())
			{
				return new LinksProcessorWithDeactivationResult(false, false);
			}

			componentLinksToDeactivate = new List<Guid>();
			onBeforeProcessDelegate?.Invoke();

			var hadConcurrencyException = false;

			var tryPolicy = Policy
					.Handle<TransferRulesAreInvalidException>((_) => false)
					.Or<ZConcurrencyCheckFailureException>().Or<SaveConcurrencyException>()
					.Retry(NumberOfReasonableAttempts - 1, (ex, _) => hadConcurrencyException = true);

			foreach (var link in linksToProcess)
			{
				token.ThrowIfCancellationRequested();

				if (link.HasNoFilterWhenRequired)
				{
					MarkLinkForDeactivation(link);
					continue;
				}

				var shouldProcessLink = shouldProcessLinkDelegate?.Invoke(link) ?? true;

				if (!shouldProcessLink)
				{
					continue;
				}

				var processingLinkResult = tryPolicy.ExecuteAndCapture(_ => ProcessLink(link, token), token);
				CheckResult(processingLinkResult.Outcome, processingLinkResult.FinalException, link);

				dataAccessor.Recreate();
			}

			dataAccessor.CreateNewWithoutSave();

			bool shouldSave = false;
			var finalProcessingResult = tryPolicy.ExecuteAndCapture(_ => shouldSave = onAllLinksProcessedDelegateReturningIfShouldSave?.Invoke(token) ?? false, token);
			CheckResult(finalProcessingResult.Outcome, finalProcessingResult.FinalException);

			var hasComponentLinksToDeactivate = componentLinksToDeactivate.Count > 0;

			if (hasComponentLinksToDeactivate)
			{
				DeactivateLinkIfRequiredAndSendNotification();
			}

			if (hasComponentLinksToDeactivate || shouldSave)
			{
				dataAccessor.Save(createNew: false);
			}

			return new LinksProcessorWithDeactivationResult(hasComponentLinksToDeactivate, hadConcurrencyException);
		}

		void CheckResult(OutcomeType outcome, Exception finalException, IComponentLink link = null)
		{
			if (outcome != OutcomeType.Failure)
			{
				return;
			}

			switch (finalException)
			{
				case TransferRulesAreInvalidException ex:
					if (link != null)
					{
						MarkLinkForDeactivation(link);
					}
					else
					{
						ErrorReporter.ReportOnce("We should not get this exception when running a function not associated with a link", ex);
					}
					break;
				case ZConcurrencyCheckFailureException _:
				case SaveConcurrencyException _:
					dataAccessor.LogSaveConcurrencyError(finalException.ToString());
					break;
				default:
					throw new ApplicationException(finalException.Message, finalException);
			}
		}

		void MarkLinkForDeactivation(IComponentLink link) => componentLinksToDeactivate.Add(link.PK);

		void ProcessLink(IComponentLink link, CancellationToken token)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, link.Branch, link.Department))
			using (var workflowBatchLoader = dataAccessor.GetLinkAssociatedWorkflowBatchLoader(link, shouldUseSecondaryServerIfAllowed))
			{
				ITransferrableProcessHeader[] workflowBatch;

				while ((workflowBatch = workflowBatchLoader.LoadNextBatch().ToArray()).Any())
				{
					token.ThrowIfCancellationRequested();

					dataAccessor.AddFetchHintsForLinkAssociatedWorkflowProcessing(workflowBatch, link);

					foreach (var workflow in workflowBatch)
					{
						processHeaderDelegate(workflow, link);
					}
					onBatchProcessedDelegate?.Invoke(link, workflowBatch);
				}
			}
		}

		void DeactivateLinkIfRequiredAndSendNotification()
		{
			var subject = Res.GetString("740B9145-AF77-4050-8805-03F74178BAD7", "Component Links with invalid filters have been deactivated.");
			var messageBody = new ZStringBuilder(Res.GetString("71BB731F-2A96-4DB3-B4E8-2A591B4896BA", "The following component links have been deactivated since they have no filters or invalid filters:"));

			var linksToDeactivate = dataAccessor.LoadComponentLinksToDeactivate(componentLinksToDeactivate);

			foreach (var link in linksToDeactivate)
			{
				dataAccessor.DeactivateComponentLink(link);
				messageBody.Append(link.DisplayText);
			}

			var messageBodyString = messageBody.ToStringWithNewLineBetweenAppends();
			logger.Log(LogType.Warning, messageBodyString);
			using (dataAccessor.GetTemporaryEnvironmentForServiceTaskBranch())
			{
				new BMSEmailDef(subject, messageBodyString).Send();
			}
		}

		#endregion
	}
}
