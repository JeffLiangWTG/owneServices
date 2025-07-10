using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Chief.Messaging.DLU
{
	public class DLUMessageCollection : BusinessObjectCollection<DLUMessage>
	{
		public DLUMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, ChiefConstants.CusDecTypeDLU);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);  // responses are shown inside their outbound message
			return filter;
		}
	}
}
