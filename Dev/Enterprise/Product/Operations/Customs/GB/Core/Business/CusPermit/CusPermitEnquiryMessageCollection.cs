using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class CusPermitEnquiryMessageCollection : BusinessObjectCollection<CusPermitEnquiryMessage>
	{
		public CusPermitEnquiryMessageCollection(BaseCusPermitHeader master)
			: base(master.Factory)
		{
			PermitHeader = master;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var filter = base.CreateRelationshipFilter();
			filter.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, PermitHeader.PK);
			filter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			filter.AddToFilter(EDIMessageSchema.EM_MessageType, Interrogate_DLU.FunctionCodeConst);
			return filter;
		}

		BaseCusPermitHeader PermitHeader { get; set; }
	}
}
