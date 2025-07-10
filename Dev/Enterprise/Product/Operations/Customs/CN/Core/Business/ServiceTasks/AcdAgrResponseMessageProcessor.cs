using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.CN.MessageDefinitions.ACDA;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class AcdAgrResponseMessageProcessor : BranchCustomsApplicationTypeMessageProcessor
	{
		public AcdAgrResponseMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => Res.GetString("2B1594EF-AF70-4EC7-ACD3-88C683428E7A", "Agreement of Customs Declaration Agent");

		protected override string ApplicationCodeCore => GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new[] { (ZString)EDIMessageTypeList.Codes.ACD };

		protected override void ProcessMessageCore(EDIMessage incomingMessage)
		{
			var incomingInterchange = incomingMessage.Interchange;
			if (incomingInterchange == null)
			{
				Logger.LogError("Could not find EDIInterchange for the EDIMessage, number: " + incomingMessage.EM_MessageNum);
			}
			else
			{
				var queryForOutgoingInterchange = new ZQuery();
				queryForOutgoingInterchange.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, incomingInterchange.EI_InterchangeNum);
				queryForOutgoingInterchange.AddToFilter(EDIInterchangeSchema.EI_From, incomingInterchange.EI_To);
				queryForOutgoingInterchange.AddToFilter(EDIInterchangeSchema.EI_To, incomingInterchange.EI_From);
				var outgoingInterchange = incomingMessage.Factory.LoadTop1<EDIInterchange>(queryForOutgoingInterchange);

				var outgoingMessage = outgoingInterchange?.ContainedMessages.FirstOrDefault() as EDIMessage;
				var linkedEntryHeader = outgoingMessage?.EM_LinkedObject as CusEntryHeader;
				ImportAgrResponse importAgrResponse = null;
				if (linkedEntryHeader != null)
				{
					importAgrResponse = XmlObjectSerializer.Deserialize<ImportAgrResponse>(incomingMessage.EM_MessageText);
				}

				if (importAgrResponse != null)
				{
					incomingMessage.EM_LinkedObject = linkedEntryHeader;

					if (importAgrResponse.ResponseInfo?.ResponseCode == 1)
					{
						AttachImportAgrResponse(importAgrResponse.ConsignNo, linkedEntryHeader);
					}
					incomingMessage.EM_Status = EDIMessage.Status.ProcessedOK;

					incomingMessage.EM_MessageInterpretation = GetImportAgrResponseHtmlInterpretation(importAgrResponse);
					Logger.DebugLog((NoResString)"Message processed, interchange number: " + incomingMessage.EM_InterchangeNumber);

					SendNotificationsAfterProcess(outgoingMessage, incomingMessage.EM_MessageInterpretation, linkedEntryHeader, importAgrResponse != null && importAgrResponse.ResponseInfo?.ResponseCode == 1);
				}
				else
				{
					Logger.LogError("Could not find outgoing Interchange or Message for incoming Message, or incoming Message not linked to correct Business Object, InterchangeNum: " + incomingMessage.EM_InterchangeNumber);
					incomingMessage.EM_Status = EDIMessage.Status.Discarded;
				}
			}
		}

		void AttachImportAgrResponse(ZString consignNo, CusEntryHeader linkedEntryHeader)
		{
			if (!string.IsNullOrWhiteSpace(consignNo) && linkedEntryHeader.EntryInstruction is CusEntryInstruction instruction && !CusAttachment.Exists(instruction, CSDDocTypeList.Codes._10000001, consignNo))
			{
				CusAttachment.AddNew(instruction, CSDDocTypeList.Codes._10000001, consignNo);
				linkedEntryHeader.Logs.AddNew(Events.ChinaAgreementOfDeclarationAgentNumberReceived);
			}
		}

		#region SuppressResourceStringsCheckRegion

		ZString GetImportAgrResponseHtmlInterpretation(ImportAgrResponse importAgrResponse)
		{
			ZString result = ZString.Empty;
			var responseInfo = importAgrResponse.ResponseInfo;

			if (responseInfo != null)
			{
				result = ResponseMessageProcessHelper.GetHtmlInterpretation(new[] {
					new KeyValuePair<string, string>("响应代码", responseInfo.ResponseCode.ToString(CultureInfo.InvariantCulture)),
					new KeyValuePair<string, string>("响应信息",responseInfo.ResponseMessage),
					new KeyValuePair<string, string>("代理报关委托协议编号", importAgrResponse.ConsignNo),
				});
			}

			return result;
		}

		#endregion

		void SendNotificationsAfterProcess(EDIMessage outgoingMessage, ZString incomingMessageBody, CusEntryHeader cusEntryHeader, bool succeeded)
		{
			var jobDeclaration = cusEntryHeader?.Declaration;
			if (jobDeclaration != null)
			{
				var subject = ResponseMessageProcessHelper.GetEmailSubjectWithReferenceNumbers(cusEntryHeader, succeeded, Constants.MessageDescriptions.AcdAgrResponse);
				var recipients = ResponseMessageProcessHelper.GetEmailAddressesToNotify(outgoingMessage, cusEntryHeader);

				ResponseMessageProcessHelper.CreateMail(subject, incomingMessageBody, recipients, jobDeclaration);
			}
		}
	}
}
