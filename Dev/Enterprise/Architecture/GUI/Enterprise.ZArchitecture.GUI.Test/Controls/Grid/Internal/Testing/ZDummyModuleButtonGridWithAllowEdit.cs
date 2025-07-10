namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDummyModuleButtonGridWithAllowEdit : ZDummyModuleButtonGrid
	{
		protected override bool AllowOpenInEditFormEvenIfListIsReadOnly
		{
			get { return true; }
		}
	}
}
