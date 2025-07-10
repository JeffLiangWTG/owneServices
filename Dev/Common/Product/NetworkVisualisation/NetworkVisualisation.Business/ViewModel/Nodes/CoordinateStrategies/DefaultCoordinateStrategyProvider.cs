namespace CargoWise.NetworkVisualisation.Business
{
	class DefaultCoordinateStrategyProvider : CoordinateStrategyProvider
	{
		internal DefaultCoordinateStrategyProvider(NetworkViewModel viewModel)
		{
			X = new XCoordinateStrategy((entity, val) => entity.X = val, (entity) => entity.X, viewModel);
			Y = new YCoordinateStrategy((entity, val) => entity.Y = val, (entity) => entity.Y, viewModel);
			Width = new WidthCoordinateStrategy((entity, val) => entity.Width = val, (entity) => entity.Width, viewModel);
			Height = new HeightCoordinateStrategy((entity, val) => entity.Height = val, (entity) => entity.Height, viewModel);
		}
	}
}
