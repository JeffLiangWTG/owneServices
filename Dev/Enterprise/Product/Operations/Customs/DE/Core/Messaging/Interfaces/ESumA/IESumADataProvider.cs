using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IESumADataProvider : IDataProvider
	{
		ZString InterchangeRecipientReferenceNumber { get; }

		ZString InterchangeRecipientSubsidiaryNumber { get; }

		ZString LocalReferenceNumber { get; }

		ZString ReferenceNumber { get; }
	}
}
