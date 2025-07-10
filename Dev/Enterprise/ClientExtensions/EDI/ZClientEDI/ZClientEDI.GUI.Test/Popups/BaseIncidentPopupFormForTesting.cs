using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class BaseIncidentPopupFormForTesting : BaseIncidentPopupForm
	{
		public BaseIncidentPopupFormForTesting(SupportIncidentAction action) : base(action)
		{
		}

		public bool OnSuccessfulCloseWasCalled;
		protected override void OnSuccessfulClose()
		{
			OnSuccessfulCloseWasCalled = true;
		}
	}
}
