using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public abstract class XTTInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		protected XTTInboundInterchangeProcessor() { }

		protected sealed override bool IsNoBranchFilter => true;
		protected sealed override void AddTypeFilter(ZQuery query)
		{
			query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchangeTransportTypeList.Codes.xT);
			query.AddToFilter(GetInterchangeTypeFilter());
		}

		protected abstract ZQuery GetInterchangeTypeFilter();

		public sealed override bool IsInterchangeNotDeleted(EDIInterchange interchange) => !interchange.IsDeleted && interchange.EI_Status != EDIInterchange.Status.Error;
	}
}
