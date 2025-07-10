using System;

namespace CargoWise.NetworkVisualisation.Business
{
	class ResizeStrategyProvider : CoordinateStrategyProvider, IDisposable
	{
		internal ResizeStrategyProvider(NetworkViewModel networkViewModel)
		{
			this.networkViewModel = networkViewModel;

			X = new ResizeStrategy((entity, val) => entity.X = val, e => e.X, shouldConsiderDescendants: true);
			Y = new ResizeStrategy((entity, val) => entity.Y = val, e => e.Y, shouldConsiderDescendants: true);
			Width = new ResizeStrategy((entity, val) => entity.Width = val, e => e.Width, shouldConsiderDescendants: false);
			Height = new ResizeStrategy((entity, val) => entity.Height = val, e => e.Height, shouldConsiderDescendants: false);

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
