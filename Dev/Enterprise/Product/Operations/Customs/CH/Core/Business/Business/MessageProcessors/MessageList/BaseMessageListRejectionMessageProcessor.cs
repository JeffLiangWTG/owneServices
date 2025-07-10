using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseMessageListRejectionMessageProcessor : BaseMessageProcessor
{
	public BaseMessageListRejectionMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.MSL };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Rejected };

	protected override void ProcessMessageCore(CHEDIMessage ediMessage)
	{
		if (FindLinkedObjectByOutgoingSessionID(ediMessage) is GlbCompany company)
		{
			ediMessage.EM_LinkedObject = company;
			var eventReference = StmALog.GenerateEventReference(ZString.Empty, new Dictionary<string, string>()
				{
					{ EventReferenceParameters.Codes.Type, MessageTypeCodeList.Codes.MSL },
					{ EventReferenceParameters.Codes.InterchangeNumber, ediMessage.Interchange.EI_InterchangeNum },
					{ EventReferenceParameters.Codes.DocumentSource, ApplicationCode },
				});
			company.Logs.AddNew(Events.MessageRejected, eventReference);
		}
	}
}
