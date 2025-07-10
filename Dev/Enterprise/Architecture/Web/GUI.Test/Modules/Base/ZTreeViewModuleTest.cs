namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class ZTreeViewModuleTest : ZWebModule_Test
	{
		#region Implementation

		protected override ZWebModule GetNewZWebModule()
		{
			return ZWebModuleFactory.Create(TestID, Factory);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.DummyZTreeView; }
		}

		#endregion
	}
}
