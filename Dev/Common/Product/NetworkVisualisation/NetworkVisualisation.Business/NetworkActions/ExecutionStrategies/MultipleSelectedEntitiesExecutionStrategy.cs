using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class MultipleSelectedEntitiesExecutionStrategy : NetworkActionExecutionStrategyBase
	{
		readonly RefreshType refreshTypeToEndExecution;

		public MultipleSelectedEntitiesExecutionStrategy() : this(RefreshType.None)
		{
		}

		public MultipleSelectedEntitiesExecutionStrategy(RefreshType refreshType)
		{
			refreshTypeToEndExecution = refreshType;
		}

		protected override INetworkActionAccessibility IsActionApplicableCore(IDynamicNetworkAction action)
		{
			return AllowIfAllAllow(GetEntitiesToExecute(action).Select(e => action.IsApplicableToEntity(e)));
		}

		protected override INetworkActionAccessibility IsActionEnabledCore(IDynamicNetworkAction action)
		{
			return AllowIfAtLeastOneAllows(GetEntitiesToExecute(action).Select(e => action.IsEnabledForEntity(e)).ToArray());
		}

		protected override INetworkActionAccessibility CanActionStartExecutionCore(IDynamicNetworkAction action)
		{
			return AllowIfAtLeastOneAllows(GetEntitiesToExecute(action).Select(e => action.IsEnabledForEntity(e)).ToArray())
				.UnionIfAllowed(() => action.CheckCanStartExecutionForNetwork());
		}

		protected override INetworkActionResult ExecuteCore(IDynamicNetworkAction action)
		{
			if (!CheckCanStartExecution(action))
			{
				return null;
			}

			var reasonsWhyNotExecutedForParticularEntities = NetworkActionAccessibility.Allowed;

			var entitiesToExecute = GetEntitiesToExecute(action);

			foreach (var entity in entitiesToExecute)
			{
				if (entity != null)
				{
					var preExecutionChecksResultForEntity = action.PerformPreExecutionChecksForEntity(entity);

					if (preExecutionChecksResultForEntity.IsAllowed)
					{
						action.ExecuteForEntityWithoutAccessCheck(entity);
					}
					else
					{
						reasonsWhyNotExecutedForParticularEntities = reasonsWhyNotExecutedForParticularEntities.Union(preExecutionChecksResultForEntity);
					}
				}
			}

			IsExecutionForSubsequentEntitiesConfirmed = true;
			IsUserConfirmationForSubsequentEntitiesConfirmed = false;

			if (reasonsWhyNotExecutedForParticularEntities.DenialReasons.Any(r => r.NeedsNotification))
			{
				GetController(action)?.NotifyActionExecutedPartially(reasonsWhyNotExecutedForParticularEntities);
			}

			if (refreshTypeToEndExecution != RefreshType.None)
			{
				action.NetworkViewModel.Network.Refresh(refreshTypeToEndExecution);
			}

			return null;
		}

		public bool IsExecutionForSubsequentEntitiesConfirmed { get; set; } = true;
		public bool IsUserConfirmationForSubsequentEntitiesConfirmed { get; set; }

		public IEnumerable<INetworkEntity> GetEntitiesToExecute(IDynamicNetworkAction action) => GetEntitiesToExecuteCore(action);

		protected virtual IEnumerable<INetworkEntity> GetEntitiesToExecuteCore(IDynamicNetworkAction action)
		{
			return action.NetworkViewModel.SelectedEntities.Any() ? action.NetworkViewModel.SelectedEntities : new INetworkEntity[] { action.NetworkViewModel.Network.DiagramEntity };
		}

		static INetworkActionAccessibility AllowIfAllAllow(IEnumerable<INetworkActionAccessibility> results)
		{
			return NetworkActionAccessibility.Join(results);
		}

		static INetworkActionAccessibility AllowIfAtLeastOneAllows(IEnumerable<INetworkActionAccessibility> results)
		{
			return results.Any(a => a.IsAllowed) ? NetworkActionAccessibility.Allowed : NetworkActionAccessibility.Join(results);
		}
	}
}
