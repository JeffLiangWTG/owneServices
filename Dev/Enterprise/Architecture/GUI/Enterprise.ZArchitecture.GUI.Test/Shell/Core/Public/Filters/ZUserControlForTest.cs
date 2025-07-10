namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZUserControlForTest : ZUserControl
	{
		public void ProcessTabKeyCoreExposed(bool forward)
		{
			base.ProcessTabKeyCore(forward);
		}
	}
}
