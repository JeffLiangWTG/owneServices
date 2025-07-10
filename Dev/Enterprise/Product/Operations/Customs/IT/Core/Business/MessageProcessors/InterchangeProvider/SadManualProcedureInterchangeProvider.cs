using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class SadManualProcedureInterchangeProvider : SadInterchangeProvider
{
	public SadManualProcedureInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
	{
	}

	protected override ZString ProcessedMessageStatusCode(EDIInterchange interchange) => EDIMessageStatusList.Codes.Manual;

	protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => EDIInterchangeStatusList.Codes.Manual;

	protected override void SetAdditionalInterchangeValuesForTransmit(EDIInterchange interchange)
	{
		base.SetAdditionalInterchangeValuesForTransmit(interchange);
		interchange.EI_SessionGUID = ZGuid.NewZGuid();
	}
}
