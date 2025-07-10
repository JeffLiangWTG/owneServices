using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.Business
{
	public abstract class DynamicNetworkAction : NetworkActionBase, IDynamicNetworkAction
	{
		protected DynamicNetworkAction(INetworkViewModel networkViewModel, INetworkActionExecutionStrategy executionStrategy = null, int group = 0, int groupIndex = 0, bool shouldUpdateOnNetworkEvents = true)
			: base(group, groupIndex)
		{
			NetworkViewModel = networkViewModel;
			ExecutionStrategy = executionStrategy ?? new SingleEntityExecutionStrategy();

			ShouldUpdateOnNetworkEvents = shouldUpdateOnNetworkEvents;
		}

		public bool ShouldUpdateOnNetworkEvents;

		#region Public Accessors

		public INetworkViewModel NetworkViewModel { get; }

		public NetworkViewModel GetNetworkViewModel()
		{
			if (NetworkViewModel is NetworkViewModel model)
			{
				return model;
			}
			else
			{
				throw new InvalidCastException("Cannot cast NetworkViewModel.");
			}
		}

		#endregion

		#region Accessibility

		// There are four levels of accessibility of dynamic network actions:
		// - IsApplicable - the same as for actions in general (see NetworkActionBase)
		// - IsEnabled - the same as for actions in general (see NetworkActionBase)
		// - CheckCanStartExecution - determines whether the network action can start execution and shows reasons why the action cannot be executed if there are any.
		//		Relies on CheckCanStartExecutionForNetwork and enabledness.
		//		Execution may start even when pre-execution checks for a particular entity do not allow to execute for this entity.
		//		Not enabled actions are automatically cannot start execution.
		// - PerformPreExecutionChecksForEntity - determines whether the network action can execute for a particular entity.
		//		May include some user confirmations or heavy calculations otherwise simply defined by enabledness.

		public INetworkActionAccessibility IsApplicableToEntity(INetworkEntity entity)
		{
			return new NetworkActionAccessibility(GetNode(entity) != null && !entity.IsDeleted,
					entity,
					() => Res.GetString("E45496BF-4D24-4D2D-B31F-4D4E680AF2B0", "Cannot execute as the entity was deleted."))
				.UnionIfAllowed(() => IsApplicableToEntityCore(entity));
		}

		public INetworkActionAccessibility IsEnabledForEntity(INetworkEntity entity)
		{
			return IsApplicableToEntity(entity).UnionIfAllowed(() => IsEnabledForEntityCore(entity));
		}

		public INetworkActionAccessibility PerformPreExecutionChecksForEntity(INetworkEntity entity)
		{
			return IsEnabledForEntity(entity).UnionIfAllowed(() => PerformPreExecutionChecksForEntityCore(entity));
		}

		public INetworkActionAccessibility CheckCanStartExecutionForNetwork()
		{
			return CheckCanStartExecutionForNetworkCore();
		}

		#endregion

		#region Execution

		public INetworkActionResult ExecuteForEntityWithoutAccessCheck(INetworkEntity entity)
		{
			return ExecuteForEntityCore(entity);
		}

		/// <summary>
		/// Additional execution data shared among all calls to ExecuteForEntityCore
		/// </summary>
		public object ExecutionSessionData { get; private set; }

		#endregion

		#region Implementation

		public readonly INetworkActionExecutionStrategy ExecutionStrategy;

		#region NetworkActionBase Overrides

		protected sealed override ResourceString GetNameCore() => name ?? (name = RecalculateNameCore());
		ResourceString name;

		protected sealed override ResourceString GetDescriptionCore() => description ?? (description = RecalculateDescriptionCore());
		ResourceString description;

		protected sealed override bool IsActivatedCore() => isActivated ?? (isActivated = RecalculateIsActivatedCore()).Value;
		bool? isActivated;

		protected sealed override IEnumerable<INetworkAction> GetChildActionsCore() => childActions ?? (childActions = RecalculateChildActionsCore());
		IEnumerable<INetworkAction> childActions;

		protected sealed override INetworkActionAccessibility IsApplicableCore() => isApplicable ?? (isApplicable = RecalculateIsApplicableCore());
		INetworkActionAccessibility isApplicable;

		protected sealed override INetworkActionAccessibility IsEnabledCore() => isEnabled ?? (isEnabled = RecalculateIsEnabledCore());
		INetworkActionAccessibility isEnabled;

		protected sealed override INetworkActionAccessibility CheckCanStartExecutionCore() => IsEnabled().UnionIfAllowed(() => ExecutionStrategy.CanActionStartExecution(this));

		protected sealed override INetworkActionResult ExecuteCore()
		{
			ExecutionSessionData = DefineExecutionSessionData();
			var result = ExecutionStrategy.ExecuteAction(this);
			ExecutionSessionData = null;
			return result;
		}

		protected override void RefreshCore(RefreshArgs args)
		{
			base.RefreshCore(args);

			if (ShouldRecalculateAccessibilityOnModelChanged(args))
			{
				isApplicable = RecalculateIsApplicableCore();
				isEnabled = RecalculateIsEnabledCore();
				name = null;
				description = null;
				isActivated = null;
				childActions = null;
			}

			if (ShouldRecalculateIsActivatedOnModelChanged(args))
			{
				isActivated = RecalculateIsActivatedCore();
			}

			if (ShouldRecalculateNameAndDescriptionOnModelChanged(args))
			{
				name = RecalculateNameCore();
				description = RecalculateDescriptionCore();
			}

			if (ShouldRecalculateChildActionsOnModelChanged(args))
			{
				childActions = RecalculateChildActionsCore();

				foreach (var childAction in childActions.WhereNotNull())
				{
					childAction.Refresh(args);
				}
			}
		}

		#endregion

		#region Recalculation

		protected virtual bool ShouldRecalculateAccessibilityOnModelChanged(RefreshArgs args) => true;

		protected virtual bool ShouldRecalculateNameAndDescriptionOnModelChanged(RefreshArgs args) => true;

		protected virtual bool ShouldRecalculateIsActivatedOnModelChanged(RefreshArgs args) => true;

		protected virtual bool ShouldRecalculateChildActionsOnModelChanged(RefreshArgs args) => true;

		INetworkActionAccessibility RecalculateIsApplicableCore() => ExecutionStrategy.IsActionApplicable(this);

		INetworkActionAccessibility RecalculateIsEnabledCore() => IsApplicable().UnionIfAllowed(() => ExecutionStrategy.IsActionEnabled(this));

		ResourceString RecalculateNameCore() => IsApplicable().IsAllowed
			? GetNameCore(GetActiveEntity())
			: GetDefaultNameCore();

		ResourceString RecalculateDescriptionCore() => IsApplicable().IsAllowed
			? GetDescriptionCore(GetActiveEntity())
			: GetDefaultDescriptionCore();

		bool RecalculateIsActivatedCore() => IsApplicable().IsAllowed
			? IsActivatedCore(GetActiveEntity())
			: GetDefaultIsActivatedCore();

		IEnumerable<INetworkAction> RecalculateChildActionsCore()
		{
			return IsApplicable().IsAllowed
				? GetChildActionsCore(GetActiveEntity()).ToArray()
				: GetDefaultChildActionsCore().ToArray();
		}

		#endregion

		#region Abstract and Virtual Methods

		protected abstract ResourceString GetDefaultNameCore();

		protected virtual ResourceString GetNameCore(INetworkEntity activeEntity) => GetDefaultNameCore();

		protected abstract ResourceString GetDefaultDescriptionCore();

		protected virtual ResourceString GetDescriptionCore(INetworkEntity activeEntity) => GetDefaultDescriptionCore();

		protected virtual bool GetDefaultIsActivatedCore() => false;

		protected virtual bool IsActivatedCore(INetworkEntity activeEntity) => GetDefaultIsActivatedCore();

		protected virtual IEnumerable<INetworkAction> GetDefaultChildActionsCore() => Array.Empty<INetworkAction>();

		protected virtual IEnumerable<INetworkAction> GetChildActionsCore(INetworkEntity activeEntity) => GetDefaultChildActionsCore();

		protected virtual INetworkActionAccessibility CheckCanStartExecutionForNetworkCore() => NetworkActionAccessibility.Allowed;

		protected virtual object DefineExecutionSessionData() => null;

		protected virtual string IconName { get; } = string.Empty;

		protected override string GetIconNameCore() => IconName;

		#region Entity Related Methods

		//the following methods execute a network action or determine its accessbility for a particular entity in order to execute the action or determine its overall accessibility for a given set of entities 

		protected abstract INetworkActionAccessibility IsApplicableToEntityCore(INetworkEntity entity);

		protected abstract INetworkActionAccessibility IsEnabledForEntityCore(INetworkEntity entity);

		protected virtual INetworkActionAccessibility PerformPreExecutionChecksForEntityCore(INetworkEntity entity) => NetworkActionAccessibility.Allowed;

		protected abstract INetworkActionResult ExecuteForEntityCore(INetworkEntity entity);

		#endregion

		#endregion

		#region Node and Entity Accessors

		protected NodeViewModel GetNode(INetworkEntity entity) => GetNetworkViewModel()?.GetNodeForEntity(entity);

		INetworkEntity GetActiveEntity() => GetNetworkViewModel()?.ActiveEntity;

		#endregion

		#endregion
	}
}
