using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class AmendmentNCTS5TransitOperationWrapper : NCTS5CommonTransitOperationMRNWrapper, IAmendmentNCTSTransitOperation
	{
		public AmendmentNCTS5TransitOperationWrapper(NctsHeader header, ZString messageType) : base(header)
		{
			this.messageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
		}
		readonly ZString messageType;

		public INCTSCommonCompleteTransitOperation CommonTransitOperation => commonTransitOperation ?? (commonTransitOperation = new NCTS5CommonCompleteTransitOperationWrapper(nctsHeader, messageType));
		NCTS5CommonCompleteTransitOperationWrapper commonTransitOperation;
	}
}
