namespace Enterprise.Security.Provider.Testing
{
	sealed class VisualizerFormSecurityInfoProviderTest : StmMenuItemSecurityInfoProviderTest<VisualizerFormSecurityInfoProvider>
	{
		protected override StmMenuItemCheckpointHelper MenuItemCheckpointHelper => new VisualizerFormsCheckpointHelper();
	}
}
