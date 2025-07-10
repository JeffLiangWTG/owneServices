using System;

namespace CargoWise.NetworkVisualisation.Business
{
	class SuspendChildBoundsCheckingStrategyProvider : CoordinateStrategyProvider, IDisposable
	{
		internal SuspendChildBoundsCheckingStrategyProvider(NetworkViewModel networkViewModel)
		{
			this.networkViewModel = networkViewModel;

			X = new XCoordinateStrategy((entity, val) => entity.X = val, e => e.X, networkViewModel, shouldConsiderDescendants: false);
			Y = new YCoordinateStrategy((entity, val) => entity.Y = val, e => e.Y, networkViewModel, shouldConsiderDescendants: false);
			Width = new WidthCoordinateStrategy((entity, val) => entity.Width = val, e => e.Width, networkViewModel, shouldConsiderDescendants: false);
			Height = new HeightCoordinateStrategy((entity, val) => entity.Height = val, e => e.Height, networkViewModel, shouldConsiderDescendants: false);

			replacedCoodinateStrategy = networkViewModel.CoordinateStrategyProvider;
			networkViewModel.CoordinateStrategyProvider = this;
		}

		readonly NetworkViewModel networkViewModel;
		readonly CoordinateStrategyProvider replacedCoodinateStrategy;

		public void Dispose()
		{
			networkViewModel.CoordinateStrategyProvider = replacedCoodinateStrategy;
		}
	}
}
