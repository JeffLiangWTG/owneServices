using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC556C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.MessageBuilders;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BE.Business;

public class CC556CMessageProcessor : DeclarationMessageProcessor<ICC556CDataProvider>
{
	public CC556CMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CC556C;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CC556C };

	protected override Type MessageInterpreterType => typeof(CC556CMessageInterpreter);

	protected override BusinessObject FindParentOfMessage(BEMessage message, ICC556CDataProvider messageDataProvider) => MessageHelper.LocateEntryHeaderByLRNFallbackToMRN(message.Factory, messageDataProvider);

	protected internal override ICC556CDataProvider GetMessageDataProvider(BEMessage message) => message.GetCachedInboundProvider<Cc556CType, CC556CDataProvider>();

	protected override void PreProcessMessageWhenBOFoundCore(BEMessage message, ICC556CDataProvider messageDataProvider)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;

		string[] validEntryStatuses = { String.Empty, StatusCodes.ACK, StatusCodes.Presented, StatusCodes.InvalidationRequest, StatusCodes.AmendmentRequest };
		if (!validEntryStatuses.Contains((string)entryHeader.CH_EntryStatus))
		{
			DiscardMessage(message, Res.GetString("a94a4812-3bc2-43c4-8144-78467b885dc4",
							"The message with interchange is discarded, because the present is not empty, ACK, PRN, INR or AMR."));
		}
	}

	protected override (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, ICC556CDataProvider messageDataProvider)
	{
		((CusEntryHeader)message.EM_LinkedObject).CH_Status = LogicalStatusList.Codes.Invalid;
		return CalculateJobAndMessageStatus(messageDataProvider.BusinessRejectionType);
	}

	static (ZString jobStatus, ZString messageStatus, ZString errorLog) CalculateJobAndMessageStatus(ZString businessRejectionType)
	{
		ZString jobStatus = ZString.Empty;

		switch (businessRejectionType)
		{
			case ExportOperationBusinessRejectionTypeList.Codes.PresentationNotificationRejection:
			case ExportOperationBusinessRejectionTypeList.Codes.DeclarationAmendmentRejection:
			case ExportOperationBusinessRejectionTypeList.Codes.InvalidationRequestRejection:
				{
					string GetJobStatusWhenTrue()
					{
						switch (businessRejectionType)
						{
							case ExportOperationBusinessRejectionTypeList.Codes.PresentationNotificationRejection:
								return StatusCodes.RejectedPresentation;
							case ExportOperationBusinessRejectionTypeList.Codes.DeclarationAmendmentRejection:
								return StatusCodes.RejectedAmendment;
							case ExportOperationBusinessRejectionTypeList.Codes.InvalidationRequestRejection:
								return StatusCodes.RejectedInvalidation;
						}
						return ZString.Empty;
					}
					jobStatus = GetJobStatusWhenTrue();
					break;
				}
			case ExportOperationBusinessRejectionTypeList.Codes.DeclarationRejection:
				jobStatus = StatusCodes.Rejected;
				break;
			case ExportOperationBusinessRejectionTypeList.Codes.NonExitedExportInformationRejection:
				jobStatus = StatusCodes.RejectedNonExitExport;
				break;
		}

		return (jobStatus, EDIMessageStatusList.Codes.ProcessedOK, ZString.Empty);
	}
}
