using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class DepartureNCTS5TransitOperationWrapper : NCTS5CommonTransitOperationLRNWrapper, IDepartureNCTSTransitOperation
	{
		public DepartureNCTS5TransitOperationWrapper(NctsHeader header, ZString messageType) : base(header)
		{
			this.messageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
		}
		readonly ZString messageType;

		public INCTSCommonCompleteTransitOperation CommonTransitOperation => commonTransitOperation ?? (commonTransitOperation = new NCTS5CommonCompleteTransitOperationWrapper(nctsHeader, messageType));
		NCTS5CommonCompleteTransitOperationWrapper commonTransitOperation;
	}
}
