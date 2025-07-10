namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDummyModuleButtonGridAlwaysShowFormInReadOnlyMode : ZDummyModuleButtonGrid
	{
		protected override bool AlwaysShowFormInReadOnlyMode => true;
	}
}
