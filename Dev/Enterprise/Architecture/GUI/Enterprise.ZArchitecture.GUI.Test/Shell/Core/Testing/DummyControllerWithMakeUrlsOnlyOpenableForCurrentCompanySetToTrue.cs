namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerWithMakeUrlsOnlyOpenableForCurrentCompanySetToTrue : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.Dummy4;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;
	}
}
