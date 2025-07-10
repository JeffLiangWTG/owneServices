namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerNotSupportsHyperlinking : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerNotSupportsHyperlinking;

		public override bool SupportsHyperlinking
		{
			get
			{
				return false;
			}
		}
	}
}
