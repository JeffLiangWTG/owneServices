namespace Enterprise.Security.Provider.Testing
{
	sealed class DocumentSecurityInfoProviderTest : StmMenuItemSecurityInfoProviderTest<DocumentSecurityInfoProvider>
	{
		protected override StmMenuItemCheckpointHelper MenuItemCheckpointHelper => new DocumentsCheckpointHelper();
	}
}
