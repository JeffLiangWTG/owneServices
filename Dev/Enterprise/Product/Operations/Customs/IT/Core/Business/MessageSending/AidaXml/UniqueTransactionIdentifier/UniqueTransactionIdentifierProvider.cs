using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;

public sealed class UniqueTransactionIdentifierProvider
{
	public UniqueTransactionIdentifierProvider(EDIInterchange interchange)
	{
		this.interchange = Argument.NotNull(interchange, nameof(interchange));
	}

	readonly EDIInterchange interchange;

	public ZString GetUniqueTransactionID()
	{
		var relatedInterchange = LoadRelatedInterchange();

		return relatedInterchange is null
			? ZString.Empty
			: ExtractTransactionID(relatedInterchange.EI_BodyText);
	}

	ZString ExtractTransactionID(ZString bodyText)
	{
		return UniqueTransactionIdentifierTextExtractor
			.ExtractUniqueTransactionIdentifier(bodyText);
	}

	EDIInterchange LoadRelatedInterchange()
	{
		var query = new ZQuery(EDIInterchangeSchema.EI_SessionGUID, interchange.EI_SessionGUID);
		query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
		query.AddToFilter(EDIInterchangeSchema.EI_InterchangeType, MessageProcessorConstants.InterchangeTypes.Ucc6AcknowledgementType);
		query.AddToFilter(EDIInterchangeSchema.PK, SQLComparisonOperator.NotEqual, interchange.PK);
		query.OrderBy = AutoEDIInterchange.Schema.EI_SystemCreateTimeUtc;
		return interchange.Factory.LoadTop1<EDIInterchange>(query);
	}
}
