using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaCargoOutturnSendMessagesToCustoms : SendsMessagesToCustomsGUI
	{
		public override bool LodgementAllowed
		{
			get { return true; }
		}

		protected override ZForm GetBackDoorForSavingForm(RequiredMessagesInformation detectionResult, IDeferredAmendmentSavingOptions savingOptions)
		{
			var lastSavingOptions = (DeferredCusOutturnHeaderSavingOptions)savingOptions;
			if (lastSavingOptions != null)
			{
				return new SeaCargoDepotOutturnBackdoorForSavingOnAmendmentForm(lastSavingOptions);
			}

			return new BackdoorForSavingOnAmendmentForm();
		}
	}
}
