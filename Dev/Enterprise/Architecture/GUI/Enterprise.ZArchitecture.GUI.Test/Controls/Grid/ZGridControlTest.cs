namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridControlTest : ZControlBaseTestCase<ZGrid>
	{
		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}

		protected override bool IsDragDropOverriddenWithoutEDocs => true;
	}
}
