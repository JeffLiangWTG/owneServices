using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class StandAloneFsrEnquiryCollection : BusinessObjectCollection<StandAloneFsrEnquiry>
	{
		public StandAloneFsrEnquiryCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry.Code);
			filter.AddToFilter(EDIMessageSchema.EM_MessageSubType, CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry.Subcode);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);  // responses are shown inside their outbound message
			return filter;
		}
	}
}
