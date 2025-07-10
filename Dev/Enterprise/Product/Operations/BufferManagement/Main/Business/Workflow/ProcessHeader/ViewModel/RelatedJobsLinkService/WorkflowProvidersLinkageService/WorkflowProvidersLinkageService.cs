using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowProvidersLinkageService : IWorkflowProvidersLinkageService
	{
		#region IWorkflowProvidersLinkageService Members

		void IWorkflowProvidersLinkageService.WorkflowProvidersLinked(IWorkflowProviderCore sourceWorkflowProvider, IWorkflowProviderCore connectedWorkflowProvider, BusinessObjectFactory factory)
		{
			Argument.NotNull(sourceWorkflowProvider, nameof(sourceWorkflowProvider));
			Argument.NotNull(connectedWorkflowProvider, nameof(connectedWorkflowProvider));
			MaybePerformLinkAction(sourceWorkflowProvider, connectedWorkflowProvider, factory, ProposeLinkCreation, (from, to) => { });
		}

		void IWorkflowProvidersLinkageService.WorkflowLinkedTemplatesApplied(IWorkflowProviderCore sourceWorkflowProvider, IWorkflowProviderCore connectedWorkflowProvider, BusinessObjectFactory factory, IEnumerable<IProcessTaskTemplate> templatesToApply)
		{
			Argument.NotNull(sourceWorkflowProvider, nameof(sourceWorkflowProvider));
			Argument.NotNull(connectedWorkflowProvider, nameof(connectedWorkflowProvider));
			MaybeCreateLinksForAppliedTemplatesOnly(sourceWorkflowProvider, connectedWorkflowProvider, factory, templatesToApply);
		}

		void IWorkflowProvidersLinkageService.WorkflowProvidersUnLinked(IWorkflowProviderCore sourceWorkflowProvider, IWorkflowProviderCore disconnectedWorkflowProvider, BusinessObjectFactory factory)
		{
			Argument.NotNull(sourceWorkflowProvider, nameof(sourceWorkflowProvider));
			Argument.NotNull(disconnectedWorkflowProvider, nameof(disconnectedWorkflowProvider));
			MaybePerformLinkAction(sourceWorkflowProvider, disconnectedWorkflowProvider, factory, ProposeLinkRemoval, DeletedProviderLinkRemoval);
		}

		#endregion

		#region Implementation

		static bool ShouldPerformLinkActions
		{
			get { return BMSRegistryProvider.IsBufferManagementEnabled; }
		}

		delegate void LinkAction(ProcessJobHeader fromJobHeader, ProcessJobHeader toJobHeader);
		delegate void DeletedProviderLinkAction(IWorkflowProviderCore fromWorkflowProvider, IWorkflowProviderCore toWorkflowProvider);

		static void MaybePerformLinkAction(
			IWorkflowProviderCore fromWorkflowProvider,
			IWorkflowProviderCore toWorkflowProvider,
			BusinessObjectFactory factory,
			LinkAction linkAction,
			DeletedProviderLinkAction handleDeletedProviderAction)
		{
			if (ShouldPerformLinkActions)
			{
				if ((fromWorkflowProvider is BusinessObject fromBizo && fromBizo.IsDeleted)
					|| toWorkflowProvider is BusinessObject toBizo && toBizo.IsDeleted)
				{
					handleDeletedProviderAction(fromWorkflowProvider, toWorkflowProvider);
					return;
				}

				var firstJobHeader = ProcessJobHeaderProvider.GetForParent(fromWorkflowProvider, factory, addDefaultProcessHeaderIfNone: false);
				var secondJobHeader = ProcessJobHeaderProvider.GetForParent(toWorkflowProvider, factory, addDefaultProcessHeaderIfNone: false);

				if (firstJobHeader != null && secondJobHeader != null)
				{
					linkAction(firstJobHeader, secondJobHeader);
				}
			}
		}

		void MaybeCreateLinksForAppliedTemplatesOnly(
			IWorkflowProviderCore fromWorkflowProvider,
			IWorkflowProviderCore toWorkflowProvider,
			BusinessObjectFactory factory,
			IEnumerable<IProcessTaskTemplate> templatesToApply)
		{
			if (ShouldPerformLinkActions)
			{
				var firstJobHeader = ProcessJobHeaderProvider.GetForParent(fromWorkflowProvider, factory, addDefaultProcessHeaderIfNone: false);
				var secondJobHeader = ProcessJobHeaderProvider.GetForParent(toWorkflowProvider, factory, addDefaultProcessHeaderIfNone: false);

				if (firstJobHeader != null && secondJobHeader != null)
				{
					ProposeLinkCreationForSpecificTemplates(firstJobHeader, secondJobHeader, templatesToApply);
				}
			}
		}

		#region Linking Actions

		void ProposeLinkCreation(ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader)
		{
			var viewModel = new WorkflowProvidersLinkViewModel(firstJobHeader, secondJobHeader);
			CreateLinks(viewModel, firstJobHeader, secondJobHeader);
		}

		void ProposeLinkCreationForSpecificTemplates(ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader, IEnumerable<IProcessTaskTemplate> templatesToApply)
		{
			var viewModel = new WorkflowProvidersLinkViewModel(firstJobHeader, secondJobHeader, templatesToApply);
			CreateLinks(viewModel, firstJobHeader, secondJobHeader);
		}

		void CreateLinks(WorkflowProvidersLinkViewModel viewModel, ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader)
		{
			if (viewModel.ProposedProcessHeaderLinks.Count > 0)
			{
				var strategy = GetInteractionStrategy();

				if (strategy.ShouldCommitProposedLinks(this, viewModel, firstJobHeader, secondJobHeader))
				{
					viewModel.CreateRealLinksFromProposed();
				}
			}
		}

		#endregion

		#region Unlinking Actions

		void ProposeLinkRemoval(ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader)
		{
			var firstJobPKs = firstJobHeader.ProcessHeaders.Select(w => w.PK).Concat(new[] { firstJobHeader.PK }).ToArray();
			var secondJobPKs = secondJobHeader.ProcessHeaders.Select(w => w.PK).Concat(new[] { secondJobHeader.PK }).ToArray();

			var query = new ZQuery();

			var firstDirectionQuery = new ZQuery();
			firstDirectionQuery.AddToFilter(new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, firstJobPKs));
			firstDirectionQuery.AddToFilter(new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, secondJobPKs));

			var secondDirectionQuery = new ZQuery();
			secondDirectionQuery.AddToFilter(new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, firstJobPKs));
			secondDirectionQuery.AddToFilter(new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, secondJobPKs));

			query.AddToFilter(firstDirectionQuery);
			query.AddToFilter(secondDirectionQuery, JoinCondition.Or);

			var links = firstJobHeader.Factory.Load<ProcessHeaderLink>(query);

			if (links.Length > 0)
			{
				var strategy = GetInteractionStrategy();

				if (strategy.ShouldRemoveExistingLinks(this, links, firstJobHeader, secondJobHeader))
				{
					foreach (var link in links)
					{
						link.Delete();
					}
				}
			}
		}

		void DeletedProviderLinkRemoval(IWorkflowProviderCore fromWorkflowProvider, IWorkflowProviderCore toWorkflowProvider)
		{
			if (!(fromWorkflowProvider is BusinessObject && toWorkflowProvider is BusinessObject))
			{
				ErrorReporter.ReportOnce("You have a WorkflowProvider that isn't a BusinessObject, and it's scaring me.");
				return;
			}

			var factory = new BusinessObjectFactory();
			var fromWorkflow = factory.Load(fromWorkflowProvider.GetType(), fromWorkflowProvider.PK);
			var toWorkflow = factory.Load(toWorkflowProvider.GetType(), toWorkflowProvider.PK);

			if (fromWorkflow == null && toWorkflow == null)
			{
				return;
			}

			var linkQuery = new ZDBOnlyQuery(typeof(ProcessHeaderLink));

			if (fromWorkflow != null)
			{
				var (fromProviderQueryText, fromParams) = GetProviderQueryParts(fromPKParamName, fromWorkflow);
				linkQuery.AddFilterAndZSQLParameterCollection(fromProviderQueryText, fromParams, JoinCondition.Or);
			}

			if (toWorkflow != null)
			{
				var (toProviderQueryText, toParams) = GetProviderQueryParts(toPKParamName, toWorkflow);
				linkQuery.AddFilterAndZSQLParameterCollection(toProviderQueryText, toParams, JoinCondition.Or);
			}

			var linksToDelete = factory.Load<ProcessHeaderLink>(linkQuery);

			foreach (var link in linksToDelete)
			{
				link.Delete();
			}

			factory.Save();
		}

		const string fromPKParamName = "@ParentFromJobPK";
		const string toPKParamName = "@ParentToJobPK";

		(string queryText, ZSqlParameterCollection parameters) GetProviderQueryParts(string paramName, BusinessObject workflowToQuery)
		{
			var parameters = new ZSqlParameterCollection();
			parameters.Add(paramName, workflowToQuery.PK, workflowToQuery.PKSchemaColumn);

			var genericQueryText = @"{0} IN ( SELECT FH_PK FROM dbo.ProcessHeader WHERE FH_ParentId IN ( {1} ) )";
			var toOrFromColumn = GetProviderLinkColumn(paramName);
			var realQuery = string.Format(CultureInfo.InvariantCulture, genericQueryText, toOrFromColumn, paramName);

			return (realQuery, parameters);
		}

		string GetProviderLinkColumn(string paramName)
		{
			switch (paramName)
			{
				case fromPKParamName:
					return ProcessHeaderLinkSchema.Constants.FP_FH_HeaderFrom;
				case toPKParamName:
					return ProcessHeaderLinkSchema.Constants.FP_FH_HeaderTo;
				default: // this should never happen wtf
					ErrorReporter.ReportOnce("Invalid switch case fall through for WorkflowProvidersLinkageService.GetProviderQueryParts()",
						"Attempted to make a query for a third, unspecified type of workflowprovider. This should only ever be ParentFromJobPK or ParentToJobPK. The value was " + paramName);
					return string.Empty;
			}
		}

		#endregion

		public enum LinkOperationType
		{
			Addition,
			Deletion,
		}

		public DialogDefaultContext CreateDialogContext(LinkOperationType operation, ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader, MultilingualString caption, ResourceStringData nullContextDescription, ZMessageBoxIcon icon, ZMessageBoxButtons? buttons = null)
		{
			var contextString = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", operation, firstJobHeader.Parent.WorkflowType, secondJobHeader.Parent.WorkflowType);
			var contextBlob = DialogDefaultContext.ToZBlob(contextString);

			var context = new DialogDefaultContext(
				operation == LinkOperationType.Addition ? new ZGuid("81909AE5-58F7-4E57-BA0B-29465080D16E") : new ZGuid("E2358AEC-89AC-4709-B9DB-51FE490713AB"),
				caption,
				buttons,
				icon,
				context: contextBlob,
				nullContextDescription: nullContextDescription,
				forceOverriddenDefaults: true
				);

			return context;
		}

		#endregion

		#region User Interaction

		static ILinkageServiceInteractionStrategy GetInteractionStrategy()
		{
			if (Globals.CanShowDialogs)
			{
				if (!Globals.IsTest && Db.Connection.IsInTransaction)
				{
					ErrorReporter.ReportOnce("We really want to show the user a form to confirm candidate links to be created, but we can't because we're inside a transaction (and we don't want to hold db locks for too long).");
				}
				else
				{
					return ObjectFactory.Get<ILinkageServiceInteractionStrategy>("UserInteractiveInteractionStrategy");
				}
			}

			return new NonInteractiveInteractionStrategy();
		}

		public interface ILinkageServiceInteractionStrategy
		{
			bool ShouldCommitProposedLinks(WorkflowProvidersLinkageService service, WorkflowProvidersLinkViewModel viewModel, ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader);
			bool ShouldRemoveExistingLinks(WorkflowProvidersLinkageService service, ProcessHeaderLink[] links, ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader);
		}

		#region Non-User Interactive

		class NonInteractiveInteractionStrategy : ILinkageServiceInteractionStrategy
		{
			bool ILinkageServiceInteractionStrategy.ShouldCommitProposedLinks(WorkflowProvidersLinkageService service, WorkflowProvidersLinkViewModel viewModel, ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader)
			{
				return ShouldCreateLinks;
			}

			bool ILinkageServiceInteractionStrategy.ShouldRemoveExistingLinks(WorkflowProvidersLinkageService service, ProcessHeaderLink[] links, ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader)
			{
				return true;
			}

			bool ShouldCreateLinks => WorkflowDataRegistry.Instance.AlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates.Value;
		}

		#endregion

		#endregion
	}
}
