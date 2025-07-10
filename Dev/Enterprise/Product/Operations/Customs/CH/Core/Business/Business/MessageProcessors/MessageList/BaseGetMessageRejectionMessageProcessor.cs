using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseGetMessageRejectionMessageProcessor : BaseGetMessageInboundMessageProcessor
{
	public BaseGetMessageRejectionMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Rejected };

	internal protected override bool LinkMessageToCompany => true;

	protected internal override bool MustProcessInOrder => false;

	protected override void UpdateTransaction(CusPollingTransaction transaction)
	{
		transaction.CPT_Status = transaction.CPT_NumberOfAttempts < CHCustomsDataRegistry.Instance.MaxNumberOfGetMessageAttempts.Value ? CompanyPollingTransaction.StatusCodes.Rejected : CompanyPollingTransaction.StatusCodes.Skip;
		transaction.CPT_StatusTimeUtc = ZDateTime.UtcNow;
	}
}
