using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class IETS414Wrapper : IIETS414
	{
		IETS414Wrapper(TemporaryStorageHeader header, InvalidationRequestTSDMessageFunction messageFunction)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.messageFunction = Argument.NotNull(messageFunction, nameof(messageFunction));
		}

		readonly TemporaryStorageHeader header;
		readonly InvalidationRequestTSDMessageFunction messageFunction;

		public IMessageHeader MessageHeader => messageHeader ?? (messageHeader = new MessageHeaderWrapper());
		IMessageHeader messageHeader;

		public string Crn => crn ?? (crn = header.CRN);
		string crn;

		public string InvalidationReason => invalidationReason ?? (invalidationReason = messageFunction.VOCReason);
		string invalidationReason;

		public IDeclarant Declarant => declarant ?? (header.Declarant?.Header != null ? declarant = DeclarantWrapper.New(header) : null);
		IDeclarant declarant;

		public IRepresentative Representative => representative ?? (representative = RepresentativeWrapper.New(header));
		IRepresentative representative;

		public static IETS414Wrapper New(TemporaryStorageHeader header, InvalidationRequestTSDMessageFunction messageFunction) => header == null || messageFunction == null ? null : new IETS414Wrapper(header, messageFunction);
	}
}
