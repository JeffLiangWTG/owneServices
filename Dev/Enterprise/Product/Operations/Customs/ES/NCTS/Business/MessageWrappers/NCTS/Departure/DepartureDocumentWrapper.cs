using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class DepartureDocumentWrapper : DocumentCommonWrapper, IDepartureDocuments
	{
		public DepartureDocumentWrapper(ZString code, ZString referenceNumber)
			: base(code, referenceNumber)
		{
		}

		public ZString Source => ZString.Empty; //Not specified in the mapping document, might be changed in future WIs
	}
}
