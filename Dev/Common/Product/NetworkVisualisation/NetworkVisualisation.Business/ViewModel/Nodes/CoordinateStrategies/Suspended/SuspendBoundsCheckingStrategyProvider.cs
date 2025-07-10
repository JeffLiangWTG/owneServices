using System;

namespace CargoWise.NetworkVisualisation.Business
{
	class SuspendBoundsCheckingStrategyProvider : CoordinateStrategyProvider, IDisposable
	{
		internal SuspendBoundsCheckingStrategyProvider(NetworkViewModel networkViewModel)
		{
			this.networkViewModel = networkViewModel;

			X = new SuspendedBoundsCoordinatePropertyStrategy((entity, val) => entity.X = val);
			Y = new SuspendedBoundsCoordinatePropertyStrategy((entity, val) => entity.Y = val);
			Width = new SuspendedBoundsCoordinatePropertyStrategy((entity, val) => entity.Width = val);
			Height = new SuspendedBoundsCoordinatePropertyStrategy((entity, val) => entity.Height = val);

			replacedCoodinateStrategy = networkViewModel.CoordinateStrategyProvider;
			networkViewModel.CoordinateStrategyProvider = this;
		}

		readonly NetworkViewModel networkViewModel;
		readonly CoordinateStrategyProvider replacedCoodinateStrategy;

		void UpdateSuspendedNodes()
		{
			((SuspendedBoundsCoordinatePropertyStrategy)X).UpdateSuspended((CoordinatePropertyStrategy)replacedCoodinateStrategy.X);
			((SuspendedBoundsCoordinatePropertyStrategy)Y).UpdateSuspended((CoordinatePropertyStrategy)replacedCoodinateStrategy.Y);
			((SuspendedBoundsCoordinatePropertyStrategy)Width).UpdateSuspended((CoordinatePropertyStrategy)replacedCoodinateStrategy.Width);
			((SuspendedBoundsCoordinatePropertyStrategy)Height).UpdateSuspended((CoordinatePropertyStrategy)replacedCoodinateStrategy.Height);
		}

		public void Dispose()
		{
			networkViewModel.CoordinateStrategyProvider = replacedCoodinateStrategy;
			UpdateSuspendedNodes();
		}
	}
}
