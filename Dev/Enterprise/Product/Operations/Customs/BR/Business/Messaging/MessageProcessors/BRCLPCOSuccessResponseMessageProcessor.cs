using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.LPCO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business
{
	public class BRCLPCOSuccessResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCLPCOSuccessResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("0F446C9A-BA10-4389-B353-5A497C39E708", "LPCO Success Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.LPC };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Success };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusLPCOHeader lpcoHeader)
			{
				var lpcoDetail = BRMessageHelper.DeserializeObject<LpcoDetalhado>(message.EM_MessageText);
				if (lpcoDetail == null)
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;
					Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
				}
				else if (!string.IsNullOrEmpty(lpcoDetail.numero))
				{
					lpcoHeader.CPH_MessageStatus = BRMessageStatusList.Codes.Accepted;
					lpcoHeader.CPH_Number = lpcoDetail.numero;

					if (lpcoDetail.EffectiveDate.HasValue)
					{
						lpcoHeader.CPH_StartDate = (ZDate)lpcoDetail.EffectiveDate.Value.Date;
					}

					if (lpcoDetail.ExpirationDate.HasValue)
					{
						lpcoHeader.CPH_EndDate = (ZDate)lpcoDetail.ExpirationDate.Value.Date;
					}
				}
				else
				{
					lpcoHeader.CPH_MessageStatus = BRMessageStatusList.Codes.Rejected;
				}
			}
		}
	}
}
