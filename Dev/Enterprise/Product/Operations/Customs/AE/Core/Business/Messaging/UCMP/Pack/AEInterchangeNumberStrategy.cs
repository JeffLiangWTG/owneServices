using CargoWise.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

sealed class AEInterchangeNumberStrategy : IMessageNumberStrategy
{
	public AEInterchangeNumberStrategy(EDIInterchange interchange)
	{
		Interchange = Argument.NotNull(interchange, nameof(interchange));
	}
	EDIInterchange Interchange { get; }

	string IMessageNumberStrategy.GetMessageReferenceNumber()
	{
		var referenceNumberGenerator = new ReferenceNumberGenerator(Interchange.Factory);
		return referenceNumberGenerator.GenerateInterchangeReferenceNumber(Interchange.EI_ApplicationCode, Interchange.EI_InterchangeType);
	}
}
