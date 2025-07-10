
namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class NewStandAloneFsrEnquiryManager
	{
		public NewStandAloneFsrEnquiryManager(NonPersistentStandAloneFsrEnquiryForNew nonPersistentBO)
		{
			this.NonPersistentBO = nonPersistentBO;
		}

		public void ExecuteMakingRealEdiMessageFromNonPersistentHelper()
		{
			StandAloneFsrEnquiry.MakeNewOutboundFromPayload(NonPersistentBO);
			NonPersistentBO.Factory.Save();
		}

		public NonPersistentStandAloneFsrEnquiryForNew NonPersistentBO { get; private set; }
	}
}
