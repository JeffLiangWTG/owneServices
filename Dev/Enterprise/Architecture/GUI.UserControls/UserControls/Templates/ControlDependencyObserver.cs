using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ControlDependencyObserver
	{
		public ControlDependencyObserver(IControlHost controlHost)
		{
			this.controlHost = Argument.NotNull(controlHost, nameof(controlHost));
		}

		readonly IControlHost controlHost;

		public void ConfigureControlBehaviourObservers(ControlReference controlReference, ControlBehaviourContainerCollection behaviourContainerCollection)
		{
			var currentDataItem = controlHost.GetCurrentDataItem();
			var allControlStates = behaviourContainerCollection.GetAll();

			foreach (var stateContainer in allControlStates)
			{
				var behaviourDependencyChangeHandler = new ControlBehaviourDependencyChangeHandler(controlHost, stateContainer, controlReference);
				var dependencies = stateContainer.GetDependencies(currentDataItem);

				foreach (var dependency in dependencies)
				{
					dependency.ValueChanged += behaviourDependencyChangeHandler.HandleDependencyChange;
					dependencyObservers.Add((dependency, behaviourDependencyChangeHandler));
				}
			}
		}

		public void UnsubscribeObservers()
		{
			foreach (var (dependency, handler) in dependencyObservers)
			{
				dependency.ValueChanged -= handler.HandleDependencyChange;
			}

			dependencyObservers.Clear();
		}

		readonly List<(ZPropertyInfo, IDependencyChangeHandler)> dependencyObservers = new List<(ZPropertyInfo, IDependencyChangeHandler)>();

		#region DependencyChangeHandler

		interface IDependencyChangeHandler
		{
			void HandleDependencyChange(object sender, EventArgs e);
		}

		class ControlBehaviourDependencyChangeHandler : IDependencyChangeHandler
		{
			public ControlBehaviourDependencyChangeHandler(IControlHost controlHost, IControlBehaviourContainer behaviourContainer, ControlReference controlReference)
			{
				this.controlHost = controlHost;
				this.behaviourContainer = behaviourContainer;
				this.controlReference = controlReference;
			}

			readonly IControlHost controlHost;
			readonly IControlBehaviourContainer behaviourContainer;
			readonly ControlReference controlReference;

			public void HandleDependencyChange(object sender, EventArgs e)
			{
				var control = controlHost.GetControl(controlReference);
				var currentDataItem = controlHost.GetCurrentDataItem();
				if (behaviourContainer.IsRefreshRequired(currentDataItem, control))
				{
					controlHost.UpdateLayout();
				}
			}
		}

		#endregion
	}
}
